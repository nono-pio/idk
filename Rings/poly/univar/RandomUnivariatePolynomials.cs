using System.Numerics;
using Rings;
using Rings.poly.univar;

public static class RandomUnivariatePolynomials {

    private const int DEFAULT_BOUND = 100;


    public static Poly randomPoly<Poly>(Poly factory, int degree, RandomGenerator rnd) where Poly :IUnivariatePolynomial<Poly> {
        if (factory is UnivariatePolynomialZ64)
            return (Poly) randomPoly(degree, rnd);
        else if (factory is UnivariatePolynomialZp64)
            return (Poly) randomMonicPoly(degree, ((UnivariatePolynomialZp64) factory).modulus(), rnd);
        else if (factory is UnivariatePolynomial) {
            UnivariatePolynomial p = randomPoly(degree, ((UnivariatePolynomial) factory).ring, rnd);
            if (factory.isOverField())
                p = p.monic();
            return (Poly) p;
        }
        throw new Exception(factory.GetType().toString());
    }

    
    public static UnivariatePolynomialZ64 randomPoly(int degree, RandomGenerator rnd) {
        return randomPoly(degree, DEFAULT_BOUND, rnd);
    }

    
    public static UnivariatePolynomialZp64 randomMonicPoly(int degree, long modulus, RandomGenerator rnd) {
        UnivariatePolynomialZ64 r = randomPoly(degree, modulus, rnd);
        while (r.data[degree] % modulus == 0) {
            r.data[r.Degree] = rnd.nextLong();
        }
        return r.modulus(modulus, false).monic();
    }

    
    public static UnivariatePolynomial<BigInteger> randomMonicPoly(int degree, BigInteger modulus, RandomGenerator rnd) {
        UnivariatePolynomial<BigInteger> r = randomPoly(degree, modulus, rnd);
        while ((r.data[degree]%modulus).IsZero) {r.data[r.Degree] = RandomUtil.randomInt(modulus, rnd);}
        return r.setRing(new IntegersZp(modulus)).monic();
    }

    
    public static  UnivariatePolynomial<E> randomMonicPoly<E>(int degree, Ring<E> ring, RandomGenerator rnd) {
        return randomPoly(degree, ring, rnd).monic();
    }

    
    public static UnivariatePolynomialZ64 randomPoly(int degree, long bound, RandomGenerator rnd) {
        return UnivariatePolynomialZ64.create(randomLongArray(degree, bound, rnd));
    }

    
    public static UnivariatePolynomial<BigInteger> randomPoly(int degree, BigInteger bound, RandomGenerator rnd) {
        return UnivariatePolynomial.createUnsafe(Rings.Z, randomBigArray(degree, bound, rnd));
    }


    
    public static  UnivariatePolynomial<E> randomPoly<E>(int degree, Ring<E> ring, RandomGenerator rnd) {
        return UnivariatePolynomial.createUnsafe(ring, randomArray(degree, ring, rnd));
    }

    
    public static  UnivariatePolynomial<E> randomPoly<E>(int degree, Ring<E> ring, Func<RandomGenerator, E> method, RandomGenerator rnd) {
        return UnivariatePolynomial.createUnsafe(ring, randomArray(degree, ring, method, rnd));
    }

    
    public static long[] randomLongArray(int degree, long bound, RandomGenerator rnd) {
        long[] data = new long[degree + 1];
        RandomDataGenerator rndd = new RandomDataGenerator(rnd);
        for (int i = 0; i <= degree; ++i) {
            data[i] = rndd.nextLong(0, bound - 1);
            if (rnd.nextBoolean() && rnd.nextBoolean())
                data[i] = -data[i];
        }
        while (data[degree] == 0)
            data[degree] = rndd.nextLong(0, bound - 1);
        return data;
    }

    
    public static BigInteger[] randomBigArray(int degree, BigInteger bound, RandomGenerator rnd) {
        long lBound = bound.isLong() ? (long)bound : long.MaxValue;
        RandomDataGenerator rndd = new RandomDataGenerator(rnd);
        BigInteger[] data = new BigInteger[degree + 1];
        for (int i = 0; i <= degree; ++i) {
            data[i] = RandomUtil.randomInt(bound, rnd);
            if (rnd.nextBoolean() && rnd.nextBoolean())
                data[i] = data[i].negate();
        }
        while (data[degree].Equals(BigInteger.Zero))
            data[degree] = new BigInteger(rndd.nextLong(0, lBound));
        return data;
    }

    
    public static  E[] randomArray<E>(int degree, Ring<E> ring, RandomGenerator rnd) {
        return randomArray(degree, ring, ring.randomElement, rnd);
    }

    
    public static  E[] randomArray<E>(int degree, Ring<E> ring, Func<RandomGenerator, E> method, RandomGenerator rnd) {
        E[] data = new E[degree + 1];
        for (int i = 0; i <= degree; ++i)
            data[i] = method(rnd);
        while (ring.isZero(data[degree]))
            data[degree] = method(rnd);
        return data;
    }
}
