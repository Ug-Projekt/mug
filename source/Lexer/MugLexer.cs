﻿using Mug.Compilation;
using System;
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

        /// <summary>
        /// restores all the fields to their default values
        /// </summary>
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

        /// <summary>
        /// adds a keyword to the tokens stream and returns true
        /// </summary>
        private bool AddKeyword(TokenKind kind, string keyword)
        {
            TokenCollection.Add(new(kind, keyword, (_currentIndex - keyword.Length).._currentIndex));
            return true;
        }

        /// <summary>
        /// returns true and insert a keyword token if s is a keyword, otherwise returns false, see the caller to understand better
        /// </summary>
        private bool CheckAndSetKeyword(string s) => s switch
        {
            "return" => AddKeyword(TokenKind.KeyReturn, s),
            "continue" => AddKeyword(TokenKind.KeyContinue, s),
            "break" => AddKeyword(TokenKind.KeyBreak, s),
            "while" => AddKeyword(TokenKind.KeyWhile, s),
            "pub" => AddKeyword(TokenKind.KeyPub, s),
            "priv" => AddKeyword(TokenKind.KeyPriv, s),
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
            "i32" => AddKeyword(TokenKind.KeyTi32, s),
            "i64" => AddKeyword(TokenKind.KeyTi64, s),
            "u1" => AddKeyword(TokenKind.KeyTbool, s),
            "u8" => AddKeyword(TokenKind.KeyTu8, s),
            "u32" => AddKeyword(TokenKind.KeyTu32, s),
            "u64" => AddKeyword(TokenKind.KeyTu64, s),
            "ptr" => AddKeyword(TokenKind.KeyTPtr, s),
            "unknown" => AddKeyword(TokenKind.KeyTunknown, s),
            _ => false
        };

        private T InExpressionError<T>(string error)
        {
            this.Throw(_currentIndex, error);
            throw new Exception("unreachable");
        }

        /// <summary>
        /// checks if there is a next char, to avoid index out of range exception
        /// </summary>
        private bool HasNext()
        {
            return _currentIndex + 1 < Source.Length;
        }

        private char GetNext()
        {
            return Source[_currentIndex + 1];
        }

        /// <summary>
        /// recognizes a single symbol or launches compilation-error
        /// </summary>
        private TokenKind GetSingle(char c) => c switch
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
            '?' => TokenKind.KeyTVoid,
            _ => InExpressionError<TokenKind>("Invalid char")
        };

        private void AddToken(TokenKind kind, string value)
        {
            // identifiers must follow rules
            if (kind == TokenKind.Identifier)
                CheckValidIdentifier(value);

            if (value is not null)
                TokenCollection.Add(new(kind, value, (_currentIndex - value.ToString().Length).._currentIndex));
            else // chatching null reference exception
                TokenCollection.Add(new(kind, value, _currentIndex..(_currentIndex + 1)));
        }

        /// <summary>
        /// adds a single symbol
        /// </summary>
        private void AddSingle(TokenKind kind, string value)
        {
            InsertCurrentSymbol();
            TokenCollection.Add(new(kind, value, _currentIndex..(_currentIndex + 1)));
        }

        /// <summary>
        /// adds a double symbol
        /// </summary>
        private void AddDouble(TokenKind kind, string value)
        {
            /*
             * current index as start position
             * moves the index by one: a double token occupies 2 chars
             */

            TokenCollection.Add(new(kind, value, _currentIndex..(++_currentIndex + 1)));
        }

        /// <summary>
        /// inserts current symbol in the tokens stream and clears it if it's not empty
        /// </summary>
        private void InsertCurrentSymbol()
        {
            if (!string.IsNullOrWhiteSpace(_currentSymbol.ToString()))
            {
                // symbol recognition
                ProcessSymbol(_currentSymbol.ToString());
                _currentSymbol.Clear();
            }
        }

        private bool IsDigit(string s)
        {
            return long.TryParse(s, out _);
        }

        private bool IsFloatDigit(ref string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return false;

            if (s[0] == '.')
                s = '0' + s;

            return double.TryParse(s, out _);
        }

        /// <summary>
        /// checks if an identifier matches the language rules
        /// </summary>
        private void CheckValidIdentifier(string identifier)
        {
            var bad = new Token(TokenKind.Bad, null, (_currentIndex - identifier.Length).._currentIndex);

            if (!char.IsLetter(identifier[0]) && identifier[0] != '_')
                this.Throw(bad, "Invalid identifier, following the mug's syntax rules, an ident cannot start by `", identifier[0].ToString(), "`;");

            if (identifier.Contains('.'))
                this.Throw(bad, "Invalid identifier, following the mug's syntax rules, an ident cannot contain `.`;");
        }

        /// <summary>
        /// tests if value is a boolean constant
        /// </summary>
        private bool IsBoolean(string value)
        {
            return value == "true" || value == "false";
        }

        /// <summary>
        /// adds a new token with the recognized kind (based on the symbol format)
        /// </summary>
        private void ProcessSymbol(string value)
        {
            if (IsDigit(value))
                AddToken(TokenKind.ConstantDigit, value);
            else if (IsFloatDigit(ref value))
                AddToken(TokenKind.ConstantFloatDigit, value);
            else if (IsBoolean(value))
                AddToken(TokenKind.ConstantBoolean, value);
            else if (!CheckAndSetKeyword(value)) // if value is a keyword InsertKeyword will add a new token and will return true, otherwise false
            {
                // value is an identifier
                // the identifier must follow the language rules
                CheckValidIdentifier(value);

                AddToken(TokenKind.Identifier, value);
            }
        }

        /// <summary>
        /// checks if next char is a digit
        /// </summary>
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
        /// collects a constant character
        /// </summary>
        private void CollectChar()
        {
            var start = _currentIndex++;

            //consume string until EOF or closed " is found
            while (_currentIndex < Source.Length && Source[_currentIndex] != '\'')
            {
                char c = Source[_currentIndex++];

                if (c == '\\')
                    c = RecognizeEscapedChar(Source[_currentIndex++]);

                _currentSymbol.Append(c);
            }

            var end = _currentIndex;

            //if you found an EOF, throw
            if (_currentIndex == Source.Length && Source[_currentIndex - 1] != '"')
                this.Throw(_currentIndex - 1, $"Char has not been correctly enclosed");

            //longer than one char
            if (_currentSymbol.Length > 1)
                this.Throw(start..end, "Too many characters in const char");
            else if (_currentSymbol.Length < 1)
                this.Throw(start..end, "Not enough characters in const char");

            //else add closing simbol
            TokenCollection.Add(new(TokenKind.ConstantChar, _currentSymbol.ToString(), new(start, end + 1)));
            _currentSymbol.Clear();
        }

        private char RecognizeEscapedChar(char escapedchar)
        {
            return escapedchar switch
            {
                'n' => '\n',
                't' => '\t',
                'r' => '\r',
                '0' => '\0',
                '\'' or '"' or '\\' => escapedchar,
                _ => InExpressionError<char>("Unable to recognize escaped char")
            };
        }

        /// <summary>
        /// collects a string, now it does not support the escaped chars yet
        /// </summary>
        private void CollectString()
        {
            var start = _currentIndex++;

            //consume string until EOF or closed " is found
            while (_currentIndex < Source.Length && Source[_currentIndex] != '"')
            {
                char c = Source[_currentIndex++];

                if (c == '\\')
                    c = RecognizeEscapedChar(Source[_currentIndex++]);

                _currentSymbol.Append(c);
            }

            var end = _currentIndex;

            //if you found an EOF, throw
            if (_currentIndex == Source.Length && Source[_currentIndex - 1] != '"')
                this.Throw(_currentIndex - 1, $"String has not been correctly enclosed");

            //else add closing simbol
            TokenCollection.Add(new(TokenKind.ConstantString, _currentSymbol.ToString(), new(start, end + 1)));
            _currentSymbol.Clear();
        }

        /// <summary>
        /// collects a symbol incapsulated in a backtick string and add it to the token stream as identifier
        /// </summary>
        private void CollectBacktick()
        {
            var start = _currentIndex++;

            //consume string until EOF or closed ` is found
            while (_currentIndex < Source.Length && Source[_currentIndex] != '`')
                _currentSymbol.Append(Source[_currentIndex++]);

            var end = _currentIndex;

            //if you found an EOF, throw
            if (_currentIndex == Source.Length && Source[_currentIndex - 1] != '`')
                this.Throw(_currentIndex - 1, $"Backtick sequence has not been correctly enclosed");

            if (_currentSymbol.Length < 1)
                this.Throw(start..end, "Not enough characters in backtick sequence");

            //else add closing simbol
            TokenCollection.Add(new(TokenKind.Identifier, _currentSymbol.ToString(), new(start, end + 1)));
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
            InsertCurrentSymbol();
            if (!HasNext())
            {
                AddSingle(GetSingle(current), current.ToString());
                return;
            }

            var doubleToken = current.ToString() + GetNext();

            // checks if there is a double token
            switch (doubleToken)
            {
                case "==": AddDouble(TokenKind.BooleanEQ, doubleToken); break;
                case "!=": AddDouble(TokenKind.BooleanNEQ, doubleToken); break;
                case "++": AddDouble(TokenKind.OperatorIncrement, doubleToken); break;
                case "+=": AddDouble(TokenKind.AddAssignment, doubleToken); break;
                case "--": AddDouble(TokenKind.OperatorDecrement, doubleToken); break;
                case "-=": AddDouble(TokenKind.SubAssignment, doubleToken); break;
                case "*=": AddDouble(TokenKind.MulAssignment, doubleToken); break;
                case "/=": AddDouble(TokenKind.DivAssignment, doubleToken); break;
                case "<=": AddDouble(TokenKind.BooleanMinEQ, doubleToken); break;
                case ">=": AddDouble(TokenKind.BooleanMajEQ, doubleToken); break;
                case "..": AddDouble(TokenKind.RangeDots, doubleToken); break;
                case "@[": AddDouble(TokenKind.OpenPragmas, doubleToken); break;
                default:
                    if (current == '"') CollectString();
                    else if (current == '\'') CollectChar();
                    else if (current == '`') CollectBacktick();
                    else
                        AddSingle(GetSingle(current), current.ToString());

                    break;
            }
        }

        /// <summary>
        /// recognize the kind of the char
        /// </summary>
        private void ProcessCurrentChar()
        {
            // remove useless comments
            ConsumeComments();

            // check if the newly stripped code is empty
            if (_currentIndex >= Source.Length)
                return;

            // to avoid a massive array access, also better to read
            char current = Source[_currentIndex];

            if (IsEscapedChar(current)) // skipping it and add the symbol if it's not empty
                InsertCurrentSymbol();
            else if (IsValidIdentifierChar(current)) // adding a char to the current symbol
                _currentSymbol.Append(current);
            else
                ProcessSpecial(current); // if current is not a valid id char, a control or a string quote
        }

        /// <summary>
        /// generates a token stream from a string
        /// </summary>
        public List<Token> Tokenize()
        {
            // set up all fields
            Reset();

            // go to the next char while there is one
            while (_currentIndex < Source.Length)
            {
                ProcessCurrentChar();
                _currentIndex++;
            }



            // end of file token
            AddSingle(TokenKind.EOF, "<EOF>");

            return TokenCollection;
        }
    }
}
