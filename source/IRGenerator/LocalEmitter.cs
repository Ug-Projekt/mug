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
        public void EmitConstDefining(string type, string name, string body)
        {
            EmitLine("register "+type + " " + name + " = " + body + ";");
        }
        public void EmitCall(string name, string parameters)
        {
            EmitLine(name + '(' + parameters + ");");
        }
        public void EmitVarDefiningWithoutBody(string type, string name)
        {
            EmitLine(type + " " + name + ";");
        }
        public void EmitAssignment(string name, string body)
        {
            EmitLine(name + " = " + body + ";");
        }
        public void EmitCode(string code)
        {
            EmitLine(code);
        }
        public void EmitReturn(string body)
        {
            EmitLine("return " + body + ";");
        }
    }
}
