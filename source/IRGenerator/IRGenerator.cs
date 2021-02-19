using LLVMSharp;
using LLVMSharp.Interop;
using Mug.Compilation;
using Mug.Models.Generator.Emitter;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using Mug.Models.Parser.NodeKinds.Directives;
using Mug.Models.Parser.NodeKinds.Statements;
using Mug.TypeSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mug.Models.Generator
{
    public class IRGenerator
    {
        public readonly MugParser Parser;
        public LLVMModuleRef Module { get; set; }

        private Dictionary<string, LLVMValueRef> Symbols { get; set; } = new();

        private const string EntryPointName = "main";

        public string LocalPath
        {
            get
            {
                return Path.GetFullPath(Parser.Lexer.ModuleName);
            }
        }


        public IRGenerator(string moduleName, string source)
        {
            Parser = new(moduleName, source);

            Module = LLVMModuleRef.CreateWithName(moduleName);
        }

        public IRGenerator(MugParser parser)
        {
            Parser = parser;
            Module = LLVMModuleRef.CreateWithName(parser.Lexer.ModuleName);
        }

        public void Error(Range position, params string[] error)
        {
            Parser.Lexer.Throw(position, error);
        }

        public bool MatchStringType(LLVMTypeRef exprType)
        {
            return exprType == LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0);
        }

        /// <summary>
        /// the function launches an exception and returns a generic value,
        /// this function comes in statement switch in expressions
        /// </summary>
        public T NotSupportedType<T>(string type, Range position)
        {
            Error(position, "`", type, "` type is not supported yet");
            throw new Exception("unreachable");
        }

        public bool MatchSameIntType(LLVMTypeRef ft, LLVMTypeRef st)
        {
            return
                // make sure it is an int
                ft.Kind == LLVMTypeKind.LLVMIntegerTypeKind &&
                // check are the same type
                ft == st &&
                // allowed int
                (ft == LLVMTypeRef.Int32 ||
                ft == LLVMTypeRef.Int64 ||
                ft == LLVMTypeRef.Int8);
        }

        public bool MatchAnyTypeOfIntType(LLVMTypeRef type)
        {
            return
                type == LLVMTypeRef.Int1  ||
                type == LLVMTypeRef.Int8  ||
                type == LLVMTypeRef.Int16 ||
                type == LLVMTypeRef.Int32 ||
                type == LLVMTypeRef.Int64;
        }

        /// <summary>
        /// the function converts a Mugtype to the corresponding Llvmtyperef
        /// </summary>
        public LLVMTypeRef TypeToLLVMType(MugType type, Range position)
        {
            return type.Kind switch
            {
                TypeKind.Int32 => LLVMTypeRef.Int32,
                TypeKind.UInt8 => LLVMTypeRef.Int8,
                TypeKind.Int64 => LLVMTypeRef.Int64,
                TypeKind.Bool => LLVMTypeRef.Int1,
                TypeKind.Void => LLVMTypeRef.Void,
                TypeKind.Char => LLVMTypeRef.Int16,
                TypeKind.String => LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0),
                _ => NotSupportedType<LLVMTypeRef>(type.Kind.ToString(), position)
            };
        }

        public bool MatchCharType(LLVMTypeRef ft)
        {
            return ft == LLVMTypeRef.Int16;
        }

        public ulong StringCharToIntChar(string value)
        {
            return Convert.ToUInt64(Convert.ToChar(value));
        }

        /// <summary>
        /// the function tries to declare the symbol: if it has already been declared launches a compilation-error
        /// </summary>
        private void DeclareSymbol(string name, LLVMValueRef value, Range position)
        {
            if (!Symbols.TryAdd(name, value))
                Error(position, "`", name, "` member already declared");
        }

        /// <summary>
        /// calls the function <see cref="TypeToLLVMType(MugType, Range)"/> for each parameter in the past array
        /// </summary>
        private LLVMTypeRef[] ParameterTypesToLLVMTypes(ParameterNode[] parameterTypes)
        {
            var result = new LLVMTypeRef[parameterTypes.Length];
            for (int i = 0; i < parameterTypes.Length; i++)
                result[i] = TypeToLLVMType(parameterTypes[i].Type, parameterTypes[i].Position);
            return result;
        }

        /// <summary>
        /// declares the prototype symbol of a function, the function <see cref="DefineFunction(FunctionNode)"/> will take the declared symbol
        /// in this function and will convert the ast of the function node into the corresponding low-level code appending the result to the symbol
        /// body
        /// </summary>
        private void InstallFunction(string name, MugType type, Range position, ParameterListNode paramTypes)
        {
            var parameterTypes = ParameterTypesToLLVMTypes(paramTypes.Parameters);
            var ft = LLVMTypeRef.CreateFunction(
                    TypeToLLVMType(type, position),
                    parameterTypes
                );

            name = BuildFunctionName(name, parameterTypes);

            var f = Module.AddFunction(name, ft);

            DeclareSymbol(name, f, position);
        }

        private bool IsVoid(MugType type)
        {
            return type.Kind == TypeKind.Void;
        }

        /// <summary>
        /// converts a boolean value in string format to one in int format
        /// </summary>
        public ulong StringBoolToIntBool(string value)
        {
            // converts for first string "true" or "false" to a boolean value, then to a ulong, so 0 or 1
            return Convert.ToUInt64(Convert.ToBoolean(value));
        }

        /// <summary>
        /// the function checks that all the past types are the same, therefore compatible with each other
        /// </summary>
        public void ExpectSameTypes(LLVMTypeRef firstType, Range position, string error, params LLVMTypeRef[] types)
        {
            for (int i = 0; i < types.Length; i++)
                if (firstType != types[i])
                    Error(position, error);
        }

        public void ExpectBoolType(LLVMTypeRef type, Range position)
        {
            ExpectSameTypes(type, position, $"Expected `u1` type, got `{type}`", LLVMTypeRef.Int1);
        }

        public void ExpectIntType(LLVMTypeRef type, Range position)
        {
            ExpectSameTypes(type, position, $"Expected `i32`, `i64` type, got `{type}`",
                LLVMTypeRef.Int8,
                LLVMTypeRef.Int32,
                LLVMTypeRef.Int64);
        }

        /// <summary>
        /// same of <see cref="ExpectNonVoidType(LLVMTypeRef, Range)"/> but tests a <see cref="MugType"/> instead of
        /// a <see cref="LLVMTypeRef"/>
        /// </summary>
        public void ExpectNonVoidType(MugType type, Range position)
        {
            if (IsVoid(type))
                Error(position, "In the current context `void` is not allowed");
        }

        /// <summary>
        /// launches a compilation-error if the type that is passed is of void type:
        /// this function is used in all contexts where the type void is not allowed,
        /// for example in the declaration of variables
        /// </summary>
        public void ExpectNonVoidType(LLVMTypeRef type, Range position)
        {
            if (type.Kind == LLVMTypeKind.LLVMVoidTypeKind)
                Error(position, "Expected a non-void type");
        }

        /// <summary>
        /// the function verifies that a symbol is declared and a compilation-error if it is not
        /// </summary>
        public LLVMValueRef GetSymbol(string name, Range position)
        {
            if (!Symbols.TryGetValue(name, out var member))
                Error(position, "`", name, "` undeclared member");
            return member;
        }

        /// <summary>
        /// defines the body of a function by taking from the declared symbols its own previously defined symbol,
        /// to allow the call of a method declared under the caller.
        /// see the first part of the Generate function
        /// </summary>
        private void DefineFunction(FunctionNode function)
        {
            MugEmitter emitter = new MugEmitter(this);

            var llvmfunction = Symbols[function.Name];
            // basic block, won't be emitted any block because the name is empty
            var entry = llvmfunction.AppendBasicBlock("");

            emitter.Builder.PositionAtEnd(entry);

            var generator = new LocalGenerator(this, ref llvmfunction, ref function, ref emitter);
            generator.Generate();

            // implicit return with void functions
            if (function.Type.Kind == TypeKind.Void &&
                // if the type is void check if the last statement was ret, if it was not ret add one implicitly
                entry.Terminator.IsAReturnInst.Handle == IntPtr.Zero) // is null
                generator.AddImplicitRetVoid();
        }


        /// <summary>
        /// check if an id is equal to the id of the entry point and if the parameters are 0,
        /// to allow overload of the main function
        /// </summary>
        public bool IsEntryPoint(string name, int paramsLen)
        {
            return
                name == EntryPointName && paramsLen == 0;
        }


        /// <summary>
        /// create a string representing the name of a function that includes its id and the list of parameters, separated by ', ', in brackets
        /// </summary>
        private string BuildFunctionName(string name, LLVMTypeRef[] types)
        {
            return
                IsEntryPoint(name, types.Length) ? name : $"{name}({string.Join(", ", types)})";
        }

        private void ReadModule(string filename)
        {
            unsafe
            {
                LLVMOpaqueMemoryBuffer* memoryBuffer;
                LLVMOpaqueModule* module;
                sbyte* message;

                using (var marshalledFilename = new MarshaledString(filename))
                    if (LLVM.CreateMemoryBufferWithContentsOfFile(marshalledFilename, &memoryBuffer, &message) != 0)
                        CompilationErrors.Throw("Unable to open file: `", filename, "`");
                
                if (LLVM.ParseBitcode(memoryBuffer, &module, &message) != 0)
                    CompilationErrors.Throw("Unable to parse file: `", filename, "`");

                if (LLVM.LinkModules2(Module, module) != 0)
                    CompilationErrors.Throw("Unable to link file: `", filename, "`, with the main module");
            }
        }

        private void DeclareFunction(FunctionNode function)
        {
            InstallFunction(function.Name, function.Type, function.Position, function.ParameterList);

            // allowing to call entrypoint
            if (function.Name == EntryPointName)
                DeclareSymbol(EntryPointName + "()", Symbols[EntryPointName], function.Position);
        }

        private void EmitFunction(FunctionNode function)
        {
            // change the name of the function in the corresponding with the types of parameters, to allow overload of the methods
            function.Name = BuildFunctionName(function.Name, ParameterTypesToLLVMTypes(function.ParameterList.Parameters));

            DefineFunction(function);
        }

        private void EmitFunctionPrototype(FunctionPrototypeNode prototype)
        {
            var parameters = ParameterTypesToLLVMTypes(prototype.ParameterList.Parameters);
            // search for the function
            LLVMValueRef function = Module.GetNamedFunction(prototype.Name);

            // if the function is not declared yet
            if (function.Handle == IntPtr.Zero)
                // declares it
                function = Module.AddFunction(prototype.Name,
                        LLVMTypeRef.CreateFunction(
                            TypeToLLVMType(prototype.Type, prototype.Position),
                            parameters));

            // adding a new symbol
            DeclareSymbol(
                BuildFunctionName(prototype.Name, parameters),
                function,
                prototype.Position);
        }

        private void EmitImport(ImportDirective import)
        {
            CompilationUnit unit = null;

            if (import.Mode == ImportMode.FromPackages) // dirof(mug.exe)/include/
                unit = new CompilationUnit("include/" + ((Token)import.Member).Value + ".mug");
            else
            {
                var path = (Token)import.Member;
                var filekind = Path.GetExtension(path.Value);
                var fullpath = Path.GetFullPath(path.Value, Path.GetDirectoryName(LocalPath));

                if (filekind == ".bc") // llvm bitcode file
                {
                    ReadModule(fullpath);
                    return;
                }
                else if (filekind == ".mug") // dirof(file.mug)
                    unit = new CompilationUnit(fullpath);
                else
                    CompilationErrors.Throw("Unrecognized file kind: `", path.Value, "`");
            }

            // pass the current module to generate the llvm code together by the irgenerator
            unit.IRGenerator.Module = Module;
            // pass the current symbols to declare those in the module here
            unit.IRGenerator.Symbols = Symbols;
            unit.Generate();
        }

        /// <summary>
        /// recognize the type of the AST node and depending on the type call methods
        /// to convert it to the corresponding low-level code
        /// </summary>
        private void RecognizeMember(INode member, bool declareOnly)
        {
            switch (member)
            {
                case FunctionNode function:
                    if (declareOnly) // declares the prototype of the function
                        DeclareFunction(function);
                    else // defines the function body
                        EmitFunction(function);
                    break;
                case FunctionPrototypeNode prototype:
                    if (declareOnly)
                        EmitFunctionPrototype(prototype);
                    break;
                case ImportDirective import:
                    if (declareOnly)
                        EmitImport(import);
                    break;
                default:
                    Error(member.Position, "Declaration not supported yet");
                    break;
            }
        }

        /// <summary>
        /// declares the prototypes of all the global members, then defines them,
        /// to allow the use of a member declared under the member that uses it
        /// </summary>
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
