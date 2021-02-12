using Mug.Models.Lexer;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace MugTests
{
    public class LexerTests
    {
        // Well constructed code strings
        private const string OPERATION01 = "1 + 2";
        private const string VARIABLE01 = "var x = 0;";
        private const string VARIABLE02 = "var number: i32 = 50;";

        private const string COMMENTS01 = "# This is a comment";
        private const string COMMENTS02 = "#[ This is a  multi-line comment ]#";

        //Ill-constructed code strings
        private const string EMPTYSTRING = "";
        private const string VARIABLE03 = ";50 = i32 :number var";
        private const string VARIABLE04 = "varnumber";
        private const string VARIABLE05 = "i33";

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void GetLength_EmptyCollection_ReturnZero()
        {
            MugLexer lexer = new MugLexer("test", OPERATION01);

            Assert.AreEqual(lexer.Length, 0);
        }

        [Test]
        public void GetLength_NonEmptyCollection_ReturnLength()
        {
            MugLexer lexer = new MugLexer("test", VARIABLE01);
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
            MugLexer lexer = new MugLexer("test", VARIABLE01);
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

        [Test]
        public void Test02_CorrectTokenization()
        {
            MugLexer lexer = new MugLexer("test", VARIABLE02);
            lexer.Tokenize();

            List<Token> tokens = lexer.TokenCollection;

            List<Token> expected = new List<Token>
            {
                new Token(TokenKind.KeyVar, "var", 0..3),
                new Token(TokenKind.Identifier, "number", 4..10),
                new Token(TokenKind.Colon, ":", 10..11),
                new Token(TokenKind.KeyTi32, "i32", 12..15),
                new Token(TokenKind.Equal, "=", 16..17),
                new Token(TokenKind.ConstantDigit, "50", 18..20),
                new Token(TokenKind.Semicolon, ";", 20..21),
                new Token(TokenKind.EOF, "<EOF>", 21..22)
            };

            AreListEqual(expected, tokens);
        }

        [Test]
        public void Test03_CorrectTokenization()
        {
            MugLexer lexer = new MugLexer("test", VARIABLE03);
            lexer.Tokenize();

            List<Token> tokens = lexer.TokenCollection;

            List<Token> expected = new List<Token>
            {
                new Token(TokenKind.Semicolon, ";", 0..1),
                new Token(TokenKind.ConstantDigit, "50", 1..3),
                new Token(TokenKind.Equal, "=", 4..5),
                new Token(TokenKind.KeyTi32, "i32", 6..9),
                new Token(TokenKind.Colon, ":", 10..11),
                new Token(TokenKind.Identifier, "number", 11..17),
                new Token(TokenKind.KeyVar, "var", 18..21),
                new Token(TokenKind.EOF, "<EOF>", 21..22)
            };

            AreListEqual(expected, tokens);
        }

        [Test]
        public void Test04_CorrectTokenization()
        {
            MugLexer lexer = new MugLexer("test", VARIABLE04);
            lexer.Tokenize();

            List<Token> tokens = lexer.TokenCollection;

            List<Token> expected = new List<Token>
            {
                new Token(TokenKind.Identifier, "varnumber", 0..9),
                new Token(TokenKind.EOF, "<EOF>", 9..10)
            };

            AreListEqual(expected, tokens);
        }

        [Test]
        public void Test05_CorrectTokenization()
        {
            MugLexer lexer = new MugLexer("test", VARIABLE05);
            lexer.Tokenize();

            List<Token> tokens = lexer.TokenCollection;

            List<Token> expected = new List<Token>
            {
                new Token(TokenKind.Identifier, "i33", 0..3),
                new Token(TokenKind.EOF, "<EOF>", 3..4)
            };

            AreListEqual(expected, tokens);
        }

        [Test]
        public void TestComments01_CorrectTokenization()
        {
            // A comments gets consumed, turning it into an empty string
            MugLexer lexer = new MugLexer("test", COMMENTS01);
            lexer.Tokenize();

            List<Token> tokens = lexer.TokenCollection;

            List<Token> expected = new List<Token>
            {
                new Token(TokenKind.EOF, "<EOF>", 20..21)
            };

            AreListEqual(expected, tokens);
        }

        [Test]
        public void TestComments02_CorrectTokenization()
        {
            // A comments gets consumed, turning it into an empty string
            MugLexer lexer = new MugLexer("test", COMMENTS02);
            lexer.Tokenize();

            List<Token> tokens = lexer.TokenCollection;

            List<Token> expected = new List<Token>
            {
                new Token(TokenKind.EOF, "<EOF>", 36..37)
            };

            AreListEqual(expected, tokens);
        }

        [Test]
        public void EmptyString_CorrectTokenization()
        {
            // An empty string gets converted into an <EOF>
            MugLexer lexer = new MugLexer("test", EMPTYSTRING);
            lexer.Tokenize();

            List<Token> tokens = lexer.TokenCollection;

            List<Token> expected = new List<Token>
            {
                new Token(TokenKind.EOF, "<EOF>", 0..1)
            };

            AreListEqual(expected, tokens);
        }
    }
}
