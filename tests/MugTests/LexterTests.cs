using NUnit.Framework;
using Mug.Models.Lexer;
using System;
using System.Collections.Generic;

namespace MugTests
{
    public class Tests
    {
        private string operation1 = "1 + 2";
        private string variable1 = "var x = 0;";
        private string variable2 = "var number: i32 = 5;";

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
            Console.WriteLine("0: " + lexer.TokenCollection[0].Value);
            Console.WriteLine("1: " + lexer.TokenCollection[1].Value);
            Console.WriteLine("2: " + lexer.TokenCollection[2].Value);
            Console.WriteLine("3: " + lexer.TokenCollection[3].Value);
            Console.WriteLine("4: " + lexer.TokenCollection[4].Value);
            Console.WriteLine("5: " + lexer.TokenCollection[5].Value);
            Assert.AreEqual(lexer.Length, 6);
        }

        public void AreListEqual(List<Token> list1, List<Token> list2)
        {
            for(int i = 0; i < list1.Count; i++)
            {
                if (!list1[i].Equals(list2[i]))
                {
                    Assert.Fail("Assert different values. Expected: " + list1[1].Kind +
                        ", " + list1[i].Value + ", " + list1[i].Position +". Found: " +
                        list2[i].Kind + ", " + list2[i].Value + ", " + list2[i].Position);
                }
            }

            Assert.Pass();
        }

        [Test]
        public void Test01_CorrectTokenization()
        {
            MugLexer lexer = new MugLexer("test", variable1);
            lexer.Tokenize();
            List<Token> tokens = lexer.TokenCollection;

            List<Token> expectedTokens = new List<Token>
            {
                new Token(TokenKind.KeyVar, "var", 0..3),
                new Token(TokenKind.Identifier, "x", 3..4),
                new Token(TokenKind.Equal, "=", 5..6),
                new Token(TokenKind.KeyTi32, "0", 7..8),
                new Token(TokenKind.Colon, ";", 9..10),
                new Token(TokenKind.EOF, "<EOF>", 11..12)
            };

            AreListEqual(tokens, expectedTokens);
        }
    }
}