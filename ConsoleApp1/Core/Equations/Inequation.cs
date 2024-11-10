using ConsoleApp1.Core.Expressions.Others;
using ConsoleApp1.Core.Sets;

namespace ConsoleApp1.Core.Equations;

public enum InequationType
{
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual
}

public class Inequation(Expr lhs, Expr rhs, InequationType type)
{
    public Expr Lhs = lhs;
    public Expr Rhs = rhs;
    public InequationType Type = type;

    public Set SolveFor(string variable)
    {
        throw new NotImplementedException();
    }
    
    public bool SolveNumericallyFor(string variable)
    {
        throw new NotImplementedException();
    }

    public Equation ToEquation()
    {
        var set = Type switch
        {
            InequationType.LessThan => Interval(double.NegativeInfinity, 0, false, false),
            InequationType.LessThanOrEqual => Interval(double.NegativeInfinity, 0, false, true),
            InequationType.GreaterThan => Interval(0, double.PositiveInfinity, false, false),
            InequationType.GreaterThanOrEqual => Interval(0, double.PositiveInfinity, true, false),
            _ => throw new ArgumentOutOfRangeException()
        };

        return new Equation(Lhs - Rhs, SetExpr.Construct(set));
    }

}