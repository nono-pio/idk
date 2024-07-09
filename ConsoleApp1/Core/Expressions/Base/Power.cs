using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Latex;

namespace ConsoleApp1.Core.Expressions.Base;

public class Power : Expr
{
    public Power(Expr value, Expr exp) : base(value, exp) { }

    public Expr Base
    {
        get => Args[0];
        private set => Args[0] = value;
    }

    public Expr Exp
    {
        get => Args[1];
        private set => Args[1] = value;
    }
    
    public override OrderOfOperation GetOrderOfOperation()
    {
        return OrderOfOperation.Power;
    }
    public override Expr Derivee(string variable)
    {
        if (Exp.Constant(variable)) 
            return Exp * Pow(Base, Exp - 1) * Base.Derivee(variable);

       
        if (Base.Constant(variable))
            return Exp.Derivee(variable) * Log(Base) * this;

        throw new NotImplementedException("Power.Derivee : Base et Exp non constant");
        return null /**/;
    }

    public override Expr Derivee(string variable, int n)
    {
        
        if (n == 0)
            return this;

        if (Exp.Constant(variable))
        {

            if (Base.IsVar(variable)) // ddx base = 1
            {
                return NumberUtils.FactorialExpr(Exp, n - 1) * Pow(Base, Exp - n);
            }
            
            // n > 0
            // base != cste

            var therms = new Expr[n];
            var exp = Exp;
            foreach (var (bin, (m, k)) in NumberUtils.BinomialCoefficients(n - 1))
            {
                var f_in = Base.Derivee(variable, m + 1);
                var f_ddx = Pow(Base, Exp - (k + 1));
                therms[k] = Mul(bin.Expr(), f_in, exp, f_ddx); // n=1 : bin=1 * f'(x) * exp * f(x)^(exp - 1)
                
                exp *= Exp - (k + 1);
            }

            return Add(therms);
        }

        if (Base.Constant(variable))
        {
            
            if (Exp.IsVar(variable)) // ddx exp = 1
            {
                return Pow(Log(Base), n.Expr()) * this;
            }
            
            var therms = new Expr[n];
            foreach (var (bin, (m, k)) in NumberUtils.BinomialCoefficients(n - 1))
            {
                var f_in = Base.Derivee(variable, m + 1);
                var f_ln = Pow(Log(Base), (k + 1).Expr());
                therms[k] = Mul(bin.Expr(), f_in, f_ln); // n=1 : bin=1 * f'(x) * exp * f(x)^(exp - 1)
            }

            return Add(therms) * this;
        }
        
        return base.Derivee(variable, n);
    }

    public override double N()
    {
        return Math.Pow(Base.N(), Exp.N());
    }
    
    /// a^b : inv(c, 0) -> c^(1/b)
    /// a^b : inv(c, 1) -> ln_a(c)
    public override Expr Inverse(Expr y, int argIndex)
    {
        return argIndex switch
        {
            0 => Sqrt(y, Exp),
            1 => throw new Exception("not implemented"),
            _ => throw new Exception("Power as only 2 args (value:0, exp:1), not " + argIndex)
        };
    }

    public Expr Eval()
    {
        // 1. Special Values    a^b
        // a = 1 -> 1; a = 0 -> 0
        // b = 0 -> 1; b = 1 -> a
        // a, b is number -> a^b simplified


        if (Base.IsOne() || Exp.IsZero()) 
            return Num(1);
        if (Base.IsZero()) 
            return Num(0);
        if (Exp.IsOne()) 
            return Base;
        
        if (Base is Number a && Exp is Number b) 
            return Number.SimplifyPow(a, b);

        // 2. Power Tower       pow(pow(a,b),c) -> pow(a,bc)
        if (Base is Power pow)
        {
            Base = pow.Base;
            Exp = Exp * pow.Exp;
            return this;
        }

        return this;
    }
    
    public static Expr NewtonBinomial(Expr a, Expr b, int n)
    {
        var therms = new Expr[n + 1];
        for (int k = 0; k <= n; k++)
        {
            therms[k] = NumberUtils.Binomial(n, k) * Pow(a, Num(n - k)) * Pow(b, Num(k));
        }

        return Add(therms);
    }
    
    public static Expr NewtonMultinomial(Expr[] values, int n)
    {
        int m = values.Length;
        var therms = new Expr[NumberUtils.MultinomialCoefficientsLength(n, m)];
        
        int i = 0;
        foreach (var (coef, k_vec) in NumberUtils.MultinomialCoefficients(n, m))
        {
            var factors = new Expr[values.Length + 1];
            factors[0] = coef.Expr();
            for (int j = 0; j < k_vec.Length; j++)
            {
                factors[j + 1] = Pow(values[j], k_vec[j].Expr());
            }
            
            therms[i] = Mul(factors);
            i++;
        }

        return Add(therms.ToArray());
    }

    public override string ToLatex()
    {
        return LatexUtils.Power(ParenthesisLatexIfNeeded(Base), Exp.ToLatex());
    }

    public override string ToString()
    {
        return ToLatex();
    }
}