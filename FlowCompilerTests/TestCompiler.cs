using FlowCompiler;
using FluentAssertions;

namespace FlowCompilerTests
{
    [TestClass]
    public class TestCompiler
    {
        private const string _expression = "Expression";
        private const string _step = "Step";
        private const string _structure = "Structure";

        [TestCategory(_expression)]
        [TestMethod]
        public void AcceptsIntExpression()
        {
            var compiler = new Compiler();
            var testLine = "val value = 1 + 2 * ( 3 + 5 ) / 4 - - 2";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<GoodLine>();
            compiledLine.ToString().Should().Be(testLine);
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void AcceptsWithBraceAtEnd()
        {
            var compiler = new Compiler();
            var testLine = "val value = 1 + 2 * ( 3 + 5 )";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<GoodLine>();
            compiledLine.ToString().Should().Be(testLine);
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void AcceptsFloatExpression()
        {
            var compiler = new Compiler();
            var testLine = "val value = 1.0 + 2.5f * ( 3.4 + 5 ) / 4.2 - - 2f";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<GoodLine>();
            compiledLine.ToString().Should().Be(testLine);
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void RejectsUnclosedTerm()
        {
            var compiler = new Compiler();
            var testLine = "val value = 1 + 2 * (3 + 5) / 4 - ";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("Value expected"));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void RejectsUnbalancedPerens()
        {
            var compiler = new Compiler();
            var testLine = "val value = 1 + 2 * (3 + 5 / 4 - 2";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("Unclosed bracket"));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void RejectsInvalidTerms()
        {
            var compiler = new Compiler();
            var testLine = "val value = 1 + five * (3 + 5 / 4) - 2";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("Value expected"));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void RejectsIncompleteSubExpression()
        {
            var compiler = new Compiler();
            var testLine = "val value = 1 + 5 * (3 + 5 / ) - 2";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("Value expected"));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void RejectsOverlengthLine()
        {
            var compiler = new Compiler();
            var testLine = "val value = 1 + 2 * (3 + 5) / 4 - 2 + 1 + 2 * (3 + 5) / 4 - 2 + 1 + 2 * (3 + 5) / 4 - 2 + 1 + 2 * (3 + 5) / 4 - 2";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("Line too long"));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void ValShouldHaveName()
        {
            var compiler = new Compiler();
            var testLine = "val = 1 + 2 * (3 + 5) / 4";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("val must have a name."));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void ValShouldHaveAssignment()
        {
            var compiler = new Compiler();
            var testLine = "val value 1 + 2 * (3 + 5) / 4";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("expression must have an assignment."));
        }

        [TestCategory(_structure)]
        [TestMethod]
        public void LineShouldHaveOpeningKeyword()
        {
            var compiler = new Compiler();
            var testLine = "1 + 2 * (3 + 5)";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
            Should().Contain(e => e.Error.Contains("""Line must start with "val" or "step"."""));
        }


        [TestCategory(_step)]
        [TestMethod]
        public void StepShouldHaveName()
        {
            var compiler = new Compiler();
            var testLine = "step";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
            Should().Contain(e => e.Error.Contains("""step must have a name."""));
        }
    }
}