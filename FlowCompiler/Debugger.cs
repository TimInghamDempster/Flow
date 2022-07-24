using System.Diagnostics;

namespace FlowCompiler
{
    public interface IDebugger
    {
        void LaunchApplication(string exePath);
    }

    public class Debugger : IDebugger
    {
        public void LaunchApplication(string exePath)
        {
            var testProg = new ProcessStartInfo(exePath);
            testProg.UseShellExecute = true;
            Process.Start(testProg);
        }
    }
}
