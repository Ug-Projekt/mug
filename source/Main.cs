using Mug;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using Mug.Models.Parser.NodeKinds.Statements;
using System;

// TOY EVALUATOR (don't work sum and sub expressions)
//int getValue(INode unknowExpression)
//{
//    if (unknowExpression is ExpressionNode e)
//        return evalExpression(e);
//    if (unknowExpression is ValueNode v)
//        return int.Parse(v.SingleValue.Value);
//    throw new Exception("Unknow type of unknowExpression");
//}
//int getLeftValue(INode expression)
//{
//    if (expression is ExpressionNode e)
//        return getLeftValue(e.Left);
//    if (expression is ValueNode v)
//        return int.Parse(v.SingleValue.Value);
//    throw new Exception("Unknow type of unknowExpression");
//}
//bool tryGetRigthOperator(INode expression, OperatorKind def, out OperatorKind op)
//{
//    if (expression is ExpressionNode e)
//    {
//        op = e.Operator;
//        return true;
//    }
//    op = def;
//    return false;
//}
//INode getRigthValue(INode expression)
//{
//    if (expression is ExpressionNode e)
//        return e.Rigth;
//    if (expression is ValueNode v)
//        return v;
//    throw new Exception("Unknow type of unknowExpression");
//}
//int sumExpression(INode left, INode rigth)
//{
//    debug.print("sum left: ", getValue(left).ToString(), ", rigth: ", getValue(rigth).ToString());
//    return getValue(left)+getValue(rigth);
//}
//int subExpression(INode left, INode rigth)
//{
//    return getValue(left) - getValue(rigth);
//}
//int mulExpression(INode left, int rigth)
//{
//    return getValue(left) * rigth;
//}
//int divExpression(INode left, int rigth)
//{
//    debug.print("div left: ", getValue(left).ToString(), ", rigth: ", rigth.ToString());
//    return getValue(left) / rigth;
//}
//int evalExpression(ExpressionNode expression)
//{
//    var left = new ValueNode()
//    {
//        SingleValue = new Token(0, TokenKind.ConstantDigit, (expression.Operator switch
//        {
//            OperatorKind.Sum => sumExpression(expression.Left, expression.Rigth),
//            OperatorKind.Subtract => subExpression(expression.Left, expression.Rigth),
//            OperatorKind.Multiply => mulExpression(expression.Left, getLeftValue(expression.Rigth)),
//            OperatorKind.Divide => divExpression(expression.Left, getLeftValue(expression.Rigth)),
//            _ => throw new Exception("Unknow operator")
//        }).ToString(), new())
//    };
//    if (!tryGetRigthOperator(expression.Rigth, expression.Operator, out OperatorKind op))
//        return int.Parse(left.SingleValue.Value);
//    var rigth = getRigthValue(expression.Rigth);
//    var next = new ExpressionNode()
//    {
//        Left = left,
//        Rigth = rigth,
//        Operator = op
//    };
//    return evalExpression(next);
//}
//int eval(INode expression)
//{
//    if (expression is ValueNode v)
//        return int.Parse(v.SingleValue.Value);
//    return evalExpression((ExpressionNode)expression);
//}

if (debug.isDebug())
{
    var test = @"
func main() {
    var index: u8 = 2*(6/3+2);
}";

    var compUnit = new CompilationUnit("test.mug", test);
    var tokens = compUnit.GetTokenCollection(out MugLexer lexer);
    var tree = new MugParser(lexer).GetNodeCollection();
    //debug.print("Evaluation: ", eval(((VariableStatement)((FunctionNode)tree.GlobalScope.Nodes[0]).Body.Statements[0]).Body).ToString());
    debug.print(tree.Stringize());
    debug.print(string.Join("\n", tokens));
}