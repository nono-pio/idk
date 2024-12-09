using System.Collections.Immutable;
using Cc.Redberry.Rings.Io;
using Cc.Redberry.Rings.Poly.Multivar;

namespace Cc.Redberry.Rings.Poly.Multivar
{
    /// <summary>
    /// Ideal represented by its Groebner basis.
    /// </summary>
    /// <remarks>@since2.3</remarks>
    public sealed class Ideal<Term, Poly> : Stringifiable<Poly> where Term : AMonomial<Term>
        where Poly : AMultivariatePolynomial<Term, Poly>
    {
        /// <summary>
        /// list of original generators
        /// </summary>
        private readonly List<Poly> originalGenerators;

        /// <summary>
        /// monomial order used for standard basis
        /// </summary>
        public readonly Comparator<DegreeVector> ordering;

        /// <summary>
        /// util factory polynomial (ordered by monomialOrder)
        /// </summary>
        private readonly Poly factory;

        /// <summary>
        /// Groebner basis with respect to {@code monomialOrder}
        /// </summary>
        private readonly List<Poly> groebnerBasis;

        /// <summary>
        /// the whole ring instance (ordered by monomialOrder)
        /// </summary>
        private readonly MultivariateRing<Poly> ring;

        private Ideal(List<Poly> originalGenerators, List<Poly> groebnerBasis)
        {
            this.originalGenerators = Collections.UnmodifiableList(originalGenerators);
            this.factory = groebnerBasis[0].CreateZero();
            this.groebnerBasis = groebnerBasis;
            this.ordering = factory.ordering;
            this.ring = Rings.MultivariateRing(factory);
        }

        private Ideal(List<Poly> groebnerBasis) : this(groebnerBasis, groebnerBasis)
        {
        }


        /// <summary>
        /// The monomial order used for Groebner basis
        /// </summary>
        public Comparator<DegreeVector> GetMonomialOrder()
        {
            return ordering;
        }


        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        public Ideal<Term, Poly> ChangeOrder(Comparator<DegreeVector> newMonomialOrder)
        {
            if (ordering == newMonomialOrder)
                return this;
            if (IsGradedOrder(ordering) || !IsGradedOrder(newMonomialOrder))
                return new Ideal<Term, Poly>(originalGenerators, HilbertConvertBasis(groebnerBasis, newMonomialOrder));
            return Create(originalGenerators, newMonomialOrder);
        }

        /// <summary>
        /// Set the monomial order used for Groebner basis of this ideal
        /// </summary>
        private static Poly SetOrdering(Poly poly, Comparator<DegreeVector> monomialOrder)
        {
            return poly.ordering == monomialOrder ? poly : poly.SetOrdering(monomialOrder);
        }

        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        private Poly SetOrdering(Poly poly)
        {
            return SetOrdering(poly, ordering);
        }


        /// <summary>
        /// set ordering of poly to monomialOrder
        /// </summary>
        private Poly Mod0(Poly poly)
        {
            return MultivariateDivision.PseudoRemainder(SetOrdering(poly), groebnerBasis);
        }


        /// <summary>
        /// Reduces {@code poly} modulo this ideal
        /// </summary>
        public Poly NormalForm(Poly poly)
        {
            Comparator<DegreeVector> originalOrder = poly.ordering;
            return SetOrdering(Mod0(poly), originalOrder);
        }


        /// <summary>
        /// Returns the list of original generators
        /// </summary>
        public List<Poly> GetOriginalGenerators()
        {
            return originalGenerators;
        }


        /// <summary>
        /// Groebner basis of this ideal
        /// </summary>
        public ImmutableList<Poly> GetGroebnerBasis()
        {
            return groebnerBasis.ToImmutableList();
        }


        /// <summary>
        /// Returns the number of elements in Groebner basis
        /// </summary>
        public int NBasisGenerators()
        {
            return groebnerBasis.Count;
        }

        /// <summary>
        /// Returns i-th element of Groebner basis
        /// </summary>
        public Poly GetBasisGenerator(int i)
        {
            return groebnerBasis[i];
        }


        /// <summary>
        /// Whether this ideal is the whole ring (basis consists of pne constant polynomial)
        /// </summary>
        public bool IsTrivial()
        {
            return NBasisGenerators() == 1 && GetBasisGenerator(0).IsConstant() && !GetBasisGenerator(0).IsZero();
        }


        /// <summary>
        /// Whether this is a proper ideal
        /// </summary>
        public bool IsProper()
        {
            return !IsTrivial();
        }


        /// <summary>
        /// Whether this ideal is empty
        /// </summary>
        public bool IsEmpty()
        {
            return NBasisGenerators() == 1 && GetBasisGenerator(0).IsZero();
        }


        /// <summary>
        /// Whether this ideal is principal
        /// </summary>
        public bool IsPrincipal()
        {
            return NBasisGenerators() == 1;
        }


        /// <summary>
        /// Whether this ideal is homogeneous
        /// </summary>
        public bool IsHomogeneous()
        {
            return IsHomogeneousIdeal(groebnerBasis);
        }

        /// <summary>
        /// Whether this ideal is monomial
        /// </summary>
        public bool IsMonomial()
        {
            return IsMonomialIdeal(groebnerBasis);
        }


        /// <summary>
        /// Returns true if this ideal is maximal (that is its affine variety has only one point)
        /// </summary>
        public bool IsMaximal()
        {
            return (factory.IsOverZ() || factory.IsOverField()) && Dimension() == 0 &&
                   groebnerBasis.Count == factory.nVariables &&
                   groebnerBasis.Stream().AllMatch(AMultivariatePolynomial.IsLinearExactly());
        }


        /// <summary>
        /// Ideal of leading terms
        /// </summary>
        public Ideal<Term, Poly> LtIdeal()
        {
            if (IsMonomial())
                return this;
            return new Ideal<Term, Poly>(groebnerBasis.Select(m => m.LtAsPoly()).ToList());
        }


        /// <summary>
        /// Tests whether specified poly is an element of this ideal
        /// </summary>
        public bool Contains(Poly poly)
        {
            return Mod0(poly).IsZero();
        }


        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        public bool Contains(Ideal<Term, Poly> oth)
        {
            return Quotient(oth).IsTrivial();
        }


        /// <summary>
        /// Whether this ideal contains the specified one
        /// </summary>
        // lazy Hilbert-Poincare series
        private HilbertSeries hilbertSeries = null;


        /// <summary>
        /// Hilbert-Poincare series of this ideal
        /// </summary>
        public HilbertSeries HilbertSeries()
        {
            lock (this)
            {
                if (hilbertSeries == null)
                {
                    if (IsHomogeneous() || IsGradedOrder(ordering))
                        hilbertSeries = HilbertSeriesOfLeadingTermsSet(groebnerBasis);
                    else

                        // use original generators to construct basis when current ordering is "hard"
                        hilbertSeries =
                            HilbertSeriesOfLeadingTermsSet(GroebnerBasisWithOptimizedGradedOrder(originalGenerators));
                }

                return hilbertSeries;
            }
        }


        /// <summary>
        /// Returns the affine dimension of this ideal
        /// </summary>
        public int Dimension()
        {
            return HilbertSeries().Dimension();
        }


        /// <summary>
        /// Returns the affine degree of this ideal
        /// </summary>
        public int Degree()
        {
            return HilbertSeries().Degree();
        }

        /// <summary>
        /// Whether this ideal contains the product of two specified ideals
        /// </summary>
        public bool ContainsProduct(Ideal<Term, Poly> a, Ideal<Term, Poly> b)
        {
            if (a.NBasisGenerators() > b.NBasisGenerators())
                return ContainsProduct(b, a);
            return Quotient(a).Contains(b);
        }

        /// <summary>
        /// Tests whether {@code poly} belongs to the radical of this
        /// </summary>
        public bool RadicalContains(Poly poly)
        {
            // adjoin new variable to all generators (convert to F[X][y])
            List<Poly> yGenerators = groebnerBasis.Select(m => m.JoinNewVariable()).ToList();
            Poly yPoly = poly.JoinNewVariable();

            // add 1 - y*poly
            yGenerators.Add(yPoly.CreateOne().Subtract(yPoly.CreateMonomial(yPoly.nVariables - 1, 1).Multiply(yPoly)));
            return Create(yGenerators).IsTrivial();
        }


        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        public Ideal<Term, Poly> Union(Poly oth)
        {
            factory.AssertSameCoefficientRingWith(oth);
            if (oth.IsZero())
                return this;
            if (oth.IsOne())
                return Trivial(factory);
            List<Poly> l = new List<Poly>(groebnerBasis);
            l.Add(oth);
            return Create(l, ordering);
        }


        /// <summary>
        /// Returns the union of this and oth
        /// </summary>
        public Ideal<Term, Poly> Union(Ideal<Term, Poly> oth)
        {
            AssertSameDomain(oth);
            if (IsEmpty() || oth.IsTrivial())
                return oth;
            if (oth.IsEmpty() || IsTrivial())
                return this;
            List<Poly> l = new List<Poly>();
            l.AddRange(groebnerBasis);
            l.AddRange(oth.groebnerBasis);
            return Create(l, ordering);
        }


        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        public Ideal<Term, Poly> Multiply(Ideal<Term, Poly> oth)
        {
            AssertSameDomain(oth);
            if (IsTrivial() || oth.IsEmpty())
                return oth;
            if (oth.IsTrivial() || this.IsEmpty())
                return this;
            List<Poly> generators = new List<Poly>();
            foreach (Poly a in groebnerBasis)
            foreach (Poly b in oth.groebnerBasis)
                generators.Add(a.Clone().Multiply(b));
            return Create(generators, ordering);
        }


        /// <summary>
        /// Returns squared ideal
        /// </summary>
        public Ideal<Term, Poly> Square()
        {
            return Multiply(this);
        }


        /// <summary>
        /// Returns this in a power of exponent
        /// </summary>
        public Ideal<Term, Poly> Pow(int exponent)
        {
            if (exponent < 0)
                throw new ArgumentException();
            if (exponent == 1)
                return this;
            Ideal<Term, Poly> result = Trivial(factory);
            Ideal<Term, Poly> k2p = this;
            for (;;)
            {
                if ((exponent & 1) != 0)
                    result = result.Multiply(k2p);
                exponent >>= 1;
                if (exponent == 0)
                    return result;
                k2p = k2p.Multiply(k2p);
            }
        }


        /// <summary>
        /// Returns the product of this and oth
        /// </summary>
        public Ideal<Term, Poly> Multiply(Poly oth)
        {
            factory.AssertSameCoefficientRingWith(oth);
            if (IsTrivial())
                return Create([oth], ordering);
            if (oth.IsZero())
                return Trivial(oth, ordering);
            if (oth.IsOne() || this.IsEmpty())
                return this;
            return new Ideal<Term, Poly>(Canonicalize(groebnerBasis.Select((p) => p.Clone().Multiply(oth)).ToList()));
        }


        /// <summary>
        /// Returns the intersection of this and oth
        /// </summary>
        public Ideal<Term, Poly> Intersection(Ideal<Term, Poly> oth)
        {
            AssertSameDomain(oth);
            if (IsTrivial() || oth.IsEmpty())
                return oth;
            if (oth.IsTrivial() || this.IsEmpty())
                return this;
            if (IsPrincipal() && oth.IsPrincipal())

                // intersection of principal ideals is easy
                return Create([ring.Lcm(GetBasisGenerator(0), oth.GetBasisGenerator(0))], ordering);

            // we compute (t * I + (1 - t) * J) ∩ R[X]
            Poly t = factory.InsertVariable(0).CreateMonomial(0, 1);
            List<Poly> tGenerators = new List<Poly>();
            foreach (Poly gI in this.groebnerBasis)
                tGenerators.Add(gI.InsertVariable(0).Multiply(t));
            Poly omt = t.Clone().Negate().Increment(); // 1 - t
            foreach (Poly gJ in oth.groebnerBasis)
                tGenerators.Add(gJ.InsertVariable(0).Multiply(omt));

            // elimination
            List<Poly> result = GroebnerMethods.Eliminate(tGenerators, 0).Stream().Map((p) => p.DropVariable(0))
                .Map((p) => p.SetOrdering(ordering)).Collect(Collectors.ToList());
            return Create(result, ordering);
        }

        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        public Ideal<Term, Poly> Quotient(Poly oth)
        {
            if (oth.IsZero())
                return Trivial(factory);
            if (oth.IsConstant())
                return this;
            return Create(Intersection(Create(oth)).groebnerBasis.Select((p) => ring.Quotient(p, oth)).ToList());
        }


        /// <summary>
        /// Returns the quotient this : oth
        /// </summary>
        public Ideal<Term, Poly> Quotient(Ideal<Term, Poly> oth)
        {
            if (oth.IsEmpty())
                return Trivial(factory);
            if (oth.IsTrivial())
                return this;
            return oth.groebnerBasis.Select(this.Quotient).Aggregate(Trivial(factory), (agg, b) => agg.Intersection(b));
        }


        Ideal<Term, Poly> InsertVariable(int variable)
        {
            return new Ideal<Term, Poly>(groebnerBasis.Select((p) => p.InsertVariable(variable)).ToList());
        }


        private void AssertSameDomain(Ideal<Term, Poly> oth)
        {
            factory.AssertSameCoefficientRingWith(oth.factory);
        }


        public bool Equals(object o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o.GetType())
                return false;
            Ideal < ?, ?> ideal = (Ideal < ?,  ?>)o;
            return ordering.Equals(ideal.ordering) && groebnerBasis.Equals(ideal.groebnerBasis);
        }


        public int GetHashCode()
        {
            return groebnerBasis.GetHashCode();
        }


        public string ToString(IStringifier<Poly> stringifier)
        {
            return "<" + string.Join(", ", groebnerBasis.Select(stringifier.Stringify)) + ">";
        }


        public string ToString()
        {
            return ToString(IStringifier.Dummy());
        }


        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to GREVLEX order will be used.
        /// </summary>
        public static Ideal<Term, Poly> Create(List<Poly> generators)
        {
            return Create(generators, GREVLEX);
        }


        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to GREVLEX order will be used.
        /// </summary>
        public static Ideal<Term, Poly> Create(params Poly[] generators)
        {
            return Create(generators.ToList());
        }


        /// <summary>
        /// Creates ideal given by a list of generators. Groebner basis with respect to specified {@code monomialOrder} will
        /// be used.
        /// </summary>
        /// <param name="monomialOrder">monomial order for unique Groebner basis of the ideal</param>
        public static Ideal<Term, Poly> Create(List<Poly> generators, Comparator<DegreeVector> monomialOrder)
        {
            return new Ideal<Term, Poly>(generators, GroebnerBasis(generators, monomialOrder));
        }


        /// <summary>
        /// Creates trivial ideal (ideal = ring)
        /// </summary>
        public static Ideal<Term, Poly> Trivial(Poly factory)
        {
            return Trivial(factory, GREVLEX);
        }


        /// <summary>
        /// Creates trivial ideal (ideal = ring)
        /// </summary>
        public static Ideal<Term, Poly> Trivial(Poly factory, Comparator<DegreeVector> monomialOrder)
        {
            return new Ideal<Term, Poly>([factory.CreateOne().SetOrdering(monomialOrder)]);
        }


        /// <summary>
        /// Creates empty ideal
        /// </summary>
        public static Ideal<Term, Poly> Empty(Poly factory)
        {
            return Empty(factory, GREVLEX);
        }


        /// <summary>
        /// Creates empty ideal
        /// </summary>
        public static Ideal<Term, Poly> Empty(Poly factory, Comparator<DegreeVector> monomialOrder)
        {
            return new Ideal<Term, Poly>([factory.CreateZero().SetOrdering(monomialOrder)]);
        }


        /// <summary>
        /// Shortcut for parse
        /// </summary>
        public static Ideal<Monomial<E>, MultivariatePolynomial<E>> Parse<E>(string[] generators, Ring<E> field,
            string[] variables)
        {
            return Parse(generators, field, GREVLEX, variables);
        }


        /// <summary>
        /// Shortcut for parse
        /// </summary>
        public static Ideal<Monomial<E>, MultivariatePolynomial<E>> Parse<E>(string[] generators, Ring<E> field,
            Comparator<DegreeVector> monomialOrder, string[] variables)
        {
            return Create(
                generators.Select((p) => MultivariatePolynomial<E>.Parse(p, field, monomialOrder, variables)).ToList(),
                monomialOrder);
        }
    }
}