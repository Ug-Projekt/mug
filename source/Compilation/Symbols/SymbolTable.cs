using LLVMSharp;
using Mug.Models.Generator;
using Mug.Models.Parser.NodeKinds.Statements;
using Mug.MugValueSystem;
using Mug.TypeSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Compilation.Symbols
{
    public struct FunctionSymbol
    {
        public MugValueType ReturnType { get; }
        public MugValue Value { get; }
        public MugValueType? BaseType { get; }
        public MugValueType[] GenericParameters { get; }
        public MugValueType[] Parameters { get; }

        public FunctionSymbol(
            MugValueType? baseType,
            MugValueType[] genericParameters,
            MugValueType[] parameters,
            MugValueType returntype,
            MugValue value)
        {
            BaseType = baseType;
            GenericParameters = genericParameters;
            Parameters = parameters;
            Value = value;
            ReturnType = returntype;
        }

        public override bool Equals(object obj)
        {
            if (obj is not FunctionSymbol id ||
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

            return !id.BaseType.HasValue || id.BaseType.Value.Equals(BaseType.Value);
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

        public readonly Dictionary<string, List<FunctionSymbol>> DefinedFunctions = new();
        public readonly List<FunctionNode> DeclaredFunctions = new();
        public readonly Dictionary<string, List<TypeIdentifier>> Types = new();
        public readonly List<string> CompilerSymbols = new();

        public SymbolTable(IRGenerator generator)
        {
            _generator = generator;
        }

        public void DeclareFunctionSymbol(string name, FunctionSymbol identifier, Range position)
        {
            if (!DefinedFunctions.TryAdd(name, new() { identifier }))
            {
                if (DefinedFunctions[name].FindIndex(id => id.Equals(identifier)) != -1)
                {
                    _generator.Report(position, $"Function '{name}' already declared");
                    return;
                }

                DefinedFunctions[name].Add(identifier);
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

        /*public FunctionNode GetEntryPoint()
        {
            var index = DeclaredFunctions.FindIndex(
                function =>
                function.Name == IRGenerator.EntryPointName &&
                function.ParameterList.Length == 0 &&
                function.Generics.Count == 0 &&
                function.Base == null);
            
            if (index == -1)
                CompilationErrors.Throw("No entry point declared");

            return DeclaredFunctions[index];
        }*/

        public void DefineFunctionSymbol(string name, int index, FunctionSymbol definition)
        {
            DefinedFunctions[name][index] = definition;
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
