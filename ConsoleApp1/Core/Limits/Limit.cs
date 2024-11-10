using System.Diagnostics;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Models;

namespace ConsoleApp1.Core.Limits;

public enum Direction
{
    Smaller,
    Greater,
    Both
}

public static class Limit
{
    // lim x->a f(x) = L
    public static Expr LimitOf(Expr expr, Variable variable, Expr value, Direction dir = Direction.Smaller)
    {
        if (expr.Constant(variable))
            return expr;

        if (value.IsInfinity)
            return LimitInf(expr, variable);
        if (value.IsNegativeInfinity)
            return LimitInf(expr.Substitue(variable, -variable), variable);

        var sign = dir switch
        {
            Direction.Both => 0,
            Direction.Greater => 1,
            Direction.Smaller => -1,
            _ => throw new UnreachableException()
        };

        if (sign == 0)
        {
            var sm = LimitOf(expr, variable, value, Direction.Smaller);
            var gt = LimitOf(expr, variable, value, Direction.Greater);

            if (sm != gt)
                throw new Exception($"The Limit of {expr} from the right and the left are not the same");

            return sm;
        }
        
        return LimitInf(expr.Substitue(variable, value + sign / variable), variable);
    }

    public static Expr LimitInf(Expr expr, Variable variable)
    {
        if (PolyRational.IsPolyRational(expr, variable))
        {
            var poly = PolyRational.ToPolyRational(expr, variable);

            if (poly.Num.Deg() > poly.Den.Deg())
            {
                var deg = poly.Num.Deg();
                var numPos = poly.Num.LC().Positivity;
                var denPos = poly.Den.LC().Positivity;
                if (numPos is null || denPos is null)
                    goto gruntz;

                var sign = !(numPos.Value ^ denPos.Value);
                
                if (deg % 2 == 0)
                    return sign ? Expr.Inf : Expr.NegInf;

                return sign ? Expr.Inf : Expr.NegInf;

            }
            else if (poly.Num.Deg() == poly.Den.Deg())
                return poly.Num.LC() / poly.Den.LC();
            else
                return 0;
        }
        
        gruntz :
            return Gruntz.LimitInf(expr, variable);
    }
}