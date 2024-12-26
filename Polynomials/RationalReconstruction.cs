using System.Numerics;
using Polynomials.Poly.Univar;
using Polynomials.Utils;

namespace Polynomials;

public sealed class RationalReconstruction
{
    public static long[]? Reconstruct(long n, long modulus, long numeratorBound, long denominatorBound)
    {
        long[] v =
        [
            modulus,
            0
        ];
        long[] w =
        [
            n,
            1
        ];
        while (w[0] > numeratorBound)
        {
            var q = v[0] / w[0];
            var z = new[]
            {
                v[0] - q * w[0],
                v[1] - q * w[1]
            }; // this is safe (no long overflow)
            v = w;
            w = z;
        }

        if (w[1] < 0)
        {
            w[0] = -w[0];
            w[1] = -w[1];
        }

        if (w[1] <= denominatorBound && MachineArithmetic.Gcd(w[0], w[1]) == 1)
            return w;
        return null;
    }


    public static BigInteger[]? Reconstruct(BigInteger n, BigInteger modulus, BigInteger numeratorBound,
        BigInteger denominatorBound)
    {
        var v = new[]
        {
            modulus,
            BigInteger.Zero
        };
        var w = new[]
        {
            n,
            BigInteger.One
        };
        while (w[0].CompareTo(numeratorBound) > 0)
        {
            var q = v[0] / w[0];
            BigInteger[] z =
            [
                v[0] - q * w[0],
                v[1] - q * w[1]
            ];
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


    public static BigInteger[]? ReconstructFarey(BigInteger n, BigInteger modulus)
    {
        var v = new[]
        {
            modulus,
            BigInteger.Zero
        };
        var w = new[]
        {
            n,
            BigInteger.One
        };
        while (((BigInteger.Pow(w[0], 2) << 1) + 1).CompareTo(modulus) > 0)
        {
            var q = v[0] / w[0];
            BigInteger[] z =
            [
                v[0] - q * w[0],
                v[1] - q * w[1]
            ];
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


    public static BigInteger[]? ReconstructFareyErrorTolerant(BigInteger n, BigInteger modulus)
    {
        BigInteger[] v =
        [
            modulus,
            BigInteger.Zero
        ];
        BigInteger[] w =
        [
            n,
            BigInteger.One
        ];
        BigInteger qNum, wqDen = BigInteger.Pow(w[0], 2) + BigInteger.Pow(w[1], 2), vqDen;
        do
        {
            qNum = w[0] * v[0] + w[1] * v[1];
            var q = qNum.Sign == wqDen.Sign
                ? (BigInteger.Abs(qNum) + BigInteger.Abs(wqDen) - 1) / BigInteger.Abs(wqDen)
                : qNum / wqDen;
            BigInteger[] z =
            [
                v[0] - q * w[0],
                v[1] - q * w[1]
            ];
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


    public static UnivariatePolynomial<E>[]? Reconstruct<E>(UnivariatePolynomial<E> n, UnivariatePolynomial<E> modulus, 
        int numeratorBound, int denominatorBound)
    {
        UnivariatePolynomial<E>[] v = [modulus, n.CreateZero()];
        UnivariatePolynomial<E>[] w = [n, n.CreateOne()];
        while (w[0].Degree() > numeratorBound)
        {
            var q = UnivariateDivision.Quotient(v[0], w[0]);
            UnivariatePolynomial<E>[] z = [v[0].Clone().Subtract(q.Clone().Multiply(w[0])),
                v[1].Clone().Subtract(q.Clone().Multiply(w[1]))];
            v = w;
            w = z;
        }

        if (w[1].SignumOfLC() < 0)
        {
            w[0].Negate();
            w[1].Negate();
        }

        if (w[1].Degree() <= denominatorBound && UnivariateGCD.PolynomialGCD(w[0], w[1]).IsConstant())
            return w;
        return null;
    }
}