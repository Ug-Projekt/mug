using Mug.Compilation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mug.Models.Lexer
{
    public class MugLexer
    {
        public readonly string Source;
        public readonly string ModuleName;
        public readonly char[] ValidBacktickSequenceCharacters = { '[', ']', '!', '-', '+', '*', '/', '=' };

        public List<Token> TokenCollection { get; set; }

        private StringBuilder CurrentSymbol { get; set; }
        private int CurrentIndex { get; set; }

        /// <summary>
        /// restores all the fields to their default values
        /// </summary>
        public void Reset()
        {
            TokenCollection = new();
            CurrentIndex = 0;
            CurrentSymbol = new();
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
            TokenCollection.Add(new(kind, keyword, (CurrentIndex - keyword.Length)..CurrentIndex));
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
            "enum" => AddKeyword(TokenKind.KeyEnum, s),
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
            "void" => AddKeyword(TokenKind.KeyTVoid, s),
            "u1" => AddKeyword(TokenKind.KeyTbool, s),
            "u8" => AddKeyword(TokenKind.KeyTu8, s),
            "u32" => AddKeyword(TokenKind.KeyTu32, s),
            "u64" => AddKeyword(TokenKind.KeyTu64, s),
            "unknown" => AddKeyword(TokenKind.KeyTunknown, s),
            "when" => AddKeyword(TokenKind.KeyWhen, s),
            "declare" => AddKeyword(TokenKind.KeyDeclare, s),
            "catch" => AddKeyword(TokenKind.KeyCatch, s),
            "error" => AddKeyword(TokenKind.KeyError, s),
            _ => false
        };

        private T InExpressionError<T>(string error)
        {
            this.Throw(CurrentIndex, error);
            throw new Exception("unreachable");
        }

        /// <summary>
        /// checks if there is a next char, to avoid index out of range exception
        /// </summary>
        private bool HasNext()
        {
            return CurrentIndex + 1 < Source.Length;
        }

        private char GetNext()
        {
            return Source[CurrentIndex + 1];
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
            '<' => TokenKind.BooleanLess,
            '>' => TokenKind.BooleanGreater,
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
            _ => InExpressionError<TokenKind>("Invalid char")
        };

        private void AddToken(TokenKind kind, string value)
        {
            // identifiers must follow rules
            if (kind == TokenKind.Identifier)
                CheckValidIdentifier(value);

            if (value is not null)
                TokenCollection.Add(new(kind, value, (CurrentIndex - value.ToString().Length)..CurrentIndex));
            else // chatching null reference exception
                TokenCollection.Add(new(kind, value, CurrentIndex..(CurrentIndex + 1)));
        }

        /// <summary>
        /// adds a single symbol
        /// </summary>
        private void AddSingle(TokenKind kind, string value)
        {
            InsertCurrentSymbol();
            TokenCollection.Add(new(kind, value, CurrentIndex..(CurrentIndex + 1)));
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

            TokenCollection.Add(new(kind, value, CurrentIndex..(++CurrentIndex + 1)));
        }

        /// <summary>
        /// inserts current symbol in the tokens stream and clears it if it's not empty
        /// </summary>
        private void InsertCurrentSymbol()
        {
            if (!string.IsNullOrWhiteSpace(CurrentSymbol.ToString()))
            {
                // symbol recognition
                ProcessSymbol(CurrentSymbol.ToString());
                CurrentSymbol.Clear();
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
            var bad = new Token(TokenKind.Bad, null, (CurrentIndex - identifier.Length)..CurrentIndex);

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
            return Source[CurrentIndex] == '#';
        }

        /// <summary>
        /// match if the line ends or the source ends
        /// </summary>
        private bool MatchEolOrEof()
        {
            return CurrentIndex == Source.Length || Source[CurrentIndex] == '\n';
        }

        /// <summary>
        /// checks if there is '#['
        /// </summary>
        private bool MatchStartMultiLineComment()
        {
            return HasNext() && Source[CurrentIndex] == '#' && GetNext() == '[';
        }

        /// <summary>
        /// checks if there is ']#'
        /// </summary>
        private bool MatchEndMultiLineComment()
        {
            return HasNext() && Source[CurrentIndex] == ']' && GetNext() == '#';
        }

        /// <summary>
        /// eats comments
        /// </summary>
        private void ConsumeComments()
        {
            if (MatchStartMultiLineComment())
            {
                // eats first two chars '#['
                CurrentIndex += 2;

                while (!MatchEndMultiLineComment() && CurrentIndex != Source.Length)
                    CurrentIndex++;

                if (MatchEndMultiLineComment())
                    CurrentIndex += 2;
            }
            else if (MatchInlineComment())
                while (!MatchEolOrEof())
                    CurrentIndex++;
        }

        /// <summary>
        /// collects a constant character
        /// </summary>
        private void CollectChar()
        {
            var start = CurrentIndex++;

            //consume string until EOF or closed " is found
            while (CurrentIndex < Source.Length && Source[CurrentIndex] != '\'')
            {
                char c = Source[CurrentIndex++];

                if (c == '\\')
                    c = RecognizeEscapedChar(Source[CurrentIndex++]);

                CurrentSymbol.Append(c);
            }

            var end = CurrentIndex;

            //if you found an EOF, throw
            if (CurrentIndex == Source.Length && Source[CurrentIndex - 1] != '"')
                this.Throw(CurrentIndex - 1, $"Char has not been correctly enclosed");

            end++;

            //longer than one char
            if (CurrentSymbol.Length > 1)
                this.Throw(start..end, "Too many characters in const char");
            else if (CurrentSymbol.Length < 1)
                this.Throw(start..end, "Not enough characters in const char");

            //else add closing simbol
            TokenCollection.Add(new(TokenKind.ConstantChar, CurrentSymbol.ToString(), new(start, end)));
            CurrentSymbol.Clear();
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
            var start = CurrentIndex++;

            //consume string until EOF or closed " is found
            while (CurrentIndex < Source.Length && Source[CurrentIndex] != '"')
            {
                char c = Source[CurrentIndex++];

                if (c == '\\')
                    c = RecognizeEscapedChar(Source[CurrentIndex++]);

                CurrentSymbol.Append(c);
            }

            var end = CurrentIndex;

            //if you found an EOF, throw
            if (CurrentIndex == Source.Length && Source[CurrentIndex - 1] != '"')
                this.Throw(CurrentIndex - 1, $"String has not been correctly enclosed");

            //else add closing simbol
            TokenCollection.Add(new(TokenKind.ConstantString, CurrentSymbol.ToString(), new(start, end + 1)));
            CurrentSymbol.Clear();
        }

        private bool IsValidBackTickSequence(string sequence)
        {
            for (int i = 0; i < sequence.Length; i++)
            {
                var chr = sequence[i];

                if (!ValidBacktickSequenceCharacters.Contains(chr))
                    return false;
            }

            return true;
        }

        private bool IsKeyword(string sequence)
        {
            var iskeyword = CheckAndSetKeyword(sequence);

            if (iskeyword) TokenCollection.RemoveAt(TokenCollection.Count - 1);

            return iskeyword;
        }

        /// <summary>
        /// collects a symbol incapsulated in a backtick string and add it to the token stream as identifier
        /// </summary>
        private void CollectBacktick()
        {
            var start = CurrentIndex++;

            //consume string until EOF or closed ` is found
            while (CurrentIndex < Source.Length && Source[CurrentIndex] != '`')
                CurrentSymbol.Append(Source[CurrentIndex++]);

            var end = CurrentIndex;

            //if you found an EOF, throw
            if (CurrentIndex == Source.Length && Source[CurrentIndex - 1] != '`')
                this.Throw(CurrentIndex - 1, $"Backtick sequence has not been correctly enclosed");

            var pos = start..(end+1);

            if (CurrentSymbol.Length < 1)
                this.Throw(pos, "Not enough characters in backtick sequence");

            string sequence = CurrentSymbol.ToString().Replace(" ", "");

            if (!IsValidBackTickSequence(sequence) && !IsKeyword(sequence))
                this.Throw(pos, "Invalid backtick sequence");

            //else add closing simbol, removing whitespaces
            TokenCollection.Add(new(TokenKind.Identifier, sequence, pos));
            CurrentSymbol.Clear();
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
                case "<=": AddDouble(TokenKind.BooleanLEQ, doubleToken); break;
                case ">=": AddDouble(TokenKind.BooleanGEQ, doubleToken); break;
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
            if (CurrentIndex >= Source.Length)
                return;

            // to avoid a massive array access, also better to read
            char current = Source[CurrentIndex];

            if (IsEscapedChar(current)) // skipping it and add the symbol if it's not empty
                InsertCurrentSymbol();
            else if (IsValidIdentifierChar(current)) // adding a char to the current symbol
                CurrentSymbol.Append(current);
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
            while (CurrentIndex < Source.Length)
            {
                ProcessCurrentChar();
                CurrentIndex++;
            }



            // end of file token
            AddSingle(TokenKind.EOF, "<EOF>");

            return TokenCollection;
        }
    }
}
