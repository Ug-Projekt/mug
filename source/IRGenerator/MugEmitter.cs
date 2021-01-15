using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Generator.Emitter
{
    public class MugEmitter
    {
        readonly StringBuilder Module = new();
        void EmitLine(string code)
        {
            Module.AppendLine(code);
        }
        void Emit(string code)
        {
            Module.Append(code);
        }
        public void DefineFunction(string name, string ctype, string parameters, string code)
        {
            EmitLine(ctype+" "+name+"("+parameters+")");
            EmitLine("{");
            Emit(code);
            EmitLine("}");
        }
        public void DefineInclude(string include)
        {
            EmitLine("#include \""+include+'"');
        }
        public void DefineEntryPoint(string entryPointName)
        {
            DefineFunction("main", "int", "int argcount, char** args", @$"   {entryPointName}();
");
        }
        public string Build()
        {
            return Module.ToString();
        }
    }
}
