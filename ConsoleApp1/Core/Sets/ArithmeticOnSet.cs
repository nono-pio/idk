
namespace ConsoleApp1.Core.Sets;

public class ArithmeticOnSet
{
    // univariate function
    public static Set FunctionOnSet(Func<Expr, Expr> func, Set set, Func<BasicNumberSet, Set>? basic = null,  Func<Interval, Set>? interval = null )
    {

        switch (set)
        {
            case FiniteSet finite:
                return Set.CreateFiniteSet(finite.Elements.Select(func).ToArray());
            
            case Union union:
                return Set.CreateUnion(union.Sets.Select(union_set => FunctionOnSet(func, union_set, basic, interval)).ToArray());
            
            case Complement:
                break;
            
            case EmptySet:
                return Set.EmptySet;
            
            case BasicNumberSet bns:
                if (basic is null)
                    break;
                return basic(bns);
            
            case Interval interval_set:
                if (interval is null)
                    break;
                return interval(interval_set);
            
            case Intersection:
                break;
        }

        throw new NotImplementedException();
    }

    public static Set FunctionIncreasingOnSet(Func<Expr, Expr> func, Set set, Func<BasicNumberSet, Set>? basic = null) => FunctionOnSet(func, set, basic, interval => FunctionIncreasing(func, interval));
    public static Set FunctionDecreasingOnSet(Func<Expr, Expr> func, Set set, Func<BasicNumberSet, Set>? basic = null) => FunctionOnSet(func, set, basic, interval => FunctionDecreasing(func, interval));
    public static Set FunctionOnSet(Func<Expr, Expr> func, Set set, Func<Expr, Expr, Set> interval_from_func, Func<BasicNumberSet, Set>? basic = null) => FunctionOnSet(func, set, basic, interval => FunctionIntervalFromFunc(func, interval, interval_from_func));
    
    public static Set FunctionIncreasing(Func<Expr, Expr> func, Interval interval)
    {
        return Interval.CreateInterval(func(interval.Start), func(interval.End), interval.StartInclusive, interval.EndInclusive);
    }
    public static Set FunctionDecreasing(Func<Expr, Expr> func, Interval interval)
    {
        return Interval.CreateInterval(func(interval.End), func(interval.Start), interval.EndInclusive, interval.StartInclusive);
    }

    public static Set FunctionIntervalFromFunc(Func<Expr, Expr> func, Interval interval,
        Func<Expr, Expr, Set> interval_from_func)
    {
        return interval_from_func(func(interval.Start), func(interval.End)); // TODO: bornes
    }

    public static Func<BasicNumberSet, Set> FunctionBasicNumber(Set? natural = null, Set? integer = null, Set? rational = null, Set? real = null)
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

    public static Set BiCommutativeFunctionOnSet(Func<Expr, Expr, Expr> func, Set setA, Set setB, Func<BasicNumberSet, BasicNumberSet, Set>? bns = null, Func<Interval, Interval, Set>? interval = null, Func<Expr, BasicNumberSet, Set>? expr_bns = null, Func<Expr, Interval, Set>? expr_interval = null, Func<BasicNumberSet, Interval, Set>? bns_integral = null)
    {
        Set UnionCase(Union union, Set set) =>
            Set.CreateUnion(union.Sets.Select(union_set => BiCommutativeFunctionOnSet(func, union_set, set)).ToArray());

        Set SetASetBCase<TA, TB>(Func<TA, TB, Set>? f, TA set1, TB set2)
        {
            if (f is null)
                throw new NotImplementedException();
            
            return f(set1, set2);
        }

        Set FiniteSetCase<T>(Func<Expr, T, Set>? f, FiniteSet finiteSet, T set)
        {
            return Set.CreateUnion(finiteSet.Elements.Select(el => SetASetBCase(f, el, set)).ToArray());
        }

        if (setA is Union unionA)
            return UnionCase(unionA, setB);
        
        if (setB is Union unionB)
            return UnionCase(unionB, setA);

        if (setA.IsEmpty || setB.IsEmpty)
            return Set.EmptySet;

        if (setA is FiniteSet finiteA && setB is FiniteSet finiteB)
        {
            return Set.CreateFiniteSet(finiteA.Elements.SelectMany(elA => finiteB.Elements.Select(elB => func(elA, elB))).ToArray());
        }

        if (setA is FiniteSet fA)
        {
            if (setB is BasicNumberSet bnsB)
                return FiniteSetCase(expr_bns, fA, bnsB);
            if (setB is Interval intB)
                return FiniteSetCase(expr_interval, fA, intB);
        }

        if (setB is FiniteSet fB)
        {
            if (setA is BasicNumberSet bnsA)
                return FiniteSetCase(expr_bns, fB, bnsA);
            if (setA is Interval intA)
                return FiniteSetCase(expr_interval, fB, intA);
        }

        if (setA is BasicNumberSet && setB is BasicNumberSet)
            return SetASetBCase(bns, (BasicNumberSet)setA, (BasicNumberSet)setB);
        
        if (setA is Interval && setB is Interval)
            return SetASetBCase(interval, (Interval)setA, (Interval)setB);
        
        if (setA is BasicNumberSet && setB is Interval)
            return SetASetBCase(bns_integral, (BasicNumberSet)setA, (Interval)setB);
        if (setA is Interval && setB is BasicNumberSet)
            return SetASetBCase(bns_integral, (BasicNumberSet)setB, (Interval)setA);
        
        throw new NotImplementedException();
    }
    
}