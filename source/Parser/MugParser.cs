using Mug.Compilation;
using Mug.Models.Lexer;
using Mug.Models.Parser.NodeKinds;
using Mug.Models.Parser.NodeKinds.Directives;
using Mug.Models.Parser.NodeKinds.Statements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mug.Models.Parser
{
    public class MugParser
    {
        public readonly NamespaceNode Module = new();
        public readonly MugLexer Lexer;
        int _currentIndex = 0;
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
        Token Next
        {
            get
            {
                if (_currentIndex + 1 < Lexer.TokenCollection.Count)
                    return Lexer.TokenCollection[_currentIndex + 1];
                return new Token();
            }
        }
        Token Current
        {
            get
            {
                return Lexer.TokenCollection[_currentIndex];
            }
        }
        Token Back
        {
            get
            {
                if (_currentIndex - 1 >= 0)
                    return Lexer.TokenCollection[_currentIndex - 1];
                return new Token();
            }
        }
        Token ExpectMultiple(string error, params TokenKind[] kinds)
        {
            for (int i = 0; i < kinds.Length; i++)
                if (Current.Kind == kinds[i])
                {
                    _currentIndex++;
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
                    _currentIndex++;
                    return Back;
                }
            ParseError(error);
            return new();
        }
        Token Expect(string error, TokenKind kind)
        {
            if (Current.Kind != kind)
                ParseError("Expected `", kind.ToString(), "`, found `", Current.Kind.ToString(), (error != "" ? "`: " + error : "`"));
            _currentIndex++;
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
                _currentIndex++;
            return expect;
        }
        INode ExpectType(bool expectKeyTypeInGeneric = false)
        {
            if (MatchAdvance(TokenKind.OpenBracket, out Token open))
            {
                var type = ExpectType();
                var close = Expect("An array type definition must end with `CloseBracket`;", TokenKind.CloseBracket);
                return new Token(0, TokenKind.KeyTarray, type, new(open.Position.Start, close.Position.End));
            }
            INode find;
            if (MatchIdentifier(out var members))
                find = members;
            else
                find = ExpectKeywordTypes();
            if (MatchAdvance(TokenKind.OpenBracket))
            {
                if (expectKeyTypeInGeneric)
                    Expect("", TokenKind.KeyType);
                var type = ExpectType();
                find = new Token(0, TokenKind.KeyTgeneric, type, new(find.Position.Start, type.Position.End));
                Expect("Generic type specification must be wrote between `[]`;", TokenKind.CloseBracket);
            }
            
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
        ParameterNode ExpectParameter(bool isFirst)
        {
            if (!isFirst)
                Expect("Parameters must be separed by a comma;", TokenKind.Comma);
            if (MatchAdvance(TokenKind.KeySelf))
            {
                if (!isFirst)
                {
                    _currentIndex--;
                    ParseError("The `self` keyword must be placed as first parameter;");
                }
                else
                {
                    Expect("In the current context must specify the type of the instance to extend;", TokenKind.Colon);
                    var t = ExpectType(true);
                    if (Match(TokenKind.Equal))
                        ParseError("A self parameter cannot be an optional parameter;");
                    return new ParameterNode(t, "self", new(), true);
                }
            }
            var name = Expect("In parameter declaration must specify the param name;", TokenKind.Identifier).Value;
            Expect("In parameter declaration must specify the param type;", TokenKind.Colon);
            var type = ExpectType();
            var defaultvalue = new Token();
            if (MatchAdvance(TokenKind.Equal))
                 defaultvalue = ExpectConstantMute("A default parameter value must be evaluable at compilation-time, so a constant;");
            ExpectMultiple("In the current context is only allowed to close the parameter list or add a parameter to it;", TokenKind.Comma, TokenKind.ClosePar);
            _currentIndex--;
            return new ParameterNode(type, name.ToString(), defaultvalue);
        }
        OperatorKind ToOperatorKind(TokenKind op)
        {
            return op switch
            {
                TokenKind.Plus => OperatorKind.Sum,
                TokenKind.Minus => OperatorKind.Subtract,
                TokenKind.Star => OperatorKind.Multiply,
                TokenKind.Slash => OperatorKind.Divide,
                TokenKind.RangeDots => OperatorKind.Range,
            };
        }
        INode ExpectKeywordTypes()
        {
            var match = MatchBuiltInTypes(out var type);
            if (!match)
                ParseError("Expected a built in type, but found `" + Current.Kind.ToString() + "`;");
            return type;
        }
        bool MatchBuiltInTypes(out Token type)
        {
            return
                MatchAdvance(TokenKind.KeyTi32, out type) ||
                MatchAdvance(TokenKind.KeyTVoid, out type) ||
                MatchAdvance(TokenKind.KeyTbool, out type) ||
                MatchAdvance(TokenKind.KeyTchr, out type) ||
                MatchAdvance(TokenKind.KeyTi64, out type) ||
                MatchAdvance(TokenKind.KeyTi8, out type) ||
                MatchAdvance(TokenKind.KeyTu32, out type) ||
                MatchAdvance(TokenKind.KeyTu8, out type) ||
                MatchAdvance(TokenKind.KeyTu64, out type) ||
                MatchAdvance(TokenKind.KeyTstr, out type) ||
                MatchAdvance(TokenKind.KeyTunknown, out type);
        }
        bool MatchAllowedFirstBase()
        {
            return
                MatchAdvance(TokenKind.Identifier) ||
                MatchAdvance(TokenKind.KeySelf) ||
                MatchConstantAdvance() ||
                MatchBuiltInTypes(out _);
        }
        bool MatchIdentifier(out INode identifier)
        {
            return MatchIdentifier(out identifier, out _);
        }
        bool MatchIdentifier(out INode identifier, out int memberCount)
        {
            identifier = null;
            memberCount = 0;
            if (!MatchAllowedFirstBase())
                return false;
            identifier = Back;
            memberCount = 1;
            while (MatchAdvance(TokenKind.Dot))
            {
                identifier = new MemberNode() { Position = identifier.Position, Base = identifier, Member = Expect("Expected member, after `.`", TokenKind.Identifier) };
                memberCount+=2;
            }
            return true;
        }
        INode ExpectIdentifier()
        {
            if (!MatchIdentifier(out var identifier))
                ParseError("Expected identifier here, found `", Current.Kind.ToString(), "`;");
            return identifier;
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
                _currentIndex++;
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
            return MatchIdentifier(out e);
        }
        bool MatchCallStatement(out INode e)
        {
            e = null;
            if (!MatchIdentifier(out var name, out var count))
            {
                _currentIndex -= count;
                return false;
            }
            List<INode> generics = new();
            if (MatchAdvance(TokenKind.BooleanMinor))
            {
                if (Match(TokenKind.BooleanMajor))
                    ParseError("Invalid generic type passing content;");
                do
                    generics.Add(ExpectType());
                while (MatchAdvance(TokenKind.Comma));
                Expect("", TokenKind.BooleanMajor);
            }
            if (!MatchAdvance(TokenKind.OpenPar))
            {
                _currentIndex -=count;
                return false;
            }
            if (Current.Kind == TokenKind.ClosePar)
            {
                var c1 = new CallStatement() { Name = name, Parameters = null, Position = name.Position };
                c1.SetGenericTypes(generics);
                e = c1;
                _currentIndex++;
                return true;
            }
            NodeBuilder parameters = new();
            while (Back.Kind != TokenKind.ClosePar)
                parameters.Add(ExpectExpression(true, TokenKind.Comma, TokenKind.ClosePar));
            var c = new CallStatement() { Name = name, Parameters = parameters, Position = name.Position };
            c.SetGenericTypes(generics);
            e = c;
            return true;
        }
        bool MatchTerm(out INode e)
        {
            e = null;
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
                    _currentIndex--;
                    ParseError("Unexpected prefix operator;");
                }
                _currentIndex--;
                e = new PrefixOperator() { Expression = e, Position = Back.Position, Prefix = Back.Kind };
                _currentIndex++;
                return true;
            }
            var value = MatchValue(out e);
            var par = false;
            if (!value)
                par = MatchInParExpression(out e);
            start:
            if (par)
            {
                while (MatchAdvance(TokenKind.Dot))
                    e = new MemberNode() { Position = Back.Position, Base = e, Member = Expect("Expected member, after `.`", TokenKind.Identifier) };
                value = true;
            }
            if (value)
            {
                var oldIndex = _currentIndex;
                List<INode> generics = new();
                if (MatchAdvance(TokenKind.BooleanMinor))
                {
                    if (Match(TokenKind.BooleanMajor))
                        ParseError("Invalid generic type passing content;");
                    do
                        generics.Add(ExpectType());
                    while (MatchAdvance(TokenKind.Comma));
                    Expect("", TokenKind.BooleanMajor);
                }
                if (!MatchAdvance(TokenKind.OpenPar))
                {
                    if (generics.Count > 0)
                        ParseError("Expected parameter list, after generic types specification");
                    _currentIndex = oldIndex;
                    goto ret;
                }
                if (e is Token t)
                    if (t.Kind != TokenKind.Identifier)
                    {
                        _currentIndex-=2;
                        ParseError("Impossible perform operation call on this item;");
                    }
                if (Match(TokenKind.ClosePar))
                {
                    var c1 = new CallStatement() { Name = e, Parameters = null, Position = e.Position };
                    c1.SetGenericTypes(generics);
                    e = c1;
                    _currentIndex++;
                    goto ret;
                }
                NodeBuilder parameters = new();
                while (Back.Kind != TokenKind.ClosePar)
                    parameters.Add(ExpectExpression(true, TokenKind.Comma, TokenKind.ClosePar));
                var c = new CallStatement() { Name = e, Parameters = parameters, Position = e.Position };
                c.SetGenericTypes(generics);
                e = c;
            }
        ret:
            while (MatchAdvance(TokenKind.OpenBracket))
            {
                if (e is null)
                {
                    _currentIndex--;
                    ParseError("Cannot find a value to index, in the current context;");
                }
                var startPos = Back.Position.Start;
                var index = ExpectExpression(true, TokenKind.CloseBracket);
                e = new ArraySelectElemNode() { IndexExpression = index, Left = e, Position = new(startPos, Back.Position.End)};
            }
            if (Match(TokenKind.Dot))
            {
                par = true;
                goto start;
            }
            return value;
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
                MatchAdvance(TokenKind.RangeDots);
        }
        FieldAssignmentNode ExpectFieldAssign()
        {
            if (!MatchAdvance(TokenKind.Identifier))
                ParseError("Expected field assign;");
            var name = Back;
            Expect("Required `:` after field name;", TokenKind.Colon);
            var expression = ExpectExpression(true, TokenKind.Comma, TokenKind.CloseBrace);
            _currentIndex--;
            return new FieldAssignmentNode() { Name = name.Value.ToString(), Body = expression, Position = name.Position };
        }
        INode ExpectTerm()
        {
            if (!MatchTerm(out var e))
                ParseError("Expected term");
            return e;
        }
        bool MatchFactor(out INode e)
        {
            e = null;
            if (!MatchTerm(out INode left))
                return false;
            e = left;
            if (MatchFactorOps())
            {
                var op = Back;
                var right = ExpectTerm();
                do
                {
                    e = new ExpressionNode() { Left = e, Right = right, Operator = ToOperatorKind(op.Kind), Position = new(left.Position.Start, right.Position.End) };
                    if (MatchFactorOps())
                        op = Back;
                    else
                        break;
                } while (MatchTerm(out right));
            }
            return true;
        }
        bool MatchBooleanOperator(out Token op)
        {
            return MatchAdvance(TokenKind.BooleanEQ, out op) ||
                MatchAdvance(TokenKind.BooleanNEQ, out op) ||
                MatchAdvance(TokenKind.BooleanMajor, out op) ||
                MatchAdvance(TokenKind.BooleanMinor, out op) ||
                MatchAdvance(TokenKind.BooleanMajEQ, out op) ||
                MatchAdvance(TokenKind.BooleanMinEQ, out op) ||
                MatchAdvance(TokenKind.BooleanOR, out op) ||
                MatchAdvance(TokenKind.BooleanAND, out op) ||
                MatchAdvance(TokenKind.KeyIn, out op);
        }
        INode ExpectExpression(bool isFirst, params TokenKind[] end)
        {
            if (MatchAdvance(TokenKind.KeyIf)) {
                var expression = ExpectExpression(true, TokenKind.KeyTVoid);
                var ifBody = ExpectFactor();
                Expect("In inline conditions there must be the else body: place it here;", TokenKind.KeyElse);
                var elseBody = ExpectFactor();
                _currentIndex++;
                return new InlineConditionalExpression() { Expression = expression, IFBody = ifBody, ElseBody = elseBody };
            }
            if (MatchAdvance(TokenKind.KeyNew, out var token))
            {
                if (MatchAdvance(TokenKind.OpenBracket))
                {
                    var type = ExpectType();
                    Expect("Expected array size after its type;", TokenKind.Comma);
                    var size = ExpectExpression(true, TokenKind.CloseBracket);
                    _currentIndex--;
                    Expect("Expected `]` and the array body;", TokenKind.CloseBracket);
                    var array = new ArrayAllocationNode() { Size = size, Type = type };
                    Expect("Expected the array body, empty (`{}`) if has to be instanced with type default values;", TokenKind.OpenBrace);
                    if (!Match(TokenKind.CloseBrace))
                    {
                        do
                        {
                            array.AddArrayElement(ExpectExpression(true, TokenKind.Comma, TokenKind.CloseBrace));
                            _currentIndex--;
                        }
                        while (MatchAdvance(TokenKind.Comma));
                    }
                    Expect("", TokenKind.CloseBrace);
                    _currentIndex++;
                    return array;
                }
                var name = ExpectType();
                var allocation = new TypeAllocationNode() { Name = name , Position = token.Position };
                Expect("Type allocation requires `{}`;", TokenKind.OpenBrace);
                if (Match(TokenKind.Identifier))
                    do
                        allocation.AddFieldAssign(ExpectFieldAssign());
                    while (MatchAdvance(TokenKind.Comma));
                Expect("", TokenKind.CloseBrace);
                _currentIndex++;
                return allocation;
            }
            INode e = null;
            if (MatchFactor(out INode left))
            {
                if (!MatchAdvance(TokenKind.Plus) &&
                    !MatchAdvance(TokenKind.Minus))
                    e = left;
                else
                {
                    var op = Back.Kind;
                    e = left;
                    var right = ExpectFactor();
                    do
                    {
                        e = new ExpressionNode() { Operator = ToOperatorKind(op), Left = e, Right = right, Position = new(left.Position.Start, right.Position.End) };
                        if (MatchAdvance(TokenKind.Plus) ||
                            MatchAdvance(TokenKind.Minus))
                            op = Back.Kind;
                        else
                            break;
                    } while (MatchFactor(out right));
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
                _currentIndex--;
                if (MatchBooleanOperator(out _))
                {
                    _currentIndex--;
                    ParseError("Double boolean operator not allowed, to compare two boolean expressions please put two operand into `()`;");
                }
                _currentIndex++;
                return e;
            }
            ExpectMultipleMute("`"+Current.Kind+"` is not a valid token in the current context;", end);
            return e;
        }
        bool VariableDefinition(out INode statement)
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
        bool ConstantDefinition(out INode statement)
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
        bool ReturnDeclaration(out INode statement)
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
        bool MatchAssigmentOperators()
        {
            return
                MatchAdvance(TokenKind.Equal) ||
                MatchAdvance(TokenKind.AddAssignment) ||
                MatchAdvance(TokenKind.SubAssignment) ||
                MatchAdvance(TokenKind.MulAssignment) ||
                MatchAdvance(TokenKind.OperatorIncrement) ||
                MatchAdvance(TokenKind.OperatorDecrement) ||
                MatchAdvance(TokenKind.DivAssignment);
        }
        bool ValueAssignment(out INode statement)
        {
            statement = null;
            if (!MatchIdentifier(out var name, out var count))
            {
                _currentIndex -= count;
                return false;
            }
            if (!MatchAssigmentOperators())
            {
                _currentIndex-=count;
                return false;
            }
            var op = Back.Kind;
            var pos = Back.Position;
            statement = new AssignmentStatement() { Operator = op, Position = pos, Name = name };
            if (op == TokenKind.OperatorIncrement || op == TokenKind.OperatorDecrement)
            {
                _currentIndex++;
                return true;
            }
            var body = ExpectExpression(true, TokenKind.Semicolon);
            (statement as AssignmentStatement).Body = body;
            return true;
        }
        bool ConditionDefinition(out INode statement)
        {
            statement = null;
            if (!MatchAdvance(TokenKind.KeyIf, out Token key) &&
                !MatchAdvance(TokenKind.KeyElif, out key) &&
                !MatchAdvance(TokenKind.KeyWhile, out key))
                return false;
            var expression = ExpectExpression(true, TokenKind.OpenBrace);
            _currentIndex--;
            var body = ExpectBlock();
            statement = new ConditionalStatement() { Position = key.Position, Expression = expression, Kind = key.Kind, Body = body };
            return true;
        }
        bool ForLoopDefinition(out INode statement)
        {
            statement = null;
            if (!MatchAdvance(TokenKind.KeyFor, out Token key))
                return false;
            var name = Expect("", TokenKind.Identifier);
            INode counter = new ForCounterReference() { Position = name.Position, ReferenceName = name };
            var pos = name.Position;
            if (MatchAdvance(TokenKind.Colon))
            {
                INode varBody = null;
                var type = ExpectType();
                if (MatchAdvance(TokenKind.Equal))
                    varBody = ExpectFactor();
                counter = new VariableStatement() { Position = pos, Body = varBody, IsAssigned = varBody != null, Name = name.Value.ToString(), Type = type };
            }
            else if (MatchAdvance(TokenKind.Equal))
                counter = new AssignmentStatement() { Operator = TokenKind.Equal, Position = pos, Body = ExpectFactor(), Name = name };
            var op = ExpectMultiple("Expected an operator for the for statement, allowed: `in`, `to`;", TokenKind.KeyIn, TokenKind.KeyTo).Kind;
            var expression = ExpectFactor();
            var body = ExpectBlock();
            statement = new ForLoopStatement() { Counter = counter, Position = key.Position, RightExpression = expression, Operator = op, Body = body };
            return true;
        }
        bool OtherwiseConditionDefinition(out INode statement)
        {
            statement = null;
            if (!MatchAdvance(TokenKind.KeyElse))
                return false;
            var pos = Back.Position;
            var body = ExpectBlock();
            statement = new ConditionalStatement() { Position = pos, Kind = TokenKind.KeyElse, Body = body };
            return true;
        }
        bool LoopManagerDefintion(out INode statement)
        {
            statement = null;
            if (!MatchAdvance(TokenKind.KeyContinue) &&
                !MatchAdvance(TokenKind.KeyBreak))
                return false;
            statement = new LoopManagmentStatement() { Managment = Back };
            Expect("", TokenKind.Semicolon);
            return true;
        }
        INode ExpectStatement()
        {
            INode statement;
            if (!VariableDefinition(out statement))
                if (!ReturnDeclaration(out statement))
                    if (!ConstantDefinition(out statement))
                        if (!ConditionDefinition(out statement))
                            if (!ForLoopDefinition(out statement))
                                if (!OtherwiseConditionDefinition(out statement))
                                    if (!LoopManagerDefintion(out statement))
                                        if (MatchCallStatement(out statement))
                                            _currentIndex++;
                                        else
                                        {
                                            if (!ValueAssignment(out statement))
                                                ParseError("In the current local context, this is not a valid imperative statement;");
                                        }
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
        bool FunctionDefinition(out INode node)
        {
            node = null;
            if (!MatchAdvance(TokenKind.KeyFunc))
                return false;
            List<Token> generics = new();
            var name = Expect("In function definition must specify the name;", TokenKind.Identifier);
            if (MatchAdvance(TokenKind.OpenBracket))
            {
                if (Match(TokenKind.CloseBracket))
                    ParseError("Invalid generic definition content;");
                var count = 0;
                while (!MatchAdvance(TokenKind.CloseBracket))
                {
                    generics.Add(ExpectGenericType(count == 0));
                    count++;
                }
            }
            var parameters = ExpectParameterListDeclaration();
            INode type;
            if (Match(TokenKind.OpenBrace))
                type = new Token(name.LineAt, TokenKind.KeyTVoid, null, name.Position);
            else
            {
                Expect("In function definition must specify the type, or if it returns void the type can by omitted;", TokenKind.Colon);
                type = ExpectType();
            }
            var body = ExpectBlock();
            var f = new FunctionNode() { Body = body, Modifier = Modifier.Public, Name = name.Value.ToString(), ParameterList = parameters, Type = type, Position = name.Position };
            f.SetGenericTypes(generics);
            node = f;
            return true;
        }
        bool NamespaceDefinition(out INode node)
        {
            node = null;
            if (!MatchIdentifier(out var name))
                return false;
            Expect("", TokenKind.OpenBrace);
            var body = ExpectNamespaceMembers(TokenKind.CloseBrace);
            Expect("", TokenKind.CloseBrace);
            node = new NamespaceNode() { Members = body, Position = name.Position, Name = name };
            return true;
        }
        INode ExpectPath()
        {
            INode path = Expect("", TokenKind.Identifier);
            while (MatchAdvance(TokenKind.Dot))
            {
                var member = ExpectMultiple("Expected member, after `.`", TokenKind.Identifier, TokenKind.Star);
                path = new MemberNode() { Position = Back.Position, Base = path, Member = member };
                if (member.Kind == TokenKind.Star)
                    break;
            }
            return path;
        }
        bool MatchImportDirective(out INode directive)
        {
            directive = null;
            if (!MatchAdvance(TokenKind.KeyImport, out var token))
                return false;
            INode body;
            ImportMode mode = ImportMode.FromPackages;
            if (MatchAdvance(TokenKind.ConstantString))
            {
                body = Back;
                mode = ImportMode.FromLocal;
            }
            else
                body = ExpectPath();
            Expect("Expected `;`, at the end of an import directive;", TokenKind.Semicolon);
            directive = new ImportDirective() { Mode = mode, Member = body, Position = token.Position };
            return true;
        }
        bool MatchUseDirective(out INode directive)
        {
            directive = null;
            UseMode mode = UseMode.UsingNamespace;
            if (!MatchAdvance(TokenKind.KeyUse, out var token))
                return false;
            INode body = ExpectPath();
            Token alias = new();
            if (MatchAdvance(TokenKind.KeyAs))
            {
                alias = Expect("Expected the use path alias, after `as` in a use directive;", TokenKind.Identifier);
                mode = UseMode.UsingAlias;
            }
            Expect("Expected `;`, at the end of a use directive;", TokenKind.Semicolon);
            directive = new UseDirective() { Mode = mode, Body = body, Alias = alias, Position = token.Position };
            return true;
        }
        bool DirectiveDefinition(out INode directive)
        {
            directive = null;
            if (!MatchImportDirective(out var dir))
                if (!MatchUseDirective(out dir))
                    return false;
            directive = dir;
            return true;
        }
        Token ExpectGenericType(bool isFirst = true)
        {
            if (!isFirst)
                Expect("Generic types must be separated by a `,`;", TokenKind.Comma);
            if (!MatchAdvance(TokenKind.KeyType))
                ParseError("`", Current.Kind.ToString(), "` invalid token in generic type definition;");
            return Expect("In generic type definition, expected ident after `type` keyword;", TokenKind.Identifier);
        }
        Modifier FindModifiers()
        {
            var modifier = Modifier.Private;
            if (MatchAdvance(TokenKind.KeyPub))
                modifier = Modifier.Public;
            return modifier;
        }
        FieldNode ExpectFieldDefinition()
        {
            var modifier = FindModifiers();
            var name = Expect("Expected field name;", TokenKind.Identifier);
            Expect("Expected `:` after field name, then the field type;", TokenKind.Colon);
            var type = ExpectType();
            return new FieldNode() { Name = name.Value.ToString(), Type = type, Modifier = modifier };
        }
        bool TypeDefinition(out INode node)
        {
            node = null;
            if (!MatchAdvance(TokenKind.KeyType))
                return false;
            var name = Expect("Expected the type name after `type` keyword;", TokenKind.Identifier);
            var statement = new TypeStatement() { Name = name.Value.ToString(), Position = name.Position };
            var count = 0;
            if (MatchAdvance(TokenKind.OpenBracket))
            {
                if (Match(TokenKind.CloseBracket))
                    ParseError("Invalid generic definition content;");
                while (!MatchAdvance(TokenKind.CloseBracket))
                {
                    statement.AddGenericType(ExpectGenericType(count == 0));
                    count++;
                }
            }
            Expect("", TokenKind.OpenBrace);
            statement.AddField(ExpectFieldDefinition());
            while (MatchAdvance(TokenKind.Comma))
                statement.AddField(ExpectFieldDefinition());
            Expect("", TokenKind.CloseBrace);
            node = statement;
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
                if (!FunctionDefinition(out INode statement))
                    if (!VariableDefinition(out statement))
                        if (!TypeDefinition(out statement))
                            if (!DirectiveDefinition(out statement))
                                if (!NamespaceDefinition(out statement))
                                    ParseError("In the current global context, this is not a valid global statement;");
                nodes.Add(statement);
            }
            return nodes;
        }
        public NamespaceNode Parse()
        {
            var firstToken = Lexer.TokenCollection[_currentIndex];
            Module.Name = new Token(0, TokenKind.Identifier, Lexer.ModuleName, new(0, Lexer.Source.Length-1));
            if (firstToken.Kind == TokenKind.EOF)
                return Module;
            Module.Members = ExpectNamespaceMembers();
            return Module;
        }
        public List<Token> GetTokenCollection() => Lexer.Tokenize();
        public List<Token> GetTokenCollection(out MugLexer lexer) { lexer = Lexer; return Lexer.Tokenize(); }
    }
}