using System.Diagnostics;

namespace FlowCompiler
{
    public class ToDo
    {
        public static void BuildAssembly(string il)
        {
            var path = @"C:\Users\Tim\source\repos\2022\Flow\content\test.il";
            var exePath = @"C:\Users\Tim\source\repos\2022\Flow\content\test.exe";
            File.WriteAllText(path, il);

            var ilasm = new ProcessStartInfo(@"C:\Users\Tim\source\repos\2022\Flow\content\ilasm.exe", path);
            ilasm.WorkingDirectory = @"C:\Users\Tim\source\repos\2022\Flow\content";
            var ilProc =Process.Start(ilasm);
            ilProc?.WaitForExit();

            var testProg = new ProcessStartInfo(exePath);
            testProg.UseShellExecute = true;
            Process.Start(testProg);
        }
    }
}