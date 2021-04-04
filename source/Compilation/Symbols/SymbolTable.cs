using LLVMSharp;
using Mug.Models.Generator;
using Mug.Models.Parser.NodeKinds.Statements;
using Mug.MugValueSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Compilation.Symbols
{
    public interface IFunctionID
    {

    }

    public class FunctionPrototypeIdentifier : IFunctionID
    {
        public FunctionNode Prototype { get; set; }

        public FunctionPrototypeIdentifier(FunctionNode prototype)
        {
            Prototype = prototype;
        }
    }

    public class UndefinedFunctionID : IFunctionID
    {
        public MugValueType? BaseType { get; }
        public MugValueType[] GenericParameters { get; }
        public MugValueType[] Parameters { get; }

        public UndefinedFunctionID(
            MugValueType? baseType,
            MugValueType[] genericParameters,
            MugValueType[] parameters)
        {
            BaseType = baseType;
            GenericParameters = genericParameters;
            Parameters = parameters;
        }

        public override bool Equals(object obj)
        {
            if (obj is not UndefinedFunctionID id ||
                id.Parameters.Length != Parameters.Length ||
                id.GenericParameters.Length != GenericParameters.Length)
                return false;

            for (int i = 0; i < id.Parameters.Length; i++)
                if (!id.Parameters[i].Equals(Parameters[i]))
                    return false;

            for (int i = 0; i < id.GenericParameters.Length; i++)
                if (!id.GenericParameters[i].Equals(GenericParameters[i]))
                    return false;

            if (id.BaseType.HasValue != BaseType.HasValue)
                return false;

            return id.BaseType.HasValue ? id.BaseType.Value.Equals(BaseType.Value) : true;
        }

        public static UndefinedFunctionID EntryPoint()
        {
            return new UndefinedFunctionID(null, Array.Empty<MugValueType>(), Array.Empty<MugValueType>());
        }
    }

    public class FunctionIdentifier : UndefinedFunctionID
    {
        public MugValueType ReturnType { get; }
        public MugValue Value { get; }

        public FunctionIdentifier(
            MugValueType? baseType,
            MugValueType[] genericParameters,
            MugValueType[] parameters,
            MugValueType returnType,
            MugValue value) : base(baseType, genericParameters, parameters)
        {
            ReturnType = returnType;
            Value = value;
        }
    }

    public struct TypeIdentifier
    {
        public MugValueType[] GenericParameters { get; }
        public MugValue? Value { get; }

        public TypeIdentifier(MugValueType[] genericParameters, MugValue? value)
        {
            GenericParameters = genericParameters;
            Value = value;
        }

        public static TypeIdentifier CreatePrototype(MugValueType[] genericParameters)
        {
            return new TypeIdentifier(genericParameters, null);
        }

        public override bool Equals(object obj)
        {
            if (obj is not TypeIdentifier id ||
                id.GenericParameters.Length != GenericParameters.Length)
                return false;

            for (int i = 0; i < id.GenericParameters.Length; i++)
                if (!id.GenericParameters[i].Equals(GenericParameters[i]))
                    return false;

            return true;
        }
    }

    public class SymbolTable
    {
        private readonly IRGenerator _generator;

        public readonly Dictionary<string, List<IFunctionID>> Functions = new();
        public readonly Dictionary<string, List<TypeIdentifier>> Types = new();
        public readonly List<string> CompilerSymbols = new();

        public SymbolTable(IRGenerator generator)
        {
            _generator = generator;
        }

        public void DeclareFunctionSymbol(string name, IFunctionID identifier, Range position)
        {
            if (!Functions.TryAdd(name, new() { identifier }))
            {
                if (Functions[name].FindIndex(id => id.Equals(identifier)) != -1)
                {
                    _generator.Report(position, $"Function '{name}' already declared");
                    return;
                }

                Functions[name].Add(identifier);
            }
        }

        public void DeclareType(string name, TypeIdentifier identifier, Range position)
        {
            if (!Types.TryAdd(name, new() { identifier }))
            {
                if (Types[name].FindIndex(id => id.Equals(identifier)) != -1)
                {
                    _generator.Report(position, $"Type '{name}' already declared");
                    return;
                }

                Types[name].Add(identifier);
            }
        }

        public bool DeclareCompilerSymbol(string name, Range position)
        {
            if (CompilerSymbols.Contains(name))
            {
                _generator.Report(position, "Already declared compiler symbol");
                return false;
            }

            CompilerSymbols.Add(name);
            return true;
        }

        public FunctionPrototypeIdentifier GetEntryPoint(out int index)
        {
            if (!Functions.TryGetValue(IRGenerator.EntryPointName, out var overloads))
                CompilationErrors.Throw("No entry point declared");

            index = overloads.FindIndex(id =>
            {
                var prototype = (FunctionPrototypeIdentifier)id;
                var symbol = new UndefinedFunctionID(
                    prototype.Prototype.Base?.Type.ToMugValueType(_generator),
                    Array.Empty<MugValueType>(),
                    _generator.ParameterTypesToMugTypes(prototype.Prototype.ParameterList.Parameters));
                return symbol.Equals(UndefinedFunctionID.EntryPoint());
            });

            if (index == -1)
                CompilationErrors.Throw("Cannot find a good overload for entry point");

            return (FunctionPrototypeIdentifier)overloads[index];
        }

        public IFunctionID GetFunction(string name, IFunctionID identifier, out int index, Range position)
        {
            index = 0;

            if (!Functions.TryGetValue(name, out var overloads))
            {
                _generator.Report(position, $"Undeclared function '{name}'");
                return null;
            }

            index = overloads.FindIndex(id =>
            {
                if (id is FunctionPrototypeIdentifier prototype)
                    id = new UndefinedFunctionID(
                        prototype.Prototype.Base?.Type.ToMugValueType(_generator),
                        Array.Empty<MugValueType>(),
                        _generator.ParameterTypesToMugTypes(prototype.Prototype.ParameterList.Parameters));
                
                return id.Equals(identifier);
            });

            if (index == -1)
            {
                _generator.Report(position, $"Cannot find a good overload for function '{name}'");
                return null;
            }

            return overloads[index];
        }

        public void DefineFunctionSymbol(string name, int index, FunctionIdentifier definition)
        {
            Functions[name][index] = definition;
        }

        public TypeIdentifier? GetType(string name, TypeIdentifier identifier, Range position)
        {
            if (!Types.TryGetValue(name, out var overloads))
            {
                _generator.Report(position, $"Undeclared type '{name}'");
                return null;
            }

            var index = overloads.FindIndex(id => id.Equals(identifier));

            if (index == -1)
            {
                _generator.Report(position, $"Cannot find a good overload for type '{name}'");
                return null;
            }

            return overloads[index];
        }

        public bool CompilerSymbolIsDeclared(string name)
        {
            return CompilerSymbols.Contains(name);
        }

        public TypeIdentifier GetType(string enumname, Range position)
        {
            // tofix
            throw new();
        }
    }
}
