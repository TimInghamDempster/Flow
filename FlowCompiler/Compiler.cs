﻿using System.Diagnostics;

namespace FlowCompiler
{
    public record ParsedLine(IReadOnlyList<Token> Tokens);
    public record GoodLine(IReadOnlyList<Token> Tokens) : ParsedLine(Tokens)
    {
        public override string ToString()
        {
            return string.Join(' ', Tokens.Select(t => t.Value));
        }
    }

    public record ErrorLine(IReadOnlyList<Token> Tokens) : ParsedLine(Tokens);


    public record Token(int StartIndex, int EndIndex, string Value);
    public record OpenPeren(int StartIndex, int EndIndex) : Token(StartIndex, EndIndex, "(");
    public record ClosePeren(int StartIndex, int EndIndex) : Token(StartIndex, EndIndex, ")");
    public record Space(int StartIndex, int EndIndex) : Token(StartIndex, EndIndex, " ");
    public record Assignment(int StartIndex, int EndIndex) : Token(StartIndex, EndIndex, "=");
    public record NumberToken(int StartIndex, int EndIndex, string Value) : Token(StartIndex, EndIndex, Value);
    public record IntNum(int StartIndex, int EndIndex, string Value) : NumberToken(StartIndex, EndIndex, Value);
    public record DoubleNum(int StartIndex, int EndIndex, string Value) : NumberToken(StartIndex, EndIndex, Value);
    public record FloatNum(int StartIndex, int EndIndex, string Value) : NumberToken(StartIndex, EndIndex, Value);
    public record Operator(int StartIndex, int EndIndex, string Value) : Token(StartIndex, EndIndex, Value);
    public record ErrorToken(int StartIndex, int EndIndex, string Value, string Error) : Token(StartIndex, EndIndex, Value);
    public record Name(int StartIndex, int EndIndex, string Value) : Token(StartIndex, EndIndex, Value);
    public record Keyword(int StartIndex, int EndIndex, string Value) : Token(StartIndex, EndIndex, Value);
    public record StringLiteral(int StartIndex, int EndIndex, string Value) : Token(StartIndex,EndIndex, Value);

    public interface ICompiler
    {
        ParsedLine CompileLine(string line);

        void BuildDll(string exePath, ParsedLine genereatedCode);
    }

    public class Compiler : ICompiler
    {
        private const int _maxLineLength = 80;
        public void BuildDll(string dllPath, ParsedLine generatedCode)
        {
            if (generatedCode is not GoodLine line) return;

            var code = $" __declspec(dllexport) int test_func() {{ return {line.Tokens.Last().Value};}}";

            var path = Path.Combine(Environment.CurrentDirectory, @"Content\test.c");
            File.WriteAllText(path, code);

            Process compiler = new Process();

            compiler.StartInfo.FileName = "cmd.exe";
            compiler.StartInfo.WorkingDirectory = Path.Combine(Environment.CurrentDirectory, "Content");
            compiler.StartInfo.RedirectStandardInput = true;
            compiler.StartInfo.RedirectStandardOutput = true;
            compiler.StartInfo.RedirectStandardError = true;
            compiler.StartInfo.UseShellExecute = false;

            compiler.Start();
            compiler.StandardInput.WriteLine("\"" + @"C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat" + "\"");
            compiler.StandardInput.WriteLine("cl.exe /LD test.c");
            compiler.StandardInput.WriteLine(@"exit");
            string output = compiler.StandardOutput.ReadToEnd();
            string error = compiler.StandardError.ReadToEnd();
            compiler.WaitForExit();
            compiler.Close();
        }

        public ParsedLine CompileLine(string line)
        {
            return line switch
            {
                var s when s.StartsWith("val") => CompileExpression(s),
                var s when s.StartsWith("step") => CompileStep(s),
                _ => new ErrorLine(new List<Token> { new ErrorToken(0,0,line, """Line must start with "val" or "step".""") })
            };
        }

        private ParsedLine CompileStep(string step)
        {
            var tokens = Tokenize(step, "step");

            if(tokens.Count() > 2)
            {
                for(int i = 0; i < tokens.Count(); i++) 
                {
                    var token = tokens[i];
                    tokens[i] = new ErrorToken(token.StartIndex, token.EndIndex, token.Value, "step can only have one name");
                }
            }

            if(tokens.Count() < 2 )
            {
                tokens.Add(new ErrorToken(1, 1, "", "A step must have a name"));
            }

            if (tokens[1] is not Name)
            {
                var token = tokens[1];
                tokens[1] = new ErrorToken(token.StartIndex, token.EndIndex, token.Value, "A step must have a name");
            }

            return TokensToLine(tokens);
        }

        public ParsedLine CompileExpression(string expression)
        {
            List<Token> tokens = Tokenize(expression, "val");

            if (tokens.LastOrDefault() is not (NumberToken or ClosePeren or Name or StringLiteral))
            {
                tokens.Add(new ErrorToken(expression.Length, expression.Length, "", "Value expected"));
            }

            if (expression.Length > _maxLineLength) tokens.Add(new ErrorToken(expression.Length, expression.Length, "", "Line too long"));

            if (tokens.OfType<OpenPeren>().Count() != tokens.OfType<ClosePeren>().Count())
            {
                tokens.Add(new ErrorToken(expression.Length, expression.Length, "", "Unclosed bracket"));
            }

            if (tokens.Count < 4) tokens.Insert(1, new ErrorToken(1, 1, "", "Expression incomplete"));
            if (tokens.ElementAt(1) is not Name) tokens.Insert(1, new ErrorToken(1, 1, "", "val must have a name."));
            if (tokens.ElementAt(2) is not Assignment) tokens.Insert(1, new ErrorToken(1, 1, "", "expression must have an assignment."));

            if (tokens.OfType<NumberToken>().Any() && tokens.Count > 4)
            {
                var id = tokens.FindIndex(t => t is NumberToken);
                var et = tokens[id];
                tokens[id] = new ErrorToken(et.StartIndex, et.EndIndex, et.Value, "Complex expressions cannot contain numeric literals");
            }
            if (tokens.OfType<StringLiteral>().Any() && tokens.Count > 4)
            {
                var id = tokens.FindIndex(t => t is StringLiteral);
                var et = tokens[id];
                tokens[id] = new ErrorToken(et.StartIndex, et.EndIndex, et.Value, "Complex expressions cannot contain string literals");
            }

            CheckSyntax(tokens, 3);
            return TokensToLine(tokens);
        }

        private static ParsedLine TokensToLine(List<Token> tokens)
        {
            return tokens.OfType<ErrorToken>().Any() ?
                new ErrorLine(tokens) :
                new GoodLine(tokens);
        }

        private List<Token> Tokenize(string expression, string keyword)
        {
            int index = keyword.Length;
            List<Token> tokens = new();

            tokens.Add(new Keyword(0, 3, keyword));

            while (index < expression.Length)
            {
                var nextToken = GetToken(expression, index);
                tokens.Add(nextToken);
                index = nextToken.EndIndex + 1;
            }

            tokens.RemoveAll(t => t is Space);

            return tokens;
        }

        private void CheckSyntax(List<Token> tokens, int skipCount)
        {
            var previousToken = tokens[skipCount];
            for(int i = skipCount + 1; i < tokens.Count; i++)
            {
                var token = tokens[i];

                tokens[i] = ValidateNextToken(previousToken, token);
                previousToken = token;
            }
        }

        private Token ValidateNextToken(Token previousToken, Token token) => (previousToken, token) switch
            {
                (Name, Operator) => token,
                (Operator, Name) => token,
                (Operator, Operator) and {token.Value: "-" } => token,
                (Operator, OpenPeren) => token,
                (ClosePeren, Operator) => token,
                (OpenPeren, Name) => token,
                (Name, ClosePeren) => token,
                (_, ErrorToken) => token,
                (Operator, _) => new ErrorToken(token.StartIndex, token.EndIndex, token.Value, "Value expected"),
                _ => new ErrorToken(token.StartIndex, token.EndIndex, token.Value, "Unknown syntax error")
            };

        private Token GetToken(string line, int index) => line[index] switch
            {
                '(' => new OpenPeren(index, index),
                ')' => new ClosePeren(index, index),
                ' ' => new Space(index, index),
                '+' => new Operator(index, index, "+"),
                '-' => new Operator(index, index, "-"),
                '/' => new Operator(index, index, "/"),
                '*' => new Operator(index, index, "*"),
                '=' => new Assignment(index, index),
                >= '0' and <= '9' => Number(line, index),
                '\"' => GetStringLiteral(line, index),
                _ => GetName(line, index)
            };

        private Token GetStringLiteral(string line, int index)
        {
            var endIndex = index+ 1;

            while(endIndex< line.Length && line[endIndex] != '\"') { endIndex++; }
            endIndex++;

            if(endIndex > line.Length)
            {
                return new ErrorToken(index, endIndex, line.Substring(index), "Unclosed string literal");
            }

            return new StringLiteral(index, endIndex, line.Substring(index, endIndex - index));
        }

        private Name GetName(string line, int index)
        {
            var characters = new List<char>();

            var workingIndex = index;
            while (workingIndex < line.Length)
            {
                var c = line[workingIndex];

                if(c == ' ') break;

                characters.Add(c);

                workingIndex++;
            }

            // This is a bit complicated, during the loop we
            // increment to the next index to TEST, but we want
            // the index of the last char to PASS which is one
            // less becuase the last char to test is always a fail
            // The exception is if we exit on the first or last char
            // tested
            if (workingIndex > index &&
                workingIndex < line.Length)
            {
                workingIndex--;
            }

            var value = string.Concat(characters);

            return new(index, workingIndex, value);
        }

        enum NumberType
        {
            Int,
            Float,
            Double,
        }

        private Token Number(string line, int index)
        {
            var digits = new List<char>();
            var numberType = NumberType.Int;

            var workingIndex = index;
            while(workingIndex< line.Length)
            {
                var c = line[workingIndex];

                if (c == '.') numberType = NumberType.Double;

                if (c == 'f')
                {
                    numberType = NumberType.Float;
                }

                if (char.IsDigit(c) || c == '.' || c == 'f')
                {
                    digits.Add(c);
                }
                else
                {
                    break;
                }
                workingIndex++;
            }

            // This is a bit complicated, during the loop we
            // increment to the next index to TEST, but we want
            // the index of the last char to PASS which is one
            // less becuase the last char to test is always a fail
            // The exception is if we exit on the first or last char
            // tested
            if(workingIndex > index &&
                workingIndex < line.Length)
            {
                workingIndex--;
            }

            var value = string.Concat(digits);

            return numberType switch
            {
                NumberType.Int => new IntNum(index, workingIndex, value),
                NumberType.Double => new DoubleNum(index, workingIndex, value),
                NumberType.Float => new FloatNum(index, workingIndex, value),
                _ => new ErrorToken(index, workingIndex, value, "Invalid number format")
            };
        }

    }
}
