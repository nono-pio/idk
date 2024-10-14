using ConsoleApp1.Core.Expressions.Atoms;

namespace ConsoleApp1.Core.Expressions.Trigonometrie;

public abstract class TrigonometrieExpr : FonctionExpr
{
    protected TrigonometrieExpr(Expr x) : base(x) {}

    public static Dictionary<Number, (Expr Sin, Expr Cos, Expr Tan)> MapValues = 
        new Dictionary<Number, (Expr Sin, Expr Cos, Expr Tan)>()
    {
        {0, (0, 1, 0)},
        {Num(1, 2), (1, 0, NaN)},
        {1, (0, -1, 0)},
        {Num(3, 2), (-1, 0, NaN)},
        
        {Num(1, 4), (Num(1, 2)*Sqrt2, Num(1, 2)*Sqrt2, 1)},
        {Num(3, 4), (Num(1, 2)*Sqrt2, -Num(1, 2)*Sqrt2, -1)},
        {Num(5, 4), (-Num(1, 2)*Sqrt2, -Num(1, 2)*Sqrt2, 1)},
        {Num(7, 4), (-Num(1, 2)*Sqrt2, Num(1, 2)*Sqrt2, -1)},
    };

    public static Expr NaN  => new Number(double.NaN);
    private static Expr Sqrt2 => Sqrt(2);
}