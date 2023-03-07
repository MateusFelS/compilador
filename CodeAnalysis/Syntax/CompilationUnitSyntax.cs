public sealed partial class AssignmentExpressionSyntax
{
    public sealed class CompilationUnitSyntax : SyntaxNode{
        public CompilationUnitSyntax(StatementSyntax statement, SyntaxToken eofToken){
            Statement = statement;
            EofToken = eofToken;
        }

        public StatementSyntax Statement { get; }
        public SyntaxToken EofToken { get; }

        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
    }
}
