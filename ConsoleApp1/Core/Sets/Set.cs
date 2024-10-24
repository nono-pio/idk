﻿using ConsoleApp1.Core.Expressions.Atoms;
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
    public static Set EmptySet => new EmptySet();
    public static UniversalSet U => new UniversalSet();
    public static Natural N => new Natural();
    public static Integer Z => new Integer();
    public static Rational Q => new Rational();
    public static Real R => new Real();

    public Set Positive => CreateIntersection(this, CreateInterval(0, Expr.Inf));
    public Set Negative => CreateIntersection(this, CreateInterval(Expr.NegInf, 0));
    
    public virtual bool IsElementsNatural => throw new NotImplementedException();
    public virtual bool IsElementsInteger => IsElementsNatural;
    public virtual bool IsElementsRational => IsElementsInteger;
    public virtual bool IsElementsReal => IsElementsRational;
    public virtual bool IsElementsComplex => IsElementsReal;

    public virtual bool IsElementsPositive => throw new NotImplementedException();
    public virtual bool IsElementsNegative => throw new NotImplementedException();

    public bool IsEmpty => IsEmptySet();
    public bool IsEmptySet() => this is EmptySet;

    public bool IsR => this is Real;

    /* Set Creation */
    public static implicit operator Set(Expr expr) => CreateFiniteSet(expr);

    public static Set CreateConditionSet(Boolean condition, Variable variable, Set domain)
    {
        return new ConditionSet(condition, variable, domain);
    }
    
    public static Set CreateFiniteSet(params Expr[] elements)
    {
        return FiniteSet.CreateFiniteSet(elements);
    }

    public static Set CreateInterval(Expr start, Expr end, bool startInclusive = true, bool endInclusive = true)
    {
        return Interval.CreateInterval(start, end, startInclusive, endInclusive);
    }
    
    public static Set CreateUnion(params Set[] sets)
    {
        return Sets.Union.CreateUnion(sets);
    }
    
    public static Set CreateIntersection(params Set[] sets)
    {
        return new Intersection(sets);
        throw new NotImplementedException(); // return Intersection.CreateIntersection(elements);
    }
    
    public static Set CreateComplement(Set set, Set universe)
    {
        return new Complement(set, universe);
        throw new NotImplementedException(); // return Complement.CreateComplement(set, universe);
    }

    /* Operation on Sets */

    public Set Union(Set other)
    {
        return CreateUnion(this, other);
    }

    public Set Intersection(Set other)
    {
        return CreateIntersection(this, other);
    }

    public virtual Set Complement(Set universe)
    {
        return CreateComplement(this, universe);
    }

    public Set SymmetricDifference(Set other)
    {
        throw new NotImplementedException();
    }

    // Power Set of this set

    public Set PowerSet()
    {
        throw new NotImplementedException();
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
    
    // TODO
    public Boolean IsSubset(Set other)
    {
        throw new NotImplementedException();
    }
    
    public Boolean IsProperSubset(Set other)
    {
        throw new NotImplementedException();
    }
    
    public Boolean IsSuperset(Set other)
    {
        throw new NotImplementedException();
    }
    
    public Boolean IsProperSuperset(Set other)
    {
        throw new NotImplementedException();
    }

    public bool? IsOpen()
    {
        return Intersection(Boundary()).IsEmpty;
    }
    
    public Boolean? IsClosed()
    {
        return Boundary().IsSubset(this);
    }

    public abstract string ToLatex();
}