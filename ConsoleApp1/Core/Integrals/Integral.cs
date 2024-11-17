using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Base;
using ConsoleApp1.Core.Expressions.Trigonometrie;
using ConsoleApp1.Core.Models;

namespace ConsoleApp1.Core.Integrals;

public class Integral
{
    // return null if fail
    public static Expr? Integrate(Expr f, Variable var, (Expr a, Expr b)? bornes = null, Expr? constant = null, bool useConstant = true)
    {
        if (constant is null)
            constant = "C";

        var F = AntiDerive(f, var);
        if (F is null)
            return null;

        if (bornes is not null)
        {
            var Fa = F.Substitue(var, bornes.Value.a);
            var Fb = F.Substitue(var, bornes.Value.b);
            return Fb - Fa;
        }

        if (useConstant)
            return F + constant;

        return F;
    }

    public static Expr? AntiDerive(Expr f, Variable var)
    {
        /*
        - I af(x) = a If(x)
        - I f + g = I f + I g
        
        Special cases
        - I poly
        - I sin^n cos^m for n,m in N
        */

        (var cste, f) = f.SeparateConstant(var);

        // Polynomial cases
        if (Poly.IsPolynomial(f, var))
        {
            var poly = Poly.ToPoly(f, var);
            return poly.Integral().Of(var) * cste;
        }
        
        if (PolyRational.IsPolyRational(f, var))
        {
            var poly = PolyRational.ToPolyRational(f, var);
            return IntPolyRational(poly.Num, poly.Den);
        }
        
        // Trig cases
        if (f.Has<SinExpr>() || f.Has<CosExpr>())
        {
            var sincos = SinCosCase(f, var);
            if (sincos is not null)
                return sincos * cste;
        }
        
        // Try divide into sum
        if (f is Addition sum)
        {
            var therms = new Expr[sum.Therms.Length];
            bool define = true;
            for (int i = 0; i < sum.Therms.Length; i++)
            {
                var therm = AntiDerive(sum.Therms[i], var);
                if (therm is null)
                {
                    define = false;
                    break;
                }
                therms[i] = therm;
            }

            if (define)
                return Add(therms);
        }

        return null;
    }

    private static Expr? SinCosCase(Expr f, Variable var)
    {
        // I sin^n cos^m
        
        /*
        sin^n -> Pow/sin
        cos^m -> Pow/Cos
        sin^n cos^m -> Mul 2
        */
        
        
        int n = 0, m = 0;

        if (f is Multiplication mul)
        {
            if (mul.Factors.Length != 2)
                return null;
            
            var (exp, expr) = mul.Factors[0].AsPowInt();
            if (expr is SinExpr)
                n += exp;
            else if (expr is CosExpr)
                m += exp;
            else
                return null;
        
            var (exp1, expr1) = mul.Factors[1].AsPowInt();
            if (expr1 is SinExpr)
                n += exp1;
            else if (expr1 is CosExpr)
                m += exp1;
            else
                return null;
        }
        else
        {
            var (exp2, expr2) = f.AsPowInt();
            if (expr2 is SinExpr)
                n += exp2;
            else if (expr2 is CosExpr)
                m += exp2;
            else
                return null;
        }

        if (n == 0)
            return TrigoIntegrals.IntegralCosPow(m, var);
        
        return TrigoIntegrals.IntegralCosSin(m, n, var);
    }

    public static Expr? IntPolyRational(Poly Num, Poly Den)
    {
        return null;
    }
}