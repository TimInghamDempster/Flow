namespace FlowCompiler
{
    public interface ICompiler
    {
        string CompileLine(string line);
    }

    public class Compiler : ICompiler
    {
        public string CompileLine(string line)
        {
            return $" {line}";
        }
    }
}
