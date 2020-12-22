using Mug;
using System;




if (debug.isDebug())
{
    var tests = new string[]
    {
@"func main(): ? {
    println('cc');
}"
    };
    for (int i = 0; i < tests.Length; i++)
    {
        debug.printc(ConsoleColor.DarkGreen, "Test: ", i.ToString());
        var compUnit = new CompilationUnit("test"+i+".mug", tests[i]);
        var tokens = string.Join("\n", compUnit.GetTokenCollection());
        debug.print(tokens);
        debug.readfast("Press to next test");
        Console.Clear();
    }
}