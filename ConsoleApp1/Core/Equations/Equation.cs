using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.NumericalAnalysis;
using ConsoleApp1.Core.Sets;
using ConsoleApp1.Core.Solvers;

namespace ConsoleApp1.Core.Equations;

public class Equation(Expr lhs, Expr rhs)
{
    public Expr Lhs = lhs;
    public Expr Rhs = rhs;

    public Set SolveFor(Variable variable)
    {
        var solution = Solve.SolveFor(Lhs, Rhs, variable);
        if (solution is null)
            throw new Exception("Equation is not solvable at this moment.");
        return solution;
    }

    public double SolveNumericallyFor(string variable)
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return $"{Lhs} = {Rhs}";
    }
    
    public string ToLatex()
    {
        return $"{Lhs.ToLatex()} = {Rhs.ToLatex()}";
    }
}