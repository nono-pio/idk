

using System.Numerics;
using Cc.Redberry.Rings.Bigint;

namespace Cc.Redberry.Rings
{
    /// <summary>
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class ChineseRemainders
    {

        /// <summary>
        /// Runs Chinese Remainders algorithm
        /// </summary>
        /// <param name="prime1">#1 prime</param>
        /// <param name="prime2">#2 prime</param>
        /// <param name="remainder1">#1 remainder</param>
        /// <param name="remainder2">#2 remainder</param>
        /// <returns>the result</returns>
        public static long ChineseRemainders(long prime1, long prime2, long remainder1, long remainder2)
        {
            if (prime1 <= 0 || prime2 <= 0)
                throw new Exception("Negative CRT input: " + prime1 + " " + prime2);
            long modulus = MultiplyExact(prime1, prime2);
            long result;

            //(result + (prime2 * ((bezout0(prime2, prime1) * remainder1) % prime1)) % modulus) % modulus
            result = FloorMod(MultiplyExact(prime2, FloorMod(MultiplyExact(Bezout0(prime2, prime1), remainder1), prime1)), modulus);
            result = FloorMod(AddExact(result, FloorMod(MultiplyExact(prime1, FloorMod(MultiplyExact(Bezout0(prime1, prime2), remainder2), prime2)), modulus)), modulus);
            return result;
        }

        /// <summary>
        /// Runs Chinese Remainders algorithm
        /// </summary>
        /// <param name="prime1">#1 prime</param>
        /// <param name="prime2">#2 prime</param>
        /// <param name="remainder1">#1 remainder</param>
        /// <param name="remainder2">#2 remainder</param>
        /// <returns>the result</returns>
        public static BigInteger ChineseRemainders(BigInteger prime1, BigInteger prime2, BigInteger remainder1, BigInteger remainder2)
        {
            if (prime1.Signum() <= 0 || prime2.Signum() <= 0)
                throw new Exception("Negative CRT input: " + prime1 + " " + prime2);
            return ChineseRemainders(Rings.Z, prime1, prime2, remainder1, remainder2);
        }

        /// <summary>
        /// Runs Chinese Remainders algorithm
        /// </summary>
        /// <param name="prime1">#1 prime</param>
        /// <param name="prime2">#2 prime</param>
        /// <param name="remainder1">#1 remainder</param>
        /// <param name="remainder2">#2 remainder</param>
        /// <returns>the result</returns>
        public static E ChineseRemainders<E>(Ring<E> ring, E prime1, E prime2, E remainder1, E remainder2)
        {
            E modulus = ring.Multiply(prime1, prime2);
            E result = ring.GetZero();

            //(result + (prime2 * ((bezout0(prime2, prime1) * remainder1) % prime1)) ) % modulus
            result = ring.Remainder(ring.Add(result, ring.Multiply(prime2, ring.Remainder(ring.Multiply(Bezout0(ring, prime2, prime1), remainder1), prime1))), modulus);
            result = ring.Remainder(ring.Add(result, ring.Multiply(prime1, ring.Remainder(ring.Multiply(Bezout0(ring, prime1, prime2), remainder2), prime2))), modulus);
            return result;
        }

        /// <summary>
        /// Runs Chinese Remainders algorithm using the precomputed magic (speed's up computation when several invocations
        /// with the same {@code magic} performed)
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="magic">magic (create by {@link #createMagic(Ring, Object, Object)})</param>
        /// <param name="remainder1">#1 remainder</param>
        /// <param name="remainder2">#2 remainder</param>
        /// <returns>the result</returns>
        public static E ChineseRemainders<E>(Ring<E> ring, ChineseRemaindersMagic<E> magic, E remainder1, E remainder2)
        {
            E result = ring.GetZero();

            //(result + (prime2 * ((bezout0(prime2, prime1) * remainder1) % prime1)) ) % modulus
            result = ring.Remainder(ring.Add(result, ring.Multiply(magic.prime2, ring.Remainder(ring.Multiply(magic.bezout0_prime2_prime1, remainder1), magic.prime1))), magic.mulPrimes);
            result = ring.Remainder(ring.Add(result, ring.Multiply(magic.prime1, ring.Remainder(ring.Multiply(magic.bezout0_prime1_prime2, remainder2), magic.prime2))), magic.mulPrimes);
            return result;
        }

        /// <summary>
        /// Magic data to make CRT faster via precomputing Bezout coefficients
        /// </summary>
        public class ChineseRemaindersMagic<E>
        {
            public readonly E prime1;
            public readonly E prime2;
            public readonly E mulPrimes;
            public readonly E bezout0_prime2_prime1;
            public readonly E bezout0_prime1_prime2;

            public ChineseRemaindersMagic(Ring<E> ring, E prime1, E prime2)
            {
                this.prime1 = prime1;
                this.prime2 = prime2;
                this.mulPrimes = ring.Multiply(prime1, prime2);
                this.bezout0_prime1_prime2 = Bezout0(ring, prime1, prime2);
                this.bezout0_prime2_prime1 = Bezout0(ring, prime2, prime1);
            }
        }

        /// <summary>
        /// Magic for fast repeated Chinese Remainders
        /// </summary>
        public static ChineseRemaindersMagic<E> CreateMagic<E>(Ring<E> ring, E prime1, E prime2)
        {
            return new ChineseRemaindersMagic<E>(ring, prime1, prime2);
        }

        /// <summary>
        /// Runs Chinese Remainders algorithm using the precomputed magic (speed's up computation when several invocations
        /// with the same {@code magic} performed)
        /// </summary>
        /// <param name="magic">magic structure for fast modular arithmetic ({@link #createMagic(long, long)}</param>
        /// <param name="remainder1">#1 remainder</param>
        /// <param name="remainder2">#2 remainder</param>
        /// <returns>the result</returns>
        public static long ChineseRemainders(ChineseRemaindersMagicZp64 magic, long remainder1, long remainder2)
        {
            long result;

            //(result + (prime2 * ((bezout0(prime2, prime1) * remainder1) % prime1)) % modulus) % modulus
            result = magic.MulModModulus(magic.prime2, magic.MulModPrime1(magic.bezoutPrime2Prime1, remainder1));
            result = magic.AddMod(result, magic.MulModModulus(magic.prime1, magic.MulModPrime2(magic.bezoutPrime1Prime2, remainder2)));
            return result;
        }

        /// <summary>
        /// Magic for fast repeated Chinese Remainders
        /// </summary>
        public static ChineseRemaindersMagicZp64 CreateMagic(long prime1, long prime2)
        {
            long modulus = MultiplyExact(prime1, prime2);
            return new ChineseRemaindersMagicZp64(prime1, prime2, modulus, FastDivision.MagicUnsigned(prime1), FastDivision.MagicUnsigned(prime2), FastDivision.MagicUnsigned(modulus), prime1 <= Integer.MAX_VALUE, prime2 <= Integer.MAX_VALUE, modulus <= Integer.MAX_VALUE, Bezout0(prime1, prime2), Bezout0(prime2, prime1));
        }

        public class ChineseRemaindersMagicZp64
        {
            public readonly long prime1;
            public readonly long prime2;
            readonly long modulus;
            readonly FastDivision.Magic magic1, magic2, magicModulus;
            readonly bool prime1IsInt, prime2IsInt, modulusIsInt;
            readonly FastDivision.Magic magic32Prime1, magic32Prime2, magic32Modulus;
            public readonly long bezoutPrime1Prime2;
            public readonly long bezoutPrime2Prime1;

            public ChineseRemaindersMagicZp64(long prime1, long prime2, long modulus, FastDivision.Magic magic1, FastDivision.Magic magic2, FastDivision.Magic magicModulus, bool prime1IsInt, bool prime2IsInt, bool modulusIsInt, long bezoutPrime1Prime2, long bezoutPrime2Prime1)
            {
                this.prime1 = prime1;
                this.prime2 = prime2;
                this.modulus = modulus;
                this.magic1 = magic1;
                this.magic2 = magic2;
                this.magicModulus = magicModulus;
                this.prime1IsInt = prime1IsInt;
                this.prime2IsInt = prime2IsInt;
                this.modulusIsInt = modulusIsInt;
                this.magic32Prime1 = prime1IsInt ? null : FastDivision.Magic32ForMultiplyMod(prime1);
                this.magic32Prime2 = prime2IsInt ? null : FastDivision.Magic32ForMultiplyMod(prime2);
                this.magic32Modulus = modulusIsInt ? null : FastDivision.Magic32ForMultiplyMod(modulus);
                this.bezoutPrime1Prime2 = Math.FloorMod(bezoutPrime1Prime2, prime2);
                this.bezoutPrime2Prime1 = Math.FloorMod(bezoutPrime2Prime1, prime1);
            }

            public virtual long MulModPrime1(long a, long b)
            {
                return prime1IsInt ? FastDivision.ModUnsignedFast(a * b, magic1) : FastDivision.MultiplyMod128Unsigned(a, b, prime1, magic32Prime1);
            }

            public virtual long MulModPrime2(long a, long b)
            {
                return prime2IsInt ? FastDivision.ModUnsignedFast(a * b, magic2) : FastDivision.MultiplyMod128Unsigned(a, b, prime2, magic32Prime2);
            }

            public virtual long MulModModulus(long a, long b)
            {
                return modulusIsInt ? FastDivision.ModUnsignedFast(a * b, magicModulus) : FastDivision.MultiplyMod128Unsigned(a, b, modulus, magic32Modulus);
            }

            public virtual long AddMod(long a, long b)
            {
                long r = a + b;
                return r - modulus >= 0 ? r - modulus : r;
            }
        }

        /// <summary>
        /// Runs Chinese Remainders algorithm
        /// </summary>
        /// <param name="primes">list of coprime numbers</param>
        /// <param name="remainders">remainder</param>
        /// <returns>the result</returns>
        public static long ChineseRemainders(long[] primes, long[] remainders)
        {
            if (primes.Length != remainders.Length)
                throw new ArgumentException();
            long modulus = primes[0];
            for (int i = 1; i < primes.Length; ++i)
            {
                if (primes[i] <= 0)
                    throw new Exception("Negative CRT input: " + primes[i]);
                modulus = MultiplyExact(primes[i], modulus);
            }

            long result = 0;
            for (int i = 0; i < primes.Length; ++i)
            {
                long iModulus = modulus / primes[i];
                long bezout = Bezout0(iModulus, primes[i]);
                result = FloorMod(AddExact(result, FloorMod(MultiplyExact(iModulus, FloorMod(MultiplyExact(bezout, remainders[i]), primes[i])), modulus)), modulus);
            }

            return result;
        }

        /// <summary>
        /// Runs Chinese Remainders algorithm
        /// </summary>
        /// <param name="primes">list of coprime numbers</param>
        /// <param name="remainders">remainder</param>
        /// <returns>the result</returns>
        public static BigInteger ChineseRemainders(BigInteger[] primes, BigInteger[] remainders)
        {
            if (primes.Length != remainders.Length)
                throw new ArgumentException();
            BigInteger modulus = primes[0];
            for (int i = 1; i < primes.Length; i++)
            {
                if (primes[i].Signum() <= 0)
                    throw new Exception("Negative CRT input: " + primes[i]);
                modulus = primes[i].Multiply(modulus);
            }

            BigInteger result = BigInteger.Zero;
            for (int i = 0; i < primes.Length; i++)
            {
                BigInteger iModulus = modulus.Divide(primes[i]);
                BigInteger bezout = Bezout0(Rings.Z, iModulus, primes[i]);
                result = result.Add(iModulus.Multiply(bezout.Multiply(remainders[i]).Mod(primes[i]))).Mod(modulus);
            }

            return result;
        }

        /// <summary>
        /// Runs Chinese Remainders algorithm
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="primes">primes</param>
        /// <param name="remainders">remainders</param>
        /// <returns>the result</returns>
        public static E ChineseRemainders<E>(Ring<E> ring, E[] primes, E[] remainders)
        {
            if (primes.Length != remainders.Length)
                throw new ArgumentException();
            E modulus = primes[0];
            for (int i = 1; i < primes.Length; i++)
                modulus = ring.Multiply(primes[i], modulus);
            E result = ring.GetZero();
            for (int i = 0; i < primes.Length; i++)
            {
                E iModulus = ring.DivideExact(modulus, primes[i]);
                E bezout = Bezout0(ring, iModulus, primes[i]);
                result = ring.Remainder(ring.Add(result, ring.Remainder(ring.Multiply(iModulus, ring.Remainder(ring.Multiply(bezout, remainders[i]), primes[i])), modulus)), modulus);
            }

            return result;
        }

        private static long Bezout0(long a, long b)
        {
            long s = 0, old_s = 1;
            long r = b, old_r = a;
            long q;
            long tmp;
            while (r != 0)
            {
                q = old_r / r;
                tmp = old_r;
                old_r = r;
                r = SubtractExact(tmp, MultiplyExact(q, r));
                tmp = old_s;
                old_s = s;
                s = SubtractExact(tmp, MultiplyExact(q, s));
            }

            return old_s;
        }

        private static E Bezout0<E>(Ring<E> ring, E a, E b)
        {
            E[] rs = ring.FirstBezoutCoefficient(a, b);
            E r = rs[0], s = rs[1];
            if (!ring.IsOne(r))
                s = ring.DivideExact(s, r);
            return s;
        }
    }
}