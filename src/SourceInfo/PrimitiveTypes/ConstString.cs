struct ConstPrimitiveTString
{
    string value;
    public ConstPrimitiveTString(string val)
    {
        value = val;
    }
    public override string ToString() => ".str " + value.ToString();
}