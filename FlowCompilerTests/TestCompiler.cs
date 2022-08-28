using FlowCompiler;
using FluentAssertions;

namespace FlowCompilerTests
{
    [TestClass]
    public class TestCompiler
    {
        [DataRow(1)]
        [DataRow(14)]
        [DataRow(5)]
        [DataRow(1098342)]
        [DataTestMethod]
        public void WritesConstantLoad(int value)
        {
            // Arrange
            var op = new LoadConstInt(value);
            var expected = $"ldc.i4 {value}{Environment.NewLine}";

            // Act
            var result = Compiler.ParseOp(op);

            // Assert
            result.Should().Be(expected);
        }
    }
}