﻿using ConsoleApp1.Core.Classes;
using ConsoleApp1.Core.Complexes;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Sets;
using ConsoleApp1.Latex;
using Sdcb.Arithmetic.Mpfr;

namespace ConsoleApp1.Core.Expressions.Base;

public class Multiplication : Expr
{
    public Expr[] Factors => Args;
    
    # region Constructors
    
    public Multiplication(params Expr[] factors) : base(factors)
    {
        if (factors.Length < 2) 
            throw new Exception("You must add two or more factor");
    }

    public override (Expr Constant, Expr Variate) SeparateConstant(Variable var)
    {

        var cstes = new Expr[Factors.Length];
        var vars = new Expr[Factors.Length];

        for (int i = 0; i < Factors.Length; i++)
        {
            var cv = Factors[i].SeparateConstant(var);
            cstes[i] = cv.Constant;
            vars[i] = cv.Variate;
        }

        return (Mul(cstes), Mul(vars));
    }
    
    public override (Expr Constant, Expr Variate) SeparateConstant()
    {

        var cstes = new Expr[Factors.Length];
        var vars = new Expr[Factors.Length];

        for (int i = 0; i < Factors.Length; i++)
        {
            var cv = Factors[i].SeparateConstant();
            cstes[i] = cv.Constant;
            vars[i] = cv.Variate;
        }

        return (Mul(cstes), Mul(vars));
    }

    public override (Expr Num, Expr Den) AsFraction(bool expNumber = true)
    {
        var nums = new Expr[Factors.Length];
        var dens = new Expr[Factors.Length];
        for (int i = 0; i < Factors.Length; i++)
        {
            var frac = Factors[i].AsFraction(expNumber);
            nums[i] = frac.Num;
            dens[i] = frac.Den;
        }

        return (Mul(nums), Mul(dens));
    }

    public static Expr Construct(params Expr[] exprs)
    {
        // Rules
        // 1. x * x = x^2
        // 2. x * 0 = 0
        // 4. x * 1 = x
        // 4. x * (y * z) = x * y * z
        // 5. Sort the factors
        
        if (exprs.Length == 0)
            return 1;
        if (exprs.Length == 1)
            return exprs[0];

        NumberStruct numbersProduct = 1;
        var exprsFactors = new List<Expr>();
        foreach (var expr in exprs.SelectMany(e => e is Multiplication mul ? mul.Factors : [e]))
        {
            if (expr is Number num)
            {
                if (num.IsZero)
                    return 0;
                
                numbersProduct *= num.Num;
                continue;
            }

            var hasCombined = false;
            for (int i = 0; i < exprsFactors.Count; i++)
            {
                Expr? combine = Combine(expr, exprsFactors[i]);
                if (combine is not null)
                {
                    if (combine.IsNumOne)
                        exprsFactors.RemoveAt(i);
                    else
                        exprsFactors[i] = combine;
    
                    hasCombined = true;
                    break;
                }
            }
        
            if (!hasCombined)
                exprsFactors.Add(expr);
        }

        if (!numbersProduct.IsOne)
            exprsFactors.Add(Num(numbersProduct));

        return exprsFactors.Count switch
        {
            0 => 1,
            1 => exprsFactors[0],
            _ => new Multiplication(Sorting.BubbleSort(exprsFactors.ToArray()))
        };
    }
    private static Expr? Combine(Expr a, Expr b)
    {
        if (a == b)
            return Pow(a, 2);
        
        if (a is not Power && b is not Power)
            return null;

        if (a is Power aPow && b is Power bPow && !aPow.Base.Constant())
            return aPow.Base == bPow.Base ? Pow(aPow.Base, aPow.Exp + bPow.Exp) : null;

        Power pow;
        Expr expr;
        if (a is Power ap)
        {
            pow = ap;
            expr = b;
        }
        else
        {
            pow = (Power)b;
            expr = a;
        }

        if (pow.Base == expr && !expr.Constant())
        {
            var newExp = pow.Exp + 1;
            if (newExp.IsZero)
                return 1;
            
            return Pow(pow.Base, newExp);
        }

        return null;
    }
    
    public static Expr MulOpti(Expr a, Expr b)
    {
        if (a is Multiplication || b is Multiplication)
            return Construct(a, b);
        
        if (a.IsNumZero || b.IsNumZero)
            return 0;
        
        if (a is Number numA && b is Number numB)
            return new Number(numA.Num * numB.Num);

        if (a.IsNumOne)
            return b;
        if (b.IsNumOne)
            return a;

        var result = Combine(a, b);
        if (result is not null)
            return result;

        return new Multiplication(a, b);
    }

    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Construct(exprs);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => new Multiplication(exprs);

    # endregion

    public override Complex AsComplex()
    {
        return Factors.Aggregate<Expr, Complex>(1, (product, factor) => product * factor.AsComplex());
    }
    
    public override bool IsNatural => Factors.All(factor => factor.IsNatural);
    public override bool IsInteger => Factors.All(factor => factor.IsInteger);
    public override bool IsRational => Factors.All(factor => factor.IsRational);
    public override bool IsReal => Factors.All(factor => factor.IsReal);
    public override bool IsComplex => Factors.All(factor => factor.IsComplex);
    
    public override Set AsSet()
    {
        Set SetAAddSetB(Set setA, Set setB) => ArithmeticOnSets.BiCommutativeFunctionOnSet(
            (el1, el2) => el1 * el2, setA, setB, 
            interval: (interval, interval1) => Interval(Min(interval.Start * interval1.End, interval.Start * interval1.Start, interval.End * interval1.End, interval.End * interval1.Start), Max(interval.Start * interval1.End, interval.Start * interval1.Start, interval.End * interval1.End, interval.End * interval1.Start)), // TODO : bornes
            expr_interval: (expr, interval) => Interval(Min(interval.Start * expr, interval.End * expr), Max(interval.Start * expr, interval.End * expr)), // TODO : bornes
            bns: (bnsA, bnsB) => bnsA._Level >= bnsB._Level ? bnsA : bnsB,
            bns_integral: (bns, interval) => bns is Real ? bns : null,
            expr_bns: (expr, bns) => bns switch
            {
                Natural => expr.Is(-1) ? Z.Negative : throw new NotImplementedException(),
                Integer => expr.Is(-1) ? bns : throw new NotImplementedException(),
                Rational => expr.IsRational ? bns : throw new NotImplementedException(),
                Real => expr.IsReal ? bns : throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            }
        );


        return Factors.Skip(1).Aggregate(Factors[0].AsSet(), (result, factor) => SetAAddSetB(result, factor.AsSet()));
    }

    public override Expr fDerivee(int argIndex)
    {
        return Mul(Factors.Where((_, i) => i != argIndex).ToArray());
    }

    public override Expr Derivee(Variable variable)
    {
        var therms = new Expr[Factors.Length];
        for (var i = 0; i < Factors.Length; i++)
        {
            var facteurs = new Expr[Factors.Length];
            for (var j = 0; j < Factors.Length; j++)
            {
                facteurs[j] = i == j ? Factors[j].Derivee(variable) : Factors[j];
            }

            therms[i] = Mul(facteurs);
        }

        return Add(therms);
    }

    public override Expr Develop()
    {
        return DistributeOverAddition();
    }

    public Expr DistributeOverAddition()
    {
        var therms = new List<Expr>();
        var factors = new List<Expr>();
        foreach (var factor in Factors)
        {
            if (factor is Addition add)
            {
                if (therms.Count == 0)
                {
                    therms.AddRange(add.Args);
                }
                else
                {
                    var newTherms = new List<Expr>();
                    foreach (var term in therms)
                    {
                        foreach (var addTerm in add.Args)
                        {
                            newTherms.Add(Mul(term, addTerm));
                        }
                    }

                    therms = newTherms;
                }
            }
            else
            { 
                factors.Add(factor);
            }
        }

        if (therms.Count == 0)
            return Mul(factors.ToArray());
        
        return Add(therms.Select(t => Mul(factors.Append(t).ToArray())).ToArray());
    }

    public override Expr Derivee(Variable variable, int n)
    {

        int m = Factors.Length;
        var therms = new Expr[NumberUtils.MultinomialCoefficientsLength(n, m)];
        
        int i = 0;
        foreach (var (coef, k_vec) in NumberUtils.MultinomialCoefficients(n, m))
        {
            var factors = new Expr[Factors.Length + 1];
            factors[0] = coef.Expr();
            for (int j = 0; j < k_vec.Length; j++)
            {
                factors[j + 1] = Factors[j].Derivee(variable, k_vec[j]);
            }
            
            therms[i] = Mul(factors);
            i++;
        }

        return Add(therms.ToArray());
    }

    public override double N()
    {
        return Factors.Aggregate<Expr, double>(1, (current, factor) => current * factor.N());
    }

    public override Expr Reciprocal(Expr y, int argIndex)
    {
        // a*b : inv(c, 0) -> c/b
        // a*b : inv(c, 1) -> c/a


        var rest = new Expr[Factors.Length - 1]; // 1+
        for (var i = 0; i < Factors.Length; i++)
        {
            if (i == argIndex) continue;

            if (i < argIndex)
                rest[i] = 1 / Factors[i];
            else
                rest[i - 1] = 1 / Factors[i];
        }

        return y * Mul(rest);
    }
    
    public override OrderOfOperation GetOrderOfOperation()
    {
        return OrderOfOperation.Multiplication;
    }
    
    public override (NumberStruct, Expr?) AsMulCoef()
    {
        NumberStruct coef = 1;
        var newFactors = new List<Expr>();
        foreach (var factor in Factors)
        {
            if (factor is Number num)
                coef *= num.Num;
            else
              newFactors.Add(factor);
        }
        
        if (newFactors.Count == 0)
            return (coef, null);
        if (newFactors.Count == 1)
            return (coef, newFactors[0]);

        return (coef, NotEval(newFactors.ToArray()));
    }

    public override string? ToString()
    {
        if (Factors.Length < 2) 
            throw new Exception("You must mul two or more factors");
        
        var result = ParenthesisIfNeeded(Factors[0]);
        var isM1 = result == "-1";

        for (var i = 1; i < Factors.Length; i++)
        {
            // TODO : Check if the pow is negative : * -> /
            result += '*' + ParenthesisIfNeeded(Factors[i]);
        }
        
        return isM1 && result is not null ? '-' + result[3..] : result;
    }

    public override string ToLatex()
    {
        if (Factors.Length < 2) 
            throw new Exception("You must mul two or more factors");

        if (CanRemoveNegativeSign())
            return "-" + (-this).ToLatex();

        string MulLatex(Expr[] items) => string.Join("", items.Select(ParenthesisLatexIfNeeded));

        string FractionLatex((Expr num, Expr den) frac)
        {
            var denLatex = frac.den is Multiplication denMul ? MulLatex(denMul.Args) : frac.den.ToLatex();
            var numLatex = frac.num is Multiplication numMul ? 
                MulLatex(numMul.Args) : 
                denLatex == "1" ? 
                    ParenthesisLatexIfNeeded(frac.num) : 
                    frac.num.ToLatex();

            return denLatex == "1" ? numLatex : LatexUtils.Fraction(numLatex, denLatex);
        }
        
        var (cste, var) = SeparateConstant();

        var var_frac = AsMulFraction(var);
        if (var_frac.Num.IsNumOne && cste is Number num && num.Num.IsInt && !cste.IsNumOne && !cste.Is(-1))
            FractionLatex((cste, var_frac.Den));
        
        
        var var_latex = var.IsNumOne ? "" : FractionLatex(var_frac);
        var cste_latex = cste.IsNumOne ? "" : FractionLatex(AsMulFraction(cste));

        if (var_latex == "" && cste_latex == "")
            return "1";
        
        return cste_latex + var_latex;
    }

    private (Expr Num, Expr Den) AsMulFraction(Expr expr)
    {
        if (expr is Multiplication mul)
            return mul.Args.Select(AsMulFraction).Aggregate((frac1, frac2) => (frac1.Num * frac2.Num, frac1.Den * frac2.Den));
        
        if (expr is Power pow && pow.Exp is Number && pow.Exp.IsNegative)
            return (1, Pow(pow.Base, -pow.Exp));
        
        return (expr, 1);
    }
    
    // a*f(x) = a, f(x)
    public (Expr Constant, Expr Variable) AsIndependent(Variable var, bool exact=false)
    {
        Expr constant = 1;
        Expr variable = 1;
        foreach (var factor in Factors)
        {
            if (exact ? factor == var : !factor.Constant(var))
                variable *= factor;
            else
                constant *= factor;
            
        }

        return (constant, variable);
    }
    
    public override MpfrFloat NPrec(int precision = 333, MpfrRounding rnd = MpfrRounding.ToEven)
    {
        var result = new MpfrFloat(precision);
        result.Assign(1, rnd);
        foreach (var factor in Factors)
        {
            MpfrFloat.MultiplyInplace(result, result, factor.NPrec(precision, rnd), rnd);
        }

        return result;
    }
}