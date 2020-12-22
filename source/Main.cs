using Mug;
using System;




if (debug.isDebug())
{
    var tests = new string[]
    {
@"var i: u32 = 0;
i++;
i--;
i+=(1+1)-2;"
    };
    for (int i = 0; i < tests.Length; i++)
    {
        debug.printc(ConsoleColor.DarkGreen, "Test: ", i.ToString());
        var compUnit = new CompilationUnit("test"+i+".mug", tests[i]);
        var tokens = compUnit.GetTokenCollection();
        debug.print(string.Join("\n", tokens));
        debug.readfast("Press to next test");
        Console.Clear();
    }
}