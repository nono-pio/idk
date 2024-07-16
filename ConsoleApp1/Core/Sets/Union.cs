using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Sets;

public class Union(params Set[] sets) : Set
{
    public Set[] Sets = sets;
    
    public new static Set CreateUnion(params Set[] sets)
    {
        // TODO
        if (sets.Length == 0)
            return EmptySet;
        
        return new Union(sets);
    }

    public Set UnionOf(Set a, Set b)
    {
        switch (a, b)
        {
            
            // case (Naturals0, Naturals):
            //     return a;

            case (Rational, Natural):
                return a;

            // case (Rationals, Naturals0):
            //     return a;

            case (Real, Natural):
                return a;
            
            // case (Reals, Naturals0):
            //     return a;

            case (Real, Rational):
                return a;

            case (Integer, Set):
                var intersect = CreateIntersection(a, b);
                if (intersect == a)
                    return b;
                else if(intersect == b)
                    return a;
                return null;

            case (EmptySet, Set):
                return b;

            case (UniversalSet, Set):
                return a;

            case (ProductSet, ProductSet):
                if (b.isSubset(a))
                    return a;
                if (b.Sets.Lenght != a.Sets.Lenght)
                    return null;
                if (a.Sets.Lenght == 2)
                {
                    var (a1, a2) = (a.Sets[0], a.Sets[1]);
                    var (b1, b2) = (b.Sets[0], b.Sets[1]);
                    if (a1 == b1)
                        return a1 * CreateUnion(a2, b2);
                    if (a2 == b2)
                        return CreateUnion(a1, b1) * a2;
                }

                return null;

            case (ProductSet, Set):
                if (b.isSubset(a))
                    return a;
                return null;

            case (Interval intA, Interval intB):
                // TODO
                
                // if (a._is_comparable(b))
                // {
                //     // Non-overlapping intervals
                //     var end = Min(intA.End, intB.End);
                //     var start = Max(intA.Start, intB.Start);
                //     if (end < start || (end == start && (!intA.Contains(end) && intB.Contains(end))))
                //         return null;
                //     else
                //     {
                //         start = Min(intA.Start, intB.Start);
                //         end = Max(intA.End, intB.End);
                //
                //         left_open = ((intA.start != start or intA.left_open) and
                //             (b.start != start or b.left_open))
                //         right_open = ((intA.end != end or intA.right_open) and
                //             (b.end != end or b.right_open))
                //         return Interval(start, end, left_open, right_open)
                //     }
                // }

                return null;
                    

            case (Interval, UniversalSet):
                return UniversalSet;

            case (Interval intA, Set):
                // # If I have open end points and these endpoints are contained in b
                // # But only in case, when endpoints are finite. Because
                // # interval does not contain oo or -oo.
                var open_left_in_b_and_finite = intA.left_open && b.Contains(intA.Start) == true && intA.Start.IsFinite;
                var open_right_in_b_and_finite = intA.right_open && b.Contains(intA.End)) == true && intA.End.IsFinite;
                if (open_left_in_b_and_finite || open_right_in_b_and_finite)
                {
                    // Fill in my end points and return
                    var open_left = a.left_open and a.start not in b; 
                    var open_right = a.right_open and a.end not in b;
                    var new_a = Interval(a.start, a.end, open_left, open_right);
                    return {new_a, b}
                }
                    
                return null;

@union_sets.register(FiniteSet, FiniteSet)
def _(a, b):
    return FiniteSet(*(a._elements | b._elements))

@union_sets.register(FiniteSet, Set)
def _(a, b):
    # If `b` set contains one of my elements, remove it from `a`
    if any(b.contains(x) == True for x in a):
        return {
            FiniteSet(*[x for x in a if b.contains(x) != True]), b}
    return None

@union_sets.register(Set, Set)
def _(a, b):
    return None

            
        }
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

    public override Boolean? Contains(Expr x)
    {
        var contain = new Boolean[Sets.Length];
        for (int i = 0; i < Sets.Length; i++)
        {
            var c = Sets[i].Contains(x);
            if (c is null)
                return null;
            contain[i] = c;
        }
        return Boolean.Or(contain);
    }
    
    public Boolean AsCondition(Expr x)
    {
        var contain = new Boolean[Sets.Length];
        for (int i = 0; i < Sets.Length; i++)
        {
            contain[i] = Sets[i].Contains(x) ?? throw new Exception("Invalid condition.");
        }
        return Boolean.Or(contain);
    }
}