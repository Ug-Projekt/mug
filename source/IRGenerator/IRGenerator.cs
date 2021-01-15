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
        public String EntryPointGeneratedName { get; set; }
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
        public static string BuildFunctionName(string nsName, FunctionNode function)
        {
            var name = nsName + '.' + function.Name + '(';
            var par = function.ParameterList.Parameters;
            var len = par.Length;
            for (int i = 0; i < len; i++)
                name += SolveName(par[i].Type)+(i < len-1 ? "," : "");
            return name+')';
        }
        public static string BuildFunctionCallingName(CallStatement function, INode[] types)
        {
            var name = SolveName(function.Name) + '(';
            var par = function.Parameters.Nodes;
            var len = par.Length;
            for (int i = 0; i < len; i++)
                name += SolveName(types[i]) + (i < len - 1 ? "," : "");
            return name + ')';
        }
        string BuildFunctionParameters(ParameterListNode parameters)
        {
            var declaration = "";
            var len = parameters.Parameters.Length;
            for (int i = 0; i < len; i++)
                declaration += SolveType(parameters.Parameters[i].Type, SolveName(parameters.Parameters[i].Type)) + ' ' + parameters.Parameters[i].Name + (i < len - 1 ? ", " : "");
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
        string RemoveNamespace(string member, string nsName)
        {
            if (member.StartsWith(member))
                member.Remove(0, nsName.Length);
            return member;
        }
        void BuildFunctionCall(string nsName, SymbolTable symbolTable, CallStatement c, out string genName, out string[] paramBodies)
        {
            genName = "";
            paramBodies = null;
            if (c.Name is Token t && t.Kind == TokenKind.Identifier)
            {
                var _paramTypes = new List<INode>();
                var _paramBodies = new List<string>();
                if (c.Parameters is null)
                    c.Parameters = new();
                for (int i = 0; i < c.Parameters.Nodes.Length; i++)
                {
                    _paramBodies.Add(EvaluateExpression(nsName, c.Parameters.Nodes[i], out var expressionType, symbolTable));
                    _paramTypes.Add(expressionType);
                }
                var name = nsName + '.' + BuildFunctionCallingName(c, _paramTypes.ToArray());
                if (!RedefinitionTable.TryGetValue(name, out genName))
                    GenerationError(c, "Undeclared function");
                paramBodies = _paramBodies.ToArray();
            }
            else if (c.Name is MemberNode m)
            {
            }
        }
        string GenerateFromFunction(string nsName, FunctionNode function)
        {
            var symbolTable = new SymbolTable();
            LocalEmitter local = new();
            foreach (var par in function.ParameterList.Parameters)
                symbolTable.Add(par.Name, par);
            foreach (var statement in function.Body.Statements)
            {
                switch (statement)
                {
                    case VariableStatement v:
                        if (!symbolTable.TryAdd(v.Name, v))
                            GenerationError(v, "Variable already declared");
                        var type = SolveType(v.Type, SolveName(v.Type));
                        if (v.IsAssigned)
                            local.EmitVarDefining(type, v.Name, EvaluateExpression(nsName, v.Body, symbolTable));
                        else
                        {
                            var body = DefaultValue(type);
                            if (body == "")
                                local.EmitVarDefiningWithoutBody(type, v.Name);
                            else
                                local.EmitVarDefining(type, v.Name, body);
                        }
                        break;
                    case ConstantStatement cs:
                        if (!symbolTable.TryAdd(cs.Name, cs))
                            GenerationError(cs, "Variable already declared");
                        var cstype = SolveType(cs.Type, SolveName(cs.Type));
                        local.EmitConstDefining(cstype, cs.Name, EvaluateExpression(nsName, cs.Body, symbolTable));
                        break;
                    case AssignmentStatement a:
                        if (!symbolTable.ContainsKey(SolveName(a.Name)))
                        {
                            if (!RedefinitionTable.ContainsKey(SolveName(a.Name)))
                                GenerationError(a, "Undefined variable");
                        }
                        else if (symbolTable[SolveName(a.Name)] is ConstantStatement)
                            GenerationError(a, "Cannot perform assign operation with constants");
                        local.EmitAssignment(SolveName(a.Name), EvaluateExpression(nsName, a.Body, symbolTable));
                        break;
                    case CallStatement c:
                        BuildFunctionCall(nsName, symbolTable, c, out var name, out var paramBodies);
                        local.EmitCall(name, string.Join(", ", paramBodies));
                        break;
                    case ReturnStatement r:
                        local.EmitReturn(EvaluateExpression(nsName, r.Body, symbolTable));
                        break;
                    default:
                        break;
                }
            }
            return local.Build();
        }
        public string EvaluateExpression(string nsName, INode expression, SymbolTable symbolTable)
        {
            return new ExpressionEvaluator(nsName, symbolTable, Parser, RedefinitionTable).EvaluateExpression(expression);
        }
        public string EvaluateExpression(string nsName, INode expression, out INode expressionType, SymbolTable symbolTable)
        {
            return new ExpressionEvaluator(nsName, symbolTable, Parser, RedefinitionTable).EvaluateExpression(expression, out expressionType);
        }
        bool DefineSymbol(string symbol, INode value)
        {
            return SymbolTable.TryAdd(symbol, value);
        }
        void RecognizeGlobalStatement(string nsName, string redefinition, INode statement)
        {
            switch (statement)
            {
                case FunctionNode function:
                    Emitter.DefineFunction(redefinition, SolveType(function.Type, SolveName(function.Type)), BuildFunctionParameters(function.ParameterList), GenerateFromFunction(nsName, function));
                    break;
                default:
                    break;
            }
        }
        public static string SolveName(INode name)
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
                    "chr" => "char",
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
        void InitBuiltIns()
        {
            Emitter.DefineInclude("MugStandard.h");
            RedefinitionTable.Add(Parser.Lexer.ModuleName+".print(chr)", "putchar");
            //SymbolTable.Add("STD_putchar", putchar);
        }
        bool IsEntryPoint(string name)
        {
            return name == Parser.Lexer.ModuleName+".main()";
        }
        bool TryDefine(string name, INode value)
        {
            string gen;
            do
                gen = GenerateName();
            while (!RedefinitionTable.TryAdd(name, gen));
            if (IsEntryPoint(name))
                EntryPointGeneratedName = gen;
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
        void EmitEntryPoint()
        {
            if (EntryPointGeneratedName is null)
                CompilationErrors.Throw("Missing entry point");
            Emitter.DefineEntryPoint(EntryPointGeneratedName);
        }
        void ProcessNamespace(NamespaceNode ns, string nsName = "")
        {
            foreach (var node in ns.Members.Nodes)
                DefineNode(nsName + SolveName(ns.Name), node);
        }
        void ProcessSymbols()
        {
            foreach (var node in SymbolTable)
                RecognizeGlobalStatement(Parser.Lexer.ModuleName, node.Key, node.Value);
        }
        public string Generate()
        {
            InitBuiltIns();
            ProcessNamespace(Parser.Module);
            ProcessSymbols();
            EmitEntryPoint();
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
