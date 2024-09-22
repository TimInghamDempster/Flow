using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace FlowCompiler
{
    public record Program(IReadOnlyList<Test> Tests);
    public record Test(
        string Name, 
        IReadOnlyList<Declaration> InitialState,
        Statement Statement,
        IReadOnlyList<Declaration> ExpectedState);

    public record Declaration(string Name, ValueToken Value);

    public record Statement(string Name, IReadOnlyList<Expression> Arguments);

    public record Expression(string Name, IReadOnlyList<Token> Arguments);

    public static class Compiler
    {
        public static Program AddTest(Program program, Test test)
        {
            var tests = new List<Test>(program.Tests);
            tests.Add(test);
            return new Program(tests);
        }

        public static Program RemoveTest(Program program, Test test)
        {
            var tests = new List<Test>(program.Tests);
            tests.Remove(test);
            return new Program(tests);
        }

        public static Program UpdateTest(Program program, Test original, Test updated)
        {
            var tests = new List<Test>(program.Tests);
            tests.Remove(original);
            tests.Add(updated);
            return new Program(tests);
        }
    }
}
