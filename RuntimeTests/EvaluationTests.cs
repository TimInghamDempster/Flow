using FluentAssertions;
using System.Runtime.InteropServices;

namespace RuntimeTests
{
    public class Tests
    {

        [DllImport("..\\..\\..\\..\\Flow\\bin\\Debug\\net7.0-windows\\Runtime", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int Evaluate(int[] input, int programSize);
    
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void EvaluatesProgram()
        {
            var success = new[] { 7, 5, 42 };

            var res = Evaluate(success, success.Length);

            res.Should().Be(success[2]);
        }
    }
}