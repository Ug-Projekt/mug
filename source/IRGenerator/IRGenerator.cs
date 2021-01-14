using Mug.Compilation;
using Mug.Models.Generator.Emitter;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using Mug.Models.Parser.NodeKinds.Statements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SymbolTable = System.Collections.Generic.Dictionary<string, object>;

namespace Mug.Models.Generator
{
    public class IRGenerator
    {
        public readonly MugParser Parser;
        public readonly MugEmitter Emitter;
        public readonly SymbolTable SymbolTable = new();
        public IRGenerator(string moduleName, string source)
        {
            Parser = new(moduleName, source);
            Parser.Parse();
            Emitter = new(moduleName);
        }
        public IRGenerator(MugParser parser)
        {
            Parser = parser;
            Emitter = new(parser.Lexer.ModuleName);
        }
        void GenerationError(INode node, params string[] error)
        {
            Parser.Throw(node, error);
        }
        void GenerationError(Token token, params string[] error)
        {
            Parser.Lexer.Throw(token, error);
        }
        LowCodeInstruction[] EvaluateExpression(ref SymbolTable symbolTable, INode expression)
        {
            List<LowCodeInstruction> code = new();
            if (expression is Token t)
            {
                if (t.Kind == TokenKind.Identifier)
                {
                    if (!symbolTable.TryGetValue(t.Value.ToString(), out object v))
                        GenerationError(t, "Undeclared variable in the current scope");
                    code.Add(new LowCodeInstruction() { Kind = LowCodeInstructionKind.load});
                }
            }
        }
        LowCodeInstruction[] GenerateFromFunction(FunctionNode function)
        {
            var symbolTable = new SymbolTable(SymbolTable);
            List<LowCodeInstruction> code = new();
            foreach (var statement in function.Body.Statements)
            {
                switch (statement)
                {
                    case VariableStatement v:
                        symbolTable.Add(v.Name, v);
                        var allocation = new LowCodeInstruction() { Kind = LowCodeInstructionKind.alloca, Label = "%"+v.Name };
                        allocation.AddArgument(new LowCodeInstructionArgument() { Value = "i32",  });
                        code.Add(allocation);
                        if (v.IsAssigned)
                            code.AddRange(EvaluateExpression(ref symbolTable, v.Body));
                        else
                        {
                            var defaultValue = new LowCodeInstruction() { Kind = LowCodeInstructionKind.store, new LowCodeInstructionArgument[] { new LowCodeInstructionArgument() { Type = "i32", Value = "0" } } };
                            code.Add();
                        }
                        break;
                    default:
                        break;
                }
            }
            return code.ToArray();
        }
        void DefineSymbol(string symbol, INode value)
        {
            SymbolTable.Add(symbol, value);
        }
        void RecognizeGlobalStatement(string namespaceName, INode statement)
        {
            switch (statement)
            {
                case FunctionNode function:
                    var name = namespaceName + '.' + function.Name;
                    Emitter.DefineFunction(name, SolveType(function.Type, SolveName(function.Type)), GenerateFromFunction(function));
                    break;
                default:
                    break;
            }
        }
        string SolveName(INode name)
        {
            return name switch
            {
                MemberNode m => SolveName(m.Base) + '.' + SolveName(m.Member),
                Token m => m.ToString(),
                _ => name.ToString(),
            };
        }
        string SolveType(INode errorNode, string type)
        {
            try
            {
                return type switch
                {
                    "i8" => "i8",
                    "i32" => "i32",
                    "i64" => "i64",
                    _ => ((TypeStatement)SymbolTable[type]).Name,
                };
            } catch (Exception)
            {
                GenerationError(errorNode, "Undeclared type");
                return "";
            }
        }
        void DefineNode(string nsName, INode value)
        {
            if (value is NamespaceNode ns)
                ProcessNamespace(nsName + '.', ns);
            else
            {
                nsName += '.';
                if (value is FunctionNode fn)
                    nsName += fn.Name;
                else if (value is VariableStatement v)
                    nsName += v.Name;
                if (!SymbolTable.TryAdd(nsName, value))
                {
                    var pos = ((INode)SymbolTable[nsName]).Position;
                    GenerationError(value, "Member already declared(", pos.Start.Value.ToString(), "..", pos.End.Value.ToString(), ")");
                }
            }
        }
        void ProcessNamespace(string nsName, NamespaceNode ns)
        {
            var memberName = nsName + SolveName(ns.Name);
            foreach (var node in ns.Members.Nodes)
                DefineNode(memberName, node);
            foreach (var node in ns.Members.Nodes)
                RecognizeGlobalStatement(memberName, node);
        }
        public string Generate()
        {
            ProcessNamespace("", Parser.Module);
            return Emitter.Build();
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
