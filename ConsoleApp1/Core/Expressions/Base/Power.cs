using ConsoleApp1.Core.Complexes;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Latex;
using Sdcb.Arithmetic.Mpfr;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Expressions.Base;

public class Power : Expr
{
    public override bool IsNatural => Base.IsNatural && Exp.IsNatural;
    public override bool IsInteger => Base.IsInteger && Exp.IsNatural;
    public override bool IsRational => Base.IsRational && Exp.IsInteger;
    public override bool IsReal =>Base.IsReal && Exp.IsInteger; // todo: (-1)^0.5
    public override bool IsComplex => true;
    
    public override bool IsPositive => Base.IsPositive;
    public override bool IsNegative => base.IsNegative; // todo: (-1)^(2n+1)

    public override bool IsInfinity => ((Base is Number num && num.Num > 1) || Base is Constant { AppValue: > 1 }) && Exp.IsInfinity;
    public override bool IsNegativeInfinity => false;

    public bool IsExp => Base == Atoms.Constant.E;
    
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

     public override Boolean DomainCondition =>
         Base > 0 | (Boolean.Equal(Base, 0) & Exp > 0) | (Base < 0 & Z.Contains(Exp));

     public Power(Expr value, Expr exp) : base(value, exp) { }

    public override (Expr Num, Expr Den) AsFraction(bool expNumber = true)
    {
        // (n/d)^b
        // b>0 -> n^b, d^b
        // b<0 -> d^-b, n^-b
        var base_frac = Base.AsFraction(expNumber);

        if (expNumber)
        {
            if (Exp is Number num && num.IsNegative)
                return (Pow(base_frac.Den, -num), Pow(base_frac.Num, -num));
        }
        else if (Exp.IsNegative)
            return (Pow(base_frac.Den, -Exp), Pow(base_frac.Num, -Exp));

        if (base_frac.Den.IsOne)
            return (this, 1);
        
        return (Pow(base_frac.Num, Exp), Pow(base_frac.Den, Exp));
    }

    public override Expr Substitue(Expr expr, Expr value)
    {
        if (this == expr)
            return value;
        
        var @base = Base.Substitue(expr, value);
        var exp = Exp.Substitue(expr, value);
        
        if (expr is Power pow && pow.Base == @base)
        {
            var new_exp = exp / pow.Exp;

            if (new_exp.Constant())
                return Pow(value, new_exp);
        }

        return Pow(@base, exp);
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


        if (@base.IsNumOne || exp.IsNumZero) 
            return Num(1);
        if (@base.IsNumZero)
        {
            if (exp.IsPositive)
                return Num(0);
            if (exp.IsNegative)
                return double.NaN;
        }
        if (exp.IsNumOne) 
            return @base;
        
        if (@base is Number a && exp is Number b) 
            return Number.SimplifyPow(a, b);

        if (exp is Number num && num.Num.IsEven && @base.CanRemoveNegativeSign())
            @base = -@base;

        // 2. Power Tower       pow(pow(a,b),c) -> pow(a,bc)
        if (@base is Power pow)
        {
            @base = pow.Base;
            exp = exp * pow.Exp;
            return Pow(@base, exp);
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

    public override Expr fDerivee(int argIndex)
    {
        if (argIndex == 0)
        {
            return Exp * Pow(Base, Exp - 1);
        }
        if (argIndex == 1)
        {
            return Ln(Base) * this;
        }
        
        throw new ArgumentException("ArgIndex must be 0 (base) or 1 (exp)");
    }

    public override Expr Derivee(Variable variable, int n)
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
            1 => Log(y, Base),
            _ => throw new Exception("Power as only 2 args (value:0, exp:1), not " + argIndex)
        };
    }
    
    public override Expr[] AllReciprocal(Expr y, int argIndex)
    {
        if (argIndex == 0 && Exp is Number num && num.Num.IsEven)
        {
            return [Sqrt(y, Exp), -Sqrt(y, Exp)];
        }

        return base.AllReciprocal(y, argIndex);
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
        if (Exp is Addition eAdd)
            return Mul(eAdd.Therms.Select(n => Pow(Base, n).Develop()).ToArray());
        
        
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
        
        if (Exp.Is(-1))
            return LatexUtils.Fraction("1", Base.ToLatex());

        if (Exp is Power pow && pow.Exp.Is(-1) && pow.Base.IsInteger) // base^{exp_b^-1}=base^{1/exp_b}=sqrt[exp_b]{base}
            return LatexUtils.NthRoot(pow.Base.ToLatex(), Base.ToLatex());
        
        if (Exp is Number num && num.Num.IsFraction && num.Num.Numerator == 1) // idem for fraction
            return LatexUtils.NthRoot(num.Num.Denominator.ToString(), Base.ToLatex());

        if (Exp is Number && Exp.IsNegative)
            return LatexUtils.Fraction("1", LatexUtils.Power(ParenthesisLatexIfNeeded(Base), (-Exp).ToLatex()));
        
        return LatexUtils.Power(ParenthesisLatexIfNeeded(Base), LatexUtils.LatexBracesIfNotSingle(Exp.ToLatex()));
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

    public override MpfrFloat NPrec(int precision = 333, MpfrRounding rnd = MpfrRounding.ToEven)
    {
        return MpfrFloat.Power(Base.NPrec(precision, rnd), Exp.NPrec(precision, rnd), precision, rnd);
    }
}