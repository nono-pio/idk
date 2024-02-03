using ConsoleApp1.core.atoms;
using ConsoleApp1.core.expr.atoms;
using ConsoleApp1.core.expr.fonctions;
using ConsoleApp1.core.expr.fonctions.@base;
using ConsoleApp1.core.expr.fonctions.trigonometrie;
using ConsoleApp1.utils;

namespace ConsoleApp1.core;

public static class Constructor
{
    // <-- Number Expression -->
    public static Expr Zero => Num(0);
    public static Expr Un => Num(1);
    public static Expr Deux => Num(2);

    public static Expr NUn => Num(-1);
    
    // <-- Polynome Expression -->
    
    public static Poly PolyZero => new Poly(Zero);

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

    public static Expr Num(double num)
    {
        return new Number(num);
    }

    public static Expr Var(string id)
    {
        return new Variable(id);
    }

    public static Variable[] Vars(params string[] ids)
    {
        var vars = new Variable[ids.Length];
        for (var i = 0; i < ids.Length; i++) vars[i] = new Variable(ids[i]);

        return vars;
    }

    public static Expr Cos(Expr expr)
    {
        return new Cos(expr);
    }

    public static Expr Sin(Expr expr)
    {
        return new Sin(expr);
    }

    public static Expr Csc(Expr expr)
    {
        return new Csc(expr);
    }

    public static Expr Sec(Expr expr)
    {
        return new Sec(expr);
    }

    public static Expr Tan(Expr expr)
    {
        return new Tan(expr);
    }

    public static Expr Cot(Expr expr)
    {
        return new Cot(expr);
    }

    // Misc
    public static int Gcd(int a, int b)
    {
        while (a != b)
            if (a > b)
                a -= b;
            else
                b -= a;

        return a;
    }

    public static int Ppmc(int a, int b)
    {
        return a * b / Gcd(a, b);
    }
}