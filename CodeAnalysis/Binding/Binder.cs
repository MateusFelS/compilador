using System.Collections.Immutable;
using static AssignmentExpressionSyntax;
using static Compilation;

internal abstract partial class BoundNode{

    public abstract BoundNodeKind Kind {get;}

    internal sealed class Binder{
        private readonly DiagnosticsBag _diagnostics = new DiagnosticsBag();

        private BoundScope _scope;

        public Binder(BoundScope parent)
        {
            _scope = new BoundScope(parent);
        }

        public static BoundGlobalScope BindGlobalScope(BoundGlobalScope previous, CompilationUnitSyntax syntax){
            var parentScope = CreateParentScope(previous);
            var binder = new Binder(parentScope);
            var statement = binder.BindStatement(syntax.Statement);
            var variables = binder._scope.GetDeclaredVariables();
            var diagnostics = binder.Diagnostics.ToImmutableArray();

            if(previous != null)
                diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);
            return new BoundGlobalScope(previous, diagnostics, variables, statement);
        }

        private static BoundScope CreateParentScope(BoundGlobalScope previous)
        {
            var stack = new Stack<BoundGlobalScope>();
            while (previous != null)
            {
                stack.Push(previous);
                previous = previous.Previous;
            }

            var parent = CreateRootScope();

            while (stack.Count > 0)
            {
                previous = stack.Pop();
                var scope = new BoundScope(parent);
                foreach (var v in previous.Variables)
                    scope.TryDeclareVariable(v);
                parent = scope;
            }
            return parent;
        }

        private static BoundScope CreateRootScope()
        {
            var result = new BoundScope(null);
            foreach(var f in BuiltinFunctions.GetAll())
                result.TryDeclareFunction(f);
            
            return result;
        }

        public DiagnosticsBag Diagnostics => _diagnostics;

        private BoundStatement BindStatement(StatementSyntax syntax){
            switch(syntax.Kind){
                case SyntaxKind.BlockStatement:
                    return BindBlockStatement((BlockStatementSyntax)syntax);
                case SyntaxKind.VariableDeclaration:
                    return BindVariableDeclaration((VariableDeclarationSyntax)syntax);
                case SyntaxKind.IfStatement:
                    return BindIfStatement((IfStatementSyntax)syntax);
                case SyntaxKind.WhileStatement:
                    return BindWhileStatement((WhileStatementSyntax)syntax);
                case SyntaxKind.ForStatement:
                    return BindForStatement((ForStatementSyntax)syntax);
                case SyntaxKind.ExpressionStatement:
                    return BindExpressionStatement((ExpressionStatementSyntax)syntax);
                default:
                    throw new Exception($"Sintaxe não esperada: {syntax.Kind}");
            }
        }

        private BoundStatement BindForStatement(ForStatementSyntax syntax)
        {
            var lowerBound = BindExpression(syntax.LowerBound, TypeSymbol.Int);
            var upperBound = BindExpression(syntax.UpperBound, TypeSymbol.Int);

            _scope = new BoundScope(_scope);
            
            var variable = BindVariable(syntax.Identifier, isReadOnly: true, TypeSymbol.Int);

            var body = BindStatement(syntax.Body);

            _scope = _scope.Parent;

            return new BoundForStatement(variable, lowerBound, upperBound, body);
        }

        private VariableSymbol BindVariable(SyntaxToken identifier, bool isReadOnly, TypeSymbol type)
        {
            var name = identifier.Text ?? "?";
            var declare = identifier != null;
            var variable = new VariableSymbol(name, isReadOnly, type);
            
            if (!_scope.TryDeclareVariable(variable))
                _diagnostics.ReportVariableAlreadyDeclared(identifier.Span, name);
            return variable;
        }

        private BoundStatement BindWhileStatement(WhileStatementSyntax syntax)
        {
            var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            var body = BindStatement(syntax.Body);
            return new BoundWhileStatement(condition, body);
        }

        private BoundStatement BindIfStatement(IfStatementSyntax syntax)
        {
            var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            var thenStatement = BindStatement(syntax.ThenStatement);
            var elseStatement = syntax.ElseClause == null? null : BindStatement(syntax.ElseClause.ElseStatement);
            return new BoundIfStatement(condition, thenStatement, elseStatement);
        }   

        private BoundExpression BindExpression(ExpressionSyntax syntax, TypeSymbol targetType){
            var result = BindExpression(syntax);
            if(targetType != TypeSymbol.Error && result.Type != TypeSymbol.Error && result.Type != targetType)
                _diagnostics.ReportCannotConvert(syntax.Span, result.Type, targetType);
            
            return result;
        }

        private BoundStatement BindVariableDeclaration(VariableDeclarationSyntax syntax)
        {
            var isReadOnly = syntax.Keyword.Kind == SyntaxKind.LetKeyword;
            var initializer = BindExpression(syntax.Initializer);
            var variable = BindVariable(syntax.Identifier, isReadOnly, initializer.Type);
        
            return new BoundVariableDeclaration(variable, initializer);
        }

        public BoundExpression BindExpression(ExpressionSyntax syntax, bool canBeVoid = false){
            var result = BindExpression(syntax);
            if(!canBeVoid && result.Type == TypeSymbol.Void){
                _diagnostics.ReportExpressionMustHaveValue(syntax.Span);
                return new BoundErrorExpression();
            }
            return result;
        }

        public BoundExpression BindExpression(ExpressionSyntax syntax){
            switch(syntax.Kind){
                case SyntaxKind.NumberExpression:
                    return BindLiteralExpression((NumberExpressionSyntax)syntax);
                case SyntaxKind.UnaryExpression:
                    return BindUnaryExpression((UnaryExpressionSyntax)syntax);
                case SyntaxKind.BinaryExpression:
                    return BindBinaryExpression((BinaryExpressionSyntax)syntax);
                case SyntaxKind.ParExpression:
                    return BindParExpression((ParentesesExpressionSyntax)syntax);
                case SyntaxKind.NameExpression:
                    return BindNameExpression((NameExpressionSyntax)syntax);
                case SyntaxKind.AssignmentExpression:
                    return BindAssignmentExpression((AssignmentExpressionSyntax)syntax);
                case SyntaxKind.CallExpression:
                    return BindCallExpression((CallExpressionSyntax)syntax);
                default:
                    throw new Exception($"Sintaxe não esperada: {syntax.Kind}");
            }
        }

        private BoundExpression BindCallExpression(CallExpressionSyntax syntax)
        {
            if(syntax.Arguments.Count == 1 && LookupType(syntax.Identifier.Text) is TypeSymbol type){
                return BindConversion(type, syntax.Arguments[0]);
            }
            
            var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

            foreach(var argument in syntax.Arguments){
                var boundArgument = BindExpression(argument);
                boundArguments.Add(boundArgument); 
            }
            
            if(!_scope.TryLookupFunction(syntax.Identifier.Text, out var function)){
                _diagnostics.ReportUndefinedFunction(syntax.Identifier.Span, syntax.Identifier.Text);
                return new BoundErrorExpression();
            }
            
            if(syntax.Arguments.Count != function.Parameter.Length){
                _diagnostics.ReportWrongArgumentCount(syntax.Span, function.Name, function.Parameter.Length, syntax.Arguments.Count);
                return new BoundErrorExpression();
            }

            for(var i = 0; i < syntax.Arguments.Count; i++){
                var argument = boundArguments[i];
                var parameter = function.Parameter[i];

                if(argument.Type != parameter.Type){
                    _diagnostics.ReportWrongArgumentType(syntax.Span, parameter.Name, parameter.Type, argument.Type);
                    return new BoundErrorExpression();
                }
            }
            
            return new BoundCallExpression(function, boundArguments.ToImmutable());
        }

        private BoundExpression BindConversion(TypeSymbol type, ExpressionSyntax syntax)
        {
            var expression = BindExpression(syntax);
            var conversion = Conversion.Classify(expression.Type, type);
            if(!conversion.Exists){
                _diagnostics.ReportCannotConvert(syntax.Span, expression.Type, type);
                return new BoundErrorExpression();
            }
            return new BoundConversionExpression(type, expression);
        }

        private BoundStatement BindExpressionStatement(ExpressionStatementSyntax syntax)
        {
            var expression = BindExpression(syntax.Expression, canBeVoid: true);
            return new BoundExpressionStatement(expression);
        }

        private BoundStatement BindBlockStatement(BlockStatementSyntax syntax)
        {
            var statements = ImmutableArray.CreateBuilder<BoundStatement>();
            _scope = new BoundScope(_scope);

            foreach(var statementSyntax in syntax.Statements){
                var statement = BindStatement(statementSyntax);
                statements.Add(statement);
            }

            _scope = _scope.Parent;

            return new BoundBlockStatement(statements.ToImmutable());
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
        {
            var name = syntax.IdentifierToken.Text;
            var boundExpression = BindExpression(syntax.Expression);

            if(!_scope.TryLookupVariable(name, out var variable)){
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return boundExpression;
            }

            if(variable.IsReadOnly){
                _diagnostics.ReportCannotAssign(syntax.EqualsToken.Span, name);
            }

            if(boundExpression.Type != variable.Type){
                _diagnostics.ReportCannotConvert(syntax.Expression.Span, boundExpression.Type, variable.Type);
                return boundExpression;
            }

            return new BoundAssignmentExpression(variable, boundExpression);
        }

        private BoundExpression BindNameExpression(NameExpressionSyntax syntax)
        {
            var name = syntax.IdentifierToken.Text;
            if(string.IsNullOrEmpty(name)){
                return new BoundErrorExpression();
            }

            if(!_scope.TryLookupVariable(name, out var variable)){
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return new BoundErrorExpression();
            }
            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindParExpression(ParentesesExpressionSyntax syntax)
        {
            return BindExpression(syntax.Expression);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        {
            var boundLeft = BindExpression(syntax.Left);
            var boundRight = BindExpression(syntax.Right);
            
            if(boundLeft.Type == TypeSymbol.Error || boundRight.Type == TypeSymbol.Error)
                return new BoundErrorExpression();

            var boundOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);     
            
            if(boundOperator == null){
                _diagnostics.ReportUndefinedBinaryOpetaror(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
                return new BoundErrorExpression();
            }
            return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
            
        }

        private BoundExpression BindLiteralExpression(NumberExpressionSyntax syntax)
        {
            var value = syntax.Value ?? 0;
            return new BoundLiteralExpression(value);
        }


        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
        {
            var boundOperand = BindExpression(syntax.Operand);
            
            if(boundOperand.Type == TypeSymbol.Error)
                return new BoundErrorExpression();
            
            var boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);
            
            if(boundOperator == null){
                _diagnostics.ReportUndefinedUnaryOpetaror(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundOperand.Type);
                return new BoundErrorExpression();
            }
            return new BoundUnaryExpression(boundOperator, boundOperand);

        }
        
        private BoundBinaryOperatorKind? BindBinaryOperatorKind(SyntaxKind kind, Type leftType, Type rightType)
        {
            if (leftType == typeof(int) && rightType == typeof(int))
            {
                switch (kind)
                {
                    case SyntaxKind.PlusToken:
                        return BoundBinaryOperatorKind.Addition;
                    case SyntaxKind.MinusToken:
                        return BoundBinaryOperatorKind.Subtraction;
                    case SyntaxKind.MultToken:
                        return BoundBinaryOperatorKind.Multiplication;
                    case SyntaxKind.DivToken:
                        return BoundBinaryOperatorKind.Division;
                }
            }
            if (leftType == typeof(bool) && rightType == typeof(bool))
            {
                switch (kind)
                {
                    case SyntaxKind.AmpersandToken:
                        return BoundBinaryOperatorKind.LogicalAnd;
                    case SyntaxKind.PipeToken:
                        return BoundBinaryOperatorKind.LogicalOr;
                }
            }
            return null;
        }

        private BoundUnaryOperatorKind? BindUnaryOperatorKind(SyntaxKind kind, Type operandType)
        {
            if(operandType == typeof(int)){
                switch(kind){
                    case SyntaxKind.PlusToken: 
                        return BoundUnaryOperatorKind.LogicalNegation; 
                }
            }
            if(operandType == typeof(bool)){
                switch(kind){
                    case SyntaxKind.BangToken: 
                        return BoundUnaryOperatorKind.Identity; 
                    case SyntaxKind.MinusToken: 
                        return BoundUnaryOperatorKind.Negation; 
                }
            }
            return null;
        }
    }

    private static TypeSymbol LookupType(string name){
        switch(name){
            case "bool": return TypeSymbol.Bool;
            case "int": return TypeSymbol.Int;
            case "string": return TypeSymbol.String;
            default: return null;
        }
    }

}