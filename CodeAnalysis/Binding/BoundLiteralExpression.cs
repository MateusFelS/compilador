internal abstract partial class BoundNode{
    internal sealed class BoundLiteralExpression : BoundExpression{
        public BoundLiteralExpression(object value)
        {
            Value = value;

            if(value is bool)
                Type = TypeSymbol.Bool;
            else if (value is int)
                Type = TypeSymbol.Int;
            else if (value is string)
                Type = TypeSymbol.String;
            else
                throw new Exception($"Literal inexperado '{value}' do tipo '{value.GetType()}'!");
        }

        public override TypeSymbol Type {get;}
        public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
        public object Value { get; }

       
    }

}