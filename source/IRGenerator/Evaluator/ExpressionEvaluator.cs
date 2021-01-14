using Mug.Models.Generator;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using System;
using System.Collections.Generic;
using System.Text;

using SymbolTable = System.Collections.Generic.Dictionary<string, Mug.Models.Parser.INode>;

namespace Mug.Models.Evaluator
{
    public class ExpressionEvaluator
    {
        readonly SymbolTable SymbolTable;
        readonly StringBuilder Builder = new();
        public ExpressionEvaluator(ref SymbolTable symbolTable)
        {
            SymbolTable = symbolTable;
        }
        void EmitOperator(OperatorKind kind)
        {
            Builder.Append(kind switch
            {
                OperatorKind.Divide => '/',
                OperatorKind.Multiply => '*',
                OperatorKind.Subtract => '-',
                OperatorKind.Sum => '+',
                _ => "+"
            });
        }
        void Build(INode expression)
        {
            if (expression is ExpressionNode e)
            {
                Build(e.Left);
                EmitOperator(e.Operator);
                Build(e.Right);
            }
            else if (expression is InParExpressionNode i)
            {
                Builder.Append('(');
                Build(i.Content);
                Builder.Append(')');
            }
            else if (expression is Token t)
            {
                if (t.Kind == TokenKind.ConstantDigit)
                    Builder.Append(t.Value.ToString());
            }
        }
        public string EvaluateExpression(INode expression)
        {
            Build(expression);
            return Builder.ToString();
        }
    }
}
