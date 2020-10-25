struct ConstPrimitiveTBool
{
    public bool Value;
    public ConstPrimitiveTBool(bool val)
    {
        Value = val;
    }
    public override string ToString() => ".bool " + Value.ToString();
}