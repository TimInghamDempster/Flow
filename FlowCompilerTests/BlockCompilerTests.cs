using FlowCompiler;
using FluentAssertions;

namespace FlowCompilerTests
{
    [TestClass]
    public class BlockCompilerTests
    {
        private const string _expression = "Expression";
        private const string _step = "Step";
        private const string _structure = "Structure";
        private const string _message = "Message";
        private const string _test = "Test";

        [TestCategory(_expression)]
        [TestMethod]
        public void RejectsCompoundIntLiterals()
        {
            var compiler = new BlockCompiler();
            var testLine = "val value = one + two * ( three + 5 ) / 4 - - two";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<ErrorLine>();
            compiledLine.Tokens.OfType<ErrorToken>().Should().Contain(e => e.Error.Contains("literal"));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void RejectsCompoundStringLiterals()
        {
            var compiler = new BlockCompiler();
            var testLine = "val value = one + \"three\"";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<ErrorLine>();

            compiledLine.Tokens.OfType<ErrorToken>().Should().Contain(e => e.Error.Contains("literal"));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void AcceptsLiteralInt()
        {
            var compiler = new BlockCompiler();
            var testLine = "val value = 15";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<StatementLine>();
            compiledLine.ToString().Should().Be(testLine);
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void RejectsSingleQuote()
        {
            var compiler = new BlockCompiler();
            var testLine = "val value = \"";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<ErrorLine>();
            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("Unclosed string literal"));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void AcceptsLiteralString()
        {
            var compiler = new BlockCompiler();
            var testLine = "val value = \"fifteen\"";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<StatementLine>();
            compiledLine.ToString().Should().Be(testLine);
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void AcceptsConstDouble()
        {
            var compiler = new BlockCompiler();
            var testLine = "val value = 15.0";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<StatementLine>();
            compiledLine.ToString().Should().Be(testLine);
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void AcceptsConstFloat()
        {
            var compiler = new BlockCompiler();
            var testLine = "val value = 1.5f";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<StatementLine>();
            compiledLine.ToString().Should().Be(testLine);
        }
        [TestCategory(_expression)]
        [TestMethod]
        public void AcceptsIntExpression()
        {
            var compiler = new BlockCompiler();
            var testLine = "val value = one + two * ( three + five ) / four - - two";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<StatementLine>();
            compiledLine.ToString().Should().Be(testLine);
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void AcceptsWithBraceAtEnd()
        {
            var compiler = new BlockCompiler();
            var testLine = "val value = one + two * ( three + five )";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<StatementLine>();
            compiledLine.ToString().Should().Be(testLine);
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void RejectsUnclosedTerm()
        {
            var compiler = new BlockCompiler();
            var testLine = "val value = one + two * (three + five) / four - ";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("Value expected"));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void RejectsUnbalancedPerens()
        {
            var compiler = new BlockCompiler();
            var testLine = "val value = one + two * (three + five / four - two";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("Unclosed bracket"));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void RejectsIncompleteSubExpression()
        {
            var compiler = new BlockCompiler();
            var testLine = "val value = one + 5 * (three + five / ) - two";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("Value expected"));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void RejectsOverlengthLine()
        {
            var compiler = new BlockCompiler();
            var testLine = "val value = one + two * (three + five) / four - two + one + two * (three + five) / four - two + 1 + two * (three + five) / four - two + one + two * (three + five) / four - two";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("Line too long"));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void ValShouldHaveName()
        {
            var compiler = new BlockCompiler();
            var testLine = "val = one + two * (three + five) / four";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("val must have a name."));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void ValShouldHaveAssignment()
        {
            var compiler = new BlockCompiler();
            var testLine = "val value one + two * (three + five) / four";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("expression must have an assignment."));
        }

        [TestCategory(_structure)]
        [TestMethod]
        public void LineShouldHaveOpeningKeyword()
        {
            var compiler = new BlockCompiler();
            var testLine = "one + two * (three + five)";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
            Should().Contain(e => e.Error.Contains("""Line must start with "val" or "step"."""));
        }


        [TestCategory(_step)]
        [TestMethod]
        public void StepShouldHaveName()
        {
            var compiler = new BlockCompiler();
            var testLine = "step";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
            Should().Contain(e => e.Error.Contains("step must have a name"));
        }

        [TestCategory(_step)]
        [TestMethod]
        public void StepCantHaveAnythingAfterName()
        {
            var compiler = new BlockCompiler();
            var testLine = "step name another";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
            Should().Contain(e => e.Error.Contains("step can only have one name"));
        }

        [TestCategory(_step)]
        [TestMethod]
        public void StepNameCannotBeNumber()
        {
            var compiler = new BlockCompiler();
            var testLine = "step 12";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
            Should().Contain(e => e.Error.Contains("step must have a name"));
        }

        [TestCategory(_step)]
        [TestMethod]
        public void StepParses()
        {
            var compiler = new BlockCompiler();
            var testLine = "step SomeStep";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<BlockStartLine>();
            compiledLine.ToString().Should().Be(testLine);
        }

        [TestCategory(_message)]
        [TestMethod]
        public void MessageShouldHaveName()
        {
            var compiler = new BlockCompiler();
            var testLine = "message";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
            Should().Contain(e => e.Error.Contains("message must have a name"));
        }

        [TestCategory(_test)]
        [TestMethod]
        public void TestShouldHaveName()
        {
            var compiler = new BlockCompiler();
            var testLine = "test";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
            Should().Contain(e => e.Error.Contains("test must have a name"));
        }

        [TestCategory(_test)]
        [TestMethod]
        public void TestNameShouldBeExtracted()
        {
            var compiler = new BlockCompiler();
            var testName = "someTest";
            var testLine = $"test {testName}";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<Name>().
            Should().Contain(s => s.Value == testName);
        }

        [TestCategory(_test)]
        [TestMethod]
        public void TestNameShouldBeLabel()
        {
            var compiler = new BlockCompiler();
            var testLine = "test 123";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
            Should().Contain(e => e.Error.Contains("test name must be valid"));
        }

        [TestCategory(_test)]
        [TestMethod]
        public void TestCanOnlyHaveOneName()
        {
            var compiler = new BlockCompiler();
            var testLine = "test name another";

            var compiledLine = compiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
            Should().Contain(e => e.Error.Contains("test can only have one name"));
        }
    }
}