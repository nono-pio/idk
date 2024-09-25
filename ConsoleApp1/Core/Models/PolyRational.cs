using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Base;

namespace ConsoleApp1.Core.Models;

public class PolyRational
{
    public Poly Num;
    public Poly Den;
    
    public PolyRational(Poly num, Poly den)
    {
        Num = num;
        Den = den;
    }
    
    public PolyRational(Poly num)
    {
        Num = num;
        Den = PolyOne;
    }

    public bool IsConstant()
    {
        return Num.Deg() == 0 && Den.Deg() == 0;
    }
    
    public PolyRational Normal()
    {
        var gcd = Poly.Gcd(Num, Den);
        return new PolyRational(Num / gcd, Den / gcd);
    }

    public static bool IsPolyRational(Expr expr, Variable var)
    {
        if (expr.Constant(var))
            return true; // deg=0
        return expr switch
        {
            Addition add => add.Therms.All(therm => IsPolyRational(therm, var)), // deg=max(deg(therm))
            Multiplication mul => mul.Factors.All(factor => IsPolyRational(factor, var)), // deg=sum(deg(factor))
            Number => true, // deg=0 
            Power pow => IsPolyRational(pow.Base, var) && pow.Exp.IsNumberInt(), //deg=deg(base)*exp
            Variable _ => true, // var.Name == variable ? deg=1 : deg=0
            _ => false
        };

    }
    
    public static PolyRational ToPolyRational(Expr expr, Variable variable)
    {
        if (expr.Constant(variable))
            return new PolyRational(new Poly(expr));

        switch (expr) 
        {
            case Addition add :
                var poly = add.Therms.Aggregate(PolyRationalZero, (sum, therm) =>
                {
                    var th = ToPolyRational(therm, variable);
                    return sum + th;
                });
                return poly;
            
            case Multiplication mul :
                poly = mul.Factors.Aggregate(PolyRationalOne, (pro, therm) =>
                {
                    var th = ToPolyRational(therm, variable);
                    return pro * th;
                });
                return poly;
                
            case Number num:
                return new PolyRational(new Poly(num)); 
            
            case Power pow:
                
                var basePoly = ToPolyRational(pow.Base, variable);

                if (!pow.Exp.IsNumberInt())
                    throw new Exception("This is not a polynomial"); 
                
                int exp = pow.Exp.ToInt();

                return basePoly.Pow(exp);
            
            case Variable var: 
                return var.Name == variable ? new PolyRational(new Poly(1, 0)) : new PolyRational(new Poly(var)); // var.Name == variable ? deg=1 : deg=0
            
            default:
                throw new Exception("This is not a polynomial"); 
        }
    }
    
    public PolyRational Pow(int n)
    {
        if (n > 0)
            return new PolyRational(Num.Pow(n), Den.Pow(n));
        
        return new PolyRational(Den.Pow(-n), Num.Pow(-n));
    }

    public static PolyRational operator +(PolyRational a, PolyRational b)
    {
        return new PolyRational(a.Num * b.Den + b.Num * a.Den, a.Den * b.Den);
    }
    
    public static PolyRational operator -(PolyRational a, PolyRational b)
    {
        return new PolyRational(a.Num * b.Den - b.Num * a.Den, a.Den * b.Den);
    }
    
    public static PolyRational operator *(PolyRational a, PolyRational b)
    {
        return new PolyRational(a.Num * b.Num, a.Den * b.Den);
    }
    
    public static PolyRational operator /(PolyRational a, PolyRational b)
    {
        return new PolyRational(a.Num * b.Den, a.Den * b.Num);
    }

    public override string ToString()
    {
        return $"({Num}) / ({Den})";
    }
}