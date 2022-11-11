using System.Diagnostics;
using System.Text;

namespace FlowCompiler
{
    public enum LineState
    {
        Success = 0,
        UnclosedBraces = 1,
        UnrecognisedParameter = 2,
        UnclosedTerm = 4,
        LineOverlength = 8
    }

    public record ParsedLine(string StyledLine, LineState Status);

    public interface ICompiler
    {
        ParsedLine CompileLine(string line);

        void BuildDll(string exePath, ParsedLine genereatedCode);
    }

    public class Compiler : ICompiler
    {
        private const int _maxLineLength = 80;
        public void BuildDll(string dllPath, ParsedLine generatedCode)
        {
            if (generatedCode.Status is not LineState.Success) return;

            var code = $"extern \"C\" {{ __declspec(dllexport) int test_func() {{ return {generatedCode.StyledLine};}} }}";

            var path = Path.Combine(Environment.CurrentDirectory, @"Content\test.cpp");
            File.WriteAllText(path, code);

            Process compiler = new Process();

            compiler.StartInfo.FileName = "cmd.exe";
            compiler.StartInfo.WorkingDirectory = Path.Combine(Environment.CurrentDirectory, "Content");
            compiler.StartInfo.RedirectStandardInput = true;
            compiler.StartInfo.RedirectStandardOutput = true;
            compiler.StartInfo.RedirectStandardError = true;
            compiler.StartInfo.UseShellExecute = false;

            compiler.Start();
            compiler.StandardInput.WriteLine("\"" + @"C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat" + "\"");
            compiler.StandardInput.WriteLine("cl.exe /LD test.cpp");
            compiler.StandardInput.WriteLine(@"exit");
            string output = compiler.StandardOutput.ReadToEnd();
            string error = compiler.StandardError.ReadToEnd();
            compiler.WaitForExit();
            compiler.Close();
        }

        public ParsedLine CompileLine(string line)
        {

            if (line.Length > _maxLineLength) return new(line, LineState.LineOverlength);

            return new(line, LineState.Success);
        }
    }
}
