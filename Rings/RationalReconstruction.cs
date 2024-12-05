using System.Numerics;
using Rings.poly;
using Rings.poly.univar;

namespace Rings;

public sealed class RationalReconstruction
{
    private RationalReconstruction()
    {
    }

    /**
     * Performs a rational number reconstruction. If the answer is not unique, {@code null} is returned.
     *
     * @param n                num * den^(-1) mod modulus
     * @param modulus          the modulus
     * @param numeratorBound   numerator bound
     * @param denominatorBound denominator bound
     */
    public static long[] reconstruct(long n, long modulus,
        long numeratorBound,
        long denominatorBound)
    {
        long[] v = { modulus, 0 };
        long[] w = { n, 1 };

        while (w[0] > numeratorBound)
        {
            long q = v[0] / w[0];
            long[] z = { v[0] - q * w[0], v[1] - q * w[1] }; // this is safe (no long overflow)
            v = w;
            w = z;
        }

        if (w[1] < 0)
        {
            w[0] = -w[0];
            w[1] = -w[1];
        }

        if (w[1] <= denominatorBound && MachineArithmetic.gcd(w[0], w[1]) == 1)
            return w;
        return null;
    }

    /**
     * Performs a rational number reconstruction. If the answer is not unique, {@code null} is returned.
     */
    public static BigInteger[] reconstruct(BigInteger n, BigInteger modulus,
        BigInteger numeratorBound,
        BigInteger denominatorBound)
    {
        BigInteger[] v = { modulus, BigInteger.Zero };
        BigInteger[] w = { n, BigInteger.One };

        while (w[0].CompareTo(numeratorBound) > 0)
        {
            BigInteger q = v[0] / w[0];
            BigInteger[] z = { v[0]- q * w[0], v[1] - q * w[1] };
            v = w;
            w = z;
        }

        if (w[1].Sign < 0)
        {
            w[0] = -w[0];
            w[1] = -w[1];
        }

        if (w[1].CompareTo(denominatorBound) <= 0 && BigInteger.GreatestCommonDivisor(w[0], w[1]).IsOne)
            return w;
        return null;
    }

    /**
     * Performs a rational number reconstruction via Farey images, that is reconstructuction with bound B = sqrt(N/2 -
     * 1/2)
     */
    public static BigInteger[] reconstructFarey(BigInteger n, BigInteger modulus)
    {
        BigInteger[] v = { modulus, BigInteger.Zero };
        BigInteger[] w = { n, BigInteger.One };

        while ((BigInteger.Pow(w[0], 2) << 1 + 1).CompareTo(modulus) > 0)
        {
            BigInteger q = v[0] / w[0];
            BigInteger[] z = { v[0] - q * w[0], v[1] - q * w[1] };
            v = w;
            w = z;
        }

        if (w[1].Sign < 0)
        {
            w[0] = -w[0];
            w[1] = -w[1];
        }

        if ((BigInteger.Pow(w[1], 2) * 2).CompareTo(modulus) <= 0 && BigInteger.GreatestCommonDivisor(w[0], w[1]).IsOne)
            return w;
        return null;
    }

    /**
     * Performs a error tolerant rational number reconstruction as described in Algorithm 5 of Janko Boehm, Wolfram
     * Decker, Claus Fieker, Gerhard Pfister, "The use of Bad Primes in Rational Reconstruction",
     * https://arxiv.org/abs/1207.1651v2
     */
    public static BigInteger[] reconstructFareyErrorTolerant(BigInteger n, BigInteger modulus)
    {
        BigInteger[] v = [modulus, BigInteger.Zero];
        BigInteger[] w = [n, BigInteger.One];

        BigInteger qNum, wqDen = BigInteger.Pow(w[0], 2) + BigInteger.Pow(w[1], 2), vqDen;
        do
        {
            qNum = w[0] * v[0] + w[1] * v[1];
            BigInteger q
                = qNum.Sign == wqDen.Sign
                    ? (BigInteger.Abs(qNum) + BigInteger.Abs(wqDen) - 1) / BigInteger.Abs(wqDen)
                    : qNum / wqDen;
            BigInteger[] z = { v[0] - q * w[0], v[1] - q * w[1] };

            v = w;
            vqDen = wqDen;

            w = z;
            wqDen = BigInteger.Pow(z[0], 2) + BigInteger.Pow(z[1], 2);
        } while (wqDen.CompareTo(vqDen) < 0);

        if (vqDen.CompareTo(modulus) < 0)
        {
            if (v[1].Sign < 0)
            {
                v[0] = -v[0];
                v[1] = -v[1];
            }

            return v;
        }

        return null;
    }

    /**
     * Performs a rational number reconstruction. If the answer is not unique, {@code null} is returned.
     *
     * @param n                num * den^(-1) mod modulus
     * @param modulus          the modulus
     * @param numeratorBound   numerator bound
     * @param denominatorBound denominator bound
     */
    public static Poly[] reconstruct<Poly>(Poly n, Poly modulus,
        int numeratorBound,
        int denominatorBound) where Poly : IUnivariatePolynomial<Poly>
    {
        Poly[] v = [modulus, n.createZero()];
        Poly[] w = [n, n.createOne()];

        while (w[0].degree() > numeratorBound)
        {
            Poly q = UnivariateDivision.quotient(v[0], w[0], true);
            Poly[] z = [
                v[0].clone().subtract(q.clone().multiply(w[0])),
                v[1].clone().subtract(q.clone().multiply(w[1]))
            ];
            v = w;
            w = z;
        }

        if (w[1].signumOfLC() < 0)
        {
            w[0].negate();
            w[1].negate();
        }

        if (w[1].degree() <= denominatorBound && UnivariateGCD.PolynomialGCD(w[0], w[1]).isConstant())
            return w;
        return null;
    }
}