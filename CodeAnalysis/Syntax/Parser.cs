using System.Collections.Immutable;
using static AssignmentExpressionSyntax;
using static Compilation;
using static IfStatementSyntax;

internal sealed class Parser
{
    private readonly ImmutableArray<SyntaxToken> _tokens;
    private readonly SourceText _text;
    private DiagnosticsBag _diagnostics = new DiagnosticsBag();
    private int _position;

    public Parser(SourceText text){
        var tokens = new List<SyntaxToken>();
        var lexer = new Lexer(text);
        SyntaxToken token;
        
        do{
            token = lexer.Lex();
            if(token.Kind != SyntaxKind.WhiteSpaceToken && token.Kind != SyntaxKind.BadToken){
                tokens.Add(token);
            }
        } while(token.Kind != SyntaxKind.EOFToken);

        _tokens = tokens.ToImmutableArray();
        _diagnostics.AddRange(lexer.Diagnostics);
        _text = text;
    }

    public DiagnosticsBag Diagnostics => _diagnostics;

    private SyntaxToken Peek(int offset){
        var index = _position + offset;
        if(index >= _tokens.Length)
            return _tokens[_tokens.Length - 1];
        return _tokens[index];
    }

    private SyntaxToken Current => Peek(0);

    private SyntaxToken NextToken(){
        var current = Current;
        _position++;
        return current;
    }

    private SyntaxToken MatchToken(SyntaxKind kind){
        if(Current.Kind == kind)
            return NextToken();
        _diagnostics.ReportUnexpectedToken(Current.Span, Current.Kind, kind);
        return new SyntaxToken(kind, Current.Position, null, null);
    }
    public CompilationUnitSyntax ParseCompilationUnit(){
        var statement = ParseStatement();
        var endOfFileToken = MatchToken(SyntaxKind.EOFToken);
        return new CompilationUnitSyntax(statement, endOfFileToken);
    }

    private StatementSyntax ParseStatement()
    {
        switch (Current.Kind)
        {
            case SyntaxKind.OpenBraceToken:
                return ParseBlockStatement();
            case SyntaxKind.LetKeyword:
            case SyntaxKind.VarKeyword:
                return ParseVariableDeclaration();
            case SyntaxKind.IfKeyword:
                return ParseIfStatement();
            case SyntaxKind.WhileKeyword:
                return ParseWhileStatement();
            case SyntaxKind.ForKeyword:
                return ParseForStatement();
            default:
                return ParseExpressionStatement();
        }
    }

    private StatementSyntax ParseForStatement()
    {
        var keyword = MatchToken(SyntaxKind.ForKeyword);
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var equalsToken = MatchToken(SyntaxKind.EqualsToken);
        var lowerBound = ParseExpression();
        var toKeyword = MatchToken(SyntaxKind.ToKeyword);
        var upperBound = ParseExpression();
        var body = ParseStatement();
        return new ForStatementSyntax(keyword, identifier, equalsToken, lowerBound, toKeyword, upperBound, body);
    }

    private StatementSyntax ParseWhileStatement()
    {
        var keyword = MatchToken(SyntaxKind.WhileKeyword);
        var condition = ParseExpression();
        var body = ParseStatement();
        return new WhileStatementSyntax(keyword, condition, body);
    }

    private StatementSyntax ParseIfStatement()
    {
        var keyword = MatchToken(SyntaxKind.IfKeyword);
        var condition = ParseExpression();
        var statement = ParseStatement();
        var elseClause = ParseElseClause();
        return new IfStatementSyntax(keyword, condition, statement, elseClause);
    }

    private ElseClauseSyntax ParseElseClause()
    {
        if(Current.Kind != SyntaxKind.ElseKeyword)
            return null;
        
        var keyword = NextToken();
        var statement = ParseStatement();
        return new ElseClauseSyntax(keyword, statement);
    }

    private StatementSyntax ParseVariableDeclaration()
{
    var expected = Current.Kind == SyntaxKind.LetKeyword ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword;
    var keyword = MatchToken(expected);
    var identifier = MatchToken(SyntaxKind.IdentifierToken);
    var typeClause = ParseOptionalTypeClause();
    var equals = MatchToken(SyntaxKind.EqualsToken);
    var initializer = ParseExpression();
    return new VariableDeclarationSyntax(keyword, identifier, typeClause, equals, initializer);
}

    private TypeClauseSyntax ParseOptionalTypeClause()
    {
        if(Current.Kind != SyntaxKind.ColonToken)
            return null;
        return ParseTypeClause();
    }

    private TypeClauseSyntax ParseTypeClause()
    {
        var colonToken = MatchToken(SyntaxKind.ColonToken);
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        return new TypeClauseSyntax(colonToken, identifier);
    }

    private StatementSyntax ParseExpressionStatement()
    {
        var expression = ParseExpression();
        return new ExpressionStatementSyntax(expression);
    }

    private BlockStatementSyntax ParseBlockStatement()
    {
        var statements = ImmutableArray.CreateBuilder<StatementSyntax>();

        var openBraceToken = MatchToken(SyntaxKind.OpenBraceToken);
        while(Current.Kind != SyntaxKind.EOFToken && Current.Kind != SyntaxKind.CloseBraceToken){
            var startToken = Current;
            var statement = ParseStatement();
            statements.Add(statement);

            if(Current == startToken)
                NextToken();

        }
        var closeBraceToken = MatchToken(SyntaxKind.CloseBraceToken);

        return new BlockStatementSyntax(openBraceToken, statements.ToImmutable(), closeBraceToken);
    }

    private ExpressionSyntax ParseExpression(){
        return ParseAssignmentExpression();
    }
    private ExpressionSyntax ParseAssignmentExpression(){
        // a + b + 5      a = b = 5
        //
        //       +          =
        //      / \        / \
        //     +   5      a   =
        //    / \            / \
        //   a   b          b   5

        if(Peek(0).Kind == SyntaxKind.IdentifierToken && Peek(1).Kind == SyntaxKind.EqualsToken){
            var identifierToken = NextToken();
            var operatorToken = NextToken();
            var right = ParseAssignmentExpression();
            return new AssignmentExpressionSyntax(identifierToken, operatorToken, right);
        }
        return ParseBinaryExpression();
    }

    private ExpressionSyntax ParseBinaryExpression(int parentPrecedence = 0){
        ExpressionSyntax left;
        var unaryOperatorPrecedente = Current.Kind.GetUnaryOpetarorPrecedence();
        if(unaryOperatorPrecedente != 0 && unaryOperatorPrecedente >= parentPrecedence){
            var operatorToken = NextToken();
            var operand = ParseBinaryExpression(unaryOperatorPrecedente);
            left  = new UnaryExpressionSyntax(operatorToken, operand);
        } else{
            left = ParsePrimaryExpression();
        }

        while(true){
            var precedence = Current.Kind.GetBinaryOpetarorPrecedence();
            if(precedence == 0 || precedence <= parentPrecedence)
                break;
            var operatorToken = NextToken(); 
            var right = ParseBinaryExpression(precedence); 
            left = new BinaryExpressionSyntax(left, operatorToken, right);
        }
        return left;
    }

    private ExpressionSyntax ParsePrimaryExpression(){
        switch (Current.Kind)
        {
            case SyntaxKind.OpenParToken:
                return ParseParExpression();

            case SyntaxKind.FalseKeyword:
            case SyntaxKind.TrueKeyword:
                return ParseBooleanLiteral();

            case SyntaxKind.NumberToken:
                return ParseNumberLiteral();

            case SyntaxKind.StringToken:
                return ParseStringLiteral();
            
            case SyntaxKind.IdentifierToken:
            default:
                return ParseNameOrCallExpression();
                
        }
        
    }

    private ExpressionSyntax ParseNameOrCallExpression()
    {
        if(Peek(0).Kind == SyntaxKind.IdentifierToken && Peek(1).Kind == SyntaxKind.OpenParToken)
            return ParseCallExpression();
        return ParseNameExpression();
    }

    private ExpressionSyntax ParseCallExpression()
    {
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var openPar = MatchToken(SyntaxKind.OpenParToken);
        var arguments = ParseArguments();
        var closePar = MatchToken(SyntaxKind.CloseParToken);
        return new CallExpressionSyntax(identifier, openPar, arguments, closePar);
    }

    private SeparatedSyntaxList<ExpressionSyntax> ParseArguments()
    {
        var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();
        while(Current.Kind != SyntaxKind.CloseParToken && Current.Kind != SyntaxKind.EOFToken){
            var expression = ParseExpression();
            nodesAndSeparators.Add(expression);

            if(Current.Kind != SyntaxKind.CloseParToken){
                var comma = MatchToken(SyntaxKind.Comma);
                nodesAndSeparators.Add(comma);
            }
        }
        return new SeparatedSyntaxList<ExpressionSyntax>(nodesAndSeparators.ToImmutable());
    }

    private ExpressionSyntax ParseNumberLiteral()
    {
        var numberToken = MatchToken(SyntaxKind.NumberToken);
        return new NumberExpressionSyntax(numberToken);
    }
    
    private ExpressionSyntax ParseStringLiteral()
    {
        var stringToken = MatchToken(SyntaxKind.StringToken);
        return new NumberExpressionSyntax(stringToken);
    }

    private ExpressionSyntax ParseParExpression()
    {
        var left = MatchToken(SyntaxKind.OpenParToken);
        var expression = ParseExpression();
        var right = MatchToken(SyntaxKind.CloseParToken);
        return new ParentesesExpressionSyntax(left, expression, right);
    }

    private ExpressionSyntax ParseBooleanLiteral()
    {
        var isTrue = Current.Kind == SyntaxKind.TrueKeyword;
        var keywordToken = isTrue ? MatchToken(SyntaxKind.TrueKeyword) : MatchToken(SyntaxKind.FalseKeyword);
        return new NumberExpressionSyntax(keywordToken, isTrue);
    }

    private ExpressionSyntax ParseNameExpression()
    {
        var identifierToken = MatchToken(SyntaxKind.IdentifierToken);
        return new NameExpressionSyntax(identifierToken);
    }
}
