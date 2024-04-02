namespace ConsoleApp1.Core.Sets;

/*

Different types of sets: (class derived from Set)
1. Finite set {1,2,3} : Special case - empty set {} - Singleton set {1}
2. Interval [1,2[
3. Rule based set {x | x > 0}
(4. Universal set U)

Operations on sets: (class derived from Set)
1. Union
2. Intersection
3. Complement
4. Difference
5. Symmetric difference
6. Cartesian product
7. Power set

 */
public abstract class Set
{

    public abstract long? Length();
    public bool IsFinite() => Length() is not null;
    public bool IsInfinite() => Length() is null;
    
    public abstract bool IsEnumerable();
    public abstract IEnumerable<Expr> GetEnumerable();
    public bool IsEnumerableFinite() => IsFinite() && IsEnumerable();
    public bool IsEnumerableInfinite() => IsInfinite() && IsEnumerable();

    public abstract Expr Max();
    public abstract Expr Min();

    public abstract Expr PrincipalValue();

    public abstract bool Contain();

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
    
}