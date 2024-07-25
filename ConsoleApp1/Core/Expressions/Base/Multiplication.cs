using ConsoleApp1.Core.Classes;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Latex;

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
            0 => 0,
            1 => exprsFactors[0],
            _ => new Multiplication(Sorting.BubbleSort(exprsFactors.ToArray()))
        };
    }
    private static Expr? Combine(Expr a, Expr b)
    {
        if (a is not Power && b is not Power)
            return null;

        if (a is Power aPow && b is Power bPow)
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

        if (pow.Base == expr)
        {
            var newExp = pow.Exp + 1;
            if (newExp.IsZero())
                return 1;
            
            return Pow(pow.Base, newExp);
        }

        return null;
    }

    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Construct(exprs);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => new Multiplication(exprs);

    # endregion

    public override Expr Derivee(string variable)
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
        var therms = new List<Expr>();
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
                for (int i = 0; i < therms.Count; i++)
                {
                    therms[i] = Mul(therms[i], factor);
                }
            }
        }

        return Add(therms.ToArray());
    }

    public override Expr Derivee(string variable, int n)
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
        
        var result = ParenthesisLatexIfNeeded(Factors[0]);
        var isM1 = result == "-1";

        for (var i = 1; i < Factors.Length; i++)
        {
            // TODO : Check if the pow is negative : * -> /
            result += Symbols.Mul + ParenthesisLatexIfNeeded(Factors[i]);
        }
        
        return isM1 ? '-' + result[3..] : result;
    }
}