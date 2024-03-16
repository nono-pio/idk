using ConsoleApp1.Core.Expressions.Atoms;

namespace ConsoleApp1.Core.Expressions.Base;

public class Multiplication : Expr, ICoefGrouping<Multiplication>
{
    public Multiplication(params Expr[] factors) : base(factors)
    {
        if (factors.Length < 2) throw new Exception("You must add two or more factor");
    }

    public Expr[] Factors
    {
        get => Args;
        set => Args = value;
    }

    public override Expr Derivee(string variable)
    {
        var therms = new Expr[Factors.Length];
        for (var i = 0; i < Factors.Length; i++)
        {
            var facteurs = new Expr[Factors.Length];
            for (var j = 0; j < Factors.Length; j++) facteurs[j] = i == j ? Factors[j].Derivee(variable) : Factors[j];

            therms[i] = Mul(facteurs);
        }

        return Add(therms);
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
    public double Absorbent() => 0;
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
        List<Expr> newFactors = new List<Expr>();
        foreach (var factor in Factors)
        {
            if (factor is Number num)
            {
                coef *= num.Num;
            } else
            {
              newFactors.Add(factor);  
            }
        }

        return newFactors.Count switch
        {
            0 => ((double, Expr?))(coef, null),
            1 => (coef, newFactors[0]),
            _ => (coef, new Multiplication(newFactors.ToArray()))
        };
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