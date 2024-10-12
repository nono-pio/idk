using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Expressions.Others;

public static class ConstructorOthers
{
    public static Expr Max(params Expr[] elements) => MaxExpr.Construct(elements);
    public static Expr Min(params Expr[] elements) => MinExpr.Construct(elements);
    public static Expr Abs(Expr x) => AbsExpr.Eval(x);
    public static Expr Floor(Expr x) => FloorExpr.Eval(x);
    public static Expr Ceil(Expr x) => CeilExpr.Eval(x);
    public static Expr Round(Expr x) => RoundExpr.Eval(x);
    public static Expr Sign(Expr x) => SignExpr.Eval(x);
    public static Expr Piece(params (Expr, Boolean)[] values) => Piecewise.Eval(values);
    public static Expr Factorial(Expr x) => FactorialExpr.Eval(x);
}