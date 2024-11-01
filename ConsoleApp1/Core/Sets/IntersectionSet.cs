using ConsoleApp1.Core.Booleans;
using ConsoleApp1.Latex;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Sets;

public class IntersectionSet(params Set[] sets) : Set
{
    public Set[] Sets = sets;
    
    public override bool IsElementsNatural => Sets.All(set => set.IsElementsNatural);
    public override bool IsElementsInteger => Sets.All(set => set.IsElementsInteger);
    public override bool IsElementsRational => Sets.All(set => set.IsElementsRational);
    public override bool IsElementsReal => Sets.All(set => set.IsElementsReal);
    
    public override bool IsElementsPositive => Sets.All(set => set.IsElementsPositive);
    public override bool IsElementsNegative => Sets.All(set => set.IsElementsNegative);
    
    public static Set Construct(params Set[] sets)
    {
        if (sets.Length == 0)
            return EmptySet;
        
        // TODO
        
        return new IntersectionSet(sets);
    }

    public static Set? EvalIntersection(Set A, Set B)
    {
        // Empty Set
        if (A is SetEmpty || B is SetEmpty)
            return EmptySet;
        
        // Universal Set
        if (A is UniversalSet)
            return B;
        if (B is UniversalSet)
            return A;
        
        // Basic Number Sets
        if (A is NumberSet bA && B is NumberSet bB)
            return NumberSet.GetIntersectionOf(bA, bB);
        
        // Interval Sets
        if (A is IntervalSet intA && B is IntervalSet intB)
            return OverlapIntervals(intA, intB);

        if (A is NumberSet bA2 && B is IntervalSet intB2)
            return IntersectionIntervalNumberSet(intB2, bA2);
        if (A is IntervalSet intA2 && B is NumberSet bB2)
            return IntersectionIntervalNumberSet(intA2, bB2);

        // Finite Sets
        if (A is FiniteSet fA && B is FiniteSet fB)
            return fA.IntersectionSelf(fB);
        
        if (A is FiniteSet fA2)
            return fA2.IntersectionSet(B);
        if (B is FiniteSet fB2)
            return fB2.IntersectionSet(A);
        
        return null;
    }
    
    public static IntervalSet? OverlapIntervals(IntervalSet a, IntervalSet b)
    {
        // TODO
        return null;
    }
    
    public static Set? IntersectionIntervalNumberSet(IntervalSet a, NumberSet b)
    {
        return b._Level < Real.Level ? null/*TODO:Range*/ : a;
    }

    public override Expr? Infimum()
    {
        var infTest = Sets[0].Infimum();
        if (Sets.Skip(0).All(s => s.Infimum() == infTest))
            return infTest;
        
        throw new NotImplementedException();
    }

    public override Expr? Supremum()
    {
        var supTest = Sets[0].Supremum();
        if (Sets.Skip(0).All(s => s.Supremum() == supTest))
            return supTest;
        
        throw new NotImplementedException();
    }

    // public Set Complement(Set universe)
    // {
    //     return Union(Sets.Select(set => set.Complement(universe)).ToArray());
    // }

    public override Set Boundary()
    {
        throw new NotImplementedException();
    }

    public override Boolean Contains(Expr x) => AsCondition(x);
    
    public Boolean AsCondition(Expr x)
    {
        return And.Eval(Sets.Select(s => s.Contains(x)));
    }
    
    public override string ToString()
    {
        return string.Join('\u2229', Sets.Select(s => s.ToString()));
    }

    public override string ToLatex()
    {
        return string.Join(Symbols.Intersection, Sets.Select(s => s.ToLatex()));
    }
}