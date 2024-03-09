using ConsoleApp1.utils;

namespace ConsoleApp1.Core.Expr.Fonctions.Base;

public class Multiplication : Expr
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
        ;
    }

    public Expr Eval()
    {

        var newFactors = new List<Expr>(Factors.Length);

        // 1. Flatting                      mul(mul(x,y),z) -> mul(x,y,z)
        // 2. Identity removing (One)       mul(1, x) -> x
        // 3. Zero return                   mul(x, 0) -> 0

        foreach (var factor in Factors)
        {
            if (factor.IsZero()) return 0.Expr();

            if (factor.IsOne()) continue;

            if (factor is Multiplication mul)
                newFactors.AddRange(mul.Factors);
            else
                newFactors.Add(factor);
        }

        // 4. Coefficient Grouping      mul(x,y,x) -> mul(x^2,y)
        // 5. Constant Grouping         mul(2,2) -> 4

        /*
         * Dictionary {
         *  expr (Expr) : coefficient (Float)
         * }
         */

        double constant = 1;
        var coefficientMap = new Dictionary<Expr, double>();

        foreach (var factor in newFactors)
        {
            var (coef, rest) = factor.AsMulCoef();


            if (rest is null)
            {
                constant *= coef;
                continue;
            }

            Console.WriteLine("Create dico > " + coef + ";" + rest);

            Console.WriteLine("Has rest > " + coefficientMap.ContainsKey(rest));
            if (!coefficientMap.TryAdd(rest, coef))
            {
                Console.WriteLine("Add");
                coefficientMap[rest] += coef;
            }
        }

        var i = 0;
        if (constant == 1)
        {
            Factors = new Expr[coefficientMap.Count];
        }
        else
        {
            Factors = new Expr[coefficientMap.Count + 1];
            Factors[0] = constant.Expr();
            i = 1;
        }

        foreach (var (expr, coef) in coefficientMap)
        {
            Console.WriteLine("dico > " + expr + ";" + coef);
            Factors[i] = coef == 1 ? expr : Pow(expr, coef.Expr());
            i++;
        }

        // 6. Factors Length             0 -> 1; 1 -> therm

        if (Factors.Length == 0) return 1.Expr();

        if (Factors.Length == 1) return Factors[0];

        // 7. Factors sorting            mul(x,y,2) -> mul(2,x,y)

        SortArgs();

        return this;
    }


    // TODO
    public override (double, Expr?) AsMulCoef()
    {
        return base.AsMulCoef();
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