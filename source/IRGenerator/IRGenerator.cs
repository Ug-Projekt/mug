using LLVMSharp;
using Microsoft.VisualBasic;
using Mug.Models.Generator.Emitter;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using Mug.Models.Parser.NodeKinds.Statements;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mug.Models.Generator
{
    public class IRGenerator
    {
        public readonly MugParser Parser;
        public readonly LLVMModuleRef Module;
        public IRGenerator(string moduleName, string source)
        {
            Parser = new(moduleName, source);
            Parser.Parse();
            Module = LLVM.ModuleCreateWithName(moduleName);
        }
        public IRGenerator(MugParser parser)
        {
            Parser = parser;
            Module = LLVM.ModuleCreateWithName(parser.Lexer.ModuleName);
        }
        LLVMTypeRef PrimitiveTypeToLLVMType(TokenKind primitiveType)
        {
            return primitiveType switch
            {
                TokenKind.KeyTi32 => LLVMTypeRef.Int32Type(),
                TokenKind.KeyTVoid => LLVMTypeRef.VoidType(),
            };
        }
        LLVMTypeRef TypeToLLVMType(INode type)
        {
            return type switch
            {
                Token t => PrimitiveTypeToLLVMType(t.Kind)
            };
        }
        LLVMTypeRef[] ParameterTypesToLLVMTypes(ParameterNode[] parameterTypes)
        {
            var result = new LLVMTypeRef[parameterTypes.Length];
            for (int i = 0; i < parameterTypes.Length; i++)
                result[i] = TypeToLLVMType(parameterTypes[i].Type);
            return result;
        }
        LLVMBasicBlockRef InstallFunction(string name, INode type, ParameterListNode paramTypes)
        {
            var ft = LLVM.FunctionType(
                    TypeToLLVMType(type),
                    ParameterTypesToLLVMTypes(paramTypes.Parameters),
                    false
                );
            var f = LLVM.AddFunction(Module, name, ft);
            return LLVM.AppendBasicBlock(f, "");
        }
        void EmitOperator(ref MugEmitter emitter, OperatorKind kind)
        {
            switch (kind)
            {
                case OperatorKind.Sum: emitter.Add(); break;
                case OperatorKind.Subtract: emitter.Sub(); break;
                case OperatorKind.Multiply: emitter.Mul(); break;
                case OperatorKind.Divide: emitter.Div(); break;
                case OperatorKind.Range: break;
            }
        }
        LLVMValueRef ConstToLLVMConst(Token constant)
        {
            return constant.Kind switch
            {
                TokenKind.ConstantDigit => LLVMTypeRef.ConstInt(LLVMTypeRef.Int32Type(), Convert.ToUInt64(constant.Value), MugEmitter._llvmfalse)
            };
        }
        LLVMValueRef EvaluateExpression(ref MugEmitter emitter, INode expression)
        {
            if (expression is ExpressionNode e)
            {
                emitter.Load(EvaluateExpression(ref emitter, e.Left));
                emitter.Load(EvaluateExpression(ref emitter, e.Right));
                EmitOperator(ref emitter, e.Operator);
            }
            else if (expression is Token t)
                return ConstToLLVMConst(t);
            return new();
        }
        void RecognizeStatement(ref MugEmitter emitter, INode statement)
        {
            switch (statement)
            {
                case VariableStatement variable:
                    emitter.DeclareVariable(variable.Name, TypeToLLVMType(variable.Type));
                    emitter.Load(EvaluateExpression(ref emitter, variable.Body));
                    emitter.StoreVariable(variable.Name);
                    break;
                default:
                    break;
            }
        }
        void ProcessFunction(FunctionNode function)
        {
            var entry = InstallFunction(function.Name, function.Type, function.ParameterList);
            MugEmitter emitter = new MugEmitter();
            LLVM.PositionBuilderAtEnd(emitter.Builder, entry);
            foreach (var statement in function.Body.Statements)
                RecognizeStatement(ref emitter, statement);
        }
        void RecognizeMember(INode member)
        {
            switch (member)
            {
                case FunctionNode function:
                    ProcessFunction(function);
                    break;
                default:
                    break;
            }
        }
        public void Generate()
        {
            foreach (var member in Parser.Module.Members.Nodes)
                RecognizeMember(member);
        }
        public List<Token> GetTokenCollection() => Parser.GetTokenCollection();
        public List<Token> GetTokenCollection(out MugLexer lexer) => Parser.GetTokenCollection(out lexer);
        public NamespaceNode GetNodeCollection() => Parser.Parse();
        public NamespaceNode GetNodeCollection(out MugParser parser)
        {
            var nodes = Parser.Parse();
            parser = Parser;
            return nodes;
        }
    }
}
