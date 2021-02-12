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
    public class IRGenerator
    {
        public readonly MugParser Parser;
        public readonly LLVMModuleRef Module;
        private readonly Dictionary<string, LLVMValueRef> _symbols = new();

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
        public void Error(Range position, params string[] error)
        {
            Parser.Lexer.Throw(position, error);
        }
        public T NotSupportedType<T>(string type, Range position)
        {
            Error(position, "`", type, "` type is not supported yet");
            throw new Exception("unreachable");
        }
        public LLVMTypeRef TypeToLLVMType(MugType type, Range position)
        {
            return type.Kind switch
            {
                TypeKind.Int32 => LLVMTypeRef.Int32Type(),
                TypeKind.Bool => LLVMTypeRef.Int1Type(),
                TypeKind.Void => LLVMTypeRef.VoidType(),
                _ => NotSupportedType<LLVMTypeRef>(type.Kind.ToString(), position)
            };
        }

        private void DeclareSymbol(string name, LLVMValueRef value, Range position)
        {
            if (!_symbols.TryAdd(name, value))
                Error(position, "`", name, "` member already declared");
        }

        private LLVMTypeRef[] ParameterTypesToLLVMTypes(ParameterNode[] parameterTypes)
        {
            var result = new LLVMTypeRef[parameterTypes.Length];
            for (int i = 0; i < parameterTypes.Length; i++)
                result[i] = TypeToLLVMType(parameterTypes[i].Type, parameterTypes[i].Position);
            return result;
        }

        private LLVMBasicBlockRef InstallFunction(string name, MugType type, Range position, ParameterListNode paramTypes)
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

        private bool IsVoid(MugType type)
        {
            return type.Kind == TypeKind.Void;
        }

        private ulong StringBoolToIntBool(string value)
        {
            return Convert.ToUInt64(Convert.ToBoolean(value));
        }
        public LLVMValueRef ConstToLLVMConst(Token constant, Range position)
        {
            return constant.Kind switch
            {
                TokenKind.ConstantDigit => LLVMTypeRef.ConstInt(LLVMTypeRef.Int32Type(), Convert.ToUInt64(constant.Value), MugEmitter.ConstLLVMFalse),
                TokenKind.ConstantBoolean => LLVMTypeRef.ConstInt(LLVMTypeRef.Int1Type(), StringBoolToIntBool(constant.Value), MugEmitter.ConstLLVMTrue),
                _ => NotSupportedType<LLVMValueRef>(constant.Kind.ToString(), position)
            };
        }

        public void ExpectSameTypes(LLVMTypeRef firstType, Range position, string error, params LLVMTypeRef[] types)
        {
            for (int i = 0; i < types.Length; i++)
                if (!Unsafe.Equals(firstType, types[i]))
                    Error(position, error);
        }

        public void ExpectBoolType(LLVMTypeRef type, Range position)
        {
            ExpectSameTypes(type, position, "Expected `Bool` type", LLVMTypeRef.Int1Type());
        }

        public void ExpectIntType(LLVMTypeRef type, Range position)
        {
            ExpectSameTypes(type, position, "Expected `Int8`, `Int32`, `Int64` type", LLVMTypeRef.Int32Type());
        }

        public void ExpectNonVoidType(MugType type, Range position)
        {
            if (IsVoid(type))
                Error(position, "In the current context `Void` is not allowed");
        }

        public void ExpectNonVoidType(LLVMTypeRef type, Range position)
        {
            if (type.TypeKind == LLVMTypeKind.LLVMVoidTypeKind)
                Error(position, "In the current context `Void` is not allowed");
        }

        private LLVMValueRef GetSymbol(string name, Range position)
        {
            if (!_symbols.TryGetValue(name, out var member))
                Error(position, "`", name, "` undeclared member");
            return member;
        }

        private void DefineFunction(FunctionNode function)
        {
            var entry = InstallFunction(function.Name, function.Type, function.Position, function.ParameterList);
            MugEmitter emitter = new MugEmitter(this);
            LLVM.PositionBuilderAtEnd(emitter.Builder, entry);

            var generator = new LocalGenerator(this, _symbols, ref function, ref emitter);
            generator.AllocParameters(GetSymbol(function.Name, function.Position), function.ParameterList);
            generator.Generate();
        }

        private void RecognizeMember(INode member)
        {
            switch (member)
            {
                case FunctionNode function:
                    DefineFunction(function);
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
    }
}
