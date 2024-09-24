using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows.Input;
using FlowCompiler;
using CommunityToolkit.Mvvm.Input;

namespace FlowUI
{
    public class RootControlViewModel
    {
        private readonly Context<Program> _programContext;

        public RootControlViewModel(
            TestBrowserViewModel testBrowserViewModel, 
            CodeEditorViewModel codeEditorViewModel,
            Context<Program> programContext)
        {
            TestBrowserViewModel = testBrowserViewModel;
            CodeEditorViewModel = codeEditorViewModel;
            _programContext = programContext;

            SaveProgram = new RelayCommand(OnSaveProgram);
            OpenProgram = new RelayCommand(OnOpenProgram);
        }

        public TestBrowserViewModel TestBrowserViewModel { get; }
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

                File.WriteAllText(filePath, JsonSerializer.Serialize(_programContext.Current));
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
                    _programContext.Update(program);
            }
        }
    }
}
