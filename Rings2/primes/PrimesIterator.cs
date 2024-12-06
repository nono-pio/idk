using Cc.Redberry.Rings.Bigint;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Cc.Redberry.Rings.Primes.RoundingMode;
using static Cc.Redberry.Rings.Primes.Associativity;
using static Cc.Redberry.Rings.Primes.Operator;
using static Cc.Redberry.Rings.Primes.TokenType;
using static Cc.Redberry.Rings.Primes.SystemInfo;

namespace Cc.Redberry.Rings.Primes
{
    /// <summary>
    /// Iterator over prime numbers.
    /// </summary>
    public sealed class PrimesIterator
    {
        private readonly int[] smallPrimes = SmallPrimes.SmallPrimes12;
        private long pointer;
        private int smallPrimesPointer = Integer.MAX_VALUE;
        private static readonly int largeSieveLimit = 16777216;
        private readonly SieveOfAtkin smallSieve = SieveOfAtkin.SmallPrimesSieve;
        private SieveOfAtkin largeSieve = null;
        /// <summary>
        /// Create iterator over prime numbers starting from 2.
        /// </summary>
        public PrimesIterator()
        {
            pointer = 0;
        }

        /// <summary>
        /// Create iterator over prime numbers starting from the prime closest to the specified value (prime >= from)
        /// </summary>
        public PrimesIterator(long from)
        {
            if (from < smallPrimes[smallPrimes.Length - 1])
            {
                smallPrimesPointer = Arrays.BinarySearch(smallPrimes, (int)from);
                if (smallPrimesPointer < 0)
                    smallPrimesPointer = ~smallPrimesPointer;
                pointer = smallPrimes[smallPrimesPointer];
            }
            else
                pointer = from;
        }

        /// <summary>
        /// Get the next prime number
        /// </summary>
        public long Take()
        {
            if (smallPrimesPointer < smallPrimes.Length)
                return (pointer = smallPrimes[smallPrimesPointer++] + 1) - 1;
            for (; pointer < smallSieve.GetLimit();)
            {
                if (smallSieve.IsPrime((int)(pointer++)))
                    return pointer - 1;
            }

            if (pointer < largeSieveLimit)
            {
                if (largeSieve == null)
                    largeSieve = SieveOfAtkin.CreateSieve(largeSieveLimit);
                for (; pointer < largeSieve.GetLimit();)
                    if (largeSieve.IsPrime((int)pointer++))
                        return pointer - 1;
            }

            if (pointer < Integer.MAX_VALUE - 1)
                return pointer = SmallPrimes.NextPrime((int)(pointer + 1));
            if (pointer < Long.MAX_VALUE - 1)
                try
                {
                    return pointer = BigPrimes.NextPrime(BigInteger.ValueOf(pointer + 1)).LongValueExact();
                }
                catch (ArithmeticException e)
                {
                    return -1;
                }

            return -1;
        }
    }
}