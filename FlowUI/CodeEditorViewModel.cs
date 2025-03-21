﻿using FlowCompiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using Utils;

namespace FlowUI
{
    public record RenderableToken(
        Brush Foreground,
        string Text);

    internal record LoadedExamplesAddedToCodeEditor(IReadOnlyList<ExampleUIFormat> Examples) : IMessage;

    public class CodeEditorViewModel : INotifyPropertyChanged
    {
        private ExampleUIFormat _example = new(new(), "", []);
        private readonly MessageQueue _messageQueue;

        private readonly Store<ExampleUIFormat> _exampleStore = new();

        public ObservableCollection<LineEditorViewModel> Lines { get; } = [];

        public CodeEditorViewModel(MessageQueue messageQueue)
        {
            _messageQueue = messageQueue;

            var firstLine = new LineEditorViewModel(new([]));
            firstLine.OnCodeChanged += OnCodeChanged;
            Lines.Add(firstLine);

            _messageQueue.Register<SelectedExampleChanged>(m => OnSelectedExampleChanged(m.Example));
            _messageQueue.Register<ExampleCompiledForUI>(m => OnSelectedExampleCompiled(m.Example));
            _messageQueue.Register<ExampleRenamedInUI>(m => OnExampleRenamed(m.Example, m.NewName));
            _messageQueue.Register<LoadedExamplesCompiled>(m => OnProgramLoaded(m.Examples));
        }

        private void OnProgramLoaded(IReadOnlyList<ExampleUIFormat> examples)
        {
            _exampleStore.Clear();
            foreach (var example in examples)
            {
                _exampleStore.Add(example.Id, example);
            }
            _messageQueue.Send(new LoadedExamplesAddedToCodeEditor(examples));
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
            UpdateCodeView();
        }

        private void UpdateCodeView()
        {
            while (_example.Lines.Count > Lines.Count)
            {
                var newLine = new LineEditorViewModel(_example.Lines[Lines.Count]);
                newLine.OnCodeChanged += OnCodeChanged;
                Lines.Add(newLine);
            }
            while (_example.Lines.Count < Lines.Count)
            {
                Lines.RemoveAt(Lines.Count - 1);
            }
            for (var i = 0; i < _example.Lines.Count; i++)
            {
                Lines[i].LineEdited(_example.Lines[i]);
            }
        }

        private void OnSelectedExampleCompiled(ExampleUIFormat example)
        {
            _exampleStore.Update(example.Id, example);
            _example = example;
            if (_example.Id == example.Id)
            {
                _example = example;
            }
            UpdateCodeView();
        }

        public string Name
        { 
            get => _example.Name;
            set => RenameExample(value);
        }

        private void OnCodeChanged()
        {
            var codeBlock = string.Join(Environment.NewLine, Lines.Select(l => l.CodeRaw));
            _messageQueue.Send(new ExampleCodeModifiedInUI(_example.Id, codeBlock));
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
