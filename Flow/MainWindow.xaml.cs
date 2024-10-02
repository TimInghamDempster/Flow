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

            var initialTest = new Example(
                Guid.NewGuid(),
                "Test 0",
                new List<Declaration>(),
                new Statement("New Statement", new List<FlowCompiler.Expression>()),
                new List<Declaration>());
            

            var initialProgram = new Program(new List<Guid>() {});
            var exampleStore = new Store<Example>();

            var compiler = new Compiler(messageQueue, initialProgram, exampleStore);
            var testBrowser = new ExampleBrowserViewModel(exampleStore, messageQueue, initialProgram);
            var codeEditor = new CodeEditorViewModel(messageQueue, exampleStore);

            DataContext = new RootControlViewModel(testBrowser, codeEditor, initialProgram, messageQueue);

            InitializeComponent();
        }
    }
}
