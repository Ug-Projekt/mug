struct ConstPrimitiveTFloat
{
    public float Value;
    public ConstPrimitiveTFloat(float val)
    {
        Value = val;
    }
    public override string ToString() => ".f32 " + Value.ToString();
}