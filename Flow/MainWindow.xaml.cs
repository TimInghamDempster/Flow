using FlowCompiler;
using FlowUI;
using System.Windows;
using System.Collections.Generic;

namespace Flow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var initialTest = new Test(
                "Test 0",
                new List<Declaration>(),
                new Statement("New Statement", new List<FlowCompiler.Expression>()),
                new List<Declaration>());

            var programContext = new Context<Program>(
                new Program(new List<Test>() { initialTest}));
            var testContext = new Context<Test>(initialTest);

            var testBrowser = new TestBrowserViewModel(programContext, testContext);
            var codeEditor = new CodeEditorViewModel(testContext);

            DataContext = new RootControlViewModel(testBrowser, codeEditor, programContext);

            InitializeComponent();
        }
    }
}
