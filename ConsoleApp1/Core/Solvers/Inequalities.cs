using ConsoleApp1.Core.Booleans;
using ConsoleApp1.Core.Equations;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Base;
using ConsoleApp1.Core.Sets;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Solvers;

public class Inequalities
{
    public static Set RelationnalsToSet(Boolean condition, Variable var)
    {
        if (condition is BooleanValue value)
        {
            return value.Value ? Set.R : Set.EmptySet;
        }
        
        if (condition is Or or)
            return Set.CreateUnion(or.Values.Select(b => RelationnalsToSet(b, var)).ToArray());
        if (condition is And and)
            return Set.CreateIntersection(and.Values.Select(b => RelationnalsToSet(b, var)).ToArray());
        if (condition is Not not)
            return Set.CreateComplement(RelationnalsToSet(not.Value, var), Set.R);

        if (condition is Relationnals relationnals)
        {
            if (relationnals.A != var)
                relationnals = relationnals.Swap();
            if (relationnals.A != var)
                throw new Exception("Variable not found in relationnals");

            var b = relationnals.B;
            return relationnals.Relation switch
            {
                Relations.Equal => Set.CreateFiniteSet(b),
                Relations.NotEqual => Set.CreateComplement(Set.CreateFiniteSet(b), Set.R),
                Relations.Greater => Set.CreateInterval(b, Expr.Inf, false, false),
                Relations.GreaterOrEqual => Set.CreateInterval(b, Expr.Inf, true, false),
                Relations.Less => Set.CreateInterval(Expr.NegInf, b, false, false),
                Relations.LessOrEqual => Set.CreateInterval(Expr.NegInf, b, false, true),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        return Set.CreateConditionSet(condition, var, Set.R);
    }
    
    public static Set FindDomain(Expr f, Variable variable)
    {
        // Add, Mul, Pow, Ln, Exp
        Set dom = Set.R;
        f.ForEach<Logarithm>(log =>
        {
            dom = dom.Intersection(SolveFor(log.Value, 0, InequationType.GreaterThan, variable));
        });
        
        f.ForEach<Power>(pow =>
        {
            if (pow.Exp.IsNegative)
            {
                dom = dom.Intersection(Solve.SolveFor(pow.Base, 0, variable)!.Complement(Set.R));
            }
        });

        return dom;
    }
    
    public static Set SolveFor(Expr lhs, Expr rhs, InequationType type, Variable variable)
    {
        var f = lhs - rhs;
        Set domain = FindDomain(f, variable);

        var solutions = Solve.SolveFor(f, 0, variable);
        // var singularities = FindSingularities(f, variable);

        var criticalPoints = solutions.Union(Set.CreateFiniteSet(domain.Infimum(), domain.Supremum())); // U singularities
        if (criticalPoints is not FiniteSet criticalSet)
            throw new NotImplementedException();

        var xs = criticalSet.Elements.ToArray();
        Array.Sort(xs, (a, b) => a < b ? -1 : 1);
        
        var intervals = Set.EmptySet;
        for (int i = 1; i < xs.Length; i++)
        {
            var x_j = xs[i - 1];
            var x_j2 = xs[i];

            var p = MidPoint(x_j, x_j2);
            var y = f.Substitue(variable, p);

            if (Valid(y, type))
            {
                intervals = intervals.Union(Set.CreateInterval(x_j, x_j2));
            }

        }

        return intervals;
    }
    
    // x < y
    private static Expr MidPoint(Expr x, Expr y)
    {
        if (x.IsInfinite && y.IsInfinite)
            return 0;
        if (x.IsInfinite)
            return y - 1;
        if (y.IsInfinite)
            return x + 1;
        
        return (x + y) / 2;
    }
    
    private static bool Valid(Expr y, InequationType type)
    {
        return type switch
        {
            InequationType.LessThan => y.IsNegative,
            InequationType.LessThanOrEqual => y.IsNegative || y.IsZero,
            InequationType.GreaterThan => y.IsPositive,
            InequationType.GreaterThanOrEqual => y.IsPositive || y.IsZero,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
}