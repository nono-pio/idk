using ConsoleApp1.Core.Models;

namespace ConsoleApp1.Core.Solvers;

public enum Solution
{
    None,
    All,
    Some,
    Unsolved
}

public class Solve
{

    // expr(var) = y(var)
    public static Solution SolveFor(Expr expr, Expr y, string variable) => FindRoots(expr - y, variable);
    
    // f(x) = 0 
    public static Solution FindRoots(Expr f, string variable)
    {
        // If f is constant : True or False (ex: 0 = 0 -> True or 1 = 0 -> False)
        if (f.Constant(variable))
        {
            return f.IsZero() ? Solution.All : Solution.None;
        }
        
        return UnfoldReciprocal(f, variable);
    }

    // TODO
    // Brute = expr relie plusieurs f(var)
    // expr(var) et y est une cste
    public static Solution SolveBrute(Expr expr, Expr y, string variable)
    {
        
        // Polynomial
        if (Poly.IsPolynomial(expr, variable))
        {
            return Solution.Some; //Poly.ToPoly(expr, variable).Solve();
        }
        
        // change variable, exemple :
        // f(x) = x^2 + x^4 + x^6 + x^8 + x^10
        // u = x^2
        // f(u) = u + u^2 + u^3 + u^4 + u^5

        // Find Paterns
        
        
        return Solution.Unsolved;
    }

    public static Solution UnfoldReciprocal(Expr f, string variable)
    {
        Expr expr = f; // variable
        Expr y = Zero; // constante
        
        while (true)
        {
            // expr = y
            
            // If expr = x -> x = y
            if (expr.IsVar(variable))
                return Solution.Some;//y;

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
                    return expr == y ? Solution.All : Solution.None;
                case -2: // multiple variables
                    return SolveBrute(f, Zero, variable);
                default:
                {
                    expr = f.Args[index];
                    y = f.Args[index].Reciprocal(y, index);
                    break;
                }
            }
        }

        return Solution.None; // unreachable
    }
}