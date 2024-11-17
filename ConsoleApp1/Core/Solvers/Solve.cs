using ConsoleApp1.Core.Booleans;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Base;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Core.Sets;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Solvers;

public class Solve
{
    
    public static Set? SolveFor(Expr expr, Expr y, Variable variable) => FindRoots(expr - y, variable);
    
    public static Set? FindRoots(Expr f, Variable variable)
    {
        var period = Period.FindPeriod(f, variable);
        
        if (f.Constant(variable))
        {
            return f.IsZero ? R/*return x domain or R*/ : EmptySet;
        }

        (f, var ys) = Reciprocal.Unfolds(f, 0, variable);

        Set result;
        if (f.IsVar(variable))
            result =  ArraySet(ys.ToArray());
        else
        {
            result = EmptySet;
            foreach (var y in ys)
            {
                var xs = MatchPattern(f, y, variable);
                if (xs is not null)
                    result = result.UnionWith(xs);
            }    
        }

        if (period is not null && !period.IsZero)
        {
            var dummy_n = new Variable("n", dummy: true);
            if (result is FiniteSet fs)
            {
                return Union(fs.Elements.Select(e => LambdaSet(e + dummy_n * period, In.Eval(dummy_n, Z), [dummy_n])).ToArray());
            }
            
            var dummy_x = new Variable("x", dummy: true);
            return LambdaSet(dummy_x + dummy_n * period, Boolean.And(In.Eval(dummy_x, result), In.Eval(dummy_n, Z)), [dummy_x, dummy_n]);
        }

        return result;
    }
    
    public static Set? MatchPattern(Expr expr, Expr y, Variable variable)
    {
        // Multiplication
        // f1(x) * f2(x) = 0 -> f1(x) = 0 or f2(x) = 0
        if (expr is Multiplication mul && y.IsZero)
        {
            List<Set> solutions = new();
            var isAllSolvable = true;
            foreach (var f in mul.Factors)
            {
                var sol = FindRoots(f, variable);
                if (sol is null)
                {
                    isAllSolvable = false;
                    break;
                }
                
                solutions.Add(sol);
            }

            if (isAllSolvable)
            {
                return Union(solutions.ToArray());    
            }
        }
        
        
        // Polynomial
        var polyExpr = expr - y;
        if (Poly.IsPolynomial(polyExpr, variable))
        {
            var poly = Poly.ToPoly(polyExpr, variable);
            return ArraySet(poly.Solve());
        }

        if (PolyRational.IsPolyRational(polyExpr, variable))
        {
            var poly = PolyRational.ToPolyRational(polyExpr, variable);
            return ArraySet(poly.Num.Solve());
        }
        
        // change variable, exemple :
        // f(x) = x^2 + x^4 + x^6 + x^8 + x^10
        // u = x^2
        // f(u) = u + u^2 + u^3 + u^4 + u^5

        // Find Paterns
        
        
        return null;
    }
}