﻿using ConsoleApp1.Core.Classes;
using ConsoleApp1.Core.Complexes;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Core.Sets;
using ConsoleApp1.Latex;
using Sdcb.Arithmetic.Mpfr;

namespace ConsoleApp1.Core.Expressions.Base;

public class Addition : Expr
{
    public Expr[] Therms => Args;

    public Addition(params Expr[] therms) : base(therms)
    {
        if (therms.Length < 2) 
            throw new Exception("You must add two or more therms");
    }

    public override (Expr Num, Expr Den) AsFraction(bool expNumber = true)
    {
        var fracs = Therms.Select(t => t.AsFraction(expNumber)).ToArray();

        Expr num = 0;
        for (int i = 0; i < fracs.Length; i++)
        {
            var therm = fracs[i].Num;
            for (int j = 0; j < fracs.Length; j++)
            {
                if (i == j)
                    continue;

                therm *= fracs[j].Den;
            }

            num += therm;
        }

        var den = fracs.Aggregate((Expr) 1, (pro, frac) => pro * frac.Den);

        return (num, den);
    }

    # region Constructors / Eval
    
    public static Expr Construct(params Expr[] exprs)
    {
        // Rules
        // 1. x + x = 2x
        // 2. x + 0 = x
        // 3. x + (y + z) = x + y + z
        // 4. Sort the terms
        
        if (exprs.Length == 0)
            return 0;
        if (exprs.Length == 1)
            return exprs[0];

        NumberStruct numbersSum = 0;
        var exprsSum = new List<Expr>();
        foreach (var expr in exprs.SelectMany(e => e is Addition add ? add.Therms : [e]))
        {
            if (expr is Number num)
            {
                numbersSum += num.Num;
                continue;
            }

            var hasCombined = false;
            for (int i = 0; i < exprsSum.Count; i++)
            {
                Expr? combine = Combine(expr, exprsSum[i]);
                if (combine is not null)
                {
                    if (combine.IsNumZero)
                        exprsSum.RemoveAt(i);
                    else 
                        exprsSum[i] = combine;
                    
                    hasCombined = true;
                    break;
                }
            }
        
            if (!hasCombined)
                exprsSum.Add(expr);
        }

        if (!numbersSum.IsZero)
            exprsSum.Add(Num(numbersSum));

        return exprsSum.Count switch
        {
            0 => 0,
            1 => exprsSum[0],
            _ => new Addition(Sorting.BubbleSort(exprsSum.ToArray()))
        };
    }
    private static Expr? Combine(Expr a, Expr b)
    {
        if (a is not Multiplication && b is not Multiplication)
            return a == b ? 2 * a : null;

        if (a is Multiplication aMul && b is Multiplication bMul)
        {
            var (csteA, varA) = aMul.AsMulCoef();
            var (csteB, varB) = bMul.AsMulCoef();

            if (varA != varB)
                return null;
            
            var sum = csteA + csteB;
            
            if (sum == 0)
                return 0;

            if (sum == 1)
                return varA;

            if (varA is Multiplication finalMul)
            {
                var exprs = new Expr[finalMul.Factors.Length + 1];

                exprs[0] = Num(sum);
                Array.Copy(finalMul.Factors, 0, exprs, 1, finalMul.Factors.Length);
                return aMul.NotEval(exprs);
            }

            return new Multiplication([Num(sum), varA]);
        }

        Multiplication mul;
        Expr other;
        if (a is Multiplication aMult)
        {
            mul = aMult;
            other = b;
        }
        else
        {
            mul = (Multiplication)b;
            other = a;
        }

        if (mul.Factors.Length != 2 || mul.Factors[0] is not Number)
            return null;

        if (mul.Factors[1] == other)
        {
            var factor = ((Number)mul.Factors[0]).Num + 1;
            return factor == 0 ? 0 : mul.NotEval([Num(factor), other]);
        }
    
        return null;
    }

    public static Expr AddOpti(Expr a, Expr b)
    {
        if (a is Addition || b is Addition)
            return Construct(a, b);
        
        if (a is Number numA && b is Number numB)
            return new Number(numA.Num + numB.Num);
        
        if (a.IsNumZero)
            return b;
        if (b.IsNumZero)
            return a;


        var result = Combine(a, b);
        if (result is not null)
            return result;

        return new Addition(a, b);
    }
    
    public override Expr Eval(Expr[] args, object[]? objects = null) => Construct(args);
    public override Expr NotEval(Expr[] args, object[]? objects = null) => new Addition(args);
    
    # endregion

    public override bool IsNatural => Therms.All(therm => therm.IsNatural);
    public override bool IsInteger => Therms.All(therm => therm.IsInteger);
    public override bool IsRational => Therms.All(therm => therm.IsRational);
    public override bool IsReal => Therms.All(therm => therm.IsReal);
    public override bool IsComplex => Therms.All(therm => therm.IsComplex);


    public override Set AsSet()
    {
        Set SetAAddSetB(Set setA, Set setB) => ArithmeticOnSets.BiCommutativeFunctionOnSet(
            (el1, el2) => el1 + el2, setA, setB, 
            interval: (interval, interval1) => interval.ArithmeticAdd(interval1),
            expr_interval: (expr, interval) => Interval(interval.Start + expr, interval.End+expr, interval.StartInclusive, interval.EndInclusive),
            bns: (bnsA, bnsB) => bnsA._Level >= bnsB._Level ? bnsA : bnsB,
            bns_integral: (bns, interval) => bns is Real ? bns : null, // question: Q + [x1,x2] = R x1 != x2; TODO: N/Z + [x1, x2] = R si x2-x1 > 1
            expr_bns: (expr, bns) => bns switch
            {
                Natural => throw new NotImplementedException(), // si x in Z -> range(x, oo)
                Integer => expr.IsInteger ? bns : throw new NotImplementedException(), // si x in Q/R/.., ex x=1/2 -> {-oo...-3/2,-1/2,1/2,3/2...oo}
                Rational => expr.IsRational ? bns : throw new NotImplementedException(), // idem R/..
                Real => expr.IsReal ? bns : throw new NotImplementedException(), // idem C/..
                _ => throw new NotImplementedException()
            }
            );


        return Therms.Skip(1).Aggregate(Therms[0].AsSet(), (result, therm) => SetAAddSetB(result, therm.AsSet()));
    }

    public override Complex AsComplex()
    {
        return Therms.Aggregate<Expr, Complex>(0, (current, term) => current + term.AsComplex());
    }

    public override OrderOfOperation GetOrderOfOperation()
    {
        return OrderOfOperation.Addition;
    }
    
    # region Derivee

    public override Expr fDerivee(int argIndex) => 1;

    public override Expr Derivee(Variable variable)
    {
        var newThermes = new Expr[Therms.Length];
        for (var i = 0; i < Therms.Length; i++) 
            newThermes[i] = Therms[i].Derivee(variable);

        return Add(newThermes);
    }

    public override Expr Derivee(Variable variable, int n)
    {
        var newThermes = new Expr[Therms.Length];
        for (var i = 0; i < Therms.Length; i++) 
            newThermes[i] = Therms[i].Derivee(variable, n);

        return Add(newThermes);
    }
    
    # endregion

    public override Expr Reciprocal(Expr y, int argIndex)
    {
        // a+b : inv(c, 0) -> c-b
        // a+b : inv(c, 1) -> c-a

        var rest = new Expr[Therms.Length];
        for (var i = 0; i < Therms.Length; i++)
        {
            if (i == argIndex) 
                continue;

            if (i < argIndex)
                rest[i] = -Therms[i];
            else
                rest[i - 1] = -Therms[i];
        }

        rest[^1] = y;

        return Add(rest);
    }

    public override double N()
    {
        return Therms.Sum(t => t.N());
    }

    public override string? ToString()
    {
        var result = ParenthesisIfNeeded(Therms[0]);

        for (var i = 1; i < Therms.Length; i++)
        {
            var str = ParenthesisIfNeeded(Therms[i]);

            if (str.StartsWith('-')) // if str = -... add it directly
                result += str;
            else
                result += '+' + str;
        }
        
        return result;
    }

    public override string ToLatex()
    {

        var vars = GetVariables();
        if (vars.Length == 1 && Poly.IsDirectPolynomial(this, vars[0]))
        {
            var poly = Poly.ToPoly(this, vars[0]);
            return poly.ToLatex(vars[0].Name);
        }
        
        var result = ParenthesisLatexIfNeeded(Therms[0]);

        for (var i = 1; i < Therms.Length; i++)
        {
            var str = ParenthesisLatexIfNeeded(Therms[i]);

            if (str.StartsWith(Symbols.Sub)) // if str = -... add it directly
                result += str;
            else
                result += Symbols.Add + str;
        }
        
        return result;
    }
    public (Expr Constant, Expr Variable) AsIndependent(Variable var, bool exact=false)
    {
        Expr constant = 0;
        Expr variable = 0;
        foreach (var therm in Therms)
        {
            if (exact ? therm == var : !therm.Constant(var))
                variable += therm;
            else
                constant += therm;
            
        }

        return (constant, variable);
    }

    public override MpfrFloat NPrec(int precision = 333, MpfrRounding rnd = MpfrRounding.ToEven)
    {
        var result = new MpfrFloat(precision);
        foreach (var therm in Therms)
        {
            MpfrFloat.AddInplace(result, result, therm.NPrec(precision, rnd), rnd);
        }

        return result;
    }
}