using System.Runtime.InteropServices;
using Utils;

namespace FlowCompiler
{
    public class Debugger
    {
        public Debugger(MessageQueue messageQueue)
        {
            messageQueue.Register<ExampleModified>(m => TestExample(m.Example));
        }

        [DllImport("..\\..\\..\\..\\Flow\\bin\\Debug\\net8.0-windows\\Runtime", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr Evaluate(byte[] input, int programSize, int processorCount, int resultAdress);


        private void TestExample(Example example)
        {
            var program = example.Assembly.ToBytes();

            Evaluate(program, program.Length, 1, 10);
        }
    }
}
