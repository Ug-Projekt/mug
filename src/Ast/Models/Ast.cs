using System;
class Ast {
    public Ast(AstElement[] ElementKind, int[] LineIndex) {
        this.Elements = ElementKind;
        this.LineIndex = LineIndex;
    }
    public int Length => Elements.Length;
    public AstElement[] Elements = { };
    public int[] LineIndex = { };
    public void PrintTree(string indent = "") {
        for (int i = 0; i < Length; i++) {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(indent + "(" + LineIndex[i] + ") ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(Elements[i].ElementKind);
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write(" -> ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write(Elements[i].ElementValueType + " ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(Elements[i].ElementValue);
            Console.ResetColor();
            if (Elements[i].ElementBody != null && Elements[i].ElementBody.Length != 0)
                Elements[i].ElementBody.PrintTree(indent + "    ");
        }
    }
    public Tuple<AstElement, int> this[int index] => new Tuple<AstElement, int>(Elements[index], LineIndex[index]);
}