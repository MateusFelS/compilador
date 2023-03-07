using System.Collections.Immutable;
using static Compilation;

public sealed class EvaluationResult{
    public EvaluationResult(ImmutableArray<Diagnostics> diagnostics, object  value){
        Diagnostics = diagnostics;
        Value = value;
    }
    public ImmutableArray<Diagnostics> Diagnostics {get;}
    public object Value { get; }
}
