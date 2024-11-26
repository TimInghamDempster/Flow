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
            var testLine = "val value = one + two * ( three + 5 ) / 4 - - two";

            var compiledLine = ExampleCompiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<ErrorLine>();
            compiledLine.Tokens.OfType<ErrorToken>().Should().Contain(e => e.Error.Contains("literal"));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void RejectsCompoundStringLiterals()
        {
            var testLine = "val value = one + \"three\"";

            var compiledLine = ExampleCompiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<ErrorLine>();

            compiledLine.Tokens.OfType<ErrorToken>().Should().Contain(e => e.Error.Contains("literal"));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void AcceptsLiteralInt()
        {
            var testLine = "val value = 15";

            var compiledLine = ExampleCompiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<StatementLine>();
            compiledLine.ToString().Should().Be(testLine);
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void RejectsSingleQuote()
        {
            var testLine = "val value = \"";

            var compiledLine = ExampleCompiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<ErrorLine>();
            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("Unclosed string literal"));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void AcceptsLiteralString()
        {
            var testLine = "val value = \"fifteen\"";

            var compiledLine = ExampleCompiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<StatementLine>();
            compiledLine.ToString().Should().Be(testLine);
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void AcceptsConstDouble()
        {
            var testLine = "val value = 15.0";

            var compiledLine = ExampleCompiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<StatementLine>();
            compiledLine.ToString().Should().Be(testLine);
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void AcceptsConstFloat()
        {
            var testLine = "val value = 1.5f";

            var compiledLine = ExampleCompiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<StatementLine>();
            compiledLine.ToString().Should().Be(testLine);
        }
        [TestCategory(_expression)]
        [TestMethod]
        public void AcceptsIntExpression()
        {
            var testLine = "val value = one + two * ( three + five ) / four - - two";

            var compiledLine = ExampleCompiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<StatementLine>();
            compiledLine.ToString().Should().Be(testLine);
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void AcceptsWithBraceAtEnd()
        {
            var testLine = "val value = one + two * ( three + five )";

            var compiledLine = ExampleCompiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<StatementLine>();
            compiledLine.ToString().Should().Be(testLine);
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void RejectsUnclosedTerm()
        {
            var testLine = "val value = one + two * (three + five) / four - ";

            var compiledLine = ExampleCompiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("Value expected"));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void RejectsUnbalancedPerens()
        {
            var testLine = "val value = one + two * (three + five / four - two";

            var compiledLine = ExampleCompiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("Unclosed bracket"));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void RejectsIncompleteSubExpression()
        {
            var testLine = "val value = one + 5 * (three + five / ) - two";

            var compiledLine = ExampleCompiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("Value expected"));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void RejectsOverlengthLine()
        {
            var testLine = "val value = one + two * (three + five) / four - two + one + two * (three + five) / four - two + 1 + two * (three + five) / four - two + one + two * (three + five) / four - two";

            var compiledLine = ExampleCompiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("Line too long"));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void ValShouldHaveName()
        {
            var testLine = "val = one + two * (three + five) / four";

            var compiledLine = ExampleCompiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("val must have a name."));
        }

        [TestCategory(_expression)]
        [TestMethod]
        public void ValShouldHaveAssignment()
        {
            var testLine = "val value one + two * (three + five) / four";

            var compiledLine = ExampleCompiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
                Should().Contain(e => e.Error.Contains("expression must have an assignment."));
        }

        [TestCategory(_structure)]
        [TestMethod]
        public void LineShouldHaveOpeningKeyword()
        {
            var testLine = "one + two * (three + five)";

            var compiledLine = ExampleCompiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
            Should().Contain(e => e.Error.Contains("""Line must start with "val" or "step"."""));
        }


        [TestCategory(_step)]
        [TestMethod]
        public void StepShouldHaveName()
        {
            var testLine = "step";

            var compiledLine = ExampleCompiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
            Should().Contain(e => e.Error.Contains("step must have a name"));
        }

        [TestCategory(_step)]
        [TestMethod]
        public void StepCantHaveAnythingAfterName()
        {
            var testLine = "step name another";

            var compiledLine = ExampleCompiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
            Should().Contain(e => e.Error.Contains("step can only have one name"));
        }

        [TestCategory(_step)]
        [TestMethod]
        public void StepNameCannotBeNumber()
        {
            var testLine = "step 12";

            var compiledLine = ExampleCompiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
            Should().Contain(e => e.Error.Contains("step must have a name"));
        }

        [TestCategory(_step)]
        [TestMethod]
        public void StepParses()
        {
            var testLine = "step SomeStep";

            var compiledLine = ExampleCompiler.CompileLine(testLine);

            compiledLine.Should().BeOfType<BlockStartLine>();
            compiledLine.ToString().Should().Be(testLine);
        }

        [TestCategory(_message)]
        [TestMethod]
        public void MessageShouldHaveName()
        {
            var testLine = "message";

            var compiledLine = ExampleCompiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
            Should().Contain(e => e.Error.Contains("message must have a name"));
        }

        [TestCategory(_test)]
        [TestMethod]
        public void TestShouldHaveName()
        {
            var testLine = "test";

            var compiledLine = ExampleCompiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
            Should().Contain(e => e.Error.Contains("test must have a name"));
        }

        [TestCategory(_test)]
        [TestMethod]
        public void TestNameShouldBeExtracted()
        {
            var testName = "someTest";
            var testLine = $"test {testName}";

            var compiledLine = ExampleCompiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<Name>().
            Should().Contain(s => s.Value == testName);
        }

        [TestCategory(_test)]
        [TestMethod]
        public void TestNameShouldBeLabel()
        {
            var testLine = "test 123";

            var compiledLine = ExampleCompiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
            Should().Contain(e => e.Error.Contains("test name must be valid"));
        }

        [TestCategory(_test)]
        [TestMethod]
        public void TestCanOnlyHaveOneName()
        {
            var testLine = "test name another";

            var compiledLine = ExampleCompiler.CompileLine(testLine);

            compiledLine.Tokens.OfType<ErrorToken>().
            Should().Contain(e => e.Error.Contains("test can only have one name"));
        }
    }
}