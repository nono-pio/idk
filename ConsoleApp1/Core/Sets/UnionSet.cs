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
                    switch (eval)
                    {
                        case UniversalSet:
                            return U;
                        case SetEmpty:
                            newSets.RemoveAt(i);
                            break;
                        case UnionSet union:
                            newSets[i] = union.Sets[0];
                            newSets.AddRange(union.Sets.Skip(0));
                            break;
                        default:
                            newSets[i] = eval;
                            break;
                    }

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

        switch (A, B)
        {
            case (IntersectionSet intersectionA, _):
                return Intersection(intersectionA.Sets.Select(set => Union(set, B)).ToArray());
            case (_, IntersectionSet intersectionB):
                return Intersection(intersectionB.Sets.Select(set => Union(set, A)).ToArray());
            
            case (SetEmpty, _):
                return B;
            case (_, SetEmpty):
                return A;
            
            case (UniversalSet, _):
                return U;
            case (_, UniversalSet):
                return U;
            
            case (NumberSet bA, NumberSet bB):
                return bA._Level > bB._Level ? bA : bB;

            case (IntervalSet intA, IntervalSet intB):
                return CombineIntervals(intA, intB);

            case (NumberSet bA2, IntervalSet):
                return bA2._Level < Real.Level ? null : bA2;
            case (IntervalSet, NumberSet bB2):
                return bB2._Level < Real.Level ? null : bB2;

            case (FiniteSet fA, FiniteSet fB):
                var newEls = new HashSet<Expr>(fA.Elements);
                newEls.UnionWith(fB.Elements);

                return ArraySet(newEls);
            
            case (FiniteSet f, IntervalSet i):
                var new_els = new HashSet<Expr>(f.Elements);
                var start_inc = i.StartInclusive;
                var end_inc = i.EndInclusive;
                
                foreach (var el in f.Elements)
                {
                    if (el == i.Start)
                        start_inc = true;
                    if (el == i.End)
                        end_inc = true;
                    
                    if (el > i.Start & el < i.End)
                        continue;

                    new_els.Add(el);
                }

                return new_els.Count switch
                {
                    0 => Interval(i.Start, i.End, start_inc, end_inc),
                    _ => new UnionSet(Interval(i.Start, i.End, start_inc, end_inc), ArraySet(new_els))
                };
            case (IntervalSet i, FiniteSet f):
                var new_els2 = new HashSet<Expr>(f.Elements);
                var start_inc2 = i.StartInclusive;
                var end_inc2 = i.EndInclusive;
                
                foreach (var el in f.Elements)
                {
                    if (el == i.Start)
                        start_inc2 = true;
                    if (el == i.End)
                        end_inc2 = true;
                    
                    if (el > i.Start & el < i.End)
                        continue;

                    new_els2.Add(el);
                }

                return new_els2.Count switch
                {
                    0 => Interval(i.Start, i.End, start_inc2, end_inc2),
                    _ => new UnionSet(Interval(i.Start, i.End, start_inc2, end_inc2), ArraySet(new_els2))
                };
            
            case (FiniteSet fA2, _):
                return CombineFiniteAndSet(fA2, B);
            case (_, FiniteSet fB2):
                return CombineFiniteAndSet(fB2, A);
            
        }
        
        return null;
    }
    
    public static Set? CombineIntervals(IntervalSet a, IntervalSet b)
    {
        if (a.Overlap(b))
        {
            Expr start;
            bool startInc;
            if (a.Start < b.Start)
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
            if (a.End > b.End)
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
        
        return null;
    }
    
    public static Set CombineFiniteAndSet(FiniteSet f, Set set)
    {
        // TODO
        var newElements = new HashSet<Expr>(f.Elements);
        newElements.RemoveWhere(x => set.Contains(x).IsTrue);

        if (newElements.Count == 0)
            return set;
        
        return new UnionSet(set, ArraySet(newElements));
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