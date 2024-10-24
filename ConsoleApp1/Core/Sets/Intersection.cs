using ConsoleApp1.Core.Booleans;
using ConsoleApp1.Latex;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Sets;

public class Intersection(params Set[] sets) : Set
{
    public Set[] Sets = sets;
    
    public override bool IsElementsNatural => Sets.All(set => set.IsElementsNatural);
    public override bool IsElementsInteger => Sets.All(set => set.IsElementsInteger);
    public override bool IsElementsRational => Sets.All(set => set.IsElementsRational);
    public override bool IsElementsReal => Sets.All(set => set.IsElementsReal);
    
    public override bool IsElementsPositive => Sets.All(set => set.IsElementsPositive);
    public override bool IsElementsNegative => Sets.All(set => set.IsElementsNegative);
    
    public new static Set CreateIntersection(params Set[] sets)
    {
        if (sets.Length == 0)
            return EmptySet;
        
        // TODO
        
        return new Intersection(sets);
    }

    public static Set? EvalIntersection(Set A, Set B)
    {
        // Empty Set
        if (A is EmptySet || B is EmptySet)
            return EmptySet;
        
        // Universal Set
        if (A is UniversalSet)
            return B;
        if (B is UniversalSet)
            return A;
        
        // Basic Number Sets
        if (A is BasicNumberSet bA && B is BasicNumberSet bB)
            return BasicNumberSet.GetIntersectionOf(bA, bB);
        
        // Interval Sets
        if (A is Interval intA && B is Interval intB)
            return OverlapIntervals(intA, intB);

        if (A is BasicNumberSet bA2 && B is Interval intB2)
            return IntersectionIntervalBasicNumberSet(intB2, bA2);
        if (A is Interval intA2 && B is BasicNumberSet bB2)
            return IntersectionIntervalBasicNumberSet(intA2, bB2);

        // Finite Sets
        if (A is FiniteSet fA && B is FiniteSet fB)
            return fA.IntersectionSelf(fB);
        
        if (A is FiniteSet fA2)
            return fA2.IntersectionSet(B);
        if (B is FiniteSet fB2)
            return fB2.IntersectionSet(A);
        
        return null;
    }
    
    public static Interval? OverlapIntervals(Interval a, Interval b)
    {
        // TODO
        return null;
    }
    
    public static Set? IntersectionIntervalBasicNumberSet(Interval a, BasicNumberSet b)
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

    public override Set Complement(Set universe)
    {
        return CreateUnion(Sets.Select(set => set.Complement(universe)).ToArray());
    }

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