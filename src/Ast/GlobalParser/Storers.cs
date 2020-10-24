using System;
partial class GlobalParser
{
    void StoreGlobalFunction()
    {
        //Console.WriteLine("Function: "+Objects["func"]);
        //Console.WriteLine("Parameters: " + string.Join(" ", Objects["params"]));
        Advance(toAdvance);
        toAdvance = 0;
        var bodySyntaxBuilder = new SyntaxTreeBuilder();
        short considerationBrace = -1;
        //Console.WriteLine("Current: "+Current);
        while (Current.Item1 != TokenKind.ControlEndOfFile)
        {
            if (Current.Item1 == TokenKind.SymbolOpenBrace)
                considerationBrace++;
            else if (Current.Item1 == TokenKind.SymbolCloseBrace)
                considerationBrace--;
            if (Current.Item1 != TokenKind.SymbolCloseBrace && considerationBrace != 0)
                break;
            bodySyntaxBuilder.Add(Current);
            Advance();
        }
        //Console.WriteLine("BodySyntaxTree:");
        //Console.WriteLine("Count: "+bodySyntaxBuilder.Count);
        bodySyntaxBuilder.Remove(0);
        bodySyntaxBuilder.Remove(bodySyntaxBuilder.Count - 1);
        var body = bodySyntaxBuilder.Build();
        //body.PrintTree();
        var function = new FunctionData() { Body = new LocalParser().GetAbstractSyntaxTree(body), Data = new Data { Name = Objects["func"].Name, Type = Objects["func"].Type } };
        if (!Functions.TryAdd(Objects["func"].Name, function))
        {
            CompilationErrors.Add(
                "Function Already Declared",
                "Declarated a function, you can write an overload of that function, but not redeclare it",
                "Change parameters to make the function different to its already declared similar", GetLineFromToken(), null);
        }
        //Console.WriteLine("end of function declaration statement: " + Current);
    }
}