namespace ConsoleApp1.Utils;

// associative, commutative, identity, groupping, absorbant
// (a+b)+c = a+(b+c) = a+b+c, a+b = b+a, a+0 = a, a+a = 2a, a*0 = 0

public interface ICoefGrouping<out T> where T : Expr, ICoefGrouping<T>
{
    double Identity();
    double Absorbent();
    double GroupConstant(double a, double b);
    (double, Expr?) AsCoefExpr(Expr expr);
    T FromArrayList(Expr[] exprs);
    Expr GroupCoefExpr(double coef, Expr expr);

    public Expr GroupEval() => GroupEval((T) this);
    public static Expr GroupEval(T mainExpr)
    {
        
        var values = new List<(double, Expr)>();
        var id = mainExpr.Identity();
        
        var (is_absorb, constant) = AddRange(values, mainExpr);

        if (is_absorb)
        {
            return mainExpr.Absorbent().Expr();
        }

        for (int i = values.Count - 1; i >= 0; i--)
        {
            if (values[i].Item1 == 0)
            {
                values.RemoveAt(i);
            }
        }

        if (constant != id)
        {
            values.Add((1, constant.Expr()));
        }

        switch (values.Count)
        {
            case 0: return id.Expr();
            case 1: return Build(mainExpr, values[0]);
            default:
            {
                var exprs = values.Select(val => Build(mainExpr, val)).ToArray();
                exprs = Sorting.BubbleSort(exprs);
                return mainExpr.FromArrayList(exprs);
            }
        }
    }

    private static Expr Build(T mainExpr, (double, Expr) value) => Build(mainExpr, value.Item1, value.Item2);
    private static Expr Build(T mainExpr, double coef, Expr expr)
    {
        return coef == mainExpr.Identity() ? expr : mainExpr.GroupCoefExpr(coef, expr);
    }

    private static (bool, double) AddRange(List<(double, Expr)> values, T mainExpr)
    {
        
        var constant = mainExpr.Identity();
        foreach (var expr in mainExpr.Args)
        {

            if (expr is T exprT)
            {
                var (is_absorb, new_constant) = AddRange(values, exprT);
                if (is_absorb)
                    return (true, Double.NaN);
                
                constant = mainExpr.GroupConstant(constant, new_constant);
                continue;
            }

            var (coef, val) = mainExpr.AsCoefExpr(expr);

            if (coef == mainExpr.Absorbent())
            {
                return (true, Double.NaN);
            }

            if (val is null)
            {
                constant = mainExpr.GroupConstant(constant, coef);
                continue;
            }
            
            Add(values, coef, val);
        }

        return (false, constant);
    }

    private static void Add(List<(double, Expr)> values, double coef, Expr expr)
    {
        for (int i = 0; i < values.Count; i++)
        {
            if (values[i].Item2 == expr)
            {
                values[i] = (values[i].Item1 + coef, expr);
                return;
            }
        }
        
        values.Add((coef, expr));
    }
}