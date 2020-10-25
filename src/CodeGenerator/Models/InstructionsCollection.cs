struct InstructionsCollection
{
    public readonly string[] OpCode;
    public readonly string[] Arg;
    public readonly string[] Labels;
    public InstructionsCollection(string[] opCode, string[] arg, string[] labels)
    {
        OpCode = opCode;
        Arg = arg;
        Labels = labels;
    }
    public override string ToString()
    {
        string instructions = "";
        for (int i = 0; i < OpCode.Length; i++)
        {
            instructions += "\n\t"+(Labels[i] != "" ? Labels[i] + ": " : "") + OpCode[i] + " " + Arg[i];
        }
        return instructions;
    }
}