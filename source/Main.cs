using Mug;
using System;




if (debug.isDebug())
{
    var tests = new string[] {
@"
func main(): ? {
    println(""this is a string"", 0.1);
}",
@"
func syntax_error ."
};
    for (int i = 0; i < tests.Length; i++)
    {
        debug.print("Test: ", i.ToString());
        var compUnit = new CompilationUnit("test.mug", tests[i]);
        var tokens = string.Join("\n", compUnit.GetTokenCollection());
        debug.printif(debug.askfast("Show TokenCollection"), tokens);
    }
}