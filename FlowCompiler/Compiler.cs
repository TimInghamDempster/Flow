using System.Diagnostics;
using System.Text;

namespace FlowCompiler
{
    public interface IROp { };
    public record LoadConstInt(int Value) : IROp;
    public interface IRLine 
    {
        IReadOnlyList<IROp> Ops { get; }
    };

    public record Expression(IReadOnlyList<IROp> Ops) : IRLine;
    public record ParsedLine(string StyledLine, IRLine IR);

    public interface ICompiler
    {
        ParsedLine CompileLine(string line);

        void BuildAssembly(string exePath, ParsedLine genereatedCode);
    }

    public class Compiler : ICompiler
    {
        public void BuildAssembly(string exePath, ParsedLine generatedCode)
        {
            var preamble = File.ReadAllText(@"Content\Preamble.il");
            var postamble = File.ReadAllText(@"Content\Postamble.il");

            var code = WriteCode(generatedCode.IR);

            var finalIl = $"{preamble}{code}{postamble}";

            var path = Path.Combine(Environment.CurrentDirectory, @"Content\test.il");
            File.WriteAllText(path, finalIl);

            var ilasm = new ProcessStartInfo(@"Content\ilasm.exe", path);
            ilasm.WorkingDirectory = Path.Combine(Environment.CurrentDirectory, "Content");
            var ilProc = Process.Start(ilasm);
            ilProc?.WaitForExit();
        }

        public static string WriteCode(IRLine line)
        {
            var nl = Environment.NewLine;

            var codeStream = new StringBuilder();
            codeStream.Append(nl);

            foreach(var op in line.Ops)
            {
                codeStream.Append(ParseOp(op));
            }

            return codeStream.ToString();
        }

        public static string ParseOp(IROp op)
        {
            var nl = Environment.NewLine;
            return op switch
            {
                LoadConstInt ld => $"ldc.i4 {ld.Value}{nl}",
                _ => throw new NotImplementedException()
            };
        }

        public ParsedLine CompileLine(string line)
        {
            return new(line, new Expression(new List<IROp> { new LoadConstInt(7)}));
        }
    }
}
