using ConsoleApp1.utils;

namespace ConsoleApp1.core.expr.fonctions.@base;

public class Addition : Expr
{
    public Addition(params Expr[] therms) : base(therms)
    {
        if (therms.Length < 2) throw new Exception("You must add two or more therms");
    }

    public Expr[] Therms
    {
        get => Args;
        set => Args = value;
    }

    public override Expr Derivee(string variable)
    {
        var newThermes = new Expr[Therms.Length];
        for (var i = 0; i < Therms.Length; i++) newThermes[i] = Therms[i].Derivee(variable);

        return Add(newThermes);
    }

    public override Expr Inverse(Expr y, int argIndex)
    {
        // a+b : inv(c, 0) -> c-b
        // a+b : inv(c, 1) -> c-a


        var rest = new Expr[Therms.Length - 1]; // 1+
        for (var i = 0; i < Therms.Length; i++)
        {
            if (i == argIndex) continue;

            if (i < argIndex)
                rest[i] = -Therms[i];
            else
                rest[i - 1] = -Therms[i];
        }

        return y + Add(rest);
        ;
    }

    public Expr Eval()
    {
        var newTherms = new List<Expr>(Therms.Length);

        // 1. Flatting                      add(add(x,y),z) -> add(x,y,z)
        // 2. Identity removing (Zero)      add(0, x) -> x

        foreach (var therm in Therms)
        {
            if (therm.IsZero()) continue;

            if (therm is Addition add)
                newTherms.AddRange(add.Therms);
            else
                newTherms.Add(therm);
        }

        // 3. Coefficient Grouping      add(x,y,x) -> add(2x,y)
        // 4. Constant Grouping         add(2,2) -> 4

        /*
         * Dictionary {
         *  expr (Expr) : coefficient (Float)
         * }
         */

        double constant = 0;
        var coefficientMap = new Dictionary<Expr, double>();

        foreach (var term in newTherms)
        {
            var (coef, rest) = term.HasMulCoef();

            if (rest is null)
            {
                constant += coef;
                continue;
            }

            if (!coefficientMap.TryAdd(rest, coef)) coefficientMap[rest] += coef;
        }

        var i = 0;
        if (constant == 0)
        {
            Therms = new Expr[coefficientMap.Count];
        }
        else
        {
            Therms = new Expr[coefficientMap.Count + 1];
            Therms[0] = constant.Expr();
            i = 1;
        }

        foreach (var (expr, coef) in coefficientMap)
        {
            Therms[i] = coef == 1 ? expr : Mul(coef.Expr(), expr);
            i++;
        }

        // 4. Therms Length             0 -> 0; 1 -> therm

        if (Therms.Length == 0) return Num(0);

        if (Therms.Length == 1) return Therms[0];

        // 5. Therms sorting            add(x,y,2) -> add(2,x,y)

        SortArgs();

        return this;
    }

    public override string? ToString()
    {
        return Join("+");
    }

    public override string? ToLatex()
    {
        return JoinLatex("+");
    }
}