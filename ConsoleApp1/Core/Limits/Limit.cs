using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Core.Sets;

namespace ConsoleApp1.Core.Limits;

public static class Limit
{
    // lim x->a f(x) = L
    public static Expr? LimitOf(Expr expr, Variable variable, Expr value)
    {
        if (expr.Constant(variable))
            return expr;

        if (value.IsInfinity)
            return LimitInf(expr, variable);
        if (value.IsNegativeInfinity)
            return LimitInf(expr, variable, true);
        
        return LimitFinite(expr, variable, value);
    }

    public static Expr? LimitInf(Expr expr, Variable variable, bool negativeInf = false)
    {
        if (PolyRational.IsPolyRational(expr, variable))
        {
            var poly = PolyRational.ToPolyRational(expr, variable);

            if (poly.Num.Deg() > poly.Den.Deg())
            {
                var deg = poly.Num.Deg();
                bool lcpos;
                if (poly.Num.LC().IsPositive)
                    lcpos = true;
                else if (poly.Num.LC().IsNegative)
                    lcpos = false;
                else
                    return null;
                
                
                if (deg % 2 == 0)
                    return lcpos ? Expr.Inf : Expr.NegInf;

                return negativeInf ^ lcpos ? Expr.Inf : Expr.NegInf;

            }
            else if (poly.Num.Deg() == poly.Den.Deg())
                return poly.Num.LC() / poly.Den.LC();
            else
                return 0;
        }

        return null;
    }

    public static Expr? LimitFinite(Expr expr, Variable variable, Expr value)
    {
        if (expr.IsContinue(variable, value).IsTrue)
            return expr.Substitue(variable, value);
        
        return null;
    }
}