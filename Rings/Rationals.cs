using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using Rings.io;

namespace Rings;

public sealed class Rationals<E> : Ring<Rational<E>>
{
    private static readonly long serialVersionUID = 1L;

    /** Ring that numerator and denominator belongs to */
    public readonly Ring<E> ring;

    public Rationals(Ring<E> ring)
    {
        this.ring = ring;
    }

    /** Gives rational with a given numerator and unit denominator */
    public Rational<E> mkNumerator(E num)
    {
        return new Rational<E>(ring, num);
    }

    /** Gives rational with a given numerator and unit denominator */
    public Rational<E> mkNumerator(long num)
    {
        return mkNumerator(ring.valueOf(num));
    }

    /** Gives rational with a given denominator and unit numerator */
    public Rational<E> mkDenominator(E den)
    {
        return new Rational<E>(ring, ring.getOne(), den);
    }

    /** Gives rational with a given denominator and unit numerator */
    public Rational<E> mkDenominator(long den)
    {
        return mkDenominator(ring.valueOf(den));
    }

    /** Gives rational with a given numerator and denominator */
    public Rational<E> mk(E num, E den)
    {
        return new Rational<E>(ring, num, den);
    }

    /** Gives rational with a given numerator and denominator */
    public Rational<E> mk(long num, long den)
    {
        return new Rational<E>(ring, ring.valueOf(num), ring.valueOf(den));
    }


    public bool isField()
    {
        return true;
    }


    public bool isEuclideanRing()
    {
        return true;
    }


    public BigInteger cardinality()
    {
        return null;
    }


    public BigInteger characteristic()
    {
        return BigInteger.Zero;
    }


    public bool isPerfectPower()
    {
        return false;
    }


    public BigInteger perfectPowerBase()
    {
        return null;
    }


    public BigInteger perfectPowerExponent()
    {
        return null;
    }


    public Rational<E> add(Rational<E> a, Rational<E> b)
    {
        return a.add(b);
    }

//    
//    public Rational<E> addMutable(Rational<E> a, Rational<E> b) {
//        return a.addMutable(b);
//    }


    public Rational<E> subtract(Rational<E> a, Rational<E> b)
    {
        return a.subtract(b);
    }

//    
//    public Rational<E> subtractMutable(Rational<E> a, Rational<E> b) {
//        return a.subtractMutable(b);
//    }


    public Rational<E> multiply(Rational<E> a, Rational<E> b)
    {
        return a.multiply(b);
    }

//    
//    public Rational<E> multiplyMutable(Rational<E> a, Rational<E> b) {
//        return a.multiplyMutable(b);
//    }


    public Rational<E> negate(Rational<E> element)
    {
        return element.negate();
    }

//    
//    public Rational<E> negateMutable(Rational<E> element) {
//        return element.negateMutable();
//    }


    public int signum(Rational<E> element)
    {
        return element.signum();
    }
    
    public Rational<E>[] divideAndRemainder(Rational<E> dividend, Rational<E> divider)
    {
        return  [ dividend.divide(divider), Rational<E>.zero(ring) ];
    }

//    
//    public Rational<E> divideExactMutable(Rational<E> dividend, Rational<E> divider) {
//        return dividend.divideMutable(divider);
//    }


    public Rational<E> reciprocal(Rational<E> element)
    {
        return element.reciprocal();
    }


    public Rational<E> gcd(Rational<E> a, Rational<E> b)
    {
        return Rational<E>.one(ring);
    }


    private FactorDecomposition<Rational<E>> factor(Rational<E> element, Func<E, FactorDecomposition<E>> factor)
    {
        if (element.isZero())
            return FactorDecomposition<Rational<E>>.of(this, element);

        FactorDecomposition<E> numFactors = element.numerator.stream()
            .map(factor)
            .reduce(FactorDecomposition.empty(ring), FactorDecomposition::addAll);
        FactorDecomposition<Rational<E>> factors = FactorDecomposition.empty(this);

        for (int i = 0; i < numFactors.size(); i++)
            factors.addNonUnitFactor(new Rational<E>(ring, numFactors.get(i)), numFactors.getExponent(i));
        factors.addFactor(new Rational<E>(ring, numFactors.unit), 1);

        FactorDecomposition<E> denFactors = element.denominator.stream()
            .map(factor)
            .reduce(FactorDecomposition.empty(ring), FactorDecomposition::addAll);
        for (int i = 0; i < denFactors.size(); i++)
            factors.addNonUnitFactor(new Rational<E>(ring, ring.getOne(), denFactors.get(i)), denFactors.getExponent(i));
        factors.addFactor(new Rational<E>(ring, ring.getOne(), denFactors.unit), 1);

        return factors;
    }


    public FactorDecomposition<Rational<E>> factorSquareFree(Rational<E> element)
    {
        return factor(element, ring.factorSquareFree);
    }


    public FactorDecomposition<Rational<E>> factor(Rational<E> element)
    {
        return factor(element, ring.factor);
    }


    public Rational<E> getZero()
    {
        return Rational<E>.zero(ring);
    }


    public Rational<E> getOne()
    {
        return Rational<E>.one(ring);
    }


    public bool isZero(Rational<E> element)
    {
        return element.isZero();
    }


    public bool isOne(Rational<E> element)
    {
        return element.isOne();
    }


    public bool isUnit(Rational<E> element)
    {
        return !isZero(element);
    }


    public Rational<E> valueOf(long val)
    {
        return new Rational<E>(ring, ring.valueOf(val));
    }


    public Rational<E> valueOfBigInteger(BigInteger val)
    {
        return new Rational<E>(ring, ring.valueOfBigInteger(val));
    }


    public Rational<E> copy(Rational<E> element)
    {
        return new Rational<E>(true, ring, element.numerator.deepCopy(), element.denominator.deepCopy());
    }


    public Rational<E> valueOf(Rational<E> val)
    {
        if (val.ring.Equals(ring))
            return val;
        else
            return new Rational<E>(ring, val.numerator.map(ring.valueOf), val.denominator.map(ring.valueOf));
    }

    public int compare(Rational<E> o1, Rational<E> o2)
    {
        return o1.compareTo(o2);
    }


    public Rational<E> getNegativeOne()
    {
        return Rational<E>.one(ring).negate();
    }
    

    public Rational<E> randomElement(RandomGenerator rnd)
    {
        long den;
        E eden;
        do
        {
            den = rnd.nextInt();
        } while (ring.isZero(eden = ring.valueOf(den)));

        return new Rational<E>(ring, ring.valueOf(rnd.nextInt()), eden);
    }


    public Rational<E> randomElementTree(RandomGenerator rnd)
    {
        E den;
        do
        {
            den = ring.randomElementTree(rnd);
        } while (ring.isZero(den));

        return new Rational<E>(ring, ring.randomElementTree(rnd), den);
    }


    public IEnumerable<Rational<E>> iterator()
    {
        throw new ArgumentException("Ring of infinite cardinality.");
    }


    public bool equals(Object o)
    {
        if (this == o) return true;
        if (o == null || GetType() != o.GetType()) return false;

        var rationals = (Rationals <E>) o;

        return ring.Equals(rationals.ring);
    }


    public int hashCode()
    {
        return ring.GetHashCode();
    }


    public string toString(IStringifier<Rational<E>> stringifier)
    {
        return ring.Equals(Rings.Z) ? "Q" : "Frac(" + ring.toString(stringifier.substringifier(ring)) + ")";
    }


    public string toString()
    {
        return toString(IStringifier<Rational<E>>.dummy<Rational<E>>());
    }
}