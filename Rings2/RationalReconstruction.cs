using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Poly;
using Cc.Redberry.Rings.Poly.Univar;
using System.Numerics;

namespace Cc.Redberry.Rings
{
    /// <summary>
    /// </summary>
    /// <remarks>@since2.3</remarks>
    public sealed class RationalReconstruction
    {
        /// <summary>
        /// Performs a rational number reconstruction. If the answer is not unique, {@code null} is returned.
        /// </summary>
        /// <param name="n">num * den^(-1) mod modulus</param>
        /// <param name="modulus">the modulus</param>
        /// <param name="numeratorBound">numerator bound</param>
        /// <param name="denominatorBound">denominator bound</param>
        public static long[]? Reconstruct(long n, long modulus, long numeratorBound, long denominatorBound)
        {
            long[] v = new[]
            {
                modulus,
                0
            };
            long[] w = new[]
            {
                n,
                1
            };
            while (w[0] > numeratorBound)
            {
                long q = v[0] / w[0];
                long[] z = new[]
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

        /// <summary>
        /// Performs a rational number reconstruction. If the answer is not unique, {@code null} is returned.
        /// </summary>
        public static BigInteger[]? Reconstruct(BigInteger n, BigInteger modulus, BigInteger numeratorBound,
            BigInteger denominatorBound)
        {
            BigInteger[] v = new[]
            {
                modulus,
                BigInteger.Zero
            };
            BigInteger[] w = new[]
            {
                n,
                BigInteger.One
            };
            while (w[0].CompareTo(numeratorBound) > 0)
            {
                BigInteger q = v[0].Divide(w[0]);
                BigInteger[] z = new[]
                {
                    v[0].Subtract(q.Multiply(w[0])),
                    v[1].Subtract(q.Multiply(w[1]))
                };
                v = w;
                w = z;
            }

            if (w[1].Signum() < 0)
            {
                w[0] = w[0].Negate();
                w[1] = w[1].Negate();
            }

            if (w[1].CompareTo(denominatorBound) <= 0 && BigIntegerUtil.Gcd(w[0], w[1]).IsOne)
                return w;
            return null;
        }

        /// <summary>
        /// Performs a rational number reconstruction via Farey images, that is reconstructuction with bound B = sqrt(N/2 -
        /// 1/2)
        /// </summary>
        public static BigInteger[]? ReconstructFarey(BigInteger n, BigInteger modulus)
        {
            BigInteger[] v = new[]
            {
                modulus,
                BigInteger.Zero
            };
            BigInteger[] w = new[]
            {
                n,
                BigInteger.One
            };
            while (w[0].Pow(2).ShiftLeft(1).Increment().CompareTo(modulus) > 0)
            {
                BigInteger q = v[0].Divide(w[0]);
                BigInteger[] z = new[]
                {
                    v[0].Subtract(q.Multiply(w[0])),
                    v[1].Subtract(q.Multiply(w[1]))
                };
                v = w;
                w = z;
            }

            if (w[1].Signum() < 0)
            {
                w[0] = w[0].Negate();
                w[1] = w[1].Negate();
            }

            if (w[1].Pow(2).Multiply(2).CompareTo(modulus) <= 0 && BigIntegerUtil.Gcd(w[0], w[1]).IsOne)
                return w;
            return null;
        }

        /// <summary>
        /// Performs a error tolerant rational number reconstruction as described in Algorithm 5 of Janko Boehm, Wolfram
        /// Decker, Claus Fieker, Gerhard Pfister, "The use of Bad Primes in Rational Reconstruction",
        /// https://arxiv.org/abs/1207.1651v2
        /// </summary>
        public static BigInteger[]? ReconstructFareyErrorTolerant(BigInteger n, BigInteger modulus)
        {
            BigInteger[] v = new[]
            {
                modulus,
                BigInteger.Zero
            };
            BigInteger[] w = new[]
            {
                n,
                BigInteger.One
            };
            BigInteger qNum, wqDen = w[0].Pow(2).Add(w[1].Pow(2)), vqDen;
            do
            {
                qNum = w[0].Multiply(v[0]).Add(w[1].Multiply(v[1]));
                BigInteger q = qNum.Signum() == wqDen.Signum()
                    ? qNum.Abs().Add(wqDen.Abs()).Decrement().Divide(wqDen.Abs())
                    : qNum.Divide(wqDen);
                BigInteger[] z = new[]
                {
                    v[0].Subtract(q.Multiply(w[0])),
                    v[1].Subtract(q.Multiply(w[1]))
                };
                v = w;
                vqDen = wqDen;
                w = z;
                wqDen = z[0].Pow(2).Add(z[1].Pow(2));
            } while (wqDen.CompareTo(vqDen) < 0);

            if (vqDen.CompareTo(modulus) < 0)
            {
                if (v[1].Signum() < 0)
                {
                    v[0] = v[0].Negate();
                    v[1] = v[1].Negate();
                }

                return v;
            }

            return null;
        }

        /// <summary>
        /// Performs a rational number reconstruction. If the answer is not unique, {@code null} is returned.
        /// </summary>
        /// <param name="n">num * den^(-1) mod modulus</param>
        /// <param name="modulus">the modulus</param>
        /// <param name="numeratorBound">numerator bound</param>
        /// <param name="denominatorBound">denominator bound</param>
        public static Poly[]? Reconstruct<Poly>(Poly n, Poly modulus, int numeratorBound, int denominatorBound)
            where Poly : IUnivariatePolynomial<Poly>
        {
            Poly[] v = [modulus, n.CreateZero()];
            Poly[] w = [n, n.CreateOne()];
            while (w[0].Degree() > numeratorBound)
            {
                Poly q = UnivariateDivision.Quotient(v[0], w[0], true);
                Poly[] z = n.CreateArray(v[0].Clone().Subtract(q.Clone().Multiply(w[0])),
                    v[1].Clone().Subtract(q.Clone().Multiply(w[1])));
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
}