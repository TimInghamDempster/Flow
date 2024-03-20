using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace FlowCompiler
{
    public record BlockChanged(Test test, int BlockIndex, string NewCode);
    public record Test(
        string Name,
        IReadOnlyList<CodeBlock> Input,
        IReadOnlyList<Step> Code,
        IReadOnlyList<Message> Result)
    {
        public IEnumerable<CodeBlock> CodeBlocks =>
            ((IEnumerable<CodeBlock>)Input).Concat(Code).Concat(Result);
    }

    public interface ICompiler
    {
        Test DefaultProgram();

        Test ProgramChanged(BlockChanged change);

        void SaveProgram(Test program, string path);

        void BuildDll(string exePath, IEnumerable<ILCode> genereatedCode);
        void LoadProgram(string path);
        event Action<Test>? OnProgramLoaded;
    }

    public class Compiler : ICompiler
    {
        private readonly IBlockCompiler _blockCompiler;

        public Compiler(IBlockCompiler blockCompiler)
        {
            _blockCompiler = blockCompiler;
        }

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

        public Test ProgramChanged(BlockChanged change)
        {
            var newBlock = _blockCompiler.Compile(change.NewCode);

            var blocks = change.test.CodeBlocks.ToList();
            blocks[change.BlockIndex] = newBlock;

            return new Test(
                change.test.Name,
                //change.test.Input,
                blocks,
                blocks.OfType<Step>().ToList(),
                change.test.Result);
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
