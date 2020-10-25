struct ConstPrimitiveTString
{
    public string Value;
    public ConstPrimitiveTString(string val)
    {
        Value = val;
    }
    public override string ToString() => ".str " + Value.ToString();
}