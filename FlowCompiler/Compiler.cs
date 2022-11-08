using System.Diagnostics;
using System.Text;

namespace FlowCompiler
{
    public interface IROp { };
    public record LoadConstInt(int Value) : IROp;
    public interface IRLine 
    {
        IReadOnlyList<IROp> Ops { get; }
    };

    public record Expression(IReadOnlyList<IROp> Ops) : IRLine;
    public record ParsedLine(string StyledLine, IRLine IR);

    public interface ICompiler
    {
        ParsedLine CompileLine(string line);

        void BuildDll(string exePath, ParsedLine genereatedCode);
    }

    public class Compiler : ICompiler
    {
        public void BuildDll(string dllPath, ParsedLine generatedCode)
        {

            var code = "extern \"C\" { __declspec(dllexport) int test_func() { return 9;}}";

            var path = Path.Combine(Environment.CurrentDirectory, @"Content\test.cpp");
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
            compiler.StandardInput.WriteLine("cl.exe /LD test.cpp");
            compiler.StandardInput.WriteLine(@"exit");
            string output = compiler.StandardOutput.ReadToEnd();
            string error = compiler.StandardError.ReadToEnd();
            compiler.WaitForExit();
            compiler.Close();
        }

        public static string WriteCode(IRLine line)
        {
            var nl = Environment.NewLine;

            var codeStream = new StringBuilder();
            codeStream.Append(nl);

            foreach(var op in line.Ops)
            {
                codeStream.Append(ParseOp(op));
            }

            return codeStream.ToString();
        }

        public static string ParseOp(IROp op)
        {
            var nl = Environment.NewLine;
            return op switch
            {
                LoadConstInt ld => $"ldc.i4 {ld.Value}{nl}",
                _ => throw new NotImplementedException()
            };
        }

        public ParsedLine CompileLine(string line)
        {
            return new(line, new Expression(new List<IROp> { new LoadConstInt(7)}));
        }
    }
}
