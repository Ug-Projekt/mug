using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;

namespace Mug.Models.Lexer
{
    public class MugLexer
    {
        public string Source;
        public string ModuleName;
        public List<Token> TokenCollection;
        string CurrentSymbol;
        int CurrentIndex;
        int CurrentLine;
        public MugLexer(string moduleName, string source)
        {
            ModuleName = moduleName;
            Source = source;
        }
        void AddToken(TokenKind kind, object value)
        {
            TokenCollection.Add(new (CurrentLine, kind, value, new (CurrentIndex, CurrentIndex+value.ToString().Length)));
        }
        void AddSpecial(TokenKind kind)
        {
            InsertCurrentSymbol();
            TokenCollection.Add(new (CurrentLine, kind, null, new(CurrentIndex, CurrentIndex+1)));
        }
        TokenKind IllegalChar()
        {
            this.Throw(CurrentIndex, CurrentLine, "Found illegal SpecialSymbol: mug's syntax does not use this character");
            return TokenKind.Unknow;
        }
        void InsertCurrentSymbol()
        {
            if (!string.IsNullOrWhiteSpace(CurrentSymbol))
            {
                ProcessSymbol(CurrentSymbol);
                CurrentSymbol = "";
            }
        }
        bool IsDigit(string s)
        {
            for (int i = 0; i < s.Length; i++)
                if (!char.IsDigit(s[i]) && s[i] != '.')
                    return false;
            return true;
        }
        bool IsString(string s) => s[0] == '"' && s[^1] == '"';
        bool IsChar(string s)
        {
            if (s[0] != '\'' || s[^1] != '\'')
                return false;
            if (s.Length != 3)
                this.Throw(CurrentIndex-s.Length, CurrentLine, "Character constant cannot contains more than 1 value");
            return true;
        }
        bool InsertKeyword(string s)
        {
            bool Add(TokenKind kind) {
                TokenCollection.Add(new (CurrentLine, kind, null, new(CurrentIndex, CurrentIndex + 1)));
                return true;
            }
            return s switch {
                "func" => Add(TokenKind.KeyFunc),
                "var" => Add(TokenKind.KeyVar),
                "const" => Add(TokenKind.KeyConst),
                _ => false
            };
        }
        void ProcessSymbol(string value)
        {
            if (IsDigit(value))
                AddToken(TokenKind.ConstantDigit, value);
            else if (IsString(value))
                AddToken(TokenKind.ConstantString, value);
            else if (IsChar(value))
                AddToken(TokenKind.ConstantChar, value);
            else if (!InsertKeyword(value))
                AddToken(TokenKind.Identifier, value);
        }
        void ProcessChar(char current)
        {
            if (current == '"')
            {
                do
                    CurrentSymbol += Source[CurrentIndex];
                while (CurrentIndex++ < Source.Length - 1 && Source[CurrentIndex] != '"');
                ProcessSymbol(CurrentSymbol+'"');
                CurrentSymbol = "";
                return;
            }
            else if (current == '\n')
                CurrentLine++;
            if (char.IsControl(current) || char.IsWhiteSpace(current))
                return;
            if (char.IsLetterOrDigit(current) || current == '_')
                CurrentSymbol += current;
            else if (IsDigit(CurrentSymbol) && current == '.')
                CurrentSymbol += '.';
            else
                AddSpecial(current switch {
                    '(' => TokenKind.OpenPar,
                    ')' => TokenKind.ClosePar,
                    ';' => TokenKind.Semicolon,
                    _ => IllegalChar()
                });
        }
        public List<Token> Tokenize()
        {
            if (TokenCollection is not null)
                return TokenCollection;
            TokenCollection = new ();
            do
                ProcessChar(Source[CurrentIndex]);
            while (CurrentIndex++ < Source.Length-1);
            InsertCurrentSymbol();
            return TokenCollection;
        }
    }
}
