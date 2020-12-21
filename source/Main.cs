using Mug;

const string test = @"func(89.1);";
var compUnit = new CompilationUnit("test.mug", test);
debug.printif(debug.askfast("Show TokenCollection"), string.Join("\n", compUnit.GetTokenCollection()));