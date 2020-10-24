struct ConstPrimitiveTFloat
{
    float value;
    public ConstPrimitiveTFloat(float val)
    {
        value = val;
    }
    public override string ToString() => ".f32 "+value.ToString();
}