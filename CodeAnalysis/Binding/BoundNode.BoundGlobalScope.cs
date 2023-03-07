using System.Collections.Immutable;
using static Compilation;

internal abstract partial class BoundNode{
    internal sealed class BoundGlobalScope{
        public BoundGlobalScope(BoundGlobalScope previous, ImmutableArray<Diagnostics> diagnostics, ImmutableArray<VariableSymbol> variables, BoundStatement statement){
            Previous = previous;
            Diagnostics = diagnostics;
            Variables = variables;
            Statement = statement;
        }

        public BoundGlobalScope Previous { get; }
        public ImmutableArray<Diagnostics> Diagnostics { get; }
        public ImmutableArray<VariableSymbol> Variables { get; }
        public BoundStatement Statement { get; }
    }

}