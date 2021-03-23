using LLVMSharp.Interop;
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
using System.Resources;
using System.Text;

namespace Mug.Models.Generator
{
    public class IRGenerator
    {
        public LLVMModuleRef Module { get; set; }

        public readonly MugParser Parser;
        public readonly List<Symbol> Map = new();

        internal readonly List<string> IllegalTypes = new();
        internal List<(string, MugValueType)> _genericParameters = new();

        private readonly Dictionary<string, List<FunctionNode>> _genericFunctions = new();
        private readonly bool _isMainModule = false;

        private const string EntryPointName = "main";
        private const string AsOperatorOverloading = "as";

        public string LocalPath
        {
            get
            {
                return Path.GetFullPath(Parser.Lexer.ModuleName);
            }
        }

        public IRGenerator(string moduleName, string source, bool isMainModule)
        {
            Parser = new(moduleName, source);

            Module = LLVMModuleRef.CreateWithName(moduleName);

            _isMainModule = isMainModule;
        }

        public IRGenerator(MugParser parser, bool isMainModule)
        {
            Parser = parser;
            Module = LLVMModuleRef.CreateWithName(parser.Lexer.ModuleName);
            _isMainModule = isMainModule;
        }

        public void Error(Range position, params string[] error)
        {
            Parser.Lexer.Throw(position, error);
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

        public ulong StringCharToIntChar(string value)
        {
            return Convert.ToUInt64(Convert.ToChar(value));
        }

        internal bool IsDeclared(string name, out Symbol symbol)
        {
            for (int i = 0; i < Map.Count; i++)
                if (Map[i].Name == name)
                {
                    symbol = Map[i];
                    return true;
                }

            symbol = null;
            return false;
        }

        internal bool IsIllegalType(string name)
        {
            for (int i = 0; i < IllegalTypes.Count; i++)
                if (IllegalTypes[i] == name)
                    return true;

            return false;
        }

        /// <summary>
        /// the function tries to declare the symbol: if it has already been declared launches a compilation-error
        /// </summary>
        public void DeclareSymbol(string name, bool isdefined, object value, Range position, bool ispublic)
        {
            if (IsDeclared(name, out _))
                Error(position, "`", name.ToString(), "` member already declared");

            Map.Add(new Symbol(name, isdefined, value, position, ispublic));
        }

        public void DeclareSymbol(Symbol symbol)
        {
            if (IsDeclared(symbol.Name, out _))
                Error(symbol.Position, "`", symbol.Name.ToString(), "` member already declared");

            Map.Add(symbol);
        }

        /// <summary>
        /// calls the function <see cref="TypeToMugType(MugType, Range)"/> for each parameter in the past array
        /// </summary>
        internal MugValueType[] ParameterTypesToMugTypes(List<ParameterNode> parameterTypes)
        {
            var result = new MugValueType[parameterTypes.Count];
            for (int i = 0; i < parameterTypes.Count; i++)
                result[i] = parameterTypes[i].Type.ToMugValueType(this);

            return result;
        }

        /// <summary>
        /// calls the function <see cref="TypeToMugType(MugType, Range)"/> for each parameter in the past array
        /// </summary>
        internal MugValueType[] MugTypesToMugValueTypes(List<MugType> types)
        {
            var result = new MugValueType[types.Count];
            for (int i = 0; i < types.Count; i++)
                result[i] = types[i].ToMugValueType(this);

            return result;
        }

        internal bool IsGenericParameter(string name, out MugValueType genericParameter)
        {
            for (int i = 0; i < _genericParameters.Count; i++)
                if (_genericParameters[i].Item1 == name)
                {
                    genericParameter = _genericParameters[i].Item2;
                    return true;
                }

            genericParameter = new();
            return false;
        }

        internal static LLVMTypeRef[] MugTypesToLLVMTypes(MugValueType[] parameterTypes)
        {
            var result = new LLVMTypeRef[parameterTypes.Length];
            for (int i = 0; i < parameterTypes.Length; i++)
                result[i] = parameterTypes[i].LLVMType;

            return result;
        }

        private void DefineSymbol(string name, object value, Range position, bool ispublic)
        {
            if (IsDeclared(name, out var symbol))
            {
                symbol.IsDefined = true;
                symbol.Value = value;
            }
            else
                DeclareSymbol(name, true, value, position, ispublic);
        }

        private void PopIllegalType()
        {
            IllegalTypes.RemoveAt(IllegalTypes.Count - 1);
        }

        private void PushIllegalType(string illegalType)
        {
            IllegalTypes.Add(illegalType);
        }

        internal MugValue EvaluateStruct(string name, List<MugValueType> genericsInput, Range position)
        {
            var symbolname = $"{name}{(genericsInput.Count != 0 ? $"<{string.Join(", ", genericsInput)}>" : "")}";

            if (IsDeclared(symbolname, out var declared) && declared.IsDefined)
                return declared.GetValue<MugValue>();

            PushIllegalType(name);

            var symbol = GetSymbol(name, position);

            name = symbolname;

            symbol.IsDefined = true;

            if (symbol.Value is not TypeStatement type)
            {
                Error(position, "This member is not declared as type");
                throw new();
            }

            var structure = type;

            if (structure.Generics.Count != genericsInput.Count)
                Error(position, "Incorrect number of generic parameters");

            var oldGenericParameters = _genericParameters;

            _genericParameters = new();
            for (int i = 0; i < structure.Generics.Count; i++)
                GenericParametersAdd((structure.Generics[i].Value, genericsInput[i]), structure.Generics[i].Position);

            var fields = new string[structure.Body.Count];
            var structModel = new MugValueType[structure.Body.Count];
            var fieldPositions = new Range[structure.Body.Count];

            for (int i = 0; i < structure.Body.Count; i++)
            {
                var field = structure.Body[i];

                if (fields.Contains(field.Name))
                    Error(field.Position, "Already declared field");

                fields[i] = field.Name;
                structModel[i] = field.Type.ToMugValueType(this);
                fieldPositions[i] = field.Position;
            }

            var structuretype = MugValueType.Struct(structure.Name, structModel, fields, fieldPositions);
            var structsymbol = MugValue.Struct(Module.AddGlobal(structuretype.LLVMType, structure.Name), structuretype);

            DefineSymbol(name, structsymbol, position, true);
            PopIllegalType();

            _genericParameters = oldGenericParameters;

            return structsymbol;
        }

        private void GenericParametersAdd((string, MugValueType) genericParameter, Range position)
        {
            if (_genericParameters.FindIndex(elem => elem.Item1 == genericParameter.Item1) != -1)
                Error(position, "Already declared generic parameter");

            _genericParameters.Add(genericParameter);
        }

        public bool IsCompilerSymbolDeclared(string symbol)
        {
            return IsDeclared($"@symbol: {symbol.ToLower()}", out _);
        }

        public void DeclareCompilerSymbol(string symbol, bool hasGoodPosition = false, Range position = new())
        {
            symbol = symbol.ToLower();

            if (IsCompilerSymbolDeclared(symbol))
            {
                var error = $"Compiler symbol `{symbol}` is already declared";

                if (!hasGoodPosition)
                    CompilationErrors.Throw(error);

                Error(position, error);
            }

            DeclareSymbol(new Symbol($"@symbol: {symbol}", false));
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
        internal void ExpectNonVoidType(LLVMTypeRef type, Range position)
        {
            if (type == LLVMTypeRef.Void)
                Error(position, "Expected a non-void type");
        }

        /// <summary>
        /// the function verifies that a symbol is declared and a compilation-error if it is not
        /// </summary>
        internal Symbol GetSymbol(string name, Range position)
        {
            if (!IsDeclared(name, out var symbol))
                Error(position, "`", name, "` undeclared member");

            return symbol;
        }

        /*private List<MugValue> GetFunctions(string name)
        {
            
        }*/

        /// <summary>
        /// the function returns a string representing the function id and the array of
        /// parameter types in parentheses separated by ', ',
        /// to allow overload of functions
        /// </summary>
        private string BuildName(string name, MugValueType? basetype, MugValueType[] genericsInput, MugValueType[] parameters, bool isAsOperaor)
        {
            if (isAsOperaor || IsEntryPoint(name, parameters.Length))
                return name;
            else
                return $"{(basetype.HasValue && genericsInput.Length == 0 ? basetype.Value.ToString() : "")}{name}{(genericsInput.Length > 0 ? $"<{string.Join(", ", genericsInput)}>" : "")}({string.Join(", ", parameters)})";
        }

        private FunctionNode GetGenericFunctionSymbol(string name, MugValueType? basetype, MugValueType[] parameters, MugValueType[] genericsInput, Range position)
        {
            if (!_genericFunctions.TryGetValue(name, out var overloads))
            {
                Error(position, "Undeclared generic function");
                throw new();
            }

            for (int i = 0; i < overloads.Count; i++)
            {
                var types = new MugValueType[overloads[i].ParameterList.Length];

                var oldGenericParamters = _genericParameters;
                _genericParameters = new();

                for (int j = 0; j < overloads[i].Generics.Count; j++)
                    GenericParametersAdd((overloads[i].Generics[j].Value, genericsInput[j]), overloads[i].Generics[j].Position);

                var overloadBasetype = overloads[i].Base?.Type.ToMugValueType(this);

                for (int j = 0; j < overloads[i].ParameterList.Length; j++)
                    types[j] = overloads[i].ParameterList.Parameters[j].Type.ToMugValueType(this);

                _genericParameters = oldGenericParamters;

                if (parameters.Length != types.Length)
                    continue;
                
                int h = 0;

                for (; h < types.Length; h++)
                {
                    if (!parameters[h].Equals(types[h]))
                        goto end;
                }

                if (basetype.HasValue)
                    if (!basetype.Value.Equals(overloadBasetype.Value))
                        goto end;

                return overloads[i];
            end:;
            }

            Error(position, "Undeclared generic function");
            throw new();
        }

        internal MugValue EvaluateFunction(string name, MugValueType? basetype, MugValueType[] parameters, MugValueType[] genericsInput, Range position, bool isAsOperaor = false)
        {
            Symbol symbol;

            if (genericsInput.Length > 0)
            {
                var genericFunctionName = BuildName(name, null, genericsInput, parameters, isAsOperaor);
                if (IsDeclared(genericFunctionName, out var genericFunction))
                    return genericFunction.GetValue<MugValue>();

                var genericDeclaredFunction = GetGenericFunctionSymbol($"{name}{(genericsInput.Length > 0 ? $"<{new string('.', genericsInput.Length)}>" : "")}", basetype, parameters, genericsInput, position);

                symbol = new Symbol(name, false, genericDeclaredFunction, position, genericDeclaredFunction.Modifier == TokenKind.KeyPub);

                // symbol = GetSymbol($"{name}{(genericsInput.Length > 0 ? $"<{new string('.', genericsInput.Length)}>" : "")}({string.Join(", ", parameters)})", position);
                name = genericFunctionName;
            }
            else
                symbol = GetSymbol(name = BuildName(name, basetype, genericsInput, parameters, isAsOperaor), position);

            if (symbol.IsDefined)
                return symbol.GetValue<MugValue>();

            return EvaluateFunction(name, symbol.GetValue<FunctionNode>(), genericsInput, symbol.IsPublic, position);
        }

        /// <summary>
        /// defines the body of a function by taking from the declared symbols its own previously defined symbol,
        /// to allow the call of a method declared under the caller.
        /// see the first part of the Generate function
        /// </summary>
        private MugValue EvaluateFunction(string name, FunctionNode function, MugValueType[] genericsInput, bool ispublic, Range position)
        {
            if (function.Generics.Count != genericsInput.Length)
                Error(position, "Incorrect number of generic parameters");

            var oldGenericParameters = _genericParameters;

            _genericParameters = new();
            for (int i = 0; i < function.Generics.Count; i++)
                GenericParametersAdd((function.Generics[i].Value, genericsInput[i]), function.Generics[i].Position);

            var baseoffset = Convert.ToInt32(function.Base.HasValue);

            var paramTypes = new MugValueType[function.ParameterList.Length + baseoffset];
            var retType = function.Type.ToMugValueType(this);
            var types = ParameterTypesToMugTypes(function.ParameterList.Parameters);

            if (function.Base.HasValue)
                paramTypes[0] = function.Base.Value.Type.ToMugValueType(this);

            for (int i = 0; i < types.Length; i++)
                paramTypes[i + baseoffset] = types[i];

            var llvmfunction = Module.AddFunction(name, LLVMTypeRef.CreateFunction(
                retType.LLVMType,
                MugTypesToLLVMTypes(paramTypes)));

            var func = MugValue.From(llvmfunction, MugValueType.Function(paramTypes, retType));

            DefineSymbol(name, func, function.Position, ispublic);

            // basic block, won't be emitted any block because the name is empty
            var entry = llvmfunction.AppendBasicBlock("");

            var emitter = new MugEmitter(this, entry, false);

            emitter.Builder.PositionAtEnd(entry);

            var generator = new LocalGenerator(this, ref llvmfunction, ref function, ref emitter);
            generator.Generate();

            // if the type is void check if the last statement was ret, if it was not ret add one implicitly
            if (llvmfunction.LastBasicBlock.Terminator.IsAReturnInst.Handle == IntPtr.Zero &&
                function.Type.Kind == TypeKind.Void)
                generator.AddImplicitRetVoid();

            _genericParameters = oldGenericParameters;

            return func;
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
        private string BuildFunctionName(ParameterNode? basetype, string name, MugValueType[] types, MugValueType returntype)
        {
            if (IsEntryPoint(name, types.Length))
            {
                if (basetype.HasValue)
                    Error(basetype.Value.Position, "Cannot define a base parameter for the entrypoint");

                return name;
            }
            else if (IsAsOperatorOverloading(name))
                return $"as({string.Join(", ", types)}): {returntype}";
            else
                return $"{(basetype.HasValue ? basetype.Value.Type.ToMugValueType(this) + "." : "")}{name}({string.Join(", ", types)})";
        }

        /// <summary>
        /// the same of <see cref="BuildFunctionName(ParameterNode?, string, MugValueType[], MugValueType)"/>
        /// </summary>
        private string BuildFunctionTmpSymbol(ParameterNode? basetype, string name/*, int genericParamsCount*/, List<ParameterNode> parameters, MugType returntype)
        {
            var types = new MugType[parameters.Count];

            for (int i = 0; i < parameters.Count; i++)
                types[i] = parameters[i].Type;

            if (IsEntryPoint(name, parameters.Count))
            {
                if (basetype.HasValue)
                    Error(basetype.Value.Position, "Cannot define a base parameter for the entrypoint");

                return name;
            }
            else if (IsAsOperatorOverloading(name))
                return $"{name}({string.Join<MugType>(", ", types)}): {returntype}";
            else
                return $"{(basetype.HasValue ? $"{basetype?.Type}." : "")}{name}{""/*(genericParamsCount > 0 ? $"<{new string('.', genericParamsCount)}>" : "")*/}({string.Join<MugType>(", ", types)})";
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

        private void EmitFunctionPrototype(FunctionPrototypeNode prototype)
        {
            if (prototype.Generics.Count > 0)
                Error(prototype.Position, "Function prototypes cannot have generic parameters");

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

            var type = prototype.Type.ToMugValueType(this);
            
            // if the function is not declared yet
            if (function.Handle == IntPtr.Zero)
                // declares it
                function = Module.AddFunction(prototype.Pragmas.GetPragma("extern"),
                        LLVMTypeRef.CreateFunction(
                            type.LLVMType,
                            MugTypesToLLVMTypes(parameters)));

            // adding a new symbol
            DeclareSymbol(
                BuildFunctionName(null, prototype.Name, parameters, type),
                true,
                MugValue.From(function, MugValueType.Function(parameters, type)),
                prototype.Position, prototype.Modifier == TokenKind.KeyPub);
        }

        private void IncludeCHeader(string path)
        {
            var bc = Path.ChangeExtension(path, "bc");

            // compiling c code to llvm bit code
            CompilationUnit.CallClang($"-emit-llvm -c {path} -o {bc}", 3);

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
            return IsDeclared($"@import: {path}", out _);
        }

        private void EmitIncludeGuard(string path)
        {
            // pragma once
            var symbol = $"@import: {path}";
            if (IsDeclared(symbol, out _))
                return;

            Map.Add(new Symbol(symbol, true));
        }

        private void MergeSymbols(ref CompilationUnit unit)
        {
            for (int i = 0; i < unit.IRGenerator.Map.Count; i++)
            {
                var symbol = unit.IRGenerator.Map[i];

                if (symbol.IsPublic)
                {
                    symbol.IsPublic = false;
                    DeclareSymbol(symbol);
                }
            }

            for (int i = 0; i < unit.IRGenerator._genericFunctions.Count; i++)
            {
                var symbol = unit.IRGenerator._genericFunctions.Values.ElementAt(i);

                for (int j = 0; j < symbol.Count; j++)
                {
                    if (symbol[j].Modifier == TokenKind.KeyPub)
                    {
                        symbol[j].Modifier = TokenKind.KeyPriv;
                        DeclareGenericFunctionSymbol(symbol[j]);
                    }
                }
            }
        }

        private void EmitImport(ImportDirective import)
        {
            if (import.Member is not Token)
                Error(import.Position, "Unsupported import member");

            CompilationUnit unit = null;

            if (import.Mode == ImportMode.FromPackages) // dirof(mug.exe)/include/
            {
                // compilerpath\include\package.mug
                var path = AppDomain.CurrentDomain.BaseDirectory + "include\\" + ((Token)import.Member).Value + ".mug";

                if (AlreadyIncluded(path))
                    return;

                EmitIncludeGuard(path);

                unit = new CompilationUnit(path, false, false);

                if (unit.FailedOpeningPath)
                    Error(import.Member.Position, "Unable to find package");
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
                {
                    unit = new CompilationUnit(fullpath, false, false);

                    if (unit.FailedOpeningPath)
                        Error(import.Member.Position, "Unable to open source file");
                }
                else if (filekind == ".c") // c code
                {
                    IncludeCHeader(fullpath);
                    return;
                }
                else
                    Error((import.Member.Position.Start.Value+path.Value.Length-filekind.Length+2)..(import.Member.Position.End.Value-1), "Unrecognized file kind");
            }

            // pass the current module to generate the llvm code together by the irgenerator
            unit.IRGenerator.Module = Module;
            unit.Generate();
            MergeSymbols(ref unit);
        }

        private TokenKind GetValueTokenKindFromType(MugValueTypeKind kind, Range position)
        {
            return kind switch
            {
                MugValueTypeKind.Void => Error<TokenKind>(position, "Enum base type must be a non-void type"),
                MugValueTypeKind.String => TokenKind.ConstantString,
                MugValueTypeKind.Int8 or MugValueTypeKind.Int32 or MugValueTypeKind.Int64 => TokenKind.ConstantDigit,
                MugValueTypeKind.Bool => TokenKind.ConstantBoolean,
                MugValueTypeKind.Char => TokenKind.ConstantChar,
            };
        }

        private void CheckCorrectEnum(ref EnumStatement enumstatement,  MugValueType basetype)
        {
            var expectedValue = GetValueTokenKindFromType(basetype.TypeKind, enumstatement.Position);
            var members = new List<string>();

            for (int i = 0; i < enumstatement.Body.Count; i++)
            {
                var member = enumstatement.Body[i];

                if (member.Value.Kind != expectedValue)
                    Error(member.Position, "Expected type `", basetype.ToString(), "`");

                if (members.Contains(member.Name))
                    Error(member.Position, "Member already declared");

                members.Add(member.Name);
            }
        }

        private void EmitEnum(EnumStatement enumstatement)
        {
            var basetype = enumstatement.BaseType.ToMugValueType(this);

            CheckCorrectEnum(ref enumstatement, basetype);
            
            var type = MugValueType.Enum(basetype, enumstatement);

            DeclareSymbol(
                enumstatement.Name,
                true,
                MugValue.Enum(type),
                enumstatement.Position,
                enumstatement.Modifier == TokenKind.KeyPub);
        }

        private void MergeTree(NodeBuilder body)
        {
            foreach (var member in body.Nodes)
                RecognizeMember(member);
        }

        internal bool EvaluateCompTimeExprAndGetResult(CompTimeExpression comptimeExpr)
        {
            bool result = true;
            var lastOP = new Token(TokenKind.Bad, null, new());

            foreach (var token in comptimeExpr.Expression)
            {
                if (token.Kind != TokenKind.Identifier)
                    lastOP = token;
                else
                {
                    var symbolResult = IsCompilerSymbolDeclared(token.Value);

                    if (lastOP.Kind == TokenKind.BooleanOR)
                        result |= symbolResult;
                    else
                        result &= symbolResult;
                }
            }

            return result;
        }

        private void EmitCompTimeWhen(CompTimeWhenStatement when)
        {
            if (EvaluateCompTimeExprAndGetResult(when.Expression))
                MergeTree((NodeBuilder)when.Body);
        }

        private void GenerateEntryPoint()
        {
            if (!IsDeclared(EntryPointName, out var entrypoint))
                CompilationErrors.Throw("No entry point declared");

            EvaluateFunction(EntryPointName, entrypoint.GetValue<FunctionNode>(), Array.Empty<MugValueType>(), true, new());
        }

        private void DeclareGenericFunctionSymbol(FunctionNode function)
        {
            var symbol = $"{new string('.', Convert.ToInt32(function.Base.HasValue))}{function.Name}<{new string('.', function.Generics.Count)}>";

            _genericFunctions.TryAdd(symbol, new());

            var types = new MugType[function.ParameterList.Length];

            for (int i = 0; i < function.ParameterList.Length; i++)
                types[i] = function.ParameterList.Parameters[i].Type;

            var f = _genericFunctions[symbol];

            for (int i = 0; i < f.Count; i++)
            {
                if (f[i].Name == function.Name && f[i].ParameterList.Length == function.ParameterList.Length)
                {
                    var ftypes = new MugType[f[i].ParameterList.Length];

                    for (int j = 0; j < f[i].ParameterList.Length; j++)
                        ftypes[j] = f[i].ParameterList.Parameters[j].Type;

                    for (int j = 0; j < types.Length; j++)
                        if (!types[j].Equals(ftypes[j]))
                            goto end;

                    Error(function.Position, "Function overload already declared");
                end:;
                }
            }

            f.Add(function);
        }

        /// <summary>
        /// recognize the type of the AST node and depending on the type call methods
        /// to convert it to the corresponding low-level code
        /// </summary>
        private void RecognizeMember(INode member)
        {
            switch (member)
            {
                case FunctionNode function:
                    if (function.Generics.Count > 0)
                        DeclareGenericFunctionSymbol(function);
                    else
                        DeclareSymbol(BuildFunctionTmpSymbol(function.Base, function.Name/*, function.Generics.Count*/, function.ParameterList.Parameters, function.Type), false, function, function.Position, function.Modifier == TokenKind.KeyPub);
                    break;
                case FunctionPrototypeNode prototype:
                    EmitFunctionPrototype(prototype);
                    break;
                case TypeStatement structure:
                    DeclareSymbol(structure.Name, false, structure, structure.Position, structure.Modifier == TokenKind.KeyPub);
                    break;
                case EnumStatement enumstatement:
                    EmitEnum(enumstatement);
                    break;
                case ImportDirective import:
                    EmitImport(import);
                    break;
                case CompTimeWhenStatement comptimewhen:
                    EmitCompTimeWhen(comptimewhen);
                    break;
                case DeclareDirective declare:
                    DeclareCompilerSymbol(declare.Symbol.Value, true, declare.Position);
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
                RecognizeMember(member);

            if (_isMainModule)
                GenerateEntryPoint();
        }
    }
}
