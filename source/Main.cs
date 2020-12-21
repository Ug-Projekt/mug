using Mug;

const string test = @"func;";
var compUnit = new CompilationUnit("test.mug", test);
debug.printif(debug.askfast("Show TokenCollection"), string.Join("\n", compUnit.GetTokenCollection()));