using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Generator
{
    public class LowCodeInstruction
    {
        public string Label { get; set; }
        public LowCodeInstructionKind Kind { get; set; }
        List<LowCodeInstructionArgument> _arguments { get; set; } = new();
        public void AddArgument(LowCodeInstructionArgument argument)
        {
            _arguments.Add(argument);
        }
        public LowCodeInstructionArgument[] Arguments
        {
            get
            {
                return _arguments.ToArray();
            }
        }
        public Int32 ArgumentsCount
        {
            get
            {
                return _arguments.Count;
            }
        }
    }
}
