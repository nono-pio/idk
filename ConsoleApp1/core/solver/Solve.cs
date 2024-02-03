namespace ConsoleApp1.core.solver;

public class Solve
{
    // expr(var) = y(var) -> Expr
    public static Expr SolveFor(Expr expr, Expr y, string variable)
    {
        if (!y.Constant(variable) && !y.IsZero())
        {
            expr = expr - y;
            y = Zero;
        }

        if (expr.IsVar(variable)) return y;

        // -1 -> pas de var; -2 -> plusieurs var; i>0 -> var a l'index i
        var varIndex = -1;
        for (var i = 0; i < expr.Args.Length; i++)
            if (!expr.Args[i].Constant(variable))
            {
                // deja un var
                if (varIndex != -1)
                {
                    varIndex = -2;
                    break;
                }

                // pas de var
                varIndex = i;
            }

        // pas de var
        if (varIndex == -1) throw new Exception("pas de var");

        if (varIndex == -2) // plusieurs var
        {
            return SolveBrute(expr, y, variable);
        }

        // 1) expr = 2*x + 3; y = 0
        // 2) expr = 2x; y = 0-3
        y = expr.Inverse(y, varIndex);
        expr = expr.Args[varIndex];
        return SolveFor(expr, y, variable);
    }

    // TODO
    // Brute = expr relie plusieurs f(var)
    // expr(var) et y est une cste
    public static Expr SolveBrute(Expr expr, Expr y, string variable)
    {
        /* Patternes :
         * - Polynome
         * -
         */

        return expr;
    }
}