using System.Reflection;
using static Compilation;

public abstract class SyntaxNode{
    public abstract SyntaxKind Kind{get;}
    public virtual TextSpan Span{
        get{
            var first = GetChildren().First().Span;
            var last = GetChildren().Last().Span;
            return TextSpan.FromBounds(first.Start, last.End);
        }
    }
    public string Text { get;}

    public IEnumerable<SyntaxNode> GetChildren(){
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach(var property in properties){
            if(typeof(SyntaxNode).IsAssignableFrom(property.PropertyType)){
                var child = (SyntaxNode)property.GetValue(this);
                if(child != null)
                    yield return child;
            } else if(typeof(IEnumerable<SyntaxNode>).IsAssignableFrom(property.PropertyType)){
                var separatedSyntaxList = (IEnumerable<SyntaxNode>)property.GetValue(this);
                foreach(var child in separatedSyntaxList)
                    yield return child;
            } else if(typeof(SeparatedSyntaxList).IsAssignableFrom(property.PropertyType)){
                var list = (SeparatedSyntaxList)property.GetValue(this);
                foreach(var child in list.GetWithSeparators())
                    if(child != null)
                        yield return child;
            }
        }

    }
    public void WriterTo(TextWriter writer){
        PrettyPrint(writer, this);
    }
    private static void PrettyPrint(TextWriter writer, SyntaxNode node, string indent = ""){
        var isToConsole = writer == Console.Out;
        writer.Write(indent);

        if(isToConsole)
            Console.ForegroundColor = node is SyntaxToken ? ConsoleColor.Blue : ConsoleColor.Cyan;
        
        writer.Write(node.Kind);

        if(node is SyntaxToken t && t.Value != null){
            writer.Write(" ");
            writer.Write(t.Value);
        }

        if(isToConsole)
            Console.ResetColor();

        writer.WriteLine();

        indent += "    ";
        foreach(var child in node.GetChildren())
            PrettyPrint(writer, child, indent);
    }
    public override string ToString()
    {
        using(var writer = new StringWriter()){
            WriterTo(writer);
            return writer.ToString();
        }
    }
}
