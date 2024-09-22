using CommunityToolkit.Mvvm.Input;
using FlowCompiler;
using System.ComponentModel;
using System.Windows.Input;

namespace FlowUI
{
    public class TestBrowserViewModel : INotifyPropertyChanged
    {
        private readonly Context<Program> _programContext;
        private readonly Context<Test> _testContext;

        public event PropertyChangedEventHandler? PropertyChanged;

        public TestBrowserViewModel(
            Context<Program> programContext,
            Context<Test> testContext)
        {
            _programContext = programContext;
            _testContext = testContext;
            AddTest = new RelayCommand(OnAddTest);

            _programContext.Updated += () => OnPropertyChanged(nameof(Tests));
        }

        private void OnAddTest()
        {
            var newTest = new Test(
                $"Test {Tests.Count()}",
                Array.Empty<Declaration>(),
                new Statement("New Statement", Array.Empty<Expression>()),
                Array.Empty<Declaration>());

            var newProgram = Compiler.AddTest(_programContext.Current, newTest);

            _programContext.Update(newProgram);
        }

        public IEnumerable<Test> Tests => _programContext.Current.Tests;

        public ICommand AddTest { get; }

        public Test SelectedTest 
        {
            get => _testContext.Current; 
            set => _testContext.Update(value);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
