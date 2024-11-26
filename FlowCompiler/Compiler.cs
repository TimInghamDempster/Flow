using Utils;

namespace FlowCompiler
{
    public record Program(IReadOnlyList<Guid> Examples);

    public record Example(
        Guid Id,
        string Name, 
        IReadOnlyList<Declaration> InitialState,
        Expression Expression,
        IReadOnlyList<Declaration> ExpectedState,
        AssemblyProgram Assembly);

    public record UserAddedExample(Example Example) : IMessage;
    public record ProgramUpdated(Program Program) : IMessage;
    public record ExampleModified(Example Example) : IMessage;
    public record ExampleRenamedInUI(Guid Example, string NewName) : IMessage;
    public record ExampleCodeModifiedInUI(Guid Example, string Code) : IMessage;
    public record ExampleCompiledForUI(ExampleUIFormat Example) : IMessage;

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
                _messageQueue.Send(new ExampleCompiledForUI(
                    new(m.Example.Id, m.Example.Name, [])));
            });

            _messageQueue.Register<ExampleRenamedInUI>(m =>
            {
                var example = _examples.Get(m.Example) with { Name = m.NewName };

                _examples.Update(m.Example, example);
                _messageQueue.Send(new ExampleModified(example));
                _messageQueue.Send(new ProgramUpdated(_program));
            });

            _messageQueue.Register<ExampleCodeModifiedInUI>(m =>
            {
                var example = CompileExample(m.Example, _examples.Get(m.Example).Name, m.Code);
                _examples.Update(example.Item1.Id, example.Item1);
                _messageQueue.Send(new ExampleCompiledForUI(example.Item2));
            });
        }

        public static Program AddExample(Program program, Example example)
        {
            var examples = new List<Guid>(program.Examples) { example.Id };
            return new Program(examples);
        }

        public static (Example, ExampleUIFormat) CompileExample(Guid Id, string name, string code)
        {
            var example = ExampleCompiler.Compile(Id, name, code);

            return example;
        }

        public static Program RemoveExample(Program program, Guid example)
        {
            var examples = new List<Guid>(program.Examples);
            examples.Remove(example);
            return new Program(examples);
        }
    }
}
