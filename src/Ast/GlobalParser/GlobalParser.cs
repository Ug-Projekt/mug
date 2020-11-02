using System.Collections.Generic;
partial class GlobalParser : Parser
{
    public static Dictionary<string, FunctionData> Functions = new Dictionary<string, FunctionData>()
    {
        { "println", new FunctionData() { Data = new Data() { Name = "println", Type = "?" }, Reference = "void [mscorlib]System.Console::WriteLine(string)", Parameters = new Data[] { new Data() { Name = "value", Type = "obj" } } } },
        { "print", new FunctionData() { Data = new Data() { Name = "print", Type = "?" }, Reference = "void [mscorlib]System.Console::Write(string)", Parameters = new Data[] { new Data() { Name = "value", Type = "obj" } } } },
        { "printf", new FunctionData() { Data = new Data() { Name = "printf", Type = "?" }, Reference = "void [mscorlib]System.Console::Write(string, object)", Parameters = new Data[] { new Data() { Name = "value", Type = "str" }, new Data() { Name = "arg", Type = "obj" } } } },
    };
    public static Dictionary<string, ClassData> Classes = new Dictionary<string, ClassData>();
    public static Dictionary<string, VariableData> Variables = new Dictionary<string, VariableData>();
    override public Ast GetAbstractSyntaxTree(SyntaxTree synT)
    {
        _syntaxTree = synT;
        while (Current.Item1 != TokenKind.ControlEndOfFile)
        {
            CheckGlobalParsable();
            Advance();
        }
        return _astBuilder.Build(); // fix
    }
}
