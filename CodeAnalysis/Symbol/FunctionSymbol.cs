using System.Collections.Immutable;

public sealed class FunctionSymbol : Symbol{
    public FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameter, TypeSymbol type) : base(name){
        Parameter = parameter;
        Type = type;
    }

    public override SymbolKind Kind => SymbolKind.Function;

    public ImmutableArray<ParameterSymbol> Parameter { get; }
    public TypeSymbol Type { get; }
}
