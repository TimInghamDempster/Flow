using FlowCompiler;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Utils;

namespace FlowUI
{
    public record RenderableToken(
        Brush Foreground,
        string Text);

    public class CodeEditorViewModel : INotifyPropertyChanged
    {
        private ExampleUIFormat _example = new(new(), "", []);
        private readonly MessageQueue _messageQueue;

        private readonly Store<ExampleUIFormat> _exampleStore = new();

        public LineEditorViewModel Line { get; } = new(new([]));

        public CodeEditorViewModel(MessageQueue messageQueue)
        {
            _messageQueue = messageQueue;

            Line.OnCodeChanged += s => OnCodeChanged(s);

            _messageQueue.Register<SelectedExampleChanged>(m => OnSelectedExampleChanged(m.Example));
            _messageQueue.Register<ExampleCompiledForUI>(m => OnSelectedExampleCompiled(m.Example));
            _messageQueue.Register<ExampleRenamedInUI>(m => OnExampleRenamed(m.Example, m.NewName));
        }


        private void OnExampleRenamed(Guid example, string newName)
        {
            var currentExample = _exampleStore.Get(example);
            var updatedExample = currentExample with { Name = newName };
            _exampleStore.Update(example, updatedExample);
            if (_example.Id == example)
            {
                _example = updatedExample;
                OnPropertyChanged(nameof(Name));
            }
        }

        private void OnSelectedExampleChanged(Guid example)
        {
            _example = _exampleStore.Get(example);
            OnPropertyChanged(nameof(Name));
            Line.LineEdited(_example.Lines.FirstOrDefault() ?? new([]));
        }

        private void OnSelectedExampleCompiled(ExampleUIFormat example)
        {
            _exampleStore.Update(example.Id, example);
            _example = example;
            Line.LineEdited(_example.Lines.FirstOrDefault() ?? new([]));
            if (_example.Id == example.Id)
            {
                _example = example;
            }
        }

        public string Name
        { 
            get => _example.Name;
            set => RenameExample(value);
        }

        private void OnCodeChanged(string value)
        {
            _messageQueue.Send(new ExampleCodeModifiedInUI(_example.Id, value));
        }

        private string GetDeclaration(Either<Declaration, ErrorLine> declaration) =>
            declaration switch
            {
                EitherA<Declaration, ErrorLine> d => $"{d.Value.Name}",
                EitherB<Declaration, ErrorLine> e => $"{e.Value.Tokens.First().Value}",
                _ => ""
            };



        private void RenameExample(string value)
        {
            _messageQueue.Send(new ExampleRenamedInUI(_example.Id, value));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
