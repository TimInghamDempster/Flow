﻿using CommunityToolkit.Mvvm.Input;
using FlowCompiler;
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
        private readonly Store<Example> _exampleStore;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ExampleBrowserViewModel(
            Store<Example> examples,
            MessageQueue messageQueue,
            Program program)
        {
            _messageQueue = messageQueue;

            _exampleStore = examples;

            AddExample = new RelayCommand(OnAddExample);
            _program = program;

            _messageQueue.Register<ProgramUpdated>(m => OnProgramUpdated(m.Program));
        }

        private void OnProgramUpdated(Program program)
        {
            _program = program;
            
            _exampleList = 
                program.
                Examples.
                Select(id => _exampleStore.Get(id)).
                ToList();

            OnPropertyChanged(nameof(Examples));
        }

        private void OnAddExample()
        {
            var newExample = new Example(
                Guid.NewGuid(),
                $"Example {Examples.Count()}",
                Array.Empty<Declaration>(),
                new Expression(new(0, 0, "statement"), []),
                Array.Empty<Declaration>(),
                new([], [], []));

            _messageQueue.Send(new UserAddedExample(newExample));
        }

        private List<Example> _exampleList = [];
        public IEnumerable<Example> Examples => _exampleList;

        public ICommand AddExample { get; }

        private Example? _selectedExample;
        public Example? SelectedExample 
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
