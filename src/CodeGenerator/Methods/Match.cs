partial class CodeGenerator
{
    void MatchStatements()
    {
        if (Current.Item1.ElementKind == AstElementKind.StatementCallingFunction)
            StoreCallFunctionStatement();
    }
}