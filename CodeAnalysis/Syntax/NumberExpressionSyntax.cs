public sealed class NumberExpressionSyntax : ExpressionSyntax{
    public NumberExpressionSyntax(SyntaxToken numberToken) : this(numberToken, numberToken.Value){
    }
    public NumberExpressionSyntax(SyntaxToken numberToken, object value){
        NumberToken = numberToken;
        Value = value;
    }

    public override SyntaxKind Kind => SyntaxKind.NumberExpression;
    public SyntaxToken NumberToken {get;}
    public object Value { get; }

}
