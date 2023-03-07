using System.Collections;

public sealed partial class Compilation{
    internal sealed class DiagnosticsBag : IEnumerable<Diagnostics>{
        private readonly List<Diagnostics> _diagnostics = new List<Diagnostics>();

        public object ReportExpressionMustHaveVa { get; internal set; }

        public IEnumerator<Diagnostics> GetEnumerator() => _diagnostics.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void AddRange(DiagnosticsBag diagnostics)
        {
            _diagnostics.AddRange(diagnostics._diagnostics);
        }
        // public void AddConcat(DiagnosticsBag diagnostics)
        // {
        //     _diagnostics.AddRange(diagnostics._diagnostics);
        // }
        private void Report(TextSpan span, string message){
            var diagnostic = new Diagnostics(span, message);
            _diagnostics.Add(diagnostic);
        }

        public void ReportInvalidNumber(TextSpan span, string text, TypeSymbol type)
        {
            var message = $"O número {text} não é um {type} válido!";
            Report(span, message);
        }

        public void ReportBadCharacter(int position, char character)
        {
            var span = new TextSpan(position, 1);
            var message = $"Entrada de caracter ruim: '{character}'!";
            Report(span, message);
        }

        public void ReportUnexpectedToken(TextSpan span, SyntaxKind actualKind, SyntaxKind expectedKind)
        {
            var message = $"Token não esperado! <{actualKind}>. Esperava-se <{expectedKind}!>";
            Report(span, message);
        }

        public void ReportUndefinedUnaryOpetaror(TextSpan span, string operatorText, TypeSymbol operandType)
        {
            var message = $"Operador unário '{operatorText}' não está definido para o tipo {operandType}!";
            Report(span, message);
        }

        public void ReportUndefinedBinaryOpetaror(TextSpan span, string operatorText, TypeSymbol leftType, TypeSymbol rightType)
        {
            var message = $"Operador binário '{operatorText}' não está definido para o tipo {leftType} e {rightType}!";
            Report(span, message);
        }

        public void ReportUndefinedName(TextSpan span, string name)
        {
            var message = $"Variável '{name}' não existe!";
            Report(span, message);
        }

        public void ReportVariableAlreadyDeclared(TextSpan span, string name)
        {
            var message = $"Variável '{name}' já foi declarada!";
            Report(span, message);
        }

        public void ReportCannotConvert(TextSpan span, TypeSymbol fromType, TypeSymbol toType)
        {
            var message = $"Não é possível converter o tipo '{fromType}' para '{toType}!'";
            Report(span, message);
        }

        public void ReportCannotAssign(TextSpan span, string name)
        {
            var message = $"A variável '{name}' é Somente-Leitura e não pode ser atribuida!";
            Report(span, message);
        }

        public void ReportUnterminatedString(TextSpan span)
        {
            var message = $"String literal não terminada!";
            Report(span, message);
        }

        public void ReportUndefinedFunction(TextSpan span, string name)
        {
            var message = $"Função '{name}' não existe!";
            Report(span, message);
        }

        public void ReportWrongArgumentCount(TextSpan span, string name, int expectedCount, int actualCount)
        {
            var message = $"Função '{name}' requer {expectedCount} argumentos, porém, foi dado {actualCount}!";
            Report(span, message);
        }

        public void ReportWrongArgumentType(TextSpan span, string name, TypeSymbol expectedType, TypeSymbol actualType)
        {
            var message = $"Parametro '{name}' requer um valor do tipo '{name}', porém, foi dado um valor do tipo '{actualType}'!";
            Report(span, message);
        }

        public void ReportExpressionMustHaveValue(TextSpan span)
        {
            var message = $"Expressão deve ter um valor!";
            Report(span, message);
        }
    }
}