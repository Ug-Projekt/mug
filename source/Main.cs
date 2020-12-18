using Mug;
using Mug.Models.Lexer;
using System;
CompilationErrors.Lexer = new MugLexer("riga completa");
var tok = new Token(0, TokenKind.ConstantString, "...", new(1, 3));
CompilationErrors.Throw(ref tok, "Error");

const string test = @"

";
var compUnit = new CompilationUnit("", test);
//debug.printif(debug.askfast('y', "Show TokenCollection"), "...");
