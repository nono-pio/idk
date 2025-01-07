using System.Diagnostics;
using OptTest.Utils;

namespace OptTest.Basics;

// TODO Replace
using Integer = int;
using PositiveInteger = int;
using NonNegativeInteger = uint;
using Boolean = bool;

// files: catdef, naalgc

public interface AbelianGroup<Self> : CancellationAbelianMonoid<Self>
{
    public Self Negate(Self a);
    public Self Subtract(Self a, Self b) => Add(a, Negate(b));
    public Self Multiply(Self a, Integer n);
}

public interface AbelianMonoid<Self> : AbelianSemiGroup<Self>
{
    public Self Zero { get; }
    public Self Sample => Zero;
    public Boolean IsZero(Self a) => Equal(a, Zero);
    public Self Multiply(Self a, NonNegativeInteger n);
    public Boolean IsOpposite(Self a, Self b) => IsZero(Add(a, b));
}

public interface AbelianSemiGroup<Self> : SetCategory<Self>
{
    public Self Add(Self a, Self b);
    public Self Multiply(PositiveInteger n, Self a);
}


public interface Algebra<Self, R> : Ring<Self>, NonAssociativeAlgebra<Self, R> where R : CommutativeRing<R> where Self : Algebra<Self, R>
{
    public Self Coerce(R r) => Multiply(r, One);
}

public interface BiModule<Self, R, S> : LeftModule<Self, R>, RightModule<Self, S>;

public interface
    LeftModule<Self,
        R> : AbelianSemiGroup<Self> // TODO if R has AbelianMonoid : AbelianMonoid, if R has AbelianGroup : AbelianGroup
{
    public Self Multiply(R r, Self a);
}

public interface
    RightModule<Self,
        R> : AbelianSemiGroup<Self> // TODO if R has AbelianMonoid : AbelianMonoid, if R has AbelianGroup : AbelianGroup
{
    public Self Multiply(Self a, R r);
}

public interface CancellationAbelianMonoid<Self> : AbelianMonoid<Self>
{
    public MayFail<Self> SubtractIfCan(Self a, Self b);
}

public interface CommutativeRing<Self> : Ring<Self>, Algebra<Self, Self>, CommutativeStar
    where Self : CommutativeRing<Self>
{
}

public interface DifferentialRing<Self> : Ring<Self>
{
    public Self Differentiate(Self a);
    public Self D(Self a) => Differentiate(a);

    public Self Differentiate(Self a, NonNegativeInteger n)
    {
        for (int i = 0; i < n; i++)
        {
            a = Differentiate(a);
        }

        return a;
    }

    public Self D(Self a, NonNegativeInteger n) => Differentiate(a, n);
}

public interface EuclideanDomain<Self> : PrincipalIdealDomain<Self> where Self : EuclideanDomain<Self>
{
    public Boolean IsSizeLess(Self a, Self b)
    {
        if (IsZero(b)) return false;
        if (IsZero(a)) return true;
        return EuclideanSize(a) < EuclideanSize(b);
    }

    public NonNegativeInteger EuclideanSize(Self a);
    public (Self Quotient, Self Remainder) Divide(Self a, Self b);
    public Self Quo(Self a, Self b) => Divide(a, b).Quotient;
    public Self Rem(Self a, Self b) => Divide(a, b).Remainder;
    public (Self Coef1, Self Coef2, Self Generator) ExtendedEuclidean(Self a, Self b);
    public MayFail<(Self Coef1, Self Coef2)> ExtendedEuclidean(Self a, Self b, Self c);
    public MayFail<Self[]> MultiEuclidean(Self[] fs, Self z);
}

public interface PrincipalIdealDomain<Self> : GcdDomain<Self> where Self : PrincipalIdealDomain<Self>
{
    public (Self[] Coef, Self Generator) PrincipalIdeal(Self[] fs);
    public MayFail<Self[]> ExpressIdealMember(Self[] fs, Self h);
}

public interface EntireRing<Self> : Ring<Self>, noZeroDivisors
{
    public MayFail<Self> Exquo(Self a, Self b);
    public (Self Unit, Self Canonical, Self Associate) UnitNormal(Self a);
    public Self UnitCanonical(Self a) => UnitNormal(a).Canonical;
    public Boolean IsAssociates(Self a, Self b);
    public Boolean IsUnit(Self a);
}

public interface IntegralDomain<Self> : CommutativeRing<Self>, EntireRing<Self> where Self : IntegralDomain<Self>;

public interface LeftOreRing<Self> : EntireRing<Self>
{
    public (Self lcmRes, Self Coef1, Self Coef2) LcmCoef(Self a, Self b);
}

public interface GcdDomain<Self> : IntegralDomain<Self>, LeftOreRing<Self> where Self : GcdDomain<Self>
{
    public Self Gcd(Self a, Self b);
    public Self Gcd(Self[] l) => Reduce(Gcd, l, Zero, One);

    public Self Lcm(Self a, Self b)
    {
        if (IsZero(a) || IsZero(b)) return Zero;
        var lcm = Exquo(b, Gcd(a, b));
        if (!lcm.IsFailed) return Multiply(a, lcm.Value);
        throw new UnreachableException("Bad Implementation of Gcd");
    }

    public Self Lcm(Self[] l) => Reduce(Lcm, l, One, Zero);

    // TODO
    // public SparseUnivariatePolynomial<Self> GcdPolynomial(SparseUnivariatePolynomial<Self> p,
    //     SparseUnivariatePolynomial<Self> q)
    // {
    //     
    // }
}

public interface Ring<Self> : Rng<Self>, SemiRing<Self>, NonAssociativeRing<Self>, unitsKnown;

public interface SemiRng<Self> : NonAssociativeSemiRng<Self>, BiModule<Self, Self, Self>, SemiGroup<Self>;

public interface SemiRing<Self> : SemiRng<Self>, NonAssociativeSemiRing<Self>, Monoid<Self>;

public interface Rng<Self> : SemiRng<Self>, NonAssociativeRng<Self>;

public interface SemiGroup<Self> : Magma<Self>;

public interface SetCategory<Self> : BasicType<Self>, CoercibleTo<OutputForm>;

public interface BasicType<Self>
{
    public Boolean Equal(Self a, Self b);
    public Boolean NotEqual(Self a, Self b) => !Equal(a, b);
}

public interface Monoid<Self> : SemiGroup<Self>, MagmaWithUnit<Self>;

public interface Magma<Self> : SetCategory<Self>
{
    public Self Multiply(Self a, Self b);

    public Self RightPower(Self a, PositiveInteger n)
    {
        if (n == 1)
            return a;
        var res = a;
        for (int i = 0; i < n; i++)
        {
            res = Multiply(res, a);
        }

        return res;
    }

    public Self LeftPower(Self a, PositiveInteger n)
    {
        if (n == 1)
            return a;
        var res = a;
        for (int i = 0; i < n; i++)
        {
            res = Multiply(a, res);
        }

        return res;
    }

    public Self Power(Self a, PositiveInteger n);
}

public interface MagmaWithUnit<Self> : Magma<Self>
{
    public Self One { get; }
    public Self Sample => One;
    public Boolean IsOne(Self a) => Equal(a, One);
    public Self RightPower(Self a, NonNegativeInteger n){
        if (n == 0)
            return One;
        var res = One;
        for (int i = 0; i < n; i++)
        {
            res = Multiply(res, a);
        }

        return res;
    }
    public Self LeftPower(Self a, NonNegativeInteger n){
        if (n == 0)
            return One;
        var res = One;
        for (int i = 0; i < n; i++)
        {
            res = Multiply(res, a);
        }

        return res;
    }
    public Self Power(Self a, NonNegativeInteger n);
    public MayFail<Self> Recip(Self a) => IsOne(a) ? new MayFail<Self>(One) : new MayFail<Self>();
    public MayFail<Self> LeftRecip(Self a);
    public MayFail<Self> RightRecip(Self a);
}

public interface NonAssociativeSemiRng<Self> : AbelianSemiGroup<Self>, Magma<Self>
{
    public Self AntiCommutator(Self a, Self b) => Add(Multiply(a, b), Multiply(b, a));
}

public interface NonAssociativeSemiRing<Self> : NonAssociativeSemiRng<Self>, AbelianMonoid<Self>, MagmaWithUnit<Self>;

public interface NonAssociativeRng<Self> : NonAssociativeSemiRng<Self>, AbelianGroup<Self>
{
    public Self Associator(Self a, Self b, Self c) => Subtract(Multiply(Multiply(a, b), c), Multiply(a, Multiply(b, c)));
    public Self Commutator(Self a, Self b) => Subtract(Multiply(a, b), Multiply(b, a));
}

public interface NonAssociativeRing<Self> : NonAssociativeSemiRing<Self>, NonAssociativeRng<Self>
{
    public NonNegativeInteger Characteristic { get; }
    public Self Coerce(Integer n) => Multiply(n, One);
}

public interface NonAssociativeAlgebra<Self, R> : NonAssociativeRng<Self>, Module<Self, R> where R : CommutativeRing<R>
{
    public Self PlenaryPower(Self a, PositiveInteger n)
    {
        if (n == 1)
            return a;
        var n1 = n - 1;
        return Multiply(PlenaryPower(a, n1), PlenaryPower(a, n1));
    }
}

public interface Module<Self, R> : BiModule<Self, R, R>
{
}