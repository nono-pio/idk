using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Io;
using Cc.Redberry.Rings.Poly;
using Cc.Redberry.Rings.Poly.Univar;
using Cc.Redberry.Rings.Util;

namespace Cc.Redberry.Rings.Poly.Multivar
{
    /// <summary>
    /// Multivariate polynomial over Zp ring with the modulus in the range (0, 2^62) (see {@link
    /// MachineArithmetic#MAX_SUPPORTED_MODULUS}). For details on polynomial data structure and properties see {@link
    /// AMultivariatePolynomial}.
    /// </summary>
    /// <remarks>
    /// @seeAMultivariatePolynomial
    /// @since1.0
    /// </remarks>
    public sealed class MultivariatePolynomialZp64 : AMultivariatePolynomial<MonomialZp64, MultivariatePolynomialZp64>
    {
        private static readonly long serialVersionUID = 1;

        /// <summary>
        /// The ring.
        /// </summary>
        public readonly IntegersZp64 ring;

        private MultivariatePolynomialZp64(int nVariables, IntegersZp64 ring, IComparer<DegreeVector> ordering,
            MonomialSet<MonomialZp64> lMonomialTerms) : base(nVariables, ordering, new IMonomialAlgebra<>.MonomialAlgebraZp64(ring),
            lMonomialTerms)
        {
            this.ring = ring;
        }


        static void Add(Dictionary<DegreeVector, MonomialZp64> polynomial, MonomialZp64 term, IntegersZp64 ring)
        {
            if (term.coefficient == 0)
                return;
            polynomial.Merge(term, term, (o, n) =>
            {
                long r = ring.Add(o.coefficient, n.coefficient);
                if (r == 0)
                    return null;
                else
                    return o.SetCoefficient(r);
            });
        }

        static void Subtract(Dictionary<DegreeVector, MonomialZp64> polynomial, MonomialZp64 term, IntegersZp64 ring)
        {
            Add(polynomial, term.SetCoefficient(ring.Negate(term.coefficient)), ring);
        }


        /// <summary>
        /// Creates multivariate polynomial from a set of monomials
        /// </summary>
        /// <param name="nVariables">number of variables</param>
        /// <param name="ring">the ring</param>
        /// <param name="ordering">term ordering</param>
        /// <param name="terms">the monomials</param>
        /// <returns>multivariate polynomial</returns>
        public static MultivariatePolynomialZp64 Create(int nVariables, IntegersZp64 ring,
            IComparer<DegreeVector> ordering, MonomialSet<MonomialZp64> terms)
        {
            return new MultivariatePolynomialZp64(nVariables, ring, ordering, terms);
        }


        /// <summary>
        /// Creates multivariate polynomial from a list of monomial terms
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="ordering">term ordering</param>
        /// <param name="terms">the monomial terms</param>
        /// <returns>multivariate polynomial</returns>
        public static MultivariatePolynomialZp64 Create(int nVariables, IntegersZp64 ring,
            IComparer<DegreeVector> ordering, IEnumerable<MonomialZp64> terms)
        {
            MonomialSet<MonomialZp64> map = new MonomialSet<MonomialZp64>(ordering);
            foreach (MonomialZp64 term in terms)
                Add(map, term.SetCoefficient(ring.Modulus(term.coefficient)), ring);
            return new MultivariatePolynomialZp64(nVariables, ring, ordering, map);
        }


        /// <summary>
        /// Creates multivariate polynomial from a list of monomial terms
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="ordering">term ordering</param>
        /// <param name="terms">the monomial terms</param>
        /// <returns>multivariate polynomial</returns>
        public static MultivariatePolynomialZp64 Create(int nVariables, IntegersZp64 ring,
            IComparer<DegreeVector> ordering, params MonomialZp64[] terms)
        {
            return Create(nVariables, ring, ordering, terms.ToList());
        }


        /// <summary>
        /// Creates zero polynomial.
        /// </summary>
        /// <param name="nVariables">number of variables</param>
        /// <param name="ring">the ring</param>
        /// <param name="ordering">the ordering</param>
        /// <returns>zero</returns>
        public static MultivariatePolynomialZp64 Zero(int nVariables, IntegersZp64 ring,
            IComparer<DegreeVector> ordering)
        {
            return new MultivariatePolynomialZp64(nVariables, ring, ordering, new MonomialSet<MonomialZp64>(ordering));
        }


        /// <summary>
        /// Creates unit polynomial.
        /// </summary>
        /// <param name="nVariables">number of variables</param>
        /// <param name="ring">the ring</param>
        /// <param name="ordering">the ordering</param>
        /// <returns>unit</returns>
        public static MultivariatePolynomialZp64 One(int nVariables, IntegersZp64 ring,
            IComparer<DegreeVector> ordering)
        {
            return Create(nVariables, ring, ordering, new MonomialZp64(nVariables, 1));
        }


        /// <summary>
        /// Parse multivariate polynomial from string.
        /// </summary>
        /// <param name="string">the string</param>
        /// <param name="ring">the ring</param>
        /// <param name="variables">string variables that should be taken into account. For examples: {@code parse("a", LEX)} and
        ///                  {@code parse("a", LEX, "a", "b")} although give the same mathematical expressions are differ,
        ///                  since the first one is considered as Z[x], while the second as Z[x1,x2]</param>
        /// <returns>multivariate polynomial</returns>
        public static MultivariatePolynomialZp64 Parse(string @string, IntegersZp64 ring, params string[] variables)
        {
            return Parse(@string, ring, MonomialOrder.DEFAULT, variables);
        }


        /// <summary>
        /// Parse multivariate polynomial from string.
        /// </summary>
        /// <param name="string">the string</param>
        /// <param name="ring">the ring</param>
        /// <returns>multivariate polynomial</returns>
        /// <remarks>@deprecateduse #parse(string, ring, ordering, variables)</remarks>
        public static MultivariatePolynomialZp64 Parse(string @string, IntegersZp64 ring)
        {
            return Parse(@string, ring, MonomialOrder.DEFAULT);
        }


        /// <summary>
        /// Parse multivariate polynomial from string.
        /// </summary>
        /// <param name="string">the string</param>
        /// <param name="ring">the ring</param>
        /// <param name="ordering">monomial order</param>
        /// <param name="variables">string variables that should be taken into account. For examples: {@code parse("a", LEX)} and
        ///                  {@code parse("a", LEX, "a", "b")} although give the same mathematical expressions are differ,
        ///                  since the first one is considered as Z[x], while the second as Z[x1,x2]</param>
        /// <returns>multivariate polynomial</returns>
        public static MultivariatePolynomialZp64 Parse(string @string, IntegersZp64 ring,
            IComparer<DegreeVector> ordering, params string[] variables)
        {
            IntegersZp lDomain = ring.AsGenericRing();
            return MultivariatePolynomial<BigInteger>.AsOverZp64(
                MultivariatePolynomial<BigInteger>.Parse(@string, lDomain, ordering, variables));
        }


        /// <summary>
        /// Parse multivariate polynomial from string.
        /// </summary>
        /// <param name="string">the string</param>
        /// <param name="ring">the ring</param>
        /// <param name="ordering">monomial order</param>
        /// <returns>multivariate polynomial</returns>
        /// <remarks>@deprecateduse #parse(string, ring, ordering, variables)</remarks>
        public static MultivariatePolynomialZp64 Parse(string @string, IntegersZp64 ring,
            IComparer<DegreeVector> ordering)
        {
            IntegersZp lDomain = ring.AsGenericRing();
            return MultivariatePolynomial<BigInteger>.AsOverZp64(MultivariatePolynomial<BigInteger>.Parse(@string, lDomain, ordering));
        }


        /// <summary>
        /// Converts univariate polynomial to multivariate.
        /// </summary>
        /// <param name="poly">univariate polynomial</param>
        /// <param name="nVariables">number of variables in the result</param>
        /// <param name="variable">variable that will be used as a primary variable</param>
        /// <param name="ordering">the ordering</param>
        /// <returns>multivariate polynomial</returns>
        public static MultivariatePolynomialZp64 AsMultivariate(UnivariatePolynomialZp64 poly, int nVariables,
            int variable, IComparer<DegreeVector> ordering)
        {
            MonomialSet<MonomialZp64> map = new MonomialSet<MonomialZp64>(ordering);
            for (int i = poly.Degree(); i >= 0; --i)
            {
                if (poly.IsZeroAt(i))
                    continue;
                int[] degreeVector = new int[nVariables];
                degreeVector[variable] = i;
                map.Add(new MonomialZp64(degreeVector, i, poly[i]));
            }

            return new MultivariatePolynomialZp64(nVariables, poly.ring, ordering, map);
        }


        /// <summary>
        /// Converts univariate polynomial to multivariate.
        /// </summary>
        /// <param name="poly">univariate polynomial</param>
        /// <param name="nVariables">number of variables in the result</param>
        /// <param name="variable">variable that will be used as a primary variable</param>
        /// <param name="ordering">the ordering</param>
        /// <returns>multivariate polynomial</returns>
        public override UnivariatePolynomialZp64 AsUnivariate()
        {
            if (IsConstant())
                return UnivariatePolynomialZp64.CreateUnsafe(ring, new long[] { Lc() });
            int[] degrees = DegreesRef();
            int theVar = -1;
            for (int i = 0; i < degrees.Length; i++)
            {
                if (degrees[i] != 0)
                {
                    if (theVar != -1)
                        throw new ArgumentException("not a univariate polynomial: " + this);
                    theVar = i;
                }
            }

            if (theVar == -1)
                throw new InvalidOperationException("Not a univariate polynomial: " + this);
            long[] univarData = new long[degrees[theVar] + 1];
            foreach (MonomialZp64 e in terms)
                univarData[e.exponents[theVar]] = e.coefficient;
            return UnivariatePolynomialZp64.CreateUnsafe(ring, univarData);
        }


        public override MultivariatePolynomial<UnivariatePolynomialZp64> AsOverUnivariate(int variable)
        {
            UnivariatePolynomialZp64 factory = UnivariatePolynomialZp64.Zero(ring);
            UnivariateRing<UnivariatePolynomialZp64> pDomain = new UnivariateRing<UnivariatePolynomialZp64>(factory);
            MonomialSet<Monomial<UnivariatePolynomialZp64>> newData = new MonomialSet<Monomial<UnivariatePolynomialZp64>>(ordering);
            foreach (MonomialZp64 e in terms)
            {
                Monomial<UnivariatePolynomialZp64> eMonomial = new Monomial<UnivariatePolynomialZp64>(e.DvSetZero(variable),
                    factory.CreateMonomial(e.coefficient, e.exponents[variable]));
                MultivariatePolynomial<UnivariatePolynomialZp64>.Add(newData, eMonomial, pDomain);
            }

            return new MultivariatePolynomial<UnivariatePolynomialZp64>(nVariables, pDomain, ordering, newData);
        }


        public override MultivariatePolynomial<UnivariatePolynomialZp64> AsOverUnivariateEliminate(int variable)
        {
            UnivariatePolynomialZp64 factory = UnivariatePolynomialZp64.Zero(ring);
            UnivariateRing<UnivariatePolynomialZp64> pDomain = new UnivariateRing<UnivariatePolynomialZp64>(factory);
            MonomialSet<Monomial<UnivariatePolynomialZp64>> newData = new MonomialSet<Monomial<UnivariatePolynomialZp64>>(ordering);
            foreach (MonomialZp64 e in terms)
            {
                Monomial<UnivariatePolynomialZp64> eMonomial = new Monomial<UnivariatePolynomialZp64>(e.DvWithout(variable),
                    factory.CreateMonomial(e.coefficient, e.exponents[variable]));
                MultivariatePolynomial<UnivariatePolynomialZp64>.Add(newData, eMonomial, pDomain);
            }

            return new MultivariatePolynomial<UnivariatePolynomialZp64>(nVariables - 1, pDomain, ordering, newData);
        }


        /// <summary>
        /// Converts univariate polynomial to multivariate.
        /// </summary>
        /// <param name="poly">univariate polynomial</param>
        /// <param name="nVariables">number of variables in the result</param>
        /// <param name="variable">variable that will be used as a primary variable</param>
        /// <param name="ordering">the ordering</param>
        /// <returns>multivariate polynomial</returns>
        public override MultivariatePolynomial<MultivariatePolynomialZp64> AsOverMultivariate(params int[] variables)
        {
            Ring<MultivariatePolynomialZp64> ring = new MultivariateRing<MultivariatePolynomialZp64>(this);
            MonomialSet<Monomial<MultivariatePolynomialZp64>> terms = new MonomialSet<Monomial<MultivariatePolynomialZp64>>(ordering);
            foreach (MonomialZp64 term in this)
            {
                int[] coeffExponents = new int[nVariables];
                foreach (int var in variables)
                    coeffExponents[var] = term.exponents[var];
                Monomial<MultivariatePolynomialZp64> newTerm = new Monomial<MultivariatePolynomialZp64>(term.DvSetZero(variables),
                    Create(new MonomialZp64(coeffExponents, term.coefficient)));
                MultivariatePolynomial<MultivariatePolynomialZp64>.Add(terms, newTerm, ring);
            }

            return new MultivariatePolynomial<MultivariatePolynomialZp64>(nVariables, ring, ordering, terms);
        }


        public override MultivariatePolynomial<MultivariatePolynomialZp64> AsOverMultivariateEliminate(int[] variables,
            IComparer<DegreeVector> ordering)
        {
            variables = (int[])variables.Clone();
            Array.Sort(variables);
            int[] restVariables = ArraysUtil.IntSetDifference(ArraysUtil.Sequence(nVariables), variables);
            Ring<MultivariatePolynomialZp64> ring =
                new MultivariateRing<MultivariatePolynomialZp64>(Create(variables.Length, new MonomialSet<Monomial<MultivariatePolynomialZp64>>(ordering)));
            MonomialSet<Monomial<MultivariatePolynomialZp64>> terms = new MonomialSet<Monomial<MultivariatePolynomialZp64>>(ordering);
            foreach (MonomialZp64 term in this)
            {
                int i = 0;
                int[] coeffExponents = new int[variables.Length];
                foreach (int var in variables)
                    coeffExponents[i++] = term.exponents[var];
                i = 0;
                int[] termExponents = new int[restVariables.Length];
                foreach (int var in restVariables)
                    termExponents[i++] = term.exponents[var];
                Monomial<MultivariatePolynomialZp64> newTerm = new Monomial<MultivariatePolynomialZp64>(termExponents,
                    Create(variables.Length, this.ring, this.ordering,
                        new MonomialZp64(coeffExponents, term.coefficient)));
                MultivariatePolynomial<MultivariatePolynomialZp64>.Add(terms, newTerm, ring);
            }

            return new MultivariatePolynomial<MultivariatePolynomialZp64>(restVariables.Length, ring, ordering, terms);
        }


        /// <summary>
        /// Converts multivariate polynomial over univariate polynomial ring (Zp[variable][other_variables]) to a
        /// multivariate polynomial over coefficient ring (Zp[all_variables])
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <param name="variable">the variable to insert</param>
        /// <returns>multivariate polynomial over normal coefficient ring</returns>
        public static MultivariatePolynomialZp64 AsNormalMultivariate(
            MultivariatePolynomial<UnivariatePolynomialZp64> poly, int variable)
        {
            IntegersZp64 ring = poly.ring.GetZero().ring;
            int nVariables = poly.nVariables + 1;
            MultivariatePolynomialZp64 result = Zero(nVariables, ring, poly.ordering);
            foreach (Monomial<UnivariatePolynomialZp64> entry in poly.terms)
            {
                UnivariatePolynomialZp64 uPoly = entry.coefficient;
                DegreeVector dv = entry.DvInsert(variable);
                for (int i = 0; i <= uPoly.Degree(); ++i)
                {
                    if (uPoly.IsZeroAt(i))
                        continue;
                    result.Add(new MonomialZp64(dv.DvSet(variable, i), uPoly[i]));
                }
            }

            return result;
        }


        /// <summary>
        /// Converts multivariate polynomial over multivariate polynomial ring to a multivariate polynomial over coefficient
        /// ring
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>multivariate polynomial over normal coefficient ring</returns>
        public static MultivariatePolynomialZp64 AsNormalMultivariate(
            MultivariatePolynomial<MultivariatePolynomialZp64> poly)
        {
            IntegersZp64 ring = poly.ring.GetZero().ring;
            int nVariables = poly.nVariables;
            MultivariatePolynomialZp64 result = Zero(nVariables, ring, poly.ordering);
            foreach (Monomial<MultivariatePolynomialZp64> term in poly.terms)
            {
                MultivariatePolynomialZp64 uPoly = term.coefficient;
                result.Add(uPoly.Clone().Multiply(new MonomialZp64(term.exponents, term.totalDegree, 1)));
            }

            return result;
        }


        /// <summary>
        /// Converts multivariate polynomial over multivariate polynomial ring to a multivariate polynomial over coefficient
        /// ring
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>multivariate polynomial over normal coefficient ring</returns>
        public static MultivariatePolynomialZp64 AsNormalMultivariate(
            MultivariatePolynomial<MultivariatePolynomialZp64> poly, int[] coefficientVariables, int[] mainVariables)
        {
            IntegersZp64 ring = poly.ring.GetZero().ring;
            int nVariables = coefficientVariables.Length + mainVariables.Length;
            MultivariatePolynomialZp64 result = Zero(nVariables, ring, poly.ordering);
            foreach (Monomial<MultivariatePolynomialZp64> term in poly.terms)
            {
                MultivariatePolynomialZp64 coefficient =
                    term.coefficient.JoinNewVariables(nVariables, coefficientVariables);
                Monomial<MultivariatePolynomialZp64> t = term.JoinNewVariables(nVariables, mainVariables);
                result.Add(coefficient.Multiply(new MonomialZp64(t.exponents, t.totalDegree, 1)));
            }

            return result;
        }


        /// <summary>
        /// Returns polynomial over Z formed from the coefficients of this represented in symmetric modular form ({@code
        /// -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <returns>Z[X] version of this with coefficients represented in symmetric modular form ({@code -modulus/2 <= cfx <=
        ///         modulus/2}).</returns>
        public MultivariatePolynomial<BigInteger> AsPolyZSymmetric()
        {
            MonomialSet<Monomial<BigInteger>> bTerms = new MonomialSet<Monomial<BigInteger>>(ordering);
            foreach (MonomialZp64 t in this)
                bTerms.Add(new Monomial<BigInteger>(t.exponents, t.totalDegree,
                    new BigInteger(ring.SymmetricForm(t.coefficient))));
            return new MultivariatePolynomial<BigInteger>(nVariables, Rings.Z, ordering, bTerms);
        }


        /// <summary>
        /// Returns polynomial over Z formed from the coefficients of this
        /// </summary>
        /// <returns>Z[X] version of this</returns>
        public MultivariatePolynomial<BigInteger> AsPolyZ()
        {
            MonomialSet<Monomial<BigInteger>> bTerms = new MonomialSet<Monomial<BigInteger>>(ordering);
            foreach (MonomialZp64 t in this)
                bTerms.Add(t.ToBigMonomial());
            return new MultivariatePolynomial<BigInteger>(nVariables, Rings.Z, ordering, bTerms);
        }


        /// <summary>
        /// Returns polynomial over Z formed from the coefficients of this
        /// </summary>
        /// <returns>Z[X] version of this</returns>
        public MultivariatePolynomial<BigInteger> ToBigPoly()
        {
            MonomialSet<Monomial<BigInteger>> bTerms = new MonomialSet<Monomial<BigInteger>>(ordering);
            foreach (MonomialZp64 t in this)
                bTerms.Add(t.ToBigMonomial());
            return new MultivariatePolynomial<BigInteger>(nVariables, ring.AsGenericRing(), ordering, bTerms);
        }

        /* ============================================ Main methods ============================================ */
        public override MultivariatePolynomialZp64 ContentAsPoly()
        {
            return CreateConstant(Content());
        }


        public override MultivariatePolynomialZp64 LcAsPoly()
        {
            return CreateConstant(Lc());
        }


        public override MultivariatePolynomialZp64 LcAsPoly(IComparer<DegreeVector> ordering)
        {
            return CreateConstant(Lc(ordering));
        }


        public override MultivariatePolynomialZp64 CcAsPoly()
        {
            return CreateConstant(Cc());
        }


        override MultivariatePolynomialZp64 Create(int nVariables, IComparer<DegreeVector> ordering,
            MonomialSet<MonomialZp64> lMonomialTerms)
        {
            return new MultivariatePolynomialZp64(nVariables, ring, ordering, lMonomialTerms);
        }


        public override bool IsOverField()
        {
            return true;
        }


        public override bool IsOverFiniteField()
        {
            return true;
        }


        public override bool IsOverZ()
        {
            return false;
        }


        public override BigInteger CoefficientRingCardinality()
        {
            return new BigInteger(ring.modulus);
        }


        public override BigInteger CoefficientRingCharacteristic()
        {
            return new BigInteger(ring.modulus);
        }


        public override bool IsOverPerfectPower()
        {
            return ring.IsPerfectPower();
        }


        public override BigInteger CoefficientRingPerfectPowerBase()
        {
            return new BigInteger(ring.PerfectPowerBase());
        }


        public override BigInteger CoefficientRingPerfectPowerExponent()
        {
            return new BigInteger(ring.PerfectPowerExponent());
        }

        public override bool SameCoefficientRingWith(MultivariatePolynomialZp64 oth)
        {
            return nVariables == oth.nVariables && ring.Equals(oth.ring);
        }


        public override MultivariatePolynomialZp64 SetCoefficientRingFrom(MultivariatePolynomialZp64 lMonomialTerms)
        {
            return SetRing(lMonomialTerms.ring);
        }


        /// <summary>
        /// release caches
        /// </summary>
        protected override void Release()
        {
            base.Release(); /* add cache in the future */
        }


        /// <summary>
        /// Switches to another ring specified by {@code newModulus}
        /// </summary>
        /// <param name="newModulus">the new modulus</param>
        /// <returns>a copy of this reduced to the ring specified by {@code newModulus}</returns>
        public MultivariatePolynomialZp64 SetRing(long newModulus)
        {
            return SetRing(new IntegersZp64(newModulus));
        }


        /// <summary>
        /// Switches to another ring specified by {@code newDomain}
        /// </summary>
        /// <param name="newDomain">the new ring</param>
        /// <returns>a copy of this reduced to the ring specified by {@code newDomain}</returns>
        public MultivariatePolynomialZp64 SetRing(IntegersZp64 newDomain)
        {
            MonomialSet<MonomialZp64> newData = new MonomialSet<MonomialZp64>(ordering);
            foreach (MonomialZp64 e in terms)
                Add(newData, e.SetCoefficient(newDomain.Modulus(e.coefficient)));
            return new MultivariatePolynomialZp64(nVariables, newDomain, ordering, newData);
        }


        /// <summary>
        /// Switches to another ring specified by {@code newRing}
        /// </summary>
        /// <param name="newRing">the new ring</param>
        /// <returns>a copy of this reduced to the ring specified by {@code newRing}</returns>
        public MultivariatePolynomial<E> SetRing<E>(Ring<E> newRing)
        {
            MonomialSet<Monomial<E>> newData = new MonomialSet<Monomial<E>>(ordering);
            foreach (MonomialZp64 e in terms)
                MultivariatePolynomial<E>.Add(newData, new Monomial<E>(e, newRing.ValueOf(e.coefficient)), newRing);
            return new MultivariatePolynomial<E>(nVariables, newRing, ordering, newData);
        }


        /// <summary>
        /// internal API
        /// </summary>
        public MultivariatePolynomialZp64 SetRingUnsafe(IntegersZp64 newDomain)
        {
            return new MultivariatePolynomialZp64(nVariables, newDomain, ordering, terms);
        }


        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="val">value</param>
        /// <returns>constant polynomial with specified value</returns>
        public MultivariatePolynomialZp64 CreateConstant(long val)
        {
            MonomialSet<MonomialZp64> data = new MonomialSet<MonomialZp64>(ordering);
            if (val != 0)
                data.Add(new MonomialZp64(nVariables, val));
            return new MultivariatePolynomialZp64(nVariables, ring, ordering, data);
        }


        public override MultivariatePolynomialZp64 CreateConstantFromTerm(MonomialZp64 monomial)
        {
            return CreateConstant(monomial.coefficient);
        }


        public override MultivariatePolynomialZp64 CreateZero()
        {
            return CreateConstant(0);
        }


        public override MultivariatePolynomialZp64 CreateOne()
        {
            return CreateConstant(1);
        }


        /// <summary>
        /// Creates linear polynomial of the form {@code cc + lc * variable}
        /// </summary>
        /// <param name="variable">the variable</param>
        /// <param name="cc">the constant coefficient</param>
        /// <param name="lc">the leading coefficient</param>
        /// <returns>linear polynomial {@code cc + lc * variable}</returns>
        public MultivariatePolynomialZp64 CreateLinear(int variable, long cc, long lc)
        {
            MonomialSet<MonomialZp64> data = new MonomialSet<MonomialZp64>(ordering);
            lc = ring.Modulus(lc);
            cc = ring.Modulus(cc);
            if (cc != 0)
                data.Add(new MonomialZp64(nVariables, cc));
            if (lc != 0)
            {
                int[] lcDegreeVector = new int[nVariables];
                lcDegreeVector[variable] = 1;
                data.Add(new MonomialZp64(lcDegreeVector, 1, lc));
            }

            return new MultivariatePolynomialZp64(nVariables, ring, ordering, data);
        }


        public override bool IsMonic()
        {
            return Lc() == 1;
        }


        public override int SignumOfLC()
        {
            return long.Sign(Lc());
        }

        public override bool IsOne()
        {
            if (Size() != 1)
                return false;
            MonomialZp64 lt = terms.First();
            return lt.IsZeroVector() && lt.coefficient == 1;
        }


        public override bool IsUnitCC()
        {
            return Cc() == 1;
        }


        public override bool IsConstant()
        {
            return Size() == 0 || (Size() == 1 && terms.First().IsZeroVector());
        }


        /// <summary>
        /// Returns the leading coefficient of this polynomial that is coefficient of the largest term according to the
        /// ordering.
        /// </summary>
        /// <returns>leading coefficient of this polynomial</returns>
        public long Lc()
        {
            return Lt().coefficient;
        }


        /// <summary>
        /// Returns the leading coefficient of this polynomial with respect to specified ordering
        /// </summary>
        /// <returns>leading coefficient of this polynomial with respect to specified ordering</returns>
        public long Lc(IComparer<DegreeVector> ordering)
        {
            return Lt(ordering).coefficient;
        }


        /// <summary>
        /// Sets the leading coefficient to the specified value
        /// </summary>
        /// <param name="val">new value for the lc</param>
        /// <returns>the leading coefficient to the specified value</returns>
        public MultivariatePolynomialZp64 SetLC(long val)
        {
            if (IsZero())
                return Add(val);
            terms.Add(Lt().SetCoefficient(ring.Modulus(val)));
            Release();
            return this;
        }


        /// <summary>
        /// Returns the constant coefficient of this polynomial.
        /// </summary>
        /// <returns>constant coefficient of this polynomial</returns>
        public long Cc()
        {
            MonomialZp64 zero = new MonomialZp64(nVariables, 0);
            return terms.GetValueOrDefault(zero, zero).coefficient;
        }

        /// <summary>
        /// Returns the content of this polynomial.
        /// </summary>
        /// <returns>content of this polynomial</returns>
        public long Content()
        {
            return Lc(); //        long gcd = -1;
            //        for (MonomialZp64 term : terms) {
            //            if (gcd == -1)
            //                gcd = term.coefficient;
            //            else
            //                gcd = MachineArithmetic.gcd(gcd, term.coefficient);
            //        }
            //        return gcd;
        }


        /// <summary>
        /// Returns array of polynomial coefficients
        /// </summary>
        /// <returns>array of polynomial coefficients</returns>
        public long[] Coefficients()
        {
            return terms.Values.Select((x) => x.coefficient).ToArray();
        }


        public override MultivariatePolynomialZp64 PrimitivePart(int variable)
        {
            return AsNormalMultivariate(AsOverUnivariateEliminate(variable).PrimitivePart(), variable);
        }


        public override UnivariatePolynomialZp64 ContentUnivariate(int variable)
        {
            return AsOverUnivariate(variable).Content();
        }


        public override MultivariatePolynomialZp64 PrimitivePart()
        {
            return Divide(Content());
        }


        public override MultivariatePolynomialZp64 PrimitivePartSameSign()
        {
            return PrimitivePart();
        }


        public override MultivariatePolynomialZp64 DivideByLC(MultivariatePolynomialZp64 other)
        {
            return Divide(other.Lc());
        }


        /// <summary>
        /// Divides this polynomial by a {@code factor}
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this / factor}</returns>
        public MultivariatePolynomialZp64 Divide(long factor)
        {
            if (factor == 1)
                return this;
            return Multiply(ring.Reciprocal(factor)); // <- this is typically faster than the division
        }


        public override MultivariatePolynomialZp64 DivideOrNull(MonomialZp64 monomial)
        {
            if (monomial.IsZeroVector())
                return Divide(monomial.coefficient);
            long reciprocal = ring.Reciprocal(monomial.coefficient);
            MonomialSet<MonomialZp64> map = new MonomialSet<MonomialZp64>(ordering);
            foreach (MonomialZp64 term in terms)
            {
                DegreeVector dv = term.DvDivideOrNull(monomial);
                if (dv == null)
                    return null;
                map.Add(new MonomialZp64(dv, ring.Multiply(term.coefficient, reciprocal)));
            }

            LoadFrom(map);
            Release();
            return this;
        }


        /// <summary>
        /// Makes this polynomial monic
        /// </summary>
        /// <returns>monic this</returns>
        public override MultivariatePolynomialZp64 Monic()
        {
            if (IsMonic())
                return this;
            if (IsZero())
                return this;
            return Divide(Lc());
        }


        public override MultivariatePolynomialZp64 Monic(IComparer<DegreeVector> ordering)
        {
            if (IsZero())
                return this;
            return Divide(Lc(ordering));
        }


        /// <summary>
        /// Sets {@code this} to its monic part (with respect to given ordering) multiplied by the given factor;
        /// </summary>
        public MultivariatePolynomialZp64 Monic(long factor)
        {
            return Multiply(ring.Multiply(ring.Modulus(factor), ring.Reciprocal(Lc())));
        }


        public MultivariatePolynomialZp64 Monic(IComparer<DegreeVector> ordering, long factor)
        {
            return Multiply(ring.Multiply(ring.Modulus(factor), ring.Reciprocal(Lc(ordering))));
        }


        /// <summary>
        /// Sets {@code this} to its monic part (with respect to given ordering) multiplied by the given factor;
        /// </summary>
        public override MultivariatePolynomialZp64 MonicWithLC(MultivariatePolynomialZp64 other)
        {
            if (Lc() == other.Lc())
                return this;
            return Monic(other.Lc());
        }


        public override MultivariatePolynomialZp64 MonicWithLC(IComparer<DegreeVector> ordering,
            MultivariatePolynomialZp64 other)
        {
            long lc = Lc(ordering);
            long olc = other.Lc(ordering);
            if (lc == olc)
                return this;
            return Monic(ordering, olc);
        }


        /// <summary>
        /// Gives a recursive univariate representation of this poly.
        /// </summary>
        public IUnivariatePolynomial ToDenseRecursiveForm()
        {
            if (nVariables == 0)
                throw new ArgumentException("#variables = 0");
            return ToDenseRecursiveForm(nVariables - 1);
        }


        private IUnivariatePolynomial<Poly> ToDenseRecursiveForm<Poly>(int variable) where Poly : IUnivariatePolynomial<Poly>
        {
            if (variable == 0)
                return AsUnivariate();
            UnivariatePolynomial<MultivariatePolynomialZp64> result = AsUnivariateEliminate(variable);
            IUnivariatePolynomial<Poly>[] data = new IUnivariatePolynomial<Poly>[result.Degree() + 1];
            for (int j = 0; j < data.Length; ++j)
                data[j] = result[j].ToDenseRecursiveForm(variable - 1);
            return UnivariatePolynomial.Create(Rings.PolynomialRing(data[0]), data);
        }


        /// <summary>
        /// Converts poly from a recursive univariate representation.
        /// </summary>
        /// <param name="recForm">recursive univariate representation</param>
        /// <param name="ordering">monomial order</param>
        public static MultivariatePolynomialZp64 FromDenseRecursiveForm<Poly>(IUnivariatePolynomial<Poly> recForm,
            IComparer<DegreeVector> ordering) where Poly : IUnivariatePolynomial<Poly>
        {
            // compute number of variables
            int nVariables = 1;
            IUnivariatePolynomial<Poly> p = recForm;
            while (p is UnivariatePolynomial<Poly>)
            {
                p = (IUnivariatePolynomial<Poly>)((UnivariatePolynomial)p).Cc();
                ++nVariables;
            }

            return FromDenseRecursiveForm(recForm, nVariables, ordering);
        }


        /// <summary>
        /// Converts poly from a recursive univariate representation.
        /// </summary>
        /// <param name="recForm">recursive univariate representation</param>
        /// <param name="nVariables">number of variables in multivariate polynomial</param>
        /// <param name="ordering">monomial order</param>
        public static MultivariatePolynomialZp64 FromDenseRecursiveForm<Poly>(IUnivariatePolynomial<Poly> recForm, int nVariables,
            IComparer<DegreeVector> ordering) where Poly : IUnivariatePolynomial<Poly>
        {
            return FromDenseRecursiveForm(recForm, nVariables, ordering, nVariables - 1);
        }


        /// <summary>
        /// Converts poly from a recursive univariate representation.
        /// </summary>
        /// <param name="recForm">recursive univariate representation</param>
        /// <param name="nVariables">number of variables in multivariate polynomial</param>
        /// <param name="ordering">monomial order</param>
        private static MultivariatePolynomialZp64 FromDenseRecursiveForm(IUnivariatePolynomial recForm, int nVariables,
            IComparer<DegreeVector> ordering, int variable)
        {
            if (variable == 0)
                return AsMultivariate((UnivariatePolynomialZp64)recForm, nVariables, 0, ordering);
            UnivariatePolynomial<IUnivariatePolynomial> _recForm = (UnivariatePolynomial<IUnivariatePolynomial>)recForm;
            MultivariatePolynomialZp64[] data = new MultivariatePolynomialZp64[_recForm.Degree() + 1];
            for (int j = 0; j < data.Length; ++j)
                data[j] = FromDenseRecursiveForm(_recForm[j], nVariables, ordering, variable - 1);
            return AsMultivariate(UnivariatePolynomial.Create(Rings.MultivariateRing(data[0]), data), variable);
        }


        /// <summary>
        /// Evaluates polynomial given in a dense recursive form at a given points
        /// </summary>
        public static long EvaluateDenseRecursiveForm(IUnivariatePolynomial recForm, long[] values)
        {
            // compute number of variables
            int nVariables = 1;
            IUnivariatePolynomial p = recForm;
            while (p is UnivariatePolynomial)
            {
                p = (IUnivariatePolynomial)((UnivariatePolynomial)p).Cc();
                ++nVariables;
            }

            if (nVariables != values.Length)
                throw new ArgumentException();
            return EvaluateDenseRecursiveForm(recForm, values, ((UnivariatePolynomialZp64)p).ring, nVariables - 1);
        }


        /// <summary>
        /// Evaluates polynomial given in a dense recursive form at a given points
        /// </summary>
        // compute number of variables
        private static long EvaluateDenseRecursiveForm(IUnivariatePolynomial recForm, long[] values, IntegersZp64 ring,
            int variable)
        {
            if (variable == 0)
                return ((UnivariatePolynomialZp64)recForm).Evaluate(values[0]);
            UnivariatePolynomial<IUnivariatePolynomial> _recForm = (UnivariatePolynomial<IUnivariatePolynomial>)recForm;
            long result = 0;
            for (int i = _recForm.degree(); i >= 0; --i)
                result = ring.Add(ring.Multiply(values[variable], result),
                    EvaluateDenseRecursiveForm(_recForm[i], values, ring, variable - 1));
            return result;
        }


        /// <summary>
        /// Gives a recursive sparse univariate representation of this poly.
        /// </summary>
        public AMultivariatePolynomial ToSparseRecursiveForm()
        {
            if (nVariables == 0)
                throw new ArgumentException("#variables = 0");
            return ToSparseRecursiveForm(nVariables - 1);
        }


        /// <summary>
        /// Gives a recursive sparse univariate representation of this poly.
        /// </summary>
        private AMultivariatePolynomial ToSparseRecursiveForm(int variable)
        {
            if (variable == 0)
            {
                return this.SetNVariables(1);
            }

            MultivariatePolynomial<MultivariatePolynomialZp64> result =
                AsOverMultivariateEliminate(ArraysUtil.Sequence(0, variable), MonomialOrder.GRLEX);
            Monomial<AMultivariatePolynomial>[] data = new Monomial[result.Count == 0 ? 1 : result.Count];
            int j = 0;
            foreach (Monomial<MultivariatePolynomialZp64> term in result.Count == 0
                         ? Collections.SingletonList(result.Lt())
                         : result)
                data[j++] = new Monomial(term, term.coefficient.ToSparseRecursiveForm(variable - 1));
            return MultivariatePolynomial.Create(1, Rings.MultivariateRing(data[0].coefficient), MonomialOrder.GRLEX,
                data);
        }


        /// <summary>
        /// Converts poly from a sparse recursive univariate representation.
        /// </summary>
        /// <param name="recForm">recursive univariate representation</param>
        /// <param name="ordering">monomial order</param>
        public static MultivariatePolynomialZp64 FromSparseRecursiveForm(AMultivariatePolynomial recForm,
            IComparer<DegreeVector> ordering)
        {
            // compute number of variables
            int nVariables = 1;
            AMultivariatePolynomial p = recForm;
            while (p is MultivariatePolynomial)
            {
                p = (AMultivariatePolynomial)((MultivariatePolynomial)p).Cc();
                ++nVariables;
            }

            return FromSparseRecursiveForm(recForm, nVariables, ordering);
        }


        /// <summary>
        /// Converts poly from a recursive univariate representation.
        /// </summary>
        /// <param name="recForm">recursive univariate representation</param>
        /// <param name="nVariables">number of variables in multivariate polynomial</param>
        /// <param name="ordering">monomial order</param>
        public static MultivariatePolynomialZp64 FromSparseRecursiveForm(AMultivariatePolynomial recForm,
            int nVariables, IComparer<DegreeVector> ordering)
        {
            return FromSparseRecursiveForm(recForm, nVariables, ordering, nVariables - 1);
        }


        /// <summary>
        /// Converts poly from a recursive univariate representation.
        /// </summary>
        /// <param name="recForm">recursive univariate representation</param>
        /// <param name="nVariables">number of variables in multivariate polynomial</param>
        /// <param name="ordering">monomial order</param>
        private static MultivariatePolynomialZp64 FromSparseRecursiveForm(AMultivariatePolynomial recForm,
            int nVariables, IComparer<DegreeVector> ordering, int variable)
        {
            if (variable == 0)
            {
                return ((MultivariatePolynomialZp64)recForm).SetNVariables(nVariables).SetOrdering(ordering);
            }

            MultivariatePolynomial<AMultivariatePolynomial> _recForm =
                (MultivariatePolynomial<AMultivariatePolynomial>)recForm;
            Monomial<MultivariatePolynomialZp64>[] data = new Monomial[_recForm.Count == 0 ? 1 : _recForm.Count];
            int j = 0;
            foreach (Monomial<AMultivariatePolynomial> term in _recForm.Count == 0
                         ? Collections.SingletonList(_recForm.Lt())
                         : _recForm)
            {
                int[] exponents = new int[nVariables];
                exponents[variable] = term.totalDegree;
                data[j++] = new Monomial(exponents, term.totalDegree,
                    FromSparseRecursiveForm(term.coefficient, nVariables, ordering, variable - 1));
            }

            MultivariatePolynomial<MultivariatePolynomialZp64> result =
                MultivariatePolynomial.Create(nVariables, Rings.MultivariateRing(data[0].coefficient), ordering, data);
            return AsNormalMultivariate(result);
        }


        /// <summary>
        /// Evaluates polynomial given in a sparse recursive form at a given points
        /// </summary>
        public static long EvaluateSparseRecursiveForm(AMultivariatePolynomial recForm, long[] values)
        {
            // compute number of variables
            int nVariables = 1;
            AMultivariatePolynomial p = recForm;
            TIntArrayList degrees = new TIntArrayList();
            while (p is MultivariatePolynomial)
            {
                p = (AMultivariatePolynomial)((MultivariatePolynomial)p).Cc();
                degrees.Add(p.Degree());
                ++nVariables;
            }

            degrees.Add(p.Degree());
            if (nVariables != values.Length)
                throw new ArgumentException();
            IntegersZp64 ring = ((MultivariatePolynomialZp64)p).ring;
            lPrecomputedPowers[] pp = new lPrecomputedPowers[nVariables];
            for (int i = 0; i < nVariables; ++i)
                pp[i] = new lPrecomputedPowers(Math.Min(degrees[i], MAX_POWERS_CACHE_SIZE), values[i], ring);
            return EvaluateSparseRecursiveForm(recForm, new lPrecomputedPowersHolder(ring, pp), nVariables - 1);
        }


        static long EvaluateSparseRecursiveForm(AMultivariatePolynomial recForm, lPrecomputedPowersHolder ph,
            int variable)
        {
            IntegersZp64 ring = ph.ring;
            if (variable == 0)
            {
                MultivariatePolynomialZp64 _recForm = (MultivariatePolynomialZp64)recForm;
                IEnumerator<MonomialZp64> it = _recForm.terms.DescendingIterator();
                int previousExponent = -1;
                long result = 0;
                while (it.HasNext())
                {
                    MonomialZp64 m = it.Next();
                    result = ring.Add(
                        ring.Multiply(result,
                            ph.Pow(variable, previousExponent == -1 ? 1 : previousExponent - m.totalDegree)),
                        m.coefficient);
                    previousExponent = m.totalDegree;
                }

                if (previousExponent > 0)
                    result = ring.Multiply(result, ph.Pow(variable, previousExponent));
                return result;
            }

            MultivariatePolynomial<AMultivariatePolynomial> _recForm =
                (MultivariatePolynomial<AMultivariatePolynomial>)recForm;
            IEnumerator<Monomial<AMultivariatePolynomial>> it = _recForm.terms.DescendingIterator();
            int previousExponent = -1;
            long result = 0;
            while (it.HasNext())
            {
                Monomial<AMultivariatePolynomial> m = it.Next();
                result = ring.Add(
                    ring.Multiply(result,
                        ph.Pow(variable, previousExponent == -1 ? 1 : previousExponent - m.totalDegree)),
                    EvaluateSparseRecursiveForm(m.coefficient, ph, variable - 1));
                previousExponent = m.totalDegree;
            }

            if (previousExponent > 0)
                result = ring.Multiply(result, ph.Pow(variable, previousExponent));
            return result;
        }


        /// <summary>
        /// Gives data structure for fast Horner-like sparse evaluation of this multivariate polynomial
        /// </summary>
        /// <param name="evaluationVariables">variables which will be substituted</param>
        public HornerFormZp64 GetHornerForm(int[] evaluationVariables)
        {
            int[] evalDegrees = ArraysUtil.Select(DegreesRef(), evaluationVariables);
            MultivariatePolynomial<MultivariatePolynomialZp64> p = AsOverMultivariateEliminate(evaluationVariables);
            Ring<AMultivariatePolynomial> newRing = Rings.PolynomialRing(p.Cc().ToSparseRecursiveForm());
            return new HornerFormZp64(ring, evalDegrees, evaluationVariables.Length,
                p.MapCoefficients(newRing, m => m.ToSparseRecursiveForm()));
        }


        /// <summary>
        /// A representation of multivariate polynomial specifically optimized for fast evaluation of given variables
        /// </summary>
        public sealed class HornerFormZp64
        {
            private readonly IntegersZp64 ring;
            private readonly int nEvalVariables;
            private readonly int[] evalDegrees;
            private readonly MultivariatePolynomial<AMultivariatePolynomial> recForm;

            public HornerFormZp64(IntegersZp64 ring, int[] evalDegrees, int nEvalVariables,
                MultivariatePolynomial<AMultivariatePolynomial> recForm)
            {
                this.ring = ring;
                this.evalDegrees = evalDegrees;
                this.nEvalVariables = nEvalVariables;
                this.recForm = recForm;
            }

            /// <summary>
            /// Substitute given values for evaluation variables (for example, if this is in R[x1,x2,x3,x4] and evaluation
            /// variables are x2 and x4, the result will be a poly in R[x1,x3]).
            /// </summary>
            public MultivariatePolynomialZp64 Evaluate(long[] values)
            {
                if (values.Length != nEvalVariables)
                    throw new ArgumentException();
                lPrecomputedPowers[] pp = new lPrecomputedPowers[nEvalVariables];
                for (int i = 0; i < nEvalVariables; ++i)
                    pp[i] = new lPrecomputedPowers(Math.Min(evalDegrees[i], MAX_POWERS_CACHE_SIZE), values[i], ring);
                return recForm.MapCoefficientsZp64(ring,
                    (p) => EvaluateSparseRecursiveForm(p, new lPrecomputedPowersHolder(ring, pp), nEvalVariables - 1));
            }
        }


        /// <summary>
        /// Returns a copy of this with {@code value} substituted for {@code variable}
        /// </summary>
        /// <param name="variable">the variable</param>
        /// <param name="value">the value</param>
        /// <returns>a new multivariate polynomial with {@code value} substituted for {@code variable} but still with the same
        ///         {@link #nVariables} (though the effective number of variables is {@code nVariables - 1}, compare to
        ///         {@link #eliminate(int, long)})</returns>
        public MultivariatePolynomialZp64 Evaluate(int variable, long value)
        {
            value = ring.Modulus(value);
            if (value == 0)
                return EvaluateAtZero(variable);
            lPrecomputedPowers powers = new lPrecomputedPowers(value, ring);
            return Evaluate(variable, powers);
        }


        /// <summary>
        /// Returns a copy of this with {@code value} substituted for {@code variable}
        /// </summary>
        /// <param name="variable">the variable</param>
        /// <returns>a new multivariate polynomial with {@code value} substituted for {@code variable} but still with the same
        ///         {@link #nVariables} (though the effective number of variables is {@code nVariables - 1}, compare to
        ///         {@link #eliminate(int, long)})</returns>
        MultivariatePolynomialZp64 Evaluate(int variable, lPrecomputedPowers powers)
        {
            if (Degree(variable) == 0)
                return Clone();
            if (powers.value == 0)
                return EvaluateAtZero(variable);
            MonomialSet<MonomialZp64> newData = new MonomialSet<MonomialZp64>(ordering);
            foreach (MonomialZp64 el in terms)
            {
                long val = ring.Multiply(el.coefficient, powers.Pow(el.exponents[variable]));
                Add(newData, el.SetZero(variable).SetCoefficient(val));
            }

            return new MultivariatePolynomialZp64(nVariables, ring, ordering, newData);
        }


        UnivariatePolynomialZp64 EvaluateAtZeroAllExcept(int variable)
        {
            long[] uData = new long[Degree(variable) + 1];
            foreach (MonomialZp64 el in terms)
            {
                if (el.totalDegree != el.exponents[variable])
                    continue;
                int uExp = el.exponents[variable];
                uData[uExp] = ring.Add(uData[uExp], el.coefficient);
            }

            return UnivariatePolynomialZp64.CreateUnsafe(ring, uData);
        }


        /// <summary>
        /// Returns a copy of this with {@code values} substituted for {@code variables}
        /// </summary>
        /// <param name="variables">the variables</param>
        /// <param name="values">the values</param>
        /// <returns>a new multivariate polynomial with {@code value} substituted for {@code variable} but still with the same
        ///         {@link #nVariables} (though the effective number of variables is {@code nVariables - 1}, compare to
        ///         {@link #eliminate(int, long)})</returns>
        public MultivariatePolynomialZp64 Evaluate(int[] variables, long[] values)
        {
            foreach (long value in values)
                if (value != 0)
                    return Evaluate(MkPrecomputedPowers(variables, values), variables);

            // <- all values are zero
            return EvaluateAtZero(variables);
        }


        /// <summary>
        /// substitutes {@code values} for {@code variables}
        /// </summary>
        MultivariatePolynomialZp64 Evaluate(lPrecomputedPowersHolder powers, int[] variables)
        {
            MonomialSet<MonomialZp64> newData = new MonomialSet<MonomialZp64>(ordering);
            foreach (MonomialZp64 el in terms)
            {
                MonomialZp64 r = el;
                long value = el.coefficient;
                foreach (int variable in variables)
                    value = ring.Multiply(value, powers.Pow(variable, el.exponents[variable]));
                r = r.SetZero(variables).SetCoefficient(value);
                Add(newData, r);
            }

            return new MultivariatePolynomialZp64(nVariables, ring, ordering, newData);
        }


        /// <summary>
        /// Evaluates this polynomial at specified points
        /// </summary>
        public long Evaluate(params long[] values)
        {
            if (values.Length != nVariables)
                throw new ArgumentException();
            if (nVariables == 1 && MonomialOrder.IsGradedOrder(ordering))

                // use Horner scheme in simple cases
                return EvaluateSparseRecursiveForm(this, values);
            return Evaluate(ArraysUtil.Sequence(0, nVariables), values).Cc();
        }


        /// <summary>
        /// Evaluates this polynomial at specified points
        /// </summary>
        public MultivariatePolynomialZp64[] Evaluate(int variable, params long[] values)
        {
            return values.Select((p) => Evaluate(variable, p))
                .ToArray();
        }


        /// <summary>
        /// Substitutes {@code value} for {@code variable} and eliminates {@code variable} from the list of variables so that
        /// the resulting polynomial has {@code result.nVariables = this.nVariables - 1}.
        /// </summary>
        /// <param name="variable">the variable</param>
        /// <param name="value">the value</param>
        /// <returns>a new multivariate polynomial with {@code value} substituted for {@code variable} and  {@code nVariables
        ///         = nVariables - 1})</returns>
        public MultivariatePolynomialZp64 Eliminate(int variable, long value)
        {
            value = ring.Modulus(value);
            MonomialSet<MonomialZp64> newData = new MonomialSet<MonomialZp64>(ordering);
            lPrecomputedPowers powers = new lPrecomputedPowers(value, ring);
            foreach (MonomialZp64 el in terms)
            {
                long val = ring.Multiply(el.coefficient, powers.Pow(el.exponents[variable]));
                Add(newData, el.Without(variable).SetCoefficient(val));
            }

            return new MultivariatePolynomialZp64(nVariables - 1, ring, ordering, newData);
        }


        public MultivariatePolynomialZp64 Eliminate(int[] variables, long[] values)
        {
            foreach (long value in values)
                if (value != 0)
                    return Eliminate(MkPrecomputedPowers(variables, values), variables);

            // <- all values are zero
            return EvaluateAtZero(variables).DropVariables(variables);
        }


        MultivariatePolynomialZp64 Eliminate(lPrecomputedPowersHolder powers, int[] variables)
        {
            MonomialSet<MonomialZp64> newData = new MonomialSet<MonomialZp64>(ordering);
            foreach (MonomialZp64 el in terms)
            {
                MonomialZp64 r = el;
                long value = el.coefficient;
                foreach (int variable in variables)
                    value = ring.Multiply(value, powers.Pow(variable, el.exponents[variable]));
                r = r.Without(variables).SetCoefficient(value);
                Add(newData, r);
            }

            return new MultivariatePolynomialZp64(nVariables - variables.Length, ring, ordering, newData);
        }


        /// <summary>
        /// the default cache size for precomputed powers
        /// </summary>
        static readonly int DEFAULT_POWERS_CACHE_SIZE = 64;


        /// <summary>
        /// the maximal cache size for precomputed powers
        /// </summary>
        static readonly int MAX_POWERS_CACHE_SIZE = 1014;


        /// <summary>
        /// cached powers used to save some time
        /// </summary>
        public sealed class lPrecomputedPowers
        {
            public readonly long value;
            public readonly IntegersZp64 ring;
            public readonly long[] precomputedPowers;

            public lPrecomputedPowers(long value, IntegersZp64 ring) : this(DEFAULT_POWERS_CACHE_SIZE, value, ring)
            {
            }

            public lPrecomputedPowers(int cacheSize, long value, IntegersZp64 ring)
            {
                this.value = ring.Modulus(value);
                this.ring = ring;
                this.precomputedPowers = new long[cacheSize + 1];
                Array.Fill(precomputedPowers, -1);
            }

            public long Pow(int exponent)
            {
                if (exponent >= precomputedPowers.Length)
                    return ring.PowMod(value, exponent);
                if (precomputedPowers[exponent] != -1)
                    return precomputedPowers[exponent];
                long result = 1;
                long k2p = value;
                int rExp = 0, kExp = 1;
                for (;;)
                {
                    if ((exponent & 1) != 0)
                        precomputedPowers[rExp += kExp] = result = ring.Multiply(result, k2p);
                    exponent = exponent >> 1;
                    if (exponent == 0)
                        return precomputedPowers[rExp] = result;
                    precomputedPowers[kExp *= 2] = k2p = ring.Multiply(k2p, k2p);
                }
            }
        }


        public lPrecomputedPowersHolder MkPrecomputedPowers(int variable, long value)
        {
            lPrecomputedPowers[] pp = new lPrecomputedPowers[nVariables];
            pp[variable] = new lPrecomputedPowers(Math.Min(Degree(variable), MAX_POWERS_CACHE_SIZE), value, ring);
            return new lPrecomputedPowersHolder(ring, pp);
        }


        public lPrecomputedPowersHolder MkPrecomputedPowers(int[] variables, long[] values)
        {
            int[] degrees = DegreesRef();
            lPrecomputedPowers[] pp = new lPrecomputedPowers[nVariables];
            for (int i = 0; i < variables.Length; ++i)
                pp[variables[i]] = new lPrecomputedPowers(Math.Min(degrees[variables[i]], MAX_POWERS_CACHE_SIZE),
                    values[i], ring);
            return new lPrecomputedPowersHolder(ring, pp);
        }


        public static lPrecomputedPowersHolder MkPrecomputedPowers(int nVariables, IntegersZp64 ring, int[] variables,
            long[] values)
        {
            lPrecomputedPowers[] pp = new lPrecomputedPowers[nVariables];
            for (int i = 0; i < variables.Length; ++i)
                pp[variables[i]] = new lPrecomputedPowers(MAX_POWERS_CACHE_SIZE, values[i], ring);
            return new lPrecomputedPowersHolder(ring, pp);
        }


        public lPrecomputedPowersHolder MkPrecomputedPowers(long[] values)
        {
            if (values.Length != nVariables)
                throw new ArgumentException();
            int[] degrees = DegreesRef();
            lPrecomputedPowers[] pp = new lPrecomputedPowers[nVariables];
            for (int i = 0; i < nVariables; ++i)
                pp[i] = new lPrecomputedPowers(Math.Min(degrees[i], MAX_POWERS_CACHE_SIZE), values[i], ring);
            return new lPrecomputedPowersHolder(ring, pp);
        }


        /// <summary>
        /// holds an array of precomputed powers
        /// </summary>
        public sealed class lPrecomputedPowersHolder
        {
            readonly IntegersZp64 ring;
            readonly lPrecomputedPowers[] powers;

            public lPrecomputedPowersHolder(IntegersZp64 ring, lPrecomputedPowers[] powers)
            {
                this.ring = ring;
                this.powers = powers;
            }

            public void Set(int i, long point)
            {
                if (powers[i] == null || powers[i].value != point)
                    powers[i] = new lPrecomputedPowers(
                        powers[i] == null ? DEFAULT_POWERS_CACHE_SIZE : powers[i].precomputedPowers.Length, point,
                        ring);
            }

            public long Pow(int variable, int exponent)
            {
                return powers[variable].Pow(exponent);
            }

            public lPrecomputedPowersHolder Clone()
            {
                return new lPrecomputedPowersHolder(ring, powers.Clone());
            }
        }


        /// <summary>
        /// Returns a copy of this with {@code poly} substituted for {@code variable}
        /// </summary>
        /// <param name="variable">the variable</param>
        /// <param name="poly">the replacement for the variable</param>
        /// <returns>a copy of this with  {@code variable -> poly}</returns>
        public MultivariatePolynomialZp64 Substitute(int variable, MultivariatePolynomialZp64 poly)
        {
            if (poly.IsConstant())
                return Evaluate(variable, poly.Cc());
            lPrecomputedSubstitution subsPowers;
            if (poly.IsEffectiveUnivariate())
                subsPowers = new lUSubstitution(poly.AsUnivariate(), poly.UnivariateVariable(), nVariables, ordering);
            else
                subsPowers = new lMSubstitution(poly);
            MultivariatePolynomialZp64 result = CreateZero();
            foreach (MonomialZp64 term in this)
            {
                int exponent = term.exponents[variable];
                if (exponent == 0)
                {
                    result.Add(term);
                    continue;
                }

                result.Add(subsPowers.Pow(exponent).Multiply(term.SetZero(variable)));
            }

            return result;
        }


        /// <summary>
        /// Returns a copy of this with {@code variable -> variable + shift}
        /// </summary>
        /// <param name="variable">the variable</param>
        /// <param name="shift">shift amount</param>
        /// <returns>a copy of this with  {@code variable -> variable + shift}</returns>
        public MultivariatePolynomialZp64 Shift(int variable, long shift)
        {
            if (shift == 0)
                return Clone();
            lUSubstitution shifts = new lUSubstitution(UnivariatePolynomialZ64.Create(shift, 1).Modulus(ring), variable,
                nVariables, ordering);
            MultivariatePolynomialZp64 result = CreateZero();
            foreach (MonomialZp64 term in this)
            {
                int exponent = term.exponents[variable];
                if (exponent == 0)
                {
                    result.Add(term);
                    continue;
                }

                result.Add(shifts.Pow(exponent).Multiply(term.SetZero(variable)));
            }

            return result;
        }


        /// <summary>
        /// Substitutes {@code variable -> variable + shift} for each variable from {@code variables} array
        /// </summary>
        /// <param name="variables">the variables</param>
        /// <param name="shifts">the corresponding shifts</param>
        /// <returns>a copy of this with  {@code variable -> variable + shift}</returns>
        public MultivariatePolynomialZp64 Shift(int[] variables, long[] shifts)
        {
            lPrecomputedSubstitution[] powers = new lPrecomputedSubstitution[nVariables];
            bool allShiftsAreZero = true;
            for (int i = 0; i < variables.Length; ++i)
            {
                if (shifts[i] != 0)
                    allShiftsAreZero = false;
                powers[variables[i]] =
                    new lUSubstitution(UnivariatePolynomialZ64.Create(shifts[i], 1).Modulus(ring, false), variables[i],
                        nVariables, ordering);
            }

            if (allShiftsAreZero)
                return Clone();
            lPrecomputedSubstitutions calculatedShifts = new lPrecomputedSubstitutions(powers);
            MultivariatePolynomialZp64 result = CreateZero();
            foreach (MonomialZp64 term in this)
            {
                MultivariatePolynomialZp64 temp = CreateOne();
                foreach (int variable in variables)
                {
                    if (term.exponents[variable] != 0)
                    {
                        temp = temp.Multiply(calculatedShifts.GetSubstitutionPower(variable, term.exponents[variable]));
                        term = term.SetZero(variable);
                    }
                }

                if (temp.IsOne())
                {
                    result.Add(term);
                    continue;
                }

                result.Add(temp.Multiply(term));
            }

            return result;
        }

        sealed class lPrecomputedSubstitutions
        {
            readonly lPrecomputedSubstitution[] subs;

            public lPrecomputedSubstitutions(lPrecomputedSubstitution[] subs)
            {
                this.subs = subs;
            }

            public MultivariatePolynomialZp64 GetSubstitutionPower(int var, int exponent)
            {
                if (subs[var] == null)
                    throw new ArgumentException();
                return subs[var].Pow(exponent);
            }
        }


        interface lPrecomputedSubstitution
        {
            MultivariatePolynomialZp64 Pow(int exponent);
        }


        sealed class lUSubstitution : lPrecomputedSubstitution
        {
            readonly int variable;
            readonly int nVariables;
            readonly IComparer<DegreeVector> ordering;
            readonly UnivariatePolynomialZp64 @base;
            readonly TIntObjectHashMap<UnivariatePolynomialZp64> uCache = new TIntObjectHashMap();
            readonly TIntObjectHashMap<MultivariatePolynomialZp64> mCache = new TIntObjectHashMap();

            public lUSubstitution(UnivariatePolynomialZp64 @base, int variable, int nVariables,
                IComparer<DegreeVector> ordering)
            {
                this.nVariables = nVariables;
                this.variable = variable;
                this.ordering = ordering;
                this.@base = @base;
            }

            public MultivariatePolynomialZp64 Pow(int exponent)
            {
                MultivariatePolynomialZp64 cached = mCache[exponent];
                if (cached != null)
                    return cached.Clone();
                UnivariatePolynomialZp64 r = PolynomialMethods.PolyPow(@base, exponent, true, uCache);
                mCache.Put(exponent, cached = AsMultivariate(r, nVariables, variable, ordering));
                return cached.Clone();
            }
        }


        sealed class lMSubstitution : lPrecomputedSubstitution
        {
            readonly MultivariatePolynomialZp64 @base;
            readonly TIntObjectHashMap<MultivariatePolynomialZp64> cache = new TIntObjectHashMap();

            public lMSubstitution(MultivariatePolynomialZp64 @base)
            {
                this.@base = @base;
            }

            public MultivariatePolynomialZp64 Pow(int exponent)
            {
                return PolynomialMethods.PolyPow(@base, exponent, true, cache);
            }
        }


        override void Add(MonomialSet<MonomialZp64> terms, MonomialZp64 term)
        {
            Add(terms, term, ring);
        }


        override void Subtract(MonomialSet<MonomialZp64> terms, MonomialZp64 term)
        {
            Subtract(terms, term, ring);
        }


        /// <summary>
        /// Adds {@code oth} to this polynomial and returns it
        /// </summary>
        /// <param name="oth">other polynomial</param>
        /// <returns>{@code this + oth}</returns>
        public MultivariatePolynomialZp64 Add(long oth)
        {
            oth = ring.Modulus(oth);
            if (oth == 0)
                return this;
            Add(terms, new MonomialZp64(nVariables, oth));
            Release();
            return this;
        }


        /// <summary>
        /// Subtracts {@code oth} from this polynomial and returns it
        /// </summary>
        /// <param name="oth">other polynomial</param>
        /// <returns>{@code this - oth}</returns>
        public MultivariatePolynomialZp64 Subtract(long oth)
        {
            return Add(ring.Negate(ring.Modulus(oth)));
        }


        /// <summary>
        /// Subtracts {@code oth} from this polynomial and returns it
        /// </summary>
        /// <param name="oth">other polynomial</param>
        /// <returns>{@code this - oth}</returns>
        public override MultivariatePolynomialZp64 Increment()
        {
            return Add(1);
        }


        public override MultivariatePolynomialZp64 Decrement()
        {
            return Subtract(1);
        }


        public override MultivariatePolynomialZp64 Multiply(long factor)
        {
            factor = ring.Modulus(factor);
            if (factor == 1)
                return this;
            if (factor == 0)
                return ToZero();
            IEnumerator<Entry<DegreeVector, MonomialZp64>> it = terms.EntrySet().Iterator();
            while (it.HasNext())
            {
                Entry<DegreeVector, MonomialZp64> entry = it.Next();
                MonomialZp64 term = entry.GetValue();
                long val = ring.Multiply(term.coefficient, factor);
                if (val == 0)
                    it.Remove();
                else
                    entry.SetValue(term.SetCoefficient(val));
            }

            Release();
            return this;
        }


        public override MultivariatePolynomialZp64 MultiplyByLC(MultivariatePolynomialZp64 other)
        {
            return Multiply(other.Lc());
        }


        public override MultivariatePolynomialZp64 MultiplyByBigInteger(BigInteger factor)
        {
            return Multiply(factor.Mod(new BigInteger(ring.modulus)).LongValueExact());
        }


        public override MultivariatePolynomialZp64 Multiply(MonomialZp64 monomial)
        {
            CheckSameDomainWith(monomial);
            if (monomial.IsZeroVector())
                return Multiply(monomial.coefficient);
            if (monomial.coefficient == 0)
                return ToZero();
            MonomialSet<MonomialZp64> newMap = new MonomialSet<MonomialZp64>(ordering);
            foreach (MonomialZp64 thisElement in terms)
            {
                MonomialZp64 mul = monomialAlgebra.Multiply(thisElement, monomial);
                if (mul.coefficient != 0)
                    newMap.Add(mul);
            }

            return LoadFrom(newMap);
        }


        public override MultivariatePolynomialZp64 Multiply(MultivariatePolynomialZp64 oth)
        {
            AssertSameCoefficientRingWith(oth);
            if (oth.IsZero())
                return ToZero();
            if (IsZero())
                return this;
            if (oth.IsConstant())
                return Multiply(oth.Cc());
            if (Size() > KRONECKER_THRESHOLD && oth.Size() > KRONECKER_THRESHOLD)
                return MultiplyKronecker(oth);
            else
                return MultiplyClassic(oth);
        }


        private MultivariatePolynomialZp64 MultiplyClassic(MultivariatePolynomialZp64 oth)
        {
            MonomialSet<MonomialZp64> newMap = new MonomialSet<MonomialZp64>(ordering);
            foreach (MonomialZp64 othElement in oth.terms)
            foreach (MonomialZp64 thisElement in terms)
                Add(newMap, monomialAlgebra.Multiply(thisElement, othElement), ring);
            return LoadFrom(newMap);
        }


        private MultivariatePolynomialZp64 MultiplyKronecker(MultivariatePolynomialZp64 oth)
        {
            int[] resultDegrees = new int[nVariables];
            int[] thisDegrees = DegreesRef();
            int[] othDegrees = oth.DegreesRef();
            for (int i = 0; i < resultDegrees.Length; i++)
                resultDegrees[i] = thisDegrees[i] + othDegrees[i];
            long[] map = KroneckerMap(resultDegrees);
            if (map == null)
                return MultiplyClassic(oth);

            // check that degrees fit long
            double threshold = 0;
            for (int i = 0; i < nVariables; i++)
                threshold += 1 * resultDegrees[i] * map[i];
            threshold *= 2;
            if (threshold > long.MaxValue)
                return MultiplyClassic(oth);
            return FromKronecker(MultiplySparseUnivariate(ring, ToKronecker(map), oth.ToKronecker(map)), map);
        }


        /// <summary>
        /// Convert to Kronecker's representation
        /// </summary>
        private long[,] ToKronecker(long[] kroneckerMap)
        {
            long[,] result = new long[Size(), 2];
            int j = 0;
            foreach (MonomialZp64 term in this)
            {
                long exponent = term.exponents[0];
                for (int i = 1; i < term.exponents.Length; i++)
                    exponent += term.exponents[i] * kroneckerMap[i];
                result[j,0] = exponent;
                result[j,1] = term.coefficient;
                ++j;
            }

            return result;
        }


        private static TLongObjectHashMap<CfHolder> MultiplySparseUnivariate(IntegersZp64 ring, long[,] a, long[,] b)
        {
            TLongObjectHashMap<CfHolder> result = new TLongObjectHashMap(a.Length + b.Length);
            foreach (long[] ai in a)
            {
                foreach (long[] bi in b)
                {
                    long deg = ai[0] + bi[0];
                    long val = ring.Multiply(ai[1], bi[1]);
                    CfHolder r = result[deg];
                    if (r != null)
                        r.coefficient = ring.Add(r.coefficient, val);
                    else
                        result.Put(deg, new CfHolder(val));
                }
            }

            return result;
        }


        private MultivariatePolynomialZp64 FromKronecker(TLongObjectHashMap<CfHolder> p, long[] kroneckerMap)
        {
            terms.Clear();
            TLongObjectIterator<CfHolder> it = p.Iterator();
            while (it.HasNext())
            {
                it.Advance();
                if (it.Value().coefficient == 0)
                    continue;
                long exponent = it.Key();
                int[] exponents = new int[nVariables];
                for (int i = 0; i < nVariables; i++)
                {
                    long div = exponent / kroneckerMap[nVariables - i - 1];
                    exponent = exponent - (div * kroneckerMap[nVariables - i - 1]);
                    exponents[nVariables - i - 1] = MachineArithmetic.SafeToInt(div);
                }

                terms.Add(new MonomialZp64(exponents, it.Value().coefficient));
            }

            Release();
            return this;
        }


        sealed class CfHolder
        {
            long coefficient = 0;

            CfHolder(long coefficient)
            {
                this.coefficient = coefficient;
            }
        }


        public override MultivariatePolynomialZp64 Square()
        {
            return Multiply(this);
        }


        public override MultivariatePolynomialZp64 EvaluateAtRandom(int variable, Random rnd)
        {
            return Evaluate(variable, ring.RandomElement(rnd));
        }


        public override MultivariatePolynomialZp64 EvaluateAtRandomPreservingSkeleton(int variable, Random rnd)
        {
            //desired skeleton
            HashSet<DegreeVector> skeleton = GetSkeletonExcept(variable);
            MultivariatePolynomialZp64 tmp;
            do
            {
                long randomPoint = ring.RandomElement(rnd);
                tmp = Evaluate(variable, randomPoint);
            } while (!skeleton.Equals(tmp.GetSkeleton()));

            return tmp;
        }


        //desired skeleton
        public override MultivariatePolynomialZp64 Derivative(int variable, int order)
        {
            if (order == 0)
                return this.Clone();
            if (IsConstant())
                return CreateZero();
            MonomialSet<MonomialZp64> newTerms = new MonomialSet<MonomialZp64>(ordering);
            foreach (MonomialZp64 term in this)
            {
                int exponent = term.exponents[variable];
                if (exponent < order)
                    continue;
                long newCoefficient = term.coefficient;
                for (int i = 0; i < order; ++i)
                    newCoefficient = ring.Multiply(newCoefficient, exponent - i);
                int[] newExponents = (int[])term.exponents.Clone();
                newExponents[variable] -= order;
                Add(newTerms, new MonomialZp64(newExponents, term.totalDegree - order, newCoefficient));
            }

            return new MultivariatePolynomialZp64(nVariables, ring, ordering, newTerms);
        }


        /// <summary>
        /// exp * (exp - 1) * ... * (exp - order + 1) / (1 * 2 * ... * order) mod modulus
        /// </summary>
        static long SeriesCoefficientFactor(int exponent, int order, IntegersZp64 ring)
        {
            IntegersZp64 baseDomain = ring.PerfectPowerBaseDomain();
            if (order < baseDomain.modulus)
            {
                long factor = 1;
                for (int i = 0; i < order; ++i)
                    factor = ring.Multiply(factor, exponent - i);
                factor = ring.Divide(factor, ring.Factorial(order));
                return factor;
            }

            long numerator = 1, denominator = 1;
            int numZeros = 0, denZeros = 0;
            for (int i = 1; i <= order; ++i)
            {
                long num = exponent - i + 1, numMod = baseDomain.Modulus(num);
                while (num > 1 && numMod == 0)
                {
                    num = FastDivision.DivideSignedFast(num, baseDomain.magic);
                    numMod = baseDomain.Modulus(num);
                    ++numZeros;
                }

                if (numMod != 0)
                    numerator = ring.Multiply(numerator, ring == baseDomain ? numMod : ring.Modulus(num));
                long den = i, denMod = baseDomain.Modulus(i);
                while (den > 1 && denMod == 0)
                {
                    den = FastDivision.DivideSignedFast(den, baseDomain.magic);
                    denMod = baseDomain.Modulus(den);
                    ++denZeros;
                }

                if (denMod != 0)
                    denominator = ring.Multiply(denominator, ring == baseDomain ? denMod : ring.Modulus(den));
            }

            if (numZeros > denZeros)
                numerator = ring.Multiply(numerator, ring.PowMod(baseDomain.modulus, numZeros - denZeros));
            else if (denZeros < numZeros)
                denominator = ring.Multiply(denominator, ring.PowMod(baseDomain.modulus, denZeros - numZeros));
            if (numerator == 0)
                return numerator;
            return ring.Divide(numerator, denominator);
        }


        public override MultivariatePolynomialZp64 SeriesCoefficient(int variable, int order)
        {
            if (order == 0)
                return this.Clone();
            if (IsConstant())
                return CreateZero();
            MonomialSet<MonomialZp64> newTerms = new MonomialSet<MonomialZp64>(ordering);
            foreach (MonomialZp64 term in this)
            {
                int exponent = term.exponents[variable];
                if (exponent < order)
                    continue;
                int[] newExponents = (int[])term.exponents.Clone();
                newExponents[variable] -= order;
                long newCoefficient = ring.Multiply(term.coefficient, SeriesCoefficientFactor(exponent, order, ring));
                Add(newTerms, new MonomialZp64(newExponents, term.totalDegree - order, newCoefficient));
            }

            return new MultivariatePolynomialZp64(nVariables, ring, ordering, newTerms);
        }


        /// <summary>
        /// Maps terms of this using specified mapping function
        /// </summary>
        /// <param name="newRing">the new ring</param>
        /// <param name="mapper">mapping</param>
        /// <param name="<T>">new element type</param>
        /// <returns>a new polynomial with terms obtained by applying mapper to this terms</returns>
        public MultivariatePolynomial<T> MapTerms<T>(Ring<T> newRing, Func<MonomialZp64, Monomial<T>> mapper)
        {
            return terms.Values.Select(mapper)
                .Collect(new PolynomialCollector(() => MultivariatePolynomial.Zero(nVariables, newRing, ordering)));
        }


        /// <summary>
        /// Maps terms of this using specified mapping function
        /// </summary>
        /// <param name="newRing">the new ring</param>
        /// <param name="mapper">mapping</param>
        /// <returns>a new polynomial with terms obtained by applying mapper to this terms</returns>
        public MultivariatePolynomialZp64 MapTerms(IntegersZp64 newRing, Func<MonomialZp64, MonomialZp64> mapper)
        {
            return terms.Values.Select(mapper)
                .Collect(new PolynomialCollector(() => MultivariatePolynomialZp64.Zero(nVariables, newRing, ordering)));
        }


        /// <summary>
        /// Maps coefficients of this using specified mapping function
        /// </summary>
        /// <param name="newRing">the new ring</param>
        /// <param name="mapper">mapping</param>
        /// <param name="<T>">new element type</param>
        /// <returns>a new polynomial with terms obtained by applying mapper to this terms (only coefficients are changed)</returns>
        public MultivariatePolynomial<T> MapCoefficients<T>(Ring<T> newRing, LongFunction<T> mapper)
        {
            return MapTerms(newRing, (t) => new Monomial<T>(t.exponents, t.totalDegree, mapper.Apply(t.coefficient)));
        }

        public override MultivariatePolynomial<E> MapCoefficientsAsPolys<E>(Ring<E> ring, Func<MultivariatePolynomialZp64, E> mapper)
        {
            return MapCoefficients(ring, ((cf) => mapper(CreateConstant(cf))));
        }


        public override int CompareTo(MultivariatePolynomialZp64 oth)
        {
            int c = Size().CompareTo(oth.Size());
            if (c != 0)
                return c;
            IEnumerator<MonomialZp64> thisIt = Iterator(), othIt = oth.Iterator();
            while (thisIt.MoveNext() && othIt.MoveNext())
            {
                MonomialZp64 a = thisIt.Current, b = othIt.Current;
                if ((c = ordering.Compare(a, b)) != 0)
                    return c;
                if ((c = a.coefficient.CompareTo(b.coefficient)) != 0)
                    return c;
            }

            return 0;
        }


        public override MultivariatePolynomialZp64 Clone()
        {
            return new MultivariatePolynomialZp64(nVariables, ring, ordering, terms.Clone());
        }


        public override MultivariatePolynomialZp64 ParsePoly(string @string)
        {
            MultivariatePolynomialZp64 r = Parse(@string, ring, ordering);
            if (r.nVariables != nVariables)
                return Parse(@string, ring, ordering, IStringifier<MultivariatePolynomialZp64>.DefaultVars(nVariables));
            return r;
        }


        public override string ToString(IStringifier<MultivariatePolynomialZp64> stringifier)
        {
            if (IsConstant())
                return Cc().ToString();
            string[] varStrings = new string[nVariables];
            for (int i = 0; i < nVariables; ++i)
                varStrings[i] = stringifier.GetBindings()
                    .GetValueOrDefault(CreateMonomial(i, 1), IStringifier<MultivariatePolynomialZp64>.DefaultVar(i, nVariables));
            StringBuilder sb = new StringBuilder();
            foreach (MonomialZp64 term in terms)
            {
                long cf = term.coefficient;
                string cfString;
                if (cf != 1 || term.totalDegree == 0)
                    cfString = cf.ToString();
                else
                    cfString = "";
                if (sb.Length != 0 && !cfString.StartsWith("-"))
                    sb.Append("+");
                StringBuilder cfBuilder = new StringBuilder();
                cfBuilder.Append(cfString);
                for (int i = 0; i < nVariables; ++i)
                {
                    if (term.exponents[i] == 0)
                        continue;
                    if (cfBuilder.Length != 0)
                        cfBuilder.Append("*");
                    cfBuilder.Append(varStrings[i]);
                    if (term.exponents[i] > 1)
                        cfBuilder.Append("^").Append(term.exponents[i]);
                }

                sb.Append(cfBuilder);
            }

            return sb.ToString();
        }


        public override string CoefficientRingToString(IStringifier<MultivariatePolynomialZp64> stringifier)
        {
            return ring.ToString();
        }
    }
}