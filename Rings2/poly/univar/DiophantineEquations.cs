using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Cc.Redberry.Rings.Poly.Univar.RoundingMode;
using static Cc.Redberry.Rings.Poly.Univar.Associativity;
using static Cc.Redberry.Rings.Poly.Univar.Operator;
using static Cc.Redberry.Rings.Poly.Univar.TokenType;
using static Cc.Redberry.Rings.Poly.Univar.SystemInfo;

namespace Cc.Redberry.Rings.Poly.Univar
{
    /// <summary>
    /// </summary>
    /// <remarks>
    /// @authorStanislav Poslavsky
    /// @since2.1
    /// </remarks>
    public sealed class DiophantineEquations
    {
        private DiophantineEquations()
        {
        }

        /// <summary>
        /// runs xgcd for coprime polynomials ensuring that gcd is 1 (not another constant)
        /// </summary>
        public static Poly[] MonicExtendedEuclid<Poly extends IUnivariatePolynomial<Poly>>(Poly a, Poly b)
        {
            Poly[] xgcd = UnivariateGCD.PolynomialExtendedGCD(a, b);
            if (xgcd[0].IsOne())
                return xgcd;

            //normalize: x * a + y * b = 1
            xgcd[2].DivideByLC(xgcd[0]);
            xgcd[1].DivideByLC(xgcd[0]);
            xgcd[0].Monic();
            return xgcd;
        }

        /// <summary>
        /// Solves a1 * x1 + a2 * x2 + ... = rhs for given univariate and rhs and unknown x_i
        /// </summary>
        public sealed class DiophantineSolver<Poly>
        {
            /// <summary>
            /// the given factors
            /// </summary>
            readonly Poly[] factors;
            /// <summary>
            /// the given factors
            /// </summary>
            readonly Poly[] solution;
            /// <summary>
            /// the given factors
            /// </summary>
            readonly Poly gcd;
            /// <summary>
            /// the given factors
            /// </summary>
            public DiophantineSolver(Poly[] factors)
            {
                this.factors = factors;
                this.solution = factors[0].CreateArray(factors.Length);
                Poly prev = factors[0];
                solution[0] = factors[0].CreateOne();
                for (int i = 1; i < factors.Length; i++)
                {
                    Poly[] xgcd = MonicExtendedEuclid(prev, factors[i]);
                    for (int j = 0; j < i; j++)
                        solution[j].Multiply(xgcd[1]);
                    solution[i] = xgcd[2];
                    prev = xgcd[0];
                }

                gcd = prev;
            }

            /// <summary>
            /// the given factors
            /// </summary>
            public Poly[] Solve(Poly rhs)
            {
                rhs = UnivariateDivision.DivideOrNull(rhs, gcd, true);
                if (rhs == null)
                    throw new ArgumentException("Not solvable.");
                Poly[] solution = rhs.CreateArray(this.solution.Length);
                for (int i = 0; i < solution.Length; i++)
                    solution[i] = this.solution[i].Clone().Multiply(rhs);
                return solution;
            }
        }
    }
}