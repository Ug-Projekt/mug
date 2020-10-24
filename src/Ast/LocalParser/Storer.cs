partial class LocalParser : Parser
{
    void StoreFunctionCalling()
    {
        //Console.WriteLine("Calling Function: "+Current);
        //Console.WriteLine(Objects["func"]);
        //Console.WriteLine(string.Join("________\n", Objects["params"]));
        toAdvance += 1;
        Advance(toAdvance);
        toAdvance = 0;
    }
}