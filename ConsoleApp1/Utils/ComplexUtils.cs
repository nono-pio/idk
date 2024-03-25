using ConsoleApp1.Core.Expressions.ComplexExpressions;

namespace ConsoleApp1.Utils;

public static class ComplexUtils
{
    
    public static Complex AsComplex((Expr, Expr) complexTuple) => new(complexTuple.Item1, complexTuple.Item2);
    public static (Expr, Expr) AsPolar((Expr, Expr) complexTuple) => (R(complexTuple), Theta(complexTuple));
    public static (Expr, Expr) FromPolar((Expr, Expr) polarTuple)
    {
        var (r, theta) = polarTuple;
        return (r * Cos(theta), r * Sin(theta));
    }
    
    public static Expr Theta((Expr, Expr) complexTuple)
    {
        throw new NotImplementedException("");
        // var (real, complex) = complexTuple;
        // return Atan(complex / real);
    }
    public static Expr R((Expr, Expr) complexTuple)
    {
        var (real, complex) = complexTuple;
        return Sqrt(real * real + complex * complex);
    }
    
    public static (Expr, Expr) Add((Expr, Expr) a, (Expr, Expr) b)
    {
        return (a.Item1 + b.Item1, a.Item2 + b.Item2);
    }
    
    public static (Expr, Expr) Sub((Expr, Expr) a, (Expr, Expr) b)
    {
        return (a.Item1 - b.Item1, a.Item2 - b.Item2);
    }
    
    public static (Expr, Expr) Mul((Expr, Expr) a, (Expr, Expr) b)
    {
        var (r1, c1) = a;
        var (r2, c2) = b;
        return (r1*r2 - c1*c2, r1*c2 + c1*r2);
    }
}