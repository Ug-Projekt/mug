struct ConstPrimitiveTInt
{
    public int value;
    public ConstPrimitiveTInt(int val)
    {
        value = val;
    }
    public override string ToString() => ".i32 " + value.ToString();
}