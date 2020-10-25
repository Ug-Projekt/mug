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
                string arg = tok[i].Item2.Value;
                if (tok[i].Item2 is ConstPrimitiveTString) {
                    arg = "\""+tok[i].Item2.Value + "\"";
                    instruction = "ldstr";
                }
                else if (tok[i].Item2 is ConstPrimitiveTInt)
                    instruction = "ld.i4.s";
                paramsTypes.Add(instruction.Replace("ldstr", "string").Replace("ld.i4.s", "int32"));
                Emitter.Emit(instruction, arg);
            }
        }
        Emitter.Emit("call", GlobalParser.Functions[Current.Item1.ElementValue].Reference == "" ? GlobalParser.Functions[Current.Item1.ElementValue].Data.Type + " " + Current.Item1.ElementValue + "(" + string.Join(", ", paramsTypes) + ")" : GlobalParser.Functions[Current.Item1.ElementValue].Reference);
    }
}