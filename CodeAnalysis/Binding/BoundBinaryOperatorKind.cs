internal abstract partial class BoundNode{
    internal enum BoundBinaryOperatorKind{
        Addition,
        Subtraction,
        Multiplication,
        Division,
        LogicalOr,
        LogicalAnd,
        Equals,
        NotEquals,
        LessThan,
        LessThanOrEqualsTo,
        GreatThanOrEqualsTo,
        GreatThan,
    }

}