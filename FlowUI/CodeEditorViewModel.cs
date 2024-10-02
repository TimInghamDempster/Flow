using FlowCompiler;
using System.ComponentModel;
using Utils;

namespace FlowUI
{
    public class CodeEditorViewModel : INotifyPropertyChanged
    {
        private Example _example;
        private MessageQueue _messageQueue;
        private readonly Store<Example> _exampleStore;

        public CodeEditorViewModel(MessageQueue messageQueue, Store<Example> exampleStore)
        {
            _messageQueue = messageQueue;
            _exampleStore = exampleStore;
            _example = new(
                new(), 
                "", 
                new List<Declaration>(),
                new("", new List<Expression>()),
                new List<Declaration>());

            _messageQueue.Register<SelectedExampleChanged>(m => OnSelectedExampleChanged(m.Example));
            _messageQueue.Register<ExampleModified>(m => OnExampleModified(m.Example));
        }

        private void OnExampleModified(Example example)
        {
            _example = example;
            OnPropertyChanged(nameof(Name));
        }

        private void OnSelectedExampleChanged(Guid example)
        {
            _example = _exampleStore.Get(example);
            OnPropertyChanged(nameof(Name));
        }

        public string Name
        { 
            get => _example.Name;
            set => RenameTest(value);
        }

        private void RenameTest(string value)
        {
            var newTest = _example with { Name = value };
            _messageQueue.Send(new ExampleModifiedInUI(newTest));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
