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
            var testLine = "1 + 2 * ( 3 + 5 ) / 4 - - 2";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<GoodLine>();
            compiledLine.ToString().Should().Be(testLine);
        }

        [TestMethod]
        public void AcceptsWithBraceAtEnd()
        { 
            var compiler = new Compiler();
            var testLine = "1 + 2 * ( 3 + 5 )";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<GoodLine>();
            compiledLine.ToString().Should().Be(testLine);
        }

        [TestMethod]
        public void AcceptsFloatExpression()
        {
            var compiler = new Compiler();
            var testLine = "1.0 + 2.5f * ( 3.4 + 5 ) / 4.2 - - 2f";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<GoodLine>();
            compiledLine.ToString().Should().Be(testLine);
        }

        [TestMethod]
        public void RejectsUnclosedTerm()
        {
            var compiler = new Compiler();
            var testLine = "1 + 2 * (3 + 5) / 4 - ";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("Value expected"));
        }

        [TestMethod]
        public void RejectsUnbalancedPerens()
        {
            var compiler = new Compiler();
            var testLine = "1 + 2 * (3 + 5 / 4 - 2";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("Unclosed bracket"));
        }

        [TestMethod]
        public void RejectsInvalidTerms()
        {
            var compiler = new Compiler();
            var testLine = "1 + five * (3 + 5 / 4) - 2";
            
            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("Unrecognised character"));
        }

        [TestMethod]
        public void RejectsIncompleteSubExpression()
        {
            var compiler = new Compiler();
            var testLine = "1 + 5 * (3 + 5 / ) - 2";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("Value expected"));
        }

        [TestMethod]
        public void RejectsOverlengthLine()
        {
            var compiler = new Compiler();
            var testLine = "1 + 2 * (3 + 5) / 4 - 2 + 1 + 2 * (3 + 5) / 4 - 2 + 1 + 2 * (3 + 5) / 4 - 2 + 1 + 2 * (3 + 5) / 4 - 2";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("Line too long"));
        }
    }
}