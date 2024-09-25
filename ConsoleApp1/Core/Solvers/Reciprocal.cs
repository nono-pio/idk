using System.Diagnostics;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Others;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Core.Sets;

namespace ConsoleApp1.Core.Solvers;

public class Reciprocal
{
    public static (Expr expr, Expr y) Unfold(Expr expr, Expr y, Variable variable)
    {
        if (expr.Constant(variable))
            throw new ArgumentException("expr must contain the variable.");
        
        while (true)
        {
            // x == y
            if (expr.IsVar(variable))
                return (expr, y);
            
            var varIndex = -1;
            for (var i = 0; i < expr.Args.Length; i++)
            {
                if (!expr.Args[i].Constant(variable))
                {
                    if (varIndex != -1) // multiple variables
                    {
                        varIndex = -2;
                        break;
                    }
                    varIndex = i;
                }
            }

            switch (varIndex)
            {
                case -1: // no variable (expr is constant)
                    throw new UnreachableException();
                case -2: // multiple variables
                    return (expr, y);
                default:
                {
                    var new_expr = expr.Args[varIndex];
                    y = expr.Reciprocal(y, varIndex);
                    expr = new_expr;
                    break;
                }
            }
        }

        throw new UnreachableException();
    }

    public static Expr? ReciprocalPattern(Expr expr, Variable variable)
    {
        
        // TODO: Subtitution cos(x)+e^cos(x) -> Xe^X où X=cos(x)
        
        // Polynomial
        var polyExpr = expr;
        if (Poly.IsPolynomial(polyExpr, variable))
        {
            var poly = Poly.ToPoly(polyExpr, variable);
            return new SetExpr(Set.CreateFiniteSet(poly.Solve())); // todo poly.Solve -> Expr
        }

        return null;
    }
}