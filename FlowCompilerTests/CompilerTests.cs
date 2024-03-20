using FlowCompiler;
using NSubstitute;

namespace FlowCompilerTests
{
    [TestClass]
    public class CompilerTests
    {
        [TestMethod]
        public void ChangedBlockIsRecompiled()
        {
            var blockCompiler = Substitute.For<IBlockCompiler>();

            var compiler = new Compiler(blockCompiler);
            var program = compiler.DefaultProgram();
            compiler.ProgramChanged(new (program, 0, "new line"));

            blockCompiler.Received().Compile(Arg.Any<string>());
        }
    }
}
