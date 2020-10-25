using System;
partial class LocalParser : Parser
{
    void StoreFunctionCalling()
    {
        //Console.WriteLine("Expression Tree:");
        //Objects["params"].PrintTree();
        //Console.WriteLine("Function Name: "+Objects["func"].Name);
        _astBuilder.Add(
            AstElement.New(
                AstElementKind.StatementCallingFunction,
                Objects["params"],
                Objects["func"].Name
            ),
            GetLineFromToken()
        );

        toAdvance += 1;
        Advance(toAdvance);
        toAdvance = 0;
    }
}