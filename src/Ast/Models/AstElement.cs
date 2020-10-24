class AstElement
{
    public AstElement(AstElementKind ek, Ast subAst, dynamic ev, TokenKind evt)
    {
        ElementKind = ek;
        ElementBody = subAst;
        ElementValue = ev;
        ElementValueType = evt;
    }
    public AstElementKind ElementKind;
    public dynamic ElementValue;
    public TokenKind ElementValueType;
    public Ast ElementBody;
    public void BuildAst(Ast ast) => ElementBody = ast;
    public static AstElement New(AstElementKind elementKind, Ast elementBody, dynamic elementValue, TokenKind elementValueType) => new AstElement(elementKind, elementBody, elementValue, elementValueType);
}