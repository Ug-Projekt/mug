using Mono.Cecil;
using Mono.Cecil.Cil;
using Mug.Compilation;
using Mug.Models.Evaluator;
using Mug.Models.Generator.Emitter;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using Mug.Models.Parser.NodeKinds.Statements;
using System;
using System.Collections.Generic;

namespace Mug.Models.Generator
{
    public class IRGenerator
    {
        public readonly MugParser Parser;
        public readonly MugEmitter Emitter;
        readonly Dictionary<string, object> Symbols = new();
        public IRGenerator(string moduleName, string source)
        {
            Parser = new(moduleName, source);
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
        TypeReference CreateType(INode type)
        {
            if (type is Token t)
                return Emitter.TypeOf(t.Kind);
            return null;
        }
        TypeReference FindType(INode type)
        {
            return Emitter.TypeOf(((Token)type).Kind);
        }
        string CreateName(INode name)
        {
            return (string)((Token)name).Value;
        }
        ParameterDefinition CreateParameter(ParameterNode parameter)
        {
            return new ParameterDefinition(CreateType(parameter.Type));
        }
        MethodReference CreateCall(CallStatement c)
        {
            if (c.Name is Token t && t.Kind == TokenKind.Identifier)
            {
                if (!Symbols.TryGetValue((string)t.Value, out var function))
                    GenerationError(c, "Undeclared function");
                if (function is FunctionNode func)
                {
                    var f = new MethodReference(func.Name, CreateType(func.Type));
                    foreach (var parameter in func.ParameterList.Parameters)
                        f.Parameters.Add(CreateParameter(parameter));
                    return f;
                }
                if (function is System.Reflection.MethodInfo reference)
                    return Emitter.Import(reference);
                GenerationError(c, "Uncallable member");
            }
            return null;
        }
        MethodDefinition GenerateFunction(FunctionNode function)
        {
            var body = new MethodDefinition(function.Name, MethodAttributes.Public | MethodAttributes.Static, CreateType(function.Type));
            var il = body.Body.GetILProcessor();
            foreach (var statement in function.Body.Statements)
            {
                switch (statement)
                {
                    case CallStatement c:
                        if (c.Parameters is not null)
                            foreach (var parameter in c.Parameters.Nodes)
                                new ExpressionEvaluator(il).Evaluate(parameter);
                        il.Append(il.Create(OpCodes.Call, CreateCall(c)));
                        break;
                    default:
                        GenerationError(statement, "Unallowed here");
                        break;
                }
            }
            il.Emit(OpCodes.Ret);
            return body;
        }
        void RecognizeGlobalStatement(INode statement)
        {
            switch (statement)
            {
                case FunctionNode function:
                    var body = GenerateFunction(function);
                    if (function.Name == "main")
                        Emitter.DefineMain(body);
                    else
                        Emitter.DefineFunction(body);
                    break;
                default:
                    GenerationError(statement, "Unallowed here");
                    break;
            }
        }
        void DefineBuiltinsSymbols()
        {
            Symbols.Add("println",
                typeof(Console).GetMethod("WriteLine", new[] { typeof(string) })
            );
            Symbols.Add("print",
                typeof(Console).GetMethod("Write", new[] { typeof(string) })
            );
        }
        void DefineSymbols()
        {
            DefineBuiltinsSymbols();
            foreach (var member in Parser.Module.Members.Nodes)
                switch (member)
                {
                    case FunctionNode function:
                        if (!Symbols.TryAdd(function.Name, function))
                            GenerationError(member, "Already defined");
                        break;
                    default:
                        GenerationError(member, "Unallowed here");
                        break;
                }
        }
        void ProcessGlobals()
        {
            foreach (var member in Parser.Module.Members.Nodes)
                RecognizeGlobalStatement(member);
        }
        public void Generate()
        {
            DefineSymbols();
            ProcessGlobals();
            Emitter.Save();
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
