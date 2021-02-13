using Mug.Compilation;
using System.Collections.Generic;
using System.Text;

namespace Mug.Models.Lexer
{
    public class MugLexer
    {
        public readonly string Source;
        public readonly string ModuleName;

        public List<Token> TokenCollection { get; set; }

        private StringBuilder _currentSymbol { get; set; }
        private int _currentIndex { get; set; }

        public void Reset()
        {
            TokenCollection = new();
            _currentIndex = 0;
            _currentSymbol = new();
        }

        public int Length
        {
            get
            {
                return TokenCollection == null ? 0 : TokenCollection.Count;
            }
        }

        public MugLexer(string moduleName, string source)
        {
            ModuleName = moduleName;
            Source = source;
        }

        private bool AddKeyword(TokenKind kind, string keyword)
        {
            TokenCollection.Add(new(kind, keyword, new(_currentIndex - keyword.Length, _currentIndex)));
            return true;
        }

        private bool CheckAndSetKeyword(string s) => s switch
        {
            "return" => AddKeyword(TokenKind.KeyReturn, s),
            "continue" => AddKeyword(TokenKind.KeyContinue, s),
            "break" => AddKeyword(TokenKind.KeyBreak, s),
            "while" => AddKeyword(TokenKind.KeyWhile, s),
            "pub" => AddKeyword(TokenKind.KeyPub, s),
            "use" => AddKeyword(TokenKind.KeyUse, s),
            "import" => AddKeyword(TokenKind.KeyImport, s),
            "new" => AddKeyword(TokenKind.KeyNew, s),
            "for" => AddKeyword(TokenKind.KeyFor, s),
            "type" => AddKeyword(TokenKind.KeyType, s),
            "as" => AddKeyword(TokenKind.KeyAs, s),
            "in" => AddKeyword(TokenKind.KeyIn, s),
            "to" => AddKeyword(TokenKind.KeyTo, s),
            "if" => AddKeyword(TokenKind.KeyIf, s),
            "elif" => AddKeyword(TokenKind.KeyElif, s),
            "else" => AddKeyword(TokenKind.KeyElse, s),
            "func" => AddKeyword(TokenKind.KeyFunc, s),
            "var" => AddKeyword(TokenKind.KeyVar, s),
            "const" => AddKeyword(TokenKind.KeyConst, s),
            "str" => AddKeyword(TokenKind.KeyTstr, s),
            "chr" => AddKeyword(TokenKind.KeyTchr, s),
            "bit" => AddKeyword(TokenKind.KeyTbool, s),
            "i8" => AddKeyword(TokenKind.KeyTi8, s),
            "i32" => AddKeyword(TokenKind.KeyTi32, s),
            "i64" => AddKeyword(TokenKind.KeyTi64, s),
            "u8" => AddKeyword(TokenKind.KeyTu8, s),
            "u32" => AddKeyword(TokenKind.KeyTu32, s),
            "u64" => AddKeyword(TokenKind.KeyTu64, s),
            "unknown" => AddKeyword(TokenKind.KeyTunknown, s),
            _ => false
        };

        private TokenKind IllegalChar()
        {
            this.Throw(_currentIndex, "Found illegal SpecialSymbol: mug's syntax does not use this character");
            return TokenKind.Bad;
        }

        private bool HasNext()
        {
            return _currentIndex + 1 < Source.Length;
        }
        private char GetNext()
        {
            return Source[_currentIndex + 1];
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
                TokenCollection.Add(new(kind, value, new(_currentIndex - value.Length + 1, _currentIndex + 1)));
            else if (value is not null)
                TokenCollection.Add(new(kind, value, new(_currentIndex - value.ToString().Length, _currentIndex)));
            else
                TokenCollection.Add(new(kind, value, new(_currentIndex, _currentIndex + 1)));
        }

        private void AddSpecial(TokenKind kind, string value)
        {
            InsertCurrentSymbol();
            TokenCollection.Add(new(kind, value, new(_currentIndex, _currentIndex + 1)));
        }

        private void AddDouble(TokenKind kind, string value)
        {
            InsertCurrentSymbol();
            TokenCollection.Add(new(kind, value, new(_currentIndex, _currentIndex + 2)));
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
            return CheckAndSetKeyword(s);
        }

        private void CheckValidIdentifier(string identifier)
        {
            var bad = new Token(TokenKind.Bad, null, new(_currentIndex - identifier.Length, _currentIndex));
            if (!char.IsLetter(identifier[0]) && identifier[0] != '_')
                this.Throw(bad, "Invalid identifier, following the mug's syntax rules, an ident cannot start by `", identifier[0].ToString(), "`;");
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
            return HasNext() && char.IsDigit(GetNext());
        }

        /// <summary>
        /// matches '#'
        /// </summary>
        private bool MatchInlineComment()
        {
            return Source[_currentIndex] == '#';
        }

        /// <summary>
        /// match if the line ends or the source ends
        /// </summary>
        private bool MatchEolOrEof()
        {
            return _currentIndex == Source.Length || Source[_currentIndex] == '\n';
        }

        /// <summary>
        /// checks if there is '#['
        /// </summary>
        private bool MatchStartMultiLineComment()
        {
            return HasNext() && Source[_currentIndex] == '#' && GetNext() == '[';
        }

        /// <summary>
        /// checks if there is ']#'
        /// </summary>
        private bool MatchEndMultiLineComment()
        {
            return HasNext() && Source[_currentIndex] == ']' && GetNext() == '#';
        }

        /// <summary>
        /// eats comments
        /// </summary>
        private void ConsumeComments()
        {
            if (MatchStartMultiLineComment())
            {
                // eats first two chars '#['
                _currentIndex += 2;

                while (!MatchEndMultiLineComment() && _currentIndex != Source.Length)
                    _currentIndex++;

                if (MatchEndMultiLineComment())
                    _currentIndex += 2;
            }
            else if (MatchInlineComment())
                while (!MatchEolOrEof())
                    _currentIndex++;
        }

        /// <summary>
        /// to rewrite
        /// </summary>
        private void CollectChar()
        {
            _currentSymbol.Append(Source[_currentIndex]);
            while (_currentIndex++ < Source.Length && Source[_currentIndex] != '\'')
                _currentSymbol.Append(Source[_currentIndex]);
            AddToken(TokenKind.ConstantChar, _currentSymbol.Append('\'').ToString(), true);
            if (_currentSymbol.Length > 3 || _currentSymbol.Length < 3)
                this.Throw(TokenCollection[^1], "Invalid characters in ConstantChar: it can only contain a character, not ", (_currentSymbol.Length - 2).ToString());
            _currentSymbol.Clear();
        }

        /// <summary>
        /// collects a string, now it does not support the escaped chars yet
        /// </summary>
        private void CollectString()
        {
            //add initial " and check next character
            _currentSymbol.Append(Source[_currentIndex++]);

            //consume string until EOF or closed " is found
            while (_currentIndex < Source.Length && Source[_currentIndex] != '"')
            {
                _currentSymbol.Append(Source[_currentIndex++]);
            }

            //if you found an EOF, throw
            if (_currentIndex == Source.Length && Source[_currentIndex - 1] != '"')
                this.Throw(_currentIndex - 1, $"String has not been correctly enclosed");

            //else add closing simbol
            AddToken(TokenKind.ConstantString, _currentSymbol.Append('"').ToString(), true);
            _currentSymbol.Clear();
        }

        /// <summary>
        /// follows identifier rules
        /// </summary>
        private bool IsValidIdentifierChar(char current)
        {
            return char.IsLetterOrDigit(current) || current == '_';
        }

        /// <summary>
        /// tests if current is an escaped char or a white space
        /// </summary>
        private bool IsEscapedChar(char current)
        {
            return char.IsControl(current) || char.IsWhiteSpace(current);
        }

        /// <summary>
        /// checks if there is a double symbol else add a single symbol
        /// </summary>
        private void ProcessSpecial(char current)
        {
            if (!HasNext())
            {
                AddSpecial(GetSpecial(current), current.ToString());
                return;
            }

            var doubleToken = current.ToString() + GetNext();

            if (doubleToken == "==") AddDouble(TokenKind.BooleanEQ, doubleToken);
            else if (doubleToken == "!=") AddDouble(TokenKind.BooleanNEQ, doubleToken);
            else if (doubleToken == "++") AddDouble(TokenKind.OperatorIncrement, doubleToken);
            else if (doubleToken == "+=") AddDouble(TokenKind.AddAssignment, doubleToken);
            else if (doubleToken == "--") AddDouble(TokenKind.OperatorDecrement, doubleToken);
            else if (doubleToken == "-=") AddDouble(TokenKind.SubAssignment, doubleToken);
            else if (doubleToken == "*=") AddDouble(TokenKind.MulAssignment, doubleToken);
            else if (doubleToken == "/=") AddDouble(TokenKind.DivAssignment, doubleToken);
            else if (doubleToken == "<=") AddDouble(TokenKind.BooleanMinEQ, doubleToken);
            else if (doubleToken == ">=") AddDouble(TokenKind.BooleanMajEQ, doubleToken);
            else if (doubleToken == "..") AddDouble(TokenKind.RangeDots, doubleToken);
            else
            {
                AddSpecial(GetSpecial(current), current.ToString());
                return;
            }

            // if is not a single value increments the index by one
            _currentIndex++;
        }

        /// <summary>
        /// recognize the kind of the char
        /// </summary>
        private void ProcessCurrentChar()
        {
            // remove useless comments
            ConsumeComments();

            // check if the newly stripped code is empty
            if (_currentIndex == Source.Length)
                return;

            char current = Source[_currentIndex];
            if (current == '.' && NextIsDigit())
                _currentSymbol.Append('.');

            if (current == '"')
                CollectString();
            else if (current == '\'')
                CollectChar();
            else if (IsEscapedChar(current)) // if control
                InsertCurrentSymbol(); // skip it and add the symbol if it's not empty
            else if (IsValidIdentifierChar(current))
                _currentSymbol.Append(current);
            else
                ProcessSpecial(current); // if current is not a valid id char, a control or a string quote
        }

        /// <summary>
        /// next char
        /// </summary>
        private void Advance()
        {
            _currentIndex++;
        }

        /// <summary>
        /// generates a token stream from a string
        /// </summary>
        public List<Token> Tokenize()
        {
            // set up all fields
            Reset();

            // go to the next char while there is one
            while (HasNext())
            {
                ProcessCurrentChar();
                Advance();
            }

            AddSpecial(TokenKind.EOF, "<EOF>");

            return TokenCollection;
        }
    }
}
