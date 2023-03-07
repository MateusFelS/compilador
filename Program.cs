﻿using System.Text;
using static Compilation;

internal static class Program2{
    private static void Main(string[] args){
        var showTree = false;
        var variables = new Dictionary<VariableSymbol, object>();
        var textBuilder = new StringBuilder();
        Compilation previous = null;

        while(true){
            Console.ForegroundColor = ConsoleColor.Green;
            if(textBuilder.Length == 0)
                Console.Write("> ");
            else
                Console.Write("| ");
            
            Console.ResetColor();

            var input = Console.ReadLine();
            var isBlank = string.IsNullOrWhiteSpace(input);

            if(string.IsNullOrWhiteSpace(input))
                return;
            
            if(textBuilder.Length == 0){
                if(isBlank)
                    break;
                else if(input == "#showTree"){
                    showTree = !showTree;
                    Console.WriteLine(showTree ? "Mostrando Parse Trees" : "Não será mostrado Parse Trees");
                    continue;
                } else if(input == "#cls"){
                    Console.Clear();
                    continue;
                } else if(input == "#reset"){
                    previous = null;
                    continue;
                }
            }
            
            textBuilder.AppendLine(input);
            var text = textBuilder.ToString();

            var syntaxTree = SyntaxTree.Parse(text);

            if(!isBlank && syntaxTree.Diagnostics.Any())
                continue;

            var compilation = previous == null ? new Compilation(syntaxTree) : previous.ContinueWith(syntaxTree);
            previous = compilation;
            var result = compilation.Evaluate(variables);

            var diagnostics = result.Diagnostics;

            if(showTree){
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                syntaxTree.Root.WriterTo(Console.Out);
                Console.ResetColor();
            }

            if (!result.Diagnostics.Any())
            {
                if(result.Value != null){
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.WriteLine(result.Value);
                    Console.ResetColor();
                }
                previous = compilation;
            }
            else
            {
                foreach (var diagnostic in diagnostics){
                    var lineIndex = syntaxTree.Text.GetLineIndex(diagnostic.Span.Start);
                    var lineNumber = lineIndex + 1;
                    var line = syntaxTree.Text.Lines[lineIndex];
                    var character = diagnostic.Span.Start - line.Start + 1;
                    
                    Console.WriteLine();
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write($"({lineNumber}, {character}): ");
                    Console.WriteLine(diagnostic);
                    Console.ResetColor();

                    var prefixSpan = TextSpan.FromBounds(line.Start, diagnostic.Span.Start);
                    var suffixSpan = TextSpan.FromBounds(diagnostic.Span.End, line.End);

                    var prefix = syntaxTree.Text.ToString(prefixSpan);
                    var error = syntaxTree.Text.ToString(diagnostic.Span);
                    var suffix = syntaxTree.Text.ToString(suffixSpan);

                    Console.Write("    ");
                    Console.Write(prefix);

                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write(error);
                    Console.ResetColor();

                    Console.Write(suffix);
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
            textBuilder.Clear();
        }
    }

}