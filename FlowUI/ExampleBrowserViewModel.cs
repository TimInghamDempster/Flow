using CommunityToolkit.Mvvm.Input;
using FlowCompiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Utils;

namespace FlowUI
{
    public record SelectedExampleChanged(Guid Example) : IMessage;

    public class ExampleBrowserViewModel : INotifyPropertyChanged
    {
        private readonly MessageQueue _messageQueue;
        private Program _program;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ExampleBrowserViewModel(
            MessageQueue messageQueue,
            Program program)
        {
            _messageQueue = messageQueue;

            AddExample = new RelayCommand(OnAddExample);
            _program = program;

            _messageQueue.Register<ProgramUpdated>(m => OnProgramUpdated(m.Program));
        }

        private void OnProgramUpdated(Program program)
        {
            /*_program = program;
            
            _exampleList = 
                program.
                Examples.
                Select(id => _exampleStore.Get(id)).
                ToList();

            OnPropertyChanged(nameof(Examples));*/
        }

        private void OnAddExample()
        {
            var newExample = new ExampleUIFormat(
                Guid.NewGuid(),
                $"Example {Examples.Count()}",
                [new([new Space(0,1)])]);

            _exampleList.Add(newExample);

            _messageQueue.Send(new UserAddedExample(newExample));
        }

        private ObservableCollection<ExampleUIFormat> _exampleList = [];
        public IEnumerable<ExampleUIFormat> Examples => _exampleList;

        public ICommand AddExample { get; }

        private ExampleUIFormat? _selectedExample;
        public ExampleUIFormat? SelectedExample 
        {
            get => _selectedExample;
            set
            {
                if (value is null) return;
                _selectedExample = value;
                _messageQueue.Send(new SelectedExampleChanged(value.Id));
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
