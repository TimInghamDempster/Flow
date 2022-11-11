using FlowCompiler;
using FluentAssertions;

namespace FlowCompilerTests
{
    [TestClass]
    public class TestCompiler
    {
        [TestMethod]
        public void AcceptsIntExpression()
        {
            var compiler = new Compiler();
            var testLine = "1 + 2 * (3 + 5) / 4 - -2";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.StyledLine.Should().Be(testLine);
            compiledLine.Status.Should().Be(LineState.Success);
        }

        [TestMethod]
        public void AcceptsFloatExpression()
        {
            var compiler = new Compiler();
            var testLine = "1.0 + 2.5f * (3.4 + 5) / 4.2 - -2f";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.StyledLine.Should().Be(testLine);
            compiledLine.Status.Should().Be(LineState.Success);
        }

        [TestMethod]
        public void RejectsUnclosedTerm()
        {
            var compiler = new Compiler();
            var testLine = "1 + 2 * (3 + 5) / 4 - ";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.StyledLine.Should().Be(testLine);
            compiledLine.Status.Should().Be(LineState.UnclosedTerm);
        }

        [TestMethod]
        public void RejectsUnbalancedBraces()
        {
            var compiler = new Compiler();
            var testLine = "1 + 2 * (3 + 5 / 4 - 2";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.StyledLine.Should().Be(testLine);
            compiledLine.Status.Should().Be(LineState.UnclosedBraces);
        }

        [TestMethod]
        public void RejectsInvalidTerms()
        {
            var compiler = new Compiler();
            var testLine = "1 + five * (3 + 5 / 4, - 2";
            
            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.StyledLine.Should().Be(testLine);
            compiledLine.Status.Should().Be(LineState.UnrecognisedParameter);
        }


        [TestMethod]
        public void RejectsOverlengthLine()
        {
            var compiler = new Compiler();
            var testLine = "1 + 2 * (3 + 5) / 4 - 2 + 1 + 2 * (3 + 5) / 4 - 2 + 1 + 2 * (3 + 5) / 4 - 2 + 1 + 2 * (3 + 5) / 4 - 2";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.StyledLine.Should().Be(testLine);
            compiledLine.Status.Should().Be(LineState.LineOverlenght);
        }
    }
}