using FlowCompiler;
using FluentAssertions;
using System.Runtime.InteropServices;

namespace RuntimeTests
{
    public class Tests
    {

        [DllImport("..\\..\\..\\..\\Flow\\bin\\Debug\\net7.0-windows\\Runtime", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int Evaluate(byte[] input, int programSize, int processorCount, int resultAdress);
    
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void EvaluatesProgram()
        {
            var success = new byte[] { 7, 5, 42 };

            var res = Evaluate(success, success.Length, 1, 2);

            res.Should().Be(success[2]);
        }

        [Test]
        public void AddsTwoNumbers()
        {
            // Arrange
            var program = new AssemblyProgram(
                new List<Instruction>()
                {
                    new Add(1, 5,5,6),
                    new Stop(1)
                },
                new List<IDataElement>() { new DataElement<int>(7), new DataElement<int>(5) },
                new List<int>() {0}).ToBytes();

            // Act
            var res = Evaluate(program, program.Length, 1, 5);

            // Assert
            res.Should().Be(12);
        }
    }
}