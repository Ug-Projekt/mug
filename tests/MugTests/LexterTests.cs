using NUnit.Framework;
using Mug.Models.Lexer;
using System;

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
    }
}