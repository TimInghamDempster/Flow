﻿using FlowCompiler;
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

        public CodeEditorViewModel(MessageQueue messageQueue)
        {
            _messageQueue = messageQueue;

            _messageQueue.Register<SelectedExampleChanged>(m => OnSelectedExampleChanged(m.Example));
            _messageQueue.Register<ExampleCompiledForUI>(m => OnSelectedExampleCompiled(m.Example));
            _messageQueue.Register<ExampleRenamedInUI>(m => OnExampleRenamed(m.Example, m.NewName));
        }

        public event Action? NotifyCodeChanged;

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
            OnPropertyChanged(nameof(Code));
            NotifyCodeChanged?.Invoke();
        }

        private void OnSelectedExampleCompiled(ExampleUIFormat example)
        {
            _exampleStore.Update(example.Id, example);
            if(_example.Id == example.Id)
            {
                _example = example;
                OnPropertyChanged(nameof(Code));
                OnPropertyChanged(nameof(Errors));
                OnPropertyChanged(nameof(ErrorVisible));
                NotifyCodeChanged?.Invoke();
            }
        }

        public string Name
        { 
            get => _example.Name;
            set => RenameExample(value);
        }

        public string Code
        {
            get => DeserializeCode();
            set => OnCodeChanged(value);
        }

        public IEnumerable<Token> Tokens => 
            _example.Lines.FirstOrDefault()?.Tokens ?? [];

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

        private string DeserializeCode() =>
            _example.Lines.Any() ?
            _example.Lines.
            Select(
                l => l.Tokens.Select(t => t.Value).Aggregate((a,b) => $"{a} {b}")).
            Aggregate((a,b) => $"{a}{Environment.NewLine}{b}") :
            "";


        private void RenameExample(string value)
        {
            _messageQueue.Send(new ExampleRenamedInUI(_example.Id, value));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public IEnumerable<string> Errors => 
            _example.
            Lines.
            FirstOrDefault()?.
            Tokens?.OfType<ErrorToken>().
            Select(e => e.Error)
            ?? [];

        public Visibility ErrorVisible => 
            Errors.Any() ? Visibility.Visible : Visibility.Collapsed;
    }
}
