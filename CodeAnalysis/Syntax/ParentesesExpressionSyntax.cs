public sealed class ParentesesExpressionSyntax : ExpressionSyntax{
    public ParentesesExpressionSyntax(SyntaxToken openParToken, ExpressionSyntax expression, SyntaxToken closeParToken){
        OpenParToken = openParToken;
        Expression = expression;
        CloseParToken = closeParToken;
    }

    public override SyntaxKind Kind => SyntaxKind.ParExpression;
    public SyntaxToken OpenParToken { get; }
    public ExpressionSyntax Expression { get; }
    public SyntaxToken CloseParToken { get; }

}
