using Utils;

namespace FlowCompiler
{
    public record Program(IReadOnlyList<Guid> Examples);

    public record Example(
        Guid Id,
        string Name, 
        IReadOnlyList<Declaration> InitialState,
        Statement Statement,
        IReadOnlyList<Declaration> ExpectedState);

    public record Declaration(string Name, ValueToken Value);

    public record Statement(string Name, IReadOnlyList<Expression> Arguments);

    public record Expression(string Name, IReadOnlyList<Token> Arguments);

    public record UserAddedExample(Example Example) : IMessage;
    public record ProgramUpdated(Program Program) : IMessage;
    public record ExampleModified(Example Example) : IMessage;
    public record ExampleModifiedInUI(Example Example) : IMessage;

    public class Compiler
    {
        private readonly MessageQueue _messageQueue;
        private Program _program;
        private readonly Store<Example> _examples;

        public Compiler(MessageQueue messageQueue, Program initialProgram, Store<Example> examples)
        {
            _messageQueue = messageQueue;
            _program = initialProgram;
            _examples = examples;
            
            _messageQueue.Register<UserAddedExample>(m =>
            {
                _examples.Add(m.Example.Id, m.Example);
                var newProgram = AddExample(_program, m.Example);
                _program = newProgram;
                _messageQueue.Send(new ProgramUpdated(newProgram));
            });

            _messageQueue.Register<ExampleModifiedInUI>(m =>
            {
                _examples.Update(m.Example.Id, m.Example);
                _messageQueue.Send(new ExampleModified(m.Example));
                _messageQueue.Send(new ProgramUpdated(_program));
            });
        }

        public static Program AddExample(Program program, Example example)
        {
            var examples = new List<Guid>(program.Examples) { example.Id };
            return new Program(examples);
        }

        public static Program RemoveTest(Program program, Guid test)
        {
            var tests = new List<Guid>(program.Examples);
            tests.Remove(test);
            return new Program(tests);
        }
    }
}
