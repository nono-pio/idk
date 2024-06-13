namespace ConsoleApp1.Core.Sets;

/*

Different types of sets: (class derived from Set)
1. Finite set {1,2,3} : Special case - empty set {} - Singleton set {1}
2. Interval [1,2[
3. Rule based set {x | x > 0} (TODO)
(4. Universal set U)

Operations on sets: (class derived from Set)
1. Union
2. Intersection
3. Complement
4. Difference
5. Symmetric difference (TODO)
6. Cartesian product (TODO)
7. Power set (TODO)

 */
public abstract class Set
{

    /// return null if infinite else return length of the set
    public abstract long? Length();
    public bool IsFinite() => Length() is not null;
    public bool IsInfinite() => Length() is null;
    
    public abstract bool IsEnumerable();
    public bool IsEnumerableFinite() => IsFinite() && IsEnumerable();
    public bool IsEnumerableInfinite() => IsInfinite() && IsEnumerable();
    public virtual IEnumerable<double> GetEnumerable() => throw new Exception("Set is not enumerable.");

    public bool IsEmpty() => this is FiniteSet finiteSet && finiteSet.Length() == 0;
    
    public abstract double Max();
    public abstract double Min();

    public abstract double PrincipalValue();

    public abstract bool Contain(double x);

    public Set Union(Set b)
    {
        throw new NotImplementedException();
    }
    
    public Set Complement(Set b)
    {
        throw new NotImplementedException();
    }
    
    public Set Intersection(Set b)
    {
        throw new NotImplementedException();
    }
    
    public Set SetDifference(Set b)
    {
        throw new NotImplementedException();
    }
    public Set SymmetricDifference(Set b)
    {
        throw new NotImplementedException();
    }
    
    public Set CartesianProduct(Set b)
    {
        throw new NotImplementedException();
    }
    
    // <-- Utils -->
    
    public static Set EmptySet() => new FiniteSet();
    
    public static IEnumerable<Set> GetEnumerableUnionSets(params Set[] sets) 
        => sets.SelectMany(GetEnumerableUnionSets);
    public static IEnumerable<Set> GetEnumerableUnionSets(Set set)
    {
        if (set is Union union)
        {
            return union.Sets.SelectMany(GetEnumerableUnionSets);
        }
        return new[] {set};
    }
}