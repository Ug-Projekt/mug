using System;
using System.Reflection;
using System.Text;

partial class Parser {
    void StoreFunction() {
        object[] obj = Objects.ToArray();
        Advance(toAdvance);
        if (Current.Item1 == TokenKind.ControlEndOfLine && Next.Item1 == TokenKind.ControlIndent) {
            var body = GetBody(Convert.ToInt16(Next.Item2));
        } else
            CompilationErrors.Add("Wrong Write Of Statement",
            "Cannot find expected token `\\n` and `\\t` in function declarating statement",
            "Insert new line after statement and an indent", GetLineFromToken(), null);
    }
}