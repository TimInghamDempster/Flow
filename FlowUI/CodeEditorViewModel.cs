using FlowCompiler;
using System.ComponentModel;

namespace FlowUI
{
    public class CodeEditorViewModel : INotifyPropertyChanged
    {
        private readonly Context<Test> _testContext;

        public CodeEditorViewModel(Context<Test> testContext)
        {
            _testContext = testContext;
            _testContext.Updated += () => OnPropertyChanged(nameof(Name));
        }

        public string Name => _testContext.Current.Name;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
