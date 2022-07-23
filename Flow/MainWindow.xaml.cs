using System.Windows;
using FlowCompiler;
using Microsoft.Extensions.DependencyInjection;

namespace Flow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();
            serviceCollection.AddSingleton<ICompiler, Compiler>();
            Resources.Add("services", serviceCollection.BuildServiceProvider());

            ToDo.BuildAssembly("");
        }
    }
}
