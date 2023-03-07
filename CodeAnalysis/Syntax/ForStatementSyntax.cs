public sealed class ForStatementSyntax : StatementSyntax{
    public ForStatementSyntax(SyntaxToken keyword, SyntaxToken identifier, SyntaxToken equalsTo, ExpressionSyntax lowerBound, SyntaxToken toKeyword, ExpressionSyntax upperBound, StatementSyntax body)
    {
        Keyword = keyword;
        Identifier = identifier;
        EqualsTo = equalsTo;
        LowerBound = lowerBound;
        ToKeyword = toKeyword;
        UpperBound = upperBound;
        Body = body;
    }
    public override SyntaxKind Kind => SyntaxKind.ForStatement;

    public SyntaxToken Keyword { get; }
    public SyntaxToken Identifier { get; }
    public SyntaxToken EqualsTo { get; }
    public ExpressionSyntax LowerBound { get; }
    public SyntaxToken ToKeyword { get; }
    public ExpressionSyntax UpperBound { get; }
    public StatementSyntax Body { get; }
}