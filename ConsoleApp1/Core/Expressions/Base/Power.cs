using ConsoleApp1.Core.Complexes;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Latex;

namespace ConsoleApp1.Core.Expressions.Base;

public class Power : Expr
{
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
     
    public Power(Expr value, Expr exp) : base(value, exp) { }

    public override (Expr Num, Expr Den) AsFraction()
    {
        // (n/d)^b
        // b>0 -> n^b, d^b
        // b<0 -> d^-b, n^-b
        var base_frac = Base.AsFraction();
        
        if (Exp.IsNegative)
            return (Pow(base_frac.Den, -Exp), Pow(base_frac.Num, -Exp));

        if (base_frac.Den.IsOne)
            return (this, 1);
        
        return (Pow(base_frac.Num, Exp), Pow(base_frac.Den, Exp));
    }

    public override Complex AsComplex()
    {
        var base_c = Base.AsComplex();
        var exp_c = Exp.AsComplex();
        
        return base_c.Pow(exp_c);
    }

    public static Expr Construct(Expr @base, Expr exp)
    {
        // 1. Special Values    a^b
        // a = 1 -> 1; a = 0 -> 0
        // b = 0 -> 1; b = 1 -> a
        // a, b is number -> a^b simplified


        if (@base.IsOne || exp.IsZero) 
            return Num(1);
        if (@base.IsZero) 
            return Num(0);
        if (exp.IsOne) 
            return @base;
        
        if (@base is Number a && exp is Number b) 
            return Number.SimplifyPow(a, b);

        // 2. Power Tower       pow(pow(a,b),c) -> pow(a,bc)
        if (@base is Power pow)
        {
            @base = pow.Base;
            exp = exp * pow.Exp;
            return new Power(@base, exp);
        }

        return new Power(@base, exp);
    }
    public static Expr ConstructNotEval(Expr @base, Expr exp) => new Power(@base, exp);

    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Construct(exprs[0], exprs[1]);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => ConstructNotEval(exprs[0], exprs[1]);
    
    public override OrderOfOperation GetOrderOfOperation()
    {
        return OrderOfOperation.Power;
    }
    public override Expr Derivee(string variable)
    {
        if (Exp.Constant(variable)) 
            return Exp * Pow(Base, Exp - 1) * Base.Derivee(variable);

       
        if (Base.Constant(variable))
            return Exp.Derivee(variable) * Ln(Base) * this;

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
                return Pow(Ln(Base), n.Expr()) * this;
            }
            
            var therms = new Expr[n];
            foreach (var (bin, (m, k)) in NumberUtils.BinomialCoefficients(n - 1))
            {
                var f_in = Base.Derivee(variable, m + 1);
                var f_ln = Pow(Ln(Base), (k + 1).Expr());
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
    public override Expr Reciprocal(Expr y, int argIndex)
    {
        return argIndex switch
        {
            0 => Sqrt(y, Exp),
            1 => throw new Exception("not implemented"),
            _ => throw new Exception("Power as only 2 args (value:0, exp:1), not " + argIndex)
        };
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

    public override Expr Develop()
    {
        return Base switch
        {
            Addition add => Exp.IsInteger ? NewtonMultinomial(add.Therms, (int)Exp.N()) : this,
            Multiplication mul => Mul(mul.Factors.Select(fac => Pow(fac, Exp)).ToArray()),
            _ => this
        };
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
        var exp = Exp.ToLatex();

        if (exp.StartsWith("-"))
            return LatexUtils.Fraction("1", LatexUtils.Power(ParenthesisLatexIfNeeded(Base), exp[1..]));
        
        return LatexUtils.Power(ParenthesisLatexIfNeeded(Base), exp);
    }

    public override string ToString()
    {
        var exp = Exp.ToString();

        if (exp.StartsWith("-"))
        {
            exp = exp[1..];
            if (exp == "1")
                return $"1/{ParenthesisIfNeeded(Base)}";
            
            return $"1/{ParenthesisIfNeeded(Base)}^{exp}";
        }

        return $"{ParenthesisIfNeeded(Base)}^{ParenthesisIfNeeded(Exp)}";
    }
}