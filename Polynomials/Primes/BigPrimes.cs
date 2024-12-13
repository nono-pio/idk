using System.Numerics;
using Polynomials.Utils;

namespace Polynomials.Primes;

public static class BigPrimes
{
    private static readonly BigInteger MAX_INT = new BigInteger(int.MaxValue);
    private static readonly Random privateRandom = new Random(0x1a9e2b8f);


    public static bool IsPrime(long n)
    {
        if (n < int.MaxValue)
            return SmallPrimes.IsPrime((int)n);
        return IsPrime(new BigInteger(n));
    }


    public static bool IsPrime(BigInteger n)
    {
        if (n.Sign < 0)
            throw new ArgumentException("Argument must be positive");

        //small known primes
        if (n.CompareTo(SieveOfAtkin.SmallPrimesSieve.GetLimitAsBigInteger()) < 0)
            return SieveOfAtkin.SmallPrimesSieve.IsPrime((int) n);

        //switch to deterministic Miller-Rabin
        if (n.CompareTo(MAX_INT) < 0)
            return SmallPrimes.IsPrime((int) n);

        //ok <- some trial divisions
        foreach (int p in SmallPrimes.SmallPrimes12)
            if (n % new BigInteger(p) == 0)
                return false;

        //probabilistic Miller-Rabin test
        if (!n.IsProbablePrime(5))
            return false;

        //hard Lucas test
        if (LucasPrimalityTest(n, 20, privateRandom))
            return true;

        //ok <- this is really strange if we are here
        return FindFactorHard(n).Equals(n);
    }

    public static bool LucasPrimalityTest(BigInteger n, int k, Random rnd)
    {
        int bound = n.CompareTo(MAX_INT) > 0 ? int.MaxValue : (int) n;
        BigInteger nMinusOne = n - 1;
        List<BigInteger>? factors = null;
        
        while (k-- > 0)
        {
            BigInteger a = new BigInteger(7 + rnd.Next(bound - 7));
            if (!BigInteger.ModPow(a, nMinusOne, n).IsOne)
                return false;
            
            factors ??= PrimeFactors(nMinusOne);
            foreach (BigInteger q in factors)
                if (BigInteger.ModPow(a, nMinusOne / q, n).IsOne)
                    goto loop1;
            return true;
            loop1: ;
        }

        return false;
    }


    public static BigInteger NextPrime(BigInteger n)
    {
        while (!IsPrime(n))
            n = n.NextProbablePrime();
        return n;
    }


    public static long NextPrime(long n)
    {
        BigInteger nb = new BigInteger(n);
        while (!IsPrime(nb))
            nb = nb.NextProbablePrime();
        return (long) nb;
    }


    public static BigInteger Fermat(BigInteger n, long upperBound)
    {
        long cnt = 0;
        BigInteger x = Primes.QuadraticSieve.SqrtBigInt(n) + 1;
        BigInteger u = x * 2 + 1;
        BigInteger v = BigInteger.One;
        BigInteger r = x * x - n;
        while (!r.IsZero)
        {
            cnt++;
            if (cnt > upperBound)
                return BigInteger.Zero;
            while (r.CompareTo(BigInteger.Zero) > 0)
            {
                r -= v;
                v += 2;
            }

            if (r.CompareTo(BigInteger.Zero) < 0)
            {
                r += u;
                u += 2;
            }
        }

        return (u - v) / 2;
    }


    public static BigInteger? PollardRho(BigInteger n, int attempts, Random rn)
    {
        // check divisibility by 2
        if (n % 2 == 0)
            return 2;
        BigInteger divisor; 
        BigInteger c = BigIntegerUtils.RandomBigInt((int)n.GetBitLength(), rn);
        BigInteger x = BigIntegerUtils.RandomBigInt((int)n.GetBitLength(), rn);
        BigInteger xx = x;
        do
        {
            x = (x * x % n + c) % n;
            xx = (xx * xx % n + c) % n;
            xx = (xx * xx % n + c) % n;
            divisor = BigInteger.GreatestCommonDivisor(x - xx, n);
        } while (attempts-- > 0 && divisor.IsOne);

        return divisor.IsOne ? null : divisor;
    }


    public static BigInteger? PollardRho(BigInteger n, long upperBound)
    {
        long range = 1;
        long terms = 0;
        BigInteger x1 = 2;
        BigInteger x2 = 5;
        BigInteger product = BigInteger.One;
        while (terms <= upperBound)
        {
            for (long j = 1; j <= range; j++)
            {
                x2 = (x2 * x2 + 1) % n;
                product = product * (x1 - x2) % n;
                if (terms++ > upperBound)
                    break;
                if (terms % 5 == 0)
                {
                    BigInteger g = BigInteger.GreatestCommonDivisor(n, product);
                    if (g.CompareTo(BigInteger.One) > 0)
                    {
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


    public static BigInteger? PollardP1(BigInteger n, long upperBound)
    {
        BigInteger g, i, m;
        for (int outerCnt = 0; outerCnt < 5; outerCnt++)
        {
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
            for (long cnt = 2; cnt <= upperBound; cnt++)
            {
                i++;
                m = BigInteger.ModPow(m, i, n);
                if (cnt % 5 == 0)
                {
                    g = BigInteger.GreatestCommonDivisor(n, m - 1);
                    if ((g.CompareTo(BigInteger.One) > 0) && (g.CompareTo(n) < 0))
                    {
                        return g;
                    }
                }
            }
        }

        return null;
    }

    public static BigInteger QuadraticSieve(BigInteger n, int bound)
    {
        return new QuadraticSieve(n).QuadraticSieve_(bound);
    }

    static BigInteger FindFactorHard(BigInteger n)
    {
        int numBits = (int)n.GetBitLength();
        BigInteger? r;

        //switching between algorithms
        //some hard heuristics is here
        if (numBits < 20)
        {
            // t = 1e4 - 3e6
            r = PollardRho(n, 131072);
            if (r is not null)
                return r.Value;
        }

        if (numBits < 30)
        {
            // t = 5e6
            r = PollardRho(n, 1024, privateRandom);
            if (r is not null)
                return r.Value;

            // t = 2e6 - 5e7
            r = PollardRho(n, 131072);
            if (r is not null)
                return r.Value;
        }

        if (numBits < 60)
        {
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
            r = PollardRho(n, 131072);
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
        r = PollardRho(n, 131072);
        if (r is not null)
            return r.Value;

        // t = 1e9
        r = PollardP1(n, 131072);
        if (r is not null)
            return r.Value;

        var _r = QuadraticSieve(n, 32768);
        if (_r.IsOne)
            return n;
        return _r;
    }

    private static bool CheckKnownSmallPrime(BigInteger b)
    {
        return b.CompareTo(SieveOfAtkin.SmallPrimesSieve.GetLimitAsBigInteger()) < 0 &&
               SieveOfAtkin.SmallPrimesSieve.IsPrime((int)b);
    }


    public static long[] PrimeFactors(long num)
    {
        return PrimeFactors(new BigInteger(num)).Select(p => (long)p).ToArray();
    }


    public static List<BigInteger> PrimeFactors(BigInteger num)
    {
        List<BigInteger> factors = [];
        if (num.CompareTo(2) < 0)
        {
            factors.Add(num);
            return factors;
        }


        //fast check for small prime
        if (CheckKnownSmallPrime(num))
        {
            factors.Add(num);
            return factors;
        }


        //start with trial divisions
        num = TrialDivision(num, factors);
        if (num.IsOne)
            return factors;
        if (IsPrime(num))
        {
            factors.Add(num);
            return factors;
        }


        //switch to hard algorithms
        HardFactor(num, factors);
        return factors;
    }

    static BigInteger TrialDivision(BigInteger num, List<BigInteger> factors)
    {
        foreach (int p in SmallPrimes.SmallPrimes12)
        {
            BigInteger prime = new BigInteger(p);
            var qr = BigInteger.DivRem(num, prime);
            while (qr.Remainder.IsZero)
            {
                num = qr.Quotient;
                factors.Add(prime);
                qr = BigInteger.DivRem(num, prime);
            }
        }

        return num;
    }

    static void HardFactor(BigInteger num, List<BigInteger> factors)
    {
        BigInteger factor;
        while (true)
        {
            factor = FindFactorHard(num);
            if (factor.IsOne || factor.Equals(num))
            {
                factors.Add(num);
                return;
            }
            else
            {
                if (!IsPrime(factor))
                    HardFactor(factor, factors);
                else
                    factors.Add(factor);
            }

            num = num / factor;
        }
    }
}