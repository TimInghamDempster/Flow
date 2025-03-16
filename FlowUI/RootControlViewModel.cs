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

        private Dictionary<Guid, ExampleUIFormat> _examples = new();

        public RootControlViewModel(
            ExampleBrowserViewModel exampleBrowserViewModel, 
            CodeEditorViewModel codeEditorViewModel,
            Program initialProgram,
            MessageQueue messageQueue)
        {
            ExampleBrowserViewModel = exampleBrowserViewModel;
            CodeEditorViewModel = codeEditorViewModel;
            _messageQueue = messageQueue;
            _program = initialProgram;

            messageQueue.Register<ProgramUpdated>(m => OnProgramUpdated(m.Program));
            messageQueue.Register<ExampleCompiledForUI>(OnExampleCompiledForUI);

            SaveProgram = new RelayCommand(OnSaveProgram);
            OpenProgram = new RelayCommand(OnOpenProgram);
        }

        private void OnExampleCompiledForUI(ExampleCompiledForUI message)
        {
            _examples[message.Example.Id] = message.Example;
        }

        private void OnProgramUpdated(Program program)
        {
            _program = program;
        }

        public ExampleBrowserViewModel ExampleBrowserViewModel { get; }
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

                var dto = new ProgramDTO(_program, _examples.Values.ToList());

                File.WriteAllText(filePath, JsonSerializer.Serialize(dto));
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

                var programDto = JsonSerializer.Deserialize<ProgramDTO>(File.ReadAllText(filePath));

                if(programDto is null)
                    return;

                _program = programDto.Program;

                _messageQueue.Send(new ProgramLoaded(programDto));
            }
        }
    }
}
