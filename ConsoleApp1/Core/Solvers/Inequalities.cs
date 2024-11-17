using ConsoleApp1.Core.Booleans;
using ConsoleApp1.Core.Equations;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Limits;
using ConsoleApp1.Core.Sets;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Solvers;

public class Inequalities
{
    public static Set RelationnalsToSet(Boolean condition, Variable var)
    {
        if (condition is BooleanValue value)
        {
            return value.Value ? R : EmptySet;
        }
        
        if (condition is Or or)
            return Union(or.Values.Select(b => RelationnalsToSet(b, var)).ToArray());
        if (condition is And and)
            return Intersection(and.Values.Select(b => RelationnalsToSet(b, var)).ToArray());
        if (condition is Not not)
            return Complement(RelationnalsToSet(not.Value, var), R);

        if (condition is Relationnals relationnals)
        {
            if (relationnals.A != var)
                relationnals = relationnals.Swap();
            if (relationnals.A != var)
                throw new Exception("Variable not found in relationnals");

            var b = relationnals.B;
            return relationnals.Relation switch
            {
                Relations.Equal => ArraySet(b),
                Relations.NotEqual => Complement(ArraySet(b), R),
                Relations.Greater => Interval(b, Expr.Inf, false, false),
                Relations.GreaterOrEqual => Interval(b, Expr.Inf, true, false),
                Relations.Less => Interval(Expr.NegInf, b, false, false),
                Relations.LessOrEqual => Interval(Expr.NegInf, b, false, true),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        return LambdaSet(var, condition, [var]);
    }
    
    public static Set FindDomain(Expr f, Variable variable)
    {
        // Add, Mul, Pow, Ln, Exp
        Set dom = R;
        f.ForEach<Expr>(expr =>
        {
            if (!expr.DomainCondition.IsTrue)
            {
                dom = dom.Intersect(expr.DomainCondition.SolveFor(variable));
            }
        });

        return dom;
    }

    public static Set FindRange(Expr f, Variable variable, Set? domain = null)
    {
        
        Set dom = FindDomain(f, variable).Intersect(domain ?? R);

        Set RangeFromSet(Set set)
        {
            switch (set)
            {
                case UnionSet union:
                    return Union(union.Sets.Select(RangeFromSet).ToArray());
                case IntersectionSet intersection:
                    return Intersection(intersection.Sets.Select(RangeFromSet).ToArray());
                case FiniteSet fs:
                    return ArraySet(fs.Elements.Select(el => f.Substitue(variable, el)).ToArray());
                case Real:
                    return RangeFromSet(new IntervalSet(Expr.NegInf, Expr.Inf, false, false));
                case IntervalSet interval:
                    
                    List<(Expr Value, bool Include)> criticalPoints = [
                        (interval.StartInclusive ? f.Substitue(variable, interval.Start) : Limit.LimitOf(f, variable, interval.Start, Direction.Greater), interval.StartInclusive),
                        (interval.EndInclusive ? f.Substitue(variable, interval.End) : Limit.LimitOf(f, variable, interval.End, Direction.Smaller), interval.EndInclusive),
                    ];
        
                    var solutions = Solve.SolveFor(f.Derivee(variable), 0, variable);
                    if (solutions is null || (solutions is not FiniteSet && !solutions.IsEmpty))
                        throw new NotImplementedException();
        
                    if (!solutions.IsEmpty)
                        criticalPoints.InsertRange(1, 
                            ((FiniteSet) solutions).Elements.Select(x => (f.Substitue(variable, x), true))
                        );
                    
                    var max = criticalPoints.MaxBy(cp => cp.Value, new Expr.ExprComparer());
                    var min = criticalPoints.MinBy(cp => cp.Value, new Expr.ExprComparer());
                    
                    
                    // TODO : si il y a une singularité ou une discontinuité alors Range = U [criticalP_i-1, criticalP_i] ou criticalP est ordonné
                    // sinon continus et Range = [Min(criticalP), Max(criticalP)]

                    return Interval(min.Value, max.Value, min.Include, max.Include);
                case SetEmpty:
                    return EmptySet;
                default:
                    throw new NotImplementedException();
            }
        }

        return RangeFromSet(dom);
    }
    
    public static Set SolveFor(Expr lhs, Expr rhs, InequationType type, Variable variable)
    {
        var f = lhs - rhs;
        
        var domain = FindDomain(f, variable);
        
        var inf = domain.Infimum();
        var sup = domain.Supremum();

        if (inf is null || sup is null)
            throw new NotImplementedException();
        
        
        List<(Expr Value, bool Include)> criticalPoints = [
            (inf, true),
            (sup, true),
        ];
        
        var solutions = Solve.SolveFor(f, 0, variable);
        // var singularities = FindSingularities(f, variable);
        if (solutions is not FiniteSet)
            throw new NotImplementedException();

        var equal = type == InequationType.GreaterThanOrEqual || type == InequationType.LessThanOrEqual;
        var sols = ((FiniteSet)solutions).Elements.Select(x => (x, equal)).ToArray();
        Array.Sort(sols, (a, b) =>
        {
            var cmp = a.x > b.x;
            if (cmp.IsTrue)
                return 1;
            if (cmp.IsFalse)
                return -1;
            return 0;
        });
        
        criticalPoints.InsertRange(1, sols);
        
        var intervals = EmptySet;
        for (int i = 1; i < criticalPoints.Count; i++)
        {
            var x_j = criticalPoints[i - 1];
            var x_j2 = criticalPoints[i];

            var p = MidPoint(x_j.Value, x_j2.Value);
            var y = f.Substitue(variable, p);

            if (Valid(y, type))
            {
                intervals = intervals.UnionWith(Interval(x_j.Value, x_j2.Value, x_j.Include, x_j2.Include));
            }

        }

        return intervals.Intersect(domain);
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