namespace ConsoleApp1.Core.Expressions.Others;

public static class ConstructorOthers
{
    public static Expr Max(params Expr[] elements) => new Max(elements);
    public static Expr Min(params Expr[] elements) => new Min(elements);
}