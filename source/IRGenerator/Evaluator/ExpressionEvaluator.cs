using Mug.Compilation;
using Mug.Models.Generator;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using Mug.Models.Parser.NodeKinds.Statements;
using System;
using System.Collections.Generic;
using System.Text;

using SymbolTable = System.Collections.Generic.Dictionary<string, Mug.Models.Parser.INode>;
using RedefinitionTable = System.Collections.Generic.Dictionary<string, string>;

namespace Mug.Models.Evaluator
{
    public class ExpressionEvaluator
    {
        readonly SymbolTable _symbolTable;
        readonly RedefinitionTable _redefinitionTable;
        readonly MugParser _errorHandler;
        readonly StringBuilder _builder = new();
        readonly String _nsName;
        INode _type;
        public ExpressionEvaluator(string nsName, SymbolTable symbolTable, MugParser parser, RedefinitionTable redefinitionTable)
        {
            _redefinitionTable = redefinitionTable;
            _nsName = nsName;
            _symbolTable = symbolTable;
            _errorHandler = parser;
        }
        void EmitOperator(OperatorKind kind)
        {
            _builder.Append(kind switch
            {
                OperatorKind.Divide => '/',
                OperatorKind.Multiply => '*',
                OperatorKind.Subtract => '-',
                OperatorKind.Sum => '+',
                _ => ""
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
            else if (expression is InParExpressionNode ie)
            {
                _builder.Append('(');
                Build(ie.Content);
                _builder.Append(')');
            }
            else if (expression is Token t)
            {
                if (t.Kind == TokenKind.ConstantDigit)
                {
                    _builder.Append(t.Value.ToString());
                    if (_type is null)
                        _type = new Token(0, TokenKind.KeyTi32, null, new());
                }
                else if (t.Kind == TokenKind.Identifier)
                {
                    if (!_symbolTable.ContainsKey(t.Value.ToString()))
                        _errorHandler.Throw(t, "Undeclared variable");
                    if (_type is null)
                    {
                        if (_symbolTable[t.Value.ToString()] is ConstantStatement c)
                            _type = c.Type;
                        else if (_symbolTable[t.Value.ToString()] is VariableStatement v)
                            _type = v.Type;
                    }
                    _builder.Append(t.Value.ToString());
                }
            }
            else if (expression is CallStatement c)
            {
                if (c.Name is Token t1 && t1.Kind == TokenKind.Identifier)
                {
                    var paramTypes = new List<INode>();
                    var paramBodies = new List<string>();
                    if (c.Parameters is null)
                        c.Parameters = new();
                    for (int i = 0; i < c.Parameters.Nodes.Length; i++)
                    {
                        paramBodies.Add(new ExpressionEvaluator(_nsName, _symbolTable, _errorHandler, _redefinitionTable).EvaluateExpression(c.Parameters.Nodes[i], out var expressionType));
                        paramTypes.Add(expressionType);
                    }
                    var name = _nsName + '.' + IRGenerator.BuildFunctionCallingName(c, paramTypes.ToArray());
                    if (!_redefinitionTable.TryGetValue(name, out name))
                        _errorHandler.Throw(c, "Undeclared function");
                    _builder.Append(name+'('+string.Join(", ", paramBodies)+')');
                }
            }
        }
        public string EvaluateExpression(INode expression)
        {
            return EvaluateExpression(expression, out _);
        }
        public string EvaluateExpression(INode expression, out INode expressionType)
        {
            Build(expression);
            expressionType = _type;
            return _builder.ToString();
        }
    }
}
