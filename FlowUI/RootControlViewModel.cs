using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows.Input;
using FlowCompiler;
using CommunityToolkit.Mvvm.Input;
using Utils;

namespace FlowUI
{
    public class RootControlViewModel
    {
        private Program _program;
        private readonly MessageQueue _messageQueue;

        public RootControlViewModel(
            ExampleBrowserViewModel testBrowserViewModel, 
            CodeEditorViewModel codeEditorViewModel,
            Program initialProgram,
            MessageQueue messageQueue)
        {
            TestBrowserViewModel = testBrowserViewModel;
            CodeEditorViewModel = codeEditorViewModel;
            _messageQueue = messageQueue;
            _program = initialProgram;

            messageQueue.Register<ProgramUpdated>(m => OnProgramUpdated(m.Program));

            SaveProgram = new RelayCommand(OnSaveProgram);
            OpenProgram = new RelayCommand(OnOpenProgram);
        }

        private void OnProgramUpdated(Program program)
        {
            _program = program;
        }

        public ExampleBrowserViewModel TestBrowserViewModel { get; }
        public CodeEditorViewModel CodeEditorViewModel { get; }

        public ICommand SaveProgram { get; }

        private void OnSaveProgram()
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "Flow Program (*.flow)|*.flow";
            dialog.DefaultExt = ".flow";
            dialog.AddExtension = true;

            if (dialog.ShowDialog() == true)
            {
                var filePath = dialog.FileName;

                File.WriteAllText(filePath, JsonSerializer.Serialize(_program));
            }
        }

        public ICommand OpenProgram { get; }

        private void OnOpenProgram()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Flow Program (*.flow)|*.flow";
            dialog.DefaultExt = ".flow";
            dialog.AddExtension = true;

            if (dialog.ShowDialog() == true)
            {
                var filePath = dialog.FileName;

                var program = JsonSerializer.Deserialize<Program>(File.ReadAllText(filePath));

                if(program is not null)
                    _messageQueue.Send(new ProgramUpdated(program));
            }
        }
    }
}
