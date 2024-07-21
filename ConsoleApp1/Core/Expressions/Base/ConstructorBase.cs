using ConsoleApp1.Core.Expressions.Atoms;

namespace ConsoleApp1.Core.Expressions.Base;

public static class ConstructorBase
{
    
    public static Expr Add(params Expr[] therms) => Addition.Construct(therms);

    public static Expr Sub(Expr a, Expr b)
    {
        return Add(a, Mul(-1, b));
    }

    public static Expr Neg(Expr expr)
    {
        return Mul(-1, expr);
    }
    
    public static Expr Mul(params Expr[] factors) => Multiplication.Construct(factors);
    
    public static Expr Div(Expr num, Expr den)
    {
        return Mul(num, Pow(den, -1));
    }

    public static Expr Pow(Expr value, Expr exp) => Power.Construct(value, exp);
    
    public static Expr Sqrt(Expr value)
    {
        return Pow(value, Num(1, 2));
    }

    public static Expr Sqrt(Expr value, Expr n)
    {
        return Pow(value, Div(1, n));
    }
    
    public static Expr Exp(Expr value) => Pow(Constants.E, value);

    public static Expr Log(Expr value, Expr @base) => Logarithm.Construct(value, @base);    
    public static Expr Log10(Expr value) => Logarithm.Construct(value, 10);
    public static Expr Log2(Expr value) => Logarithm.Construct(value, 2);
    public static Expr Ln(Expr value) => Logarithm.Construct(value, Constants.E);
}