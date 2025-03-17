using FlowCompiler;
using System.ComponentModel;
using System.Windows;

namespace FlowUI
{
    public class LineEditorViewModel : INotifyPropertyChanged
    {
        private LineUIFormat _line;

        public LineEditorViewModel(LineUIFormat line)
        {
            _line = line;
            CodeRaw = DeserializeCode();
        }

        public string Code
        {
            get => DeserializeCode();
            set
            {
                CodeRaw = value;
                OnCodeChanged?.Invoke();
            }
        }

        public string CodeRaw { get; private set; } = "";

        private string DeserializeCode() =>
            _line.Tokens.Any() ?
            _line.Tokens.Select(t => t.Value).Aggregate((a, b) => $"{a} {b}") :
            "";

        public IEnumerable<Token> Tokens => _line.Tokens;

        public void LineEdited(LineUIFormat line)
        {
            _line = line;
            CodeRaw = DeserializeCode();
            OnPropertyChanged(nameof(CodeRaw));
            OnPropertyChanged(nameof(Code));
            OnPropertyChanged(nameof(Errors));
            OnPropertyChanged(nameof(ErrorVisible));
            NotifyCodeChanged?.Invoke();
        }

        public IEnumerable<string> Errors =>
           _line.
           Tokens?.OfType<ErrorToken>().
           Select(e => e.Error)
           ?? [];

        public Visibility ErrorVisible =>
            Errors.Any() ? Visibility.Visible : Visibility.Collapsed;

        public event Action? OnCodeChanged;
        public event Action? NotifyCodeChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
