using ConsoleApp1.Core.Expressions.Atoms;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Sets;

public class ConstructorSets
{
    public static Set EmptySet => new SetEmpty();
    
    public static UniversalSet U => new UniversalSet();
    
    public static Natural N => new Natural();
    public static Integer Z => new Integer();
    public static Rational Q => new Rational();
    public static Real R => new Real();

    public static Set Interval(Expr start, Expr end, bool startInclusive = true, bool endInclusive = true)
    {
        return IntervalSet.Construct(start, end, startInclusive, endInclusive);
    }
    
    public static Set ArraySet(HashSet<Expr> exprs) => FiniteSet.Construct(exprs);

    public static Set ArraySet(params Expr[] exprs)
    {
        return FiniteSet.Construct(exprs);
    }

    public static Set Union(params Set[] sets)
    {
        return UnionSet.Construct(sets);
    }
    
    public static Set Intersection(params Set[] sets)
    {
        return IntersectionSet.Construct(sets);
    }
    
    public static Set SetDifference(Set a, Set b)
    {
        return DifferenceSet.Construct(a, b);
    }
    
    public static Set Complement(Set set, Set? univers = null)
    {
        univers = univers ?? R;
        
        return DifferenceSet.Construct(univers, set);
    }

    public static Set LambdaSet(Variable x, Boolean condition, Set? domain = null)
    {
        domain = domain ?? R;

        return ConditionSet.Construct(x, condition, domain);
    }
    
}