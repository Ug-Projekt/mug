using LLVMSharp;
using Mug.Compilation;
using Mug.Models.Generator.Emitter;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using Mug.Models.Parser.NodeKinds.Statements;
using Mug.TypeSystem;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Mug.Models.Generator
{
    public class LocalGenerator
    {
        private readonly MugEmitter _emitter;
        private readonly FunctionNode _function;
        private readonly IRGenerator _generator;
        public LocalGenerator(IRGenerator errorHandler, ref FunctionNode function, ref MugEmitter emitter)
        {
            _generator = errorHandler;
            _emitter = emitter;
            _function = function;
        }

        private void Error(Range position, params string[] error)
        {
            _generator.Parser.Lexer.Throw(position, error);
        }

        private void ExpectOperatorImplementation(LLVMTypeRef type, OperatorKind kind, Range position, params LLVMTypeRef[] supportedOperators)
        {
            for (int i = 0; i < supportedOperators.Length; i++)
                if (Unsafe.Equals(type, supportedOperators[i]))
                    return;
            Error(position, "The expression type does not implement the operator `", kind.ToString(), "`");
        }

        private void EmitOperator(OperatorKind kind, LLVMTypeRef ft, LLVMTypeRef st, Range position)
        {
            _generator.ExpectSameTypes(ft, position, $"Unsupported operator `{kind}` between different types", st);

            switch (kind)
            {
                case OperatorKind.Sum:
                    ExpectOperatorImplementation(ft, kind, position,
                        LLVMTypeRef.Int32Type(),
                        LLVMTypeRef.Int8Type());
                    _emitter.Add();
                    break;
                case OperatorKind.Subtract:
                    ExpectOperatorImplementation(ft, kind, position,
                        LLVMTypeRef.Int32Type(),
                        LLVMTypeRef.Int8Type());
                    _emitter.Sub();
                    break;
                case OperatorKind.Multiply:
                    ExpectOperatorImplementation(ft, kind, position,
                        LLVMTypeRef.Int32Type(),
                        LLVMTypeRef.Int8Type());
                    break;
                case OperatorKind.Divide:
                    ExpectOperatorImplementation(ft, kind, position,
                        LLVMTypeRef.Int32Type(),
                        LLVMTypeRef.Int8Type());
                    _emitter.Div();
                    break;
                case OperatorKind.Range: break;
            }
        }

        private void EmitCastInstruction(MugType type, Range position)
        {
            var expressionType = _emitter.PeekType();
            switch (expressionType.TypeKind) {
                case LLVMTypeKind.LLVMIntegerTypeKind:
                    _emitter.Cast(_generator.TypeToLLVMType(type, position));
                    break;
                default:
                    Error(position, "Cast does not support this type yet");
                    break;
            }
        }

        private string EvaluateInstanceName(INode instance)
        {
            switch (instance)
            {
                case Token t:
                    return t.Value;
                default:
                    Error(instance.Position, "Not supported yet");
                    return null;
            }
        }

        private string BuildName(string name, LLVMTypeRef[] parameters)
        {
            return $"{name}({string.Join(", ", parameters)})";
        }

        private void EmitCallStatement(CallStatement c, bool expectedNonVoid)
        {
            var parameters = new LLVMTypeRef[c.Parameters.Lenght];

            for (int i = 0; i < c.Parameters.Lenght; i++)
            {
                EvaluateExpression(c.Parameters.Nodes[i]);
                parameters[i] = _emitter.PeekType();
                // _generator.ExpectSameTypes(_emitter.PeekType(), c.Parameters.Nodes[i].Position, "The type of the parameter passed does not match with the expected", parameterTypes[i]);
            }

            // function type: <ret_type> <param_types>
            var function = _generator.GetSymbol(BuildName(EvaluateInstanceName(c.Name), parameters), c.Position);
            var functionType = function.TypeOf().GetElementType();

            if (expectedNonVoid)
                _generator.ExpectNonVoidType(
                    functionType.GetElementType(),
                    c.Position);

            _emitter.Call(function, c.Parameters.Lenght, functionType.GetElementType().TypeKind == LLVMTypeKind.LLVMVoidTypeKind);
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
                    EmitOperator(e.Operator, ft, st, e.Position);
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
                    EmitCallStatement(c, true);
                    break;
                case CastExpressionNode ce:
                    EvaluateExpression(ce.Expression);
                    EmitCastInstruction(ce.Type, ce.Position);
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
                case CallStatement c:
                    EmitCallStatement(c, false);
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
