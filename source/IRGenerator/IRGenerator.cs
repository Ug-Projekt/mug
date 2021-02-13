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
using System.Text;

namespace Mug.Models.Generator
{
    public class IRGenerator
    {
        public readonly MugParser Parser;
        public readonly LLVMModuleRef Module;
        private readonly Dictionary<string, LLVMValueRef> _symbols = new();

        private const string EntryPointName = "main";

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
                TypeKind.Char => LLVMTypeRef.Int8Type(),
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

        private void InstallFunction(string name, MugType type, Range position, ParameterListNode paramTypes)
        {
            var parameterTypes = ParameterTypesToLLVMTypes(paramTypes.Parameters);
            var ft = LLVM.FunctionType(
                    TypeToLLVMType(type, position),
                    parameterTypes,
                    false
                );

            name = BuildFunctionName(name, parameterTypes);

            var f = LLVM.AddFunction(Module, name, ft);

            DeclareSymbol(name, f, position);
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
                Error(position, "Expected a non-void type");
        }

        public LLVMValueRef GetSymbol(string name, Range position)
        {
            if (!_symbols.TryGetValue(name, out var member))
                Error(position, "`", name, "` undeclared member");
            return member;
        }

        private void DefineFunction(FunctionNode function)
        {
            MugEmitter emitter = new MugEmitter(this);

            var entry = LLVM.AppendBasicBlock(_symbols[function.Name], "");

            LLVM.PositionBuilderAtEnd(emitter.Builder, entry);

            var generator = new LocalGenerator(this, ref function, ref emitter);
            generator.AllocParameters(GetSymbol(function.Name, function.Position), function.ParameterList);
            generator.Generate();
        }

        private bool IsEntryPoint(string name, LLVMTypeRef[] types)
        {
            return
                name == EntryPointName && types.Length == 0;
        }

        private string BuildFunctionName(string name, LLVMTypeRef[] types)
        {
            return
                IsEntryPoint(name, types) ? name : $"{name}({string.Join(", ", types)})";
        }

        private void RecognizeMember(INode member, bool declareOnly)
        {
            switch (member)
            {
                case FunctionNode function:
                    if (declareOnly)
                        InstallFunction(function.Name, function.Type, function.Position, function.ParameterList);
                    else
                    {
                        function.Name = BuildFunctionName(function.Name, ParameterTypesToLLVMTypes(function.ParameterList.Parameters));
                        DefineFunction(function);
                    }
                    break;
                case FunctionPrototypeNode prototype:
                    DeclareSymbol(prototype.Name,
                        LLVM.AddFunction(Module, prototype.Name,
                            LLVMTypeRef.FunctionType(
                                TypeToLLVMType(prototype.Type, prototype.Position),
                                ParameterTypesToLLVMTypes(prototype.ParameterList.Parameters),
                                false)),
                        prototype.Position);
                    break;
                default:
                    Error(member.Position, "Declaration not supported yet");
                    break;
            }
        }

        public void Generate()
        {
            // prototypes' declaration
            foreach (var member in Parser.Module.Members.Nodes)
                RecognizeMember(member, true);

            // memebers' definition
            foreach (var member in Parser.Module.Members.Nodes)
                RecognizeMember(member, false);
        }
    }
}
