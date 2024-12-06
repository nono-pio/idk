using System.Numerics;

namespace Rings.primes;

public sealed class PrimesIterator {
    private readonly int[] smallPrimes = SmallPrimes.SmallPrimes12;
    private long pointer;
    private int smallPrimesPointer = int.MaxValue;
    private static readonly int largeSieveLimit = 16777216;
    private readonly SieveOfAtkin smallSieve = SieveOfAtkin.SmallPrimesSieve;
    private SieveOfAtkin largeSieve = null;

    /**
     * Create iterator over prime numbers starting from 2.
     */
    public PrimesIterator() {pointer = 0;}

    /**
     * Create iterator over prime numbers starting from the prime closest to the specified value (prime >= from)
     */
    public PrimesIterator(long from) {
        if (from < smallPrimes[smallPrimes.Length - 1]) {
            smallPrimesPointer = Array.BinarySearch(smallPrimes, (int) from);
            if (smallPrimesPointer < 0) smallPrimesPointer = ~smallPrimesPointer;
            pointer = smallPrimes[smallPrimesPointer];
        } else pointer = from;
    }

    /**
     * Get the next prime number
     */
    public long take() {
        if (smallPrimesPointer < smallPrimes.Length)
            return (pointer = smallPrimes[smallPrimesPointer++] + 1) - 1;

        for (; pointer < smallSieve.getLimit(); ) {
            if (smallSieve.isPrime((int) (pointer++)))
                return pointer - 1;
        }

        if (pointer < largeSieveLimit) {
            if (largeSieve == null)
                largeSieve = SieveOfAtkin.createSieve(largeSieveLimit);

            for (; pointer < largeSieve.getLimit(); )
                if (largeSieve.isPrime((int) pointer++))
                    return pointer - 1;
        }

        if (pointer < int.MaxValue - 1)
            return pointer = SmallPrimes.nextPrime((int) (pointer + 1));

        if (pointer < long.MaxValue - 1)
            try {
                return pointer = (long)BigPrimes.nextPrime(new BigInteger(pointer + 1));
            } catch (ArithmeticException e) {
                return -1;
            }

        return -1;
    }
}
