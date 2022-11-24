using System.Diagnostics;

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
    public record NumberToken(int StartIndex, int EndIndex, string Value) : Token(StartIndex, EndIndex, Value);
    public record IntNum(int StartIndex, int EndIndex, string Value) : NumberToken(StartIndex, EndIndex, Value);
    public record DoubleNum(int StartIndex, int EndIndex, string Value) : NumberToken(StartIndex, EndIndex, Value);
    public record FloatNum(int StartIndex, int EndIndex, string Value) : NumberToken(StartIndex, EndIndex, Value);
    public record Operator(int StartIndex, int EndIndex, string Value) : Token(StartIndex, EndIndex, Value);
    public record ErrorToken(int StartIndex, int EndIndex, string Value, string Error) : Token(StartIndex, EndIndex, Value);

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

            var code = $" __declspec(dllexport) int test_func() {{ return {line};}}";

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
            int index = 0;
            List<Token> tokens = new();

            while(index < line.Length )
            {
                var nextToken = GetToken(line, index);
                tokens.Add(nextToken);
                index = nextToken.EndIndex + 1;
            }

            tokens.RemoveAll(t => t is Space);

            if (tokens.LastOrDefault() is not (NumberToken or ClosePeren))
            {
                tokens.Add(new ErrorToken(line.Length, line.Length, "", "Value expected"));
            }

            if (line.Length > _maxLineLength) tokens.Add(new ErrorToken(line.Length, line.Length, "", "Line too long"));

            if (tokens.OfType<OpenPeren>().Count() != tokens.OfType<ClosePeren>().Count())
            {
                tokens.Add(new ErrorToken(line.Length, line.Length, "", "Unclosed bracket"));
            }

            CheckSyntax(tokens);

            return tokens.OfType<ErrorToken>().Any() ?
                new ErrorLine(tokens) :
                new GoodLine(tokens);
        }

        private void CheckSyntax(List<Token> tokens)
        {
            var previousToken = tokens[0];
            for(int i = 1; i < tokens.Count; i++)
            {
                var token = tokens[i];

                tokens[i] = ValidateNextToken(previousToken, token);
                previousToken = token;
            }
        }

        private Token ValidateNextToken(Token previousToken, Token token)
        {
            return (previousToken, token) switch
            {
                (NumberToken, Operator) => token,
                (Operator, NumberToken) => token,
                (Operator, Operator) and {token.Value: "-" } => token,
                (Operator, OpenPeren) => token,
                (ClosePeren, Operator) => token,
                (OpenPeren, NumberToken) => token,
                (NumberToken, ClosePeren) => token,
                (_, ErrorToken) => token,
                (Operator, _) => new ErrorToken(token.StartIndex, token.EndIndex, token.Value, "Value expected"),
                _ => new ErrorToken(token.StartIndex, token.EndIndex, token.Value, "Unknown syntax error")
            };
        }

        private Token GetToken(string line, int index) => line[index] switch
            {
                '(' => new OpenPeren(index, index),
                ')' => new ClosePeren(index, index),
                ' ' => new Space(index, index),
                '+' => new Operator(index, index, "+"),
                '-' => new Operator(index, index, "-"),
                '/' => new Operator(index, index, "/"),
                '*' => new Operator(index, index, "*"),
                >= '0' and <= '9' => Number(line, index),
                _ => new ErrorToken(index, index, line[index].ToString(), "Unrecognised character")
            };

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
