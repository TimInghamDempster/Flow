using FlowCompiler;
using FlowUI;
using System.Windows;
using System.Collections.Generic;
using System;
using Utils;

namespace Flow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var messageQueue = new MessageQueue();

            var initialProgram = new Program(new List<Guid>() {});
            var exampleStore = new Store<Example>();

            var compiler = new Compiler(messageQueue, initialProgram, exampleStore);
            var exampleBrowser = new ExampleBrowserViewModel(exampleStore, messageQueue, initialProgram);
            var codeEditor = new CodeEditorViewModel(messageQueue);

            DataContext = new RootControlViewModel(exampleBrowser, codeEditor, initialProgram, messageQueue);

            InitializeComponent();
        }
    }
}
