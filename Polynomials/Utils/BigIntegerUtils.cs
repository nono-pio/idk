using System.Diagnostics;
using System.Numerics;
using Polynomials.Primes;

namespace Polynomials.Utils;

public static class BigIntegerUtils
{
    public static BigInteger Pow(BigInteger @base, long exp)
    {
        if (exp < 0)
            throw new Exception();
        if (exp == 1)
            return @base;
        var result = BigInteger.One;
        var k2p = @base; // <= copy the base (mutable operations are used below)
        for (;;)
        {
            if ((exp & 1) == 1)
                result *= k2p;
            exp >>= 1;
            if (exp == 0)
                return result;
            k2p *= k2p;
        }
    }

    public static BigInteger Pow(BigInteger @base, BigInteger exp)
    {
        if (exp.Sign < 0)
            throw new Exception();
        if (exp.IsOne)
            return @base;
        var result = BigInteger.One;
        var k2p = @base; // <= copy the base (mutable operations are used below)
        for (;;)
        {
            if ((exp.IsEven))
                result *= k2p;
            exp >>= 1;
            if (exp.IsZero)
                return result;
            k2p *= k2p;
        }
    }

    public static BigInteger[]? PerfectPowerDecomposition(BigInteger n)
    {
        if (n.Sign < 0)
        {
            n = -n;
            var ipp = PerfectPowerDecomposition(n);
            if (ipp == null)
                return null;
            if (ipp[1].IsEven)
                return null;
            ipp[0] = -ipp[0];
            return ipp;
        }

        if (n.GetBitLength() == 1)
            return new BigInteger[]
            {
                2,
                new BigInteger(n.GetBitLength() - 1)
            };
        var lgn = 1 + (int)n.GetBitLength();
        for (var b = 2; b < lgn; b++)
        {
            //b lg a = lg n
            var lowa = BigInteger.One;
            var higha = BigInteger.One << (lgn / b + 1);
            while (lowa.CompareTo(higha - 1) < 0)
            {
                var mida = (lowa +higha) >> 1;
                var ab = Pow(mida, b);
                if (ab.CompareTo(n) > 0)
                    higha = mida;
                else if (ab.CompareTo(n) < 0)
                    lowa = mida;
                else
                {
                    var ipp = PerfectPowerDecomposition(mida);
                    if (ipp != null)
                        return new BigInteger[]
                        {
                            ipp[0],
                            ipp[1] * b
                        };
                    else
                        return new BigInteger[]
                        {
                            mida,
                            new BigInteger(b)
                        };
                }
            }
        }

        return null;
    }
    
    public static BigInteger RandomBigIntMinMax(BigInteger min, BigInteger max, Random rnd)
    {
        if (min.CompareTo(max) > 0)
            throw new Exception();
        var delta = max - min;
        return min + RandomBigIntBound(delta, rnd);
    }
    
    public static BigInteger RandomBigIntBound(BigInteger bound, Random rnd)
    {
        BigInteger r;
        do
        {
            r = RandomBigInt((int) bound.GetBitLength(), rnd);
        }
        while (r.CompareTo(bound) >= 0);
        return r;
    }

    public static BigInteger RandomBigInt(int bitLength, Random rnd)
    {

        // Calculer le nombre de bytes nécessaires
        int byteCount = (bitLength + 7) / 8;

        // Générer les bytes aléatoires
        byte[] bytes = new byte[byteCount];
        rnd.NextBytes(bytes);

        // Masquer les bits inutiles dans le byte de poids fort
        int extraBits = (8 * byteCount) - bitLength;
        bytes[bytes.Length - 1] &= (byte)(0xFF >> extraBits);

        // Convertir en BigInteger
        return new BigInteger(bytes, isUnsigned: true);
    }

    /* Extension */
    public static readonly int SMALL_PRIME_THRESHOLD = 95;
    public static readonly int DEFAULT_PRIME_CERTAINTY = 100;
    private static readonly BigInteger SMALL_PRIME_PRODUCT = new(3L * 5 * 7 * 11 * 13 * 17 * 19 * 23 * 29 * 31 * 37 * 41);
    
    public static bool IsLong(this BigInteger value) => value >= long.MinValue && value <= long.MaxValue;
    public static bool IsInt(this BigInteger value) => value >= int.MinValue && value <= int.MaxValue;

    public static bool IsProbablePrime(this BigInteger value, int certainty)
    {
        if (certainty <= 0)
            return true;
        BigInteger w = BigInteger.Abs(value);
        if (w == 2)
            return true;
        if (w.IsEven || w == 1)
            return false;

        return w.PrimeToCertainty(certainty, null);
    }

    public static uint[] Mag(this BigInteger value) => ConvertByteArrayToUIntArray(value.ToByteArray(isUnsigned: true));
    public static uint[] ConvertByteArrayToUIntArray(byte[] bytes)
    {
        int length = (bytes.Length + 3) / 4 * 4;
        Array.Resize(ref bytes, length);
        uint[] uints = new uint[length / 4];
        for (int i = 0; i < uints.Length; i++)
        {
            uints[i] = BitConverter.ToUInt32(bytes, i * 4);
        }

        return uints;
    }


    public static BigInteger NextProbablePrime(this BigInteger value) {
        if (value.Sign < 0)
            throw new ArithmeticException("start < 0: " + value);

        // Handle trivial cases
        if ((value.Sign == 0) || value.Equals(1))
            return 2;

        BigInteger result = value + 1;

        // Fastpath for small numbers
        if (result.GetBitLength() < SMALL_PRIME_THRESHOLD) {

            // Ensure an odd number
            if (result.IsEven)
                result = result + 1;

            while (true) {
                // Do cheap "pre-test" if applicable
                if (result.GetBitLength() > 6) {
                    long r = (long)(result % SMALL_PRIME_PRODUCT);
                    if ((r % 3 == 0) || (r % 5 == 0) || (r % 7 == 0) || (r % 11 == 0) ||
                            (r % 13 == 0) || (r % 17 == 0) || (r % 19 == 0) || (r % 23 == 0) ||
                            (r % 29 == 0) || (r % 31 == 0) || (r % 37 == 0) || (r % 41 == 0)) {
                        result = result + 2;
                        continue; // Candidate is composite; try another
                    }
                }

                // All candidates of bitLength 2 and 3 are prime by this point
                if (result.GetBitLength() < 4)
                    return result;

                // The expensive test
                if (result.PrimeToCertainty(DEFAULT_PRIME_CERTAINTY, null))
                    return result;

                result = result + 2;
            }
        }

        // Start at previous even number
        if (result.IsEven)
            result = result - 1;

        // Looking for the next large prime
        int searchLen = GetPrimeSearchLen((int)result.GetBitLength());

        while (true) {
            BitSieve searchSieve = new BitSieve(result, searchLen);
            var candidate = searchSieve.Retrieve(result, DEFAULT_PRIME_CERTAINTY, null);
            if (candidate is not null)
                return candidate.Value;
            result = result + new BigInteger(2 * searchLen);
        }
    }
    
    private static readonly int PRIME_SEARCH_BIT_LENGTH_LIMIT = 500000000;
    private static int GetPrimeSearchLen(int bitLength) {
        if (bitLength > PRIME_SEARCH_BIT_LENGTH_LIMIT + 1) {
            throw new ArithmeticException("Prime search implementation restriction on bitLength");
        }
        return bitLength / 20 * 64;
    }
    
    public static bool PrimeToCertainty(this BigInteger value, int certainty, Random? random) {
        int rounds = 0;
        int n = (Math.Min(certainty, int.MaxValue - 1) + 1) / 2;

        // The relationship between the certainty and the number of rounds
        // we perform is given in the draft standard ANSI X9.80, "PRIME
        // NUMBER GENERATION, PRIMALITY TESTING, AND PRIMALITY CERTIFICATES".
        int sizeInBits = (int)value.GetBitLength();
        if (sizeInBits < 100) {
            rounds = 50;
            rounds = n < rounds ? n : rounds;
            return PassesMillerRabin(value, rounds, random);
        }

        if (sizeInBits < 256) {
            rounds = 27;
        } else if (sizeInBits < 512) {
            rounds = 15;
        } else if (sizeInBits < 768) {
            rounds = 8;
        } else if (sizeInBits < 1024) {
            rounds = 4;
        } else {
            rounds = 2;
        }
        rounds = n < rounds ? n : rounds;

        return PassesMillerRabin(value, rounds, random) && PassesLucasLehmer(value);
    }
    
    private static bool PassesMillerRabin(BigInteger value, int iterations, Random? rnd) {
        // Find a and m such that m is odd and this == 1 + 2**a * m
        BigInteger thisMinusOne = value - 1;
        BigInteger m = thisMinusOne;
        int a = GetLowestSetBit(m);
        m = m >> a;

        // Do the tests
        if (rnd == null) {
            rnd = new Random();
        }
        for (int i = 0; i < iterations; i++) {
            // Generate a uniform random on (1, this)
            BigInteger b;
            do {
                b = RandomBigInt((int)value.GetBitLength(), rnd);
            } while (b.CompareTo(1) <= 0 || b.CompareTo(value) >= 0);

            int j = 0;
            BigInteger z = BigInteger.ModPow(b, m, value);
            while (!((j == 0 && z == 1) || z == thisMinusOne)) {
                if (j > 0 && z == 1 || ++j == a)
                    return false;
                z = BigInteger.ModPow(z, 2, value);
            }
        }
        return true;
    }
    
    public static int GetLowestSetBit(BigInteger value)
    {

        if (value.IsZero)
            return -1;
        
        int lsb = 0;
        
        // Search for lowest order nonzero int
        var mag = value.Mag();
        int i;
        uint b;
        for (i = 0; (b = mag[mag.Length - i - 1] ) == 0; i++)
        {
        }

        lsb += (i << 5) + (int)uint.TrailingZeroCount(b);
        
        return lsb;
    }
    
    private static bool PassesLucasLehmer(BigInteger value) {
        BigInteger thisPlusOne = value + 1;

        // Step 1
        int d = 5;
        while (JacobiSymbol(d, value) != -1) {
            // 5, -7, 9, -11, ...
            d = (d < 0) ? Math.Abs(d) + 2 : -(d + 2);
        }

        // Step 2
        BigInteger u = LucasLehmerSequence(d, thisPlusOne, value);

        // Step 3
        return (u % value) == 0;
    }
    
    private static int JacobiSymbol(int p, BigInteger n) {
        if (p == 0)
            return 0;

        // Algorithm and comments adapted from Colin Plumb's C library.
        int j = 1;
        var mag = n.Mag();
        int u = unchecked((int)mag[mag.Length - 1]);

        // Make p positive
        if (p < 0) {
            p = -p;
            int n8 = u&7;
            if ((n8 == 3) || (n8 == 7))
                j = -j; // 3 (011) or 7 (111) mod 8
        }

        // Get rid of factors of 2 in p
        while ((p&3) == 0)
            p >>= 2;
        if ((p&1) == 0) {
            p >>= 1;
            if (((u^(u >> 1))&2) != 0)
                j = -j; // 3 (011) or 5 (101) mod 8
        }
        if (p == 1)
            return j;
        // Then, apply quadratic reciprocity
        if ((p&u&2) != 0)   // p = u = 3 (mod 4)?
            j = -j;
        // And reduce u mod p
        u = (int)(n %p);

        // Now compute Jacobi(u,p), u < p
        while (u != 0) {
            while ((u&3) == 0)
                u >>= 2;
            if ((u&1) == 0) {
                u >>= 1;
                if (((p^(p >> 1))&2) != 0)
                    j = -j;     // 3 (011) or 5 (101) mod 8
            }
            if (u == 1)
                return j;
            // Now both u and p are odd, so use quadratic reciprocity
            Debug.Assert(u < p);
            int t = u;
            u = p;
            p = t;
            if ((u&p&2) != 0) // u = p = 3 (mod 4)?
                j = -j;
            // Now u >= p, so it can be reduced
            u %= p;
        }
        return 0;
    }
    
    private static BigInteger LucasLehmerSequence(int z, BigInteger k, BigInteger n) {
        BigInteger d = new BigInteger(z);
        BigInteger u = 1;
        BigInteger u2;
        BigInteger v = 1;
        BigInteger v2;

        for (int i = (int)k.GetBitLength() - 2; i >= 0; i--) {
            u2 = u * v % n;

            v2 = (v * v + d * u * u) % n;
            if (v2.IsEven)
                v2 = v2 - n;

            v2 = v2 >> 1;

            u = u2;
            v = v2;
            if ((k >> i).IsEven) {
                u2 = (u+v)%n;
                if (u2.IsEven)
                    u2 = u2 - n;

                u2 = u2>>1;
                v2 = (v + d * u) % n;
                if (v2.IsEven)
                    v2 = v2 - n;
                v2 = v2>>1;

                u = u2;
                v = v2;
            }
        }
        return u;
    }
    
    public static BigInteger SqrtCeil(BigInteger val)
    {
        if (val.Sign < 0)
            throw new ArgumentException("Negative argument.");
        if (val.IsZero || val.IsOne)
            return val;
        BigInteger y;

        // starting with y = x / 2 avoids magnitude issues with x squared
        for (y = val>>1; y.CompareTo(val / y) > 0; y = ((val / y) + y)>>1)
        {
        }

        if (val.CompareTo(y * y) == 0)
            return y;
        else
            return y + 1;
    }
    
    public static BigInteger ModInverse(BigInteger value, BigInteger mod)
    {
        if (mod <= 0)
            throw new ArgumentException("Le modulo doit être strictement positif.", nameof(mod));

        BigInteger originalMod = mod;
        BigInteger x0 = 0, x1 = 1;

        while (value > 1)
        {
            if (mod == 0)
                throw new ArithmeticException("L'inverse modulaire n'existe pas.");

            // Quotient
            BigInteger q = value / mod;

            // Reste
            BigInteger temp = mod;
            mod = value % mod;
            value = temp;

            // Mise à jour des coefficients
            temp = x0;
            x0 = x1 - q * x0;
            x1 = temp;
        }

        // Assurez-vous que x1 est positif
        if (x1 < 0)
            x1 += originalMod;

        return x1;
    }
    
    public static BigInteger Binomial(int n, int k)
    {
        if (k > n - k)
            k = n - k;
        BigInteger b = BigInteger.One;
        for (int i = 1, m = n; i <= k; i++, m--)
            b = (b * m) / i;
        return b;
    }
}