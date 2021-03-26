﻿using Mug.Models.Lexer;
using System;
using System.Collections.Generic;

namespace Mug.Models.Parser
{
    public class Pragmas
    {
        private readonly Dictionary<string, Token> _table = new()
        {
            ["inline"] = Token.NewInfo(TokenKind.ConstantBoolean, "false"),
            ["header"] = Token.NewInfo(TokenKind.ConstantString, ""),
            ["dynamiclib"] = Token.NewInfo(TokenKind.ConstantString, ""),
            ["export"] = Token.NewInfo(TokenKind.ConstantString, ""),
            ["extern"] = Token.NewInfo(TokenKind.ConstantString, ""),
            ["code"] = Token.NewInfo(TokenKind.ConstantString, "")
        };

        public string GetPragma(string pragma)
        {
            return _table[pragma].Value;
        }

        private void SetWithCheck(string pragma, string symbol)
        {
            if (_table[pragma].Value == "")
                _table[pragma] = Token.NewInfo(TokenKind.ConstantString, symbol);
        }

        public void SetName(string symbol)
        {
            SetWithCheck("export", symbol);
        }

        public void SetExtern(string symbol)
        {
            SetWithCheck("extern", symbol);
        }

        public void SetPragma(string pragma, Token value, Action<string[]> error, ref int currentIndex)
        {
            if (!_table.ContainsKey(pragma))
            {
                // going back to identifier token id(.)]
                currentIndex -= 4;
                error(new[] { "Unknown pragma" });
            }

            _table[pragma] = value;
        }

        public void SetPragma(string pragma, Token value)
        {
            _table[pragma] = value;
        }
    }
}
