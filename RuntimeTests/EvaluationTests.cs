using FlowCompiler;
using FluentAssertions;
using System.Runtime.InteropServices;

namespace RuntimeTests
{
    public class Tests
    {

        [DllImport("..\\..\\..\\..\\Flow\\bin\\Debug\\net8.0-windows\\Runtime", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr Evaluate(byte[] input, int programSize, int processorCount, int resultAdress);
    
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void AddsTwoNumbers()
        {
            // Arrange
            var program = new AssemblyProgram(
                new List<Instruction>()
                {
                    new Add(1, 10, 10, 11),
                    new Stop(1, 0, 0, 0)
                },
                new List<IDataElement>() { new DataElement<int>(7), new DataElement<int>(5) },
                new List<int>() {0}).ToBytes();

            // Act
            var res = Marshal.ReadInt32(Evaluate(program, program.Length, 1, 10));

            // Assert
            res.Should().Be(12);
        }

        [Test]
        public void SubtractsTwoNumbers()
        {
            // Arrange
            var program = new AssemblyProgram(
                new List<Instruction>()
                {
                    new Subtract(2, 10, 10, 12),
                    new Stop(1, 0, 0, 0)
                },
                new List<IDataElement>() { 
                    new DataElement<int>(7), 
                    new DataElement<int>(5),
                    new DataElement<int>(4),
                    new DataElement<int>(1)
                },
                new List<int>() { 0 }).ToBytes();

            // Act
            var ptr = Evaluate(program, program.Length, 1, 10);
            var res1 = Marshal.ReadInt32(ptr);
            var res2 = Marshal.ReadInt32(ptr + 4);

            // Assert
            res1.Should().Be(3);
            res2.Should().Be(4);
        }

        [Test]
        public void PerformsTwoOperations()
        {
            // Arrange
            var program = new AssemblyProgram(
                new List<Instruction>()
                {
                    new Add(1, 15, 15, 16),
                    new Add(1, 15, 15, 16),
                    new Stop(1, 0, 0, 0)
                },
                new List<IDataElement>() { new DataElement<int>(7), new DataElement<int>(5) },
                new List<int>() { 0 }).ToBytes();

            // Act
            var res = Marshal.ReadInt32(Evaluate(program, program.Length, 1, 15));

            // Assert
            res.Should().Be(17);
        }

        [Test]
        public void OperatesOnVectors()
        {
            // Arrange
            var program = new AssemblyProgram(
                new List<Instruction>()
                {
                    new Add(4, 10, 10, 14),
                    new Stop(1, 0, 0, 0)
                },
                new List<IDataElement>() { 
                    new DataElement<int>(7), new DataElement<int>(7), new DataElement<int>(7), new DataElement<int>(7),
                    new DataElement<int>(5), new DataElement<int>(5), new DataElement<int>(5), new DataElement<int>(5) },
                new List<int>() { 0 }).ToBytes();

            // Act
            var ptr = Evaluate(program, program.Length, 1, 10);
            var res1 = Marshal.ReadInt32(ptr);
            var res2 = Marshal.ReadInt32(ptr + 4);
            var res3 = Marshal.ReadInt32(ptr + 8);
            var res4 = Marshal.ReadInt32(ptr + 12);

            // Assert
            res1.Should().Be(12);
            res2.Should().Be(12);
            res3.Should().Be(12);
            res4.Should().Be(12);
        }

        [Test]
        public void RunsMultipleThreads()
        { // Arrange
            var program = new AssemblyProgram(
                new List<Instruction>()
                {
                    new Add(1, 22, 21, 20),
                    new Stop(1, 0, 0, 0),
                    new Subtract(1, 23, 21, 20),
                    new Stop(1, 0, 0, 0)
                },
                new List<IDataElement>() {
                    new DataElement<int>(7), new DataElement<int>(5) },
                new List<int>() { 0, 2 }).ToBytes();

            // Act1
            var ptr = Evaluate(program, program.Length, 2, 22);
            var res1 = Marshal.ReadInt32(ptr);
            var res2 = Marshal.ReadInt32(ptr + 4);

            // Assert
            res1.Should().Be(12);
            res2.Should().Be(-2);
        }
    }
}