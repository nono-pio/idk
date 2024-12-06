using System.Diagnostics;
using System.Numerics;

namespace Rings.primes;


public sealed class BigPrimes {
    private static readonly BigInteger MAX_INT = new BigInteger(int.MaxValue);
    private static readonly Well1024a privateRandom = new Well1024a(0x1a9e2b8f3c7d6a4bL);

    private BigPrimes() {}
    
    public static bool isPrime(long n) {
        if (n < int.MaxValue)
            return SmallPrimes.isPrime((int) n);
        return isPrime(new BigInteger(n));
    }

    /**
     * Strong primality test. Switches between trial divisions, probabilistic Miller-Rabin (ensures that is not prime),
     * probabilistic Lucas test (ensures that is prime) and finally (if all above fail to provide deterministic answer)
     * to Pollard's p-1, Pollard's rho and quadratic sieve.
     *
     * @param n number to test
     * @return {@code true} if input is certainly prime, {@code false} is certainly composite
     */
    public static bool isPrime(BigInteger n) {
        if (n.Sign < 0)
            throw new ArgumentException("Argument must be positive");

        //small known primes
        if (n.CompareTo(SieveOfAtkin.SmallPrimesSieve.getLimitAsBigInteger()) < 0)
            return SieveOfAtkin.SmallPrimesSieve.isPrime((int) n);

        //switch to deterministic Miller-Rabin
        if (n.CompareTo(MAX_INT) < 0)
            return SmallPrimes.isPrime((int)n);

        //ok <- some trial divisions
        foreach (int p in SmallPrimes.SmallPrimes12)
            if ((n % new BigInteger(p)).IsZero)
                return false;

        //probabilistic Miller-Rabin test
        if (!n.isProbablePrime(5))
            return false;

        //hard Lucas test
        if (LucasPrimalityTest(n, 20, privateRandom))
            return true;

        //ok <- this is really strange if we are here
        return findFactorHard(n).Equals(n);
    }

    public static bool LucasPrimalityTest(BigInteger n, int k, RandomGenerator rnd) {
        int bound = n.CompareTo(MAX_INT) > 0 ? int.MaxValue : (int)n;
        BigInteger nMinusOne = n - 1;

        List<BigInteger> factors = null;
        while (k-- > 0) {
            BigInteger a = new BigInteger(7 + rnd.nextInt(bound - 7));
            if (!BigInteger.ModPow(a, nMinusOne, n).IsOne)
                return false;

            if (factors == null)
                factors = primeFactors(nMinusOne);
            foreach (BigInteger q in factors)
                if (BigInteger.ModPow(a, nMinusOne / q, n).IsOne)
                    goto loop1; // Verify is that correct
            return true;
            loop1: ;
        }
        return false;
    }

    /**
     * Return the smallest prime greater than or equal to n.
     *
     * @param n a positive number.
     * @return the smallest prime greater than or equal to n.
     * @throws IllegalArgumentException if n &lt; 0.
     */
    public static BigInteger nextPrime(BigInteger n) {
        while (!isPrime(n))
            n = n.nextProbablePrime();
        return n;
    }

    /**
     * Return the smallest prime greater than or equal to n.
     *
     * @param n a positive number.
     * @return the smallest prime greater than or equal to n.
     * @throws IllegalArgumentException if n &lt; 0.
     */
    public static long nextPrime(long n) {
        BigInteger nb = new BigInteger(n);
        while (!isPrime(nb))
            nb = nb.nextProbablePrime();
        return (long)nb;
    }

    /**
     * Fermat's factoring algorithm works like trial division, but walks in the opposite direction. Thus, it can be used
     * to factor a number that we know has a factor in the interval [Sqrt(n) - upperBound, Sqrt(n) + upperBound].
     *
     * @param n          number to factor
     * @param upperBound upper bound
     * @return a single factor
     */
    public static BigInteger fermat(BigInteger n, long upperBound) {
        long cnt = 0;
        BigInteger x = primes.QuadraticSieve.sqrtBigInt(n) + 1;
        BigInteger u = (x << 1) + 1;
        BigInteger v = 1;
        BigInteger r = x * x - n;
        while (!r.IsZero) {
            cnt++;
            if (cnt > upperBound)
                return 0;
            while (r.CompareTo(0) > 0) {
                r -= v;
                v += 2;
            }
            if (r.CompareTo(0) < 0) {
                r += u;
                u += 2;
            }
        }
        return (u - v) >> 1;
    }

    /**
     * Pollards's rho algorithm (random search version).
     *
     * @param n        integer to factor
     * @param attempts number of random attempts
     * @return a single factor of {@code n} or null if no factors found
     */
    public static BigInteger? PollardRho(BigInteger n, int attempts, RandomGenerator rn) {
        // check divisibility by 2
        if ((n % 2).IsZero) 
            return 2;

        BigInteger divisor;
        BigInteger c = new BigInteger(n.bitLength(), rn);
        BigInteger x = new BigInteger(n.bitLength(), rn);
        BigInteger xx = x;

        do {
            x = (x * x % n + c) % n;
            xx = (xx * xx % n + c) % n;
            xx = (xx * xx % n + c) % n;
            divisor = BigInteger.GreatestCommonDivisor(x - xx, n);
        } while (attempts-- > 0 && divisor.IsOne);

        return divisor.IsOne ? null : divisor;
    }

    /**
     * Pollards's rho algorithm.
     *
     * @param n          integer to factor
     * @param upperBound expected B-smoothness
     * @return a single factor of {@code n} or null if no factors found
     */
    public static BigInteger? PollardRho(BigInteger n, long upperBound) {
        long range = 1;
        long terms = 0;
        BigInteger x1 = 2;
        BigInteger x2 = 5;
        BigInteger product = BigInteger.One;
        while (terms <= upperBound) {
            for (long j = 1; j <= range; j++)
            {
                x2 = (x2 * x2 + 1) % n;
                product = product * (x1 - x2) % n;
                if (terms++ > upperBound)
                    break;
                if (terms % 5 == 0) {
                    BigInteger g = BigInteger.GreatestCommonDivisor(n, product);
                    if (g.CompareTo(BigInteger.One) > 0) {
                        return g;
                    }
                    product = BigInteger.One;
                }
            }
            x1 = x2;
            range *= 2;
            for (long j = 1; j <= range; j++)
                x2 = (x2 * x2 + 1) % n;
        }
        return null;
    }


    /**
     * Pollards's p-1 algorithm.
     *
     * @param n          integer to factor
     * @param upperBound expected B-smoothness
     * @return a single factor of {@code n} or null if no factors found
     */
    public static BigInteger? PollardP1(BigInteger n, long upperBound) {
        BigInteger g, i, m;
        for (int outerCnt = 0; outerCnt < 5; outerCnt++) {
            m = outerCnt switch
            {
                0 => 2,
                1 => 3,
                2 => 4,
                3 => 5,
                4 => 7,
                _ => 2
            };
            i = BigInteger.One;
            for (long cnt = 2; cnt <= upperBound; cnt++) {
                i += 1;
                m = BigInteger.ModPow(m, i, n);
                if (cnt % 5 == 0) {
                    g = BigInteger.GreatestCommonDivisor(n, m - 1);
                    if ((g.CompareTo(BigInteger.One) > 0) && (g.CompareTo(n) < 0)) {
                        return g;
                    }
                }
            }
        }
        return null;
    }

    public static BigInteger? QuadraticSieve(BigInteger n, int bound) {
        return new QuadraticSieve(n).quadraticSieve(bound);
    }

    static BigInteger findFactorHard(BigInteger n) {
        int numBits = n.bitCount();
        BigInteger? r;

        //switching between algorithms
        //some hard heuristics is here

        if (numBits < 20) {
            // t = 1e4 - 3e6
            r = PollardRho(n, 131_072);
            if (r is not null)
                return r.Value;
        }

        if (numBits < 30) {
            // t = 5e6
            r = PollardRho(n, 1024, privateRandom);
            if (r is not null)
                return r.Value;
            // t = 2e6 - 5e7
            r = PollardRho(n, 131_072);
            if (r is not null)
                return r.Value;
        }

        if (numBits < 60) {
            // t = 2e5
            r = PollardRho(n, 128);
            if (r is not null)
                return r.Value;

            // t = 5e5
            r = PollardRho(n, 128, privateRandom);
            if (r is not null)
                return r.Value;

            // t = 1e6
            r = PollardP1(n, 128);
            if (r is not null)
                return r.Value;

            // t = 2e7
            r = PollardRho(n, 131_072);
            if (r is not null)
                return r.Value;
        }

        //<-really large number with large primes

        // t = 5e5
        r = PollardRho(n, 128);
        if (r is not null)
            return r.Value;

        // t = 5e5
        r = PollardP1(n, 128);
        if (r is not null)
            return r.Value;

        // t = 5e6
        r = PollardRho(n, 1032, privateRandom);
        if (r is not null)
            return r.Value;

        // t = 1e8
        r = PollardRho(n, 131_072);
        if (r is not null)
            return r.Value;


        // t = 1e9
        r = PollardP1(n, 131_072);
        if (r is not null)
            return r.Value;

        // t = 1e9 -> oo
        // be sure that trial division is done
        // TODO assert n.compareTo(BigInteger.valueOf(32768)) >= 0 : n;
        r = QuadraticSieve(n, 32768);
        Debug.Assert(r is not null);

        if (r.Value.IsOne) //<- overcome issue with QS
            return n;
        return r.Value;
    }

    private static bool checkKnownSmallPrime(BigInteger b) {
        return b.CompareTo(SieveOfAtkin.SmallPrimesSieve.getLimitAsBigInteger()) < 0
                && SieveOfAtkin.SmallPrimesSieve.isPrime((int)b);
    }

    /**
     * Prime factors decomposition. The algorithm switches between trial divisions, Pollard's p-1, Pollard's rho and
     * quadratic sieve.
     *
     * @param num number to factorize
     * @return list of prime factors of n
     * @throws IllegalArgumentException if n is negative
     */
    public static long[] primeFactors(long num) {
        return primeFactors(new BigInteger(num)).Select(b => (long)b).ToArray();
    }

    /**
     * Prime factors decomposition. The algorithm switches between trial divisions, Pollard's p-1, Pollard's rho and
     * quadratic sieve.
     *
     * @param num number to factorize
     * @return list of prime factors of n
     * @throws IllegalArgumentException if n is negative
     */
    public static List<BigInteger> primeFactors(BigInteger num) {
        List<BigInteger> factors = new ();

        if (num.CompareTo(2) < 0) {
            factors.Add(num);
            return factors;
        }

        //fast check for small prime
        if (checkKnownSmallPrime(num)) {
            factors.Add(num);
            return factors;
        }

        //start with trial divisions
        num = TrialDivision(num, factors);

        if (num.IsOne)
            return factors;

        if (isPrime(num)) {
            factors.Add(num);
            return factors;
        }

        //switch to hard algorithms
        HardFactor(num, factors);

        return factors;
    }

    static BigInteger TrialDivision(BigInteger num, List<BigInteger> factors) {
        foreach (int p in SmallPrimes.SmallPrimes12) {
            BigInteger prime = new BigInteger(p);
            var (q, r) = BigInteger.DivRem(num, prime);
            while (r.IsZero) {
                num = q;
                factors.Add(prime);
                (q, r) = BigInteger.DivRem(num, prime);
            }
        }
        return num;
    }

    static void HardFactor(BigInteger num, List<BigInteger> factors) {
        BigInteger factor;
        while (true) {
            factor = findFactorHard(num);
            if (factor.IsOne || factor.Equals(num)) {
                factors.Add(num);
                return;
            } else {
                if (!isPrime(factor))
                    HardFactor(factor, factors);
                else
                    factors.Add(factor);
            }
            num = num / factor;
        }
    }
}
