using System.Collections.Generic;
partial class CodeGenerator
{
    void StoreCallFunctionStatement()
    {
        List<string> paramsTypes = new List<string>();
        // foreach expression (parameter)
        foreach (AstElement param in Current.Item1.ElementBody.Elements)
        {
            // param.ElementValue
            //   - Expected Type: SyntaxTree
            string type = "";
            Evaluator.EvaluateSimpleExpression(ref Variables, param.ElementValue, ref Emitter, ref type);
            paramsTypes.Add(type);
        }
        FunctionData func;
        if (!GlobalParser.Functions.TryGetValue(Current.Item1.ElementValue.ToString(), out func))
            CompilationErrors.Add(
                "Undefined Function",
                "Cannot find the declared reference",
                "Define the reference or write the correct identifer", Current.Item2, null);
        else
            Emitter.Emit("call", func.Reference == "" ? func.Data.Type + " " + Current.Item1.ElementValue + "(" + string.Join(", ", paramsTypes) + ")" : func.Reference);
    }
}