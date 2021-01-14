using Mug.Models.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Generator.Emitter
{
    public class LocalEmitter
    {
        readonly StringBuilder Scope = new();
        void EmitLine(string code)
        {
            Scope.AppendLine("   "+code);
        }
        public string Build()
        {
            return Scope.ToString();
        }
        public void EmitVarDefining(string type, string name, string body)
        {
            EmitLine(type+" "+name+" = "+body+";");
        }
        public void EmitVarDefiningWithoutBody(string type, string name)
        {
            EmitLine(type + " " + name + ";");
        }
        public void EmitReturn(string body)
        {
            EmitLine("return " + body + ";");
        }
    }
}
