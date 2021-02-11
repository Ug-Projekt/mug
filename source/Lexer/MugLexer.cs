using Mug.Compilation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Mug.Models.Lexer
{
    public class MugLexer
    {
        public readonly string Source;
        public readonly string ModuleName;
        public readonly List<Token> TokenCollection = new();

        private StringBuilder _currentSymbol { get; }
        private int _currentIndex { get; set; }

        private const char InlineCommentSymbol = '#';

        public int Length
        {
            get
            {
                return TokenCollection == null ? TokenCollection.Count : 0;
            }
        }

        private bool IsValidModuleName(string moduleName)
        {
            for (int i = 0; i < moduleName.Length; i++)
                if (!IsValidIdentifierChar(moduleName[i]))
                    return false;
            return true;
        }
        public MugLexer(string moduleName, string source)
        {
            ModuleName = moduleName;
            Source = source;
        }

        private bool AddKeyword(TokenKind kind, int len)
        {
            TokenCollection.Add(new(kind, null, new(_currentIndex - len, _currentIndex)));
            return true;
        }

        private bool GetKeyword(string s) => s switch
        {
            "return" => AddKeyword(TokenKind.KeyReturn, s.Length),
            "continue" => AddKeyword(TokenKind.KeyContinue, s.Length),
            "break" => AddKeyword(TokenKind.KeyBreak, s.Length),
            "while" => AddKeyword(TokenKind.KeyWhile, s.Length),
            "pub" => AddKeyword(TokenKind.KeyPub, s.Length),
            "use" => AddKeyword(TokenKind.KeyUse, s.Length),
            "import" => AddKeyword(TokenKind.KeyImport, s.Length),
            "new" => AddKeyword(TokenKind.KeyNew, s.Length),
            "for" => AddKeyword(TokenKind.KeyFor, s.Length),
            "type" => AddKeyword(TokenKind.KeyType, s.Length),
            "as" => AddKeyword(TokenKind.KeyAs, s.Length),
            "in" => AddKeyword(TokenKind.KeyIn, s.Length),
            "to" => AddKeyword(TokenKind.KeyTo, s.Length),
            "if" => AddKeyword(TokenKind.KeyIf, s.Length),
            "elif" => AddKeyword(TokenKind.KeyElif, s.Length),
            "else" => AddKeyword(TokenKind.KeyElse, s.Length),
            "func" => AddKeyword(TokenKind.KeyFunc, s.Length),
            "var" => AddKeyword(TokenKind.KeyVar, s.Length),
            "const" => AddKeyword(TokenKind.KeyConst, s.Length),
            "str" => AddKeyword(TokenKind.KeyTstr, s.Length),
            "chr" => AddKeyword(TokenKind.KeyTchr, s.Length),
            "bit" => AddKeyword(TokenKind.KeyTbool, s.Length),
            "i8" => AddKeyword(TokenKind.KeyTi8, s.Length),
            "i32" => AddKeyword(TokenKind.KeyTi32, s.Length),
            "i64" => AddKeyword(TokenKind.KeyTi64, s.Length),
            "u8" => AddKeyword(TokenKind.KeyTu8, s.Length),
            "u32" => AddKeyword(TokenKind.KeyTu32, s.Length),
            "u64" => AddKeyword(TokenKind.KeyTu64, s.Length),
            "unknown" => AddKeyword(TokenKind.KeyTunknown, s.Length),
            _ => false
        };

        private TokenKind IllegalChar()
        {
            this.Throw(_currentIndex, "Found illegal SpecialSymbol: mug's syntax does not use this character");
            return TokenKind.Bad;
        }

        private bool MatchNext(char c)
        {
            var match = ThereIsANext(out var next) && next == c;
            // same of if (match) CurrentIndex++;
            _currentIndex += Convert.ToInt32(match);
            return match;
        }

        private TokenKind GetSpecial(char c) => c switch
        {
            '(' => TokenKind.OpenPar,
            ')' => TokenKind.ClosePar,
            '[' => TokenKind.OpenBracket,
            ']' => TokenKind.CloseBracket,
            '{' => TokenKind.OpenBrace,
            '}' => TokenKind.CloseBrace,
            '<' => TokenKind.BooleanMinor,
            '>' => TokenKind.BooleanMajor,
            '=' => TokenKind.Equal,
            '!' => TokenKind.Negation,
            '&' => TokenKind.BooleanAND,
            '|' => TokenKind.BooleanOR,
            '+' => TokenKind.Plus,
            '-' => TokenKind.Minus,
            '*' => TokenKind.Star,
            '/' => TokenKind.Slash,
            ',' => TokenKind.Comma,
            ';' => TokenKind.Semicolon,
            ':' => TokenKind.Colon,
            '.' => TokenKind.Dot,
            '@' => TokenKind.DirectiveSymbol,
            '?' => TokenKind.KeyTVoid,
            _ => IllegalChar()
        };

        private void AddToken(TokenKind kind, string value, bool isString = false)
        {
            if (kind == TokenKind.Identifier)
                CheckValidIdentifier(value);
            if (isString)
                TokenCollection.Add(new(kind, value, new(_currentIndex - value.ToString().Length + 1, _currentIndex + 1)));
            else if (value is not null)
                TokenCollection.Add(new(kind, value, new(_currentIndex - value.ToString().Length, _currentIndex)));
            else
                TokenCollection.Add(new(kind, null, new(_currentIndex, _currentIndex + 1)));
        }

        private void AddSpecial(TokenKind kind)
        {
            InsertCurrentSymbol();
            TokenCollection.Add(new(kind, null, new(_currentIndex, _currentIndex + 1)));
        }

        private void AddDouble(TokenKind kind)
        {
            InsertCurrentSymbol();
            TokenCollection.Add(new(kind, null, new(_currentIndex - 1, _currentIndex + 1)));
        }

        private void InsertCurrentSymbol()
        {
            if (!string.IsNullOrWhiteSpace(_currentSymbol.ToString()))
            {
                ProcessSymbol(_currentSymbol.ToString());
                _currentSymbol.Clear();
            }
        }

        private bool IsDigit(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return false;
            return long.TryParse(s, out long l);
        }

        private bool IsFloatDigit(ref string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return false;
            if (s[0] == '.')
                s = '0' + s;
            return double.TryParse(s, out double d);
        }

        private bool InsertKeyword(string s)
        {
            return GetKeyword(s);
        }

        private void CheckValidIdentifier(string identifier)
        {
            var bad = new Token(TokenKind.Bad, null, new(_currentIndex - identifier.Length, _currentIndex));
            if (!char.IsLetter(identifier[0]) && identifier[0] != '_')
                this.Throw(bad, "Invalid identifier, following the mug's syntax rules, an ident cannot start with `", identifier[0].ToString(), "`;");
            if (identifier.Contains('.'))
                this.Throw(bad, "Invalid identifier, following the mug's syntax rules, an ident cannot contain `.`;");
        }

        private bool IsBoolean(string value)
        {
            return value == "true" || value == "false";
        }

        private void ProcessSymbol(string value)
        {
            if (IsDigit(value))
                AddToken(TokenKind.ConstantDigit, value);
            else if (IsFloatDigit(ref value))
                AddToken(TokenKind.ConstantFloatDigit, value);
            else if (IsBoolean(value))
                AddToken(TokenKind.ConstantBoolean, value);
            else if (!InsertKeyword(value))
            {
                CheckValidIdentifier(value);
                AddToken(TokenKind.Identifier, value);
            }
        }

        private bool ThereIsANext(out char next)
        {
            next = (char)0;
            if (_currentIndex + 1 >= Source.Length)
                return false;
            next = Source[_currentIndex + 1];
            return true;
        }

        private bool NextIsDigit()
        {
            return ThereIsANext(out var next) && char.IsDigit(next);
        }

        private bool MatchInlineComment()
        {
            return Source[_currentIndex] == InlineCommentSymbol;
        }

        private bool MatchEolEof()
        {
            return _currentIndex == Source.Length || Source[_currentIndex] == '\n';
        }

        private bool MatchStartMultiLineComment()
        {
            return MatchInlineComment() && MatchNext('[');
        }

        private bool MatchEndMultiLineComment()
        {
            return ThereIsANext(out var next) && Source[_currentIndex] == ']' && Source[_currentIndex + 1] == '#';
        }

        private bool ConsumeComments()
        {
            if (MatchStartMultiLineComment())
            {
                _currentIndex += 2;
                while (!MatchEndMultiLineComment())
                    _currentIndex++;
                _currentIndex += 2;
            }
            else if (MatchInlineComment())
                while (!MatchEolEof())
                    _currentIndex++;
            else
                return false;
            return true;
        }

        private void CollectChar()
        {
            _currentSymbol.Append(Source[_currentIndex]);
            while (_currentIndex++ < Source.Length && Source[_currentIndex] != '\'')
                _currentSymbol.Append(Source[_currentIndex]);
            AddToken(TokenKind.ConstantChar, _currentSymbol.Append( '\'').ToString(), true);
            if (_currentSymbol.Length > 3 || _currentSymbol.Length < 3)
                this.Throw(TokenCollection[^1], "Invalid characters in ConstantChar: it can only contain a character, not ", (_currentSymbol.Length - 2).ToString());
            _currentSymbol.Clear();
        }

        private void CollectString()
        {
            _currentSymbol.Append(Source[_currentIndex]);
            while (_currentIndex++ < Source.Length && Source[_currentIndex] != '"')
                _currentSymbol.Append(Source[_currentIndex]);
            AddToken(TokenKind.ConstantString, _currentSymbol.Append('"').ToString(), true);
            _currentSymbol.Clear();
        }

        private bool IsValidIdentifierChar(char current)
        {
            return char.IsLetterOrDigit(current) || current == '_';
        }

        private bool IsControl(char current)
        {
            return char.IsControl(current) || char.IsWhiteSpace(current);
        }

        private bool MatchEol()
        {
            return Source[_currentIndex] == '\n' || Source[_currentIndex] == '\r';
        }

        private bool MatchPart(char next, TokenKind token)
        {
            var match = MatchNext(next);
            if (match)
                AddDouble(token);
            return match;
        }

        private void ProcessSpecial(char current)
        {
            switch (current)
            {
                case '=':
                    if (!MatchPart('=', TokenKind.BooleanEQ)) goto default;
                    break;
                case '!':
                    if (!MatchPart('=', TokenKind.BooleanNEQ)) goto default;
                    break;
                case '+':
                    if (!MatchPart('+', TokenKind.OperatorIncrement) &&
                        !MatchPart('=', TokenKind.AddAssignment))
                        goto default;
                    break;
                case '-':
                    if (!MatchPart('-', TokenKind.OperatorDecrement) &&
                        !MatchPart('=', TokenKind.SubAssignment))
                        goto default;
                    break;
                case '*':
                    if (!MatchPart('=', TokenKind.MulAssignment)) goto default;
                    break;
                case '/':
                    if (!MatchPart('=', TokenKind.DivAssignment)) goto default;
                    break;
                case '<':
                    if (!MatchPart('=', TokenKind.BooleanMinEQ)) goto default;
                    break;
                case '>':
                    if (!MatchPart('=', TokenKind.BooleanMajEQ)) goto default;
                    break;
                case '.':
                    if (!MatchPart('=', TokenKind.RangeDots)) goto default;
                    break;
                default:
                    AddSpecial(GetSpecial(current));
                    break;
            }
        }

        private void ProcessCurrentChar()
        {
            ConsumeComments();
            var current = Source[_currentIndex];
            if (current == '.' && NextIsDigit())
                _currentSymbol.Append('.');
            if (current == '"')
            {
                CollectString();
                return;
            }
            else if (current == '\'')
            {
                CollectChar();
                return;
            }
            if (IsControl(current))
                InsertCurrentSymbol();
            else if (IsValidIdentifierChar(current))
                _currentSymbol.Append(current);
            else
                ProcessSpecial(current);
        }

        private int Advance()
        {
            return _currentIndex++;
        }

        public List<Token> Tokenize()
        {
            do
                ProcessCurrentChar();
            while (Advance() < Source.Length - 1);

            AddSpecial(TokenKind.EOF);

            return TokenCollection;
        }
    }
}