

using System.Numerics;
using Cc.Redberry.Rings.Bigint;

namespace Cc.Redberry.Rings.Primes
{
    /// <summary>
    /// Prime factorization of BigIntegers
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class BigPrimes
    {
        private static readonly BigInteger MAX_INT = new BigInteger(int.MaxValue);
        private static readonly Random privateRandom = new Random(0x1a9e2b8f);
        private BigPrimes()
        {
        }

        /// <summary>
        /// Strong primality test. Switches between trial divisions, probabilistic Miller-Rabin (ensures that is not prime),
        /// probabilistic Lucas test (ensures that is prime) and finally (if all above fail to provide deterministic answer)
        /// to Pollard's p-1, Pollard's rho and quadratic sieve.
        /// </summary>
        /// <param name="n">number to test</param>
        /// <returns>{@code true} if input is certainly prime, {@code false} is certainly composite</returns>
        public static bool IsPrime(long n)
        {
            if (n < int.MaxValue)
                return SmallPrimes.IsPrime((int)n);
            return IsPrime(new BigInteger(n));
        }

        /// <summary>
        /// Strong primality test. Switches between trial divisions, probabilistic Miller-Rabin (ensures that is not prime),
        /// probabilistic Lucas test (ensures that is prime) and finally (if all above fail to provide deterministic answer)
        /// to Pollard's p-1, Pollard's rho and quadratic sieve.
        /// </summary>
        /// <param name="n">number to test</param>
        /// <returns>{@code true} if input is certainly prime, {@code false} is certainly composite</returns>
        public static bool IsPrime(BigInteger n)
        {
            if (n.Signum() < 0)
                throw new ArgumentException("Argument must be positive");

            //small known primes
            if (n.CompareTo(SieveOfAtkin.SmallPrimesSieve.GetLimitAsBigInteger()) < 0)
                return SieveOfAtkin.SmallPrimesSieve.IsPrime(n.IntValue());

            //switch to deterministic Miller-Rabin
            if (n.CompareTo(MAX_INT) < 0)
                return SmallPrimes.IsPrime(n.IntValue());

            //ok <- some trial divisions
            foreach (int p in SmallPrimes.SmallPrimes12)
                if (n.Mod(new BigInteger(p)).IsZero)
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
            int bound = n.CompareTo(MAX_INT) > 0 ? int.MaxValue : n.IntValue();
            BigInteger nMinusOne = n.Decrement();
            IList<BigInteger> factors = null;
            loop1:
                while (k-- > 0)
                {
                    BigInteger a = new BigInteger(7 + rnd.Next(bound - 7));
                    if (!a.ModPow(nMinusOne, n).IsOne)
                        return false;
                    if (factors == null)
                        factors = PrimeFactors(nMinusOne);
                    foreach (BigInteger q in factors)
                        if (a.ModPow(nMinusOne.Divide(q), n).IsOne)
                            continue;
                    return true;
                }

            return false;
        }

        /// <summary>
        /// Return the smallest prime greater than or equal to n.
        /// </summary>
        /// <param name="n">a positive number.</param>
        /// <returns>the smallest prime greater than or equal to n.</returns>
        /// <exception cref="IllegalArgumentException">if n &lt; 0.</exception>
        public static BigInteger NextPrime(BigInteger n)
        {
            while (!IsPrime(n))
                n = n.NextProbablePrime();
            return n;
        }

        /// <summary>
        /// Return the smallest prime greater than or equal to n.
        /// </summary>
        /// <param name="n">a positive number.</param>
        /// <returns>the smallest prime greater than or equal to n.</returns>
        /// <exception cref="IllegalArgumentException">if n &lt; 0.</exception>
        public static long NextPrime(long n)
        {
            BigInteger nb = new BigInteger(n);
            while (!IsPrime(nb))
                nb = nb.NextProbablePrime();
            return nb.LongValueExact();
        }

        /// <summary>
        /// Fermat's factoring algorithm works like trial division, but walks in the opposite direction. Thus, it can be used
        /// to factor a number that we know has a factor in the interval [Sqrt(n) - upperBound, Sqrt(n) + upperBound].
        /// </summary>
        /// <param name="n">number to factor</param>
        /// <param name="upperBound">upper bound</param>
        /// <returns>a single factor</returns>
        public static BigInteger Fermat(BigInteger n, long upperBound)
        {
            long cnt = 0;
            BigInteger x = Primes.QuadraticSieve.SqrtBigInt(n).Add(BigInteger.One);
            BigInteger u = x.Multiply(2).Add(BigInteger.One);
            BigInteger v = BigInteger.One;
            BigInteger r = x.Multiply(x).Subtract(n);
            while (!r.IsZero)
            {
                cnt++;
                if (cnt > upperBound)
                    return BigInteger.Zero;
                while (r.CompareTo(BigInteger.Zero) > 0)
                {
                    r = r.Subtract(v);
                    v = v.Add(2);
                }

                if (r.CompareTo(BigInteger.Zero) < 0)
                {
                    r = r.Add(u);
                    u = u.Add(2);
                }
            }

            return u.Subtract(v).Divide(2);
        }

        /// <summary>
        /// Pollards's rho algorithm (random search version).
        /// </summary>
        /// <param name="n">integer to factor</param>
        /// <param name="attempts">number of random attempts</param>
        /// <returns>a single factor of {@code n} or null if no factors found</returns>
        public static BigInteger? PollardRho(BigInteger n, int attempts, Random rn)
        {

            // check divisibility by 2
            if (n.Mod(2).IsZero)
                return 2;
            BigInteger divisor;
            BigInteger c = new BigInteger(n.BitLength(), rn);
            BigInteger x = new BigInteger(n.BitLength(), rn);
            BigInteger xx = x;
            do
            {
                x = x.Multiply(x).Mod(n).Add(c).Mod(n);
                xx = xx.Multiply(xx).Mod(n).Add(c).Mod(n);
                xx = xx.Multiply(xx).Mod(n).Add(c).Mod(n);
                divisor = x.Subtract(xx).Gcd(n);
            }
            while (attempts-- > 0 && divisor.IsOne);
            return divisor.IsOne ? null : divisor;
        }

        /// <summary>
        /// Pollards's rho algorithm.
        /// </summary>
        /// <param name="n">integer to factor</param>
        /// <param name="upperBound">expected B-smoothness</param>
        /// <returns>a single factor of {@code n} or null if no factors found</returns>
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
                    x2 = x2.Multiply(x2).Add(BigInteger.One).Mod(n);
                    product = product.Multiply(x1.Subtract(x2)).Mod(n);
                    if (terms++ > upperBound)
                        break;
                    if (terms % 5 == 0)
                    {
                        BigInteger g = n.Gcd(product);
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
                    x2 = x2.Multiply(x2).Add(BigInteger.One).Mod(n);
            }

            return null;
        }

        /// <summary>
        /// Pollards's p-1 algorithm.
        /// </summary>
        /// <param name="n">integer to factor</param>
        /// <param name="upperBound">expected B-smoothness</param>
        /// <returns>a single factor of {@code n} or null if no factors found</returns>
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
                    i = i.Add(BigInteger.One);
                    m = m.ModPow(i, n);
                    if (cnt % 5 == 0)
                    {
                        g = n.Gcd(m.Subtract(BigInteger.One));
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
            return new QuadraticSieve(n).QuadraticSieve(bound);
        }

        static BigInteger FindFactorHard(BigInteger n)
        {
            int numBits = n.BitCount();
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
            return b.CompareTo(SieveOfAtkin.SmallPrimesSieve.GetLimitAsBigInteger()) < 0 && SieveOfAtkin.SmallPrimesSieve.IsPrime(b.IntValue());
        }

        /// <summary>
        /// Prime factors decomposition. The algorithm switches between trial divisions, Pollard's p-1, Pollard's rho and
        /// quadratic sieve.
        /// </summary>
        /// <param name="num">number to factorize</param>
        /// <returns>list of prime factors of n</returns>
        /// <exception cref="IllegalArgumentException">if n is negative</exception>
        public static long[] PrimeFactors(long num)
        {
            return PrimeFactors(new BigInteger(num)).Select(p => p.LongValueExact()).ToArray();
        }

        /// <summary>
        /// Prime factors decomposition. The algorithm switches between trial divisions, Pollard's p-1, Pollard's rho and
        /// quadratic sieve.
        /// </summary>
        /// <param name="num">number to factorize</param>
        /// <returns>list of prime factors of n</returns>
        /// <exception cref="IllegalArgumentException">if n is negative</exception>
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
                BigInteger[] qr = num.DivideAndRemainder(prime);
                while (qr[1].IsZero)
                {
                    num = qr[0];
                    factors.Add(prime);
                    qr = num.DivideAndRemainder(prime);
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

                num = num.Divide(factor);
            }
        }
    }
}