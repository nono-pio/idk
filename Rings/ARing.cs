using System.Numerics;

namespace Rings;


public abstract class ARing<E> : Ring<E> {
    private static readonly long serialVersionUID = 1L;
    
    private readonly BigInteger[] perfectPowerDecomposition = new BigInteger[2];
    private volatile bool initialized = false;

    private void checkPerfectPower() {
        // lazy initialization
        if (!initialized) {
            synchronized (perfectPowerDecomposition) {
                if (initialized)
                    return;
        
                initialized = true;
                if (cardinality() == null) {
                    perfectPowerDecomposition[0] = null;
                    perfectPowerDecomposition[1] = null;
                    return;
                }
        
                BigInteger[] ipp = BigIntegerUtil.perfectPowerDecomposition(cardinality());
                if (ipp == null) {
                    // not a perfect power
                    perfectPowerDecomposition[0] = cardinality();
                    perfectPowerDecomposition[1] = BigInteger.One;
                    return;
                }
                perfectPowerDecomposition[0] = ipp[0];
                perfectPowerDecomposition[1] = ipp[1];
        
            }
        }
    }


    public bool isField()
    {
        throw new NotImplementedException();
    }

    public bool isEuclideanRing()
    {
        throw new NotImplementedException();
    }

    public BigInteger cardinality()
    {
        throw new NotImplementedException();
    }

    public BigInteger characteristic()
    {
        throw new NotImplementedException();
    }

    public bool isPerfectPower() {
        checkPerfectPower();
        return perfectPowerDecomposition[1] != null && !perfectPowerDecomposition[1].IsOne;
    }

    
    public BigInteger perfectPowerBase() {
        checkPerfectPower();
        return perfectPowerDecomposition[0];
    }

    
    public BigInteger perfectPowerExponent() {
        checkPerfectPower();
        return perfectPowerDecomposition[1];
    }

    public abstract E add(E a, E b);

    public abstract E subtract(E a, E b);

    public abstract E multiply(E a, E b);

    public abstract E negate(E element);

    public abstract E copy(E element);

    public abstract int compare(E a, E b);

    public abstract E[] divideAndRemainder(E dividend, E divider);

    public abstract E reciprocal(E element);

    public abstract E getZero();

    public abstract E getOne();

    public abstract bool isZero(E element);

    public abstract bool isOne(E element);

    public abstract bool isUnit(E element);

    public abstract E valueOf(long val);

    public abstract E valueOfBigInteger(BigInteger val);

    public abstract E valueOf(E val);

    public abstract IEnumerable<E> iterator();
}
