using Mug.Compilation;
using System.Collections.Generic;

namespace Mug.Models.Lexer
{
    public class MugLexer
    {
        public readonly string Source;
        public readonly string ModuleName;
        public List<Token> TokenCollection;
        private string CurrentSymbol = "";
        private int CurrentIndex = 0;
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
            TokenCollection.Add(new(kind, null, new(CurrentIndex - len, CurrentIndex)));
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
            this.Throw(CurrentIndex, "Found illegal SpecialSymbol: mug's syntax does not use this character");
            return TokenKind.Bad;
        }

        private bool MatchNext(char next)
        {
            var match = CurrentIndex + 1 < Source.Length && Source[CurrentIndex + 1] == next;
            if (match)
                CurrentIndex++;
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
                TokenCollection.Add(new(kind, value, new(CurrentIndex - value.ToString().Length + 1, CurrentIndex + 1)));
            else if (value is not null)
                TokenCollection.Add(new(kind, value, new(CurrentIndex - value.ToString().Length, CurrentIndex)));
            else
                TokenCollection.Add(new(kind, null, new(CurrentIndex, CurrentIndex + 1)));
        }

        private void AddSpecial(TokenKind kind)
        {
            InsertCurrentSymbol();
            TokenCollection.Add(new(kind, null, new(CurrentIndex, CurrentIndex + 1)));
        }

        private void AddDouble(TokenKind kind)
        {
            InsertCurrentSymbol();
            TokenCollection.Add(new(kind, null, new(CurrentIndex - 1, CurrentIndex + 1)));
        }

        private void InsertCurrentSymbol()
        {
            if (!string.IsNullOrWhiteSpace(CurrentSymbol))
            {
                ProcessSymbol(CurrentSymbol);
                CurrentSymbol = "";
            }
        }

        private bool IsDigit(string s)
        {
            return !string.IsNullOrWhiteSpace(s) && long.TryParse(s, out _);
        }

        private bool IsFloatDigit(ref string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return false;
            if (s[0] == '.')
                s = '0' + s;
            return double.TryParse(s, out _);
        }

        private bool InsertKeyword(string s)
        {
            return GetKeyword(s);
        }

        private void CheckValidIdentifier(string identifier)
        {
            var bad = new Token(TokenKind.Bad, null, new(CurrentIndex - identifier.Length, CurrentIndex));
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

        private bool NextIsDigit()
        {
            return CurrentIndex + 1 < Source.Length && char.IsDigit(Source[CurrentIndex + 1]);
        }

        private bool MatchInlineComment()
        {
            return Source[CurrentIndex] == '#';
        }

        private bool MatchEolEof()
        {
            return CurrentIndex == Source.Length || Source[CurrentIndex] == '\n';
        }

        private bool MatchStartMultiLineComment()
        {
            return CurrentIndex + 1 < Source.Length && Source[CurrentIndex] == '#' && Source[CurrentIndex + 1] == '[';
        }

        private bool MatchEndMultiLineComment()
        {
            return CurrentIndex + 1 < Source.Length && Source[CurrentIndex] == ']' && Source[CurrentIndex + 1] == '#';
        }

        private bool ConsumeComments()
        {
            if (MatchStartMultiLineComment())
            {
                CurrentIndex += 2;
                while (!MatchEndMultiLineComment())
                    CurrentIndex++;
                CurrentIndex += 2;
            }
            else if (MatchInlineComment())
                while (!MatchEolEof())
                    CurrentIndex++;
            else
                return false;
            return true;
        }

        private void CollectChar()
        {
            CurrentSymbol += Source[CurrentIndex];
            while (CurrentIndex++ < Source.Length && Source[CurrentIndex] != '\'')
                CurrentSymbol += Source[CurrentIndex];
            AddToken(TokenKind.ConstantChar, CurrentSymbol += '\'', true);
            if (CurrentSymbol.Length > 3 || CurrentSymbol.Length < 3)
                this.Throw(TokenCollection[^1], "Invalid characters in ConstantChar: it can only contain a character, not ", (CurrentSymbol.Length - 2).ToString());
            CurrentSymbol = "";
        }

        private void CollectString()
        {
            CurrentSymbol += Source[CurrentIndex];
            while (CurrentIndex++ < Source.Length && Source[CurrentIndex] != '"')
                CurrentSymbol += Source[CurrentIndex];
            AddToken(TokenKind.ConstantString, CurrentSymbol + '"', true);
            CurrentSymbol = "";
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
            return Source[CurrentIndex] == '\n' || Source[CurrentIndex] == '\r';
        }

        private bool MatchPart(char toMatch, TokenKind toInsert)
        {
            var match = MatchNext(toMatch);
            if (match)
                AddDouble(toInsert);
            return match;
        }

        private void ProcessSpecial(char current)
        {
            switch (current)
            {
                case '=':
                    if (MatchPart('=', TokenKind.BooleanEQ))
                        break;
                    goto default;
                case '!':
                    if (MatchPart('=', TokenKind.BooleanNEQ))
                        break;
                    goto default;
                case '+':
                    if (MatchPart('+', TokenKind.OperatorIncrement) || MatchPart('=', TokenKind.AddAssignment))
                        break;
                    goto default;
                case '-':
                    if (MatchPart('-', TokenKind.OperatorDecrement) || MatchPart('=', TokenKind.SubAssignment))
                        break;
                    goto default;
                case '*':
                    if (MatchPart('=', TokenKind.MulAssignment))
                        break;
                    goto default;
                case '/':
                    if (MatchPart('=', TokenKind.DivAssignment))
                        break;
                    goto default;
                case '<':
                    if (MatchPart('=', TokenKind.BooleanMinEQ))
                        break;
                    goto default;
                case '>':
                    if (MatchPart('=', TokenKind.BooleanMajEQ))
                        break;
                    goto default;
                case '.':
                    if (MatchPart('.', TokenKind.RangeDots))
                        break;
                    goto default;
                default:
                    AddSpecial(GetSpecial(current));
                    break;
            }
        }

        private void ProcessCurrentChar()
        {
            ConsumeComments();
            char current = Source[CurrentIndex];
            if (current == '.' && NextIsDigit())
                CurrentSymbol += '.';
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
            else if (MatchEol())
                return;
            if (IsControl(current))
                InsertCurrentSymbol();
            else if (IsValidIdentifierChar(current))
                CurrentSymbol += current;
            else
                ProcessSpecial(current);
        }
        public List<Token> Tokenize()
        {
            if (TokenCollection is not null)
                return TokenCollection;
            TokenCollection = new();
            do
                ProcessCurrentChar();
            while (CurrentIndex++ < Source.Length - 1);
            AddSpecial(TokenKind.EOF);
            return TokenCollection;
        }
    }
}