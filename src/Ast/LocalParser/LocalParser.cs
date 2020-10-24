partial class LocalParser : Parser
{
    public override Ast GetAbstractSyntaxTree(SyntaxTree synT)
    {
        _syntaxTree = synT;
        while (TokenIndex < synT.Count)
        {
            CheckLocalParsable();
            Advance();
        }
        return _astBuilder.Build();
    }
}