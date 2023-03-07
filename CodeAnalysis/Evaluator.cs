using static BoundNode;
using static Compilation;

internal sealed class Evaluator
{
    private readonly BoundStatement _root;
    private readonly Dictionary<VariableSymbol, object> _variables;
    private Random _random;

    private object _lastValue;

    public Evaluator(BoundStatement root, Dictionary<VariableSymbol, object> variables)
    {
        _root = root;
        _variables = variables;
    }

    public object Evaluate(){
        EvaluateStatement(_root);
        return _lastValue;
    }

    private void EvaluateStatement(BoundStatement node)
    {
        //Binary expression
        //Number expression
        switch(node.Kind){
            case BoundNodeKind.BlockStatement:
                EvaluateBlockStatament((BoundBlockStatement)node);
                break;
            case BoundNodeKind.VariableDeclaration:
                EvaluateVariableDeclaration((BoundVariableDeclaration)node);
                break;
            case BoundNodeKind.IfStatement:
                EvaluateIfStatement((BoundIfStatement)node);
                break;
            case BoundNodeKind.WhileStatement:
                EvaluateWhileStatement((BoundWhileStatement)node);
                break;
            case BoundNodeKind.ForStatement:
                EvaluateForStatement((BoundForStatement)node);
                break;
            case BoundNodeKind.ExpressionStatement:
                EvaluateExpressionStatament((BoundExpressionStatement)node);
                break;
            default:
                throw new Exception($"Nó inesperado {node.Kind}");
        } 
    }

    private void EvaluateForStatement(BoundForStatement node)
    {
        var lowerBound = (int)EvaluateExpression(node.LowerBound);
        var upperBound = (int)EvaluateExpression(node.UpperBound);

        for(var i = lowerBound; i <= upperBound; i++){
            _variables[node.Variable] = i;
            EvaluateStatement(node.Body);
        }
    }

    private void EvaluateWhileStatement(BoundWhileStatement node)
    {
        while((bool)EvaluateExpression(node.Condition))
            EvaluateStatement(node.Body);
    }

    private void EvaluateIfStatement(BoundIfStatement node)
    {
        var condition = (bool)EvaluateExpression(node.Condition);
        if(condition)
            EvaluateStatement(node.ThenStatement);
        else if(node.ElseStatement != null)
            EvaluateStatement(node.ElseStatement);
    }

    private void EvaluateVariableDeclaration(BoundVariableDeclaration node)
    {
        var value = EvaluateExpression(node.Initializer);
        _variables[node.Variable] = value;
        _lastValue = value;
    }

    private void EvaluateExpressionStatament(BoundExpressionStatement node)
    {
        _lastValue = EvaluateExpression(node.Expression);
    }

    private void EvaluateBlockStatament(BoundBlockStatement node)
    {
        foreach(var statement in node.Statements)
        EvaluateStatement(statement);
    }

    private object EvaluateExpression(BoundExpression node)
    {
        //Binary expression
        //Number expression
        switch (node.Kind)
        {
            case BoundNodeKind.LiteralExpression:
                return EvaluateLiteralExpression((BoundLiteralExpression)node);
            case BoundNodeKind.VariableExpression:
                return EvaluateVariableExpression((BoundVariableExpression)node);
            case BoundNodeKind.AssignmentExpression:
                return EvaluateAssignmentExpression((BoundAssignmentExpression)node);
            case BoundNodeKind.UnaryExpression:
                return EvaluateUnaryExpression((BoundUnaryExpression)node);
            case BoundNodeKind.BinaryExpression:
                return EvaluateBinaryExpression((BoundBinaryExpression)node);
            case BoundNodeKind.CallExpression:
                return EvaluateCallExpression((BoundCallExpression)node);
            case BoundNodeKind.ConversionExpression:
                return EvaluateConversionExpression((BoundConversionExpression)node);
            default:
                throw new Exception($"Nó inesperado {node.Kind}");
        }
    }

    private object EvaluateConversionExpression(BoundConversionExpression co)
    {
        var value = EvaluateExpression(co.Expression);
        if(co.Type == TypeSymbol.Bool)
            return Convert.ToBoolean(value);
        else if(co.Type == TypeSymbol.Int)
            return Convert.ToInt32(value);
        else if(co.Type == TypeSymbol.String)
            return Convert.ToString(value);
        else
            throw new Exception($"Tipo inesperado {co.Type}");
    }

    private object EvaluateCallExpression(BoundCallExpression c)
    {
        if(c.Function == BuiltinFunctions.Read){
            return Console.ReadLine();
        } else if(c.Function == BuiltinFunctions.Write){
            var message = (string)EvaluateExpression(c.Arguments[0]);
            Console.WriteLine(message);
            return null;
        } else if(c.Function == BuiltinFunctions.Rnd){
            var max = (int)EvaluateExpression(c.Arguments[0]);
            if(_random == null)
                _random = new Random();
            return _random.Next(max);
        } else{
            throw new Exception($"Função {c.Function} inesperada!");
        }
    }

    private object EvaluateBinaryExpression(BoundBinaryExpression b)
    {
        var left = EvaluateExpression(b.Left);
        var right = EvaluateExpression(b.Right);

        switch (b.Op.Kind)
        {
            case BoundBinaryOperatorKind.Addition:
                if(b.Type == TypeSymbol.Int)
                    return (int)left + (int)right;
                else
                    return (string)left + (string)right;
            case BoundBinaryOperatorKind.Subtraction:
                return (int)left - (int)right;
            case BoundBinaryOperatorKind.Multiplication:
                return (int)left * (int)right;
            case BoundBinaryOperatorKind.Division:
                return (int)left / (int)right;
            case BoundBinaryOperatorKind.LogicalAnd:
                return (bool)left && (bool)right;
            case BoundBinaryOperatorKind.LogicalOr:
                return (bool)left || (bool)right;
            case BoundBinaryOperatorKind.Equals:
                return Equals(left, right);
            case BoundBinaryOperatorKind.NotEquals:
                return !Equals(left, right);
            case BoundBinaryOperatorKind.LessThan:
                return (int)left < (int)right;
            case BoundBinaryOperatorKind.LessThanOrEqualsTo:
                return (int)left <= (int)right;
            case BoundBinaryOperatorKind.GreatThan:
                return (int)left > (int)right;
            case BoundBinaryOperatorKind.GreatThanOrEqualsTo:
                return (int)left >= (int)right;
            default:
                throw new Exception($"Operador binário inesperado {b.Op}!");
        }
    }

    private object EvaluateUnaryExpression(BoundUnaryExpression u)
    {
        var operand = EvaluateExpression(u.Operand);

        switch (u.Op.Kind)
        {
            case BoundUnaryOperatorKind.Identity:
                return (int)operand;
            case BoundUnaryOperatorKind.Negation:
                return -(int)operand;
            case BoundUnaryOperatorKind.LogicalNegation:
                return !(bool)operand;
            default:
                throw new Exception($"Operador unário inesperado {u.Op}!");
        }
    }

    private object EvaluateAssignmentExpression(BoundAssignmentExpression a)
    {
        var value = EvaluateExpression(a.Expression);
        _variables[a.Variable] = value;
        return value;
    }

    private object EvaluateVariableExpression(BoundVariableExpression v)
    {
        return _variables[v.Variable];
    }

    private static object EvaluateLiteralExpression(BoundLiteralExpression n)
    {
        return n.Value;
    }
}