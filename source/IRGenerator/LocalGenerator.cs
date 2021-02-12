﻿using LLVMSharp;
using Mug.Compilation;
using Mug.Models.Generator.Emitter;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using Mug.Models.Parser.NodeKinds.Statements;
using System;
using System.Collections.Generic;

namespace Mug.Models.Generator
{
    public class LocalGenerator
    {
        private readonly MugEmitter _emitter;
        private readonly Dictionary<string, LLVMValueRef> _symbols;
        private readonly FunctionNode _function;
        private readonly IRGenerator _generator;
        public LocalGenerator(IRGenerator errorHandler, Dictionary<string, LLVMValueRef> symbols, ref FunctionNode function, ref MugEmitter emitter)
        {
            _generator = errorHandler;
            _symbols = new Dictionary<string, LLVMValueRef>(symbols);
            _emitter = emitter;
            _function = function;
        }

        private void Error(Range position, params string[] error)
        {
            _generator.Parser.Lexer.Throw(position, error);
        }

        private void EmitOperator(OperatorKind kind)
        {
            switch (kind)
            {
                case OperatorKind.Sum: _emitter.Add(); break;
                case OperatorKind.Subtract: _emitter.Sub(); break;
                case OperatorKind.Multiply: _emitter.Mul(); break;
                case OperatorKind.Divide: _emitter.Div(); break;
                case OperatorKind.Range: break;
            }
        }

        private void EvaluateInstanceName(INode instance)
        {
            switch (instance)
            {
                case Token t:
                    var sym = _symbols.TryGetValue(t.Value, out var symbol);
                    if (!sym)
                        Error(instance.Position, "Undeclared member");
                    _emitter.Load(symbol);
                    break;
                default:
                    Error(instance.Position, "Not supported yet");
                    break;
            }
        }

        private void EvaluateExpression(INode expression)
        {
            switch (expression)
            {
                case ExpressionNode e:
                    EvaluateExpression(e.Left);
                    var ft = _emitter.PeekType();
                    EvaluateExpression(e.Right);
                    var st = _emitter.PeekType();
                    _generator.ExpectSameTypes(ft, e.Position, $"Unsupported operator `{e.Operator}` between different types", st);
                    EmitOperator(e.Operator);
                    break;
                case Token t:
                    if (t.Kind == TokenKind.Identifier)
                        _emitter.LoadFromMemory(t.Value, t.Position);
                    else
                        _emitter.Load(_generator.ConstToLLVMConst(t, t.Position));
                    break;
                case PrefixOperator p:
                    EvaluateExpression(p.Expression);
                    if (p.Prefix == TokenKind.Negation)
                    {
                        _generator.ExpectBoolType(_emitter.PeekType(), p.Position);
                        _emitter.NegBool();
                    }
                    else if (p.Prefix == TokenKind.Minus)
                    {
                        _generator.ExpectIntType(_emitter.PeekType(), p.Position);
                        _emitter.NegInt();
                    }
                    break;
                case CallStatement c:
                    foreach (var parameter in c.Parameters.Nodes)
                        EvaluateExpression(parameter);
                    EvaluateInstanceName(c.Name);
                    _generator.ExpectNonVoidType(_emitter.PeekType(), c.Position);
                    _emitter.Call(c.Parameters.Lenght, false);
                    break;
                default:
                    Error(expression.Position, "expression not supported yet");
                    break;
            }
        }
        public void AllocParameters(LLVMValueRef function, ParameterListNode parameters)
        {
            for (int i = 0; i < parameters.Parameters.Length; i++)
            {
                var parameter = parameters.Parameters[i];
                _emitter.DeclareVariable(parameter.Name, _generator.TypeToLLVMType(parameter.Type, parameter.Position), parameter.Position);
                _emitter.Load(LLVM.GetParam(function, (uint)i));
                _emitter.StoreVariable(parameters.Parameters[i].Name);
            }
        }

        private void RecognizeStatement(INode statement)
        {
            switch (statement)
            {
                case VariableStatement variable:
                    EvaluateExpression(variable.Body);
                    if (!variable.Type.IsAutomatic())
                    {
                        _emitter.DeclareVariable(variable);
                        _generator.ExpectSameTypes(_generator.TypeToLLVMType(variable.Type, variable.Position), variable.Body.Position, "The expression type and the variable type are different", _emitter.PeekType());
                    }
                    else
                        _emitter.DeclareVariable(variable.Name, _emitter.PeekType(), variable.Position);
                    _emitter.StoreVariable(variable.Name);
                    break;
                case ReturnStatement @return:
                    if (@return.IsVoid())
                    {
                        _generator.ExpectSameTypes(_generator.TypeToLLVMType(_function.Type, @return.Position), @return.Position, "Expected non-void expression", LLVM.VoidType());
                        _emitter.RetVoid();
                    }
                    else
                    {
                        EvaluateExpression(@return.Body);
                        _generator.ExpectSameTypes(_generator.TypeToLLVMType(_function.Type, @return.Position), @return.Position, "The function return type and the expression type are different", _emitter.PeekType());
                        _emitter.Ret();
                    }
                    break;
                default:
                    Error(statement.Position, "Statement not supported yet");
                    break;
            }
        }
        public void Generate()
        {
            foreach (var statement in _function.Body.Statements)
                RecognizeStatement(statement);
        }
    }
}