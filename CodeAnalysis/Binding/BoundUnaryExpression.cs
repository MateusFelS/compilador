internal abstract partial class BoundNode{
    internal sealed class BoundUnaryExpression : BoundExpression{
        public BoundUnaryExpression(BoundUnaryOperator op, BoundExpression operand){
            Op = op;
            Operand = operand;
        }

        public override TypeSymbol Type => Op.ResultType;
        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
        public BoundUnaryOperator Op { get; }
        public BoundExpression Operand { get; }
    }

}