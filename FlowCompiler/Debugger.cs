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
        private static extern void Evaluate(ref int val);

        public int LaunchApplication(string exePath)
        {
            int res = -1;
            Evaluate(ref res);
            return res;
        }
    }
}
