using CommunityToolkit.Mvvm.Input;
using FlowCompiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Utils;

namespace FlowUI
{
    public record SelectedExampleChanged(Guid Example) : IMessage;

    public class ExampleViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Guid Id { get; }
        public string Name { get; }

        private string _status;
        public string Status 
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public ExampleViewModel(Guid id, string name, string status)
        {
            Id = id;
            Name = name;
            _status = status;
        }
    }

    public class ExampleBrowserViewModel : INotifyPropertyChanged
    {
        private readonly MessageQueue _messageQueue;

        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly string _failedStatus = char.ConvertFromUtf32(0x274C);
        private readonly string _passedStatus = char.ConvertFromUtf32(0x2705);

        public ExampleBrowserViewModel(
            MessageQueue messageQueue,
            Program program)
        {
            _messageQueue = messageQueue;

            AddExample = new RelayCommand(OnAddExample);

            _messageQueue.Register<LoadedExamplesAddedToCodeEditor>(m => OnProgramLoaded(m.Examples));
            _messageQueue.Register<ExampleRenamedInUI>(OnExampleRenamedInUI);
            _messageQueue.Register<ExampleRunSuccessfully>(OnExampleRunSuccessfully);
        }

        private void OnExampleRunSuccessfully(ExampleRunSuccessfully successfully)
        {
            var example = _exampleList.FirstOrDefault(e => e.Id == successfully.Example);
            if (example is null) return;

            var exampleIndex = _exampleList.IndexOf(example);
            _exampleList[exampleIndex].Status = _passedStatus;
            OnPropertyChanged(nameof(Examples));
        }

        private void OnExampleRenamedInUI(ExampleRenamedInUI uI)
        {
            var example = _exampleList.FirstOrDefault(e => e.Id == uI.Example);
            if (example is null) return;

            var exampleIndex = _exampleList.IndexOf(example);
            _exampleList[exampleIndex] = new(example.Id, example.Name, example.Status);
            OnPropertyChanged(nameof(Examples));
        }

        private void OnProgramLoaded(IReadOnlyList<ExampleUIFormat> examples)
        {
            _exampleList.Clear();
            foreach (var example in examples)
            {
                _exampleList.Add(new(example.Id, example.Name, _failedStatus));
            }
            SelectedExample = _exampleList.FirstOrDefault();
            OnPropertyChanged(nameof(SelectedExample));
        }

        private void OnAddExample()
        {
            var newExample = new ExampleViewModel(
                Guid.NewGuid(),
                $"Example {Examples.Count()}",
                _failedStatus);

            _exampleList.Add(newExample);

            _messageQueue.Send(new UserAddedExample(new (
                newExample.Id,
                newExample.Name,
                [])));

            SelectedExample = newExample;
            OnPropertyChanged(nameof(SelectedExample));
        }

        private ObservableCollection<ExampleViewModel> _exampleList = [];
        public IEnumerable<ExampleViewModel> Examples => _exampleList;

        public ICommand AddExample { get; }

        private ExampleViewModel? _selectedExample;
        public ExampleViewModel? SelectedExample 
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
