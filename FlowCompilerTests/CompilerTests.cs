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
            var program = new Program(new List<Guid>());
            var test = new Example(
                Guid.Empty, 
                "test",
                new List<Declaration>(),
                new Expression(new(0,0,"statement"), []), 
                new List<Declaration>(),
                new ([], [], []));

            // Act
            var result = Compiler.AddExample(program, test);

            // Assert
            result.Examples.Should().Contain(test.Id);
        }

        [TestMethod]
        public void AddSecondTest()
        {
            // Arrange
            var test1 = new Example(
                Guid.Empty,
                "test",
                new List<Declaration>(),
                new Expression(new(0, 0, "statement"), []),
                new List<Declaration>(),
                new([], [], []));

            var program = new Program(new List<Guid> { test1.Id });

            var test2 = new Example(
                Guid.Empty,
                "test2", 
                new List<Declaration>(), 
                new Expression(new(0, 0, "statement"), []), 
                new List<Declaration>(),
                new([], [], []));

            // Act
            var result = Compiler.AddExample(program, test2);

            // Assert
            result.Examples.Should().Contain(test1.Id);
            result.Examples.Should().Contain(test2.Id);
        }

        [TestMethod]
        public void RemoveTest()
        {
            // Arrange
            var test = new Example(
                Guid.NewGuid(),
                "test",
                new List<Declaration>(),
                new Expression(new(0, 0, "statement"), []),
                new List<Declaration>(),
                new([], [], []));

            var test2 = test with { Id = Guid.NewGuid() };

            var program = new Program(new List<Guid> { test.Id, test2.Id });

            // Act
            var result = Compiler.RemoveExample(program, test.Id);

            // Assert
            result.Examples.Should().NotContain(test.Id);
            result.Examples.Should().Contain(test2.Id);
        }
    }
}
