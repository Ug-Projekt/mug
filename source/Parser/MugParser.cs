using Mug.Compilation;
using Mug.Models.Lexer;
using Mug.Models.Parser.NodeKinds;
using Mug.Models.Parser.NodeKinds.Directives;
using Mug.Models.Parser.NodeKinds.Statements;
using Mug.TypeSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Mug.Models.Parser
{
    public class MugParser
    {
        public readonly NamespaceNode Module = new();
        public readonly MugLexer Lexer;
        private int _currentIndex = 0;
        private Pragmas _pragmas;
        private TokenKind _modifier = TokenKind.Bad;
        private INode _leftvalue;

        private void ParseError(params string[] error)
        {
            if (Match(TokenKind.EOF))
                ParseErrorEOF();

            Lexer.Throw(Current, error);
        }

        private void ParseErrorEOF(params string[] error)
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
        }

        private Token Current
        {
            get
            {
                if (_currentIndex >= Lexer.TokenCollection.Count)
                    ParseErrorEOF("");

                return Lexer.TokenCollection[_currentIndex];
            }
        }

        private Token Back
        {
            get
            {
                if (_currentIndex - 1 >= 0)
                    return Lexer.TokenCollection[_currentIndex - 1];

                return new Token();
            }
        }

        private Token ExpectMultiple(string error, params TokenKind[] kinds)
        {
            for (int i = 0; i < kinds.Length; i++)
                if (Current.Kind == kinds[i])
                {
                    _currentIndex++;
                    return Back;
                }

            if (error == "")
                ParseError("Expected `", matchString(), "`, found `", Current.Value, "`");
            else
                ParseError(error);

            throw new(); // unreachable

            string matchString()
            {
                StringBuilder result = new();

                for (int i = 0; i < kinds.Length; i++)
                {
                    result.Append("");
                    if (i < kinds.Length - 1)
                        result.Append("`, `");
                }

                return result.ToString();
            }
        }

        private Token Expect(string error, TokenKind kind)
        {
            if (Current.Kind != kind)
            {
                if (error == "")
                    ParseError("Expected `", kind.GetDescription(), "`, found `", Current.Value, "`");
                else
                    ParseError(error);
            }

            _currentIndex++;
            return Back;
        }

        private bool Match(TokenKind kind)
        {
            return Current.Kind == kind;
        }

        private bool MatchAdvance(TokenKind kind)
        {
            var expect = Match(kind);

            if (expect)
                _currentIndex++;

            return expect;
        }

        private MugType ExpectType(bool expectKeyTypeInGeneric = false)
        {
            if (MatchAdvance(TokenKind.OpenBracket))
            {
                var type = ExpectType();
                Expect("An array type definition must end by `]`", TokenKind.CloseBracket);
                return new MugType(TypeKind.Array, type);
            }
            else if (MatchAdvance(TokenKind.KeyTPtr))
                return new MugType(TypeKind.Pointer, ExpectType());

            MugType find;
            find = ExpectBaseType();

            // removed temporanely generics
            /*if (MatchAdvance(TokenKind.OpenBracket))
            {
                if (expectKeyTypeInGeneric)
                    Expect("", TokenKind.KeyType);
 
                var type = ExpectType();
                find = new MugType(TypeKind.GenericStruct, new GenericType(find, type));

                Expect("Generic type specification must be wrote between `[]`;", TokenKind.CloseBracket);
            }*/

            return find;
        }

        private bool MatchAdvance(TokenKind kind, out Token token)
        {
            token = new();

            if (MatchAdvance(kind))
            {
                token = Back;
                return true;
            }
            return false;
        }

        private Token ExpectConstantMute(params string[] error)
        {
            var match = MatchConstantAdvance();

            if (match)
                return Back;

            ParseError(error);

            return new();
        }

        private ParameterNode ExpectParameter(bool isFirst)
        {
            if (!isFirst)
                Expect("Parameters must be separated by a comma", TokenKind.Comma);

            var name = Expect("Expected parameter name", TokenKind.Identifier);

            Expect("Expected parameter type", TokenKind.Colon);

            var type = ExpectType();
            var defaultvalue = new Token();

            if (MatchAdvance(TokenKind.Equal))
                defaultvalue = ExpectConstantMute("Expected constant expression as default parameter value");

            ExpectMultiple("", TokenKind.Comma, TokenKind.ClosePar);
            _currentIndex--;

            return new ParameterNode(type, name.Value, defaultvalue, name.Position);
        }

        private OperatorKind ToOperatorKind(TokenKind op)
        {
            return op switch
            {
                TokenKind.Plus => OperatorKind.Sum,
                TokenKind.Minus => OperatorKind.Subtract,
                TokenKind.Star => OperatorKind.Multiply,
                TokenKind.Slash => OperatorKind.Divide,
                TokenKind.RangeDots => OperatorKind.Range,
                TokenKind.BooleanEQ => OperatorKind.CompareEQ,
                TokenKind.BooleanNEQ => OperatorKind.CompareNEQ,
                TokenKind.BooleanMajor => OperatorKind.CompareMajor,
                TokenKind.BooleanMajEQ => OperatorKind.CompareMajorEQ,
                TokenKind.BooleanMinor => OperatorKind.CompareMinor,
                TokenKind.BooleanMinEQ => OperatorKind.CompareMinorEQ,
                TokenKind.BooleanAND => OperatorKind.And,
                TokenKind.BooleanOR => OperatorKind.Or,
                _ => throw new Exception($"Unable to perform cast from TokenKind(`{op}`) to OperatorKind, if you see this error please open an issue on github")
            };
        }

        private MugType ExpectBaseType()
        {
            var match = MatchPrimitiveType(out var type) || MatchAdvance(TokenKind.Identifier, out type);

            if (!match)
                ParseError("Expected a type, but found `" + Current.Value + "`");

            return MugType.FromToken(type);
        }

        private bool MatchPrimitiveType(out Token type)
        {
            return
                MatchAdvance(TokenKind.KeyTi32, out type) ||
                MatchAdvance(TokenKind.KeyTVoid, out type) ||
                MatchAdvance(TokenKind.KeyTbool, out type) ||
                MatchAdvance(TokenKind.KeyTchr, out type) ||
                MatchAdvance(TokenKind.KeyTi64, out type) ||
                MatchAdvance(TokenKind.KeyTu32, out type) ||
                MatchAdvance(TokenKind.KeyTu8, out type) ||
                MatchAdvance(TokenKind.KeyTu64, out type) ||
                MatchAdvance(TokenKind.KeyTstr, out type) ||
                MatchAdvance(TokenKind.KeyTunknown, out type);
        }

        private bool MatchValue()
        {
            return
                MatchAdvance(TokenKind.Identifier) ||
                MatchConstantAdvance();
        }

        private bool MatchConstantAdvance()
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

        private bool MatchInParExpression(out INode e)
        {
            e = null;

            if (!MatchAdvance(TokenKind.OpenPar))
                return false;

            e = ExpectExpression(true, TokenKind.ClosePar);

            return true;
        }

        private void CollectPossibleArrayAccessNode(ref INode e)
        {
            while (MatchAdvance(TokenKind.OpenBracket, out var token))
            {
                e = new ArraySelectElemNode()
                {
                    IndexExpression = ExpectExpression(true, TokenKind.CloseBracket),
                    Left = e,
                    Position = (e.Position.Start)..(token.Position.End)
                };
            }
        }

        private bool MatchMember(out INode name)
        {
            name = null;

            if (!MatchValue())
                return false;

            name = Back;

            CollectPossibleArrayAccessNode(ref name);

            while (MatchAdvance(TokenKind.Dot))
            {
                var id = Expect("expected member after `.`", TokenKind.Identifier);

                name = new MemberNode() { Base = name, Member = id, Position = (name.Position.Start)..(id.Position.End) };

                CollectPossibleArrayAccessNode(ref name);
            }

            return true;
        }

        private void CollectParameters(ref NodeBuilder parameters)
        {
            while (!MatchAdvance(TokenKind.ClosePar))
            {
                parameters.Add(ExpectExpression(true, TokenKind.Comma, TokenKind.ClosePar));

                if (Back.Kind == TokenKind.ClosePar)
                    _currentIndex--;
            }
        }

        private bool MatchCallStatement(out INode e, bool isImperativeStatement, INode previousMember = null)
        {
            e = null;
            INode name = null;

            if (previousMember is null)
            {
                if (!MatchTerm(out _leftvalue, false))
                    ParseError("Unable to find a valid local statement");
            }
            else
                name = previousMember;

            if (!MatchAdvance(TokenKind.OpenPar))
                return false;

            if (previousMember is null)
                name = GetLeftValue();

            var parameters = new NodeBuilder();

            if (name is MemberNode instanceAccesses)
            {
                parameters.Add(instanceAccesses.Base);

                name = instanceAccesses.Member;
            }

            CollectParameters(ref parameters);

            e = new CallStatement() { Name = name, Parameters = parameters, Position = previousMember is null ? name.Position : previousMember.Position };

            while (MatchAdvance(TokenKind.Dot))
            {
                name = Expect("Expected member after `.`", TokenKind.Identifier);

                if (MatchAdvance(TokenKind.OpenPar))
                {
                    parameters = new NodeBuilder();

                    CollectParameters(ref parameters);

                    parameters.Insert(0, e);

                    e = new CallStatement() { Name = name, Parameters = parameters };
                }
                else
                {
                    e = new MemberNode() { Base = e, Member = (Token)name, Position = e.Position.Start..name.Position.End };
                }
            }
            if (isImperativeStatement)
                Expect("", TokenKind.Semicolon);

            return true;
        }

        private bool MatchTerm(out INode e, bool allowCallStatement = true)
        {
            if (MatchAdvance(TokenKind.Minus) ||
                MatchAdvance(TokenKind.Plus) ||
                MatchAdvance(TokenKind.Negation))
            {
                var prefixOp = Back;

                if (!MatchTerm(out e, allowCallStatement))
                {
                    _currentIndex--;
                    ParseError("Unexpected prefix operator");
                }

                e = new PrefixOperator() { Expression = e, Position = prefixOp.Position, Prefix = prefixOp.Kind };

                return true;
            }

            // arr[]
            // base.member
            // base.member()
            // base()

            if (!MatchInParExpression(out e))
            {
                if (!MatchMember(out e))
                {
                    _currentIndex--;
                    return false;
                }

                if (allowCallStatement && MatchCallStatement(out var call, false, e))
                {
                    e = call;
                }
            }

            while (MatchAdvance(TokenKind.Dot))
            {
                if (allowCallStatement && MatchCallStatement(out var call, false))
                {
                    (call as CallStatement).Parameters.Insert(0, e);
                    e = call;
                }

                if (MatchAdvance(TokenKind.Identifier, out var token))
                    e = new MemberNode() { Base = e, Member = token, Position = (e.Position.Start)..(token.Position.End) };
            }

            CollectPossibleArrayAccessNode(ref e);

            return true;
        }

        private INode ExpectFactor()
        {
            if (!MatchFactor(out INode e) &&
                !MatchInParExpression(out e))
                ParseError("Expected expression factor here");

            return e;
        }

        private bool MatchFactorOps()
        {
            return
                MatchAdvance(TokenKind.Star) ||
                MatchAdvance(TokenKind.Slash) ||
                MatchAdvance(TokenKind.RangeDots);
        }

        private FieldAssignmentNode ExpectFieldAssign()
        {
            if (!MatchAdvance(TokenKind.Identifier))
                ParseError("Expected field assignment");

            var name = Back;
            Expect($"When assigning a field, required `:`, not `{Current.Value}`", TokenKind.Colon);

            var expression = ExpectExpression(true, TokenKind.Comma, TokenKind.CloseBrace);
            _currentIndex--;

            return new FieldAssignmentNode() { Name = name.Value.ToString(), Body = expression, Position = name.Position };
        }

        private INode ExpectTerm(bool allowCallStatement = true)
        {
            if (!MatchTerm(out var e, allowCallStatement))
                ParseError("Expected term");

            return e;
        }

        private bool MatchFactor(out INode e)
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

        private bool MatchBooleanOperator(out Token op)
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

        private INode CollectTernary(Range ifposition)
        {
            var expression = ExpectExpression(true, TokenKind.KeyTVoid);
            var ifBody = ExpectFactor();

            Expect("In inline conditions there must be the else body: place it here", TokenKind.KeyElse);

            var elseBody = ExpectFactor();
            _currentIndex++;

            return new InlineConditionalExpression() { Expression = expression, IFBody = ifBody, ElseBody = elseBody, Position = ifposition };
        }

        private INode CollectNodeNew(Range newposition)
        {
            if (MatchAdvance(TokenKind.OpenBracket))
            {
                var type = ExpectType();
                Expect("Expected array size after its type", TokenKind.Comma);

                var size = ExpectExpression(true, TokenKind.CloseBracket);
                _currentIndex--;

                Expect("Expected `]` and the array body", TokenKind.CloseBracket);

                var array = new ArrayAllocationNode() { Size = size, Type = type };
                Expect("Expected the array body, empty (`{}`) if has to be instanced with type default values", TokenKind.OpenBrace);

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
            var allocation = new TypeAllocationNode() { Name = name, Position = newposition };

            Expect("Type allocation requires `{}`", TokenKind.OpenBrace);

            if (Match(TokenKind.Identifier))
                do
                    allocation.AddFieldAssign(ExpectFieldAssign());
                while (MatchAdvance(TokenKind.Comma));

            Expect("", TokenKind.CloseBrace);
            _currentIndex++;

            return allocation;
        }

        private INode CollectBooleanExpression(ref INode e, Token boolOP, bool isFirst, params TokenKind[] end)
        {
            if (!isFirst)
                return e;

            var right = ExpectExpression(false, end);
            e = new BooleanExpressionNode() { Operator = ToOperatorKind(boolOP.Kind), Position = boolOP.Position, Left = e, Right = right };
            _currentIndex--;

            if (MatchBooleanOperator(out _))
            {
                _currentIndex--;
                ParseError("Double boolean operator not allowed, to compare two boolean expressions please put two operands into `()`");
            }

            _currentIndex++;
            return e;
        }

        private INode ExpectExpression(bool isFirst, params TokenKind[] end)
        {
            if (MatchAdvance(TokenKind.KeyIf, out var token))
                return CollectTernary(token.Position);

            if (MatchAdvance(TokenKind.KeyNew, out token))
                return CollectNodeNew(token.Position);

            if (MatchFactor(out INode e))
            {
                if (MatchAdvance(TokenKind.Plus) ||
                    MatchAdvance(TokenKind.Minus))
                {
                    var op = Back.Kind;
                    var right = ExpectFactor();

                    do
                    {
                        e = new ExpressionNode() { Operator = ToOperatorKind(op), Left = e, Right = right, Position = new(e.Position.Start, right.Position.End) };
                        if (MatchAdvance(TokenKind.Plus) ||
                            MatchAdvance(TokenKind.Minus))
                            op = Back.Kind;
                        else
                            break;
                    } while (MatchFactor(out right));
                }
            }

            if (e is null)
                ParseError("Expected expression, found `", Current.Value.ToString(), "`");

            if (MatchBooleanOperator(out var boolOP))
                return CollectBooleanExpression(ref e, boolOP, isFirst, end);
            else if (MatchAdvance(TokenKind.KeyAs, out var asToken))
                e = new CastExpressionNode() { Expression = e, Type = ExpectType(), Position = asToken.Position };

            ExpectMultiple("Invalid token in the current context, maybe `;` missing?", end);

            return e;
        }

        private MugType ExpectVariableType()
        {
            return MatchAdvance(TokenKind.Colon) ? ExpectType() : MugType.Automatic();
        }

        private bool VariableDefinition(out INode statement)
        {
            statement = null;

            if (!MatchAdvance(TokenKind.KeyVar))
                return false;

            var name = Expect("Expected the variable name", TokenKind.Identifier);
            var type = ExpectVariableType();

            if (MatchAdvance(TokenKind.Semicolon))
            {
                statement = new VariableStatement() { Body = null, IsAssigned = false, Name = name.Value.ToString(), Position = name.Position, Type = type };
                return true;
            }

            Expect("To define the value of a variable must open the body with `=`, or you can only declare a variable putting after type spec the symbol `;`", TokenKind.Equal);

            var body = ExpectExpression(true, TokenKind.Semicolon);
            statement = new VariableStatement() { Body = body, IsAssigned = true, Name = name.Value.ToString(), Position = name.Position, Type = type };

            return true;
        }

        private bool ConstantDefinition(out INode statement)
        {
            statement = null;

            if (!MatchAdvance(TokenKind.KeyConst))
                return false;

            var name = Expect("Expected the constant name", TokenKind.Identifier);
            var type = ExpectVariableType();

            if (Match(TokenKind.Semicolon))
                ParseError("A constant cannot be declared without a body");

            Expect("To define the value of a constant must open the body with `=`", TokenKind.Equal);

            var body = ExpectExpression(true, TokenKind.Semicolon);
            statement = new ConstantStatement() { Body = body, Name = name.Value.ToString(), Position = name.Position, Type = type };

            return true;
        }

        private bool ReturnDeclaration(out INode statement)
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

        private bool MatchAssigmentOperators()
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

        private bool ValueAssignment(out INode statement)
        {
            statement = null;

            var name = GetLeftValue();

            if (!MatchAssigmentOperators())
            {
                _currentIndex--;
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

        private bool ConditionDefinition(out INode statement, bool isFirstCondition = true)
        {
            statement = null;

            if (!MatchAdvance(TokenKind.KeyIf, out Token key) &&
                !MatchAdvance(TokenKind.KeyElif, out key) &&
                !MatchAdvance(TokenKind.KeyElse, out key) &&
                !MatchAdvance(TokenKind.KeyWhile, out key))
                return false;

            if (isFirstCondition && key.Kind != TokenKind.KeyIf && key.Kind != TokenKind.KeyWhile)
            {
                _currentIndex--;
                ParseError("The 'elif' and 'else' conditions shall be referenced to an 'if' block");
            }

            INode expression = null;
            if (key.Kind != TokenKind.KeyElse)
            {
                expression = ExpectExpression(true, TokenKind.OpenBrace);
                _currentIndex--;
            }

            var body = ExpectBlock();

            INode elif = null;
            if (key.Kind != TokenKind.KeyWhile && key.Kind != TokenKind.KeyElse && (Match(TokenKind.KeyElif) || Match(TokenKind.KeyElse)))
                ConditionDefinition(out elif, false);

            statement = new ConditionalStatement() { Position = key.Position, Expression = expression, Kind = key.Kind, Body = body, ElseNode = (ConditionalStatement)elif };

            return true;
        }

        private bool ForLoopDefinition(out INode statement)
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

            var op = ExpectMultiple("Expected `in`, `to` in for statement", TokenKind.KeyIn, TokenKind.KeyTo).Kind;
            var expression = ExpectFactor();
            var body = ExpectBlock();

            statement = new ForLoopStatement() { Counter = counter, Position = key.Position, RightExpression = expression, Operator = op, Body = body };

            return true;
        }

        private bool LoopManagerDefintion(out INode statement)
        {
            statement = null;

            if (!MatchAdvance(TokenKind.KeyContinue) &&
                !MatchAdvance(TokenKind.KeyBreak))
                return false;

            statement = new LoopManagementStatement() { Management = Back, Position = Back.Position };

            Expect("", TokenKind.Semicolon);

            return true;
        }

        private INode ExpectStatement()
        {
            if (!VariableDefinition(out var statement)) // var x = value;
                if (!ReturnDeclaration(out statement)) // return value;
                    if (!ConstantDefinition(out statement)) // const x = value;
                        if (!ConditionDefinition(out statement)) // if condition {}, elif
                            if (!ForLoopDefinition(out statement)) // for x: type to, in value {}
                                if (!LoopManagerDefintion(out statement)) // continue, break
                                    if (!MatchCallStatement(out statement, true)) // f();
                                        if (!ValueAssignment(out statement)) // x = value;
                                            ParseError("Invalid token here");

            return statement;
        }

        private BlockNode ExpectBlock()
        {
            Expect("A block statement must start by `{` token", TokenKind.OpenBrace);

            var block = new BlockNode();

            while (!Match(TokenKind.CloseBrace))
                block.Add(ExpectStatement());

            Expect("A block statement must end with `}` token", TokenKind.CloseBrace);

            return block;
        }

        private ParameterListNode ExpectParameterListDeclaration()
        {
            Expect("", TokenKind.OpenPar);

            var parameters = new ParameterListNode();
            var count = 0;

            while (!MatchAdvance(TokenKind.ClosePar))
            {
                parameters.Add(ExpectParameter(count == 0));
                count++;
            }

            return parameters;
        }

        private TokenKind GetModifier()
        {
            if (_modifier == TokenKind.Bad)
                return TokenKind.KeyPriv;

            var old = _modifier;

            _modifier = TokenKind.Bad;

            return old; // pub only currently
        }

        private Pragmas GetPramas()
        {
            var result = _pragmas;
            _pragmas = null;
            return result is not null ? result : new();
        }

        private INode GetLeftValue()
        {
            var result = _leftvalue;
            _leftvalue = null;

            return result;
        }

        private bool FunctionDefinition(out INode node)
        {
            node = null;

            if (!MatchAdvance(TokenKind.KeyFunc)) // <func>
                return false;

            var modifier = GetModifier();
            var pragmas = GetPramas();
            var generics = new List<Token>();
            var name = Expect("In function definition must specify the name", TokenKind.Identifier); // func <name>

            if (MatchAdvance(TokenKind.OpenBracket)) // func name<[>
            {
                if (Match(TokenKind.CloseBracket))
                    ParseError("Invalid generic definition content");

                var count = 0;

                while (!MatchAdvance(TokenKind.CloseBracket))
                {
                    generics.Add(ExpectGenericType(count == 0));
                    count++;
                }
            }

            var parameters = ExpectParameterListDeclaration(); // func name<(..)>

            MugType type;

            if (MatchAdvance(TokenKind.Colon))
                type = ExpectType();
            else
                type = new MugType(TypeKind.Void);

            if (Match(TokenKind.OpenBrace)) // function definition
            {
                var body = ExpectBlock();

                var f = new FunctionNode() { Modifier = modifier, Pragmas = pragmas, Body = body, Name = name.Value.ToString(), ParameterList = parameters, Type = type, Position = name.Position };

                f.SetGenericTypes(generics);

                node = f;
            }
            else // prototype
            {
                Expect("", TokenKind.Semicolon);

                var f = new FunctionPrototypeNode() { Modifier = modifier, Pragmas = pragmas, Name = name.Value.ToString(), ParameterList = parameters, Type = type, Position = name.Position };
                f.SetGenericTypes(generics);

                node = f;
            }

            return true;
        }

        private bool MatchImportDirective(out INode directive)
        {
            directive = null;

            if (!MatchAdvance(TokenKind.KeyImport, out var token)) // <import>
                return false;

            INode body;

            ImportMode mode = ImportMode.FromPackages;

            if (MatchAdvance(TokenKind.ConstantString)) // import <"path">
            {
                body = Back;
                mode = ImportMode.FromLocal;
            }
            else // import <path>
                body = Expect("", TokenKind.Identifier);

            Expect("Expected `;`, at the end of an import directive", TokenKind.Semicolon); // import .. <;>

            directive = new ImportDirective() { Mode = mode, Member = body, Position = token.Position };
            return true;
        }

        private bool MatchUseDirective(out INode directive)
        {
            directive = null;

            if (!MatchAdvance(TokenKind.KeyUse, out var token)) // <use>
                return false;

            var body = Expect("", TokenKind.Identifier); // use <path>

            Expect("Allowed only alias declaration with use directive", TokenKind.KeyAs); // use path <as>

            var alias = Expect("Expected the use path alias, after `as` in a use directive", TokenKind.Identifier); // use path as <alias>

            Expect("Expected `;`, at the end of a use directive", TokenKind.Semicolon); // use path alias <;>

            directive = new UseDirective() { Body = body, Alias = alias, Position = token.Position };
            return true;
        }

        private bool DirectiveDefinition(out INode directive)
        {
            if (!MatchImportDirective(out directive))
                if (!MatchUseDirective(out directive))
                    return false;

            return true;
        }

        private Token ExpectGenericType(bool isFirst = true)
        {
            if (!isFirst)
                Expect("Generic types must be separated by a `,`", TokenKind.Comma);

            if (!MatchAdvance(TokenKind.KeyType))
                ParseError("`", Current.Value.ToString(), "` invalid token in generic type definition");

            return Expect("In generic type definition, expected ident after `type` keyword", TokenKind.Identifier);
        }

        private FieldNode ExpectFieldDefinition()
        {
            var name = Expect("Expected field name", TokenKind.Identifier); // <field>

            Expect("Expected type specification", TokenKind.Colon); // field <:>

            var type = ExpectType(); // field: <error>

            return new FieldNode() { Name = name.Value.ToString(), Type = type, Position = name.Position };
        }

        /// <summary>
        /// search for a struct definition
        /// </summary>
        private bool TypeDefinition(out INode node)
        {
            // mandatory for c# convention
            node = null;

            // returns if does not match a type keyword
            if (!MatchAdvance(TokenKind.KeyType))
                return false;

            // required an identifier
            var modifier = GetModifier();
            var pragmas = GetPramas();
            var name = Expect("Expected the type name after `type` keyword", TokenKind.Identifier);
            var statement = new TypeStatement() { Modifier = modifier, Pragmas = pragmas, Name = name.Value.ToString(), Position = name.Position };

            // struct generics
            if (MatchAdvance(TokenKind.OpenBracket))
            {
                var count = 0;

                if (Match(TokenKind.CloseBracket))
                    ParseError("Invalid generic definition content");

                while (!MatchAdvance(TokenKind.CloseBracket))
                {
                    statement.AddGenericType(ExpectGenericType(count == 0));
                    count++;
                }
            }

            // struct body
            Expect("", TokenKind.OpenBrace);

            statement.AddField(ExpectFieldDefinition());

            // trailing commas are not allowed
            while (MatchAdvance(TokenKind.Comma))
                statement.AddField(ExpectFieldDefinition());

            Expect("", TokenKind.CloseBrace); // expected close body

            node = statement;

            return true;
        }

        private void CollectPragmas()
        {
            if (!MatchAdvance(TokenKind.OpenPragmas))
                return;

            _pragmas = new();

            do
            {
                var name = Expect("", TokenKind.Identifier).Value;
                Expect("", TokenKind.OpenPar);
                var value = ExpectConstantMute("Non-constant expressions not allowed in pragmas");
                Expect("", TokenKind.ClosePar);

                _pragmas.SetPragma(name, value, ParseError, ref _currentIndex);
            } while (MatchAdvance(TokenKind.Comma));

            Expect("Expected pragmas close", TokenKind.CloseBracket);
        }

        private void CollectModifier()
        {
            if (MatchAdvance(TokenKind.KeyPub))
                _modifier = Back.Kind;
        }

        /// <summary>
        /// expects at least one member
        /// </summary>
        private NodeBuilder ExpectNamespaceMembers(TokenKind end = TokenKind.EOF)
        {
            NodeBuilder nodes = new();

            // while the current token is not end
            while (!Match(end))
            {
                // collecting pragmas for first two statements (function and type)
                CollectPragmas();
                CollectModifier();

                // searches for a global statement
                // func id() {}
                if (!FunctionDefinition(out INode statement))
                    // (c struct) type MyStruct {}
                    if (!TypeDefinition(out statement))
                    {
                        if (_pragmas is not null)
                            ParseError("Invalid pragmas for this member");

                        if (_modifier != TokenKind.Bad)
                            ParseError("Invalid modifier for this member");

                        // var id = constant;
                        if (!VariableDefinition(out statement))
                            // import "", import path, use x as y
                            if (!DirectiveDefinition(out statement))
                                // if there is not global statement
                                ParseError("Invalid token here");
                    }

                // adds the statement to the members
                nodes.Add(statement);
            }

            return nodes;
        }

        /// <summary>
        /// generates the ast from a tokens stream
        /// </summary>
        public NamespaceNode Parse()
        {
            var firstToken = Lexer.TokenCollection[_currentIndex];

            Module.Name = new Token(TokenKind.Identifier, Lexer.ModuleName, 0..(Lexer.Source.Length - 1));

            // to avoid bugs
            if (firstToken.Kind == TokenKind.EOF)
                return Module;

            // search for members
            Module.Members = ExpectNamespaceMembers();

            return Module;
        }
    }
}