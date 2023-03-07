using System.Collections;
using System.Collections.Immutable;

public abstract class SeparatedSyntaxList{
    public abstract ImmutableArray<SyntaxNode> GetWithSeparators();
}

public sealed class SeparatedSyntaxList<T> : SeparatedSyntaxList, IEnumerable<T>
    where T : SyntaxNode{
        private readonly ImmutableArray<SyntaxNode> _nodesAndSeparatos;

        public SeparatedSyntaxList(ImmutableArray<SyntaxNode> nodesAndSeparators){
            _nodesAndSeparatos = nodesAndSeparators;
        }

        public int Count => (_nodesAndSeparatos.Length + 1) / 2;

        public T this[int index] => (T) _nodesAndSeparatos[index * 2];

        public SyntaxToken GetSeparator(int index){
            if(index == Count - 1)
                return null;
                
            return (SyntaxToken) _nodesAndSeparatos[index * 2 + 1];
        }
            
        public override ImmutableArray<SyntaxNode> GetWithSeparators() => _nodesAndSeparatos;
        
        public IEnumerator<T> GetEnumerator(){
            for(var i = 0; i < Count; i++){
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator(){
            return GetEnumerator();
        }
}
    

