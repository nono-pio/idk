using System.Collections;
using System.Numerics;

namespace Polynomials.Primes;

public sealed class SieveOfAtkin
{
    private readonly int limit;
    private readonly BigInteger blimit;
    private readonly BitArray sieve;

    private SieveOfAtkin(int limit, BigInteger blimit, BitArray sieve)
    {
        this.limit = limit;
        this.blimit = blimit;
        this.sieve = sieve;
    }

    SieveOfAtkin ToLimit(int newLimit)
    {
        return limit == newLimit ? this : new SieveOfAtkin(newLimit, new BigInteger(newLimit), sieve);
    }


    private SieveOfAtkin(int limit) : this(limit, new BigInteger(limit))
    {
    }


    private SieveOfAtkin(int limit, BigInteger blimit)
    {
        this.limit = limit;
        this.blimit = blimit;
        this.sieve = new BitArray(limit + 1);
        int limitSqrt = (int)Math.Sqrt(limit);

        // the sieve works only for integers > 3, so
        // set these trivially to their proper values
        sieve.Set(2, true);
        sieve.Set(3, true);

        // loop through all possible integer values for x and y
        // up to the square root of the max prime for the sieve
        // we don't need any larger values for x or y since the
        // max value for x or y will be the square root of n
        // in the quadratics
        // the theorem showed that the quadratics will produce all
        // primes that also satisfy their wheel factorizations, so
        // we can produce the value of n from the quadratic first
        // and then filter n through the wheel quadratic
        // there may be more efficient ways to do this, but this
        // is the design in the Wikipedia article
        // loop through all integers for x and y for calculating
        // the quadratics
        for (int x = 1; x <= limitSqrt; x++)
        {
            for (int y = 1; y <= limitSqrt; y++)
            {
                // first quadratic using m = 12 and r in R1 = {r : 1, 5}
                int n = (4 * x * x) + (y * y);
                if (n <= limit && (n % 12 == 1 || n % 12 == 5))
                    sieve[n] = !sieve[n];

                // second quadratic using m = 12 and r in R2 = {r : 7}
                n = (3 * x * x) + (y * y);
                if (n <= limit && (n % 12 == 7))
                    sieve[n] = !sieve[n];
                
                // third quadratic using m = 12 and r in R3 = {r : 11}
                n = (3 * x * x) - (y * y);
                if (x > y && n <= limit && (n % 12 == 11))
                    sieve[n] = !sieve[n]; // note that R1 union R2 union R3 is the set R
                // R = {r : 1, 5, 7, 11}
                // which is all values 0 < r < 12 where r is
                // a relative prime of 12
                // Thus all primes become candidates
            }
        }


        // remove all perfect squares since the quadratic
        // wheel factorization filter removes only some of them
        for (int n = 5; n <= limitSqrt; n++)
        {
            if (sieve[n])
            {
                int x = n * n;
                for (int i = x; i <= limit; i += x)
                    sieve.Set(i, false);
            }
        }
    }

    public bool IsPrime(int n)
    {
        if (n > limit)
            throw new IndexOutOfRangeException("Out of sieve bounds.");
        return sieve[n];
    }


    public int LastPrime()
    {
        for (int i = limit; i >= 0; --i)
            if (IsPrime(i))
                return i;
        throw new InvalidOperationException("No ant primes in the sieve");
    }

    public int RandomPrime(Random rnd)
    {
        int i;
        do
        {
            i = rnd.Next(limit);
        } while (!IsPrime(i));

        return i;
    }

    public int GetLimit()
    {
        return limit;
    }

    public BigInteger GetLimitAsBigInteger()
    {
        return blimit;
    }

    //cached sieve
    public static readonly SieveOfAtkin SmallPrimesSieve = new SieveOfAtkin(64 * 1024);

    public static SieveOfAtkin CreateSieve(int limit)
    {
        if (limit <= SmallPrimesSieve.limit)
            return SmallPrimesSieve.ToLimit(limit);
        return new SieveOfAtkin(limit);
    }

    public static SieveOfAtkin CreateSieve(BigInteger limit)
    {
        if (limit.CompareTo(SmallPrimesSieve.blimit) < 9)
            return SmallPrimesSieve.ToLimit((int)limit);
        return new SieveOfAtkin((int)limit, limit);
    }
}