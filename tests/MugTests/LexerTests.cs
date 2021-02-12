using Mug.Models.Lexer;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace MugTests
{
    public class LexerTests
    {
        private const string operation1 = "1 + 2";
        private const string variable1 = "var x = 0;";
        private const string variable2 = "var number: i32 = 5;";

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void GetLength_EmptyCollection_ReturnZero()
        {
            MugLexer lexer = new MugLexer("test", operation1);

            Assert.AreEqual(lexer.Length, 0);
        }

        [Test]
        public void GetLength_NonEmptyCollection_ReturnLength()
        {
            MugLexer lexer = new MugLexer("test", variable1);
            lexer.Tokenize();

            Assert.AreEqual(lexer.Length, 6);

            Console.WriteLine($"0: {lexer.TokenCollection[0].Value}");
            Console.WriteLine($"1: {lexer.TokenCollection[1].Value}");
            Console.WriteLine($"2: {lexer.TokenCollection[2].Value}");
            Console.WriteLine($"3: {lexer.TokenCollection[3].Value}");
            Console.WriteLine($"4: {lexer.TokenCollection[4].Value}");
            Console.WriteLine($"5: {lexer.TokenCollection[5].Value}");
        }

        public void AreListEqual(List<Token> expected, List<Token> reals)
        {
            if (reals.Count != expected.Count)
                Assert.Fail($"Assert different lenghts:\n   - expected {expected.Count} tokens\n   - found {reals.Count} tokens");

            for (int i = 0; i < reals.Count; i++)
                if (!reals[i].Equals(expected[i]))
                    Assert.Fail($"Assert different values:\n   - expected: {expected[i]}\n   - found: {reals[i]}");

            Assert.Pass();
        }

        [Test]
        public void Test01_CorrectTokenization()
        {
            MugLexer lexer = new MugLexer("test", variable1);
            lexer.Tokenize();

            List<Token> tokens = lexer.TokenCollection;

            List<Token> expected = new List<Token>
            {
                new Token(TokenKind.KeyVar, "var", 0..3),
                new Token(TokenKind.Identifier, "x", 4..5),
                new Token(TokenKind.Equal, "=", 6..7),
                new Token(TokenKind.ConstantDigit, "0", 8..9),
                new Token(TokenKind.Semicolon, ";", 9..10),
                new Token(TokenKind.EOF, "<EOF>", 10..11)
            };

            AreListEqual(expected, tokens);
        }
    }
}
