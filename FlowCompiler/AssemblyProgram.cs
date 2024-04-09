namespace FlowCompiler
{
    internal interface IDataElement { };
    internal record struct DataElement<T>(T Value) : IDataElement;

    internal record AssemblyProgram(
        IEnumerable<Instruction> Instructions,
        IEnumerable<IDataElement> Data,
        IEnumerable<int> StartInstructionPointers);

    internal static class APExtensions
    {
        internal static byte[] ToBytes(this AssemblyProgram program) =>
            program.Instructions.SelectMany(i => i.ToBytes()).Concat(
            program.Data.SelectMany(d => d switch
            {
                DataElement<int> i => BitConverter.GetBytes(i.Value),
                _ => throw new NotImplementedException()
            })).Concat(
            program.StartInstructionPointers.SelectMany(
                BitConverter.GetBytes)).ToArray();
    }
}
