﻿using LLVMSharp.Interop;
using Mug.Compilation;
using Mug.Models.Generator.Emitter;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using Mug.Models.Parser.NodeKinds.Directives;
using Mug.Models.Parser.NodeKinds.Statements;
using Mug.MugValueSystem;
using Mug.TypeSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mug.Models.Generator
{
    public class IRGenerator
    {
        public readonly MugParser Parser;
        public LLVMModuleRef Module { get; set; }

        private string _lastStructName = "";
        private readonly List<string> _declaredStructs = new();

        private Dictionary<string, MugValue> Symbols { get; set; } = new();

        private const string EntryPointName = "main";
        private const string AsOperatorOverloading = "as";

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

        internal MugValue RequireStandardSymbol(string symbol, string lib)
        {
            if (!Symbols.ContainsKey(symbol))
                ReadModule($"{lib}.bc");

            if (!Symbols.ContainsKey(symbol))
                CompilationErrors.Throw("Cannot load symbol ", symbol, " from lib ", lib);

            return Symbols[symbol];
        }

        /// <summary>
        /// the function launches an exception and returns a generic value,
        /// this function comes in statement switch in expressions
        /// </summary>
        internal T NotSupportedType<T>(string type, Range position)
        {
            return Error<T>(position, "`", type, "` type is not supported yet");
        }

        internal T Error<T>(Range position, params string[] error)
        {
            Error(position, error);
            throw new Exception("unreachable");
        }

        internal bool MatchSameIntType(MugValueType ft, MugValueType st)
        {
            return
                // check are the same type
                ft.Equals(st) &&
                // allowed int
                (ft.TypeKind == MugValueTypeKind.Int32 ||
                ft.TypeKind == MugValueTypeKind.Int64 ||
                ft.TypeKind == MugValueTypeKind.Int8);
        }

        public ulong StringCharToIntChar(string value)
        {
            return Convert.ToUInt64(Convert.ToChar(value));
        }

        /// <summary>
        /// the function tries to declare the symbol: if it has already been declared launches a compilation-error
        /// </summary>
        private void DeclareSymbol(string name, MugValue value, Range position)
        {
            if (!Symbols.TryAdd(name, value))
                Error(position, "`", name, "` member already declared");
        }

        /// <summary>
        /// calls the function <see cref="TypeToMugType(MugType, Range)"/> for each parameter in the past array
        /// </summary>
        internal MugValueType[] ParameterTypesToMugTypes(ParameterNode[] parameterTypes)
        {
            var result = new MugValueType[parameterTypes.Length];
            for (int i = 0; i < parameterTypes.Length; i++)
                result[i] = parameterTypes[i].Type.ToMugValueType(parameterTypes[i].Position, this);

            return result;
        }

        internal static LLVMTypeRef[] MugTypesToLLVMTypes(MugValueType[] parameterTypes)
        {
            var result = new LLVMTypeRef[parameterTypes.Length];
            for (int i = 0; i < parameterTypes.Length; i++)
                result[i] = parameterTypes[i].LLVMType;

            return result;
        }

        /// <summary>
        /// declares the prototype symbol of a function, the function <see cref="DefineFunction(FunctionNode)"/> will take the declared symbol
        /// in this function and will convert the ast of the function node into the corresponding low-level code appending the result to the symbol
        /// body
        /// </summary>
        private void InstallFunction(Pragmas pragmas, string name, MugType type, Range position, ParameterListNode paramTypes)
        {
            var parameterTypes = ParameterTypesToMugTypes(paramTypes.Parameters);

            var t = type.ToMugValueType(position, this);

            var ft = LLVMTypeRef.CreateFunction(
                    t.LLVMType,
                    MugTypesToLLVMTypes(parameterTypes));

            pragmas.SetName(name = BuildFunctionName(name, parameterTypes, t));

            var f = Module.AddFunction(pragmas.GetPragma("export"), ft);

            DeclareSymbol(name, MugValue.From(f, t), position);
        }

        /// <summary>
        /// converts a boolean value in string format to one in int format
        /// </summary>
        public ulong StringBoolToIntBool(string value)
        {
            // converts for first string "true" or "false" to a boolean value, then to a ulong, so 0 or 1
            return Convert.ToUInt64(Convert.ToBoolean(value));
        }

        internal LLVMValueRef[] MugValuesToLLVMValues(MugValue[] values)
        {
            var result = new LLVMValueRef[values.Length];
            for (int i = 0; i < values.Length; i++)
                result[i] = values[i].LLVMValue;

            return result;
        }

        /// <summary>
        /// the function checks that all the past types are the same, therefore compatible with each other
        /// </summary>
        internal void ExpectSameTypes(MugValueType firstType, Range position, string error, params MugValueType[] types)
        {
            for (int i = 0; i < types.Length; i++)
                if (!firstType.Equals(types[i]))
                    Error(position, error);
        }

        internal void ExpectBoolType(MugValueType type, Range position)
        {
            ExpectSameTypes(type, position, $"Expected `u1` type, got `{type}`", MugValueType.Bool);
        }

        internal void ExpectIntType(MugValueType type, Range position)
        {
            if (!type.MatchIntType())
                Error(position, $"Expected `u8`, `i32`, `i64` type, got `{type}`");
        }

        /// <summary>
        /// same of <see cref="ExpectNonVoidType(LLVMTypeRef, Range)"/> but tests a <see cref="MugType"/> instead of
        /// a <see cref="LLVMTypeRef"/>
        /// </summary>
        public void ExpectNonVoidType(MugType type, Range position)
        {
            if (type.Kind == TypeKind.Void)
                Error(position, "In the current context `void` is not allowed");
        }

        /// <summary>
        /// launches a compilation-error if the type that is passed is of void type:
        /// this function is used in all contexts where the type void is not allowed,
        /// for example in the declaration of variables
        /// </summary>
        public void ExpectNonVoidType(LLVMTypeRef type, Range position)
        {
            if (type == LLVMTypeRef.Void)
                Error(position, "Expected a non-void type");
        }

        /// <summary>
        /// the function verifies that a symbol is declared and a compilation-error if it is not
        /// </summary>
        internal MugValue GetSymbol(string name, Range position)
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

            var llvmfunction = Symbols[function.Name].LLVMValue;
            // basic block, won't be emitted any block because the name is empty
            var entry = llvmfunction.AppendBasicBlock("");

            MugEmitter emitter = new MugEmitter(this, entry, false);

            emitter.Builder.PositionAtEnd(entry);

            var generator = new LocalGenerator(this, ref llvmfunction, ref function, ref emitter);
            generator.Generate();

            // if the type is void check if the last statement was ret, if it was not ret add one implicitly
            if (llvmfunction.LastBasicBlock.Terminator.IsAReturnInst.Handle == IntPtr.Zero &&
                function.Type.Kind == TypeKind.Void)
                generator.AddImplicitRetVoid();
        }

        /// <summary>
        /// check if an id is equal to the id of the entry point and if the parameters are 0,
        /// to allow overload of the main function
        /// </summary>
        internal bool IsEntryPoint(string name, int paramsLen)
        {
            return
                name == EntryPointName && paramsLen == 0;
        }

        internal bool IsAsOperatorOverloading(string name)
        {
            return name == AsOperatorOverloading;
        }

        /// <summary>
        /// create a string representing the name of a function that includes its id and the list of parameters, separated by ', ', in brackets
        /// </summary>
        private string BuildFunctionName(string name, MugValueType[] types, MugValueType returntype)
        {
            if (IsEntryPoint(name, types.Length))
                return name;
            else if (IsAsOperatorOverloading(name))
                return $"as({string.Join(", ", types)}): {returntype}";
            else
                return $"{name}({string.Join(", ", types)})";
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
            InstallFunction(function.Pragmas, function.Name, function.Type, function.Position, function.ParameterList);

            // allowing to call entrypoint
            if (function.Name == EntryPointName)
                DeclareSymbol(EntryPointName + "()", Symbols[EntryPointName], function.Position);
        }

        private void EmitFunction(FunctionNode function)
        {
            // change the name of the function in the corresponding with the types of parameters, to allow overload of the methods
            function.Name = BuildFunctionName(
                function.Name,
                ParameterTypesToMugTypes(function.ParameterList.Parameters),
                function.Type.ToMugValueType(function.Position, this));

            DefineFunction(function);
        }

        private void EmitFunctionPrototype(FunctionPrototypeNode prototype)
        {
            var header = prototype.Pragmas.GetPragma("header");
            if (header != "")
            {
                var path = Path.GetFullPath(header, Path.GetDirectoryName(LocalPath));

                if (!AlreadyIncluded(path))
                {
                    EmitIncludeGuard(path);
                    IncludeCHeader(path);
                }
            }

            prototype.Pragmas.SetExtern(prototype.Name);

            var parameters = ParameterTypesToMugTypes(prototype.ParameterList.Parameters);
            // search for the function
            var function = Module.GetNamedFunction(prototype.Pragmas.GetPragma("extern"));

            var type = prototype.Type.ToMugValueType(prototype.Position, this);

            // if the function is not declared yet
            if (function.Handle == IntPtr.Zero)
                // declares it
                function = Module.AddFunction(prototype.Pragmas.GetPragma("extern"),
                        LLVMTypeRef.CreateFunction(
                            type.LLVMType,
                            MugTypesToLLVMTypes(parameters)));

            // adding a new symbol
            DeclareSymbol(
                BuildFunctionName(prototype.Name, parameters, type),
                MugValue.From(function, type),
                prototype.Position);
        }

        private void IncludeCHeader(string path)
        {
            // compiling c code to llvm bit code
            CompilationUnit.CallClang($"-emit-llvm -c {path}", 3);

            // targetting bitcode file
            path = Path.ChangeExtension(path, "bc");

            // loading bitcode file
            ReadModule(path);

            // delete bitcode file
            File.Delete(path);
        }

        private bool AlreadyIncluded(string path)
        {
            // backtick to prevent symbol declaration via backtick sequence identifier
            return Symbols.ContainsKey($"`@import: {path}");
        }

        private void EmitIncludeGuard(string path)
        {
            // pragma once
            var symbol = $"`@import: {path}";
            if (Symbols.ContainsKey(symbol))
                return;

            Symbols.Add(symbol, new());
        }

        private void EmitImport(ImportDirective import)
        {
            if (import.Member is not Token)
                Error(import.Position, "Unsupported import member");

            CompilationUnit unit = null;

            if (import.Mode == ImportMode.FromPackages) // dirof(mug.exe)/include/
            {
                var path = AppDomain.CurrentDomain.BaseDirectory + "include\\" + ((Token)import.Member).Value + ".mug";

                if (AlreadyIncluded(path))
                    return;

                EmitIncludeGuard(path);
                unit = new CompilationUnit(path);
            }
            else
            {
                var path = (Token)import.Member;
                var filekind = Path.GetExtension(path.Value);
                var fullpath = Path.GetFullPath(path.Value, Path.GetDirectoryName(LocalPath));

                if (AlreadyIncluded(fullpath))
                    return;

                EmitIncludeGuard(fullpath);

                if (filekind == ".bc") // llvm bitcode file
                {
                    ReadModule(fullpath);
                    return;
                }
                else if (filekind == ".mug") // dirof(file.mug)
                    unit = new CompilationUnit(fullpath);
                else if (filekind == ".c") // c code
                {
                    IncludeCHeader(fullpath);
                    return;
                }
                else
                    CompilationErrors.Throw("Unrecognized file kind: `", path.Value, "`");
            }

            // pass the current module to generate the llvm code together by the irgenerator
            unit.IRGenerator.Module = Module;
            // pass the current symbols to declare those in the module here
            unit.IRGenerator.Symbols = Symbols;
            unit.Generate();
        }

        private MugValueType SearchForStruct(string structName, Range position)
        {
            foreach (var member in Parser.Module.Members.Nodes)
                if (member is TypeStatement t && t.Name == structName)
                {
                    EmitStructure(t);
                    return Symbols[structName].Type;
                }

            Error(position, "Undeclared type `", structName, "`");
            throw new(); // unreachable
        }

        private void EmitStructure(TypeStatement structure)
        {
            if (Symbols.ContainsKey(structure.Name))
                return;

            var body = new MugValueType[structure.Body.Length];
            var fields = new List<string>();

            for (int i = 0; i < structure.Body.Length; i++)
            {
                var field = structure.Body[i];

                if (fields.Contains(field.Name))
                    Error(field.Position, "Redeclaration of previous field");

                fields.Add(field.Name);

                if (field.Type.ToString() == _declaredStructs.LastOrDefault() || field.Type.ToString() == structure.Name)
                    Error(field.Position, "Illegal recursion");

                if (!field.Type.TryToMugValueType(field.Position, this, out body[i]))
                    body[i] = SearchForStruct(field.Type.ToString(), field.Position);
            }

            var st = MugValueType.Struct(body, structure);

            var s = Module.AddGlobal(st.LLVMType, structure.Name);

            DeclareSymbol(structure.Name, MugValue.Struct(s, st), structure.Position);
        }

        /// <summary>
        /// recognize the type of the AST node and depending on the type call methods
        /// to convert it to the corresponding low-level code
        /// </summary>
        private void RecognizeMember(INode member, bool firstDeclaration, bool secondDeclaration)
        {
            switch (member)
            {
                case FunctionNode function:
                    if (secondDeclaration) // declares the prototype of the function
                        DeclareFunction(function);
                    else if (!firstDeclaration) // defines the function body
                        EmitFunction(function);
                    break;
                case FunctionPrototypeNode prototype:
                    if (secondDeclaration)
                        EmitFunctionPrototype(prototype);
                    break;
                case TypeStatement structure:
                    if (firstDeclaration)
                    {
                        if (_declaredStructs.Contains(structure.Name))
                            Error(structure.Position, "Type `", structure.Name, "` already declared");

                        _declaredStructs.Add(structure.Name);

                        EmitStructure(structure);
                    }
                    break;
                case ImportDirective import:
                    if (firstDeclaration)
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
                RecognizeMember(member, true, false);

            // prototypes' declaration
            foreach (var member in Parser.Module.Members.Nodes)
                RecognizeMember(member, false, true);

            // memebers' definition
            foreach (var member in Parser.Module.Members.Nodes)
                RecognizeMember(member, false, false);
        }
    }
}
