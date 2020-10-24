using System;
using System.Collections.Generic;
partial class GlobalParser : Parser {
    public static Dictionary<string, FunctionData> Functions = new Dictionary<string, FunctionData>();
    public static Dictionary<string, ClassData> Classes = new Dictionary<string, ClassData>();
    public static Dictionary<string, VariableData> Variables = new Dictionary<string, VariableData>();
    override public Ast GetAbstractSyntaxTree(SyntaxTree synT) {
        _syntaxTree = synT;
        while (Current.Item1 != TokenKind.ControlEndOfFile) {
            CheckGlobalParsable();
            Advance();
        }
        return _astBuilder.Build();
    }
}
