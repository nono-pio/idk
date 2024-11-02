using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.ComplexExpressions;

namespace ConsoleApp1.Core.Sets;
using Boolean = Booleans.Boolean;


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
    public Set Positive => Intersection(this, Interval(0, Expr.Inf));
    public Set Negative => Intersection(this, Interval(Expr.NegInf, 0));
    
    public virtual bool IsElementsNatural => throw new NotImplementedException();
    public virtual bool IsElementsInteger => IsElementsNatural;
    public virtual bool IsElementsRational => IsElementsInteger;
    public virtual bool IsElementsReal => IsElementsRational;
    public virtual bool IsElementsComplex => IsElementsReal;

    public virtual bool IsElementsPositive => throw new NotImplementedException();
    public virtual bool IsElementsNegative => throw new NotImplementedException();

    public bool IsEmpty => IsEmptySet();
    public bool IsEmptySet() => this is SetEmpty;

    public bool IsR => this is Real;

    /* Set Creation */
    public static implicit operator Set(Expr expr) => ArraySet(expr);

    /* Operation on Sets */

    public Set UnionWith(Set other)
    {
        return Union(this, other);
    }

    public Set Intersect(Set other)
    {
        return Intersection(this, other);
    }
    
    public Set Complement(Set? univers = null)
    {
        return SetDifference(univers ?? R, this);
    }
    

    /**/
    public virtual bool IsEnumerable => false;
    public virtual IEnumerable<Expr> GetEnumerable() => throw new NotImplementedException();

    /**/

    public bool IsDisjoint(Set other)
    {
        return Intersection(other).IsEmpty;
    }
    
    /**/   
    public virtual Expr? Supremum()
    {
        throw new NotImplementedException($"{this}.Supremum() is not implemented.");
    }
    
    public virtual Expr? Infimum()
    {
        throw new NotImplementedException($"{this}.Infimum() is not implemented.");
    }
    
    /**/
    public virtual Set Boundary()
    {
        throw new NotImplementedException($"{this}.Boundary() is not implemented.");
    }
    
    public Set Closure()
    {
        return Union(Boundary());
    }
    
    public Set Interior()
    {
        return Intersection(Closure());
    }
    
    /*  */
    public abstract Boolean Contains(Expr x);
    
    public virtual bool IsSubset(Set other)
    {
        throw new NotImplementedException();
    }
    
    public virtual bool IsSuperset(Set other)
    {
        throw new NotImplementedException();
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        var properties = GetType().GetProperties().Where(p => p.DeclaringType != typeof(Set));
        foreach (var property in properties)
        {
            var value1 = property.GetValue(this);
            var value2 = property.GetValue(obj);
            
             if (!Equals(value1, value2))
                return false;
        }
        return true;
    }

    public static bool operator ==(Set A, Set B)
    {
        return A.Equals(B);
    }

    public static bool operator !=(Set A, Set B)
    {
        return !(A == B);
    }


    public abstract string ToLatex();
}