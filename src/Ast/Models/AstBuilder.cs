using System.Collections.Generic;
class AstBuilder
{
    List<AstElement> ElementKind = new List<AstElement> { };
    List<int> LineIndex = new List<int>();
    public void Add(AstElement astElement, int lineIndex)
    {
        ElementKind.Add(astElement);
        LineIndex.Add(lineIndex);
    }
    public void Clear()
    {
        ElementKind.Clear();
        LineIndex.Clear();
    }
    public bool IsEmpty() => ElementKind.Count == 0 && LineIndex.Count == 0;
    public Ast Build() => new Ast(ElementKind.ToArray(), LineIndex.ToArray());
}