using ConsoleApp1.Core.Models;

namespace ConsoleApp1.Core.Integrals;

public static class TrigoIntegrals
{
    
    // ∫ cos^m(x)sin^n(x) dx (no +C)
    public static Expr IntegralCosSin(int m, int n, Expr variable)
    {
        if (m < 0 || n < 0)
            throw new Exception("m and n should be positive integers");

        if (m % 2 == 1)
            return IntegralCosOddSin(m, n, variable);

        if (n % 2 == 1)
            return IntegralCosSinOdd(m, n, variable);
        
        return IntegralCosSinEven(m, n, variable);
    }

    private static Expr IntegralCosOddSin(int m, int n, Expr variable)
    {
        var m2 = m / 2;
        return IntegralForTrig(m2, n, Sin(variable));
    }

    private static Expr IntegralCosSinOdd(int m, int n, Expr variable)
    {
        var n2 = n / 2;
        return -IntegralForTrig(n2, m, Cos(variable));
    }
    
    // ∫ (1-u^2)^m * u^n du
    private static Expr IntegralForTrig(int m, int n, Expr u)
    {
        var poly1 = new Poly(-1, 0, 1).Pow(m); // (1-u^2)^m
        var poly2 = Poly.FromMonomial(1, n); // u^n
        var poly = poly1 * poly2;
        return poly.Integral().Of(u);
    }

    private static Expr IntegralCosSinEven(int m, int n, Expr variable)
    {
        // 1/2^(m+n) ∫ (1+cos(2x))^m/2 (1-cos(2x))^n/2 dx
        var c = Num(1, (long) Math.Pow(2, (m+n)*.5));
        
        var cosPoly = new Poly(1, 1).Pow(m/2); // (1 + cos(2x))^m
        var sinPoly = new Poly(-1, 1).Pow(n/2); // (1 - cos(2x))^n
        var poly = cosPoly * sinPoly;
        Expr res = 0;
        foreach (var (coef, deg) in poly.AsCoefDeg())
        {
            // c * cos(2x) ^ deg

            res += c * coef / 2 * IntegralCosPow(deg, 2.Expr() * variable);
        }

        return res;
    }
    
    // ∫ cos^m(x) dx
    public static Expr IntegralCosPow(int m, Expr variable)
    {
        return m switch
        {
            0 => variable,
            1 => Sin(variable),
            < 0 => throw new Exception("m should be a positive integer"),
            _ => m % 2 == 1 ? IntegralCosOdd(m, variable) : IntegralCosEven(m, variable)
        };
    }

    private static Expr IntegralCosOdd(int m, Expr variable)
    {
        var m2 = m / 2;
        var poly = new Poly(-1, 0, 1).Pow(m2);
        
        return poly.Integral().Of(Sin(variable));
    }
    
    private static Expr IntegralCosEven(int m, Expr variable)
    {
        var m2 = m / 2;
        // 1/2^m2 ∫ (1-cos(2x))^m2 dx
        var c = Num(1, (long) Math.Pow(2, m2));
        
        var poly = new Poly(1, 1).Pow(m2); // (1 + cos(2x))^m
        Expr res = 0;
        foreach (var (coef, deg) in poly.AsCoefDeg())
        {
            // c * cos(2x) ^ deg

            res += c * coef / 2 * IntegralCosPow(deg, 2 * variable);
        }

        return res;
    }

    
}