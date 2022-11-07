using System.Runtime.InteropServices;

namespace FlowCompiler
{
    public interface IDebugger
    {
        int LaunchApplication(string exePath);
    }

    public class Debugger : IDebugger
    {
        [DllImport("Runtime", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int Evaluate();

        public int LaunchApplication(string exePath)
        {
            return Evaluate();
        }
    }
}
