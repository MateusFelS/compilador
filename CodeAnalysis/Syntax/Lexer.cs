using System.Text;
using static Compilation;

internal sealed class Lexer
{
    private readonly SourceText _text;
    private readonly DiagnosticsBag _diagnostics = new DiagnosticsBag();
    private int _position;
    private int _start;
    private SyntaxKind _kind;
    private object _value;

    public Lexer(SourceText text){
        _text = text;
    }

    public DiagnosticsBag Diagnostics => _diagnostics;

    private char Current => Peek(0);
    private char Lookahead => Peek(1);

    private char Peek(int offset){
        var index = _position + offset;
        if(index >= _text.Length || index < 0)
            return '\0';
        return _text[index];
    }

    private void Next(){
        _position++;
    }

    public SyntaxToken Lex(){
        //numbers
        //+ - * / ()
        //whitespace

        _start = _position;
        _kind = SyntaxKind.BadToken;
        _value = null;

        switch (Current){
            case '\0':
                _kind = SyntaxKind.EOFToken;
                break;
            case '+':
                _kind = SyntaxKind.PlusToken;
                _position++;
                break;
            case '-':
                _kind = SyntaxKind.MinusToken;
                _position++;
                break;
            case '*':
                _kind = SyntaxKind.MultToken;
                _position++;
                break;
            case '/':
                _kind = SyntaxKind.DivToken;
                _position++;
                break;
            case '(':
                _kind = SyntaxKind.OpenParToken;
                _position++;
                break;
            case ')':
                _kind = SyntaxKind.CloseParToken;
                _position++;
                break;
            case '{':
                _kind = SyntaxKind.OpenBraceToken;
                _position++;
                break;
            case '}':
                _kind = SyntaxKind.CloseBraceToken;
                _position++;
                break;
            case ':':
                _kind = SyntaxKind.ColonToken;
                _position++;
                break;
            case ',':
                _kind = SyntaxKind.Comma;
                _position++;
                break;
            case '&':
                if(Lookahead == '&'){
                    _kind = SyntaxKind.AmpersandToken;
                    _position += 2;
                    break;
                }
                break;
            case '|':
                if(Lookahead == '|'){
                    _kind = SyntaxKind.PipeToken;
                    _position += 2;
                    break;
                }              
                break;
            case '=':
                _position ++;
                if(Current != '='){
                    _kind = SyntaxKind.EqualsToken;
                }
                else{
                    _position++;
                    _kind = SyntaxKind.EqualsEqualsToken;
                }
                break;
            case '!':
                _position++;
                if (Current != '=')
                {
                    _kind = SyntaxKind.BangToken;
                }
                else
                {
                    _kind = SyntaxKind.BangEqualsToken;
                    _position++;
                }
                break;
            case '<':
                _position++;
                if(Current != '='){
                    _kind = SyntaxKind.LessToken;
                } else{
                    _kind = SyntaxKind.LessOrEqualsToken;
                    _position++;
                }
                break;
            case '>':
                _position++;
                if(Current != '='){
                    _kind = SyntaxKind.GreatToken;
                } else{
                    _kind = SyntaxKind.GreatOrEqualsToken;
                    _position++;
                }
                break;
            case '"':
                ReadString();
                break;
            case '0': case '1': case '2': case '3': case '4':
            case '5': case '6': case '7': case '8': case '9':
                ReadNumberToken();
                break;
            case ' ': case '\t': case '\n': case '\r':
                ReadWhiteSpace();
                break;

            default:
                if(char.IsWhiteSpace(Current)){
                    ReadWhiteSpace();
                } else if(char.IsLetter(Current)){
                    ReadIdentifierOrKeyword();
                } else{
                    _diagnostics.ReportBadCharacter(_position, Current);
                    _position++;
                }
                break; 
            }
       

        var length = _position - _start;
        var text = SyntaxFacts.GetText(_kind);
        if(text == null)
            text = _text.ToString(_start, length);
        return new SyntaxToken(_kind, _start, text, _value);
        
        
    }

    private void ReadString()
    {
        _position++;
        var sb = new StringBuilder();
        var done = false;

        while(!done){
            switch(Current){
                case '\0':
                case '\r':
                case '\n':
                    var span = new TextSpan(_start, 1);
                    _diagnostics.ReportUnterminatedString(span);
                    done = true;
                    break;
                case '"':
                    if(Lookahead == '"'){
                        sb.Append(Current);
                        _position += 2;
                    } else{
                        _position++;
                        done = true;
                    }
                    break;
                default:
                    sb.Append(Current);
                    _position++;
                    break;
            }
        }
        _kind = SyntaxKind.StringToken;
        _value = sb.ToString();
    }

    private void ReadIdentifierOrKeyword()
    {
        while (char.IsLetter(Current))
            Next();

        var length = _position - _start;
        var text = _text.ToString(_start, length);
        _kind = SyntaxFacts.GetKeywordKind(text);
    }

    private void ReadWhiteSpace()
    {
        while (char.IsWhiteSpace(Current))
            Next();

        _kind = SyntaxKind.WhiteSpaceToken;
    }

    private void ReadNumberToken()
    {
        while (char.IsDigit(Current))
            Next();
        var length = _position - _start;
        var text = _text.ToString(_start, length);
        if (!int.TryParse(text, out var value))
            _diagnostics.ReportInvalidNumber(new TextSpan(_start, length), text, TypeSymbol.Int);

        _value = value;
        _kind = SyntaxKind.NumberToken;
    }
}
