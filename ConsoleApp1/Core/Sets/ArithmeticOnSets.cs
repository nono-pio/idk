
namespace ConsoleApp1.Core.Sets;

public class ArithmeticOnSets
{
    // univariate function
    public static Set FunctionOnSet(Func<Expr, Expr> func, Set set, Func<NumberSet, Set>? basic = null,  Func<IntervalSet, Set>? interval = null )
    {

        switch (set)
        {
            case FiniteSet finite:
                return ArraySet(finite.Elements.Select(func).ToArray());
            
            case UnionSet union:
                return Union(union.Sets.Select(union_set => FunctionOnSet(func, union_set, basic, interval)).ToArray());
            
            case SetEmpty:
                return EmptySet;
            
            case NumberSet bns:
                if (basic is null)
                    break;
                return basic(bns);
            
            case IntervalSet interval_set:
                if (interval is null)
                    break;
                return interval(interval_set);
            
            case IntersectionSet:
                break;
        }

        throw new NotImplementedException();
    }

    public static Set FunctionIncreasingOnSet(Func<Expr, Expr> func, Set set, Func<NumberSet, Set>? basic = null) => FunctionOnSet(func, set, basic, interval => FunctionIncreasing(func, interval));
    public static Set FunctionDecreasingOnSet(Func<Expr, Expr> func, Set set, Func<NumberSet, Set>? basic = null) => FunctionOnSet(func, set, basic, interval => FunctionDecreasing(func, interval));
    public static Set FunctionOnSet(Func<Expr, Expr> func, Set set, Func<Expr, Expr, Set> interval_from_func, Func<NumberSet, Set>? basic = null) => FunctionOnSet(func, set, basic, interval => FunctionIntervalFromFunc(func, interval, interval_from_func));
    
    public static Set FunctionIncreasing(Func<Expr, Expr> func, IntervalSet intervalSet)
    {
        return Interval(func(intervalSet.Start), func(intervalSet.End), intervalSet.StartInclusive, intervalSet.EndInclusive);
    }
    public static Set FunctionDecreasing(Func<Expr, Expr> func, IntervalSet intervalSet)
    {
        return Interval(func(intervalSet.End), func(intervalSet.Start), intervalSet.EndInclusive, intervalSet.StartInclusive);
    }

    public static Set FunctionIntervalFromFunc(Func<Expr, Expr> func, IntervalSet intervalSet,
        Func<Expr, Expr, Set> interval_from_func)
    {
        return interval_from_func(func(intervalSet.Start), func(intervalSet.End)); // TODO: bornes
    }

    public static Func<NumberSet, Set> FunctionBasicNumber(Set? natural = null, Set? integer = null, Set? rational = null, Set? real = null)
    {
        return bns =>
        {
            switch (bns)
            {
                case Natural:
                    if (natural is null)
                        break;
                    return natural;
                
                case Integer:
                    if (integer is null)
                        break;
                    return integer;
                
                case Rational:
                    if (rational is null)
                        break;
                    return rational;
                
                case Real:
                    if (real is null)
                        break;
                    return real;
            }

            throw new NotImplementedException();
        };
    }

    public static Set BiCommutativeFunctionOnSet(Func<Expr, Expr, Expr> func, Set setA, Set setB, Func<NumberSet, NumberSet, Set>? bns = null, Func<IntervalSet, IntervalSet, Set>? interval = null, Func<Expr, NumberSet, Set>? expr_bns = null, Func<Expr, IntervalSet, Set>? expr_interval = null, Func<NumberSet, IntervalSet, Set>? bns_integral = null)
    {
        Set UnionCase(UnionSet union, Set set) =>
            Union(union.Sets.Select(union_set => BiCommutativeFunctionOnSet(func, union_set, set)).ToArray());

        Set SetASetBCase<TA, TB>(Func<TA, TB, Set>? f, TA set1, TB set2)
        {
            if (f is null)
                throw new NotImplementedException();
            
            return f(set1, set2);
        }

        Set FiniteSetCase<T>(Func<Expr, T, Set>? f, FiniteSet finiteSet, T set)
        {
            return Union(finiteSet.Elements.Select(el => SetASetBCase(f, el, set)).ToArray());
        }

        if (setA is UnionSet unionA)
            return UnionCase(unionA, setB);
        
        if (setB is UnionSet unionB)
            return UnionCase(unionB, setA);

        if (setA.IsEmpty || setB.IsEmpty)
            return EmptySet;

        if (setA is FiniteSet finiteA && setB is FiniteSet finiteB)
        {
            return ArraySet(finiteA.Elements.SelectMany(elA => finiteB.Elements.Select(elB => func(elA, elB))).ToArray());
        }

        if (setA is FiniteSet fA)
        {
            if (setB is NumberSet bnsB)
                return FiniteSetCase(expr_bns, fA, bnsB);
            if (setB is IntervalSet intB)
                return FiniteSetCase(expr_interval, fA, intB);
        }

        if (setB is FiniteSet fB)
        {
            if (setA is NumberSet bnsA)
                return FiniteSetCase(expr_bns, fB, bnsA);
            if (setA is IntervalSet intA)
                return FiniteSetCase(expr_interval, fB, intA);
        }

        if (setA is NumberSet && setB is NumberSet)
            return SetASetBCase(bns, (NumberSet)setA, (NumberSet)setB);
        
        if (setA is IntervalSet && setB is IntervalSet)
            return SetASetBCase(interval, (IntervalSet)setA, (IntervalSet)setB);
        
        if (setA is NumberSet && setB is IntervalSet)
            return SetASetBCase(bns_integral, (NumberSet)setA, (IntervalSet)setB);
        if (setA is IntervalSet && setB is NumberSet)
            return SetASetBCase(bns_integral, (NumberSet)setB, (IntervalSet)setA);
        
        throw new NotImplementedException();
    }
    
}