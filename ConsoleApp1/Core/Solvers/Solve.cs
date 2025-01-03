using System.Diagnostics;
using ConsoleApp1.Core.Booleans;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Base;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Core.Sets;
using ConsoleApp1.Core.Simplifiers;
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

    /// test if expr is a linear combination of vars
    private static bool IsLinear(Expr expr, Variable[] vars)
    {
        if (vars.All(expr.Constant))
            return true;

        return expr switch
        {
            Addition add => add.Therms.All(t => IsLinear(t, vars)),
            Multiplication mul => mul.Factors.All(f => IsLinear(f, vars)), // TODO change ex : x*x is not linear
            Variable v => vars.Contains(v),
            _ => false
        };
    }

    /// solve expr = 0 when expr is a linear combination of x
    private static Expr LinearValue(Expr expr, Variable x)
    {
        expr = Simplifier.Expand(expr);
        var (newExpr, value) = Reciprocal.Unfold(expr, 0, x);
        Debug.Assert(newExpr.IsVar(x));
        return value;
    }

    public static Expr[]? SolveEquations((Expr, Expr)[] equations, Variable[] variables)
    {
        var eqs = equations.Select(eq => eq.Item1 - eq.Item2).ToList();
        for (int i = eqs.Count - 1; i >= 0; i--)
        {
            var vars = eqs[i].GetVariables().Where(variables.Contains).ToArray();
            if (vars.Length == 0) // useless equation
            {
                if (!eqs[i].IsZero)
                    return null;
                
                eqs.RemoveAt(i);
            }
        }
        
        
        if (eqs.All(eq => IsLinear(eq, variables)))
        {
            /* Solve for all univariate Equations and subtitute in the others equations */
            var values = new Expr?[variables.Length];
            var toSolve = variables.Length;
            var n = 0;
            while (toSolve != n)
            {
                n = toSolve;
                for (int i = eqs.Count - 1; i >= 0; i--)
                {
                    var vars = eqs[i].GetVariables().Where(variables.Contains).ToArray();
                    if (vars.Length == 0) // useless equation
                    {
                        if (!eqs[i].IsZero)
                            return null;
                
                        eqs.RemoveAt(i);
                    }
                    if (vars.Length == 1)
                    {
                        var index = Array.IndexOf(variables, vars[0]);
                        var value = LinearValue(eqs[i], vars[0]);

                        if (values[index] is not null)
                        {
                            if (values[index] != value)
                                return null;
                        
                            continue;
                        }
                    
                        values[index] = value;
                        eqs.RemoveAt(i);
                        toSolve--;
                        
                        for (int j = 0; j < eqs.Count; j++)
                        {
                            eqs[j] = eqs[j].Substitue(variables[index], value);
                        }
                    }
                }
            }
            
            if (toSolve == 0)
            {
                Debug.Assert(values.All(v => v is not null));
                return values;
            }

            var newVariables = variables.Where((_, i) => values[i] is null).ToArray();
            if (eqs.Count < newVariables.Length)
                return null;

            var firstEqs = eqs[..newVariables.Length];
            var (matrix_coef, cste) = CreateMatrix(firstEqs, newVariables);
            var solutions = CramerLinearSystemSolver(matrix_coef, cste);
            
            if (solutions is null)
                return null;
            
            for (int i = 0; i < newVariables.Length; i++)
            {
                values[Array.IndexOf(variables, newVariables[i])] = solutions[i];
            }
            
            if (eqs.Count > newVariables.Length)
            {
                for (int i = newVariables.Length; i < eqs.Count; i++)
                {
                    var value = eqs[i].Substitue(variables, values);
                    if (!value.IsZero)
                        return null;
                }
            }
            
            Debug.Assert(values.All(v => v is not null));
            return values;
        }

        throw new NotImplementedException();
    }
    
    private static (Expr[,], Expr[]) CreateMatrix(List<Expr> eqs, Variable[] variables)
    {
        var n = eqs.Count;
        var matrix = new Expr[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                matrix[i, j] = 0;
        
        var cste = new Expr[n];
        Array.Fill(cste, Zero);

        for (int i = 0; i < eqs.Count; i++)
        {
            var eq = Simplifier.Expand(eqs[i]);
            for (int j = 0; j < eqs.Count; j++)
            {
                (eq, var coef) = eq.AsLinear(variables[j])!.Value; // write eq as newEq + coef * variables[j]
                Debug.Assert(variables.All(coef.Constant));
                matrix[i, j] = coef;
            }
            Debug.Assert(variables.All(eq.Constant));
            cste[i] = -eq;
        }
        
        return (matrix, cste);
    }
    
    // Determinant of a matrix
    private static Expr Det(Expr[,] matrix)
    {
        int n = matrix.GetLength(0);

        // Cas de base : matrice 1x1
        if (n == 1)
        {
            return matrix[0, 0];
        }

        // Cas de base : matrice 2x2
        if (n == 2)
        {
            return matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];
        }

        // Cas général : développement par les mineurs (cofactor expansion)
        Expr determinant = 0;
        for (int col = 0; col < n; col++)
        {
            Expr[,] subMatrix = GetSubMatrix(matrix, 0, col);
            Expr cofactor = (col % 2 == 0 ? 1 : -1) * matrix[0, col] * Det(subMatrix);
            determinant += cofactor;
        }

        return determinant;
    }
    
    public static Expr[]? CramerLinearSystemSolver(Expr[,] coefficients, Expr[] constants)
    {
        int n = coefficients.GetLength(0);

        // Calcul du déterminant principal
        Expr detA = Det(coefficients);
        if (detA == 0)
        {
            return null;
        }

        // Calcul des solutions
        Expr[] solutions = new Expr[n];
        for (int i = 0; i < n; i++)
        {
            // Remplacer la i-ème colonne par le vecteur des constantes
            Expr[,] modifiedMatrix = ReplaceColumn(coefficients, constants, i);
            Expr detAi = Det(modifiedMatrix);
            solutions[i] = detAi / detA;
        }

        return solutions;
    }

    private static Expr[,] GetSubMatrix(Expr[,] matrix, int excludeRow, int excludeCol)
    {
        int n = matrix.GetLength(0);
        Expr[,] subMatrix = new Expr[n - 1, n - 1];
        int subRow = 0;

        for (int row = 0; row < n; row++)
        {
            if (row == excludeRow) continue;

            int subCol = 0;
            for (int col = 0; col < n; col++)
            {
                if (col == excludeCol) continue;

                subMatrix[subRow, subCol] = matrix[row, col];
                subCol++;
            }

            subRow++;
        }

        return subMatrix;
    }

    private static Expr[,] ReplaceColumn(Expr[,] matrix, Expr[] column, int colIndex)
    {
        int n = matrix.GetLength(0);
        Expr[,] newMatrix = (Expr[,])matrix.Clone();

        for (int row = 0; row < n; row++)
        {
            newMatrix[row, colIndex] = column[row];
        }

        return newMatrix;
    }
}