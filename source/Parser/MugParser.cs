﻿using Mug.Compilation;
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

        private void ParseError(params string[] error)
        {
            if (Match(TokenKind.EOF))
                ParseErrorEOF();

            Lexer.Throw(Current, error);
        }

        private void ParseError(Range position, params string[] error)
        {
            if (Match(TokenKind.EOF))
                ParseErrorEOF();

            Lexer.Throw(position, error);
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

        private string TokenKindsToString(TokenKind[] kinds)
        {
            StringBuilder result = new();

            for (int i = 0; i < kinds.Length; i++)
            {
                result.Append(kinds[i].GetDescription());
                if (i < kinds.Length - 1)
                    result.Append("`, `");
            }

            return result.ToString();
        }

        private Token ExpectMultiple(string error, params TokenKind[] kinds)
        {
            if (kinds.Length == 0)
                return Token.NewInfo(TokenKind.Bad, null);

            for (int i = 0; i < kinds.Length; i++)
                if (Current.Kind == kinds[i])
                {
                    _currentIndex++;
                    return Back;
                }

            if (error == "")
                ParseError("Expected `", TokenKindsToString(kinds), "`, found `", Current.Value, "`");
            else
                ParseError(error);

            throw new(); // unreachable
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

        private MugType ExpectType(bool allowEnumError = false, bool allowReference = true)
        {
            if (allowReference && MatchAdvance(TokenKind.BooleanAND, out var token))
                return new MugType(token.Position, TypeKind.Reference, ExpectType(false, false));
            if (MatchAdvance(TokenKind.OpenBracket, out token))
            {
                var type = ExpectType();
                Expect("An array type definition must end by `]`", TokenKind.CloseBracket);
                return new MugType(token.Position.Start..Back.Position.End, TypeKind.Array, type);
            }
            else if (MatchAdvance(TokenKind.Star, out token))
            {
                var type = ExpectType();
                return new MugType(token.Position.Start..type.Position.End, TypeKind.Pointer, type);
            }

            var find = ExpectBaseType();

            // struct generics
            if (MatchAdvance(TokenKind.BooleanLess))
            {
                if (find.Kind != TypeKind.DefinedType)
                {
                    _currentIndex -= 2;
                    ParseError("Generic parameters cannot be passed to type `", find.ToString(), "`");
                }

                var genericTypes = new List<MugType>();

                do
                    genericTypes.Add(ExpectType());
                while (MatchAdvance(TokenKind.Comma));

                Expect("", TokenKind.BooleanGreater);

                find = new MugType(find.Position.Start..Back.Position.End, TypeKind.GenericDefinedType, (find, genericTypes));
            }

            if (allowEnumError && MatchAdvance(TokenKind.Negation))
                find = new MugType(Back.Position, TypeKind.EnumError, (find, ExpectType()));

            return find;
        }

        private bool MatchType(out MugType type)
        {
            type = null;
            MugType t = null;

            if (!Match(TokenKind.OpenBracket) && !Match(TokenKind.Star) && !MatchBaseType(out t))
                return false;

            if (t is not null)
                _currentIndex--;

            type = ExpectType();
            return true;
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

        private Token ExpectConstant(params string[] error)
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
                defaultvalue = ExpectConstant("Expected constant expression as default parameter value");

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
                TokenKind.BooleanGreater => OperatorKind.CompareGreater,
                TokenKind.BooleanGEQ => OperatorKind.CompareGEQ,
                TokenKind.BooleanLess => OperatorKind.CompareLess,
                TokenKind.BooleanLEQ => OperatorKind.CompareLEQ,
                TokenKind.BooleanAND => OperatorKind.And,
                TokenKind.BooleanOR => OperatorKind.Or,
                _ => throw new Exception($"Unable to perform cast from TokenKind(`{op}`) to OperatorKind, if you see this error please open an issue on github")
            };
        }

        private MugType ExpectBaseType()
        {
            if (!MatchBaseType(out var type))
                ParseError("Expected a type, but found `" + Current.Value + "`");

            return type;
        }

        private bool MatchBaseType(out MugType type)
        {
            type = null;
            
            if (!MatchPrimitiveType(out var token) && !MatchAdvance(TokenKind.Identifier, out token))
                return false;

            type = MugType.FromToken(token);

            return true;
        }

        private bool MatchPrimitiveType(out Token type)
        {
            return
                MatchAdvance(TokenKind.KeyTi32, out type) ||
                MatchAdvance(TokenKind.KeyTVoid, out type) ||
                MatchAdvance(TokenKind.KeyTbool, out type) ||
                MatchAdvance(TokenKind.KeyTchr, out type) ||
                MatchAdvance(TokenKind.KeyTf32, out type) ||
                MatchAdvance(TokenKind.KeyTf64, out type) ||
                MatchAdvance(TokenKind.KeyTf128, out type) ||
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

            if (!MatchAdvance(TokenKind.OpenPar, out var token))
                return false;

            e = ExpectExpression(true, end: TokenKind.ClosePar);

            return true;
        }

        private void CollectPossibleArrayAccessNode(ref INode e)
        {
            while (MatchAdvance(TokenKind.OpenBracket, out var token))
            {
                e = new ArraySelectElemNode()
                {
                    IndexExpression = ExpectExpression(end: TokenKind.CloseBracket),
                    Left = e,
                    Position = token.Position.Start..Back.Position.End
                };
            }
        }

        private bool MatchMember(out INode name)
        {
            name = null;

            if (MatchValue())
                name = Back;
            else if (!MatchInParExpression(out name))
                return false;


            CollectPossibleArrayAccessNode(ref name);

            while (MatchAdvance(TokenKind.Dot))
            {
                var id = Expect("expected member after `.`", TokenKind.Identifier);

                name = new MemberNode() { Base = name, Member = id, Position = name.Position.Start.Value..id.Position.End.Value };

                CollectPossibleArrayAccessNode(ref name);
            }

            return true;
        }

        private void CollectParameters(ref NodeBuilder parameters)
        {
            while (!MatchAdvance(TokenKind.ClosePar))
            {
                parameters.Add(ExpectExpression(end: new[] { TokenKind.Comma, TokenKind.ClosePar}));

                if (Back.Kind == TokenKind.ClosePar)
                    _currentIndex--;
            }
        }

        private List<MugType> CollectGenericParameters()
        {
            var oldindex = _currentIndex;

            if (MatchAdvance(TokenKind.BooleanLess))
            {
                if (MatchType(out var type))
                {
                    var generics = new List<MugType>() { type };

                    while (MatchAdvance(TokenKind.Comma))
                        generics.Add(ExpectType());

                    if (MatchAdvance(TokenKind.BooleanGreater))
                        return generics;
                }
            }

            _currentIndex = oldindex;
            return new List<MugType>();
        }

        private bool MatchCallStatement(out INode e, INode previousMember = null)
        {
            e = null;
            INode name = null;

            if (previousMember is null)
            {
                if (!MatchTerm(out name, false))
                    ParseError("Expressions not allowed as imperative statement");
            }
            else
                name = previousMember;

            var token = Current;

            if (!MatchAdvance(TokenKind.OpenPar) && !Match(TokenKind.BooleanLess))
                return false;

            var parameters = new NodeBuilder();

            /*if (name is MemberNode instanceAccesses)
            {
                // parameters.Add(instanceAccesses.Base);

                name = instanceAccesses*//*.Member*//*;
            }*/

            var generics = CollectGenericParameters();

            if (token.Kind == TokenKind.BooleanLess && !MatchAdvance(TokenKind.OpenPar))
            {
                if (generics.Count != 0)
                    ParseError("Expected call after generic parameter specification");

                return false;
            }

            CollectParameters(ref parameters);
            
            e = new CallStatement() { Generics = generics, Name = name, Parameters = parameters, Position = previousMember is null ? name.Position : previousMember.Position };

            while (MatchAdvance(TokenKind.Dot))
            {
                name = Expect("Expected member after `.`", TokenKind.Identifier);

                generics = CollectGenericParameters();

                if (MatchAdvance(TokenKind.OpenPar))
                {
                    parameters = new NodeBuilder();

                    CollectParameters(ref parameters);

                    // parameters.Insert(0, e);

                    e = new CallStatement() { Generics = generics, Position = name.Position, Name = new MemberNode() { Base = e, Member = (Token)name, Position = e.Position.Start..name.Position.End }, Parameters = parameters };
                }
                else
                {
                    if (generics.Count != 0)
                        ParseError("Expected call after generic parameter specification");

                    e = new MemberNode() { Base = e, Member = (Token)name, Position = e.Position.Start..name.Position.End };
                }
            }

            return true;
        }

        private bool MatchPrefixOperator(out Token prefix)
        {
            return
                MatchAdvance(TokenKind.Minus, out prefix)      ||
                MatchAdvance(TokenKind.Plus, out prefix)       ||
                MatchAdvance(TokenKind.Negation, out prefix)   ||
                MatchAdvance(TokenKind.BooleanAND, out prefix) ||
                MatchAdvance(TokenKind.Star, out prefix)       ||
                MatchAdvance(TokenKind.OperatorIncrement, out prefix) ||
                MatchAdvance(TokenKind.OperatorDecrement, out prefix);
        }

        private bool MatchTerm(out INode e, bool allowCallStatement = true)
        {
            if (MatchPrefixOperator(out var prefixOP))
            {
                if (!MatchTerm(out e, allowCallStatement))
                    ParseError("Unexpected prefix operator");

                e = new PrefixOperator() { Expression = e, Position = prefixOP.Position, Prefix = prefixOP.Kind };

                return true;
            }

            // arr[]
            // base.member
            // base.member()
            // base()
            // base.method().other_method()

            /*if (MatchInParExpression(out e))
            {
                while (MatchAdvance(TokenKind.Dot))
                    e = new MemberNode() { Base = e, Member = Expect("Expected member after `.`", TokenKind.Identifier), Position = e.Position.Start.Value..Back.Position.End.Value };
            }
            else if (!MatchMember(out e))
            {
                _currentIndex--;
                return false;
            }

            CollectPossibleArrayAccessNode(ref e);

            if (allowCallStatement && MatchCallStatement(out e, false, e))
            {
                *//*(call as CallStatement).Name = e;
                e = call;*//*
                Console.WriteLine(e.Dump());
            }*/

            if (!MatchMember(out e))
                return false;

            CollectPossibleArrayAccessNode(ref e);

            if (allowCallStatement && MatchCallStatement(out var call, e))
                e = call;

            CollectPossibleArrayAccessNode(ref e);

            if (MatchAdvance(TokenKind.OperatorIncrement) || MatchAdvance(TokenKind.OperatorDecrement))
                e = new PostfixOperator() { Expression = e, Position = Back.Position, Postfix = Back.Kind };

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

            var expression = ExpectExpression(true, true, TokenKind.Comma, TokenKind.CloseBrace);
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
                MatchAdvance(TokenKind.BooleanGreater, out op) ||
                MatchAdvance(TokenKind.BooleanLess, out op) ||
                MatchAdvance(TokenKind.BooleanGEQ, out op) ||
                MatchAdvance(TokenKind.BooleanLEQ, out op) ||
                MatchAdvance(TokenKind.KeyIn, out op);
        }

        private INode CollectTernary(Range ifposition)
        {
            var expression = ExpectExpression(end: TokenKind.KeyTVoid);
            var ifBody = ExpectFactor();

            Expect("In inline conditions there must be the else body: place it here", TokenKind.KeyElse);

            var elseBody = ExpectFactor();

            return new InlineConditionalExpression() { Expression = expression, IFBody = ifBody, ElseBody = elseBody, Position = ifposition };
        }

        private INode CollectNodeNew(Range newposition)
        {
            if (MatchAdvance(TokenKind.OpenBracket))
            {
                var type = ExpectType();
                INode size = null;

                if (MatchAdvance(TokenKind.Comma))
                {
                    size = ExpectExpression(end: TokenKind.CloseBracket);
                    _currentIndex--;
                }
                
                Expect("Expected `]` and the array body", TokenKind.CloseBracket);

                var array = new ArrayAllocationNode() { SizeIsImplicit = size == null, Size = size, Type = type };
                Expect("Expected the array body, empty (`{}`) if has to be instanced with type default values", TokenKind.OpenBrace);

                if (!Match(TokenKind.CloseBrace))
                {
                    do
                    {
                        array.AddArrayElement(ExpectExpression(true, true, TokenKind.Comma, TokenKind.CloseBrace));
                        _currentIndex--;
                    }
                    while (MatchAdvance(TokenKind.Comma));
                }

                Expect("", TokenKind.CloseBrace);

                return array;
            }

            var name = ExpectType();
            var allocation = new TypeAllocationNode() { Name = name, Position = newposition };

            Expect("Type allocation requires `{}`", TokenKind.OpenBrace);

            if (Match(TokenKind.Identifier))
                do
                    allocation.Body.Add(ExpectFieldAssign());
                while (MatchAdvance(TokenKind.Comma));

            Expect("", TokenKind.CloseBrace);

            return allocation;
        }

        /*private INode CollectBooleanExpression(ref INode e, Token boolOP, bool isFirst, params TokenKind[] end)
        {   
            var right = ExpectExpression(false, end);
            

            return e;
        }*/

        private bool MatchExpression(out INode expression)
        {
            var pos = _currentIndex;

            try
            {
                expression = ExpectExpression(true);
                return true;
            }
            catch (CompilationException)
            {
                expression = null;
                _currentIndex = pos;
                return false;
            }
        }

        private bool MatchAndOrOperator()
        {
            return MatchAdvance(TokenKind.BooleanOR) || MatchAdvance(TokenKind.BooleanAND);
        }

        private INode ExpectExpression(bool allowBoolOP = true, bool allowLogicOP = true, params TokenKind[] end)
        {
            INode e;

            if (MatchAdvance(TokenKind.KeyIf, out var token))
            {
                e = CollectTernary(token.Position);
                goto returnAndCheck;
            }

            if (MatchAdvance(TokenKind.KeyNew, out token))
            {
                e = CollectNodeNew(token.Position);
                goto returnAndCheck;
            }

            if (MatchFactor(out e))
            {
                if (MatchAdvance(TokenKind.Equal, out var eq) ||
                    MatchAdvance(TokenKind.AddAssignment, out eq) ||
                    MatchAdvance(TokenKind.SubAssignment, out eq) ||
                    MatchAdvance(TokenKind.MulAssignment, out eq) ||
                    MatchAdvance(TokenKind.DivAssignment, out eq))
                    return new AssignmentStatement() { Name = e, Operator = eq.Kind, Position = eq.Position, Body = ExpectExpression(end: end) };

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

            if (MatchAdvance(TokenKind.KeyAs, out var asToken))
                e = new CastExpressionNode() { Expression = e, Type = ExpectType(), Position = asToken.Position };

            if (MatchAdvance(TokenKind.KeyCatch))
            {
                var match = MatchAdvance(TokenKind.Identifier, out var error);

                e = new CatchExpressionNode() { Expression = e, OutError = match ? new Token?(error) : null, Position = Back.Position, Body = ExpectBlock() };
            }

            // Console.WriteLine($"cur: {Current} allboolop: {allowBoolOP} alllogicop: {allowLogicOP}");
            
            while (allowBoolOP && MatchBooleanOperator(out var boolOP))
                e = new BooleanExpressionNode() { Operator = ToOperatorKind(boolOP.Kind), Position = boolOP.Position, Left = e, Right = ExpectExpression(false, false, end) };

            while (allowLogicOP && MatchAndOrOperator())
                e = new BooleanExpressionNode() { Operator = ToOperatorKind(Back.Kind), Position = Back.Position, Left = e, Right = ExpectExpression(allowLogicOP: false, end: end) };
            
            returnAndCheck:
            if (allowBoolOP && allowLogicOP) // if is first call
                ExpectMultiple($"Invalid token in the current context, maybe missing one of `{TokenKindsToString(end)}`", end);

            return e;
        }

        private MugType ExpectVariableType()
        {
            return MatchAdvance(TokenKind.Colon) ? ExpectType() : MugType.Automatic(Back.Position);
        }

        private bool VariableDefinition(out INode statement)
        {
            statement = null;

            if (!MatchAdvance(TokenKind.KeyVar))
                return false;

            var name = Expect("Expected the variable name", TokenKind.Identifier);
            var type = ExpectVariableType();
            INode body = MatchAdvance(TokenKind.Equal) ? ExpectExpression(true) : null;

            statement = new VariableStatement() { Body = body, Name = name.Value.ToString(), Position = name.Position, Type = type };

            return true;
        }

        private bool ConstantDefinition(out INode statement)
        {
            statement = null;

            if (!MatchAdvance(TokenKind.KeyConst))
                return false;

            var name = Expect("Expected the constant name", TokenKind.Identifier);
            var type = ExpectVariableType();

            Expect("A constant cannot be declared without a body", TokenKind.Equal);

            var body = ExpectExpression(true);
            statement = new ConstantStatement() { Body = body, Name = name.Value.ToString(), Position = name.Position, Type = type };

            return true;
        }

        private bool ReturnDeclaration(out INode statement)
        {
            statement = null;

            if (!MatchAdvance(TokenKind.KeyReturn))
                return false;

            var pos = Back.Position;

            MatchExpression(out var body);

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
                expression = ExpectExpression(end: TokenKind.OpenBrace);
                _currentIndex--;
            }

            var body = ExpectBlock();

            INode elif = null;
            if (key.Kind != TokenKind.KeyWhile && key.Kind != TokenKind.KeyElse && (Match(TokenKind.KeyElif) || Match(TokenKind.KeyElse)))
                ConditionDefinition(out elif, false);

            statement = new ConditionalStatement() { Position = key.Position, Expression = expression, Kind = key.Kind, Body = body, ElseNode = (ConditionalStatement)elif };

            return true;
        }

        /*private bool ForLoopDefinition(out INode statement)
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
        }*/

        private VariableStatement CollectForLeftExpression()
        {
            if (MatchAdvance(TokenKind.Comma))
                return null;

            var name = Expect("Expected left statement or comma", TokenKind.Identifier);
            var result = new VariableStatement() { Name = name.Value, Position = name.Position };

            if (MatchAdvance(TokenKind.Colon))
                result.Type = ExpectType();
            else
            {
                result.Type = MugType.Automatic(name.Position);
                Expect("Type notation or body needed", TokenKind.Equal);
                result.Body = ExpectExpression(end: TokenKind.Comma);
            }

            return result;
        }

        private T CollectOrNull<T>(string error, TokenKind end) where T: class, INode
        {
            if (MatchAdvance(end))
                return null;

            var pos = Current.Position;
            var expr = ExpectExpression(end: end);
            if (expr is not T)
                ParseError(pos, error);

            return (T)expr;
        }

        private bool ForLoopDefinition(out INode statement)
        {
            statement = null;

            if (!MatchAdvance(TokenKind.KeyFor, out Token key))
                return false;

            var leftexpr = CollectForLeftExpression();
            var conditionexpr = CollectOrNull<INode>("Expected condition expression or comma", TokenKind.Comma);
            var rightexpr = CollectOrNull<IStatement>("Expected right statement or nothing", TokenKind.OpenBrace);

            _currentIndex--; // returning on `{` token

            statement = new ForLoopStatement()
            {
                Body = ExpectBlock(),
                LeftExpression = leftexpr,
                ConditionExpression = conditionexpr,
                RightExpression = rightexpr,
                Position = key.Position
            };

            return true;
        }

        private bool LoopManagerDefintion(out INode statement)
        {
            statement = null;

            if (!MatchAdvance(TokenKind.KeyContinue) &&
                !MatchAdvance(TokenKind.KeyBreak))
                return false;

            statement = new LoopManagementStatement() { Management = Back, Position = Back.Position };

            return true;
        }

        private INode ExpectStatement()
        {
            if (!VariableDefinition(out var statement) && // var x = value;
                !ReturnDeclaration(out statement) && // return value;
                !ConstantDefinition(out statement) && // const x = value;
                !ConstantDefinition(out statement) && // const x = value;
                !ConditionDefinition(out statement) &&
                !MatchWhenStatement(out statement, false) && // when comptimecondition {}
                !ForLoopDefinition(out statement) && // for x: type to, in value {}
                !LoopManagerDefintion(out statement))        // continue, break
                statement = ExpectExpression(true);
                /*MatchCallStatement(out statement, true) // f();
                !ValueAssignment(out statement)) // x = value;
                ParseError("Invalid token here");*/

            return statement;
        }

        private BlockNode ExpectBlock()
        {
            Expect("", TokenKind.OpenBrace);

            var block = new BlockNode();

            while (!Match(TokenKind.CloseBrace))
                block.Add(ExpectStatement());

            Expect("", TokenKind.CloseBrace);

            return block;
        }

        private ParameterListNode ExpectParameterListDeclaration()
        {
            Expect("", TokenKind.OpenPar);

            var parameters = new ParameterListNode();
            var count = 0;

            while (!MatchAdvance(TokenKind.ClosePar))
            {
                parameters.Parameters.Add(ExpectParameter(count == 0));
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

        private ParameterNode? CollectBaseDefinition()
        {
            if (!MatchAdvance(TokenKind.OpenPar))
                return null;

            var name = Expect("Expected the base instance name", TokenKind.Identifier);
            Expect("", TokenKind.Colon);
            var type = ExpectType();
            Expect("", TokenKind.ClosePar);

            return new ParameterNode(type, name.Value, new(), name.Position);
        }

        private bool FunctionDefinition(out INode node)
        {
            node = null;

            if (!MatchAdvance(TokenKind.KeyFunc)) // <func>
                return false;

            var modifier = GetModifier();
            var pragmas = GetPramas();
            var generics = new List<Token>();

            var @base = CollectBaseDefinition();

            var name = Expect("In function definition must specify the name", TokenKind.Identifier); // func <name>

            CollectGenericParameterDefinitions(generics);

            var parameters = ExpectParameterListDeclaration(); // func name<(..)>

            MugType type;

            if (MatchAdvance(TokenKind.Colon))
                type = ExpectType(true);
            else
                type = new MugType(name.Position, TypeKind.Void);

            if (Match(TokenKind.OpenBrace)) // function definition
            {
                var body = ExpectBlock();

                var f = new FunctionNode() { Base = @base, Modifier = modifier, Pragmas = pragmas, Body = body, Name = name.Value.ToString(), ParameterList = parameters, Type = type, Position = name.Position };

                f.Generics = generics;

                node = f;
            }
            else // prototype
            {
                if (@base is not null)
                    ParseError(@base.Value.Position, "The function base cannot be defined in function prototypes");

                var f = new FunctionPrototypeNode() { Modifier = modifier, Pragmas = pragmas, Name = name.Value.ToString(), ParameterList = parameters, Type = type, Position = name.Position };
                f.Generics = generics;

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

            directive = new UseDirective() { Body = body, Alias = alias, Position = token.Position };
            return true;
        }

        private bool MatchDeclareDirective(out INode directive)
        {
            directive = null;

            if (!MatchAdvance(TokenKind.KeyDeclare, out var token))
                return false;

            directive = new DeclareDirective() { Position = token.Position, Symbol = Expect("Expected symbol", TokenKind.Identifier) };
            return true;
        }

        private CompTimeExpression ExpectCompTimeExpression()
        {
            CompTimeExpression comptimeExpr = new();
            var boolOP = new Token(TokenKind.Bad, null, new());

            do
            {
                if (boolOP.Kind != TokenKind.Bad)
                    comptimeExpr.Expression.Add(boolOP);

                comptimeExpr.Expression.Add(Expect("Expected symbol", TokenKind.Identifier));

            } while (MatchAdvance(TokenKind.BooleanAND, out boolOP) || MatchAdvance(TokenKind.BooleanOR, out boolOP));

            return comptimeExpr;
        }

        private NodeBuilder ExpectWhenBlockGlobalScope()
        {
            Expect("Expected when body", TokenKind.OpenBrace);

            var members = ExpectNamespaceMembers(TokenKind.CloseBrace);

            Expect("", TokenKind.CloseBrace);

            return members;
        }

        private bool MatchWhenStatement(out INode directive, bool isGlobalScope)
        {
            directive = null;

            if (!MatchAdvance(TokenKind.KeyWhen, out var token))
                return false;

            var expression = ExpectCompTimeExpression();

            directive = new CompTimeWhenStatement()
            {
                Position = token.Position,
                Expression = expression,
                Body = isGlobalScope ? (object)ExpectWhenBlockGlobalScope() : (object)ExpectBlock()
            };
            return true;
        }

        private bool DirectiveDefinition(out INode directive)
        {
            return
                MatchImportDirective(out directive)  ||
                MatchUseDirective(out directive)     ||
                MatchDeclareDirective(out directive) ||
                MatchWhenStatement(out directive, true);
        }

        private FieldNode ExpectFieldDefinition()
        {
            var name = Expect("Expected field name", TokenKind.Identifier); // <field>

            Expect("Expected type specification", TokenKind.Colon); // field <:>

            var type = ExpectType(); // field: <error>

            return new FieldNode() { Name = name.Value.ToString(), Type = type, Position = name.Position };
        }

        private void CollectGenericParameterDefinitions(List<Token> generics)
        {
            if (MatchAdvance(TokenKind.BooleanLess))
            {
                do
                    generics.Add(Expect("Expected generic name", TokenKind.Identifier));
                while (MatchAdvance(TokenKind.Comma));

                Expect("", TokenKind.BooleanGreater);
            }
        }

        /// <summary>
        /// search for a struct definition
        /// </summary>
        private bool TypeDefinition(out INode node)
        {
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
            CollectGenericParameterDefinitions(statement.Generics);

            // struct body
            Expect("", TokenKind.OpenBrace);

            statement.Body.Add(ExpectFieldDefinition());

            // trailing commas are not allowed
            while (MatchAdvance(TokenKind.Comma))
                statement.Body.Add(ExpectFieldDefinition());

            Expect("", TokenKind.CloseBrace); // expected close body

            node = statement;

            return true;
        }

        private EnumMemberNode ExpectMemberDefinition()
        {
            var name = Expect("Expected enum member name", TokenKind.Identifier);

            Expect("Enum member must have a constant value", TokenKind.Colon);
            var value = ExpectConstant("Enum member must have a constant value");

            return new EnumMemberNode() { Name = name.Value, Value = value, Position = name.Position };
        }

        private MugType ExpectPrimitiveType()
        {
            if (!MatchPrimitiveType(out var type))
                ParseError("Expected primitive type");

            return MugType.FromToken(type);
        }

        private bool EnumDefinition(out INode node)
        {
            // mandatory for c# convention
            node = null;

            // returns if does not match a type keyword
            if (!MatchAdvance(TokenKind.KeyEnum))
                return false;

            // required an identifier
            var modifier = GetModifier();
            var pragmas = GetPramas();
            var name = Expect("Expected the type name after `enum` keyword", TokenKind.Identifier);
            var statement = new EnumStatement() { Modifier = modifier, Pragmas = pragmas, Name = name.Value.ToString(), Position = name.Position };

            // base type
            Expect("An enum must have a base type (u8, chr, ...)", TokenKind.Colon);
            statement.BaseType = ExpectPrimitiveType();

            // enum body
            Expect("", TokenKind.OpenBrace);

            statement.Body.Add(ExpectMemberDefinition());

            // trailing commas are not allowed
            while (MatchAdvance(TokenKind.Comma))
                statement.Body.Add(ExpectMemberDefinition());

            Expect("", TokenKind.CloseBrace); // expected close body

            node = statement;

            return true;
        }

        private void CollectPragmas()
        {
            if (!MatchAdvance(TokenKind.OpenBracket))
                return;

            _pragmas = new();

            do
            {
                var name = Expect("Expected pragma's name", TokenKind.Identifier).Value;
                Expect("", TokenKind.Colon);
                var value = ExpectConstant("Non-constant expressions not allowed in pragmas");

                _pragmas.SetPragma(name, value, ParseError, ref _currentIndex);
            } while (MatchAdvance(TokenKind.Comma));

            Expect("Expected pragmas close", TokenKind.CloseBracket);
        }

        private void CollectModifier()
        {
            if (MatchAdvance(TokenKind.KeyPub) || MatchAdvance(TokenKind.KeyPriv))
                _modifier = Back.Kind;
        }

        private bool EnumErrorDefinition(out INode statement)
        {
            statement = null;

            if (!MatchAdvance(TokenKind.KeyError))
                return false;

            var name = Expect("", TokenKind.Identifier);

            statement = new EnumErrorStatement() { Modifier = GetModifier(), Name = name.Value, Position = name.Position };

            Expect("", TokenKind.OpenBrace);

            do
                (statement as EnumErrorStatement).Body.Add(Expect("", TokenKind.Identifier));
            while (MatchAdvance(TokenKind.Comma));

            Expect("", TokenKind.CloseBrace);

            return true;
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
                        if (!EnumDefinition(out statement))
                        {
                            if (_pragmas is not null)
                                ParseError("Invalid pragmas for this member");

                            if (!EnumErrorDefinition(out statement))
                            {
                                if (_modifier != TokenKind.Bad)
                                    ParseError("Invalid modifier for this member");

                                // var id = constant;
                                if (!VariableDefinition(out statement))
                                    // import "", import path, use x as y
                                    if (!DirectiveDefinition(out statement))
                                        // if there is not global statement
                                        ParseError("Invalid token here");
                            }
                        }
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
            // to avoid bugs
            if (Match(TokenKind.EOF))
                return Module;

            Module.Name = new Token(TokenKind.Identifier, Lexer.ModuleName, 0..(Lexer.Source.Length - 1));

            // search for members
            Module.Members = ExpectNamespaceMembers();

            return Module;
        }
    }
}