namespace Polynomials.Primes;

using System.Numerics;
using Utils;


class BitSieve
{
    private long[] bits;


    private readonly int Length;


    private static BitSieve smallSieve = new BitSieve();


    private BitSieve()
    {
        Length = 150 * 64;
        bits = new long[(UnitIndex(Length - 1) + 1)];

        // Mark 1 as composite
        Set(0);
        int nextIndex = 1;
        int nextPrime = 3;

        // Find primes and remove their multiples from sieve
        do
        {
            SieveSingle(Length, nextIndex + nextPrime, nextPrime);
            nextIndex = SieveSearch(Length, nextIndex + 1);
            nextPrime = 2 * nextIndex + 1;
        } while ((nextIndex > 0) && (nextPrime < Length));
    }


    public BitSieve(BigInteger @base, int searchLen)
    {
        /*
         * Candidates are indicated by clear bits in the sieve. As a candidates
         * nonprimality is calculated, a bit is set in the sieve to eliminate
         * it. To reduce storage space and increase efficiency, no even numbers
         * are represented in the sieve (each bit in the sieve represents an
         * odd number).
         */
        bits = new long[(UnitIndex(searchLen - 1) + 1)];
        Length = searchLen;
        int start = 0;
        int step = smallSieve.SieveSearch(smallSieve.Length, start);
        int convertedStep = (step * 2) + 1;

        // Construct the large sieve at an even offset specified by base
        BigInteger b = @base;
        do
        {
            // Calculate base mod convertedStep
            BigInteger.DivRem(b, convertedStep, out BigInteger bigStart);
            start = (int)bigStart;

            // Take each multiple of step out of sieve
            start = convertedStep - start;
            if (start % 2 == 0)
                start += convertedStep;
            SieveSingle(searchLen, (start - 1) / 2, convertedStep);

            // Find next prime from small sieve
            step = smallSieve.SieveSearch(smallSieve.Length, step + 1);
            convertedStep = (step * 2) + 1;
        } while (step > 0);
    }


    private static int UnitIndex(int bitIndex)
    {
        return bitIndex >>> 6;
    }


    private static long Bit(int bitIndex)
    {
        return 1 << (bitIndex & ((1 << 6) - 1));
    }


    private bool Get(int bitIndex)
    {
        int unitIndex = UnitIndex(bitIndex);
        return ((bits[unitIndex] & Bit(bitIndex)) != 0);
    }


    private void Set(int bitIndex)
    {
        int unitIndex = UnitIndex(bitIndex);
        bits[unitIndex] |= Bit(bitIndex);
    }


    private int SieveSearch(int limit, int start)
    {
        if (start >= limit)
            return -1;
        int index = start;
        do
        {
            if (!Get(index))
                return index;
            index++;
        } while (index < limit - 1);

        return -1;
    }


    private void SieveSingle(int limit, int start, int step)
    {
        while (start < limit)
        {
            Set(start);
            start += step;
        }
    }


    public virtual BigInteger? Retrieve(BigInteger initValue, int certainty, Random? random)
    {
        // Examine the sieve one long at a time to find possible primes
        int offset = 1;
        for (int i = 0; i < bits.Length; i++)
        {
            long nextLong = ~bits[i];
            for (int j = 0; j < 64; j++)
            {
                if ((nextLong & 1) == 1)
                {
                    BigInteger candidate = initValue + offset;
                    if (candidate.PrimeToCertainty(certainty, random))
                        return candidate;
                }

                nextLong >>>= 1;
                offset += 2;
            }
        }

        return null;
    }
}