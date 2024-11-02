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
        
        var newSets = new List<Set>();
        foreach (var set in sets.SelectMany(set => set is IntersectionSet intersection ? intersection.Sets : new Set[] {set}))
        {
            for (int i = 0; i < newSets.Count; i++)
            {
                var eval = EvalIntersection(set, newSets[i]);
                if (eval is not null)
                {
                    if (eval.IsEmpty)
                        return EmptySet;
                    
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
        
        return new IntersectionSet(newSets.ToArray());
    }

    public static Set? EvalIntersection(Set A, Set B)
    {
        if (A is UnionSet unionA)
        {
            return Union(unionA.Sets.Select(set => Intersection(set, B)).ToArray());
        }
        if (B is UnionSet unionB)
        {
            return Union(unionB.Sets.Select(set => Intersection(set, A)).ToArray());
        }
        
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
            return bA._Level < bB._Level ? bA : bB;
        
        if ((A is Natural && B.IsElementsNatural) || 
            (A is Integer && B.IsElementsInteger) ||
            (A is Rational && B.IsElementsRational) ||
            (A is Real && B.IsElementsReal))
            return B;
        
        if ((B is Natural && A.IsElementsNatural) || 
            (B is Integer && A.IsElementsInteger) ||
            (B is Rational && A.IsElementsRational) ||
            (B is Real && A.IsElementsReal))
            return A;

        
        // Interval Sets
        if (A is IntervalSet intA && B is IntervalSet intB)
            return CombineIntervals(intA, intB);

        if (A is Real && B is IntervalSet)
            return B;
        if (B is Real && A is IntervalSet)
            return A;
        
        if (A is NumberSet bA2 && B is IntervalSet intB2)
            return CombineIntervalNumberSet(intB2, bA2);
        if (A is IntervalSet intA2 && B is NumberSet bB2)
            return CombineIntervalNumberSet(intA2, bB2);

        // Finite Sets
        if (A is FiniteSet fA && B is FiniteSet fB)
            return CombineFinite(fA, fB);
        
        if (A is FiniteSet fA2)
            return CombineFiniteAndSet(fA2, B);
        if (B is FiniteSet fB2)
            return CombineFiniteAndSet(fB2, A);
        
        return null;
    }
    
    public static Set? CombineIntervals(IntervalSet a, IntervalSet b)
    {
        if (a.Overlap(b))
        {
            Expr start;
            bool startInc;
            if (a.Start > b.Start)
            {
                start = a.Start;
                startInc = a.StartInclusive;
            }
            else
            {
                start = b.Start;
                startInc = b.StartInclusive;
            }
            
            Expr end;
            bool endInc;
            if (a.End < b.End)
            {
                end = a.End;
                endInc = a.EndInclusive;
            }
            else
            {
                end = b.End;
                endInc = b.EndInclusive;
            }
            
            return Interval(start, end, startInc, endInc);
        }
        
        return EmptySet;
    }
    
    public static Set? CombineIntervalNumberSet(IntervalSet a, NumberSet b)
    {
        return b._Level < Real.Level ? null : a;
    }
    
    public static Set CombineFinite(FiniteSet a, FiniteSet b)
    {
        var newElements = new HashSet<Expr>(a.Elements);
        newElements.IntersectWith(b.Elements);

        if (newElements.Count == 0)
            return EmptySet;
        
        return ArraySet(newElements);
    }
    
    public static Set CombineFiniteAndSet(FiniteSet f, Set set)
    {
        var newElements = new HashSet<Expr>(f.Elements);
        var newElementsIndeterminate = new HashSet<Expr>();
        foreach (var elm in f.Elements)
        {
            var contains = set.Contains(elm);
            if (contains.IsTrue)
                newElements.Add(elm);
            else if (contains.IsIndeterminate)
                newElementsIndeterminate.Add(elm);
        }

        return (newElements.Count == 0, newElementsIndeterminate.Count == 0) switch
        {
            (true, true) => EmptySet,
            (true, false) => Intersection(ArraySet(newElementsIndeterminate), set),
            (false, true) => ArraySet(newElements),
            (false, false) => Union(ArraySet(newElements), Intersection(ArraySet(newElementsIndeterminate), set))
        };
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