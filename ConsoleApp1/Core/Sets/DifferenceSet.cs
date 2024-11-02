using ConsoleApp1.Core.Expressions.Others;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Sets;

public class DifferenceSet : Set
{

    public Set A;
    public Set B;
    public DifferenceSet(Set a, Set b)
    {
        A = a;
        B = b;
    }
    
    public static Set Construct(Set a, Set b)
    {
        switch (a, b)
        {
            case (SetEmpty, SetEmpty):
                return a;
            
            case (_, UniversalSet):
                return EmptySet;
            
            case (NumberSet bA, NumberSet bB):
                if (bA._Level <= bB._Level)
                    return EmptySet;
                
                break;
            
            case (Real, FiniteSet):
                return Construct(Real.AsInterval, b);
            
            case (Natural, IntervalSet intB2):
                return Interval(Max(Floor(intB2.Start), 0), Floor(intB2.End)).Intersect(N);

            case (Real, IntervalSet intB2) :
                if (intB2.Start.IsNegativeInfinity && intB2.End.IsInfinity)
                    return EmptySet;
                if (intB2.Start.IsNegativeInfinity)
                    return Interval(intB2.End, Expr.Inf, !intB2.EndInclusive, false);
                if (intB2.End.IsInfinity)
                    return Interval(Expr.NegInf, intB2.Start, false, !intB2.StartInclusive);
                
                break;
                
            case (IntervalSet intA, IntervalSet intB):
                return Union(
                    Interval(intA.Start, intB.Start, intA.StartInclusive, !intB.StartInclusive), 
                    Interval(intB.End, intA.End, !intB.EndInclusive, intA.EndInclusive)
                    );
            
            case (IntervalSet, Real):
                return EmptySet;

            case (IntervalSet intA3, FiniteSet fb):
                var elements = fb.Elements.Where(el => intA3.Contains(el).IsTrue).ToArray();

                if (elements.Length == 0)
                    return intA3;
                
                var sets = new Set[elements.Length + 1];
                sets[0] = Interval(intA3.Start, elements.ElementAt(0), intA3.StartInclusive, false);
                sets[^1] = Interval(elements.ElementAt(^1), intA3.End, false, intA3.EndInclusive);
                
                for (int i = 1; i < elements.Length; i++)
                {
                    sets[i] = Interval(elements.ElementAt(i - 1), elements.ElementAt(i), false, false);
                }

                return Union(sets);
                
            case (FiniteSet fA, FiniteSet fB):
                var newElements = new HashSet<Expr>(fA.Elements);
                newElements.RemoveWhere(el => fB.Contains(el).IsFalse);
                
                return ArraySet(newElements);
                
            case (FiniteSet fA2, _):
                return ArraySet(fA2.Elements.Where(el => b.Contains(el).IsFalse).ToArray());
                
        }
        
        return new DifferenceSet(a, b);
    }
    
    public override Boolean Contains(Expr x)
    {
        return A.Contains(x) & !B.Contains(x);
    }

    public override string ToString()
    {
        return $"{A}/{B}";
    }

    public override string ToLatex()
    {
        return $"{A.ToLatex()}/{B.ToLatex()}";
    }

    public override bool IsSubset(Set other)
    {
        if (A.IsSubset(other))
            return true;
        
        return base.IsSubset(other);
    }

    public override bool IsSuperset(Set other)
    {
        return base.IsSuperset(other);
    }
}