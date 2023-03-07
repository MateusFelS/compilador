public static class SyntaxFacts{
        public static int GetUnaryOpetarorPrecedence(this SyntaxKind kind){
        switch(kind){
            case SyntaxKind.PlusToken:
            case SyntaxKind.MinusToken:
            case SyntaxKind.BangToken:
                return 6;
            default:
                return 0;
        }
    }

     public static int GetBinaryOpetarorPrecedence(this SyntaxKind kind){
        switch(kind){
            case SyntaxKind.MultToken:
            case SyntaxKind.DivToken:
                return 5;
            case SyntaxKind.PlusToken:
            case SyntaxKind.MinusToken:
                return 4;
            case SyntaxKind.EqualsEqualsToken:
            case SyntaxKind.BangEqualsToken:
            case SyntaxKind.LessToken:
            case SyntaxKind.LessOrEqualsToken:
            case SyntaxKind.GreatToken:
            case SyntaxKind.GreatOrEqualsToken:
                return 3;
            case SyntaxKind.AmpersandToken:
                return 2;
            case SyntaxKind.PipeToken:
                return 1;
            default:
                return 0;
        }
    }

    public static SyntaxKind GetKeywordKind(string text){
        switch(text){
            case "true":
                return SyntaxKind.TrueKeyword;
            case "false":
                return SyntaxKind.FalseKeyword;
            case "if":
                return SyntaxKind.IfKeyword;
            case "else":
                return SyntaxKind.ElseKeyword;
            case "let":
                return SyntaxKind.LetKeyword;
            case "var":
                return SyntaxKind.VarKeyword;
            case "while":
                return SyntaxKind.WhileKeyword;
            case "for":
                return SyntaxKind.ForKeyword;
            case "to":
                return SyntaxKind.ToKeyword;
            default:
                return SyntaxKind.IdentifierToken;
        }
    }

    public static IEnumerable<SyntaxKind> GetUnaryOperatorKinds(){
        var kinds = (SyntaxKind[]) Enum.GetValues(typeof(SyntaxKind));
        foreach(var kind in kinds){
            if(GetUnaryOpetarorPrecedence(kind) > 0)
                yield return kind;
        }
    }

    public static IEnumerable<SyntaxKind> GetBinaryOperatorKinds(){
        var kinds = (SyntaxKind[]) Enum.GetValues(typeof(SyntaxKind));
        foreach(var kind in kinds){
            if(GetBinaryOpetarorPrecedence(kind) > 0)
                yield return kind;
        }
    }

    public static string GetText(SyntaxKind kind){
        switch(kind){
            case SyntaxKind.PlusToken: return "+";
            case SyntaxKind.MinusToken: return "-";
            case SyntaxKind.MultToken: return "*";
            case SyntaxKind.DivToken: return "/";
            case SyntaxKind.BangToken: return "!";
            case SyntaxKind.EqualsToken: return "=";
            case SyntaxKind.LessToken: return "<";
            case SyntaxKind.LessOrEqualsToken: return "<=";
            case SyntaxKind.GreatToken: return ">";
            case SyntaxKind.GreatOrEqualsToken: return ">=";
            case SyntaxKind.AmpersandToken: return "&&";
            case SyntaxKind.PipeToken: return "||";
            case SyntaxKind.EqualsEqualsToken: return "==";
            case SyntaxKind.BangEqualsToken: return "!=";
            case SyntaxKind.OpenParToken: return "(";
            case SyntaxKind.CloseParToken: return ")";
            case SyntaxKind.OpenBraceToken: return "{";
            case SyntaxKind.CloseBraceToken: return "}";
            case SyntaxKind.ColonToken: return ":";
            case SyntaxKind.Comma: return ",";
            case SyntaxKind.IfKeyword: return "if";            
            case SyntaxKind.ElseKeyword: return "else";
            case SyntaxKind.FalseKeyword: return "false";            
            case SyntaxKind.TrueKeyword: return "true";
            case SyntaxKind.LetKeyword: return "let";            
            case SyntaxKind.VarKeyword: return "var";
            case SyntaxKind.WhileKeyword: return "while";
            case SyntaxKind.ForKeyword: return "for";
            case SyntaxKind.ToKeyword: return "to";
            default: return null;
        }
       
    }
}

