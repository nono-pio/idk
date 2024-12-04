using System.Numerics;

namespace Rings;

public sealed class ChineseRemainders
{
    private ChineseRemainders()
    {
    }

    public static long ChineseRemainders(long prime1, long prime2,
        long remainder1, long remainder2)
    {
        if (prime1 <= 0 || prime2 <= 0)
            throw new ArgumentException("Negative CRT input: " + prime1 + " " + prime2);

        long modulus = multiplyExact(prime1, prime2);
        long result;

        //(result + (prime2 * ((bezout0(prime2, prime1) * remainder1) % prime1)) % modulus) % modulus
        result = floorMod(multiplyExact(prime2,
            floorMod(multiplyExact(bezout0(prime2, prime1), remainder1), prime1)), modulus);
        result = floorMod(addExact(result,
            floorMod(multiplyExact(prime1,
                floorMod(multiplyExact(bezout0(prime1, prime2), remainder2), prime2)), modulus)), modulus);

        return result;
    }

    public static BigInteger ChineseRemainders(BigInteger prime1, BigInteger prime2,
        BigInteger remainder1, BigInteger remainder2)
    {
        if (prime1.Sign <= 0 || prime2.Sign <= 0)
            throw new ArgumentException("Negative CRT input: " + prime1 + " " + prime2);

        return ChineseRemainders(Rings.Z, prime1, prime2, remainder1, remainder2);
    }

    public static E ChineseRemainders<E>(Ring<E> ring,
        E prime1, E prime2,
        E remainder1, E remainder2)
    {
        E modulus = ring.multiply(prime1, prime2);
        E result = ring.getZero();

        //(result + (prime2 * ((bezout0(prime2, prime1) * remainder1) % prime1)) ) % modulus
        result = ring.remainder(
            ring.add(result,
                ring.multiply(prime2,
                    ring.remainder(ring.multiply(bezout0(ring, prime2, prime1), remainder1), prime1))), modulus);
        result = ring.remainder(
            ring.add(result,
                ring.multiply(prime1,
                    ring.remainder(ring.multiply(bezout0(ring, prime1, prime2), remainder2), prime2))), modulus);

        return result;
    }

    public static E ChineseRemainders<E>(Ring<E> ring, ChineseRemaindersMagic<E> magic, E remainder1, E remainder2)
    {
        E result = ring.getZero();

        //(result + (prime2 * ((bezout0(prime2, prime1) * remainder1) % prime1)) ) % modulus
        result = ring.remainder(
            ring.add(result,
                ring.multiply(magic.prime2,
                    ring.remainder(ring.multiply(magic.bezout0_prime2_prime1, remainder1), magic.prime1))),
            magic.mulPrimes);
        result = ring.remainder(
            ring.add(result,
                ring.multiply(magic.prime1,
                    ring.remainder(ring.multiply(magic.bezout0_prime1_prime2, remainder2), magic.prime2))),
            magic.mulPrimes);

        return result;
    }

    public sealed class ChineseRemaindersMagic<E>
    {
        readonly E prime1, prime2, mulPrimes;
        readonly E bezout0_prime2_prime1, bezout0_prime1_prime2;

        public ChineseRemaindersMagic(Ring<E> ring, E prime1, E prime2)
        {
            this.prime1 = prime1;
            this.prime2 = prime2;
            this.mulPrimes = ring.multiply(prime1, prime2);
            this.bezout0_prime1_prime2 = bezout0(ring, prime1, prime2);
            this.bezout0_prime2_prime1 = bezout0(ring, prime2, prime1);
        }
    }

    public static ChineseRemaindersMagic<E> createMagic<E>(Ring<E> ring, E prime1, E prime2)
    {
        return new(ring, prime1, prime2);
    }

    public static long ChineseRemainders(ChineseRemaindersMagicZp64 magic,
        long remainder1, long remainder2)
    {
        long result;

        //(result + (prime2 * ((bezout0(prime2, prime1) * remainder1) % prime1)) % modulus) % modulus
        result = magic.mulModModulus(magic.prime2, magic.mulModPrime1(magic.bezoutPrime2Prime1, remainder1));
        result = magic.addMod(result,
            magic.mulModModulus(magic.prime1, magic.mulModPrime2(magic.bezoutPrime1Prime2, remainder2)));

        return result;
    }

    public static ChineseRemaindersMagicZp64 createMagic(long prime1, long prime2)
    {
        long modulus = multiplyExact(prime1, prime2);
        return new ChineseRemaindersMagicZp64(
            prime1, prime2, modulus,
            FastDivision.magicUnsigned(prime1), FastDivision.magicUnsigned(prime2), FastDivision.magicUnsigned(modulus),
            prime1 <= int.MaxValue, prime2 <= int.MaxValue, modulus <= int.MaxValue,
            bezout0(prime1, prime2), bezout0(prime2, prime1)
        );
    }

    public sealed class ChineseRemaindersMagicZp64
    {
        readonly long prime1, prime2, modulus;
        readonly FastDivision.Magic magic1, magic2, magicModulus;
        readonly bool prime1IsInt, prime2IsInt, modulusIsInt;
        readonly FastDivision.Magic magic32Prime1, magic32Prime2, magic32Modulus;
        readonly long bezoutPrime1Prime2, bezoutPrime2Prime1;

        public ChineseRemaindersMagicZp64(long prime1, long prime2, long modulus, FastDivision.Magic magic1,
            FastDivision.Magic magic2, FastDivision.Magic magicModulus, bool prime1IsInt, bool prime2IsInt,
            bool modulusIsInt, long bezoutPrime1Prime2, long bezoutPrime2Prime1)
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
            this.magic32Prime1 = prime1IsInt ? null : FastDivision.magic32ForMultiplyMod(prime1);
            this.magic32Prime2 = prime2IsInt ? null : FastDivision.magic32ForMultiplyMod(prime2);
            this.magic32Modulus = modulusIsInt ? null : FastDivision.magic32ForMultiplyMod(modulus);
            this.bezoutPrime1Prime2 = Math.floorMod(bezoutPrime1Prime2, prime2);
            this.bezoutPrime2Prime1 = Math.floorMod(bezoutPrime2Prime1, prime1);
        }

        public long mulModPrime1(long a, long b)
        {
            return prime1IsInt
                ? FastDivision.modUnsignedFast(a * b, magic1)
                : FastDivision.multiplyMod128Unsigned(a, b, prime1, magic32Prime1);
        }

        public long mulModPrime2(long a, long b)
        {
            return prime2IsInt
                ? FastDivision.modUnsignedFast(a * b, magic2)
                : FastDivision.multiplyMod128Unsigned(a, b, prime2, magic32Prime2);
        }

        public long mulModModulus(long a, long b)
        {
            return modulusIsInt
                ? FastDivision.modUnsignedFast(a * b, magicModulus)
                : FastDivision.multiplyMod128Unsigned(a, b, modulus, magic32Modulus);
        }

        public long addMod(long a, long b)
        {
            long r = a + b;
            return r - modulus >= 0 ? r - modulus : r;
        }
    }


    public static long ChineseRemainders(long[] primes, long[] remainders)
    {
        if (primes.Length != remainders.Length)
            throw new ArgumentException();

        long modulus = primes[0];
        for (int i = 1; i < primes.Length; ++i)
        {
            if (primes[i] <= 0)
                throw new ArgumentException("Negative CRT input: " + primes[i]);
            modulus = multiplyExact(primes[i], modulus);
        }

        long result = 0;
        for (int i = 0; i < primes.Length; ++i)
        {
            long iModulus = modulus / primes[i];
            long bezout = bezout0(iModulus, primes[i]);
            result = floorMod(addExact(result,
                floorMod(multiplyExact(iModulus,
                    floorMod(multiplyExact(bezout, remainders[i]), primes[i])), modulus)), modulus);
        }

        return result;
    }

    public static BigInteger ChineseRemainders(BigInteger[] primes, BigInteger[] remainders)
    {
        if (primes.Length != remainders.Length)
            throw new ArgumentException();
        BigInteger modulus = primes[0];
        for (int i = 1; i < primes.Length; i++)
        {
            if (primes[i].Sign <= 0)
                throw new ArgumentException("Negative CRT input: " + primes[i]);
            modulus = primes[i] * modulus;
        }

        BigInteger result = BigInteger.Zero;
        for (int i = 0; i < primes.Length; i++)
        {
            BigInteger iModulus = modulus / primes[i];
            BigInteger bezout = bezout0(Rings.Z, iModulus, primes[i]);
            result = (result + iModulus * (bezout * remainders[i] % primes[i])) % modulus;
        }

        return result;
    }

    public static E ChineseRemainders<E>(Ring<E> ring,
        E[] primes,
        E[] remainders)
    {
        if (primes.Length != remainders.Length)
            throw new ArgumentException();
        E modulus = primes[0];
        for (int i = 1; i < primes.Length; i++)
            modulus = ring.multiply(primes[i], modulus);

        E result = ring.getZero();
        for (int i = 0; i < primes.Length; i++)
        {
            E iModulus = ring.divideExact(modulus, primes[i]);
            E bezout = bezout0(ring, iModulus, primes[i]);
            result = ring.remainder(ring.add(result,
                ring.remainder(ring.multiply(iModulus, ring.remainder(ring.multiply(bezout, remainders[i]), primes[i])),
                    modulus)), modulus);
        }

        return result;
    }

    private static long bezout0(long a, long b)
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
            r = subtractExact(tmp, multiplyExact(q, r));

            tmp = old_s;
            old_s = s;
            s = subtractExact(tmp, multiplyExact(q, s));
        }

        // TODO : assert old_r == 1 : "a = " + a + " b = " + b;
        return old_s;
    }

    private static E bezout0<E>(Ring<E> ring, E a, E b)
    {
        var (r, s) = ring.firstBezoutCoefficient(a, b);
        // TODO assert ring.isUnit(r) : r;
        if (!ring.isOne(r))
            s = ring.divideExact(s, r);
        return s;
    }
}