using FlowUI;
using System.Windows;

namespace Flow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var testBrowser = new TestBrowserViewModel();
            var codeEditor = new CodeEditorViewModel();

            DataContext = new RootControlViewModel(testBrowser, codeEditor);

            InitializeComponent();
        }
    }
}
