using System.Diagnostics;

namespace FlowCompiler
{
    public interface ICompiler
    {
        string CompileLine(string line);

        void BuildAssembly(string exePath);
    }

    public class Compiler : ICompiler
    {
        public void BuildAssembly(string exePath)
        {
            var il = File.ReadAllText(@"C:\Users\Tim\source\repos\2022\ConsoleApp1\ConsoleApp1\bin\Debug\ConsoleProg.il");
            var path = @"C:\Users\Tim\source\repos\2022\Flow\content\test.il";
            File.WriteAllText(path, il);

            var ilasm = new ProcessStartInfo(@"C:\Users\Tim\source\repos\2022\Flow\content\ilasm.exe", path);
            ilasm.WorkingDirectory = @"C:\Users\Tim\source\repos\2022\Flow\content";
            var ilProc = Process.Start(ilasm);
            ilProc?.WaitForExit();
        }

        public string CompileLine(string line)
        {
            return $" {line}";
        }
    }
}
