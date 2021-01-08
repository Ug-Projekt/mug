using Mug.Compilation;
using Mug.Models.Lexer;
using Mug.Models.Parser.NodeKinds;
using Mug.Models.Parser.NodeKinds.Statements;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mug.Models.Parser
{
    public class MugParser
    {
        public MugLexer Lexer;
        int CurrentIndex = 0;
        void ParseError(params string[] error)
        {
            if (Match(TokenKind.EOF))
                ParseErrorEOF();
            Lexer.Throw(Current, error);
        }
        void ParseErrorEOF(params string[] error)
        {
            var errors = string.Join("", error);
            Lexer.Throw(Back, errors, errors != "" ? ": " : "", "Unexpected <EOF>");
        }
        public MugParser(string moduleName, string source)
        {
            Lexer = new(moduleName, source);
            Lexer.Tokenize();
        }
        public MugParser(MugLexer lexer)
        {
            Lexer = lexer;
            Lexer.Tokenize();
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
                if (CurrentIndex - 1 >= 0)
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
        bool MatchIdentifierAdvance(out MemberAccessNode members)
        {
            members = null;
            if (!MatchAdvance(TokenKind.Identifier))
                return false;
            members = new MemberAccessNode();
            members.Add(Back);
            while (MatchAdvance(TokenKind.Dot))
                members.Add(Expect("After `.` mus put a member to access in;", TokenKind.Identifier));
            members.Position = members.Members[0].Position;
            return true;
        }
        bool MatchAdvance(TokenKind kind)
        {
            var expect = Match(kind);
            if (expect)
                CurrentIndex++;
            return expect;
        }
        Token ExpectType()
        {
            if (MatchAdvance(TokenKind.OpenBracket, out Token open))
            {
                var type = ExpectType();
                var close = Expect("An array type definition must end with `CloseBracket`;", TokenKind.CloseBracket);
                return new Token(type.LineAt, TokenKind.KeyTarray, type, new(open.Position.Start, close.Position.End));
            }
            Token find;
            if (MatchIdentifierAdvance(out MemberAccessNode members))
                find = new Token(members.Members[0].LineAt, TokenKind.Identifier, members, members.Position);
            else
                find = ExpectMultipleMute("Expected a type: built in or user defined, but found `" + Current.Kind.ToString() + "`",
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
            return find;
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
        Token ExpectConstantMute(params string[] error)
        {
            var match = MatchConstantAdvance();
            if (match)
                return Back;
            ParseError(error);
            return new ();
        }
        Parameter ExpectParameter(bool isFirst)
        {
            if (!isFirst)
                Expect("Parameters must be separed by a comma;", TokenKind.Comma);
            if (MatchAdvance(TokenKind.KeySelf))
            {
                if (!isFirst)
                {
                    CurrentIndex--;
                    ParseError("The `self` keyword must be placed as first parameter;");
                }
                else
                {
                    Expect("In the current context must specify the type of the instance to extend;", TokenKind.Colon);
                    var t = ExpectType();
                    if (Match(TokenKind.Equal))
                        ParseError("A self parameter cannot be an optional parameter;");
                    return new Parameter(t, "self", new(), true);
                }
            }
            var name = Expect("In parameter declaration must specify the param name;", TokenKind.Identifier).Value;
            Expect("In parameter declaration must specify the param type;", TokenKind.Colon);
            var type = ExpectType();
            var defaultvalue = new Token();
            if (MatchAdvance(TokenKind.Equal))
                 defaultvalue = ExpectConstantMute("A default parameter value must be evaluable at compilation-time, so a constant;");
            ExpectMultiple("In the current context is only allowed to close the parameter list or add a parameter to it;", TokenKind.Comma, TokenKind.ClosePar);
            CurrentIndex--;
            return new Parameter(type, name.ToString(), defaultvalue);
        }
        OperatorKind ToOperatorKind(TokenKind op)
        {
            return op switch {
                TokenKind.Plus => OperatorKind.Sum,
                TokenKind.Minus=> OperatorKind.Subtract,
                TokenKind.Star => OperatorKind.Multiply,
                TokenKind.Slash=> OperatorKind.Divide,
                TokenKind.RangeDots => OperatorKind.Range,
            };
        }
        bool MatchConstantAdvance()
        {
            var current = Current.Kind;
            var constant =
                current == TokenKind.ConstantChar ||
                current == TokenKind.ConstantDigit ||
                current == TokenKind.ConstantFloatDigit ||
                current == TokenKind.ConstantString ||
                current == TokenKind.ConstantBoolean;
            if (constant)
                CurrentIndex++;
            return constant;
        }
        bool MatchInParExpression(out INode e)
        {
            e = null;
            if (!MatchAdvance(TokenKind.OpenPar))
                return false;
            e = ExpectExpression(true, TokenKind.ClosePar);
            return true;
        }
        bool MatchValue(out INode e)
        {
            e = null;
            if (MatchAdvance(TokenKind.Identifier, out Token id))
            {
                e = _lastName;
                return true;
            }
            if (!MatchConstantAdvance() &&
                !MatchAdvance(TokenKind.KeySelf))
                return false;
            var value = Back;
            e = new ValueNode() { SingleValue = value };
            return true;
        }
        MemberAccessNode _lastName;
        bool MatchCallStatement(out INode e)
        {
            e = null;
            if (!MatchIdentifierAdvance(out MemberAccessNode name))
                return false;
            _lastName = name;
            if (!MatchAdvance(TokenKind.OpenPar))
            {
                CurrentIndex--;
                return false;
            }
            if (Current.Kind == TokenKind.ClosePar)
            {
                e = new CallStatement() { Name = name, Parameters = null, Position = name.Position };
                CurrentIndex++;
                return true;
            }
            NodeBuilder parameters = new();
            while (Back.Kind != TokenKind.ClosePar)
                parameters.Add(ExpectExpression(true, TokenKind.Comma, TokenKind.ClosePar));
            e = new CallStatement() { Name = name, Parameters = parameters, Position = name.Position };
            return true;
        }
        bool MatchTerm(out INode e)
        {
            if (MatchAdvance(TokenKind.Minus) ||
                MatchAdvance(TokenKind.Plus) ||
                MatchAdvance(TokenKind.Negation))
            {
                if (Match(TokenKind.Minus) ||
                Match(TokenKind.Plus) ||
                Match(TokenKind.Negation))
                    ParseError("Unexpected double prefix operator;");
                if (!MatchTerm(out e))
                {
                    CurrentIndex--;
                    ParseError("Unexpected prefix operator;");
                }
                CurrentIndex--;
                e = new PrefixOperator() { Expression = e, Position = Back.Position, Prefix = Back.Kind };
                CurrentIndex++;
                return true;
            }
            return MatchCallStatement(out e) || MatchValue(out e) || MatchInParExpression(out e);
        }
        INode ExpectFactor()
        {
            if (!MatchFactor(out INode e) &&
                !MatchInParExpression(out e))
                ParseError("Expected factor (term times term, or divide, etc..), but found an unknown expression stars with `", Current.Kind.ToString(), "`;");
            return e;
        }
        bool MatchFactorOps()
        {
            return
                MatchAdvance(TokenKind.Star) ||
                MatchAdvance(TokenKind.Slash) ||
                MatchAdvance(TokenKind.RangeDots) ||
                MatchAdvance(TokenKind.Dot);
        }
        bool MatchFactor(out INode e)
        {
            e = null;
            if (!MatchTerm(out INode left))
                return false;
            if (!MatchFactorOps())
            {
                e = left;
                return true;
            }
            var op = Back;
            if (op.Kind == TokenKind.Dot)
            {
                if (MatchCallStatement(out INode statement))
                    left = new CallInstanceMemberAccessNode() { Instance = left, Call = (CallStatement)statement };
                else
                {
                    left = new InstanceMemberAccessNode() { Instance = left, Members = _lastName };
                    CurrentIndex++;
                }
                if (MatchFactorOps())
                    op = Back;
                else
                {
                    e = left;
                    return true;
                }
            }
            var right = ExpectFactor();
            e = new ExpressionNode() { Left = left, Right = right, Operator = ToOperatorKind(op.Kind), Position = new(left.Position.Start, right.Position.End) };
            return true;
        }
        bool MatchBooleanOperator(out Token op)
        {
            return MatchAdvance(TokenKind.BoolOperatorEQ, out op) ||
                MatchAdvance(TokenKind.BoolOperatorNEQ, out op) ||
                MatchAdvance(TokenKind.BoolOperatorMajor, out op) ||
                MatchAdvance(TokenKind.BoolOperatorMinor, out op) ||
                MatchAdvance(TokenKind.BoolOperatorMajEQ, out op) ||
                MatchAdvance(TokenKind.BoolOperatorMinEQ, out op) ||
                MatchAdvance(TokenKind.KeyIn, out op);
        }
        INode ExpectExpression(bool isFirst, params TokenKind[] end)
        {
            INode e = null;
            if (MatchFactor(out INode left))
            {
                if (!MatchAdvance(TokenKind.Plus) &&
                    !MatchAdvance(TokenKind.Minus))
                    e = left;
                else
                {
                    var op = Back.Kind;
                    var right = ExpectExpression(false, end);
                    CurrentIndex--;
                    e = new ExpressionNode() { Operator = ToOperatorKind(op), Left = left, Right = right, Position = new(left.Position.Start, right.Position.End) };
                }
            }
            if (e is null)
                ParseError("Missing expression: `", Current.Kind.ToString(), "` is not a valid symbol in expression;");
            if (MatchBooleanOperator(out Token boolOP))
            {
                if (!isFirst)
                    return e;
                var right = ExpectExpression(false, end);
                e = new BooleanExpressionNode() { Operator = boolOP.Kind, Position = boolOP.Position, Left = e, Right = right };
                CurrentIndex--;
                if (MatchBooleanOperator(out _))
                {
                    CurrentIndex--;
                    ParseError("Double boolean operator not allowed, to compare two boolean expressions please put the first operand into `()`;");
                }
                CurrentIndex++;
                return e;
            }
            ExpectMultipleMute("`"+Current.Kind+"` is not a valid token in the current context;", end);
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
                statement = new VariableStatement() { Body = null, IsAssigned = false, Name = name.Value.ToString(), Position = name.Position, Type = type };
                return true;
            }
            Expect("To define the value of a variable must open the body with `=`, or you can only declare a variable putting after type spec the symbol `;`;", TokenKind.Equal);
            var body = ExpectExpression(true, TokenKind.Semicolon);
            statement = new VariableStatement() { Body = body, IsAssigned = true, Name = name.Value.ToString(), Position = name.Position, Type = type };
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
            var body = ExpectExpression(true, TokenKind.Semicolon);
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
                statement = new ReturnStatement() { Position = pos };
                return true;
            }
            var body = ExpectExpression(true, TokenKind.Semicolon);
            statement = new ReturnStatement() { Position = pos, Body = body };
            return true;
        }
        bool AssignValue(out IStatement statement)
        {
            statement = null;
            if (Back.Kind == TokenKind.Dot)
                return false;
            if (!MatchIdentifierAdvance(out MemberAccessNode name))
                return false;
            if (!MatchAdvance(TokenKind.Equal))
            {
                CurrentIndex--;
                return false;
            }
            var pos = Back.Position;
            var body = ExpectExpression(true, TokenKind.Semicolon);
            statement = new AssignStatement() { Position = pos, Name = name, Body = body };
            return true;
        }
        bool ConditionDefinition(out IStatement statement)
        {
            statement = null;
            if (!MatchAdvance(TokenKind.KeyIf, out Token key) &&
                !MatchAdvance(TokenKind.KeyElif, out key) &&
                !MatchAdvance(TokenKind.KeyWhile, out key))
                return false;
            var expression = ExpectExpression(true, TokenKind.OpenBrace);
            CurrentIndex--;
            var body = ExpectBlock();
            statement = new ConditionalStatement() { Position = key.Position, Expression = expression, Kind = key.Kind, Body = body };
            return true;
        }
        MemberAccessNode ExpectIdentifier()
        {
            var match = MatchIdentifierAdvance(out MemberAccessNode member);
            if (!match)
                ParseError("Expected identifier");
            return member;
        }
        bool ForLoopDefinition(out IStatement statement)
        {
            statement = null;
            if (!MatchAdvance(TokenKind.KeyFor, out Token key))
                return false;
            var name = ExpectIdentifier();
            INode counter = new ForCounterReference() { Position = name.Position, ReferenceName = name };
            var pos = name.Position;
            if (MatchAdvance(TokenKind.Colon))
            {
                INode varBody = null;
                var type = ExpectType();
                if (MatchAdvance(TokenKind.Equal))
                    varBody = ExpectFactor();
                counter = new VariableStatement() { Position = pos, Body = varBody, IsAssigned = varBody != null, Name = name.Members[0].Value.ToString(), Type = type };
            }
            else if (MatchAdvance(TokenKind.Equal))
                counter = new AssignStatement() { Position = pos, Body = ExpectFactor(), Name = name };
            var op = ExpectMultiple("Expected an operator for the for statement, allowed: `in`, `to`;", TokenKind.KeyIn, TokenKind.KeyTo).Kind;
            var expression = ExpectFactor();
            var body = ExpectBlock();
            statement = new ForLoopStatement() { Counter = counter, Position = key.Position, RightExpression = expression, Operator = op, Body = body };
            return true;
        }
        bool OtherwiseConditionDefinition(out IStatement statement)
        {
            statement = null;
            if (!MatchAdvance(TokenKind.KeyElse))
                return false;
            var pos = Back.Position;
            var body = ExpectBlock();
            statement = new ConditionalStatement() { Position = pos, Kind = TokenKind.KeyElse, Body = body };
            return true;
        }
        IStatement ExpectStatement()
        {
            IStatement statement;
            if (!VariableDefinition(out statement))
                if (!ReturnDeclaration(out statement))
                    if (!ConstantDefinition(out statement))
                        if (!ConditionDefinition(out statement))
                            if (!ForLoopDefinition(out statement))
                                if (!OtherwiseConditionDefinition(out statement))
                                    if (!AssignValue(out statement))
                                        if (MatchCallStatement(out INode node))
                                        {
                                            statement = (IStatement)node;
                                            CurrentIndex++;
                                        }
                                        else
                                            ParseError("In the current local context, this is not a valid imperative statement;");
            return statement;
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
        ParameterListNode ExpectParameterListDeclaration()
        {
            Expect("In function definition you must open a parenthesis to declare parameters, or if the function does not accept parameters just open and close pars: `()`", TokenKind.OpenPar);
            var parameters = new ParameterListNode();
            var count = 0;
            while (!MatchAdvance(TokenKind.ClosePar))
            {
                parameters.Add(ExpectParameter(count == 0));
                count++;
            }
            return parameters;
        }
        bool FunctionDefinition(out IStatement node)
        {
            node = new FunctionNode();
            if (!MatchAdvance(TokenKind.KeyFunc))
                return false;
            var name = Expect("In function definition must specify the name;", TokenKind.Identifier);
            var parameters = ExpectParameterListDeclaration();
            var type = new Token();
            if (Match(TokenKind.OpenBrace))
                type = new Token(name.LineAt, TokenKind.KeyTVoid, null, name.Position);
            else
            {
                Expect("In function definition must specify the type, or if it returns void the type can by omitted;", TokenKind.Colon);
                type = ExpectType();
            }
            var body = ExpectBlock();
            node = new FunctionNode() { Body = body, Modifier = Modifier.Public, Name = name.Value.ToString(), ParameterList = parameters, Type = type, Position = name.Position };
            return true;
        }
        bool NamespaceDefinition(out IStatement node)
        {
            node = null;
            if (!MatchIdentifierAdvance(out MemberAccessNode name))
                return false;
            Expect("", TokenKind.OpenBrace);
            var body = ExpectNamespaceMembers(TokenKind.CloseBrace);
            Expect("", TokenKind.CloseBrace);
            node = new NamespaceNode() { GlobalScope = body, Position = name.Position, Name = name };
            return true;
        }
        NodeBuilder ExpectNamespaceMembers(TokenKind end = TokenKind.EOF)
        {
            NodeBuilder nodes = new();
            // global scope: only global statements (var declaration or definition,
            //                       type definition, function definition, namespace definition);
            //
            while (!Match(end))
            {
                IStatement statement;
                if (!FunctionDefinition(out statement))
                    if (!VariableDefinition(out statement))
                        if (!ConstantDefinition(out statement))
                            if (!NamespaceDefinition(out statement))
                                ParseError("In the current global context, this is not a valid global statement;");
                nodes.Add(statement);
            }
            return nodes;
        }
        public NamespaceNode Parse()
        {
            var firstToken = Lexer.TokenCollection[CurrentIndex];
            var compilationUnit = new NamespaceNode() { Name = new() };
            compilationUnit.Name.Add(new Token(0, TokenKind.Identifier, "global", new()));
            if (firstToken.Kind == TokenKind.EOF)
                return compilationUnit;
            compilationUnit.GlobalScope = ExpectNamespaceMembers();
            return compilationUnit;
        }
        public List<Token> GetTokenCollection() => Lexer.Tokenize();
        public List<Token> GetTokenCollection(out MugLexer lexer) { lexer = Lexer; return Lexer.Tokenize(); }
    }
}
