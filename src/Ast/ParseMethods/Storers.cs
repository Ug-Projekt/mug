using System;
using System.Reflection;

partial class Parser {
    void StoreFunction() {
        Console.WriteLine("Store, function declarating: "+toAdvance);
        Console.WriteLine("\tIdentifier: "+Next.Item2);
        Advance(toAdvance);
        Console.WriteLine("\tParameters: "+Current);
        // REMAIN HERE:
        //   - PARAMETERS PRINTS ONLY FIRST/NOTHING IN CASE NO PARAMETERS ARE THERE
        //   - TODO:
        //       - TAKE FUNCTION BODY: USE 'ControlIndent'
        //       - BUILD FUNCTION PROPERTIES
        //object nElemVal = Next.Item2;
        //TokenKind cTokenKind = Current.Item1;
        //short nLineIndex = Next.Item3;
        //Advance(toAdvance);
        //Console.WriteLine(Current);
        //_astBuilder.Add(AstElement.New(AstElementKind.StatementFunctionDeclarating, null, nElemVal, cTokenKind), nLineIndex);
    }
}