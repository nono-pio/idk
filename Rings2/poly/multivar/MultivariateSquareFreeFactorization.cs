using System.Numerics;
using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Poly;
using Cc.Redberry.Rings.Poly.Univar;

namespace Cc.Redberry.Rings.Poly.Multivar
{
    /// <summary>
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class MultivariateSquareFreeFactorization
    {
        private MultivariateSquareFreeFactorization()
        {
        }

        /// <summary>
        /// Performs square-free factorization of a {@code poly.
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>square-free decomposition</returns>
        public static PolynomialFactorDecomposition<Poly> SquareFreeFactorization<Term, Poly>(Poly poly) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            if (poly.IsOverFiniteField())
                return SquareFreeFactorizationBernardin(poly);
            else if (MultivariateGCD.IsOverPolynomialRing(poly))
                return MultivariateFactorization.TryNested(poly, MultivariateSquareFreeFactorization.SquareFreeFactorization());
            else if (poly.CoefficientRingCharacteristic().IsZero())
                return SquareFreeFactorizationYunZeroCharacteristics(poly);
            else
                return SquareFreeFactorizationBernardin(poly);
        }

        /// <summary>
        /// Tests whether the given {@code poly} is square free.
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>whether the given {@code poly} is square free</returns>
        public static bool IsSquareFree<Term, Poly>(Poly poly) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            return MultivariateGCD.PolynomialGCD(poly, poly.Derivative()).IsConstant();
        }

        /// <summary>
        /// Returns square-free part of the {@code poly}
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>square free part</returns>
        public static Poly SquareFreePart<Term, Poly>(Poly poly) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            return SquareFreeFactorization(poly).factors.Where((x) => !x.IsMonomial()).Aggregate(poly.CreateOne(),
                (acc,  p)=> acc.Multiply(p));
        }

        private static void AddMonomial<Term, Poly>(PolynomialFactorDecomposition<Poly> decomposition, Poly poly) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            decomposition.AddUnit(poly.LcAsPoly());
            poly = poly.Monic();
            Term term = poly.Lt();
            IMonomialAlgebra<Term> mAlgebra = poly.monomialAlgebra;
            for (int i = 0; i < poly.nVariables; i++)
                if (term.exponents[i] > 0)
                    decomposition.AddFactor(poly.Create(mAlgebra.GetUnitTerm(poly.nVariables)[i] = 1), term.exponents[i]);
        }

        private static PolynomialFactorDecomposition<Poly> FactorUnivariate<Term, Poly>(Poly poly) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            int var = poly.UnivariateVariable();
            return UnivariateSquareFreeFactorization.SquareFreeFactorization(poly.AsUnivariate()).MapTo((p) => AMultivariatePolynomial<Term, Poly>.AsMultivariate((IUnivariatePolynomial)p, poly.nVariables, var, poly.ordering));
        }

        private static Poly[] ReduceContent<Term, Poly>(Poly poly) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            Term monomialContent = poly.MonomialContent();
            poly = poly.DivideOrNull(monomialContent);
            Poly constantContent = poly.ContentAsPoly();
            if (poly.SignumOfLC() < 0)
                constantContent = constantContent.Negate();
            poly = poly.DivideByLC(constantContent);
            return poly.CreateArray(constantContent, poly.Create(monomialContent));
        }

        /// <summary>
        /// Performs square-free factorization of a {@code poly} which coefficient ring has zero characteristic using Yun's
        /// algorithm.
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>square-free decomposition</returns>
        public static PolynomialFactorDecomposition<Poly> SquareFreeFactorizationYunZeroCharacteristics<Term, Poly>(Poly poly)  where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            if (!poly.CoefficientRingCharacteristic().IsZero())
                throw new ArgumentException("Characteristics 0 expected");
            if (poly.IsEffectiveUnivariate())
                return FactorUnivariate(poly);
            Poly original = poly;
            poly = poly.Clone();
            Poly[] content = ReduceContent(poly);
            PolynomialFactorDecomposition<Poly> decomposition = PolynomialFactorDecomposition.Unit(content[0]);
            AddMonomial(decomposition, content[1]);
            SquareFreeFactorizationYun0(poly, decomposition);

            // lc correction
            decomposition.SetLcFrom(original);
            return decomposition;
        }

        private static void SquareFreeFactorizationYun0<Term, Poly>(Poly poly, PolynomialFactorDecomposition<Poly> factorization) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            Poly[] derivative = poly.Derivative();
            Poly gcd = MultivariateGCD.PolynomialGCD(poly, derivative);
            if (gcd.IsConstant())
            {
                factorization.AddFactor(poly, 1);
                return;
            }

            Poly quot = DivideExact(poly, gcd); // safely destroy (cloned) poly (not used further)
            Poly[] dQuot = poly.CreateArray(derivative.Length);
            for (int i = 0; i < derivative.Length; i++)
                dQuot[i] = DivideExact(derivative[i], gcd);
            int i = 0;
            while (!quot.IsConstant())
            {
                ++i;
                Poly[] qd = quot.Derivative();
                for (int j = 0; j < derivative.Length; j++)
                    dQuot[j] = dQuot[j].Subtract(qd[j]);
                Poly factor = MultivariateGCD.PolynomialGCD(quot, dQuot);
                quot = DivideExact(quot, factor); // can destroy quot in divideAndRemainder
                for (int j = 0; j < derivative.Length; j++)
                    dQuot[j] = DivideExact(dQuot[j], factor); // can destroy dQuot in divideAndRemainder
                if (!factor.IsOne())
                    factorization.AddFactor(factor, i);
            }
        }

        /// <summary>
        /// Performs square-free factorization of a {@code poly} which coefficient ring has zero characteristic using
        /// Musser's algorithm.
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>square-free decomposition</returns>
        public static PolynomialFactorDecomposition<Poly> SquareFreeFactorizationMusserZeroCharacteristics<Term, Poly>(Poly poly) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            if (!poly.CoefficientRingCharacteristic().IsZero())
                throw new ArgumentException("Characteristics 0 expected");
            if (poly.IsEffectiveUnivariate())
                return FactorUnivariate(poly);
            poly = poly.Clone();
            Poly[] content = ReduceContent(poly);
            PolynomialFactorDecomposition<Poly> decomposition = PolynomialFactorDecomposition.Unit(content[0]);
            AddMonomial(decomposition, content[1]);
            SquareFreeFactorizationMusserZeroCharacteristics0(poly, decomposition);
            return decomposition;
        }

        private static void SquareFreeFactorizationMusserZeroCharacteristics0<Term, Poly>(Poly poly, PolynomialFactorDecomposition<Poly> factorization) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            Poly[] derivative = poly.Derivative();
            Poly gcd = MultivariateGCD.PolynomialGCD(poly, derivative);
            if (gcd.IsConstant())
            {
                factorization.AddFactor(poly, 1);
                return;
            }

            Poly quot = DivideExact(poly, gcd); // safely destroy (cloned) poly
            int i = 0;
            while (true)
            {
                ++i;
                Poly nextQuot = MultivariateGCD.PolynomialGCD(gcd, quot);
                gcd = DivideExact(gcd, nextQuot); // safely destroy gcd (reassigned)
                Poly factor = DivideExact(quot, nextQuot); // safely destroy quot (reassigned further)
                if (!factor.IsConstant())
                    factorization.AddFactor(factor, i);
                if (nextQuot.IsConstant())
                    break;
                quot = nextQuot;
            }
        }

        /// <summary>
        /// Performs square-free factorization of a {@code poly} over finite field using Bernardin's algorithm (see
        /// Factorization of multivariate polynomials over finite fields, 1999, https://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.34.5310&rep=rep1&type=pdf).
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>square-free decomposition</returns>
        public static PolynomialFactorDecomposition<Poly> SquareFreeFactorizationBernardin<Term, Poly>(Poly poly) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            if (poly.CoefficientRingCharacteristic().IsZero())
                throw new ArgumentException("Positive characteristic expected");
            if (CanConvertToZp64(poly))
                return SquareFreeFactorizationBernardin(AsOverZp64(poly)).MapTo(Conversions64bit.ConvertFromZp64());
            if (poly.IsEffectiveUnivariate())
                return FactorUnivariate(poly);
            poly = poly.Clone();
            Poly[] content = ReduceContent(poly);
            Poly lc = poly.LcAsPoly();
            PolynomialFactorDecomposition<Poly> fct = SquareFreeFactorizationBernardin0(poly);
            AddMonomial(fct, content[1]);
            return fct.AddFactor(content[0], 1).AddFactor(lc, 1);
        }

        /// <summary>
        /// {@code poly} will be destroyed
        /// </summary>
        private static PolynomialFactorDecomposition<Poly> SquareFreeFactorizationBernardin0<Term, Poly>(Poly poly) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            poly.Monic();
            if (poly.IsConstant())
                return PolynomialFactorDecomposition.Unit(poly);
            if (poly.Degree() <= 1)
                return PolynomialFactorDecomposition.Of(poly);
            Poly[] derivative = poly.Derivative();
            if (!Arrays.Stream(derivative).AllMatch(IPolynomial.IsZero()))
            {
                Poly gcd = MultivariateGCD.PolynomialGCD(poly, derivative);
                if (gcd.IsConstant())
                    return PolynomialFactorDecomposition.Of(poly);
                Poly quot = DivideExact(poly, gcd); // can safely destroy poly (not used further)
                PolynomialFactorDecomposition<Poly> result = PolynomialFactorDecomposition.Unit(poly.CreateOne());
                int i = 0;

                //if (!quot.isConstant())
                while (true)
                {
                    ++i;
                    Poly nextQuot = MultivariateGCD.PolynomialGCD(gcd, quot);
                    Poly factor = DivideExact(quot, nextQuot); // can safely destroy quot (reassigned further)
                    if (!factor.IsConstant())
                        result.AddFactor(factor.Monic(), i);
                    gcd = DivideExact(gcd, nextQuot); // can safely destroy gcd
                    if (nextQuot.IsConstant())
                        break;
                    quot = nextQuot;
                }

                if (!gcd.IsConstant())
                {
                    gcd = PRoot(gcd);
                    PolynomialFactorDecomposition<Poly> gcdFactorization = SquareFreeFactorizationBernardin0(gcd);
                    gcdFactorization.RaiseExponents(poly.CoefficientRingCharacteristic().IntValueExact());
                    result.AddAll(gcdFactorization);
                    return result;
                }
                else
                    return result;
            }
            else
            {
                Poly pRoot = PRoot(poly);
                PolynomialFactorDecomposition<Poly> fd = SquareFreeFactorizationBernardin0(pRoot);
                fd.RaiseExponents(poly.CoefficientRingCharacteristic().IntValueExact());
                return fd.SetUnit(poly.CreateOne());
            }
        }

        /// <summary>
        /// p-th root of poly
        /// </summary>
        private static Poly PRoot<Term, Poly>(Poly poly) where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
        {
            if (poly is MultivariatePolynomial)
                return (Poly)PRoot((MultivariatePolynomial)poly);
            else if (poly is MultivariatePolynomialZp64)
                return (Poly)PRoot((MultivariatePolynomialZp64)poly);
            else
                throw new Exception(); //        int modulus = poly.coefficientRingCharacteristic().intValueExact();
                //        MonomialSet<Term> pRoot = new MonomialSet<>(poly.ordering);
                //        for (Term term : poly) {
                //            int[] exponents = term.exponents.clone();
                //            for (int i = 0; i < exponents.length; i++) {
                //                assert exponents[i] % modulus == 0;
                //                exponents[i] = exponents[i] / modulus;
                //            }
                //            poly.add(pRoot, term.setDegreeVector(exponents));
                //        }
                //        return poly.create(pRoot);
        }

        private static MultivariatePolynomial<E> PRoot<E>(MultivariatePolynomial<E> poly)
        {
            Ring<E> ring = poly.ring;

            // p^(m -1) used for computing p-th root of elements
            BigInteger inverseFactor = ring.Cardinality().Divide(ring.Characteristic());
            int modulus = poly.CoefficientRingCharacteristic().IntValueExact();
            MonomialSet<Monomial<E>> pRoot = new MonomialSet<Monomial<E>>(poly.ordering);
            foreach (Monomial<E> term in poly)
            {
                int[] exponents = (int[])term.exponents.Clone();
                for (int i = 0; i < exponents.Length; i++)
                {
                    exponents[i] = exponents[i] / modulus;
                }

                poly.Add(pRoot, new Monomial<E>(exponents, ring.Pow(term.coefficient, inverseFactor)));
            }

            return poly.Create(pRoot);
        }

        private static MultivariatePolynomialZp64 PRoot(MultivariatePolynomialZp64 poly)
        {
            int modulus = MachineArithmetic.SafeToInt(poly.ring.modulus);
            MonomialSet<MonomialZp64> pRoot = new MonomialSet<MonomialZp64>(poly.ordering);
            foreach (MonomialZp64 term in poly)
            {
                int[] exponents = (int[])term.exponents.Clone();
                for (int i = 0; i < exponents.Length; i++)
                {
                    exponents[i] = exponents[i] / modulus;
                }

                poly.Add(pRoot, term.SetDegreeVector(exponents));
            }

            return poly.Create(pRoot);
        }
    }
}