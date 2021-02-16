using Mug.Models.Lexer;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace MugTests
{
    public class LexerTests
    {
        /*
         * TODO:
         *  - add tests for chars
         */

        // Well constructed code strings
        private const string OPERATION01 = "1 + 2";
        private const string VARIABLE01 = "var x = 0;";
        private const string VARIABLE02 = "var number: i32 = 50;";

        private const string COMMENTS01 = "# This is a comment";
        private const string COMMENTS02 = "#[ This is a  multi-line comment ]#";

        private const string SINGLE_TOKENS = "( ) [ ] { } < > = ! & | + - * / , ; : . @ ?";
        private const string DOUBLE_TOKENS = "== != ++ += -- -= *= /= <= >= ..";
        private const string FULL_TOKENS = "return continue break while pub use import new for type as in to if elif else func var const str chr u1 i8 i32 i64 u8 u32 u64 unknown";
        private const string RANDOM_TOKENS = "return == ( ) += continue pub ! *= ..";


        private const string STRINGS01 = "\"This is a string\"";

        //Ill-constructed code strings
        private const string EMPTYSTRING = "";

        private const string STRINGS02 = "\"This is a non-closed string";
        private const string STRINGS03 = "\"This is a \" nested \"string\"";
        private const string STRINGS04 = "\"\\n\\t\\r\\\"\"";
        private const string STRINGS05 = "u1\"\\\\ \"t token";
        private const string STRINGS06 = "\"";

        private const string CHARS01 = "'c'";
        private const string CHARS02 = "'c";
        private const string CHARS03 = "'cc'";
        private const string CHARS04 = "'\\\\'";
        private const string CHARS05 = "'\\\\\\\\'";

        private const string VARIABLE03 = ";50 = i32 :number var";
        private const string VARIABLE04 = "varnumber";
        private const string VARIABLE05 = "i33";

        private const string COMMENTS03 = "#[ This is a non closed multi-line comment";
        private const string COMMENTS04 = "#[ This is a nested ]# multi-line comment ]#";
        private const string COMMENTS05 = "#[ This is a #[ nested ]# multi-line comment ]#";

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
            {
                Console.WriteLine("expected contained:");
                for (int i = 0; i < expected.Count; i++)
                    Console.WriteLine($"i:{i}, contained:{expected[i]}");
                Console.WriteLine("reals contained:");
                for (int i = 0; i < reals.Count; i++)
                    Console.WriteLine($"i:{i}, contained:{reals[i]}");
                Assert.Fail($"Assert different lenghts:\n   - expected {expected.Count} tokens\n   - found {reals.Count} tokens");
            }

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
        public void TestComments03_CorrectTokenization()
        {
            // A comments gets consumed, turning it into an empty string
            MugLexer lexer = new MugLexer("test", COMMENTS03);
            lexer.Tokenize();

            List<Token> tokens = lexer.TokenCollection;

            List<Token> expected = new List<Token>
            {
                new Token(TokenKind.EOF, "<EOF>", 43..44)
            };

            AreListEqual(expected, tokens);
        }

        [Test]
        public void TestComments04_CorrectTokenization()
        {
            // A comments gets consumed, turning it into an empty string
            MugLexer lexer = new MugLexer("test", COMMENTS04);
            lexer.Tokenize();

            List<Token> tokens = lexer.TokenCollection;

            List<Token> expected = new List<Token>
            {
                new Token(TokenKind.Identifier, "multi", 23..28),
                new Token(TokenKind.Minus, "-", 28..29),
                new Token(TokenKind.Identifier, "line", 29..33),
                new Token(TokenKind.Identifier, "comment", 34..41),
                new Token(TokenKind.CloseBracket, "]", 42..43),
                new Token(TokenKind.EOF, "<EOF>", 45..46)
            };

            AreListEqual(expected, tokens);
        }

        [Test]
        public void TestComments05_CorrectTokenization()
        {
            // A comments gets consumed, turning it into an empty string
            MugLexer lexer = new MugLexer("test", COMMENTS05);
            lexer.Tokenize();

            List<Token> tokens = lexer.TokenCollection;

            List<Token> expected = new List<Token>
            {
                new Token(TokenKind.Identifier, "multi", 26..31),
                new Token(TokenKind.Minus, "-", 31..32),
                new Token(TokenKind.Identifier, "line", 32..36),
                new Token(TokenKind.Identifier, "comment", 37..44),
                new Token(TokenKind.CloseBracket, "]", 45..46),
                new Token(TokenKind.EOF, "<EOF>", 48..49)
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

        [Test]
        public void TestStrings01_CorrectTokenization()
        {
            MugLexer lexer = new MugLexer("test", STRINGS01);
            lexer.Tokenize();

            List<Token> tokens = lexer.TokenCollection;

            List<Token> expected = new List<Token>
            {
                new Token(TokenKind.ConstantString, "This is a string", 0..18),
                new Token(TokenKind.EOF, "<EOF>", 18..19)
            };

            AreListEqual(expected, tokens);
        }

        [Test]
        public void TestStrings02_ExceptionCaught()
        {
            MugLexer lexer = new MugLexer("test", STRINGS02);
            var ex = Assert.Throws<Mug.Compilation.CompilationException>(() => lexer.Tokenize());

            Assert.AreEqual("String has not been correctly enclosed", ex.Message);
        }

        [Test]
        public void TestStrings03_CorrectTokenization()
        {
            MugLexer lexer = new MugLexer("test", STRINGS03);
            lexer.Tokenize();

            List<Token> tokens = lexer.TokenCollection;

            List<Token> expected = new List<Token>
            {
                new Token(TokenKind.ConstantString, "This is a ", 0..12),
                new Token(TokenKind.Identifier, "nested", 13..19),
                new Token(TokenKind.ConstantString, "string", 20..28),
                new Token(TokenKind.EOF, "<EOF>", 28..29)
            };

            AreListEqual(expected, tokens);
        }

        [Test]
        public void TestStrings04_EscapedChars()
        {
            MugLexer lexer = new MugLexer("test", STRINGS04);
            lexer.Tokenize();

            List<Token> tokens = lexer.TokenCollection;

            List<Token> expected = new List<Token>
            {
                new Token(TokenKind.ConstantString, "\n\t\r\"", 0..10),
                new Token(TokenKind.EOF, "<EOF>", 10..11)
            };

            AreListEqual(expected, tokens);
        }

        [Test]
        public void TestStrings05_AdvancedEscapedChars()
        {
            MugLexer lexer = new MugLexer("test", STRINGS05);
            lexer.Tokenize();
            List<Token> tokens = lexer.TokenCollection;

            List<Token> expected = new List<Token>
            {
                new Token(TokenKind.KeyTbool, "u1", 0..2),
                new Token(TokenKind.ConstantString, "\\ ", 2..7),
                new Token(TokenKind.Identifier, "t", 7..8),
                new Token(TokenKind.Identifier, "token", 9..14),
                new Token(TokenKind.EOF, "<EOF>", 14..15)
            };

            AreListEqual(expected, tokens);
        }

        [Test]
        public void TestStrings06_ExceptionCaught()
        {
            MugLexer lexer = new MugLexer("test", STRINGS06);
            var ex = Assert.Throws<Mug.Compilation.CompilationException>(() => lexer.Tokenize());

            Assert.AreEqual("In the current context, this is not a valid char", ex.Message);
        }

        [Test]
        public void TestSingleTokens_CorrectTokenization()
        {
            MugLexer lexer = new MugLexer("test", SINGLE_TOKENS);
            lexer.Tokenize();

            List<Token> tokens = lexer.TokenCollection;

            List<Token> expected = new List<Token>
            {
                new Token(TokenKind.OpenPar, "(", 0..1),
                new Token(TokenKind.ClosePar, ")", 2..3),
                new Token(TokenKind.OpenBracket, "[", 4..5),
                new Token(TokenKind.CloseBracket, "]", 6..7),
                new Token(TokenKind.OpenBrace, "{", 8..9),
                new Token(TokenKind.CloseBrace, "}", 10..11),
                new Token(TokenKind.BooleanMinor, "<", 12..13),
                new Token(TokenKind.BooleanMajor, ">", 14..15),
                new Token(TokenKind.Equal, "=", 16..17),
                new Token(TokenKind.Negation, "!", 18..19),
                new Token(TokenKind.BooleanAND, "&", 20..21),
                new Token(TokenKind.BooleanOR, "|", 22..23),
                new Token(TokenKind.Plus, "+", 24..25),
                new Token(TokenKind.Minus, "-", 26..27),
                new Token(TokenKind.Star, "*", 28..29),
                new Token(TokenKind.Slash, "/", 30..31),
                new Token(TokenKind.Comma, ",", 32..33),
                new Token(TokenKind.Semicolon, ";", 34..35),
                new Token(TokenKind.Colon, ":", 36..37),
                new Token(TokenKind.Dot, ".", 38..39),
                new Token(TokenKind.DirectiveSymbol, "@", 40..41),
                new Token(TokenKind.KeyTVoid, "?", 42..43),
                new Token(TokenKind.EOF, "<EOF>", 43..44)
            };

            AreListEqual(expected, tokens);
        }

        [Test]
        public void TestDoubleTokens_CorrectTokenization()
        {
            MugLexer lexer = new MugLexer("test", DOUBLE_TOKENS);
            lexer.Tokenize();

            List<Token> tokens = lexer.TokenCollection;

            List<Token> expected = new List<Token>
            {
                new Token(TokenKind.BooleanEQ, "==", 0..2),
                new Token(TokenKind.BooleanNEQ, "!=", 3..5),
                new Token(TokenKind.OperatorIncrement, "++", 6..8),
                new Token(TokenKind.AddAssignment, "+=", 9..11),
                new Token(TokenKind.OperatorDecrement, "--", 12..14),
                new Token(TokenKind.SubAssignment, "-=", 15..17),
                new Token(TokenKind.MulAssignment, "*=", 18..20),
                new Token(TokenKind.DivAssignment, "/=", 21..23),
                new Token(TokenKind.BooleanMinEQ, "<=", 24..26),
                new Token(TokenKind.BooleanMajEQ, ">=", 27..29),
                new Token(TokenKind.RangeDots, "..", 30..32),
                new Token(TokenKind.EOF, "<EOF>", 32..33)
            };

            AreListEqual(expected, tokens);
        }

        [Test]
        public void TestFullTokens_CorrectTokenization()
        {
            MugLexer lexer = new MugLexer("test", FULL_TOKENS);
            lexer.Tokenize();

            List<Token> tokens = lexer.TokenCollection;

            List<Token> expected = new List<Token>
            {
                new Token(TokenKind.KeyReturn, "return", 0..6),
                new Token(TokenKind.KeyContinue, "continue", 7..15),
                new Token(TokenKind.KeyBreak, "break", 16..21),
                new Token(TokenKind.KeyWhile, "while", 22..27),
                new Token(TokenKind.KeyPub, "pub", 28..31),
                new Token(TokenKind.KeyUse, "use", 32..35),
                new Token(TokenKind.KeyImport, "import", 36..42),
                new Token(TokenKind.KeyNew, "new", 43..46),
                new Token(TokenKind.KeyFor, "for", 47..50),
                new Token(TokenKind.KeyType, "type", 51..55),
                new Token(TokenKind.KeyAs, "as", 56..58),
                new Token(TokenKind.KeyIn, "in", 59..61),
                new Token(TokenKind.KeyTo, "to", 62..64),
                new Token(TokenKind.KeyIf, "if", 65..67),
                new Token(TokenKind.KeyElif, "elif", 68..72),
                new Token(TokenKind.KeyElse, "else", 73..77),
                new Token(TokenKind.KeyFunc, "func", 78..82),
                new Token(TokenKind.KeyVar, "var", 83..86),
                new Token(TokenKind.KeyConst, "const", 87..92),
                new Token(TokenKind.KeyTstr, "str", 93..96),
                new Token(TokenKind.KeyTchr, "chr", 97..100),
                new Token(TokenKind.KeyTbool, "u1", 101..103),
                new Token(TokenKind.KeyTi8, "i8", 104..106),
                new Token(TokenKind.KeyTi32, "i32", 107..110),
                new Token(TokenKind.KeyTi64, "i64", 111..114),
                new Token(TokenKind.KeyTu8, "u8", 115..117),
                new Token(TokenKind.KeyTu32, "u32", 118..121),
                new Token(TokenKind.KeyTu64, "u64", 122..125),
                new Token(TokenKind.KeyTunknown, "unknown", 126..133),
                new Token(TokenKind.EOF, "<EOF>", 133..134)
            };

            AreListEqual(expected, tokens);
        }

        [Test]
        public void TestRandomTokens_CorrectTokenization()
        {
            MugLexer lexer = new MugLexer("test", RANDOM_TOKENS);
            lexer.Tokenize();

            List<Token> tokens = lexer.TokenCollection;

            List<Token> expected = new List<Token>
            {
                new Token(TokenKind.KeyReturn, "return", 0..6),
                new Token(TokenKind.BooleanEQ, "==", 7..9),
                new Token(TokenKind.OpenPar, "(", 10..11),
                new Token(TokenKind.ClosePar, ")", 12..13),
                new Token(TokenKind.AddAssignment, "+=", 14..16),
                new Token(TokenKind.KeyContinue, "continue", 17..25),
                new Token(TokenKind.KeyPub, "pub", 26..29),
                new Token(TokenKind.Negation, "!", 30..31),
                new Token(TokenKind.MulAssignment, "*=", 32..34),
                new Token(TokenKind.RangeDots, "..", 35..37),
                new Token(TokenKind.EOF, "<EOF>", 37..38)
            };

            AreListEqual(expected, tokens);
        }

        [Test]
        public void TestChars01_OneChar()
        {
            MugLexer lexer = new MugLexer("test", CHARS01);
            lexer.Tokenize();

            List<Token> tokens = lexer.TokenCollection;

            List<Token> expected = new List<Token>
            {
                new Token(TokenKind.ConstantChar, "c", 0..3),
                new Token(TokenKind.EOF, "<EOF>", 3..4)
            };

            AreListEqual(expected, tokens);
        }

        [Test]
        public void TestChars02_OneChar()
        {
            MugLexer lexer = new MugLexer("test", CHARS02);
            var ex = Assert.Throws<Mug.Compilation.CompilationException>(() => lexer.Tokenize());

            Assert.AreEqual("Char has not been correctly enclosed", ex.Message);
        }

        [Test]
        public void TestChars03_TooManyChars()
        {
            MugLexer lexer = new MugLexer("test", CHARS03);
            var ex = Assert.Throws<Mug.Compilation.CompilationException>(() => lexer.Tokenize());

            Assert.AreEqual("Too many characters in const char", ex.Message);
        }

        [Test]
        public void TestChars04_OneEscapedChar()
        {
            MugLexer lexer = new MugLexer("test", CHARS04);
            lexer.Tokenize();

            List<Token> tokens = lexer.TokenCollection;

            List<Token> expected = new List<Token>
            {
                new Token(TokenKind.ConstantChar, "\\", 0..4),
                new Token(TokenKind.EOF, "<EOF>", 4..5)
            };

            AreListEqual(expected, tokens);
        }

        [Test]
        public void TestChars05_TooManyEscapedChars()
        {
            MugLexer lexer = new MugLexer("test", CHARS05);
            var ex = Assert.Throws<Mug.Compilation.CompilationException>(() => lexer.Tokenize());

            Assert.AreEqual("Too many characters in const char", ex.Message);
        }
    }
}
