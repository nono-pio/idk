using Cc.Redberry.Rings.Bigint;
using Java.Util;
using Cc.Redberry.Rings.Bigint.BigInteger;
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
    // Simple Quadratic Sieve found somewhere on the internet
    sealed class QuadraticSieve
    {
        private readonly BigInteger n;
        private static readonly int bitChunkSize = 30;
        private static readonly int PRIME_BASE = 1500;
        private static readonly int ADDITIONAL = bitChunkSize;
        private readonly int[] primes;
        private readonly BigInteger[] primesBig;
        private readonly double primeLog;
        private readonly double[] primeLogs;
        private readonly List<BigInteger> decomposedNumbers = new List();
        private BigInteger[] s, fs;
        private double[] logs;
        private int decomposed = 0;
        public QuadraticSieve(BigInteger num)
        {
            n = num;
            primes = new int[PRIME_BASE];
            primes[0] = -1;
            primes[1] = 2;
            int k = 2, nModP;
            foreach (int j in SmallPrimes.SmallPrimes12)
            {
                if (j == 2)
                    continue;
                nModP = n.Mod(BigInteger.ValueOf(j)).IntValue();
                if (LegendreSymbol(nModP, j) == 1)
                {
                    primes[k++] = j;
                    if (k == PRIME_BASE)
                        break;
                }
            }

            if (k < PRIME_BASE)
            {
                int j = primes[k - 1] + 2;
                while (k < PRIME_BASE)
                {
                    if (SmallPrimes.IsPrime(j))
                    {
                        nModP = n.Mod(BigInteger.ValueOf(j)).IntValue();
                        if (LegendreSymbol(nModP, j) == 1)
                            primes[k++] = j;
                    }

                    j += 2;
                }
            }

            primesBig = new BigInteger[PRIME_BASE];
            primeLogs = new double[PRIME_BASE];
            for (k = 1; k < PRIME_BASE; k++)
            {
                primesBig[k] = BigInteger.ValueOf(primes[k]);
                primeLogs[k] = Math.Log(primes[k]);
            }

            primeLog = primeLogs[PRIME_BASE - 1]; //        int j, k, nModP;
            //        primes = new int[PRIME_BASE];
            //        primes[0] = -1; k = 1;
            //        primes[1] =  2; k = 2;
            //        j = 3;
            //        while (k < PRIME_BASE) {
            //            if (isPrimeInt(j)) {
            //                nModP = n.mod(BigInteger.valueOf(j)).intValue();
            //                if (legendreSymbol(nModP, j) == 1)
            //                    primes[k++] = j;
            //            }
            //            j += 2;
            //        }
            //        primesBig = new BigInteger[PRIME_BASE];
            //        primeLogs = new double[PRIME_BASE];
            //        for (k = 1; k < PRIME_BASE; k++) {
            //            primesBig[k] = BigInteger.valueOf(primes[k]);
            //            primeLogs[k] = (double)Math.log(primes[k]);
            //        }
            //        primeLog = primeLogs[PRIME_BASE - 1];
        }

        private static BigInteger QuadraticF(BigInteger x, BigInteger n)
        {
            return x.Multiply(x).Subtract(n);
        }

        private byte[] DecomposeNumber(BigInteger number)
        {
            int j;
            byte[] result = new byte[PRIME_BASE];
            bool divided = false;
            if (number.Signum() < 0)
            {
                number = ZERO.Subtract(number);
                result[0] = 1;
            }
            else
                result[0] = 0;
            for (j = 1; j < PRIME_BASE; j++)
            {
                result[j] = 0;
                BigInteger k = primesBig[j];
                while (number.Mod(k).CompareTo(ZERO) == 0)
                {
                    divided = true;
                    number = number.Divide(k);
                    result[j]++;
                }

                if ((j * 2 == PRIME_BASE) && (!divided))
                    break;
            }

            if (number.CompareTo(ONE) > 0)
                result[0] = -1;
            return result;
        }

        private byte[][] BuildMatrix(BigInteger[] numbers, int size)
        {
            byte[, ] matrix = new byte[size, size];
            BigInteger temp, prim;
            int j, k;
            for (j = 0; j < size; j++)
            {
                temp = numbers[j];
                if (temp.Signum() < 0)
                {
                    temp = temp.Negate();
                    matrix[j][0] = 1;
                }
                else
                    matrix[j][0] = 0;
                for (k = 1; k < PRIME_BASE; k++)
                {
                    matrix[j][k] = 0;
                    prim = primesBig[k];
                    while (temp.Mod(prim).CompareTo(ZERO) == 0)
                    {
                        matrix[j][k]++; // = 1 - matrix[j][k];
                        temp = temp.Divide(prim);
                    }
                }

                for (k = PRIME_BASE; k < size; k++)
                    matrix[j][k] = 0;
            }

            return matrix;
        }

        private static int[][] FlattenMatrix(byte[, ] matrix)
        {
            int[, ] m = new int[matrix.Length, matrix.Length / bitChunkSize];
            int j, k, n;
            int comparation;
            for (j = 0; j < matrix.Length; j++)
                for (k = 0; k < matrix.Length / bitChunkSize; k++)
                {
                    comparation = 1;
                    m[j][k] = 0;
                    for (n = 0; n < bitChunkSize; n++)
                    {
                        if ((matrix[j][k * bitChunkSize + n] & 1) > 0)
                            m[j][k] += comparation;
                        comparation *= 2;
                    }
                }

            return m;
        }

        private static int[][] BuildIdentity(int size)
        {
            int[, ] matrix = new int[size, size / bitChunkSize];
            int j, k;
            for (j = 0; j < size; j++)
                for (k = 0; k < size / bitChunkSize; k++)
                    matrix[j][k] = 0;
            k = -1;
            int comparation = 0;
            for (j = 0; j < size; j++)
            {
                if (j % bitChunkSize == 0)
                {
                    k++;
                    comparation = 1;
                }
                else
                    comparation *= 2;
                matrix[j][k] = comparation;
            }

            return matrix;
        }

        private static void GaussElim(int[, ] matrix, int[, ] right, int j, int k)
        {
            int c1, c2;
            int temp;
            int comparation = 1;
            for (c1 = 1; c1 <= (j % bitChunkSize); c1++)
                comparation *= 2;
            if ((matrix[j][j / bitChunkSize] & comparation) == 0)
                for (c1 = j + 1; c1 < k; c1++)
                    if ((matrix[c1][j / bitChunkSize] & comparation) > 0)
                    {
                        for (c2 = j / bitChunkSize; c2 < k / bitChunkSize; c2++)
                        {
                            temp = matrix[j][c2];
                            matrix[j][c2] = matrix[c1][c2];
                            matrix[c1][c2] = temp;
                        }

                        for (c2 = 0; c2 < k / bitChunkSize; c2++)
                        {
                            temp = right[j][c2];
                            right[j][c2] = right[c1][c2];
                            right[c1][c2] = temp;
                        }

                        break;
                    }

            if ((matrix[j][j / bitChunkSize] & comparation) > 0)
                for (c1 = j + 1; c1 < k; c1++)
                    if ((matrix[c1][j / bitChunkSize] & comparation) > 0)
                    {
                        for (c2 = j / bitChunkSize; c2 < k / bitChunkSize; c2++)
                            matrix[c1][c2] = (matrix[c1][c2] ^ matrix[j][c2]);
                        for (c2 = 0; c2 < k / bitChunkSize; c2++)
                            right[c1][c2] = (right[c1][c2] ^ right[j][c2]);
                    }
        }

        private static void SolveMatrix(int[, ] matrix, int[, ] right)
        {
            int j, k;
            k = matrix.Length;
            for (j = 0; j < k - 1; j++)
                GaussElim(matrix, right, j, k);
        }

        private byte[] ExtractLine(int[, ] right, int index)
        {
            int j, k;
            int[] line = right[index];
            byte[] result = new byte[PRIME_BASE + ADDITIONAL];
            int comparation = 1;
            for (j = 0; j < PRIME_BASE + ADDITIONAL; j++)
            {
                if ((line[j / bitChunkSize] & comparation) > 0)
                    result[j] = 1;
                else
                    result[j] = 0;
                if (j % bitChunkSize == (bitChunkSize - 1))
                    comparation = 1;
                else
                    comparation *= 2;
            }

            return result;
        }

        private long[] FindFlats(long p, long n)
        {
            long k, x;
            long[] result = new long[2];
            result[0] = -1;
            result[1] = -1;
            if (p == 2)
            {
                result[0] = n % 2;
                result[1] = -1;
                return result;
            }

            if (p % 4 == 3)
            {
                k = (p / 4);
                x = ModPowLong(n, k + 1, p) % p;
                result[0] = x;
                result[1] = (p - x);
                return result;
            }

            if (p % 8 == 5)
            {
                k = (p / 8);
                x = ModPowLong(n, 2 * k + 1, p);
                if (x == 1)
                {
                    x = ModPowLong(n, k + 1, p);
                    result[0] = x;
                    result[1] = (p - x);
                    return result;
                }

                if (x == p - 1)
                {
                    x = ModPowLong(4 * n, k + 1, p);
                    x = (x * (p + 1) / 2) % p;
                    result[0] = x;
                    result[1] = (p - x);
                    return result;
                }
            }

            long h = 13;
            do h += 2;
            while (LegendreSymbol(h * h - 4 * n, p) != -1);
            k = (p + 1) / 2;
            x = V_(k, h, n, p);
            if (x < 0)
                x += p;
            x = (x * k) % p;
            result[0] = x;
            result[1] = (p - x);
            return result;
        }

        private void RemoveHighestPower(int index, BigInteger p)
        {
            if (fs[index].Mod(p).CompareTo(ZERO) == 0)
            {
                do fs[index] = fs[index].Divide(p);
                while (fs[index].Mod(p).CompareTo(ZERO) == 0);
                if (fs[index].CompareTo(ONE) == 0)
                {
                    logs[index] = 0;
                    decomposedNumbers.Add(s[index]);
                    decomposed++;
                }
            }
            else
            {
                throw new Exception(); //            writeln("Das Sieb macht Probleme, Sir!");
                //            writeln("fs[index] = " + fs[index] + ", p = " + p);
                //            try {
                //                Thread.sleep(100);
                //            } catch (InterruptedException ie) {
                //            }
            }
        }

        /// <summary>
        /// ************************************************************************************************
        /// </summary>
        /// <summary>
        /// ** CFRAC: The Continued Fractions algorithm, first presented by Brillhart and Morrison.     ****
        /// </summary>
        /// <summary>
        /// ** Continued Fraction works - just as the Quadratic Sieve - using an x² = y² (mod n) con-   ****
        /// </summary>
        /// <summary>
        /// ** gruence. The difference: Instead of sieving, CFRAC speeds things up by producing small   ****
        /// </summary>
        /// <summary>
        /// ** y²'s that can easily be factored.                                                        ****
        /// </summary>
        /// <summary>
        /// ************************************************************************************************
        /// </summary>
        public BigInteger CFRAC(int upperBound)
        {
            int i, k;
            BigInteger sqr;
            BigInteger Ai;
            BigInteger Bi, Bj;
            BigInteger Ci, Cj, Ck;
            BigInteger Pi, Pj, Pk;
            BigInteger x, y;
            BigInteger sqrt = SqrtBigInt(n);
            Bj = sqrt;
            Ck = ONE;
            Cj = n.Subtract(sqrt.Multiply(sqrt));
            Pk = ONE;
            Pj = sqrt;
            byte[] facs;
            byte[, ] factors = new byte[PRIME_BASE + ADDITIONAL, PRIME_BASE + ADDITIONAL];
            for (i = 0; i < PRIME_BASE + ADDITIONAL; i++)
                for (k = 0; k < PRIME_BASE + ADDITIONAL; k++)
                    factors[i][k] = 0;
            BigInteger[] s = new BigInteger[PRIME_BASE + ADDITIONAL];
            decomposed = 0;
            i = 1;
            while (decomposed < PRIME_BASE + ADDITIONAL)
            {
                i = i + 1;
                Ai = sqrt.Add(Bj).Divide(Cj).Mod(n);
                Bi = Ai.Multiply(Cj).Subtract(Bj).Mod(n);
                Ci = Ck.Add(Ai.Multiply(Bj.Subtract(Bi))).Mod(n);
                Pi = Pk.Add(Ai.Multiply(Pj)).Mod(n);
                if (i % 2 == 0)
                    sqr = Ci;
                else
                    sqr = ZERO.Subtract(Ci);
                facs = DecomposeNumber(sqr);
                if (facs[0] >= 0)
                {
                    for (k = 0; k < PRIME_BASE; k++)
                        factors[decomposed][k] = facs[k];
                    s[decomposed] = Pi;
                }

                /*
                 else {
                 if (remaining.compareTo(largePrimeBound) < 0) {
                 if (ts.contains(remaining)) {
                 ts.remove(remaining);
                 almost++;
                 }
                 else {
                 if (ts.size() > PRIME_BASE * 5) {
                 writeln("Cleared.");
                 ts.clear();
                 }
                 ts.add(remaining);
                 }
                 }
                 }
                 */
                Bj = Bi;
                Ck = Cj;
                Cj = Ci;
                Pk = Pj;
                Pj = Pi;
            }

            int[, ] identity = BuildIdentity(PRIME_BASE + ADDITIONAL);
            int[, ] matrix = FlattenMatrix(factors);
            SolveMatrix(matrix, identity);
            int loop = decomposed - 1;
            do
            {
                int[] primefacs = new int[PRIME_BASE];
                byte[] factorLine = ExtractLine(identity, loop);
                x = ONE;
                for (i = 0; i < PRIME_BASE; i++)
                    primefacs[i] = 0;
                for (i = 0; i < decomposed; i++)
                {
                    if (factorLine[i] == 1)
                    {
                        for (int j = 0; j < PRIME_BASE; j++)
                            primefacs[j] += (int)factors[i][j];
                        x = x.Multiply(s[i]).Mod(n);
                    }
                }

                y = ONE;
                for (i = 0; i < PRIME_BASE; i++)
                    y = y.Multiply(BigInteger.ValueOf(primes[i]).ModPow(BigInteger.ValueOf(primefacs[i] / 2), n)).Mod(n);
                x = x.Mod(n);
                y = y.Mod(n);
                x = x.Add(y);
                y = x.Subtract(y).Subtract(y);
                x = n.Gcd(x);
                if ((x.CompareTo(ONE) != 0) && (x.CompareTo(n) != 0))
                    break;
            }
            while (--loop > PRIME_BASE);
            return x;
        }

        /// <summary>
        /// ************************************************************************************************
        /// </summary>
        /// <summary>
        /// ** QUADRATIC SIEVE: this algorithm is highly sophisticated, though not fully optimized yet. ****
        /// </summary>
        /// <summary>
        /// ** Like most modern factoring techniques, QS uses congruences x^2 = y^2 (mod n) to find     ****
        /// </summary>
        /// <summary>
        /// ** a non-trivial divisor of n.                                                              ****
        /// </summary>
        /// <summary>
        /// ** See David M. Bressoud: "Factorization and Primality Testing" (1989) for further details. ****
        /// </summary>
        /// <summary>
        /// ************************************************************************************************
        /// </summary>
        public BigInteger QuadraticSieve(int upperBound)
        {
            BigInteger m, x, y, test, prim;
            BigInteger quadraticN = n;
            int i, j, loop, mInt, p;
            int[, ] flats = new int[PRIME_BASE, 2];
            double logp;
            long[] tempflat;
            for (i = 1; i < PRIME_BASE; i++)
            {
                tempflat = FindFlats(primes[i], quadraticN.Mod(primesBig[i]).IntValue());
                flats[i][0] = (int)tempflat[0];
                flats[i][1] = (int)tempflat[1];
            }

            int offset = 0;
            int direction = 0;
            m = SqrtBigInt(n).Add(ONE);
            s = new BigInteger[upperBound + 2];
            fs = new BigInteger[upperBound + 2];
            logs = new double[upperBound + 2];
            do
            {
                switch (direction)
                {
                    case 0:
                        direction = 1;
                        offset = 0;
                        break;
                    case 1:
                        direction = -1;
                        offset = -offset + upperBound;
                        break;
                    case -1:
                        direction = 1;
                        offset = -offset;
                        break;
                }

                for (i = 0; i < upperBound; i++)
                {
                    s[i] = null;
                    fs[i] = null;
                    logs[i] = 0;
                }

                for (i = 1; i < PRIME_BASE; i++)
                {
                    p = primes[i];
                    logp = primeLogs[i];
                    mInt = m.Mod(primesBig[i]).IntValue() + offset;
                    if (flats[i][0] >= 0)
                    {
                        loop = ((flats[i][0] - mInt) % p);
                        if (loop < 0)
                            loop += p;
                        while (loop < upperBound)
                        {
                            logs[loop] += logp;
                            loop += p;
                        }
                    }

                    if (flats[i][1] >= 0)
                    {
                        loop = ((flats[i][1] - mInt) % p);
                        if (loop < 0)
                            loop += p;
                        while (loop < upperBound)
                        {
                            logs[loop] += logp;
                            loop += p;
                        }
                    }
                }

                double TARGET = (Math.Log(m.DoubleValue()) + Math.Log(upperBound) - primeLog);
                for (i = 0; i < upperBound; i++)
                {
                    if (logs[i] > TARGET)
                    {
                        s[i] = BigInteger.ValueOf(i + offset).Add(m);
                        fs[i] = QuadraticF(s[i], quadraticN).Abs();
                    }
                }

                for (i = 1; i < PRIME_BASE; i++)
                {
                    p = primes[i];
                    mInt = m.Mod(primesBig[i]).IntValue() + offset;
                    if (flats[i][0] >= 0)
                    {
                        loop = ((flats[i][0] - mInt) % p);
                        if (loop < 0)
                            loop += p;
                        while (loop < upperBound)
                        {
                            if (logs[loop] > TARGET)
                            {
                                RemoveHighestPower(loop, primesBig[i]);
                                if (decomposed >= PRIME_BASE + ADDITIONAL)
                                    break;
                            }

                            loop += p;
                        }
                    }

                    if (flats[i][1] >= 0)
                    {
                        loop = ((flats[i][1] - mInt) % p);
                        if (loop < 0)
                            loop += p;
                        while (loop < upperBound)
                        {
                            if (logs[loop] > TARGET)
                            {
                                RemoveHighestPower(loop, primesBig[i]);
                                if (decomposed >= PRIME_BASE + ADDITIONAL)
                                    break;
                            }

                            loop += p;
                        }
                    }

                    if (decomposed >= PRIME_BASE + ADDITIONAL)
                        break;
                }
            }
            while (decomposed < PRIME_BASE + ADDITIONAL);
            if (decomposed > PRIME_BASE + ADDITIONAL)
                decomposed = PRIME_BASE + ADDITIONAL;
            s = new BigInteger[decomposed + 1];
            fs = new BigInteger[decomposed + 1];
            for (i = 0; i < decomposed; i++)
            {
                s[i] = decomposedNumbers[i];
                fs[i] = QuadraticF(s[i], quadraticN);
            }

            byte[, ] factors = BuildMatrix(fs, decomposed);
            int[, ] identity = BuildIdentity(decomposed);
            int[, ] matrix = FlattenMatrix(factors);
            SolveMatrix(matrix, identity);
            loop = decomposed - 1;
            do
            {
                int[] primefacs = new int[PRIME_BASE];
                byte[] factorLine = ExtractLine(identity, loop);
                test = ONE;
                for (i = 0; i < PRIME_BASE; i++)
                    primefacs[i] = 0;
                for (i = 0; i < decomposed; i++)
                {
                    if (factorLine[i] == 1)
                    {
                        for (j = 0; j < PRIME_BASE; j++)
                            primefacs[j] += (int)factors[i][j];
                        test = test.Multiply(s[i]).Mod(n);
                    }
                }

                prim = ONE;
                for (i = 0; i < PRIME_BASE; i++)
                {
                    y = BigInteger.ValueOf(primes[i]).ModPow(BigInteger.ValueOf(primefacs[i] / 2), n);
                    prim = prim.Multiply(y).Mod(n);
                }

                test = test.Mod(n);
                prim = prim.Mod(n);
                x = test.Add(prim);
                y = test.Subtract(prim);
                test = n.Gcd(x);
                if ((test.CompareTo(ONE) != 0) && (test.CompareTo(n) != 0))
                    break;
            }
            while (--loop > PRIME_BASE);
            return test;
        }

        static long LegendreSymbol(long n, long p)
        {
            long count, temp;
            long legendre = 1;
            if (n == 0)
                return 0;
            if (n < 0)
            {
                n = -n;
                if (p % 4 == 3)
                    legendre = -1;
            }

            do
            {
                count = 0;
                while (n % 2 == 0)
                {
                    n = n / 2;
                    count = 1 - count;
                }

                if ((count * (p * p - 1)) % 16 == 8)
                    legendre = -legendre;
                if (((n - 1) * (p - 1)) % 8 == 4)
                    legendre = -legendre;
                temp = n;
                n = p % n;
                p = temp;
            }
            while (n > 1);
            return legendre;
        }

        private static long ModPowLong(long n, long p, long m)
        {
            if (p == 0)
                return 1;
            if (p % 2 == 1)
                return (n * ModPowLong(n, p - 1, m)) % m;
            else
            {
                long result = ModPowLong(n, p / 2, m);
                return (result * result) % m;
            }
        }

        static BigInteger SqrtBigInt(BigInteger i)
        {
            long c;
            BigInteger medium;
            BigInteger high = i;
            BigInteger low = BigInteger.ONE;
            while (high.Subtract(low).CompareTo(BigInteger.ONE) > 0)
            {
                medium = high.Add(low).Divide(BigInteger.ONE.Add(BigInteger.ONE));
                c = medium.Multiply(medium).CompareTo(i);
                if (c > 0)
                    high = medium;
                if (c < 0)
                    low = medium;
                if (c == 0)
                    return medium;
            }

            if (high.Multiply(high).CompareTo(i) == 0)
                return high;
            else
                return low;
        }

        private static long V(long i, long h, long n, long p)
        {
            if (i == 1)
                return h;
            if (i == 2)
                return (h * h - 2 * n) % p;
            long vi = V(i / 2, h, n, p);
            long vi_1 = V(i / 2 + 1, h, n, p);
            if (i % 2 == 1)
            {
                vi = (vi * vi_1 - h * ModPowLong(n, i / 2, p)) % p;
                if (vi < 0)
                    vi += p;
                return vi;
            }
            else
                return (vi * vi - 2 * ModPowLong(n, i / 2, p)) % p;
        }

        private static long V_(long j, long h, long n, long p)
        {
            long[] b = new long[64];
            long m = n;
            long v = h;
            long w = (h * h - 2 * m) % p;
            long x;
            int k, t;
            t = 0;
            while (j > 0)
            {
                b[++t] = j % 2;
                j /= 2;
            }

            for (k = t - 1; k >= 1; k--)
            {
                x = (v * w - h * m) % p;
                v = (v * v - 2 * m) % p;
                w = (w * w - 2 * n * m) % p;
                m = m * m % p;
                if (b[k] == 0)
                    w = x;
                else
                {
                    v = x;
                    m = n * m % p;
                }
            }

            return v;
        }
    }
}