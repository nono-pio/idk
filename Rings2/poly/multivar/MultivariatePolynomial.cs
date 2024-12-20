using Cc.Redberry.Rings;
using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Io;
using Cc.Redberry.Rings.Poly;
using Cc.Redberry.Rings.Poly.Univar;
using Cc.Redberry.Rings.Util;
using Gnu.Trove.Iterator;
using Gnu.Trove.List.Array;
using Gnu.Trove.Map.Hash;
using Org.Apache.Commons.Math3.Random;
using Java;
using Java.Util.Map;
using Java.Util.Function;
using Java.Util.Regex;
using Java.Util.Stream;
using Cc.Redberry.Rings.Poly.Multivar.MultivariatePolynomialZp64;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Cc.Redberry.Rings.Poly.Multivar.RoundingMode;
using static Cc.Redberry.Rings.Poly.Multivar.Associativity;
using static Cc.Redberry.Rings.Poly.Multivar.Operator;
using static Cc.Redberry.Rings.Poly.Multivar.TokenType;
using static Cc.Redberry.Rings.Poly.Multivar.SystemInfo;

namespace Cc.Redberry.Rings.Poly.Multivar
{
    /// <summary>
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class MultivariatePolynomial<E> : AMultivariatePolynomial<Monomial<E>, MultivariatePolynomial<E>>
    {
        private static readonly long serialVersionUID = 1;

        /// <summary>
        /// The coefficient ring
        /// </summary>
        public readonly Ring<E> ring;

        public MultivariatePolynomial(int nVariables, Ring<E> ring, IComparer<DegreeVector> ordering,
            MonomialSet<Monomial<E>> terms) : base(nVariables, ordering, new MonomialAlgebra(ring), terms)
        {
            this.ring = ring;
        }

        /* ============================================ Factory methods ============================================ */
        static void Add<E>(Dictionary<DegreeVector, Monomial<E>> polynomial, Monomial<E> term, Ring<E> ring)
        {
            if (ring.IsZero(term.coefficient))
                return;
            polynomial.Merge(term, term, (o, n) =>
            {
                E r = ring.Add(o.coefficient, n.coefficient);
                if (ring.IsZero(r))
                    return null;
                else
                    return o.SetCoefficient(r);
            });
        }

        /// <summary>
        /// The coefficient ring
        /// </summary>
        /* ============================================ Factory methods ============================================ */
        static void Subtract<E>(Dictionary<DegreeVector, Monomial<E>> polynomial, Monomial<E> term, Ring<E> ring)
        {
            Add(polynomial, term.SetCoefficient(ring.Negate(term.coefficient)), ring);
        }


        /// <param name="ring">the ring</param>
        /// <param name="ordering">term ordering</param>
        /// <param name="terms">the monomial terms</param>
        /// <returns>multivariate polynomial</returns>
        public static MultivariatePolynomial<E> Create<E>(int nVariables, Ring<E> ring,
            IComparer<DegreeVector> ordering, Iterable<Monomial<E>> terms)
        {
            MonomialSet<Monomial<E>> map = new MonomialSet(ordering);
            foreach (Monomial<E> term in terms)
                Add(map, term.SetCoefficient(ring.ValueOf(term.coefficient)), ring);
            return new MultivariatePolynomial(nVariables, ring, ordering, map);
        }


        /// <summary>
        /// Creates multivariate polynomial from a list of monomial terms
        /// </summary>
        /// <param name="ring">the ring</param>
        /// <param name="ordering">term ordering</param>
        /// <param name="terms">the monomial terms</param>
        /// <returns>multivariate polynomial</returns>
        public static MultivariatePolynomial<E> Create<E>(int nVariables, Ring<E> ring,
            IComparer<DegreeVector> ordering, params Monomial<E>[] terms)
        {
            return Create(nVariables, ring, ordering, Arrays.AsList(terms));
        }

        /// <summary>
        /// Creates zero polynomial.
        /// </summary>
        /// <param name="nVariables">number of variables</param>
        /// <param name="ring">the ring</param>
        /// <param name="ordering">the ordering</param>
        /// <returns>zero polynomial</returns>
        public static MultivariatePolynomial<E> Zero<E>(int nVariables, Ring<E> ring, IComparer<DegreeVector> ordering)
        {
            return new MultivariatePolynomial(nVariables, ring, ordering, new MonomialSet(ordering));
        }

        /// <summary>
        /// Creates unit polynomial.
        /// </summary>
        /// <param name="nVariables">number of variables</param>
        /// <param name="ring">the ring</param>
        /// <param name="ordering">the ordering</param>
        /// <returns>unit polynomial</returns>
        public static MultivariatePolynomial<E> One<E>(int nVariables, Ring<E> ring, IComparer<DegreeVector> ordering)
        {
            return Create(nVariables, ring, ordering, new Monomial(nVariables, ring.GetOne()));
        }


        /// <summary>
        /// Parse multivariate Z[X] polynomial from string.
        /// </summary>
        /// <param name="string">the string</param>
        /// <param name="variables">string variables that should be taken into account. For examples: {@code parse("a", LEX)} and
        ///                  {@code parse("a", LEX, "a", "b")} although give the same mathematical expressions are differ,
        ///                  since the first one is considered as Z[x], while the second as Z[x1,x2]</param>
        /// <returns>multivariate Z[X] polynomial</returns>
        public static MultivariatePolynomial<BigInteger> Parse(string @string, params string[] variables)
        {
            return Parse(@string, Rings.Z, variables);
        }

        /// <summary>
        /// Parse multivariate Z[X] polynomial from string.
        /// </summary>
        /// <param name="string">the string</param>
        /// <returns>multivariate Z[X] polynomial</returns>
        /// <remarks>@deprecateduse #parse(string, ring, ordering, variables)</remarks>
        public static MultivariatePolynomial<BigInteger> Parse(string @string)
        {
            return Parse(@string, GuessVariableStrings(@string));
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
        public static MultivariatePolynomial<E> Parse<E>(string @string, Ring<E> ring, params string[] variables)
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
        public static MultivariatePolynomial<E> Parse<E>(string @string, Ring<E> ring)
        {
            return Parse(@string, ring, GuessVariableStrings(@string));
        }

        /// <summary>
        /// Parse multivariate Z[X] polynomial from string.
        /// </summary>
        /// <param name="string">the string</param>
        /// <param name="ordering">monomial order</param>
        /// <param name="variables">string variables that should be taken into account. For examples: {@code parse("a", LEX)} and
        ///                  {@code parse("a", LEX, "a", "b")} although give the same mathematical expressions are differ,
        ///                  since the first one is considered as Z[x], while the second as Z[x1,x2]</param>
        /// <returns>Z[X] multivariate polynomial</returns>
        public static MultivariatePolynomial<BigInteger> Parse(string @string, IComparer<DegreeVector> ordering,
            params string[] variables)
        {
            return Parse(@string, Rings.Z, ordering, variables);
        }


        /// <summary>
        /// Parse multivariate Z[X] polynomial from string.
        /// </summary>
        /// <param name="string">the string</param>
        /// <param name="ordering">monomial order</param>
        /// <returns>Z[X] multivariate polynomial</returns>
        /// <remarks>@deprecateduse #parse(string, ring, ordering, variables)</remarks>
        public static MultivariatePolynomial<BigInteger> Parse(string @string, IComparer<DegreeVector> ordering)
        {
            return Parse(@string, Rings.Z, ordering, GuessVariableStrings(@string));
        }


        /// <summary>
        /// Parse multivariate polynomial from string.
        /// </summary>
        /// <param name="string">the string</param>
        /// <param name="ring">the ring</param>
        /// <param name="ordering">monomial order</param>
        /// <param name="variables">string variables that should be taken into account. For examples: {@code parse("a", LEX)} and
        ///                  {@code parse("a", LEX, "a", "b")} although give the same mathematical expressions are different,
        ///                  since the first one is considered as Z[a], while the second as Z[a,b]</param>
        /// <returns>multivariate polynomial</returns>
        public static MultivariatePolynomial<E> Parse<E>(string @string, Ring<E> ring,
            IComparer<DegreeVector> ordering, params string[] variables)
        {
            return Coder.MkMultivariateCoder(Rings.MultivariateRing(variables.Length, ring, ordering), variables)
                .Parse(@string);
        }


        /// <summary>
        /// Parse multivariate polynomial from string.
        /// </summary>
        /// <param name="string">the string</param>
        /// <param name="ring">the ring</param>
        /// <param name="ordering">monomial order</param>
        /// <remarks>@deprecateduse #parse(string, ring, ordering, variables)</remarks>
        public static MultivariatePolynomial<E> Parse<E>(string @string, Ring<E> ring,
            IComparer<DegreeVector> ordering)
        {
            return Parse(@string, ring, ordering, GuessVariableStrings(@string));
        }


        /// <summary>
        /// Parse multivariate polynomial from string.
        /// </summary>
        /// <param name="string">the string</param>
        /// <param name="ring">the ring</param>
        /// <param name="ordering">monomial order</param>
        /// <remarks>@deprecateduse #parse(string, ring, ordering, variables)</remarks>
        private static String[] GuessVariableStrings(string @string)
        {
            Matcher matcher = Pattern.Compile("[a-zA-Z]+[0-9]*").Matcher(@string);
            IList<string> variables = new List();
            HashSet<string> seen = new HashSet();
            while (matcher.Find())
            {
                string var = matcher.Group();
                if (seen.Contains(var))
                    continue;
                seen.Add(var);
                variables.Add(var);
            }

            variables.Sort(string.CompareTo());
            return variables.ToArray(new string[variables.Count]);
        }


        /// <summary>
        /// Converts multivariate polynomial over BigIntegers to multivariate polynomial over machine modular integers
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>multivariate polynomial over machine sized modular integers</returns>
        /// <exception cref="IllegalArgumentException">if poly.ring is not Zp</exception>
        /// <exception cref="ArithmeticException">if some of coefficients will not exactly fit in a {@code long}.</exception>
        public static MultivariatePolynomialZp64 AsOverZp64(MultivariatePolynomial<BigInteger> poly)
        {
            if (!(poly.ring is IntegersZp))
                throw new ArgumentException("Poly is not over modular ring: " + poly.ring);
            IntegersZp ring = (IntegersZp)poly.ring;
            MonomialSet<MonomialZp64> terms = new MonomialSet(poly.ordering);
            foreach (Monomial<BigInteger> term in poly.terms)
                terms.Add(new MonomialZp64(term.exponents, term.totalDegree, term.coefficient.LongValueExact()));
            return MultivariatePolynomialZp64.Create(poly.nVariables, ring.AsMachineRing(), poly.ordering, terms);
        }


        /// <summary>
        /// Converts multivariate polynomial over BigIntegers to multivariate polynomial over machine modular integers
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <param name="ring">Zp64 ring</param>
        /// <returns>multivariate polynomial over machine sized modular integers</returns>
        /// <exception cref="IllegalArgumentException">if poly.ring is not Zp</exception>
        /// <exception cref="ArithmeticException">if some of coefficients will not exactly fit in a {@code long}.</exception>
        public static MultivariatePolynomialZp64 AsOverZp64(MultivariatePolynomial<BigInteger> poly, IntegersZp64 ring)
        {
            MonomialSet<MonomialZp64> terms = new MonomialSet(poly.ordering);
            BigInteger modulus = BigInteger.ValueOf(ring.modulus);
            foreach (Monomial<BigInteger> term in poly.terms)
                terms.Add(new MonomialZp64(term.exponents, term.totalDegree,
                    term.coefficient.Mod(modulus).LongValueExact()));
            return MultivariatePolynomialZp64.Create(poly.nVariables, ring, poly.ordering, terms);
        }


        /// <summary>
        /// Converts univariate polynomial to multivariate.
        /// </summary>
        /// <param name="poly">univariate polynomial</param>
        /// <param name="nVariables">number of variables in the result</param>
        /// <param name="variable">variable that will be used as a primary variable</param>
        /// <param name="ordering">monomial order</param>
        /// <returns>multivariate polynomial</returns>
        public static MultivariatePolynomial<E> AsMultivariate<E>(UnivariatePolynomial<E> poly, int nVariables,
            int variable, IComparer<DegreeVector> ordering)
        {
            MonomialSet<Monomial<E>> map = new MonomialSet(ordering);
            for (int i = poly.degree(); i >= 0; --i)
            {
                if (poly.IsZeroAt(i))
                    continue;
                int[] degreeVector = new int[nVariables];
                degreeVector[variable] = i;
                map.Add(new Monomial(degreeVector, i, poly[i]));
            }

            return new MultivariatePolynomial(nVariables, poly.ring, ordering, map);
        }


        public override UnivariatePolynomial<E> AsUnivariate()
        {
            if (IsConstant())
                return UnivariatePolynomial.Constant(ring, Lc());
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
            E[] univarData = ring.CreateZeroesArray(degrees[theVar] + 1);
            foreach (Monomial<E> e in terms)
                univarData[e.exponents[theVar]] = e.coefficient;
            return UnivariatePolynomial.CreateUnsafe(ring, univarData);
        }


        public override MultivariatePolynomial<UnivariatePolynomial<E>> AsOverUnivariate(int variable)
        {
            UnivariatePolynomial<E> factory = UnivariatePolynomial.Zero(ring);
            UnivariateRing<UnivariatePolynomial<E>> pDomain = new UnivariateRing(factory);
            MonomialSet<Monomial<UnivariatePolynomial<E>>> newData = new MonomialSet(ordering);
            foreach (Monomial<E> e in terms)
            {
                Add(newData,
                    new Monomial(e.DvSetZero(variable), factory.CreateMonomial(e.coefficient, e.exponents[variable])),
                    pDomain);
            }

            return new MultivariatePolynomial(nVariables - 1, pDomain, ordering, newData);
        }


        public override MultivariatePolynomial<UnivariatePolynomial<E>> AsOverUnivariateEliminate(int variable)
        {
            UnivariatePolynomial<E> factory = UnivariatePolynomial.Zero(ring);
            UnivariateRing<UnivariatePolynomial<E>> pDomain = new UnivariateRing(factory);
            MonomialSet<Monomial<UnivariatePolynomial<E>>> newData = new MonomialSet(ordering);
            foreach (Monomial<E> e in terms)
            {
                Add(newData,
                    new Monomial(e.DvWithout(variable), factory.CreateMonomial(e.coefficient, e.exponents[variable])),
                    pDomain);
            }

            return new MultivariatePolynomial(nVariables - 1, pDomain, ordering, newData);
        }


        public override MultivariatePolynomial<MultivariatePolynomial<E>> AsOverMultivariate(params int[] variables)
        {
            Ring<MultivariatePolynomial<E>> ring = new MultivariateRing(this);
            MonomialSet<Monomial<MultivariatePolynomial<E>>> terms = new MonomialSet(ordering);
            foreach (Monomial<E> term in this)
            {
                int[] coeffExponents = new int[nVariables];
                foreach (int var in variables)
                    coeffExponents[var] = term.exponents[var];
                Monomial<MultivariatePolynomial<E>> newTerm = new Monomial(term.DvSetZero(variables),
                    Create(new Monomial(coeffExponents, ArraysUtil.Sum(coeffExponents), term.coefficient)));
                Add(terms, newTerm, ring);
            }

            return new MultivariatePolynomial(nVariables, ring, ordering, terms);
        }


        public override MultivariatePolynomial<MultivariatePolynomial<E>> AsOverMultivariateEliminate(int[] variables,
            IComparer<DegreeVector> ordering)
        {
            variables = variables.Clone();
            Arrays.Sort(variables);
            int[] restVariables = ArraysUtil.IntSetDifference(ArraysUtil.Sequence(nVariables), variables);
            Ring<MultivariatePolynomial<E>> ring =
                new MultivariateRing(Create(variables.Length, new MonomialSet(ordering)));
            MonomialSet<Monomial<MultivariatePolynomial<E>>> terms = new MonomialSet(ordering);
            foreach (Monomial<E> term in this)
            {
                int i = 0;
                int[] coeffExponents = new int[variables.Length];
                foreach (int var in variables)
                    coeffExponents[i++] = term.exponents[var];
                i = 0;
                int[] termExponents = new int[restVariables.Length];
                foreach (int var in restVariables)
                    termExponents[i++] = term.exponents[var];
                Monomial<MultivariatePolynomial<E>> newTerm = new Monomial(termExponents,
                    Create(variables.Length, this.ring, this.ordering, new Monomial(coeffExponents, term.coefficient)));
                Add(terms, newTerm, ring);
            }

            return new MultivariatePolynomial(restVariables.Length, ring, ordering, terms);
        }


        /// <summary>
        /// Converts multivariate polynomial over univariate polynomial ring (R[variable][other_variables]) to a multivariate
        /// polynomial over coefficient ring (R[variables])
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <param name="variable">the variable to insert</param>
        /// <returns>multivariate polynomial over normal coefficient ring</returns>
        public static MultivariatePolynomial<E> AsNormalMultivariate<E>(
            MultivariatePolynomial<UnivariatePolynomial<E>> poly, int variable)
        {
            Ring<E> ring = poly.ring.GetZero().ring;
            int nVariables = poly.nVariables + 1;
            MultivariatePolynomial<E> result = Zero(nVariables, ring, poly.ordering);
            foreach (Monomial<UnivariatePolynomial<E>> entry in poly.terms)
            {
                UnivariatePolynomial<E> uPoly = entry.coefficient;
                DegreeVector dv = entry.DvInsert(variable);
                for (int i = 0; i <= uPoly.Degree(); ++i)
                {
                    if (uPoly.IsZeroAt(i))
                        continue;
                    result.Add(new Monomial(dv.DvSet(variable, i), uPoly[i]));
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
        public static MultivariatePolynomial<E> AsNormalMultivariate<E>(
            MultivariatePolynomial<MultivariatePolynomial<E>> poly)
        {
            Ring<E> ring = poly.ring.GetZero().ring;
            int nVariables = poly.nVariables;
            MultivariatePolynomial<E> result = Zero(nVariables, ring, poly.ordering);
            foreach (Monomial<MultivariatePolynomial<E>> term in poly.terms)
            {
                MultivariatePolynomial<E> uPoly = term.coefficient;
                result.Add(uPoly.Clone().Multiply(new Monomial(term.exponents, term.totalDegree, ring.GetOne())));
            }

            return result;
        }


        /// <summary>
        /// Converts multivariate polynomial over multivariate polynomial ring to a multivariate polynomial over coefficient
        /// ring
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <returns>multivariate polynomial over normal coefficient ring</returns>
        public static MultivariatePolynomial<E> AsNormalMultivariate<E>(
            MultivariatePolynomial<MultivariatePolynomial<E>> poly, int[] coefficientVariables, int[] mainVariables)
        {
            Ring<E> ring = poly.ring.GetZero().ring;
            int nVariables = coefficientVariables.Length + mainVariables.Length;
            MultivariatePolynomial<E> result = Zero(nVariables, ring, poly.ordering);
            foreach (Monomial<MultivariatePolynomial<E>> term in poly.terms)
            {
                MultivariatePolynomial<E> coefficient =
                    term.coefficient.JoinNewVariables(nVariables, coefficientVariables);
                Monomial<MultivariatePolynomial<E>> t = term.JoinNewVariables(nVariables, mainVariables);
                result.Add(coefficient.Multiply(new Monomial(t.exponents, t.totalDegree, ring.GetOne())));
            }

            return result;
        }


        /// <summary>
        /// Returns Z[X] polynomial formed from the coefficients of the poly.
        /// </summary>
        /// <param name="poly">the polynomial</param>
        /// <param name="copy">whether to copy the internal data</param>
        /// <returns>Z[X] version of the poly</returns>
        public static MultivariatePolynomial<BigInteger> AsPolyZ(MultivariatePolynomial<BigInteger> poly, bool copy)
        {
            return new MultivariatePolynomial(poly.nVariables, Rings.Z, poly.ordering,
                copy ? poly.terms.Clone() : poly.terms);
        }


        /// <summary>
        /// Converts Zp[x] polynomial to Z[x] polynomial formed from the coefficients of this represented in symmetric
        /// modular form ({@code -modulus/2 <= cfx <= modulus/2}).
        /// </summary>
        /// <param name="poly">Zp polynomial</param>
        /// <returns>Z[x] version of the poly with coefficients represented in symmetric modular form ({@code -modulus/2 <=
        ///         cfx <= modulus/2}).</returns>
        /// <exception cref="IllegalArgumentException">is {@code poly.ring} is not a {@link IntegersZp}</exception>
        public static MultivariatePolynomial<BigInteger> AsPolyZSymmetric(MultivariatePolynomial<BigInteger> poly)
        {
            if (!(poly.ring is IntegersZp))
                throw new ArgumentException("Not a modular ring: " + poly.ring);
            IntegersZp ring = (IntegersZp)poly.ring;
            MonomialSet<Monomial<BigInteger>> newTerms = new MonomialSet(poly.ordering);
            foreach (Monomial<BigInteger> term in poly)
                newTerms.Add(term.SetCoefficient(ring.SymmetricForm(term.coefficient)));
            return new MultivariatePolynomial(poly.nVariables, Rings.Z, poly.ordering, newTerms);
        }


        /* ============================================ Main methods ============================================ */
        public override MultivariatePolynomial<E> ContentAsPoly()
        {
            return CreateConstant(Content());
        }


        public override MultivariatePolynomial<E> LcAsPoly()
        {
            return CreateConstant(Lc());
        }


        public override MultivariatePolynomial<E> LcAsPoly(IComparer<DegreeVector> ordering)
        {
            return CreateConstant(Lc(ordering));
        }


        public override MultivariatePolynomial<E> CcAsPoly()
        {
            return CreateConstant(Cc());
        }


        override MultivariatePolynomial<E> Create(int nVariables, IComparer<DegreeVector> ordering,
            MonomialSet<Monomial<E>> monomialTerms)
        {
            return new MultivariatePolynomial(nVariables, ring, ordering, monomialTerms);
        }


        public override bool IsOverField()
        {
            return ring.IsField();
        }


        public override bool IsOverFiniteField()
        {
            return ring.IsFiniteField();
        }


        public override bool IsOverZ()
        {
            return ring.Equals(Rings.Z);
        }


        public override BigInteger CoefficientRingCardinality()
        {
            return ring.Cardinality();
        }


        public override BigInteger CoefficientRingCharacteristic()
        {
            return ring.Characteristic();
        }


        public override bool IsOverPerfectPower()
        {
            return ring.IsPerfectPower();
        }


        public override BigInteger CoefficientRingPerfectPowerBase()
        {
            return ring.PerfectPowerBase();
        }


        public override BigInteger CoefficientRingPerfectPowerExponent()
        {
            return ring.PerfectPowerExponent();
        }


        public override MultivariatePolynomial<E>[] CreateArray(int length)
        {
            return new MultivariatePolynomial[length];
        }


        public override MultivariatePolynomial<E>[][] CreateArray2d(int length)
        {
            return new MultivariatePolynomial[length];
        }


        public override MultivariatePolynomial<E>[][] CreateArray2d(int length1, int length2)
        {
            return new MultivariatePolynomial[length1, length2];
        }


        public override bool SameCoefficientRingWith(MultivariatePolynomial<E> oth)
        {
            return nVariables == oth.nVariables && ring.Equals(oth.ring);
        }


        public override MultivariatePolynomial<E> SetCoefficientRingFrom(MultivariatePolynomial<E> poly)
        {
            return SetRing(poly.ring);
        }


        /// <summary>
        /// release caches
        /// </summary>
        protected override void Release()
        {
            base.Release(); /* add cache in the future */
        }


        /// <summary>
        /// Returns a copy of this with coefficient reduced to a {@code newRing}
        /// </summary>
        /// <param name="newRing">the new ring</param>
        /// <returns>a copy of this reduced to the ring specified by {@code newRing}</returns>
        public MultivariatePolynomial<E> SetRing(Ring<E> newRing)
        {
            if (ring == newRing)
                return Clone();
            MonomialSet<Monomial<E>> newData = new MonomialSet(ordering);
            foreach (Monomial<E> e in terms)
                Add(newData, e.SetCoefficient(newRing.ValueOf(e.coefficient)));
            return new MultivariatePolynomial(nVariables, newRing, ordering, newData);
        }


        /// <summary>
        /// internal API
        /// </summary>
        public MultivariatePolynomial<E> SetRingUnsafe(Ring<E> newRing)
        {
            return new MultivariatePolynomial(nVariables, newRing, ordering, terms);
        }


        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="val">value</param>
        /// <returns>constant polynomial with specified value</returns>
        public MultivariatePolynomial<E> CreateConstant(E val)
        {
            MonomialSet<Monomial<E>> data = new MonomialSet(ordering);
            if (!ring.IsZero(val))
                data.Add(new Monomial(nVariables, val));
            return new MultivariatePolynomial(nVariables, ring, ordering, data);
        }


        public override MultivariatePolynomial<E> CreateConstantFromTerm(Monomial<E> monomial)
        {
            return CreateConstant(monomial.coefficient);
        }


        public override MultivariatePolynomial<E> CreateZero()
        {
            return CreateConstant(ring.GetZero());
        }


        public override MultivariatePolynomial<E> CreateOne()
        {
            return CreateConstant(ring.GetOne());
        }


        /// <summary>
        /// Creates linear polynomial of the form {@code cc + lc * variable}
        /// </summary>
        /// <param name="variable">the variable</param>
        /// <param name="cc">the constant coefficient</param>
        /// <param name="lc">the leading coefficient</param>
        /// <returns>linear polynomial {@code cc + lc * variable}</returns>
        public MultivariatePolynomial<E> CreateLinear(int variable, E cc, E lc)
        {
            MonomialSet<Monomial<E>> data = new MonomialSet(ordering);
            if (!ring.IsZero(cc))
                data.Add(new Monomial(nVariables, cc));
            if (!ring.IsZero(lc))
            {
                int[] lcDegreeVector = new int[nVariables];
                lcDegreeVector[variable] = 1;
                data.Add(new Monomial(lcDegreeVector, 1, lc));
            }

            return new MultivariatePolynomial(nVariables, ring, ordering, data);
        }


        public override bool IsMonic()
        {
            return ring.IsOne(Lc());
        }


        public override int SignumOfLC()
        {
            return ring.Signum(Lc());
        }

        public override bool IsOne()
        {
            if (Size() != 1)
                return false;
            Monomial<E> lt = terms.First();
            return lt.IsZeroVector() && ring.IsOne(lt.coefficient);
        }


        public override bool IsUnitCC()
        {
            return ring.IsOne(Cc());
        }

        public override bool IsConstant()
        {
            return Size() == 0 || (Size() == 1 && terms.First().IsZeroVector());
        }


        public E MaxAbsCoefficient()
        {
            return Stream().Map(ring.Abs()).Max(ring).OrElseGet(ring.GetZero());
        }


        /// <summary>
        /// Returns the leading coefficient of this polynomial that is coefficient of the largest term according to the
        /// ordering.
        /// </summary>
        /// <returns>leading coefficient of this polynomial</returns>
        public E Lc()
        {
            return Lt().coefficient;
        }


        /// <summary>
        /// Returns the leading coefficient of this polynomial with respect to specified ordering
        /// </summary>
        /// <returns>leading coefficient of this polynomial with respect to specified ordering</returns>
        public E Lc(IComparer<DegreeVector> ordering)
        {
            return Lt(ordering).coefficient;
        }


        /// <summary>
        /// Sets the leading coefficient to the specified value
        /// </summary>
        /// <param name="val">new value for the lc</param>
        /// <returns>the leading coefficient to the specified value</returns>
        public MultivariatePolynomial<E> SetLC(E val)
        {
            if (IsZero())
                return Add(val);
            terms.Add(Lt().SetCoefficient(ring.ValueOf(val)));
            Release();
            return this;
        }

        /// <summary>
        /// Returns the constant coefficient of this polynomial.
        /// </summary>
        /// <returns>constant coefficient of this polynomial</returns>
        public E Cc()
        {
            Monomial<E> zero = new Monomial(nVariables, ring.GetZero());
            return terms.GetOrDefault(zero, zero).coefficient;
        }


        public E Content()
        {
            return IsOverField() ? Lc() : ring.Gcd(Coefficients());
        }


        public Iterable<E> Coefficients()
        {
            return () => new It(terms.Iterator());
        }


        public E[] CoefficientsArray()
        {
            if (IsZero())
                return ring.CreateZeroesArray(1);
            E[] array = ring.CreateArray(Size());
            int i = 0;
            foreach (Monomial<E> term in this)
                array[i++] = term.coefficient;
            return array;
        }


        private class It<V> : IEnumerator<V>
        {
            readonly IEnumerator<Monomial<V>> inner;

            public It(IEnumerator<Monomial<V>> inner)
            {
                this.inner = inner;
            }

            public virtual bool HasNext()
            {
                return inner.HasNext();
            }

            public virtual V Next()
            {
                return inner.Next().coefficient;
            }
        }


        public override MultivariatePolynomial<E> PrimitivePart(int variable)
        {
            return AsNormalMultivariate(AsOverUnivariateEliminate(variable).PrimitivePart(), variable);
        }


        public override UnivariatePolynomial<E> ContentUnivariate(int variable)
        {
            return AsOverUnivariate(variable).Content();
        }


        public override MultivariatePolynomial<E> PrimitivePart()
        {
            if (IsZero())
                return this;
            E content = Content();
            if (SignumOfLC() < 0 && ring.Signum(content) > 0)
                content = ring.Negate(content);
            MultivariatePolynomial<E> r = DivideOrNull(content);
            return r;
        }


        public override MultivariatePolynomial<E> PrimitivePartSameSign()
        {
            if (IsZero())
                return this;
            E c = Content();
            if (SignumOfLC() < 0)
                c = ring.Negate(c);
            MultivariatePolynomial<E> r = DivideOrNull(c);
            return r;
        }


        public override MultivariatePolynomial<E> DivideByLC(MultivariatePolynomial<E> other)
        {
            return DivideOrNull(other.Lc());
        }


        /// <summary>
        /// Divides this polynomial by a {@code factor} or returns {@code null} (causing loss of internal data) if some of
        /// the elements can't be exactly divided by the {@code factor}. NOTE: if {@code null} is returned, the content of
        /// {@code this} is destroyed.
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this} divided by the {@code factor} or {@code null}</returns>
        public MultivariatePolynomial<E> DivideOrNull(E factor)
        {
            if (ring.IsOne(factor))
                return this;
            if (ring.IsMinusOne(factor))
                return Negate();
            if (ring.IsField())
                return Multiply(ring.Reciprocal(factor)); // <- this is typically faster than the division
            foreach (Entry<DegreeVector, Monomial<E>> entry in terms.EntrySet())
            {
                Monomial<E> term = entry.GetValue();
                E quot = ring.DivideOrNull(term.coefficient, factor);
                if (quot == null)
                    return null;
                entry.SetValue(term.SetCoefficient(quot));
            }

            Release();
            return this;
        }


        /// <summary>
        /// Divides this polynomial by a {@code factor} or throws exception if exact division is not possible
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this} divided by the {@code factor}</returns>
        /// <exception cref="ArithmeticException">if exact division is not possible</exception>
        public MultivariatePolynomial<E> DivideExact(E factor)
        {
            MultivariatePolynomial<E> r = DivideOrNull(factor);
            if (r == null)
                throw new ArithmeticException("not divisible " + this + " / " + factor);
            return r;
        }


        public override MultivariatePolynomial<E> DivideOrNull(Monomial<E> monomial)
        {
            if (monomial.IsZeroVector())
                return DivideOrNull(monomial.coefficient);
            MonomialSet<Monomial<E>> map = new MonomialSet(ordering);
            foreach (Monomial<E> term in terms)
            {
                Monomial<E> dv = monomialAlgebra.DivideOrNull(term, monomial);
                if (dv == null)
                    return null;
                map.Add(dv);
            }

            LoadFrom(map);
            Release();
            return this;
        }


        public override MultivariatePolynomial<E> Monic()
        {
            if (IsZero())
                return this;
            return DivideOrNull(Lc());
        }


        /// <summary>
        /// Makes this polynomial monic if possible, if not -- destroys this and returns null
        /// </summary>
        /// <returns>monic this or null if the ring does not support exact division by lc</returns>
        public override MultivariatePolynomial<E> Monic(IComparer<DegreeVector> ordering)
        {
            if (IsZero())
                return this;
            return DivideOrNull(Lc(ordering));
        }


        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the {@code factor} modulo {@code modulus} (that is {@code
        /// monic(modulus).multiply(factor)} ).
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this}</returns>
        public MultivariatePolynomial<E> Monic(E factor)
        {
            E lc = Lc();
            return Multiply(factor).DivideOrNull(lc);
        }


        /// <summary>
        /// Sets {@code this} to its monic part (with respect to given ordering) multiplied by the given factor;
        /// </summary>
        public MultivariatePolynomial<E> Monic(IComparer<DegreeVector> ordering, E factor)
        {
            E lc = Lc(ordering);
            return Multiply(factor).DivideOrNull(lc);
        }

        public override MultivariatePolynomial<E> MonicWithLC(MultivariatePolynomial<E> other)
        {
            if (Lc().Equals(other.Lc()))
                return this;
            return Monic(other.Lc());
        }


        public override MultivariatePolynomial<E> MonicWithLC(IComparer<DegreeVector> ordering,
            MultivariatePolynomial<E> other)
        {
            E lc = Lc(ordering);
            E olc = other.Lc(ordering);
            if (lc.Equals(olc))
                return this;
            return Monic(ordering, olc);
        }


        /// <summary>
        /// Gives a recursive univariate representation of this poly.
        /// </summary>
        public UnivariatePolynomial ToDenseRecursiveForm()
        {
            if (nVariables == 0)
                throw new ArgumentException("#variables = 0");
            return ToDenseRecursiveForm(nVariables - 1);
        }


        private UnivariatePolynomial ToDenseRecursiveForm(int variable)
        {
            if (variable == 0)
                return AsUnivariate();
            UnivariatePolynomial<MultivariatePolynomial<E>> result = AsUnivariateEliminate(variable);
            IUnivariatePolynomial[] data = new IUnivariatePolynomial[result.Degree() + 1];
            for (int j = 0; j < data.Length; ++j)
                data[j] = result[j].ToDenseRecursiveForm(variable - 1);
            return UnivariatePolynomial.Create(Rings.PolynomialRing(data[0]), data);
        }


        /// <summary>
        /// Converts poly from a recursive univariate representation.
        /// </summary>
        /// <param name="recForm">recursive univariate representation</param>
        /// <param name="nVariables">number of variables in multivariate polynomial</param>
        /// <param name="ordering">monomial order</param>
        public static MultivariatePolynomial<E> FromDenseRecursiveForm<E>(UnivariatePolynomial recForm, int nVariables,
            IComparer<DegreeVector> ordering)
        {
            return FromDenseRecursiveForm(recForm, nVariables, ordering, nVariables - 1);
        }


        private static MultivariatePolynomial<E> FromDenseRecursiveForm<E>(UnivariatePolynomial recForm, int nVariables,
            IComparer<DegreeVector> ordering, int variable)
        {
            if (variable == 0)
                return (MultivariatePolynomial<E>)AsMultivariate(recForm, nVariables, 0, ordering);
            UnivariatePolynomial<UnivariatePolynomial> _recForm = (UnivariatePolynomial<UnivariatePolynomial>)recForm;
            MultivariatePolynomial<E>[] data = new MultivariatePolynomial[_recForm.Degree() + 1];
            for (int j = 0; j < data.Length; ++j)
                data[j] = FromDenseRecursiveForm(_recForm[j], nVariables, ordering, variable - 1);
            return AsMultivariate(UnivariatePolynomial.Create(Rings.MultivariateRing(data[0]), data), variable);
        }


        /// <summary>
        /// Evaluates polynomial given in a dense recursive form at a given points
        /// </summary>
        public static E EvaluateDenseRecursiveForm<E>(UnivariatePolynomial recForm, int nVariables, E[] values)
        {
            // compute number of variables
            UnivariatePolynomial p = recForm;
            int n = nVariables - 1;
            while (n > 0)
            {
                p = (UnivariatePolynomial)((UnivariatePolynomial)p).Cc();
                --n;
            }

            if (nVariables != values.Length)
                throw new ArgumentException();
            return EvaluateDenseRecursiveForm(recForm, values, ((UnivariatePolynomial<E>)p).ring, nVariables - 1);
        }


        private static E EvaluateDenseRecursiveForm<E>(UnivariatePolynomial recForm, E[] values, Ring<E> ring,
            int variable)
        {
            if (variable == 0)
                return ((UnivariatePolynomial<E>)recForm).Evaluate(values[0]);
            UnivariatePolynomial<UnivariatePolynomial> _recForm = (UnivariatePolynomial<UnivariatePolynomial>)recForm;
            E result = ring.GetZero();
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


        private AMultivariatePolynomial ToSparseRecursiveForm(int variable)
        {
            if (variable == 0)
            {
                return this.SetNVariables(1);
            }

            MultivariatePolynomial<MultivariatePolynomial<E>> result =
                AsOverMultivariateEliminate(ArraysUtil.Sequence(0, variable), MonomialOrder.GRLEX);
            Monomial<AMultivariatePolynomial>[] data = new Monomial[result.Count == 0 ? 1 : result.Count];
            int j = 0;
            foreach (Monomial<MultivariatePolynomial<E>> term in result.Count == 0
                         ? Collections.SingletonList(result.Lt())
                         : result)
                data[j++] = new Monomial(term, term.coefficient.ToSparseRecursiveForm(variable - 1));
            return MultivariatePolynomial.Create(1, Rings.MultivariateRing(data[0].coefficient), MonomialOrder.GRLEX,
                data);
        }


        /// <summary>
        /// Converts poly from a recursive univariate representation.
        /// </summary>
        /// <param name="recForm">recursive univariate representation</param>
        /// <param name="nVariables">number of variables in multivariate polynomial</param>
        /// <param name="ordering">monomial order</param>
        public static MultivariatePolynomial<E> FromSparseRecursiveForm<E>(AMultivariatePolynomial recForm,
            int nVariables, IComparer<DegreeVector> ordering)
        {
            return FromSparseRecursiveForm(recForm, nVariables, ordering, nVariables - 1);
        }


        private static MultivariatePolynomial<E> FromSparseRecursiveForm<E>(AMultivariatePolynomial recForm,
            int nVariables, IComparer<DegreeVector> ordering, int variable)
        {
            if (variable == 0)
            {
                return ((MultivariatePolynomial<E>)recForm).SetNVariables(nVariables).SetOrdering(ordering);
            }

            MultivariatePolynomial<AMultivariatePolynomial> _recForm =
                (MultivariatePolynomial<AMultivariatePolynomial>)recForm;
            Monomial<MultivariatePolynomial<E>>[] data = new Monomial[_recForm.Count == 0 ? 1 : _recForm.Count];
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

            MultivariatePolynomial<MultivariatePolynomial<E>> result =
                MultivariatePolynomial.Create(nVariables, Rings.MultivariateRing(data[0].coefficient), ordering, data);
            return AsNormalMultivariate(result);
        }


        /// <summary>
        /// Evaluates polynomial given in a sparse recursive form at a given points
        /// </summary>
        public static E EvaluateSparseRecursiveForm<E>(AMultivariatePolynomial recForm, int nVariables, E[] values)
        {
            // compute number of variables
            AMultivariatePolynomial p = recForm;
            TIntArrayList degrees = new TIntArrayList();
            int n = nVariables - 1;
            while (n > 0)
            {
                p = (AMultivariatePolynomial)((MultivariatePolynomial)p).Cc();
                degrees.Add(p.Degree());
                --n;
            }

            degrees.Add(p.Degree());
            if (nVariables != values.Length)
                throw new ArgumentException();
            Ring<E> ring = ((MultivariatePolynomial<E>)p).ring;
            PrecomputedPowers<E>[] pp = new PrecomputedPowers[nVariables];
            for (int i = 0; i < nVariables; ++i)
                pp[i] = new PrecomputedPowers(Math.Min(degrees[i], MAX_POWERS_CACHE_SIZE), values[i], ring);
            return EvaluateSparseRecursiveForm(recForm, new PrecomputedPowersHolder(ring, pp), nVariables - 1);
        }


        static E EvaluateSparseRecursiveForm<E>(AMultivariatePolynomial recForm, PrecomputedPowersHolder<E> ph,
            int variable)
        {
            Ring<E> ring = ph.ring;
            if (variable == 0)
            {
                MultivariatePolynomial<E> _recForm = (MultivariatePolynomial<E>)recForm;
                IEnumerator<Monomial<E>> it = _recForm.terms.DescendingIterator();
                int previousExponent = -1;
                E result = ring.GetZero();
                while (it.HasNext())
                {
                    Monomial<E> m = it.Next();
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
            E result = ring.GetZero();
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
        public HornerForm GetHornerForm(int[] evaluationVariables)
        {
            int[] evalDegrees = ArraysUtil.Select(DegreesRef(), evaluationVariables);
            MultivariatePolynomial<MultivariatePolynomial<E>> p = AsOverMultivariateEliminate(evaluationVariables);
            Ring<AMultivariatePolynomial> newRing = Rings.PolynomialRing(p.Cc().ToSparseRecursiveForm());
            return new HornerForm(ring, evalDegrees, evaluationVariables.Length,
                p.MapCoefficients(newRing, MultivariatePolynomial.ToSparseRecursiveForm()));
        }


        /// <summary>
        /// A representation of multivariate polynomial specifically optimized for fast evaluation of given variables
        /// </summary>
        public sealed class HornerForm<E>
        {
            private readonly Ring<E> ring;
            private readonly int nEvalVariables;
            private readonly int[] evalDegrees;
            private readonly MultivariatePolynomial<AMultivariatePolynomial> recForm;

            private HornerForm(Ring<E> ring, int[] evalDegrees, int nEvalVariables,
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
            public MultivariatePolynomial<E> Evaluate(E[] values)
            {
                if (values.Length != nEvalVariables)
                    throw new ArgumentException();
                PrecomputedPowers<E>[] pp = new PrecomputedPowers[nEvalVariables];
                for (int i = 0; i < nEvalVariables; ++i)
                    pp[i] = new PrecomputedPowers(Math.Min(evalDegrees[i], MAX_POWERS_CACHE_SIZE), values[i], ring);
                return recForm.MapCoefficients(ring,
                    (p) => EvaluateSparseRecursiveForm(p, new PrecomputedPowersHolder(ring, pp), nEvalVariables - 1));
            }
        }

        /// <summary>
        /// Returns a copy of this with {@code value} substituted for {@code variable}.
        /// </summary>
        /// <param name="variable">the variable</param>
        /// <param name="value">the value</param>
        /// <returns>a new multivariate polynomial with {@code value} substituted for {@code variable} but still with the same
        ///         {@link #nVariables} (though the effective number of variables is {@code nVariables - 1}, compare to
        ///         {@link #eliminate(int, Object)})</returns>
        /// <remarks>@see#eliminate(int, Object)</remarks>
        public MultivariatePolynomial<E> Evaluate(int variable, E value)
        {
            value = ring.ValueOf(value);
            if (ring.IsZero(value))
                return EvaluateAtZero(variable);
            PrecomputedPowers<E> powers = new PrecomputedPowers(value, ring);
            return Evaluate(variable, powers);
        }


        /// <summary>
        /// Returns a copy of this with {@code value} substituted for {@code variable}.
        /// </summary>
        /// <param name="variable">the variable</param>
        /// <returns>a new multivariate polynomial with {@code value} substituted for {@code variable} but still with the same
        ///         {@link #nVariables} (though the effective number of variables is {@code nVariables - 1}, compare to
        ///         {@link #eliminate(int, long)})</returns>
        MultivariatePolynomial<E> Evaluate(int variable, PrecomputedPowers<E> powers)
        {
            if (Degree(variable) == 0)
                return Clone();
            if (ring.IsZero(powers.value))
                return EvaluateAtZero(variable);
            MonomialSet<Monomial<E>> newData = new MonomialSet(ordering);
            foreach (Monomial<E> el in terms)
            {
                E val = ring.Multiply(el.coefficient, powers.Pow(el.exponents[variable]));
                Add(newData, el.SetZero(variable).SetCoefficient(val));
            }

            return new MultivariatePolynomial(nVariables, ring, ordering, newData);
        }


        /// <summary>
        /// Returns a copy of this with {@code value} substituted for {@code variable}.
        /// </summary>
        /// <param name="variable">the variable</param>
        /// <returns>a new multivariate polynomial with {@code value} substituted for {@code variable} but still with the same
        ///         {@link #nVariables} (though the effective number of variables is {@code nVariables - 1}, compare to
        ///         {@link #eliminate(int, long)})</returns>
        UnivariatePolynomial<E> EvaluateAtZeroAllExcept(int variable)
        {
            E[] uData = ring.CreateArray(Degree(variable) + 1);
                out:
            foreach (Monomial<E> el in terms)
            {
                if (el.totalDegree != 0 && el.exponents[variable] == 0)
                    continue;
                for (int i = 0; i < nVariables; ++i)
                    if (i != variable && el.exponents[i] != 0)
                        continue;
                int uExp = el.exponents[variable];
                uData[uExp] = ring.Add(uData[uExp], el.coefficient);
            }

            return UnivariatePolynomial.CreateUnsafe(ring, uData);
        }


        /// <summary>
        /// Evaluates this polynomial at specified points
        /// </summary>
        public E Evaluate(params E[] values)
        {
            if (values.Length != nVariables)
                throw new ArgumentException();
            return Evaluate(ArraysUtil.Sequence(0, nVariables), values).Cc();
        }


        /// <summary>
        /// Returns a copy of this with {@code values} substituted for {@code variables}.
        /// </summary>
        /// <param name="variables">the variables</param>
        /// <param name="values">the values</param>
        /// <returns>a new multivariate polynomial with {@code value} substituted for {@code variable} but still with the same
        ///         {@link #nVariables} (though the effective number of variables is {@code nVariables - 1}, compare to
        ///         {@link #eliminate(int, Object)})</returns>
        /// <remarks>@see#eliminate(int, Object)</remarks>
        public MultivariatePolynomial<E> Evaluate(int[] variables, E[] values)
        {
            foreach (E value in values)
                if (!ring.IsZero(value))
                    return Evaluate(MkPrecomputedPowers(variables, values), variables);

            // <- all values are zero
            return EvaluateAtZero(variables);
        }


        /// <summary>
        /// substitutes {@code values} for {@code variables}
        /// </summary>
        MultivariatePolynomial<E> Evaluate(PrecomputedPowersHolder<E> powers, int[] variables)
        {
            MonomialSet<Monomial<E>> newData = new MonomialSet(ordering);
            foreach (Monomial<E> el in terms)
            {
                Monomial<E> r = el;
                E value = el.coefficient;
                foreach (int variable in variables)
                    value = ring.Multiply(value, powers.Pow(variable, el.exponents[variable]));
                r = r.SetZero(variables).SetCoefficient(value);
                Add(newData, r);
            }

            return new MultivariatePolynomial(nVariables, ring, ordering, newData);
        }


        /// <summary>
        /// Evaluates this polynomial at specified points
        /// </summary>
        public MultivariatePolynomial<E>[] Evaluate(int variable, params E[] values)
        {
            return Arrays.Stream(values).Map((p) => Evaluate(variable, p)).ToArray(MultivariatePolynomial[].New());
        }


        /// <summary>
        /// Returns a copy of this with {@code value} substituted for {@code variable}.
        /// </summary>
        /// <param name="variable">the variable</param>
        /// <param name="value">the value</param>
        /// <returns>a new multivariate polynomial with {@code value} substituted for {@code variable} but still with the same
        ///         {@link #nVariables} (though the effective number of variables is {@code nVariables - 1}, compare to
        ///         {@link #eliminate(int, long)})</returns>
        /// <remarks>@see#eliminate(int, long)</remarks>
        public MultivariatePolynomial<E> Evaluate(int variable, long value)
        {
            return Evaluate(variable, ring.ValueOf(value));
        }


        /// <summary>
        /// Substitutes {@code value} for {@code variable} and eliminates {@code variable} from the list of variables so that
        /// the resulting polynomial has {@code result.nVariables = this.nVariables - 1}.
        /// </summary>
        /// <param name="variable">the variable</param>
        /// <param name="value">the value</param>
        /// <returns>a new multivariate polynomial with {@code value} substituted for {@code variable} and  {@code nVariables
        ///         = nVariables - 1})</returns>
        /// <remarks>@see#evaluate(int, Object)</remarks>
        public MultivariatePolynomial<E> Eliminate(int variable, E value)
        {
            value = ring.ValueOf(value);
            MonomialSet<Monomial<E>> newData = new MonomialSet(ordering);
            PrecomputedPowers<E> powers = new PrecomputedPowers(value, ring);
            foreach (Monomial<E> el in terms)
            {
                E val = ring.Multiply(el.coefficient, powers.Pow(el.exponents[variable]));
                Add(newData, el.Without(variable).SetCoefficient(val));
            }

            return new MultivariatePolynomial(nVariables - 1, ring, ordering, newData);
        }

        /// <summary>
        /// Substitutes {@code value} for {@code variable} and eliminates {@code variable} from the list of variables so that
        /// the resulting polynomial has {@code result.nVariables = this.nVariables - 1}.
        /// </summary>
        /// <param name="variable">the variable</param>
        /// <param name="value">the value</param>
        /// <returns>a new multivariate polynomial with {@code value} substituted for {@code variable} and  {@code nVariables
        ///         = nVariables - 1})</returns>
        /// <remarks>@see#evaluate(int, long)</remarks>
        public MultivariatePolynomial<E> Eliminate(int variable, long value)
        {
            return Eliminate(variable, ring.ValueOf(value));
        }


        public MultivariatePolynomial<E> Eliminate(int[] variables, E[] values)
        {
            foreach (E value in values)
                if (!ring.IsZero(value))
                    return Eliminate(MkPrecomputedPowers(variables, values), variables);

            // <- all values are zero
            return EvaluateAtZero(variables).DropVariables(variables);
        }


        /// <summary>
        /// substitutes {@code values} for {@code variables}
        /// </summary>
        MultivariatePolynomial<E> Eliminate(PrecomputedPowersHolder<E> powers, int[] variables)
        {
            MonomialSet<Monomial<E>> newData = new MonomialSet(ordering);
            foreach (Monomial<E> el in terms)
            {
                Monomial<E> r = el;
                E value = el.coefficient;
                foreach (int variable in variables)
                    value = ring.Multiply(value, powers.Pow(variable, el.exponents[variable]));
                r = r.Without(variables).SetCoefficient(value);
                Add(newData, r);
            }

            return new MultivariatePolynomial(nVariables - variables.Length, ring, ordering, newData);
        }


        /// <summary>
        /// cached powers used to save some time
        /// </summary>
        sealed class PrecomputedPowers<E>
        {
            private readonly E value;
            private readonly Ring<E> ring;
            private readonly E[] precomputedPowers;

            PrecomputedPowers(E value, Ring<E> ring) : this(DEFAULT_POWERS_CACHE_SIZE, value, ring)
            {
            }

            PrecomputedPowers(int cacheSize, E value, Ring<E> ring)
            {
                this.value = ring.ValueOf(value);
                this.ring = ring;
                this.precomputedPowers = ring.CreateArray(cacheSize);
            }

            E Pow(int exponent)
            {
                if (exponent >= precomputedPowers.Length)
                    return ring.Pow(value, exponent);
                if (precomputedPowers[exponent] != null)
                    return precomputedPowers[exponent];
                E result = ring.GetOne();
                E k2p = value;
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


        public PrecomputedPowersHolder<E> MkPrecomputedPowers(int variable, E value)
        {
            PrecomputedPowers<E>[] pp = new PrecomputedPowers[nVariables];
            pp[variable] = new PrecomputedPowers(Math.Min(Degree(variable), MAX_POWERS_CACHE_SIZE), value, ring);
            return new PrecomputedPowersHolder(ring, pp);
        }


        public PrecomputedPowersHolder<E> MkPrecomputedPowers(int[] variables, E[] values)
        {
            int[] degrees = DegreesRef();
            PrecomputedPowers<E>[] pp = new PrecomputedPowers[nVariables];
            for (int i = 0; i < variables.Length; ++i)
                pp[variables[i]] = new PrecomputedPowers(Math.Min(degrees[variables[i]], MAX_POWERS_CACHE_SIZE),
                    values[i], ring);
            return new PrecomputedPowersHolder(ring, pp);
        }


        public static PrecomputedPowersHolder<E> MkPrecomputedPowers<E>(int nVariables, Ring<E> ring, int[] variables,
            E[] values)
        {
            PrecomputedPowers<E>[] pp = new PrecomputedPowers[nVariables];
            for (int i = 0; i < variables.Length; ++i)
                pp[variables[i]] = new PrecomputedPowers(MAX_POWERS_CACHE_SIZE, values[i], ring);
            return new PrecomputedPowersHolder(ring, pp);
        }


        public PrecomputedPowersHolder<E> MkPrecomputedPowers(E[] values)
        {
            if (values.Length != nVariables)
                throw new ArgumentException();
            int[] degrees = DegreesRef();
            PrecomputedPowers<E>[] pp = new PrecomputedPowers[nVariables];
            for (int i = 0; i < nVariables; ++i)
                pp[i] = new PrecomputedPowers(Math.Min(degrees[i], MAX_POWERS_CACHE_SIZE), values[i], ring);
            return new PrecomputedPowersHolder(ring, pp);
        }


        /// <summary>
        /// holds an array of precomputed powers
        /// </summary>
        public sealed class PrecomputedPowersHolder<E>
        {
            readonly Ring<E> ring;
            readonly PrecomputedPowers<E>[] powers;

            public PrecomputedPowersHolder(Ring<E> ring, PrecomputedPowers<E>[] powers)
            {
                this.ring = ring;
                this.powers = powers;
            }

            void Set(int i, E point)
            {
                if (powers[i] == null || !powers[i].value.Equals(point))
                    powers[i] = new PrecomputedPowers(
                        powers[i] == null ? DEFAULT_POWERS_CACHE_SIZE : powers[i].precomputedPowers.Length, point,
                        ring);
            }

            E Pow(int variable, int exponent)
            {
                return powers[variable].Pow(exponent);
            }

            public PrecomputedPowersHolder<E> Clone()
            {
                return new PrecomputedPowersHolder(ring, powers.Clone());
            }
        }


        /// <summary>
        /// Returns a copy of this with {@code poly} substituted for {@code variable}.
        /// </summary>
        /// <param name="variable">the variable</param>
        /// <param name="poly">the replacement for the variable</param>
        /// <returns>a copy of this with  {@code variable -> poly}</returns>
        public MultivariatePolynomial<E> Substitute(int variable, MultivariatePolynomial<E> poly)
        {
            if (poly.IsConstant())
                return Evaluate(variable, poly.Cc());
            PrecomputedSubstitution<E> subsPowers;
            if (poly.IsEffectiveUnivariate())
                subsPowers = new USubstitution(poly.AsUnivariate(), poly.UnivariateVariable(), nVariables, ordering);
            else
                subsPowers = new MSubstitution(poly);
            MultivariatePolynomial<E> result = CreateZero();
            foreach (Monomial<E> term in this)
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
        public MultivariatePolynomial<E> Shift(int variable, long shift)
        {
            return Shift(variable, ring.ValueOf(shift));
        }


        /// <summary>
        /// Returns a copy of this with {@code variable -> variable + shift}
        /// </summary>
        /// <param name="variable">the variable</param>
        /// <param name="shift">shift amount</param>
        /// <returns>a copy of this with  {@code variable -> variable + shift}</returns>
        public MultivariatePolynomial<E> Shift(int variable, E shift)
        {
            if (ring.IsZero(shift))
                return Clone();
            shift = ring.ValueOf(shift);
            USubstitution<E> shifts =
                new USubstitution(UnivariatePolynomial.CreateUnsafe(ring, ring.CreateArray(shift, ring.GetOne())),
                    variable, nVariables, ordering);
            MultivariatePolynomial<E> result = CreateZero();
            foreach (Monomial<E> term in this)
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
        /// Returns a copy of this with {@code variables -> variables + shifts}
        /// </summary>
        /// <param name="variables">the variables</param>
        /// <param name="shifts">the corresponding shifts</param>
        /// <returns>a copy of this with {@code variables -> variables + shifts}</returns>
        public MultivariatePolynomial<E> Shift(int[] variables, E[] shifts)
        {
            PrecomputedSubstitution<E>[] powers = new PrecomputedSubstitution[nVariables];
            bool allShiftsAreZero = true;
            for (int i = 0; i < variables.Length; ++i)
            {
                if (!ring.IsZero(shifts[i]))
                    allShiftsAreZero = false;
                powers[variables[i]] =
                    new USubstitution(UnivariatePolynomial.Create(ring, ring.CreateArray(shifts[i], ring.GetOne())),
                        variables[i], nVariables, ordering);
            }

            if (allShiftsAreZero)
                return Clone();
            PrecomputedSubstitutions<E> calculatedShifts = new PrecomputedSubstitutions(powers);
            MultivariatePolynomial<E> result = CreateZero();
            foreach (Monomial<E> term in this)
            {
                MultivariatePolynomial<E> temp = CreateOne();
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


        sealed class PrecomputedSubstitutions<E>
        {
            readonly PrecomputedSubstitution<E>[] subs;

            public PrecomputedSubstitutions(PrecomputedSubstitution<E>[] subs)
            {
                this.subs = subs;
            }

            MultivariatePolynomial<E> GetSubstitutionPower(int var, int exponent)
            {
                if (subs[var] == null)
                    throw new ArgumentException();
                return subs[var].Pow(exponent);
            }
        }


        interface PrecomputedSubstitution<E>
        {
            MultivariatePolynomial<E> Pow(int exponent);
        }


        sealed class USubstitution<E> : PrecomputedSubstitution<E>
        {
            readonly int variable;
            readonly int nVariables;
            readonly IComparer<DegreeVector> ordering;
            readonly UnivariatePolynomial<E> base;
            readonly TIntObjectHashMap<UnivariatePolynomial<E>> uCache = new TIntObjectHashMap();
            readonly TIntObjectHashMap<MultivariatePolynomial<E>> mCache = new TIntObjectHashMap();

            USubstitution(UnivariatePolynomial<E> @base, int variable, int nVariables,
                IComparer<DegreeVector> ordering)
            {
                this.nVariables = nVariables;
                this.variable = variable;
                this.ordering = ordering;
                this.@base = @base;
            }

            public MultivariatePolynomial<E> Pow(int exponent)
            {
                MultivariatePolynomial<E> cached = mCache[exponent];
                if (cached != null)
                    return cached.Clone();
                UnivariatePolynomial<E> r = PolynomialMethods.PolyPow(@base, exponent, true, uCache);
                mCache.Put(exponent, cached = AsMultivariate(r, nVariables, variable, ordering));
                return cached.Clone();
            }
        }


        sealed class MSubstitution<E> : PrecomputedSubstitution<E>
        {
            readonly MultivariatePolynomial<E> base;
            readonly TIntObjectHashMap<MultivariatePolynomial<E>> cache = new TIntObjectHashMap();

            MSubstitution(MultivariatePolynomial<E> @base)
            {
                this.@base = @base;
            }

            public MultivariatePolynomial<E> Pow(int exponent)
            {
                return PolynomialMethods.PolyPow(@base, exponent, true, cache);
            }
        }


        override void Add(MonomialSet<Monomial<E>> terms, Monomial<E> term)
        {
            Add(terms, term, ring);
        }


        override void Subtract(MonomialSet<Monomial<E>> terms, Monomial<E> term)
        {
            Subtract(terms, term, ring);
        }


        /// <summary>
        /// Adds {@code oth} to this polynomial
        /// </summary>
        /// <param name="oth">other polynomial</param>
        /// <returns>{@code this + oth}</returns>
        public MultivariatePolynomial<E> Add(E oth)
        {
            oth = ring.ValueOf(oth);
            if (ring.IsZero(oth))
                return this;
            Add(terms, new Monomial(nVariables, oth));
            Release();
            return this;
        }


        /// <summary>
        /// Subtracts {@code oth} from this polynomial
        /// </summary>
        /// <param name="oth">other polynomial</param>
        /// <returns>{@code this - oth}</returns>
        public MultivariatePolynomial<E> Subtract(E oth)
        {
            return Add(ring.Negate(ring.ValueOf(oth)));
        }


        public override MultivariatePolynomial<E> Increment()
        {
            return Add(ring.GetOne());
        }


        public override MultivariatePolynomial<E> Decrement()
        {
            return Subtract(ring.GetOne());
        }


        /// <summary>
        /// Multiplies {@code this} by the {@code factor}
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code} this multiplied by the {@code factor}</returns>
        public MultivariatePolynomial<E> Multiply(E factor)
        {
            factor = ring.ValueOf(factor);
            if (ring.IsOne(factor))
                return this;
            if (ring.IsZero(factor))
                return ToZero();
            IEnumerator<Entry<DegreeVector, Monomial<E>>> it = terms.EntrySet().Iterator();
            while (it.HasNext())
            {
                Entry<DegreeVector, Monomial<E>> entry = it.Next();
                Monomial<E> term = entry.GetValue();
                E val = ring.Multiply(term.coefficient, factor);
                if (ring.IsZero(val))
                    it.Remove();
                else
                    entry.SetValue(term.SetCoefficient(val));
            }

            Release();
            return this;
        }


        /// <summary>
        /// Multiplies {@code this} by the {@code factor}
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code} this multiplied by the {@code factor}</returns>
        public override MultivariatePolynomial<E> MultiplyByLC(MultivariatePolynomial<E> other)
        {
            return Multiply(other.Lc());
        }


        public override MultivariatePolynomial<E> Multiply(Monomial<E> monomial)
        {
            CheckSameDomainWith(monomial);
            if (monomial.IsZeroVector())
                return Multiply(monomial.coefficient);
            if (ring.IsZero(monomial.coefficient))
                return ToZero();
            MonomialSet<Monomial<E>> newMap = new MonomialSet(ordering);
            foreach (Monomial<E> thisElement in terms)
            {
                Monomial<E> mul = monomialAlgebra.Multiply(thisElement, monomial);
                if (!ring.IsZero(mul.coefficient))
                    newMap.Add(mul);
            }

            return LoadFrom(newMap);
        }


        public override MultivariatePolynomial<E> Multiply(long factor)
        {
            return Multiply(ring.ValueOf(factor));
        }


        public override MultivariatePolynomial<E> MultiplyByBigInteger(BigInteger factor)
        {
            return Multiply(ring.ValueOfBigInteger(factor));
        }


        public override MultivariatePolynomial<E> Multiply(MultivariatePolynomial<E> oth)
        {
            AssertSameCoefficientRingWith(oth);
            if (oth.IsZero())
                return ToZero();
            if (IsZero())
                return this;
            if (oth.IsConstant())
                return Multiply(oth.Cc());
            if (oth.Count == 1)
                return Multiply(oth.Lt());
            if (Size() > KRONECKER_THRESHOLD && oth.Count > KRONECKER_THRESHOLD)
                return MultiplyKronecker(oth);
            else
                return MultiplyClassic(oth);
        }


        private MultivariatePolynomial<E> MultiplyClassic(MultivariatePolynomial<E> oth)
        {
            MonomialSet<Monomial<E>> newMap = new MonomialSet(ordering);
            foreach (Monomial<E> othElement in oth.terms)
            foreach (Monomial<E> thisElement in terms)
                Add(newMap, monomialAlgebra.Multiply(thisElement, othElement));
            return LoadFrom(newMap);
        }


        private MultivariatePolynomial<E> MultiplyKronecker(MultivariatePolynomial<E> oth)
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
            if (threshold > Long.MAX_VALUE)
                return MultiplyClassic(oth);
            return FromKronecker(MultiplySparseUnivariate(ring, ToKronecker(map), oth.ToKronecker(map)), map);
        }


        /// <summary>
        /// Convert to Kronecker's representation
        /// </summary>
        private TLongObjectHashMap<E> ToKronecker(long[] kroneckerMap)
        {
            TLongObjectHashMap<E> result = new TLongObjectHashMap(Size());
            foreach (Monomial<E> term in this)
            {
                long exponent = term.exponents[0];
                for (int i = 1; i < term.exponents.Length; i++)
                    exponent += term.exponents[i] * kroneckerMap[i];
                result.Put(exponent, term.coefficient);
            }

            return result;
        }


        private static TLongObjectHashMap<CfHolder<E>> MultiplySparseUnivariate<E>(Ring<E> ring,
            TLongObjectHashMap<E> a, TLongObjectHashMap<E> b)
        {
            TLongObjectHashMap<CfHolder<E>> result = new TLongObjectHashMap(a.Count + b.Count);
            TLongObjectIterator<E> ait = a.Iterator();
            while (ait.HasNext())
            {
                ait.Advance();
                TLongObjectIterator<E> bit = b.Iterator();
                while (bit.HasNext())
                {
                    bit.Advance();
                    long deg = ait.Key() + bit.Key();
                    E av = ait.Value();
                    E bv = bit.Value();
                    E val = ring.Multiply(av, bv);
                    CfHolder<E> r = result[deg];
                    if (r != null)
                    {
                        r.coefficient = ring.Add(r.coefficient, val);
                    }
                    else
                    {
                        result.Put(deg, new CfHolder(val));
                    }
                }
            }

            return result;
        }


        private MultivariatePolynomial<E> FromKronecker(TLongObjectHashMap<CfHolder<E>> p, long[] kroneckerMap)
        {
            terms.Clear();
            TLongObjectIterator<CfHolder<E>> it = p.Iterator();
            while (it.HasNext())
            {
                it.Advance();
                if (ring.IsZero(it.Value().coefficient))
                    continue;
                long exponent = it.Key();
                int[] exponents = new int[nVariables];
                for (int i = 0; i < nVariables; i++)
                {
                    long div = exponent / kroneckerMap[nVariables - i - 1];
                    exponent = exponent - (div * kroneckerMap[nVariables - i - 1]);
                    exponents[nVariables - i - 1] = MachineArithmetic.SafeToInt(div);
                }

                terms.Add(new Monomial(exponents, it.Value().coefficient));
            }

            Release();
            return this;
        }


        sealed class CfHolder<E>
        {
            E coefficient;

            CfHolder(E coefficient)
            {
                this.coefficient = coefficient;
            }
        }


        public override MultivariatePolynomial<E> Square()
        {
            return Multiply(this);
        }


        public override MultivariatePolynomial<E> EvaluateAtRandom(int variable, RandomGenerator rnd)
        {
            return Evaluate(variable, ring.RandomElement(rnd));
        }


        public override MultivariatePolynomial<E> EvaluateAtRandomPreservingSkeleton(int variable, RandomGenerator rnd)
        {
            if (Degree(variable) == 0)
                return Clone();

            //desired skeleton
            HashSet<DegreeVector> skeleton = GetSkeletonExcept(variable);
            MultivariatePolynomial<E> tmp;
            do
            {
                E randomPoint = ring.RandomElement(rnd);
                tmp = Evaluate(variable, randomPoint);
            } while (!skeleton.Equals(tmp.GetSkeleton()));

            return tmp;
        }


        //desired skeleton
        public override MultivariatePolynomial<E> Derivative(int variable, int order)
        {
            MonomialSet<Monomial<E>> newTerms = new MonomialSet(ordering);
            foreach (Monomial<E> term in this)
            {
                int exponent = term.exponents[variable];
                if (exponent < order)
                    continue;
                E newCoefficient = term.coefficient;
                for (int i = 0; i < order; ++i)
                    newCoefficient = ring.Multiply(newCoefficient, ring.ValueOf(exponent - i));
                int[] newExponents = term.exponents.Clone();
                newExponents[variable] -= order;
                Add(newTerms, new Monomial(newExponents, term.totalDegree - order, newCoefficient));
            }

            return new MultivariatePolynomial(nVariables, ring, ordering, newTerms);
        }


        /// <summary>
        /// exp * (exp - 1) * ... * (exp - order + 1) / (1 * 2 * ... * order) mod modulus
        /// </summary>
        static BigInteger SeriesCoefficientFactor0(int exponent, int order, IntegersZp ring)
        {
            if (!ring.modulus.IsInt() || order < ring.modulus.IntValueExact())
                return SeriesCoefficientFactor1(exponent, order, ring);
            return BigInteger.ValueOf(
                MultivariatePolynomialZp64.SeriesCoefficientFactor(exponent, order, ring.AsZp64()));
        }


        /// <summary>
        /// exp * (exp - 1) * ... * (exp - order + 1) / (1 * 2 * ... * order) mod modulus
        /// </summary>
        static E SeriesCoefficientFactor1<E>(int exponent, int order, Ring<E> ring)
        {
            E factor = ring.GetOne();
            for (int i = 0; i < order; ++i)
                factor = ring.Multiply(factor, ring.ValueOf(exponent - i));
            factor = ring.DivideExact(factor, ring.Factorial(order));
            return factor;
        }


        /// <summary>
        /// exp * (exp - 1) * ... * (exp - order + 1) / (1 * 2 * ... * order) mod modulus
        /// </summary>
        static E SeriesCoefficientFactor2<E>(int exponent, int order, Ring<E> ring)
        {
            BigInteger factor = BigInteger.ONE;
            for (int i = 0; i < order; ++i)
                factor = factor.Multiply(BigInteger.ValueOf(exponent - i));
            factor = factor.DivideExact(Rings.Z.Factorial(order));
            return ring.ValueOfBigInteger(factor);
        }


        /// <summary>
        /// exp * (exp - 1) * ... * (exp - order + 1) / (1 * 2 * ... * order) mod modulus
        /// </summary>
        static E SeriesCoefficientFactor<E>(int exponent, int order, Ring<E> ring)
        {
            if (ring is IntegersZp)
                return (E)SeriesCoefficientFactor0(exponent, order, (IntegersZp)ring);
            BigInteger characteristics = ring.Characteristic();
            if (characteristics == null || !characteristics.IsInt() || characteristics.IntValueExact() > order)
                return SeriesCoefficientFactor1(exponent, order, ring);
            return SeriesCoefficientFactor2(exponent, order, ring);
        }


        /// <summary>
        /// exp * (exp - 1) * ... * (exp - order + 1) / (1 * 2 * ... * order) mod modulus
        /// </summary>
        public override MultivariatePolynomial<E> SeriesCoefficient(int variable, int order)
        {
            if (order == 0)
                return Clone();
            if (IsConstant())
                return CreateZero();
            MonomialSet<Monomial<E>> newTerms = new MonomialSet(ordering);
            foreach (Monomial<E> term in this)
            {
                int exponent = term.exponents[variable];
                if (exponent < order)
                    continue;
                int[] newExponents = term.exponents.Clone();
                newExponents[variable] -= order;
                E newCoefficient = ring.Multiply(term.coefficient, SeriesCoefficientFactor(exponent, order, ring));
                Add(newTerms, new Monomial(newExponents, term.totalDegree - order, newCoefficient));
            }

            return new MultivariatePolynomial(nVariables, ring, ordering, newTerms);
        }


        /// <summary>
        /// Returns a stream of coefficients of this
        /// </summary>
        /// <returns>stream of coefficients</returns>
        public Stream<E> Stream()
        {
            return terms.Values().Stream().Map((e) => e.coefficient);
        }


        /// <summary>
        /// Maps terms of this using specified mapping function
        /// </summary>
        /// <param name="newRing">the new ring</param>
        /// <param name="mapper">mapping</param>
        /// <param name="<T>">new element type</param>
        /// <returns>a new polynomial with terms obtained by applying mapper to this terms</returns>
        public MultivariatePolynomial<T> MapTerms<T>(Ring<T> newRing, Function<Monomial<E>, Monomial<T>> mapper)
        {
            return terms.Values().Stream().Map(mapper)
                .Collect(new PolynomialCollector(() => Zero(nVariables, newRing, ordering)));
        }


        /// <summary>
        /// Maps coefficients of this using specified mapping function
        /// </summary>
        /// <param name="newRing">the new ring</param>
        /// <param name="mapper">mapping</param>
        /// <param name="<T>">new element type</param>
        /// <returns>a new polynomial with terms obtained by applying mapper to this terms (only coefficients are changed)</returns>
        public MultivariatePolynomial<T> MapCoefficients<T>(Ring<T> newRing, Function<E, T> mapper)
        {
            return MapTerms(newRing, (t) => new Monomial(t.exponents, t.totalDegree, mapper.Apply(t.coefficient)));
        }


        /// <summary>
        /// Maps coefficients of this using specified mapping function
        /// </summary>
        /// <param name="newDomain">the new ring</param>
        /// <param name="mapper">mapping</param>
        /// <returns>a new polynomial with terms obtained by applying mapper to this terms (only coefficients are changed)</returns>
        public MultivariatePolynomialZp64 MapCoefficientsZp64(IntegersZp64 newDomain, ToLongFunction<E> mapper)
        {
            return terms.Values().Stream()
                .Map((t) => new MonomialZp64(t.exponents, t.totalDegree, mapper.ApplyAsLong(t.coefficient))).Collect(
                    new PolynomialCollector(() => MultivariatePolynomialZp64.Zero(nVariables, newDomain, ordering)));
        }


        /// <summary>
        /// Maps coefficients of this using specified mapping function
        /// </summary>
        /// <param name="newDomain">the new ring</param>
        /// <param name="mapper">mapping</param>
        /// <returns>a new polynomial with terms obtained by applying mapper to this terms (only coefficients are changed)</returns>
        public override MultivariatePolynomial<T> MapCoefficientsAsPolys<T>(Ring<T> ring,
            Function<MultivariatePolynomial<E>, T> mapper)
        {
            return MapCoefficients(ring, (cf) => mapper.Apply(CreateConstant(cf)));
        }


        public override int CompareTo(MultivariatePolynomial<E> oth)
        {
            int c = Integer.Compare(Size(), oth.Count);
            if (c != 0)
                return c;
            IEnumerator<Monomial<E>> thisIt = Iterator(), othIt = oth.Iterator();
            while (thisIt.HasNext() && othIt.HasNext())
            {
                Monomial<E> a = thisIt.Next(), b = othIt.Next();
                if ((c = ordering.Compare(a, b)) != 0)
                    return c;
                if ((c = ring.Compare(a.coefficient, b.coefficient)) != 0)
                    return c;
            }

            return 0;
        }


        public override MultivariatePolynomial<E> Clone()
        {
            return new MultivariatePolynomial(nVariables, ring, ordering, terms.Clone());
        }


        public override MultivariatePolynomial<E> ParsePoly(string @string)
        {
            MultivariatePolynomial<E> r = Parse(@string, ring, ordering);
            if (r.nVariables != nVariables)
                return Parse(@string, ring, ordering, IStringifier.DefaultVars(nVariables));
            return r;
        }


        public override string ToString(IStringifier<MultivariatePolynomial<E>> stringifier)
        {
            IStringifier<E> cfStringifier = stringifier.Substringifier(ring);
            if (IsConstant())
                return cfStringifier.Stringify(Cc());
            string[] varStrings = new string[nVariables];
            for (int i = 0; i < nVariables; ++i)
                varStrings[i] = stringifier.GetBindings()
                    .GetOrDefault(CreateMonomial(i, 1), IStringifier.DefaultVar(i, nVariables));
            StringBuilder sb = new StringBuilder();
            foreach (Monomial<E> term in terms)
            {
                E cf = term.coefficient;
                string cfString;
                if (ring.IsMinusOne(cf) && term.totalDegree != 0)
                    cfString = "-";
                else if (!ring.IsOne(cf) || term.totalDegree == 0)
                    cfString = cfStringifier.Stringify(cf);
                else
                    cfString = "";
                if (term.totalDegree != 0 && IStringifier.NeedParenthesisInSum(cfString))
                    cfString = "(" + cfString + ")";
                if (sb.Length != 0 && !cfString.StartsWith("-"))
                    sb.Append("+");
                StringBuilder cfBuilder = new StringBuilder();
                cfBuilder.Append(cfString);
                bool appended = false;
                for (int i = 0; i < nVariables; ++i)
                {
                    if (term.exponents[i] == 0)
                        continue;
                    if (!(cfString.Equals("-") && !appended) && cfBuilder.Length != 0)
                        cfBuilder.Append("*");
                    cfBuilder.Append(varStrings[i]);
                    if (term.exponents[i] > 1)
                        cfBuilder.Append("^").Append(term.exponents[i]);
                    appended = true;
                }

                sb.Append(cfBuilder);
            }

            return sb.ToString();
        }


        public override string CoefficientRingToString(IStringifier<MultivariatePolynomial<E>> stringifier)
        {
            return ring.ToString(stringifier.Substringifier(ring));
        }
    }
}