partial class LocalParser : Parser
{
    public override Ast GetAbstractSyntaxTree(TokenCollection synT)
    {
        _syntaxTree = synT;
        while (TokenIndex < synT.Count)
        {
            ParseLocal();
            Advance();
        }
        return _astBuilder.Build();
    }
}