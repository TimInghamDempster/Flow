using FlowCompiler;
using FluentAssertions;
using NSubstitute;

namespace FlowCompilerTests
{
    [TestClass]
    public class CompilerTests
    {
        [TestMethod]
        public void AddTest()
        {
            // Arrange
            var program = new Program(new List<Test>());
            var test = new Test(Guid.Empty, "test", new List<Declaration>(), new Statement("statement", new List<Expression>()), new List<Declaration>());

            // Act
            var result = Compiler.AddTest(program, test);

            // Assert
            result.Tests.Should().Contain(test);
        }

        [TestMethod]
        public void AddSecondTest()
        {
            // Arrange
            var test1 = new Test(
                Guid.Empty,
                "test",
                new List<Declaration>(),
                new Statement("statement", new List<Expression>()),
                new List<Declaration>());

            var program = new Program(new List<Test> { test1 });

            var test2 = new Test(
                Guid.Empty,
                "test2", 
                new List<Declaration>(), 
                new Statement("statement", new List<Expression>()), 
                new List<Declaration>());

            // Act
            var result = Compiler.AddTest(program, test2);

            // Assert
            result.Tests.Should().Contain(test1);
            result.Tests.Should().Contain(test2);
        }

        [TestMethod]
        public void RemoveTest()
        {
            // Arrange
            var test = new Test(
                Guid.Empty,
                "test",
                new List<Declaration>(),
                new Statement("statement", new List<Expression>()),
                new List<Declaration>());

            var test2 = test with { Name = "test2" };

            var program = new Program(new List<Test> { test, test2 });

            // Act
            var result = Compiler.RemoveTest(program, test);

            // Assert
            result.Tests.Should().NotContain(test);
            result.Tests.Should().Contain(test2);
        }

        [TestMethod]
        public void UpdateTest()
        {
            // Arrange
            var test = new Test(
                Guid.Empty,
                "test",
                new List<Declaration>(),
                new Statement("statement", new List<Expression>()),
                new List<Declaration>());

            var test2 = test with { Name = "test2" };

            var program = new Program(new List<Test> { test });

            // Act
            var result = Compiler.UpdateTest(program, test, test2);

            // Assert
            result.Tests.Should().NotContain(test);
            result.Tests.Should().Contain(test2);
        }
    }
}
