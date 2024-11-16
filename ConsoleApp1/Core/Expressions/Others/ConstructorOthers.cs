using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Expressions.Others;

public static class ConstructorOthers
{
    public static Expr Max(params Expr[] elements) => MaxExpr.Construct(elements);
    public static Expr Min(params Expr[] elements) => MinExpr.Construct(elements);
    public static Expr Abs(Expr x) => AbsExpr.Evaluator.Eval(x);
    public static Expr Floor(Expr x) => FloorExpr.Evaluator.Eval(x);
    public static Expr Ceil(Expr x) => CeilExpr.Evaluator.Eval(x);
    public static Expr Round(Expr x) => RoundExpr.Evaluator.Eval(x);
    public static Expr Sign(Expr x) => SignExpr.Evaluator.Eval(x);
    public static Expr Piece(params (Expr, Boolean)[] values) => Piecewise.Eval(values);
    public static Expr Factorial(Expr x) => FactorialExpr.Evaluator.Eval(x);
}