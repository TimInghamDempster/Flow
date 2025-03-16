using Utils;

namespace FlowCompiler
{
    public record Program(IReadOnlyList<Guid> Examples);

    internal record Example(
        Guid Id,
        string Name, 
        IReadOnlyList<Declaration> InitialState,
        Expression Expression,
        IReadOnlyList<Declaration> ExpectedState,
        AssemblyProgram Assembly);

    public record UserAddedExample(ExampleUIFormat Example) : IMessage;
    public record ProgramUpdated(Program Program) : IMessage;
    public record ExampleRenamedInUI(Guid Example, string NewName) : IMessage;
    public record ExampleCodeModifiedInUI(Guid Example, string Code) : IMessage;
    public record ExampleCompiledForUI(ExampleUIFormat Example) : IMessage;
    public record ProgramLoaded(ProgramDTO Program) : IMessage;
    public record LoadedExamplesCompiled(IReadOnlyList<ExampleUIFormat> Examples) : IMessage;

    public record ProgramDTO(Program Program, IReadOnlyList<ExampleUIFormat> Examples);

    public class Compiler
    {
        private readonly MessageQueue _messageQueue;
        private Program _program;
        private readonly Store<Example> _examples;

        public Compiler(MessageQueue messageQueue, Program initialProgram)
        {
            _messageQueue = messageQueue;
            _program = initialProgram;
            _examples = new();

            _messageQueue.Register<ProgramLoaded>(m =>
            {
                _program = m.Program.Program;
                _examples.Clear();

                var uiExamples = new List<ExampleUIFormat>();

                foreach (var example in m.Program.Examples)
                {
                    var compiledExample = CompileExample(example.Id, example.Name, example.ToRawCode());
                    _examples.Add(example.Id, compiledExample.Item1);
                    uiExamples.Add(compiledExample.Item2);
                }

                _messageQueue.Send(new LoadedExamplesCompiled(uiExamples));
            });

            _messageQueue.Register<UserAddedExample>(m =>
            {
                var example = CompileExample(
                    m.Example.Id,
                    m.Example.Name,
                    m.Example.ToRawCode());

                _examples.Add(m.Example.Id, example.Item1);
                var newProgram = AddExample(_program, example.Item1);
                _program = newProgram;
                _messageQueue.Send(new ProgramUpdated(newProgram));
                _messageQueue.Send(new ExampleCompiledForUI(example.Item2));
            });

            _messageQueue.Register<ExampleRenamedInUI>(m =>
            {
                var example = _examples.Get(m.Example) with { Name = m.NewName };

                _examples.Update(m.Example, example);
                _messageQueue.Send(new ProgramUpdated(_program));
            });

            _messageQueue.Register<ExampleCodeModifiedInUI>(m =>
            {
                var example = CompileExample(m.Example, _examples.Get(m.Example).Name, m.Code);
                _examples.Update(example.Item1.Id, example.Item1);
                _messageQueue.Send(new ExampleCompiledForUI(example.Item2));
            });
        }

        internal static Program AddExample(Program program, Example example)
        {
            var examples = new List<Guid>(program.Examples) { example.Id };
            return new Program(examples);
        }

        internal static (Example, ExampleUIFormat) CompileExample(Guid Id, string name, string code)
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
