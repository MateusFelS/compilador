internal abstract partial class BoundNode{
    internal enum BoundNodeKind{
        //Exprexions
        UnaryExpression,
        LiteralExpression,
        BinaryExpression,
        VariableExpression,
        AssignmentExpression,
        
        //Statements
        BlockStatement,
        ExpressionStatement,
        VariableDeclaration,
        IfStatement,
        WhileStatement,
        ForStatement,
        ErrorExpression,
        CallExpression,
        ConversionExpression,
    }

}