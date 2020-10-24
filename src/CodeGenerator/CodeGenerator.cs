using System;
using System.Collections.Generic;
partial class CodeGenerator
{
    Emitter Emitter = new Emitter();
    Ast _ast;
    Tuple<AstElement, int> Current => _ast[ElementIndex];
    int ElementIndex = 0;
    public InstructionsCollection GetMethodAssembly(Ast ast)
    {
        _ast = ast;
        while (ElementIndex < ast.Length)
        {
            MatchStatements();
            Advance();
        }
        Emitter.Emit("ret","", "IL_EOM");
        return Emitter.Instructions;
    }
    void Advance() => ElementIndex++;
    void Advance(int val) => ElementIndex+=val;
}