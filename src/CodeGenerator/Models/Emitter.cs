using System;
using System.Collections.Generic;
class Emitter
{
    List<string> instructions = new List<string>();
    List<string> args = new List<string>();
    List<string> labels = new List<string>();
    public InstructionCollection Instructions => new InstructionCollection(instructions.ToArray(), args.ToArray(), labels.ToArray());
    public void Emit(string opCode, string arg, string label)
    {
        instructions.Add(opCode);
        args.Add(arg);
        labels.Add(label);
    }
    public void Emit(string opCode, string arg)
    {
        instructions.Add(opCode);
        args.Add(arg);
        labels.Add("");
    }
    public void Emit(string opCode)
    {
        instructions.Add(opCode);
        args.Add("");
        labels.Add("");
    }
    public void EmitLabel(string label)
    {
        instructions.Add("");
        args.Add("");
        labels.Add(label);
    }
}