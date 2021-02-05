using Mono.Cecil.Cil;
using Mug.Compilation;
using Mug.Models.Generator;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using Mug.Models.Parser.NodeKinds.Statements;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Evaluator
{
    public class ExpressionEvaluator
    {
        readonly ILProcessor _il;
        public ExpressionEvaluator(ILProcessor il)
        {
            _il = il;
        }
        Instruction CreateLoad(Token t)
        {
            return t.Kind switch
            {
                TokenKind.ConstantDigit => _il.Create(OpCodes.Ldc_I4, Convert.ToInt32(t.Value)),
                TokenKind.ConstantString => _il.Create(OpCodes.Ldstr, ((string)t.Value).Trim('"')),
            };
        }
        Instruction CreateOperator(OperatorKind kind)
        {
            return kind switch
            {
                OperatorKind.Sum => _il.Create(OpCodes.Add),
                OperatorKind.Subtract => _il.Create(OpCodes.Sub),
                OperatorKind.Multiply => _il.Create(OpCodes.Mul),
                OperatorKind.Divide => _il.Create(OpCodes.Div),
            };
        }
        public void Evaluate(INode expression)
        {
            if (expression is Token t)
                _il.Append(CreateLoad(t));
            else if (expression is ExpressionNode e)
            {
                Evaluate(e.Left);
                _il.Append(CreateOperator(e.Operator));
                Evaluate(e.Right);
            }
        }
    }
}
