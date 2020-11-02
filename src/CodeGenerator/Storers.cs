using System.Collections.Generic;
partial class CodeGenerator
{
    void StoreCallFunctionStatement()
    {
        // support for const only
        List<string> paramsTypes = new List<string>();
        // foreach expression (parameter)
        foreach (AstElement param in Current.Item1.ElementBody.Elements)
        {
            // foreach token in expression (param)
            var tok = param.ElementValue as SyntaxTree;
            for (int i=0; i < tok.Count; i++)
            {
                //System.Console.WriteLine("Token: "+ tok[i].Item2.GetType());
                string instruction = "";
                string arg = tok[i].Item2.Value.ToString();
                if (tok[i].Item2 is ConstPrimitiveTString) {
                    arg = "\""+ arg + "\"";
                    instruction = "ldstr";
                }
                else if (tok[i].Item2 is ConstPrimitiveTInt)
                    instruction = "ldc.i4.s";
                paramsTypes.Add(instruction.Replace("ldstr", "string").Replace("ldc.i4.s", "int32"));
                Emitter.Emit(instruction, arg);
            }
        }
        Emitter.Emit("call", GlobalParser.Functions[Current.Item1.ElementValue.ToString()].Reference == "" ? GlobalParser.Functions[Current.Item1.ElementValue.ToString()].Data.Type + " " + Current.Item1.ElementValue + "(" + string.Join(", ", paramsTypes) + ")" : GlobalParser.Functions[Current.Item1.ElementValue.ToString()].Reference);
    }
}