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

namespace Mug.Models.Generator
{
    public class IRGenerator
    {
        public readonly MugParser Parser;
        public readonly LLVMModuleRef Module;
        readonly Dictionary<string, LLVMValueRef> _symbols = new();
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
        void Error(Range position, params string[] error)
        {
            Parser.Lexer.Throw(position, error);
        }
        T NotSupportedType<T>(string type, Range position)
        {
            Error(position, "`", type, "` type is not supported yet");
            throw new Exception("unreachable");
        }
        LLVMTypeRef TypeToLLVMType(MugType type, Range position)
        {
            return type.Kind switch
            {
                TypeKind.Int32 => LLVMTypeRef.Int32Type(),
                TypeKind.Void => LLVMTypeRef.VoidType(),
                _ => NotSupportedType<LLVMTypeRef>(type.Kind.ToString(), position)
            };
        }
        LLVMTypeRef[] ParameterTypesToLLVMTypes(ParameterNode[] parameterTypes)
        {
            var result = new LLVMTypeRef[parameterTypes.Length];
            for (int i = 0; i < parameterTypes.Length; i++)
                result[i] = TypeToLLVMType(parameterTypes[i].Type, parameterTypes[i].Position);
            return result;
        }
        void DeclareSymbol(string name, LLVMValueRef value, Range position)
        {
            if (!_symbols.TryAdd(name, value))
                Error(position, "`", name, "` member already declared");
        }
        LLVMBasicBlockRef InstallFunction(string name, MugType type, Range position, ParameterListNode paramTypes)
        {
            var ft = LLVM.FunctionType(
                    TypeToLLVMType(type, position),
                    ParameterTypesToLLVMTypes(paramTypes.Parameters),
                    false
                );
            var f = LLVM.AddFunction(Module, name, ft);
            DeclareSymbol(name, f, position);
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
        bool IsVoid(MugType type)
        {
            return type.Kind == TypeKind.Void;
        }
        LLVMValueRef ConstToLLVMConst(Token constant, Range position)
        {
            return constant.Kind switch
            {
                TokenKind.ConstantDigit => LLVMTypeRef.ConstInt(LLVMTypeRef.Int32Type(), Convert.ToUInt64(constant.Value), MugEmitter._llvmfalse),
                _ => NotSupportedType<LLVMValueRef>(constant.Kind.ToString(), position)
            };
        }
        void EvaluateExpression(ref MugEmitter emitter, INode expression)
        {
            if (expression is ExpressionNode e)
            {
                EvaluateExpression(ref emitter, e.Left);
                EvaluateExpression(ref emitter, e.Right);
                EmitOperator(ref emitter, e.Operator);
            }
            else if (expression is Token t)
            {
                if (t.Kind == TokenKind.Identifier)
                    emitter.LoadFromMemory(t.Value);
                else
                    emitter.Load(ConstToLLVMConst(t, t.Position));
            }
        }
        MugType ExpectNonVoidType(MugType type, Range position)
        {
            if (IsVoid(type))
                Error(position, "In the current context `void` is not allowed");
            return type;
        }
        void RecognizeStatement(ref MugEmitter emitter, INode statement)
        {
            switch (statement)
            {
                case VariableStatement variable:
                    if (emitter.IsDeclared(variable.Name))
                        Error(variable.Position, "Already declared in the corrent context");
                    emitter.DeclareVariable(variable.Name, TypeToLLVMType(ExpectNonVoidType(variable.Type, variable.Position), variable.Position));
                    EvaluateExpression(ref emitter, variable.Body);
                    emitter.StoreVariable(variable.Name);
                    break;
                case ReturnStatement @return:
                    if (@return.Body is null)
                        emitter.RetVoid();
                    else
                    {
                        EvaluateExpression(ref emitter, @return.Body);
                        emitter.Ret();
                    }
                    break;
                default:
                    Error(statement.Position, "Statement not supported yet");
                    break;
            }
        }
        void AllocParameters(ref MugEmitter emitter, LLVMValueRef function, ParameterListNode parameters)
        {
            for (int i = 0; i < parameters.Parameters.Length; i++)
            {
                emitter.DeclareVariable(parameters.Parameters[i].Name, TypeToLLVMType(parameters.Parameters[i].Type, parameters.Parameters[i].Position));
                emitter.Load(LLVM.GetParam(function, (uint)i));
                emitter.StoreVariable(parameters.Parameters[i].Name);
            }
        }
        LLVMValueRef GetSymbol(string name, Range position)
        {
            if (!_symbols.TryGetValue(name, out var member))
                Error(position, "`", name, "` undeclared member");
            return member;
        }
        void ProcessFunction(FunctionNode function)
        {
            var entry = InstallFunction(function.Name, function.Type, function.Position, function.ParameterList);
            MugEmitter emitter = new MugEmitter();
            LLVM.PositionBuilderAtEnd(emitter.Builder, entry);
            AllocParameters(ref emitter, GetSymbol(function.Name, function.Position), function.ParameterList);
            foreach (var statement in function.Body.Statements)
                RecognizeStatement(ref emitter, statement);
            if (IsVoid(function.Type))
                emitter.RetVoid();
        }
        void RecognizeMember(INode member)
        {
            switch (member)
            {
                case FunctionNode function:
                    ProcessFunction(function);
                    break;
                default:
                    Error(member.Position, "Declaration not supported yet");
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
