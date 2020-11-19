using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;

class Evaluator
{
    // to replace with EvaluateExpression and checkers
    [Obsolete] public static void EvaluateSimpleExpression(ref Dictionary<string, LowData> variables, TokenCollection tokens, ref Emitter Emitter, ref string type /*param type, type...*/)
    {
        // foreach token in expression (param)
        for (int i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            if (token.Item1 == TokenKind.Const || token.Item1 == TokenKind.Identifier)
            {
                string instruction;
                string arg;
                if (token.Item2 is ConstPrimitiveTString)
                {
                    arg = '"' + token.Item2.Value + '"';
                    type = "string";
                    instruction = "ldstr";
                }
                else if (token.Item2 is ConstPrimitiveTInt)
                {
                    arg = token.Item2.Value.ToString();
                    type = "int32";
                    instruction = "ldc.i4.s";
                }
                else if (token.Item2 is ConstPrimitiveTBool)
                {
                    arg = "";
                    type = "bool";
                    instruction = "ldc.i4." + Convert.ToInt32(token.Item2.Value);
                }
                else
                {
                    arg = variables[token.Item2].LocalPosition.ToString();
                    type = variables[token.Item2].Type;
                    instruction = "ldloc.s";
                }
                Emitter.Emit(instruction, arg);
            }
            else
            {
                // fix error on identifier -> types of left and right can be identifer and const
                var left = tokens[i - 1];
                var right = tokens[i + 1];
                if (left.Item2.GetType() != right.Item2.GetType())
                    CompilationErrors.Add(
                        "Incompatible Types",
                        "Cannot perform operations between two differents types",
                        "Call a cast or box one of two values", token.Item3, null
                        );
                else if (left.Item2 is ConstPrimitiveTString)
                {
                    Emitter.Emit("ldstr", '"' + right.Item2.Value + '"');
                    string method = "";
                    switch (token.Item1) {
                        case TokenKind.SymbolPlus: method = "string [mscorlib]System.String::Concat(string, string)"; break;
                        case TokenKind.OperatorEqualEqual: method = "bool [mscorlib]System.String::Equals(string, string)"; break;                        default:
                            CompilationErrors.Add(
                                "Unimplemented Operation",
                                "Cannot perform the current operation, it's not implemented yet",
                                "Change the operation or call a method to perform the same operation", token.Item3, null
                            );
                            break;
                    }
                    Emitter.Emit("call", method);
                }
                else if (left.Item2 is ConstPrimitiveTInt)
                {
                    Emitter.Emit("ldc.i4.s", right.Item2.Value.ToString());
                    string instruction = "";
                    switch (token.Item1) {
                        // unsupported start and slash
                        case TokenKind.SymbolPlus: instruction = "add"; break;
                        case TokenKind.SymbolMinus: instruction = "sub"; break;
                        case TokenKind.OperatorEqualEqual: instruction = "ceq"; break;
                        case TokenKind.OperatorNotEqual: instruction = "cneq"; break;
                        default:
                            CompilationErrors.Add(
                                "Unimplemented Operation",
                                "Cannot perform the current operation, it's not implemented yet",
                                "Change the operation or call a method to perform the same operation", token.Item3, null
                            );
                            break;
                    }
                    Emitter.Emit(instruction);
                }
                else if (left.Item2 is ConstPrimitiveTBool)
                    CompilationErrors.Add(
                        "Unable To Perform",
                        "Cannot perform operations between types bool",
                        "Change types", token.Item3, null
                        );
                i++;
            }
        }
    }
}