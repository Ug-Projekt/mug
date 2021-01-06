using Mug.Models.Lexer;
using Mug.Models.Parser.NodeKinds;
using Mug.Models.Parser.NodeKinds.Statements;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Parser
{
    public class MugParser
    {
        public CompilationUnitNode NodeCollection;
        public MugLexer Lexer;
        int CurrentIndex = 0;
        void ParseError(params string[] error)
        {
            if (IsEOF())
                ParseErrorEOF();
            Lexer.Throw(Current, error);
        }
        void ParseErrorEOF(params string[] error)
        {
            var errors = string.Join("", error);
            Lexer.Throw(Back, errors, errors != "" ? ": " : "", "Unexpected <EOF>");
        }
        bool IsEOF()
        {
            return Lexer.TokenCollection[CurrentIndex].Kind == TokenKind.EOF;
        }
        public MugParser(string moduleName, string source)
        {
            Lexer = new(moduleName, source);
        }
        public MugParser(MugLexer lexer)
        {
            Lexer = lexer;
        }
        Token Current
        {
            get
            {
                return Lexer.TokenCollection[CurrentIndex];
            }
        }
        Token Back
        {
            get
            {
                if (CurrentIndex - 1 > 0)
                    return Lexer.TokenCollection[CurrentIndex - 1];
                return new Token();
            }
        }
        Token ExpectMultiple(string error, params TokenKind[] kinds)
        {
            for (int i = 0; i < kinds.Length; i++)
                if (Current.Kind == kinds[i])
                {
                    CurrentIndex++;
                    return Back;
                }
            ParseError("Expected `", string.Join("`, `", kinds), "`, found ", Current.Kind.ToString(), (error != "" ? "`: " + error : "`"));
            return new();
        }
        Token ExpectMultipleMute(string error, params TokenKind[] kinds)
        {
            for (int i = 0; i < kinds.Length; i++)
                if (Current.Kind == kinds[i])
                {
                    CurrentIndex++;
                    return Back;
                }
            ParseError(error);
            return new();
        }
        Token Expect(string error, TokenKind kind)
        {
            if (Current.Kind != kind)
                ParseError("Expected `", kind.ToString(), "`, found `", Current.Kind.ToString(), (error != "" ? "`: " + error : "`"));
            CurrentIndex++;
            return Back;
        }
        bool Match(TokenKind kind)
        {
            return Current.Kind == kind;
        }
        bool MatchAdvance(TokenKind kind)
        {
            var expect = Match(kind);
            if (expect)
            {
                CurrentIndex++;
                return true;
            }
            return false;
        }
        Token ExpectType()
        {
            if (MatchAdvance(TokenKind.OpenBracket, out Token open))
            {
                var type = ExpectType();
                var close = Expect("An array type definition must end with `CloseBracket`;", TokenKind.CloseBracket);
                return new Token(type.LineAt, TokenKind.KeyTarray, type, new(open.Position.Start, close.Position.End));
            }
            ExpectMultipleMute("Expected a type: built in or user defined, but found `" + Current.Kind.ToString() + "`", TokenKind.Identifier,
                TokenKind.KeyTi32,
                TokenKind.KeyTVoid,
                TokenKind.KeyTbool,
                TokenKind.KeyTchr,
                TokenKind.KeyTi64,
                TokenKind.KeyTi8,
                TokenKind.KeyTu32,
                TokenKind.KeyTu8,
                TokenKind.KeyTu64,
                TokenKind.KeyTstr,
                TokenKind.KeyTunknown);
            return Back;
        }
        bool MatchAdvance(TokenKind kind, out Token token)
        {
            token = new();
            if (MatchAdvance(kind))
            {
                token = Back;
                return true;
            }
            return false;
        }
        bool Match(TokenKind kind, out Token token)
        {
            token = new();
            if (Match(kind))
            {
                token = Back;
                return true;
            }
            return false;
        }
        Parameter ExpectParameter(bool isFirst)
        {
            if (!isFirst)
                Expect("Parameters must separed by a comma;", TokenKind.Comma);
            var name = Expect("In parameter declaration must specify the param name;", TokenKind.Identifier).Value;
            Expect("In parameter declaration must specify the param type;", TokenKind.Colon);
            var type = ExpectType();
            ExpectMultiple("", TokenKind.Comma, TokenKind.ClosePar);
            CurrentIndex--;
            return new Parameter(type, name.ToString(), new());          // add support for optional parameter
        }
        OperatorKind ToOperatorKind(TokenKind op)
        {
            return op switch {
                TokenKind.Plus => OperatorKind.Sum,
                TokenKind.Minus=> OperatorKind.Subtract,
                TokenKind.Star => OperatorKind.Multiply,
                TokenKind.Slash=> OperatorKind.Divide
            };
        }
        bool MatchConstantAdvance()
        {
            var current = Current.Kind;
            var constant =
                current == TokenKind.ConstantChar ||
                current == TokenKind.ConstantDigit ||
                current == TokenKind.ConstantFloatDigit ||
                current == TokenKind.ConstantString;//Current.Kind.ToString().Length > 8 && Current.Kind.ToString()[..8] == "Constant";
            if (constant)
                CurrentIndex++;
            return constant;
        }
        bool MatchInParExpression(out INode e)
        {
            e = null;
            if (!MatchAdvance(TokenKind.OpenPar))
                return false;
            e = ExpectExpression(TokenKind.ClosePar);
            return true;
        }
        bool MatchValue(out INode e)
        {
            e = null;
            if (!MatchConstantAdvance() &&
                !MatchAdvance(TokenKind.Identifier))
                return false;
            var digit = Back;
            e = new ValueNode() { SingleValue = digit };
            return true;
        }
        bool MatchCallStatement(out INode e)
        {
            e = null;
            if (!MatchAdvance(TokenKind.Identifier, out Token name))
                return false;
            if (!MatchAdvance(TokenKind.OpenPar))
            {
                CurrentIndex--;
                return false;
            }
            if (Current.Kind == TokenKind.ClosePar)
            {
                e = new CallStatement() { Name = name.Value.ToString(), Parameters = null, Position = name.Position };
                CurrentIndex++;
                return true;
            }
            NodeBuilder parameters = new();
            while (Back.Kind != TokenKind.ClosePar)
                parameters.Add(ExpectExpression(TokenKind.Comma, TokenKind.ClosePar));
            e = new CallStatement() { Name = name.Value.ToString(), Parameters = parameters, Position = name.Position };
            return true;
        }
        bool MatchTerm(out INode e)
        {
            return MatchCallStatement(out e) || MatchInParExpression(out e) || MatchValue(out e);      // add support for call or other
        }
        INode ExpectFactor()
        {
            if (!MatchFactor(out INode e) &&
                !MatchInParExpression(out e))
                ParseError("Expected factor (term times term, or divide, etc..), but found an unknown expression stars with `", Current.Kind.ToString(), "`;");
            return e;
        }
        bool MatchFactor(out INode e)
        {
            e = null;
            if (!MatchTerm(out INode left))
                return false;
            if (!MatchAdvance(TokenKind.Star) &&
                !MatchAdvance(TokenKind.Slash))
            {
                e = left;
                return true;
            }
            var op = Back;
            var rigth = ExpectFactor();
            e = new ExpressionNode() { Left = left, Rigth = rigth, Operator = ToOperatorKind(op.Kind), Position = new(left.Position.Start, rigth.Position.End) };
            return true;
        }
        INode ExpectExpression(params TokenKind[] end)
        {
            INode e = null;
            if (MatchFactor(out INode left))
            {
                if (!MatchAdvance(TokenKind.Plus) &&
                    !MatchAdvance(TokenKind.Minus))
                {
                    e = left;
                }
                else
                {
                    var op = Back.Kind;
                    var rigth = ExpectExpression(end);
                    CurrentIndex--;
                    e = new ExpressionNode() { Operator = ToOperatorKind(op), Left = left, Rigth = rigth, Position = new(left.Position.Start, rigth.Position.End) };
                }
            }
            if (e is null)
                ParseError("Missing expression: `", Current.Kind.ToString(), "` is not a valid symbol in expression;");
            ExpectMultiple("In the current context, the expression scope should end with the precedent printed expected symbols;", end);
            return e;
        }
        bool VariableDefinition(out IStatement statement)
        {
            statement = null;
            if (!MatchAdvance(TokenKind.KeyVar))
                return false;
            var name = Expect("Expected the variable id;", TokenKind.Identifier);
            Expect("Expected a type, in variable definition, implicit type are not supported yet;", TokenKind.Colon);
            var type = ExpectType();
            if (MatchAdvance(TokenKind.Semicolon))
            {
                statement = new VariableStatement() { Body = null, IsDefined = false, Name = name.Value.ToString(), Position = name.Position, Type = type };
                return true;
            }
            Expect("To define the value of a variable must open the body with `=`, or you can only declare a variable putting after type spec the symbol `;`;", TokenKind.Equal);
            var body = ExpectExpression(TokenKind.Semicolon);
            statement = new VariableStatement() { Body = body, IsDefined = true, Name = name.Value.ToString(), Position = name.Position, Type = type };
            return true;
        }
        bool ConstantDefinition(out IStatement statement)
        {
            statement = null;
            if (!MatchAdvance(TokenKind.KeyConst))
                return false;
            var name = Expect("Expected the constant id;", TokenKind.Identifier);
            Expect("Expected a type, in constant definition, implicit type are not supported yet;", TokenKind.Colon);
            var type = ExpectType();
            if (Match(TokenKind.Semicolon))
                ParseError("A constant cannot be declared without a body;");
            Expect("To define the value of a constant must open the body with `=`;", TokenKind.Equal);
            var body = ExpectExpression(TokenKind.Semicolon);
            statement = new ConstantStatement() { Body = body, Name = name.Value.ToString(), Position = name.Position, Type = type };
            return true;
        }
        bool ReturnDeclaration(out IStatement statement)
        {
            statement = null;
            if (!MatchAdvance(TokenKind.KeyReturn))
                return false;
            var pos = Back.Position;
            if (MatchAdvance(TokenKind.Semicolon))
            {
                statement = new ReturnStatement() { Position = pos, IsDefined = false };
                return true;
            }
            var body = ExpectExpression(TokenKind.Semicolon);
            statement = new ReturnStatement() { Position = pos, Body = body };
            return true;
        }
        IStatement ExpectStatement()
        {
            IStatement statement;
            if (!VariableDefinition(out statement))
                if (!ReturnDeclaration(out statement))
                    if (!ConstantDefinition(out statement))
                        if (MatchCallStatement(out INode node))
                        {
                            statement = (IStatement)node;
                            CurrentIndex++;
                        }
                        else
                            ParseError("In the current local context, this is not a valid imperative statement;");
            return (IStatement)statement;
        }
        BlockNode ExpectBlock()
        {
            Expect("A block statement must start with `{` token;", TokenKind.OpenBrace);
            var block = new BlockNode();
            while (!Match(TokenKind.CloseBrace))
                block.Add(ExpectStatement());
            Expect("A block statement must end with `}` token;", TokenKind.CloseBrace);
            return block;
        }
        ParametersNode ExpectParametersDeclaration()
        {
            Expect("In function definition you must open a parenthesis to declare parameters, or if the function does not accept parameters just open and close par: `()`", TokenKind.OpenPar);
            var parameters = new ParametersNode();
            while (!MatchAdvance(TokenKind.ClosePar))
            {
                parameters.Add(ExpectParameter(parameters.Parameters.Length == 0));
            }
            return parameters;
        }
        bool FunctionDefinition(out INode node)
        {
            node = new FunctionNode();
            if (!MatchAdvance(TokenKind.KeyFunc))
                return false;
            var name = Expect("In function definition must specify the name;", TokenKind.Identifier);
            var parameters = ExpectParametersDeclaration();
            var type = new Token();
            if (Match(TokenKind.OpenBrace))
                type = new Token(name.LineAt, TokenKind.KeyTVoid, null, name.Position);
            else
            {
                Expect("In function definition must specify the type, or if it returns void the type can by omitted;", TokenKind.Colon);
                type = ExpectType();
            }
            var body = ExpectBlock();
            node = new FunctionNode() { Body = body, Modifier = Modifier.Public, Name = name.Value.ToString(), Parameters = parameters, Type = type, Position = name.Position };
            return true;
        }
        NodeBuilder ProcessToken(Token token)
        {
            if (token.Kind == TokenKind.EOF)
                return null;
            NodeBuilder nodes = new();
            // global scope: only global statements (var declaration or definition,
            //                       type definition, function definition, namespace definition);
            //
            while (!IsEOF())
            {
                INode node;
                if (!FunctionDefinition(out node))
                    ParseError("In the current global context, this is not a valid global statement;");
                nodes.Add(node);
            }
            return nodes;
        }
        public CompilationUnitNode GetNodeCollection()
        {
            var firstToken = Lexer.TokenCollection[CurrentIndex];
            var compilationUnit = new CompilationUnitNode();
            if (firstToken.Kind == TokenKind.EOF)
                return compilationUnit;
            compilationUnit.GlobalScope = ProcessToken(firstToken);
            return compilationUnit;
        }
        public List<Token> GetTokenCollection() => Lexer.Tokenize();
        public List<Token> GetTokenCollection(out MugLexer lexer) { lexer = Lexer; return Lexer.Tokenize(); }
    }
}
