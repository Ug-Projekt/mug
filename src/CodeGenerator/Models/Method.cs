struct Method
{
    public readonly string Name;
    public readonly string Type;
    public readonly string Modifiers;
    public readonly string[] Parameters;
    public readonly string[] LocalVariables;
    public readonly bool EntryPoint;
    public readonly short Maxstack;
    public readonly InstructionsCollection Body;
    public Method(string name, string type, string modifiers, string[] parameters, string[] localVariables, bool entryPoint, InstructionsCollection instructions, short maxstack = 8)
    {
        Name = name;
        Type = type;
        Modifiers = modifiers;
        Parameters = parameters;
        LocalVariables = localVariables;
        EntryPoint = entryPoint;
        Maxstack = maxstack;
        Body = instructions;
    }
    public override string ToString() =>
        ".method "+Modifiers+" "+Type+" "+Name+"("+string.Join(", ", Parameters)+") cil managed {\n"
        +".maxstack "+Maxstack
        +"\n.init locals ("+string.Join(",\n", LocalVariables)+"\n)\n"
        +Body
        +"}";
}