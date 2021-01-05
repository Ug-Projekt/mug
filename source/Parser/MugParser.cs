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
            Lexer.Throw(Current(), error);
        }
        void ParseErrorEOF(params string[] error)
        {
            var errors = string.Join("", error);
            Lexer.Throw(Back(), errors, errors != "" ? ": " : "", "Unexpected <EOF>");
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
        Token Current()
        {
            return Lexer.TokenCollection[CurrentIndex];
        }
        Token Back()
        {
            return Lexer.TokenCollection[CurrentIndex - 1];
        }
        Token ExpectMultiple(string error, params TokenKind[] kinds)
        {
            for (int i = 0; i < kinds.Length; i++)
                if (Current().Kind == kinds[i])
                {
                    CurrentIndex++;
                    return Back();
                }
            ParseError("Expected `", string.Join("`, `", kinds), "`, found ", Current().Kind.ToString(), (error != "" ? "`: " + error : "`"));
            return new();
        }
        Token ExpectMultipleMute(string error, params TokenKind[] kinds)
        {
            for (int i = 0; i < kinds.Length; i++)
                if (Current().Kind == kinds[i])
                {
                    CurrentIndex++;
                    return Back();
                }
            ParseError(error);
            return new();
        }
        Token Expect(string error, TokenKind kind)
        {
            if (Current().Kind != kind)
                ParseError("Expected `", kind.ToString(), "`, found `", Current().Kind.ToString(), (error != "" ? "`: " + error : "`"));
            CurrentIndex++;
            return Back();
        }
        bool Match(TokenKind kind)
        {
            return Current().Kind == kind;
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
            ExpectMultipleMute("Expected a type: built in or user defined, but found `" + Current().Kind.ToString() + "`", TokenKind.Identifier,
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
                TokenKind.KeyTunknow);
            return Back();
        }
        Parameter ExpectParameter()
        {
            var name = Expect("In parameter declartion must specify the param name;", TokenKind.Identifier).Value;
            Expect("In parameter declaration must specify the param type;", TokenKind.Colon);
            var type = ExpectType();
            ExpectMultiple("", TokenKind.Comma, TokenKind.ClosePar);
            CurrentIndex--;
            return new Parameter(type, name, new());          // add support for optional parameter
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
            var constant = Current().Kind.ToString().Length > 8 && Current().Kind.ToString()[..8] == "Constant";
            if (constant)
                CurrentIndex++;
            return constant;
        }
        bool MatchInParExpression(out ExpressionNode e)
        {
            e = null;
            if (!Match(TokenKind.OpenPar))
                return false;
            e = ExpectExpression(TokenKind.ClosePar);
            return true;
        }
        bool MatchValue(out ExpressionNode e)
        {
            e = null;
            if (!MatchConstantAdvance() &&
                !MatchAdvance(TokenKind.Identifier))
                return false;
            var digit = Back();
            e = new ExpressionNode() { SingleValue = digit, Position = digit.Position };
            return true;
        }
        bool MatchFactor(out ExpressionNode e)
        {
            return MatchValue(out e) || MatchInParExpression(out e);      // add support for call or other
        }
        ExpressionNode ExpectTerm()
        {
            if (!MatchTerm(out ExpressionNode e))
                ParseError("Expected term (mutiply or divide of factors, factor, etc..), but found an unknow expression stars with `", Current().Kind.ToString(), "`;");
            return e;
        }
        bool MatchTerm(out ExpressionNode e)
        {
            e = null;
            if (!MatchFactor(out ExpressionNode left))
                return false;
            if (!MatchAdvance(TokenKind.Star) &&
                !MatchAdvance(TokenKind.Slash))
            {
                e = new ExpressionNode() { SingleValue = left, Operator = ToOperatorKind(Current().Kind), Position = left.Position };
                return true;
            }
            var op = Back();
            var rigth = ExpectTerm();
            e = new ExpressionNode() { Left = left, Rigth = rigth, Operator = ToOperatorKind(op.Kind), Position = new(left.Position.Start, rigth.Position.End) };
            return true;
        }
        ExpressionNode ExpectExpression(TokenKind endWith)
        {
            ExpressionNode e = null;
            if (MatchTerm(out ExpressionNode left))
            {
                if (!Match(TokenKind.Plus) &&
                    !Match(TokenKind.Minus))
                {
                    e = new ExpressionNode() { SingleValue = left, Position = left.Position };
                }
                else
                {
                    var rigth = ExpectTerm();
                    e = new ExpressionNode() { Left = left, Rigth = rigth, Position = new(left.Position.Start, rigth.Position.End) };
                }
            }
            Expect("At end of expression was expected `" + endWith+ "`;", endWith);
            return e;
        }
        bool VariableDeclaration(out IStatement statement)
        {
            statement = null;
            if (!MatchAdvance(TokenKind.KeyVar))
                return false;
            var name = Expect("Expected the variable id;", TokenKind.Identifier);
            Expect("Expected a type, in variable declaration, implicit type are not supported yet;", TokenKind.Colon);
            var type = ExpectType();
            if (MatchAdvance(TokenKind.Semicolon))
            {
                statement = new VariableStatement() { Body = null, IsDefined = false, Name = name.Value, Position = name.Position, Type = type };
                return true;
            }
            Expect("To define the value of a variable must open the body with `=`, or you can only declare a variable putting after type spec the symbol `;`;", TokenKind.Equal);
            var body = ExpectExpression(TokenKind.Semicolon);
            statement = new VariableStatement() { Body = body, IsDefined = true, Name = name.Value, Position = name.Position, Type = type };
            return true;
        }
        IStatement ExpectStatement()
        {
            IStatement statement;
            if (!VariableDeclaration(out statement))
                ParseError("Unknow local statement");
            return statement;
        }
        BlockNode ExpectBlock()
        {
            Expect("A block statement must start with `{` token", TokenKind.OpenBrace);
            var block = new BlockNode();
            while (!Match(TokenKind.CloseBrace))
                block.Add(ExpectStatement());
            Expect("A block statement must end with `}` token", TokenKind.CloseBrace);
            return block;
        }
        ParametersNode ExpectParametersDeclaration()
        {
            Expect("", TokenKind.OpenPar);
            var parameters = new ParametersNode();
            while (!MatchAdvance(TokenKind.ClosePar))
                parameters.Add(ExpectParameter());
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
            node = new FunctionNode() { Body = body, Modifier = Modifier.Public, Name = name.Value, Parameters = parameters, Type = type, Position = name.Position };
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
                    ParseError("Unknow global statement");
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
