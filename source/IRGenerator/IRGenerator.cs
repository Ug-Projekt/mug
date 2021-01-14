using Mug.Compilation;
using Mug.Models.Evaluator;
using Mug.Models.Generator.Emitter;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using Mug.Models.Parser.NodeKinds.Statements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SymbolTable = System.Collections.Generic.Dictionary<string, Mug.Models.Parser.INode>;
using RedefinitionTable = System.Collections.Generic.Dictionary<string, string>;

namespace Mug.Models.Generator
{
    public class IRGenerator
    {
        public readonly MugParser Parser;
        public readonly MugEmitter Emitter = new();
        public readonly SymbolTable SymbolTable = new();
        public readonly RedefinitionTable RedefinitionTable = new();
        public IRGenerator(string moduleName, string source)
        {
            Parser = new(moduleName, source);
        }
        public IRGenerator(MugParser parser)
        {
            Parser = parser;
        }
        void GenerationError(INode node, params string[] error)
        {
            Parser.Throw(node, error);
        }
        void GenerationError(Token token, params string[] error)
        {
            Parser.Lexer.Throw(token, error);
        }
        string BuildFunctionName(string nsName, FunctionNode function)
        {
            var name = nsName + '.' + function.Name;
            foreach (var parameter in function.ParameterList.Parameters)
                nsName += '_'+SolveName(parameter.Type);
            return name;
        }
        string BuildFunctionParameters(ParameterListNode parameters)
        {
            var declaration = "";
            foreach (var parameter in parameters.Parameters)
                declaration += SolveType(parameter.Type, SolveName(parameter.Type))+" "+parameter.Name;
            return declaration;
        }
        string DefaultValue(string type)
        {
            return type switch
            {
                "int8" or "int" or "int64" or "unsigned int8" or "unsigned int" or "unsigned int64" => "0",
                "str" => "\"\"",
                _ => "0"
            };
        }
        string GenerateFromFunction(FunctionNode function)
        {
            var symbolTable = new SymbolTable(SymbolTable);
            LocalEmitter local = new();
            foreach (var statement in function.Body.Statements)
            {
                switch (statement)
                {
                    case VariableStatement v:
                        symbolTable.Add(v.Name, v);
                        var type = SolveType(v.Type, SolveName(v.Type));
                        if (v.IsAssigned)
                            local.EmitVarDefining(type, v.Name, new ExpressionEvaluator(ref symbolTable).EvaluateExpression(v.Body));
                        else
                        {
                            var body = DefaultValue(type);
                            if (body == "")
                                local.EmitVarDefiningWithoutBody(type, v.Name);
                            else
                                local.EmitVarDefining(type, v.Name, body);
                        }
                        break;
                    case ReturnStatement r:
                        local.EmitReturn(new ExpressionEvaluator(ref symbolTable).EvaluateExpression(r.Body));
                        break;
                    default:
                        break;
                }
            }
            return local.Build();
        }
        bool DefineSymbol(string symbol, INode value)
        {
            return SymbolTable.TryAdd(symbol, value);
        }
        void RecognizeGlobalStatement(string redefinition, INode statement)
        {
            switch (statement)
            {
                case FunctionNode function:
                    Emitter.DefineFunction(redefinition, SolveType(function.Type, SolveName(function.Type)), BuildFunctionParameters(function.ParameterList), GenerateFromFunction(function));
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
                    "i8" => "int8",
                    "i32" => "int",
                    "i64" => "int64",
                    "u8" => "unsigned int8",
                    "u32" => "unsigned int",
                    "u64" => "unsigned int64",
                    "str" => "String",
                    "unknown" => "Object",
                    "bit" => "unsigned int8",
                    "?" => "void",
                    _ => ((TypeStatement)SymbolTable[RedefinitionTable[type]]).Name,
                };
            } catch (Exception)
            {
                GenerationError(errorNode, "Undeclared type");
                return "";
            }
        }
        string GenerateName()
        {
            var name = "";
            var rand = new Random();
            char GenerateLCChar()
            {
                return (char)rand.Next(97, 122);
            }
            char GenerateUCChar()
            {
                return (char)rand.Next(65, 90);
            }
            char GenerateNumChar()
            {
                return (char)rand.Next(48, 57);
            }
            for (int i = 0; i < 17; i++)
            {
                var charKind = rand.Next(0, 5);
                if (charKind == 2)
                    name += GenerateUCChar();
                else if (charKind == 1 && i > 0)
                    name += GenerateNumChar();
                else
                    name += GenerateLCChar();
            }
            return name;
        }
        bool IsEntryPoint(string name)
        {
            return name == Parser.Lexer.ModuleName+".main";
        }
        bool TryDefine(string name, INode value)
        {
            var gen = "";
            if (IsEntryPoint(name))
                return DefineSymbol("main", value);
            do
                gen = GenerateName();
            while (!RedefinitionTable.TryAdd(name, gen));
            return DefineSymbol(gen, value);
        }
        void DefineNode(string nsName, INode value)
        {
            if (value is NamespaceNode ns)
                ProcessNamespace(ns, nsName + '.');
            else
            {
                if (value is FunctionNode fn)
                    nsName = BuildFunctionName(nsName, fn);

                if (!TryDefine(nsName, value))
                {
                    var pos = SymbolTable[nsName].Position;
                    GenerationError(value, "Member already declared(", pos.Start.Value.ToString(), "..", pos.End.Value.ToString(), ")");
                }
            }
        }
        void ProcessNamespace(NamespaceNode ns, string nsName = "")
        {
            foreach (var node in ns.Members.Nodes)
                DefineNode(nsName + SolveName(ns.Name), node);
        }
        void ProcessSymbols()
        {
            foreach (var node in SymbolTable)
                RecognizeGlobalStatement(node.Key, node.Value);
        }
        public string Generate()
        {
            ProcessNamespace(Parser.Module);
            ProcessSymbols();
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
