using System.Diagnostics;

namespace FlowCompiler
{
    public interface ICompiler
    {
        string CompileLine(string line);

        void BuildAssembly(string exePath, string genereatedCode);
    }

    public class Compiler : ICompiler
    {
        public void BuildAssembly(string exePath, string generatedCode)
        {
            var preamble = File.ReadAllText(@"Content\Preamble.il");
            var postamble = File.ReadAllText(@"Content\Postamble.il");

            var nl = Environment.NewLine;
            var code = $"{nl}nop{nl}" +
                            $"ldstr \"{generatedCode}\"{nl}" +
                            $"call       void [mscorlib]System.Console::WriteLine(string){nl}" +
                            $"nop{nl}" +
                            $"call       string[mscorlib] System.Console::ReadLine(){nl}" +
                            $"pop{nl}" +
                            $"ret{nl}";

            var finalIl = $"{preamble}{code}{postamble}";

            var path = Path.Combine(Environment.CurrentDirectory, @"Content\test.il");
            File.WriteAllText(path, finalIl);

            var ilasm = new ProcessStartInfo(@"Content\ilasm.exe", path);
            ilasm.WorkingDirectory = Path.Combine(Environment.CurrentDirectory, "Content");
            var ilProc = Process.Start(ilasm);
            ilProc?.WaitForExit();
        }

        public string CompileLine(string line)
        {
            return line;
        }
    }
}
