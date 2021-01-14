using Mug.Models.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Generator.Emitter
{
    public class MugEmitter
    {
        readonly String ModuleName;
        readonly StringBuilder Module = new();
        public MugEmitter(string moduleName)
        {
            ModuleName = moduleName;
        }
        void EmitLine(string code)
        {
            Module.AppendLine(code);
        }
        void EmitInstruction(LowCodeInstruction instruction)
        {
            var arguments = "";
            if (instruction.Kind == LowCodeInstructionKind.mov)
            {
                arguments = instruction.Arguments[0].Type + " " + instruction.Arguments[0].Value;
                EmitLine($"  {(string.IsNullOrEmpty(instruction.Label) ? "" : " " + instruction.Label + " = ")} {arguments}");
            }
            else
            {
                for (int i = 0; i < instruction.ArgumentsCount; i++)
                    arguments += (i > 0 ? "," : "") + instruction.Arguments[i].Type + " " + instruction.Arguments[i].Value;
                EmitLine($"  {(string.IsNullOrEmpty(instruction.Label) ? "" : " " + instruction.Label + " = ")} {instruction.Kind} {arguments}");
            }
        }
        public void DefineFunction(string name, string type, LowCodeInstruction[] localScope)
        {
            EmitLine($"define {type} @{(name != "main" ? '"' + name + '"' : name)}()");
            EmitLine("{");
            foreach (var instruction in localScope)
                EmitInstruction(instruction);
            EmitLine("}");
        }
        public string Build()
        {
            return Module.ToString();
        }
    }
}
