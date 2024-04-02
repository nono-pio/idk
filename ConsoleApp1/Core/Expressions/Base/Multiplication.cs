using ConsoleApp1.Core.Expressions.Atoms;

namespace ConsoleApp1.Core.Expressions.Base;

public class Multiplication : Expr, ICoefGrouping<Multiplication>
{
    public Expr[] Factors => Args;
    
    # region Constructors
    
    public Multiplication(params Expr[] factors) : base(factors)
    {
        if (factors.Length < 2) throw new Exception("You must add two or more factor");
    }
    
    public static Expr Mul(params Expr[] factors) => factors.Length switch
    {
        0 => Un,
        1 => factors[0],
        _ => new Multiplication(factors).Eval()
    };
    
    public static Expr MulNotEval(params Expr[] factors) => factors.Length switch
    {
        0 => Un,
        1 => factors[0],
        _ => new Multiplication(factors)
    };
    
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

    public override Expr Inverse(Expr y, int argIndex)
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

        return y + Add(rest);
    }

    public double Identity() => 1;
    public Expr IdentityExpr() => Un;
    public (double, double) IdentityComplex() => (1, 0);
    public (Expr, Expr) IdentityComplexExpr() => (Un, Zero);
    
    public double Absorbent() => 0;
    public Expr AbsorbentExpr() => Zero;
    public (double, double) AbsorbentComplex() => (0, 0);
    public (Expr, Expr) AbsorbentComplexExpr() => (Zero, Zero);
    
    
    
    public double GroupConstant(double a, double b) => a * b;
    public (double, Expr?) AsCoefExpr(Expr expr) => expr is Number num ? (num.Num, null) : (1, expr); // TODO
    public Multiplication FromArrayList(Expr[] exprs) => new Multiplication(exprs);
    public Expr GroupCoefExpr(double coef, Expr expr) => coef == 0 ? Zero : Pow(expr, coef.Expr());

    public Expr Eval()
    {
        return ICoefGrouping<Multiplication>.GroupEval(this);
    }
    
    public override (double, Expr?) AsMulCoef()
    {
        double coef = 1;
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

        return (coef, MulNotEval(newFactors.ToArray()));
    }

    public override (Expr, Expr) AsComplex()
    {
        return Factors.Aggregate(IdentityComplexExpr(), 
            (complexTuple, factor) => ComplexUtils.Mul(complexTuple, factor.AsComplex()));
    }

    public override string? ToString()
    {
        return Join("*");
    }

    public override string? ToLatex()
    {
        return JoinLatex("\\cdot");
    }
}