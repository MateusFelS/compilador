using System.Collections.Immutable;

internal abstract partial class BoundNode{
    internal sealed class BoundBlockStatement : BoundStatement{
        public BoundBlockStatement(ImmutableArray<BoundStatement> statements){
            Statements = statements;
        }

        public ImmutableArray<BoundStatement> Statements { get; }

        public override BoundNodeKind Kind => BoundNodeKind.BlockStatement;
    }

}