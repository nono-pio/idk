using ConsoleApp1.Core.Atoms;
using ConsoleApp1.Core.Expr.Fonctions;
using ConsoleApp1.Core.Expr.Fonctions.Base;

namespace ConsoleApp1.Core.Expr.Fonction.Base;

public static class ConstructorBase
{
    // TODO
    public static Expr Add(params Expr[] therms)
    {
        return therms.Length switch
        {
            0 => new Number(0),
            1 => therms[0],
            _ => new Addition(therms).Eval()
        };
    }

    public static Expr Sub(Expr num, Expr den)
    {
        return Add(num, Mul(Num(-1), den));
    }

    public static Expr Neg(Expr expr)
    {
        return Mul(Num(-1), expr);
    }

    // TODO
    public static Expr Mul(params Expr[] factors)
    {
        return factors.Length switch
        {
            0 => new Number(1),
            1 => factors[0],
            _ => new Multiplication(factors).Eval()
        };
    }

    // TODO
    public static Expr Div(Expr num, Expr den)
    {
        return Mul(num, Pow(den, Num(-1)));
    }

    public static Expr Pow(Expr value, Expr exp)
    {
        return new Power(value, exp).Eval();
    }

    

    public static Expr Sqrt(Expr value)
    {
        return new Power(value, Div(1.Expr(), 2.Expr())).Eval();
    }

    public static Expr Sqrt(Expr value, Expr n)
    {
        return new Power(value, Div(1.Expr(), n)).Eval();
    }
    
    public static Expr Exp(Expr value)
    {
        return new Power(value, Math.E.Expr()).Eval();
    }

    public static Expr Log(Expr value, Expr @base)
    {
        return new Log(value, @base).Eval();
    }
    
    
    public static Expr Log(Expr value)
    {
        return new Log(value).Eval();
    }
}