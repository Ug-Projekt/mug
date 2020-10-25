struct ConstPrimitiveTInt
{
    public int Value;
    public ConstPrimitiveTInt(int val)
    {
        Value = val;
    }
    public override string ToString() => ".i32 " + Value.ToString();
}