namespace ConsoleApp1.Core.Sets;

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