using System.Diagnostics;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Others;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Core.Sets;

namespace ConsoleApp1.Core.Solvers;

public class Reciprocal
{
    
    public static Expr? GetReciprocal(Expr expr, Variable variable, Variable? y = null)
    {
        y = y ?? new Variable("x", dummy:true);
        // expr(x) = y
        
        var (rest, reciprocal) = Unfold(expr, y, variable); // rest(x) = reciprocal(y)
        
        if (rest == variable) // rest(x) = x 
            return reciprocal;

        return null;
    }
    
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
    
    public static (Expr exprs, List<Expr> ys) Unfolds(Expr expr, Expr y, Variable variable)
    {
        if (expr.Constant(variable))
            throw new ArgumentException("expr must contain the variable.");

        List<Expr> ys = [y];
        
        while (true)
        {
            // x == y
            if (expr.IsVar(variable))
                return (expr, ys);
            
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
                    return (expr, ys);
                default:
                {
                    var new_expr = expr.Args[varIndex];
                    var nys = ys.Count;
                    for (int i = 0; i < nys; i++)
                    {
                        var new_ys = expr.AllReciprocal(ys[i], varIndex);
                        if (new_ys.Length == 0)
                            throw new NotImplementedException();
                        else if (new_ys.Length == 1)
                            ys[i] = new_ys[0];
                        else
                        {
                            ys[i] = new_ys[0];
                            ys.AddRange(new_ys.Skip(1));
                        }
                    }
                    
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
            return new SetExpr(ArraySet(poly.Solve())); // todo poly.Solve -> Expr
        }

        return null;
    }
}