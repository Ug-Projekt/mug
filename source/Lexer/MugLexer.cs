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
        string CurrentSymbol = "";
        int CurrentIndex = 0;
        int CurrentLine = 0;
        public MugLexer(string moduleName, string source)
        {
            ModuleName = moduleName;
            Source = source;
        }
        bool AddKeyword(TokenKind kind, int len)
        {
            TokenCollection.Add(new (CurrentLine, kind, null, new (CurrentIndex-len, CurrentIndex)));
            return true;
        }
        bool GetKeyword(string s) => s switch
        {
            "func" => AddKeyword(TokenKind.KeyFunc, s.Length),
            "var" => AddKeyword(TokenKind.KeyVar, s.Length),
            "const" => AddKeyword(TokenKind.KeyConst, s.Length),
            _ => false
        };
        TokenKind IllegalChar()
        {
            this.Throw(CurrentIndex, CurrentLine, "Found illegal SpecialSymbol: mug's syntax does not use this character");
            return TokenKind.Unknow;
        }
        TokenKind GetSpecial(char c) => c switch
        {
            '(' => TokenKind.OpenPar,
            ')' => TokenKind.ClosePar,
            '[' => TokenKind.OpenBracket,
            ']' => TokenKind.CloseBracket,
            '{' => TokenKind.OpenBrace,
            '}' => TokenKind.CloseBrace,
            ',' => TokenKind.CloseBrace,
            ';' => TokenKind.Semicolon,
            ':' => TokenKind.Colon,
            '?' => TokenKind.KeyVoid,
            _ => IllegalChar()
        };

        void AddToken(TokenKind kind, object value)
        {
            TokenCollection.Add(new(CurrentLine, kind, value, new(CurrentIndex - value.ToString().Length, CurrentIndex)));
        }
        void AddSpecial(TokenKind kind)
        {
            InsertCurrentSymbol();
            TokenCollection.Add(new (CurrentLine, kind, null, new(CurrentIndex, CurrentIndex+1)));
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
            if (string.IsNullOrWhiteSpace(s))
                return false;
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
            return GetKeyword(s);
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
                InsertCurrentSymbol();
            else if (char.IsLetterOrDigit(current) || current == '_')
                CurrentSymbol += current;
            else if (IsDigit(CurrentSymbol) && current == '.')
                CurrentSymbol += '.';
            else
                AddSpecial(GetSpecial(current));
        }
        public List<Token> Tokenize()
        {
            if (TokenCollection is not null)
                return TokenCollection;
            TokenCollection = new ();
            do
                ProcessChar(Source[CurrentIndex]);
            while (CurrentIndex++ < Source.Length-1);
            AddSpecial(TokenKind.EOF);
            this.Throw(TokenCollection[1], "This is a fake error");
            return TokenCollection;
        }
    }
}
