using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace FlowCompiler
{
    public record CodeBlock(IReadOnlyList<Line> Lines);
    
    public record ChangedLine(string newLine, int lineNumber);

    public interface IProgramModified { }
    public record LineChanged(Test Code, LineChangedData ChangedLine) : IProgramModified;
    public record LineAdded(Test Code, LineAddedData LineAbove) : IProgramModified;
    public record LineRemoved(Test Code, LineRemovedData LineNumber) : IProgramModified;

    public interface IChangeData { }
    public record LineAddedData(CodeBlock Block, int LineAbove) : IChangeData;
    public record LineRemovedData(CodeBlock Block, int LineNumber) : IChangeData;
    public record LineChangedData(CodeBlock Block, int LineNumber, string NewLine) : IChangeData;

    public record Test(
        string Name,
        IReadOnlyList<Message> Input,
        IReadOnlyList<Step> Code,
        IReadOnlyList<Message> Result)
    {
        public IEnumerable<CodeBlock> CodeBlocks =>
            ((IEnumerable<CodeBlock>)Input).Concat(Code).Concat(Result);
    }

    public record Line(string Source, ParsedLine ParsedLine);

    public record Message(IReadOnlyList<Line> Lines) : CodeBlock(Lines);
    public record Step(IReadOnlyList<Line> Lines) : CodeBlock(Lines);

    public interface ILInstruction{}
    public record SetVal(int Value) : ILInstruction;
    public record EndFunc() : ILInstruction;
    public record ILCode(IReadOnlyList<ILInstruction> IL);
    public record ParsedLine(IReadOnlyList<Token> Tokens);
    public record StatementLine(IReadOnlyList<Token> Tokens) : ParsedLine(Tokens)
    {
        public override string ToString()
        {
            return string.Join(' ', Tokens.Select(t => t.Value));
        }
    }
    public record EmitLine(IReadOnlyList<Token> Tokens, ILCode IL) : ParsedLine(Tokens);
    public record TestLine(IReadOnlyList<Token> Tokens) : ParsedLine(Tokens);
    public record BlockStartLine(IReadOnlyList<Token> Tokens) : ParsedLine(Tokens)
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
        Test DefaultProgram();

        Test ProgramChanged(IProgramModified change);

        void SaveProgram(Test program, string path);

        void BuildDll(string exePath, IEnumerable<ILCode> genereatedCode);
        void LoadProgram(string path);
        event Action<Test>? OnProgramLoaded;
    }

    public class Compiler : ICompiler
    {
        private readonly BlockCompiler _blockCompiler = new();

        public Test DefaultProgram() =>
            new Test(
                "DefualtTest",
                new List<Message>()
                {
                    new Message(
                    new List<Line>()
                    {
                        new Line("", new StatementLine(new List<Token>()))
                    }) },
                new List<Step>(),
                new List<Message>());

        public Test ProgramChanged(IProgramModified change)
        {
            return change switch
            {
                LineChanged lineChanged => ProgramChangedImpl(lineChanged.ChangedLine, lineChanged.Code.Name),
                LineAdded lineAdded => ProgramChangedImpl(lineAdded.LineAbove, lineAdded.Code.Name),
                LineRemoved lineRemoved => ProgramChangedImpl(lineRemoved.LineNumber, lineRemoved.Code.Name),
                _ => throw new NotImplementedException()
            };
        }

        private Test ProgramChangedImpl(LineChangedData lineChanged, string name)
        {
            var newLine = _blockCompiler.CompileLine(lineChanged.NewLine);

            var lines = lineChanged.Block.Lines.ToList();
            lines[lineChanged.LineNumber] =
                new(lineChanged.NewLine, newLine);

            if (newLine is TestLine test) name = test.Tokens[1].Value;

            return new Test(
                name,
                new List<Message>(),
                new List<Step>() { new Step(lines) },
                new List<Message>());

        }

        private Test ProgramChangedImpl(LineAddedData lineAdded, string name)
        {
            var lines = lineAdded.Block.Lines.ToList();
            lines.Insert(
                lineAdded.LineAbove + 1,
                new Line("", new StatementLine(new List<Token>())));

            return new Test(
                name,
                new List<Message>(),
                new List<Step>() { new Step(lines) },
                new List<Message>());
        }

        private Test ProgramChangedImpl(LineRemovedData lineRemoved, string name)
        {
            var lines = lineRemoved.Block.Lines.ToList();
            lines.RemoveAt(lineRemoved.LineNumber);

            return new Test(
                name,
                new List<Message>(),
                new List<Step>() { new Step(lines) },
                new List<Message>());
        }

        private string x64FromIL(ILCode iL)
        {
            var code = new StringBuilder($"global test_func\r\n" +
               $"export test_func\r\n" +
               $"\r\n" +
               $"section .text\r\n" +
               $"\r\n" +
               $"test_func:\r\n");

            foreach (var instruction in iL.IL)
            {
                code.Append(instruction switch
                {
                    SetVal setVal => $"        mov    eax,  {setVal.Value}\r\n",
                    EndFunc endFunc => $"        ret\r\n",
                    _ => throw new NotImplementedException()
                });
            }

            return code.ToString();
        }

        public void SaveProgram(Test program, string path)
        {
            var json = JsonSerializer.Serialize(program);

            File.WriteAllText(path, json);
        }

        public void LoadProgram(string path)
        {
            var json = File.ReadAllText(path);
            var test = JsonSerializer.Deserialize<Test>(json);

            if (test is null) return;

            OnProgramLoaded?.Invoke(test);
        }

        public event Action<Test>? OnProgramLoaded;

        public void BuildDll(string dllPath, IEnumerable<ILCode> iLCode)
        {
            var dllName = Path.GetFileNameWithoutExtension(dllPath);

            var code = x64FromIL(iLCode.First());

            var path = Path.Combine(Environment.CurrentDirectory, $"Content/{dllName}.asm");
            File.WriteAllText(path, code);

            Process compiler = new Process();

            compiler.StartInfo.FileName = "cmd.exe";
            compiler.StartInfo.WorkingDirectory = Path.Combine(Environment.CurrentDirectory, "Content");
            compiler.StartInfo.RedirectStandardInput = true;
            compiler.StartInfo.RedirectStandardOutput = true;
            compiler.StartInfo.RedirectStandardError = true;
            compiler.StartInfo.UseShellExecute = false;

            compiler.Start();
            compiler.StandardInput.WriteLine($"nasm -f win64 {dllName}.asm");
            compiler.StandardInput.WriteLine($"golink /dll {dllName}.obj ntdll.dll");
            compiler.StandardInput.WriteLine(@"exit");
            string output = compiler.StandardOutput.ReadToEnd();
            string error = compiler.StandardError.ReadToEnd();
            compiler.WaitForExit();
            compiler.Close();
        }

    }
}
