using ConsoleApp1.Core.Booleans;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Sets;

public class Union(params Set[] sets) : Set
{
    public Set[] Sets = sets;
    
    public new static Set CreateUnion(params Set[] sets)
    {
        if (sets.Length == 0)
            return EmptySet;
        
        // TODO
        
        return new Union(sets);
    }

    public static Set? EvalUnion(Set A, Set B)
    {
        // Empty Set
        if (A is EmptySet)
            return B;
        if (B is EmptySet)
            return A;
        
        // Universal Set
        if (A is UniversalSet || B is UniversalSet)
            return U;
        
        // Basic Number Sets
        if (A is BasicNumberSet bA && B is BasicNumberSet bB)
            return BasicNumberSet.GetUnionOf(bA, bB);
        
        // Interval Sets
        if (A is Interval intA && B is Interval intB)
            return CombineIntervals(intA, intB);

        if (A is BasicNumberSet bA2 && B is Interval intB2)
            return CombineIntervalBasicNumberSet(intB2, bA2);
        if (A is Interval intA2 && B is BasicNumberSet bB2)
            return CombineIntervalBasicNumberSet(intA2, bB2);

        // Finite Sets
        if (A is FiniteSet fA && B is FiniteSet fB)
            return fA.UnionSelf(fB);
        
        if (A is FiniteSet fA2)
            return fA2.UnionSet(B);
        if (B is FiniteSet fB2)
            return fB2.UnionSet(A);
        
        return null;
    }
    
    public static Interval? CombineIntervals(Interval a, Interval b)
    {
        // TODO
        return null;
    }
    
    public static BasicNumberSet? CombineIntervalBasicNumberSet(Interval a, BasicNumberSet b)
    {
        return b._Level < Real.Level ? null : b;
    }

    public override Expr? Infimum()
    {
        var infSets = new Expr[Sets.Length];
        for (int i = 0; i < Sets.Length; i++)
        {
            var inf = Sets[i].Infimum();
            if (inf is null)
                return null;
            infSets[i] = inf;
        }

        return Min(infSets);
    }

    public override Expr? Supremum()
    {
        var supSets = new Expr[Sets.Length];
        for (int i = 0; i < Sets.Length; i++)
        {
            var sup = Sets[i].Supremum();
            if (sup is null)
                return null;
            supSets[i] = sup;
        }

        return Max(supSets);
    }

    public override Set Complement(Set universe)
    {
        return CreateIntersection(Sets.Select(set => set.Complement(universe)).ToArray());
    }

    public override Set Boundary()
    {
        throw new NotImplementedException();
    }

    public override Boolean Contains(Expr x) => AsCondition(x);
    
    public Boolean AsCondition(Expr x)
    {
        return Or.Eval(Sets.Select(s => s.Contains(x)));
    }
}