using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Base;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Core.Sets;

namespace ConsoleApp1.Core.Solvers;

public class Solve
{
    
    public static Set? SolveFor(Expr expr, Expr y, Variable variable) => FindRoots(expr - y, variable);
    
    public static Set? FindRoots(Expr f, Variable variable)
    {
        if (f.Constant(variable))
        {
            return f.IsZero ? Set.R/*return x domain or R*/ : Set.EmptySet;
        }

        (f, var y) = Reciprocal.Unfold(f, 0, variable);

        if (f.IsVar(variable))
            return y.AsSet();
        
        return MatchPattern(f, y, variable);
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
                return Set.CreateUnion(solutions.ToArray());    
            }
        }
        
        
        // Polynomial
        var polyExpr = expr - y;
        if (Poly.IsPolynomial(polyExpr, variable))
        {
            var poly = Poly.ToPoly(polyExpr, variable);
            return Set.CreateFiniteSet(poly.Solve());
        }
        
        // change variable, exemple :
        // f(x) = x^2 + x^4 + x^6 + x^8 + x^10
        // u = x^2
        // f(u) = u + u^2 + u^3 + u^4 + u^5

        // Find Paterns
        
        
        return null;
    }
}