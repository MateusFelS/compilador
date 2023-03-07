public sealed class CallExpressionSyntax : ExpressionSyntax{
    public CallExpressionSyntax(SyntaxToken identifier, SyntaxToken openPar, SeparatedSyntaxList<ExpressionSyntax> arguments, SyntaxToken closePar){
        Identifier = identifier;
        OpenPar = openPar;
        Arguments = arguments;
        ClosePar = closePar;
    }

    public override SyntaxKind Kind => SyntaxKind.CallExpression;

    public SyntaxToken Identifier { get; }
    public SyntaxToken OpenPar { get; }
    public SeparatedSyntaxList<ExpressionSyntax> Arguments { get; }
    public SyntaxToken ClosePar { get; }
}
    

