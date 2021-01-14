using Mug.Models.Generator;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using System;
using System.Collections.Generic;
using System.Text;

using SymbolTable = System.Collections.Generic.Dictionary<string, object>;

namespace Mug.Models.Evaluator
{
    public class ExpressionEvaluator
    {
        readonly SymbolTable SymbolTable;
        readonly LowCodeBuilder Builder = new();
        public ExpressionEvaluator(ref SymbolTable symbolTable)
        {
            SymbolTable = symbolTable;
        }
        void Operate(OperatorKind kind)
        {
            if (kind == OperatorKind.Multiply)
                Builder.EmitOp(LowCodeInstructionKind.mul);
            else if (kind == OperatorKind.Divide)
                Builder.EmitOp(LowCodeInstructionKind.div);
            else if (kind == OperatorKind.Sum)
                Builder.EmitOp(LowCodeInstructionKind.add);
            else if (kind == OperatorKind.Subtract)
                Builder.EmitOp(LowCodeInstructionKind.sub);
        }
        void Build(INode expression)
        {
            if (expression is ExpressionNode e)
            {
                Build(e.Left);
                Build(e.Right);
                Operate(e.Operator);
            }
            else if (expression is Token t)
            {
                if (t.Kind == TokenKind.ConstantDigit)
                    Builder.EmitLoadConst(t.Value.ToString(), "i32");
            }
        }
        public LowCodeInstruction[] EvaluateExpression(INode expression)
        {
            Build(expression);
            return Builder.Build();
        }
    }
}
