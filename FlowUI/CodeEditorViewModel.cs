using FlowCompiler;
using System.ComponentModel;

namespace FlowUI
{
    public class CodeEditorViewModel : INotifyPropertyChanged
    {
        private readonly Context<Program> _programContext;
        private readonly Context<Test> _testContext;

        public CodeEditorViewModel(Context<Program> programContext, Context<Test> testContext)
        {
            _programContext = programContext;
            _testContext = testContext;
            _testContext.Updated += () => OnPropertyChanged(nameof(Name));
        }

        public string Name
        { 
            get => _testContext.Current.Name;
            set => RenameTest(value);
        }

        private void RenameTest(string value)
        {
            _programContext.Update(Compiler.UpdateTest(
                _programContext.Current,
                _testContext.Current,
                _testContext.Current with { Name = value }));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
