using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Polynomials;

namespace ConsoleApp1.Core.Series;

public class TaylorSeries
{
    public static PolynomialSeries GenericTaylorSeriesOf(Expr f, Variable x, Expr? a = null)
    {
        if (a is null)
            a = 0;

        var i = Variable.CreateDummy("i");
        var coef = f.Derivee(x, i) / Factorial(i) * Pow(x - a, i);

        return new PolynomialSeries(i, coef);
    }
    
    public static Polynomial TaylorSeriesOf(Expr f, Variable x, Expr a, int n)
    {
        var serie = new Polynomial(0);
        for (int i = 0; i <= n; i++)
        {
            serie += f.Derivee(x, i).Substitue(x, a) / Factorial(i) * Polynomial.NewtonBinomial(-a, i);
        }
        
        return serie;
    }
    public static Polynomial TaylorSeriesOf(Expr f, Variable x, int n)
    {
        return TaylorSeriesOf(f, x, 0, n);
    }
}