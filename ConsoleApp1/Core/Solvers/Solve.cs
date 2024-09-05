using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Base;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Core.Sets;

namespace ConsoleApp1.Core.Solvers;

public class Solve
{

    // expr(var) = y(var)
    public static Set? SolveFor(Expr expr, Expr y, string variable) => FindRoots(expr - y, variable);
    
    // f(x) = 0 
    public static Set? FindRoots(Expr f, string variable)
    {
        // If f is constant : True or False (ex: 0 = 0 -> True or 1 = 0 -> False)
        if (f.Constant(variable))
        {
            return f.IsZero ? Set.R/*return x domain or R*/ : Set.EmptySet;
        }
        
        return UnfoldReciprocal(f, variable);
    }

    // TODO
    // Brute = expr relie plusieurs f(var)
    // expr(var) et y est une cste
    public static Set? SolveBrute(Expr expr, Expr y, string variable)
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

    public static Set? UnfoldReciprocal(Expr f, string variable)
    {
        Expr expr = f; // variable
        Expr y = Zero; // constante
        
        while (true)
        {
            // expr = y
            
            // If expr = x -> x = y
            if (expr.IsVar(variable))
                return Set.CreateFiniteSet(y);

            // find the variable in the expression
            var index = -1; // index of the variable in the expression (ex: x+3 -> x is at index 0)
            for (var i = 0; i < expr.Args.Length; i++)
            {
                if (!expr.Args[i].Constant(variable))
                {
                    if (index != -1) // multiple variables (reciprocal dont work)
                    {
                        index = -2;
                        break;
                    }
                    index = i;
                }
            }

            switch (index)
            {
                case -1: // no variable (expr is constant)
                    return expr == y ? Set.R/*x domain or R*/: Set.EmptySet;
                case -2: // multiple variables
                    return SolveBrute(expr, y, variable);
                default:
                {
                    var new_expr = expr.Args[index];
                    y = expr.Reciprocal(y, index);
                    expr = new_expr;
                    break;
                }
            }
        }

        return null; // unreachable
    }
}