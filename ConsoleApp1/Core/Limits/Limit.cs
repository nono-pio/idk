using ConsoleApp1.Core.Sets;

namespace ConsoleApp1.Core.Limits;

public static class Limit
{
    // lim x->a f(x) = L
    public static Expr LimitOf(Expr expr, string variable, Expr value)
    {
        if (expr.Constant(variable))
            return expr;
        
        if (expr.IsContinue(variable, value).IsTrue)
            return expr.Substitue(variable, value);
        
        return expr;
    }
}