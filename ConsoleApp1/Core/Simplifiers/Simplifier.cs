using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Base;

namespace ConsoleApp1.Core.Simplifiers;

public class Simplifier
{

    public static int CountVar(Expr expr)
    {
        return expr.GetVariables().Length;
    }

    public static int CountParenthesis(Expr expr)
    {
        var pcount = expr.Args.Sum(CountParenthesis);

        switch (expr)
        {
            case Addition:
                break;
            case Multiplication:
                pcount += expr.Args.Count(arg => arg.GetOrderOfOperation() < expr.GetOrderOfOperation());
                break;
            case Power pow:
                if (pow.Base.GetOrderOfOperation() < expr.GetOrderOfOperation())
                    pcount++;
                break;
        }

        return pcount;
    }

    public static int CountOperations(Expr expr)
    {
        var op = 0;
        expr.ForEach<Expr>(e =>
        {
            if (e.Args.Length != 0)
                op++;
        });

        return op;
    }

    public static int GetScore(Expr expr, int var_coef = 20, int parenth_coef = 1, int op_coef = 1)
    {
        return CountVar(expr) * var_coef + CountParenthesis(expr) * parenth_coef + CountOperations(expr) * op_coef;
    }

    public class BestExprComparer : IComparer<Expr>
    {
        public int Compare(Expr x, Expr y)
        {
            return GetScore(x).CompareTo(GetScore(y));
        }
    }

    public static Expr BestExpr(params Expr[] exprs)
    {
        return exprs.Min(new BestExprComparer())!;
    }
    
    public static Expr Simplify(Expr expr, bool deep = true, bool others_deep = true)
    {
        if (expr is Number)
            return expr;
        
        if (deep)
            expr = expr.MapArgs(e => Simplify(e, true, others_deep));

        expr = BestExpr(
            expr, 
            Expand(expr, deep: others_deep), 
            CombineFractions(expr, deep: others_deep)
            );

        if (expr.Has<Logarithm>())
        {
            expr = BestExpr(expr, ExpandLog(expr), CombineLog(expr));
        }
        
        return expr;
    }

    
    
    /// Combine Fractions in an expression
    /// - Addition : a/b + c/d -> (ad + bc)/bd
    /// - Multiplication : a/b * c/d -> ac/bd
    /// - Power : x^-2 -> 1/x^2
    public static Expr CombineFractions(Expr expr, bool deep = true, bool expNumber = true)
    {
        if (deep)
            expr = expr.MapArgs(arg => CombineFractions(arg, deep, expNumber));

        var (num, den) = expr.AsFraction(expNumber); // force the expr to be in fraction's shape
        
        return num / den;
    }

    /// Expand an expression
    /// - Multiplication : distribute (4*(x+2)(x+1) -> 4x^2 + 12x + 8)
    /// - Power :
    ///     - expand exp (x^(2+n) -> x^2 * x^n)
    ///     - distribute over mul ((3x)^4 -> 81x^4)
    ///     - multinomial / binomial ((x+1)^3 -> x^3 + 3x^2 + 3x + 1)
    public static Expr Expand(Expr expr, 
        bool deep = true, bool distribute_mul = true, bool expend_exp = false, bool distribute_pow = true, bool multinomial = true)
    {

        if (deep)
            expr = expr.MapArgs(arg => Expand(arg, deep, distribute_mul, expend_exp, distribute_pow, multinomial));

        switch (expr)
        {
            case Multiplication mul:
                return distribute_mul ? mul.DistributeOverAddition() : expr;
            case Power pow:
                
                if (expend_exp && pow.Exp is Addition eAdd)
                    return Mul(eAdd.Therms
                        .Select(n => Expand(Pow(pow.Base, n), false, distribute_mul, false, distribute_pow, multinomial)
                        ).ToArray());

                if (distribute_pow && pow.Base is Multiplication bmul)
                    return Mul(bmul.Factors.Select(fac => Pow(fac, pow.Exp)).ToArray());

                if (multinomial && pow.Base is Addition badd && pow.Exp.IsNumberIntPositif())
                    return Power.NewtonMultinomial(badd.Therms, pow.Exp.ToInt());
                break;
        }
        
        return expr;
    }
    
    /* Log Simplify */
    public static Expr CombineLog(Expr expr, bool deep = true)
    {
        if (deep)
            expr = expr.MapArgs(e => CombineLog(e, true));
        

        // TODO lnx/ln2 -> log_2x
        switch (expr)
        {
            case Addition add:

                var base_value = new Dictionary<Expr, Expr>(new Expr.ExprQuickComparer());
                foreach (var therm in add.Therms)
                {
                    if (therm is Logarithm log)
                    {
                        if (!base_value.TryAdd(log.Base, log.Value))
                            base_value[log.Base] *= log.Value;
                    }
                    else
                    {
                        if (!base_value.TryAdd(double.NaN, therm))
                            base_value[double.NaN] += therm;
                    }
                }

                return Add(base_value.Select(b_v => b_v.Key == double.NaN ? b_v.Value : Log(b_v.Value, b_v.Key)).ToArray());
                
            case Multiplication mul:
                var nlog = mul.Args.Count(e => e is Logarithm);
                if (nlog != 1)
                    break;

                var i = 0;
                Logarithm log2 = null;
                for (int j = 0; j < mul.Args.Length; j++)
                {
                    if (mul.Factors[j] is Logarithm l)
                    {
                        i = j;
                        log2 = l;
                        break;
                    }
                }

                var prod = Mul(mul.Args.Where((_, index) => index != i).ToArray());

                return Log(Pow(log2.Value, prod), log2.Base);
        }

        return expr;
    }

    public static Expr ExpandLog(Expr expr, bool deep = true)
    {
        if (!deep)
            return expr is Logarithm l ? ExpandLn(l.Value) / ExpandLn(l.Base) : expr;
        
        
        Expr ExpandLn(Expr arg)
        {
            return arg switch
            {
                Multiplication mul => Add(mul.Factors.Select(Ln).ToArray()),
                Power pow => pow.Exp * Ln(pow.Base),
                _ => Ln(arg)
            };
        }
        
        return expr.Map<Logarithm>(log => ExpandLn(log.Value) / ExpandLn(log.Base));
    }
}