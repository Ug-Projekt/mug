struct ConstPrimitiveTBool
{
    public bool value;
    public ConstPrimitiveTBool(bool val)
    {
        value = val;
    }
    public override string ToString() => ".bool " + value.ToString();
}