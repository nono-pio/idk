namespace ConsoleApp1.Core.Expressions.ComplexExpressions;

public static class ConstructorComplex
{
    public static Expr Re(Expr x) => ComplexReal.Construct(x);
    public static Expr Im(Expr x) => ComplexImag.Construct(x);
    public static Expr Arg(Expr x) => ArgExpr.Construct(x);
    public static Expr I => new ComplexExpr(0, 1);
}