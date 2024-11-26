namespace FlowCompiler
{
    public abstract record Instruction(OpCodes OpCode, uint VectorSize)
    {
        internal abstract IEnumerable<IDataElement> ArgumentData { get; }
    }

    internal static class InstructionExtensions
    {
        internal static IEnumerable<byte> ToBytes(this Instruction instruction) =>
            BitConverter.GetBytes((int)instruction.OpCode).Concat(
                BitConverter.GetBytes(instruction.VectorSize)).Concat(
                instruction.ArgumentData.SelectMany(d => 
                d switch
                {
                    DataElement<int> i => BitConverter.GetBytes(i.Value),
                    _ => throw new NotImplementedException()
                })).ToArray();
    }
}
