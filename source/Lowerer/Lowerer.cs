using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using Mug.Models.Parser.NodeKinds.Statements;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Lowerering
{
    public static class Lowerer
    {
        /*private static BlockNode LowerForInStatement(ForLoopStatement statement)
        {
            throw new();
        }*/

        /*private static BlockNode LowerForToStatement(ForLoopStatement statement)
        {
            var result = new BlockNode() { Position = statement.Position };
            result.Add( statement.Counter);

            var whilestatement = new ConditionalStatement()
            {
                Body = statement.Body,
                Kind = TokenKind.KeyWhile,
                Expression = new BooleanExpressionNode()
                {
                    Left = statement.Counter,
                    Position = statement.RightExpression.Position,
                    Operator = OperatorKind.CompareLEQ,
                    Right = statement.Counter
                },
                Position = statement.Position
            };

            whilestatement.Body.Add(new PostfixOperator() { Expression = statement.Counter, Postfix = TokenKind.OperatorIncrement });

            Console.WriteLine((result as INode).Dump());

            return result;
        }*/

        /// <summary>
        /// lowers the for statement to a while statement
        /// </summary>
        /// <returns></returns>
        /*public static BlockNode LowerForStatement(ForLoopStatement statement)
        {
            return statement.Operator switch
            {
                TokenKind.KeyTo => LowerForToStatement(statement),
                TokenKind.KeyIn => LowerForInStatement(statement)
            };
        }*/
    }
}
