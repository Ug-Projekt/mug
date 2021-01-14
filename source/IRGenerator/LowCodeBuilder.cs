using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Generator
{
    public class LowCodeBuilder
    {
        int StackCount = 0;
        List<LowCodeInstruction> _builder { get; set; } = new();
        string Pop()
        {
            return "%a" + (--StackCount).ToString();
        }
        string Push()
        {
            return "%a" + (StackCount++).ToString();
        }
        public string ResultLabel()
        {
            return "%a" + StackCount.ToString();
        }
        public void EmitInstruction(LowCodeInstruction argument)
        {
            _builder.Add(argument);
        }
        public void EmitCode(LowCodeInstruction[] lowCodeBuilder, int countStackIncrement = 0)
        {
            StackCount += countStackIncrement;
            _builder.AddRange(lowCodeBuilder);
        }
        public void EmitOp(LowCodeInstructionKind op)
        {
            var arguments = new LowCodeInstruction() { Kind = op };
            var p = Pop();
            arguments.AddArgument(new LowCodeInstructionArgument() { Type = "i32", Value = Pop() });
            arguments.AddArgument(new LowCodeInstructionArgument() { Type = "", Value = p });
            EmitInstruction(arguments);
        }
        public void EmitInstruction(LowCodeInstructionKind kind, LowCodeInstructionArgument[] arguments, string label = "")
        {
            EmitInstruction(new LowCodeInstruction() { Label = label, Kind = kind });
        }
        public void EmitStoreLocal(string name, string type)
        {
            var arguments = new LowCodeInstruction() { Kind = LowCodeInstructionKind.store };
            arguments.AddArgument(new LowCodeInstructionArgument() { Type = type, Value = Pop() });
            arguments.AddArgument(new LowCodeInstructionArgument() { Type = type + '*', Value = "%"+name });
            EmitInstruction(arguments);
        }
        public void EmitDeclareLocal(string name, string type)
        {
            var arguments = new LowCodeInstruction() { Label = "%"+name, Kind = LowCodeInstructionKind.alloca };
            arguments.AddArgument(new LowCodeInstructionArgument() { Type = type, Value = "" });
            EmitInstruction(arguments);
        }
        public void EmitLoadConst(string value, string type)
        {
            var arguments = new LowCodeInstruction() { Label = Push(), Kind = LowCodeInstructionKind.mov };
            arguments.AddArgument(new LowCodeInstructionArgument() { Type = type, Value = value });
            EmitInstruction(arguments);
        }
        public void EmitRet(string type)
        {
            var arguments = new LowCodeInstruction() { Kind = LowCodeInstructionKind.ret };
            arguments.AddArgument(new LowCodeInstructionArgument() { Type = type, Value = Pop() });
            EmitInstruction(arguments);
        }
        public LowCodeInstruction[] Build()
        {
            return _builder.ToArray();
        }
    }
}
