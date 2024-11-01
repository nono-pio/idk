using ConsoleApp1.Core.Booleans;
using ConsoleApp1.Latex;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Sets;

public class UnionSet(params Set[] sets) : Set
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
        
        var newSets = new List<Set>();
        foreach (var set in sets.SelectMany(set => set is UnionSet union ? union.Sets : new Set[] {set}))
        {
            for (int i = 0; i < newSets.Count; i++)
            {
                var eval = EvalUnion(set, newSets[i]);
                if (eval is not null)
                {
                    newSets[i] = eval;
                    goto next;
                }
            }
            
            newSets.Add(set);
            
            next: ;
        }

        if (newSets.Count == 0)
            return EmptySet;
        if (newSets.Count == 1)
            return newSets[0];
        
        return new UnionSet(newSets.ToArray());
    }

    public static Set? EvalUnion(Set A, Set B)
    {
        // Empty Set
        if (A is SetEmpty)
            return B;
        if (B is SetEmpty)
            return A;
        
        // Universal Set
        if (A is UniversalSet || B is UniversalSet)
            return U;
        
        // Basic Number Sets
        if (A is NumberSet bA && B is NumberSet bB)
            return NumberSet.GetUnionOf(bA, bB);
        
        // Interval Sets
        if (A is IntervalSet intA && B is IntervalSet intB)
            return CombineIntervals(intA, intB);

        if (A is NumberSet bA2 && B is IntervalSet intB2)
            return CombineIntervalNumberSet(intB2, bA2);
        if (A is IntervalSet intA2 && B is NumberSet bB2)
            return CombineIntervalNumberSet(intA2, bB2);

        // Finite Sets
        // if (A is FiniteSet fA && B is FiniteSet fB)
        //     return fA.UnionSelf(fB);
        
        if (A is FiniteSet fA2)
            return fA2.UnionSet(B);
        if (B is FiniteSet fB2)
            return fB2.UnionSet(A);
        
        return null;
    }
    
    public static IntervalSet? CombineIntervals(IntervalSet a, IntervalSet b)
    {
        // TODO
        return null;
    }
    
    public static NumberSet? CombineIntervalNumberSet(IntervalSet a, NumberSet b)
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

    // public override Set Complement(Set universe)
    // {
    //     return Intersection(Sets.Select(set => set.Complement(universe)).ToArray());
    // }

    public override Set Boundary()
    {
        throw new NotImplementedException();
    }

    public override Boolean Contains(Expr x) => AsCondition(x);
    
    public Boolean AsCondition(Expr x)
    {
        return Or.Eval(Sets.Select(s => s.Contains(x)));
    }

    public override string ToLatex()
    {
        return string.Join(Symbols.Union, Sets.Select(s => s.ToLatex()));
    }

    public override string ToString()
    {
        return string.Join(" U ", Sets.Select(s => s.ToString()));
    }
}