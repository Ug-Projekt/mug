class AstElement
{
    public AstElement(AstElementKind ek, Ast subAst, dynamic ev)
    {
        ElementKind = ek;
        ElementBody = subAst;
        ElementValue = ev;
    }
    public AstElementKind ElementKind;
    public dynamic ElementValue;
    public Ast ElementBody;
    public static AstElement New(AstElementKind elementKind, Ast elementBody, dynamic elementValue) => new AstElement(elementKind, elementBody, elementValue);
}