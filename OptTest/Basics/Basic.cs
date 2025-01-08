using System.Diagnostics;
using OptTest.Utils;

namespace OptTest.Basics;

// TODO Replace
using Boolean = bool;

// files: catdef, naalgc

/// Define :
/// - x == y
/// - x != y
public interface SetCategory<Self> where Self : SetCategory<Self>
{
    public static abstract Boolean operator ==(Self a, Self b);
    public static virtual Boolean operator !=(Self a, Self b) => !(a == b);
}

#region Addition

/// Define :
/// - Addition of x, y : x + y
public interface AbelianSemiGroup<Self> : SetCategory<Self> where Self : AbelianSemiGroup<Self>
{
    public static abstract Self operator +(Self a, Self b);
}

/// Define :
/// - Zero 0 such that x + 0 = x
/// - IsZero if x is zero
/// - IsOpposite if x + y = 0 (or x = -y)
/// - SubtractIfCan : try to subtract b from a
public interface AbelianMonoid<Self> : AbelianSemiGroup<Self> where Self : AbelianMonoid<Self>
{
    protected static abstract Self ZeroFactory { get; }
    public static Self Zero => Self.ZeroFactory;
    public static virtual Boolean IsZero(Self a) => a == Zero;
    public Boolean IsZero() => Self.IsZero((Self)this);
    public static virtual Boolean IsOpposite(Self a, Self b) => (a + b).IsZero();
    public Boolean IsOpposite(Self other) => Self.IsOpposite((Self)this, other);
    public static abstract MayFail<Self> SubtractIfCan(Self a, Self b);
}

/// Define :
/// - Opposite of x : -x
/// - Subtraction of x, y : x + (-y)
public interface AbelianGroup<Self> : AbelianMonoid<Self> where Self : AbelianGroup<Self>
{
    public static abstract Self operator -(Self a);
    public static virtual Self operator -(Self a, Self b) => a + -b;
}

#endregion

#region Multiplication

/// Define :
/// - Multiplication of a, b : a * b
public interface Magma<Self> : SetCategory<Self> where Self : Magma<Self>
{
    public static abstract Self operator *(Self a, Self b);
}

/// Define :
/// - One 1 such that x * 1 = 1 * x = x
/// - IsOne if x is one
/// - Recip of x : 1 if x == 1 failed if x != 1
public interface MagmaWithUnit<Self> : Magma<Self> where Self : MagmaWithUnit<Self>
{
    public static abstract Self OneFactory { get; }
    public static Self One => Self.OneFactory;
    public static virtual Boolean IsOne(Self a) => a == Self.OneFactory;
    public Boolean IsOne() => (Self)this == One;
    public MayFail<Self> Recip(Self a) => a.IsOne() ? new MayFail<Self>(One) : new MayFail<Self>();
}

/// Multiplication with unit
public interface Monoid<Self> : MagmaWithUnit<Self> where Self : Monoid<Self>;

#endregion

#region Rings And Domains

public interface Ring<Self> : AbelianGroup<Self>, Monoid<Self> where Self : Ring<Self>
{
    public static abstract uint CharacteristicFactory { get; }
    public uint Characteristic => Self.CharacteristicFactory;
}

public interface CommutativeRing<Self> : Ring<Self>, CommutativeStar where Self : CommutativeRing<Self>;

public interface EntireRing<Self> : Ring<Self> where Self : EntireRing<Self>
{
    public static abstract MayFail<Self> Exquo(Self a, Self b);
    public static abstract (Self Unit, Self Canonical, Self Associate) UnitNormal(Self a);
    public static virtual Self UnitCanonical(Self a) => Self.UnitNormal(a).Canonical;
    public static abstract Boolean IsUnit(Self a);
}

public interface IntegralDomain<Self> : CommutativeRing<Self>, EntireRing<Self> where Self : IntegralDomain<Self>;

public interface GcdDomain<Self> : IntegralDomain<Self> where Self : GcdDomain<Self>
{
    public static abstract Self Gcd(Self a, Self b);
    public static Self Gcd(Self[] l) => Util.Reduce(Self.Gcd, l, Zero, One);

    public static virtual Self Lcm(Self a, Self b)
    {
        if (a.IsZero() || b.IsZero())
            return Zero;

        var lcm = Self.Exquo(b, Self.Gcd(a, b));
        if (!lcm.IsFailed) 
            return a * lcm.Value;
        throw new UnreachableException("Bad Implementation of Gcd");
    }

    public static Self Lcm(Self[] l) => Util.Reduce(Self.Lcm, l, One, Zero);
}

public interface EuclideanDomain<Self> : GcdDomain<Self> where Self : EuclideanDomain<Self>
{
    public Boolean IsSizeLess(Self a, Self b)
    {
        if (b.IsZero()) return false;
        if (a.IsZero()) return true;
        return EuclideanSize(a) < EuclideanSize(b);
    }

    public uint EuclideanSize(Self a);
    public (Self Quotient, Self Remainder) Divide(Self a, Self b);
    public Self Quo(Self a, Self b) => Divide(a, b).Quotient;
    public Self Rem(Self a, Self b) => Divide(a, b).Remainder;
    public (Self Coef1, Self Coef2, Self Generator) ExtendedEuclidean(Self a, Self b);
    public MayFail<(Self Coef1, Self Coef2)> ExtendedEuclidean(Self a, Self b, Self c);
}

public interface Field<Self> : EuclideanDomain<Self> where Self : Field<Self>
{
    public static abstract Self Inv(Self a);
    public static abstract Self operator /(Self a, Self b);
}

#endregion