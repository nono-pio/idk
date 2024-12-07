using Cc.Redberry.Rings.Util;
using System.Collections.ObjectModel;

namespace Cc.Redberry.Rings.Poly
{
    /// <summary>
    /// {@inheritDoc}
    /// </summary>
    /// <remarks>
    /// @since1.0
    /// @since2.2 FactorDecomposition renamed to PolynomialFactorDecomposition
    /// </remarks>
    public sealed class PolynomialFactorDecomposition<Poly> : FactorDecomposition<Poly> where Poly : IPolynomial<Poly>
    {
        private static readonly long serialVersionUID = 1;
        private PolynomialFactorDecomposition(Poly unit, IList<Poly> factors, TIntArrayList exponents) : base(PolynomialRing(unit), unit, factors, exponents)
        {
        }

        private PolynomialFactorDecomposition(FactorDecomposition<Poly> factors) : base(factors.ring, factors.unit, factors.factors, factors.exponents)
        {
        }

        public override bool IsUnit(Poly element)
        {
            return element.IsConstant();
        }

        public override PolynomialFactorDecomposition<Poly> SetUnit(Poly unit)
        {
            base.SetUnit(unit);
            return this;
        }

        public override PolynomialFactorDecomposition<Poly> AddUnit(Poly unit)
        {
            base.AddUnit(unit);
            return this;
        }

        public override PolynomialFactorDecomposition<Poly> AddFactor(Poly factor, int exponent)
        {
            base.AddFactor(factor, exponent);
            return this;
        }

        public override PolynomialFactorDecomposition<Poly> AddAll(FactorDecomposition<Poly> other)
        {
            base.AddAll(other);
            return this;
        }

        public override PolynomialFactorDecomposition<Poly> Canonical()
        {
            if (factors.Count == 0)
                return this;
            ReduceUnitContent();
            Poly[] fTmp = factors.ToArray(factors[0].CreateArray(factors.Count));
            int[] eTmp = exponents.ToArray();
            for (int i = fTmp.Length - 1; i >= 0; --i)
            {
                Poly poly = fTmp[i];
                if (poly.IsMonomial() && eTmp[i] != 1)
                {
                    poly = PolynomialMethods.PolyPow(poly, eTmp[i], true);
                }

                if (poly.SignumOfLC() < 0)
                {
                    poly.Negate();
                    if (eTmp[i] % 2 == 1)
                        unit.Negate();
                }
            }

            ArraysUtil.QuickSort(fTmp, eTmp);
            for (int i = 0; i < fTmp.Length; i++)
            {
                factors[i] = fTmp[i];
                exponents[i] = eTmp[i];
            }

            return this;
        }

        /// <summary>
        /// Makes the lead coefficient of this factorization equal to the l.c. of specified poly via multiplication of this
        /// by appropriate unit
        /// </summary>
        public PolynomialFactorDecomposition<Poly> SetLcFrom(Poly poly)
        {
            Poly u = ring.GetOne();
            for (int i = 0; i < Size(); i++)
                u = u.Multiply(PolynomialMethods.PolyPow(Get(i).LcAsPoly(), GetExponent(i)));
            return SetUnit(PolynomialMethods.DivideExact(poly.LcAsPoly(), u));
        }

        /// <summary>
        /// Makes the lead coefficient of this factorization equal to the l.c. of specified poly via multiplication of this
        /// by appropriate unit
        /// </summary>
        /// <summary>
        /// Resulting lead coefficient
        /// </summary>
        public Poly Lc()
        {
            Poly u = unit.Clone();
            for (int i = 0; i < Size(); i++)
                u = u.Multiply(PolynomialMethods.PolyPow(Get(i).LcAsPoly(), GetExponent(i)));
            return u;
        }

        /// <summary>
        /// Makes the lead coefficient of this factorization equal to the l.c. of specified poly via multiplication of this
        /// by appropriate unit
        /// </summary>
        /// <summary>
        /// Resulting lead coefficient
        /// </summary>
        /// <summary>
        /// Calculates the signum of the polynomial constituted by this decomposition
        /// </summary>
        /// <returns>the signum of the polynomial constituted by this decomposition</returns>
        public int Signum()
        {
            int signum = unit.SignumOfLC();
            for (int i = 0; i < factors.Count; i++)
                signum *= exponents[i] % 2 == 0 ? 1 : factors[i].SignumOfLC();
            return signum;
        }

        /// <summary>
        /// Makes the lead coefficient of this factorization equal to the l.c. of specified poly via multiplication of this
        /// by appropriate unit
        /// </summary>
        /// <summary>
        /// Resulting lead coefficient
        /// </summary>
        /// <summary>
        /// Calculates the signum of the polynomial constituted by this decomposition
        /// </summary>
        /// <returns>the signum of the polynomial constituted by this decomposition</returns>
        /// <summary>
        /// Makes each factor monic (moving leading coefficients to the {@link #unit})
        /// </summary>
        public PolynomialFactorDecomposition<Poly> Monic()
        {
            for (int i = 0; i < factors.Count; i++)
            {
                Poly factor = factors[i];
                AddUnit(PolyPow(factor.LcAsPoly(), exponents[i], false));
                factor = factor.Monic();
            }

            return this;
        }

        /// <summary>
        /// Makes the lead coefficient of this factorization equal to the l.c. of specified poly via multiplication of this
        /// by appropriate unit
        /// </summary>
        /// <summary>
        /// Resulting lead coefficient
        /// </summary>
        /// <summary>
        /// Calculates the signum of the polynomial constituted by this decomposition
        /// </summary>
        /// <returns>the signum of the polynomial constituted by this decomposition</returns>
        /// <summary>
        /// Makes each factor monic (moving leading coefficients to the {@link #unit})
        /// </summary>
        /// <summary>
        /// Makes each factor primitive (moving contents to the {@link #unit})
        /// </summary>
        public PolynomialFactorDecomposition<Poly> Primitive()
        {
            for (int i = 0; i < factors.Count; i++)
            {
                Poly factor = factors[i];
                Poly content = factor.ContentAsPoly();
                AddUnit(PolyPow(content, exponents[i], false));
                factor = factor.DivideByLC(content);
                if (factor.SignumOfLC() < 0)
                {
                    factor.Negate();
                    if (exponents[i] % 2 == 1)
                        unit.Negate();
                }
            }

            return this;
        }

        /// <summary>
        /// Makes the lead coefficient of this factorization equal to the l.c. of specified poly via multiplication of this
        /// by appropriate unit
        /// </summary>
        /// <summary>
        /// Resulting lead coefficient
        /// </summary>
        /// <summary>
        /// Calculates the signum of the polynomial constituted by this decomposition
        /// </summary>
        /// <returns>the signum of the polynomial constituted by this decomposition</returns>
        /// <summary>
        /// Makes each factor monic (moving leading coefficients to the {@link #unit})
        /// </summary>
        /// <summary>
        /// Makes each factor primitive (moving contents to the {@link #unit})
        /// </summary>
        public PolynomialFactorDecomposition<OthPoly> MapTo<OthPoly>(Func<Poly, OthPoly> mapper) where OthPoly : IPolynomial<OthPoly>
        {
            return Of(mapper.Apply(unit), factors.Select(mapper).ToList(), exponents);
        }

        /// <summary>
        /// Makes the lead coefficient of this factorization equal to the l.c. of specified poly via multiplication of this
        /// by appropriate unit
        /// </summary>
        /// <summary>
        /// Resulting lead coefficient
        /// </summary>
        /// <summary>
        /// Calculates the signum of the polynomial constituted by this decomposition
        /// </summary>
        /// <returns>the signum of the polynomial constituted by this decomposition</returns>
        /// <summary>
        /// Makes each factor monic (moving leading coefficients to the {@link #unit})
        /// </summary>
        /// <summary>
        /// Makes each factor primitive (moving contents to the {@link #unit})
        /// </summary>
        /// <summary>
        /// Calls {@link #monic()} if the coefficient ring is field and {@link #primitive()} otherwise
        /// </summary>
        public PolynomialFactorDecomposition<Poly> ReduceUnitContent()
        {
            return unit.IsOverField() ? Monic() : Primitive();
        }

        public override PolynomialFactorDecomposition<Poly> Clone()
        {
            return new PolynomialFactorDecomposition<Poly>(unit.Clone(), factors.Select(f => f.Clone()).ToList(), new TIntArrayList(exponents));
        }

      
        /// <summary>
        /// Unit factorization
        /// </summary>
        public static PolynomialFactorDecomposition<Poly> Unit(Poly unit)
        {
            if (!unit.IsConstant())
                throw new ArgumentException();
            return Empty(unit).AddUnit(unit);
        }

      
        /// <summary>
        /// Empty factorization
        /// </summary>
        public static PolynomialFactorDecomposition<Poly> Empty(Poly factory)
        {
            return new PolynomialFactorDecomposition<Poly>(factory.CreateOne(), [], new TIntArrayList());
        }

    
        /// <summary>
        /// Factor decomposition with specified factors and exponents
        /// </summary>
        /// <param name="unit">the unit coefficient</param>
        /// <param name="factors">the factors</param>
        /// <param name="exponents">the exponents</param>
        public static PolynomialFactorDecomposition<Poly> Of(Poly unit, IList<Poly> factors, TIntArrayList exponents)
        {
            if (factors.Count != exponents.Count)
                throw new ArgumentException();
            PolynomialFactorDecomposition<Poly> r = Empty(unit).AddUnit(unit);
            for (int i = 0; i < factors.Count; i++)
                r.AddFactor(factors[i], exponents[i]);
            return r;
        }

        
        /// <summary>
        /// Factor decomposition with specified factors and exponents
        /// </summary>
        /// <param name="factors">factors</param>
        public static PolynomialFactorDecomposition<Poly> Of(params Poly[] factors)
        {
            if (factors.Length == 0)
                throw new ArgumentException();
            return Of(factors.ToList());
        }

        
        /// <summary>
        /// Factor decomposition with specified factors and exponents
        /// </summary>
        /// <param name="factors">factors</param>
        public static PolynomialFactorDecomposition<Poly> Of(IEnumerable<Poly> factors)
        {
            TObjectIntHashMap<Poly> map = new TObjectIntHashMap();
            var polynomials = factors as Poly[] ?? factors.ToArray();
            foreach (Poly e in polynomials)
                map.AdjustOrPutValue(e, 1, 1);
            List<Poly> l = [];
            TIntArrayList _e = new TIntArrayList();
            map.ForEachEntry((a, b) =>
            {
                l.Add(a);
                _e.Add(b);
                return true;
            });
            return Of(polynomials[0].CreateOne(), l, _e);
        }
    }
}