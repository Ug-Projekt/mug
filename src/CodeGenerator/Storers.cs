using System.Collections.Generic;
partial class CodeGenerator
{
    void StoreCallFunctionStatement()
    {
        // support for const only
        List<string> paramsTypes = new List<string>();
        foreach (var elem in Current.Item1.ElementBody.Elements)
        {
            string instruction = "";
            if (elem.ElementValue is ConstPrimitiveTString)
                instruction = "ldstr";
            else if (elem.ElementValue is ConstPrimitiveTInt)
                instruction = "ld.i4.s";
            paramsTypes.Add(instruction.Replace("ldstr", "string").Replace("ld.i4.s", "int32"));
            Emitter.Emit(instruction, elem.ElementValue.Value);
        }
        Emitter.Emit("call", GlobalParser.Functions[Current.Item1.ElementValue.Value].Reference == "" ? GlobalParser.Functions[Current.Item1.ElementValue.Value].Data.Type + " " + Current.Item1.ElementValue.Value + "(" + string.Join(", ", paramsTypes) + ")" : GlobalParser.Functions[Current.Item1.ElementValue.Value].Reference);
    }
}