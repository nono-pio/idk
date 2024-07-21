namespace ConsoleApp1.Core.Expressions.Others;

public static class ConstructorOthers
{
    public static Expr Max(params Expr[] elements) => MaxExpr.Construct(elements);
    public static Expr Min(params Expr[] elements) => MinExpr.Construct(elements);
}