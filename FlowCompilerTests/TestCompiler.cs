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
        public void RejectsLiterals()
        {
            var compiler = new Compiler();
            var testLine = "val value = one + two * ( three + 5 ) / 4 - - two";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<ErrorLine>();
            compiledLine.Tokens.OfType<ErrorToken>().Should().Contain(e => e.Error.Contains("literal"));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void AcceptsConstInt()
        {
            var compiler = new Compiler();
            var testLine = "val value = 15";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<GoodLine>();
            compiledLine.ToString().Should().Be(testLine);
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void AcceptsConstDouble()
        {
            var compiler = new Compiler();
            var testLine = "val value = 15.0";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<GoodLine>();
            compiledLine.ToString().Should().Be(testLine);
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void AcceptsConstFloat()
        {
            var compiler = new Compiler();
            var testLine = "val value = 1.5f";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<GoodLine>();
            compiledLine.ToString().Should().Be(testLine);
        }
        [TestCategory(_expression)]
        [TestMethod]
        public void AcceptsIntExpression()
        {
            var compiler = new Compiler();
            var testLine = "val value = one + two * ( three + five ) / four - - two";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<GoodLine>();
            compiledLine.ToString().Should().Be(testLine);
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void AcceptsWithBraceAtEnd()
        {
            var compiler = new Compiler();
            var testLine = "val value = one + two * ( three + five )";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<GoodLine>();
            compiledLine.ToString().Should().Be(testLine);
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void RejectsUnclosedTerm()
        {
            var compiler = new Compiler();
            var testLine = "val value = one + two * (three + five) / four - ";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("Value expected"));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void RejectsUnbalancedPerens()
        {
            var compiler = new Compiler();
            var testLine = "val value = one + two * (three + five / four - two";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("Unclosed bracket"));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void RejectsIncompleteSubExpression()
        {
            var compiler = new Compiler();
            var testLine = "val value = one + 5 * (three + five / ) - two";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("Value expected"));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void RejectsOverlengthLine()
        {
            var compiler = new Compiler();
            var testLine = "val value = one + two * (three + five) / four - two + one + two * (three + five) / four - two + 1 + two * (three + five) / four - two + one + two * (three + five) / four - two";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("Line too long"));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void ValShouldHaveName()
        {
            var compiler = new Compiler();
            var testLine = "val = one + two * (three + five) / four";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("val must have a name."));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void ValShouldHaveAssignment()
        {
            var compiler = new Compiler();
            var testLine = "val value one + two * (three + five) / four";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("expression must have an assignment."));
        }

        [TestCategory(_structure)]
        [TestMethod]
        public void LineShouldHaveOpeningKeyword()
        {
            var compiler = new Compiler();
            var testLine = "one + two * (three + five)";

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
            Should().Contain(e => e.Error.Contains("step must have a name"));
        }

        [TestCategory(_step)]
        [TestMethod]
        public void StepCantHaveAnythingAfterName()
        {
            var compiler = new Compiler();
            var testLine = "step name another";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
            Should().Contain(e => e.Error.Contains("step can only have one name"));
        }

        [TestCategory(_step)]
        [TestMethod]
        public void StepNameCannotBeNumber()
        {
            var compiler = new Compiler();
            var testLine = "step 12";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
            Should().Contain(e => e.Error.Contains("step must have a name"));
        }

        [TestCategory(_step)]
        [TestMethod]
        public void StepParses()
        {
            var compiler = new Compiler();
            var testLine = "step SomeStep";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<GoodLine>();
            compiledLine.ToString().Should().Be(testLine);
        }
    }
}