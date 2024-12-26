using System.Collections.Immutable;

namespace Polynomials.Poly.Multivar;

public sealed class Ideal<E>
{
    private readonly List<MultivariatePolynomial<E>> originalGenerators;


    public readonly IComparer<DegreeVector> ordering;


    private readonly MultivariatePolynomial<E> factory;


    private readonly List<MultivariatePolynomial<E>> groebnerBasis;


    private readonly MultivariateRing<E> ring;

    private Ideal(List<MultivariatePolynomial<E>> originalGenerators, List<MultivariatePolynomial<E>> groebnerBasis)
    {
        this.originalGenerators = originalGenerators;
        this.factory = groebnerBasis[0].CreateZero();
        this.groebnerBasis = groebnerBasis;
        this.ordering = factory.ordering;
        this.ring = Rings.MultivariateRing(factory);
    }

    private Ideal(List<MultivariatePolynomial<E>> groebnerBasis) : this(groebnerBasis, groebnerBasis)
    {
    }


    public IComparer<DegreeVector> GetMonomialOrder()
    {
        return ordering;
    }


    public Ideal<E> ChangeOrder(IComparer<DegreeVector> newMonomialOrder)
    {
        if (ordering == newMonomialOrder)
            return this;
        if (IsGradedOrder(ordering) || !IsGradedOrder(newMonomialOrder))
            return new Ideal<E>(originalGenerators, HilbertConvertBasis(groebnerBasis, newMonomialOrder));
        return Create(originalGenerators, newMonomialOrder);
    }


    private static MultivariatePolynomial<E> SetOrdering(MultivariatePolynomial<E> poly, IComparer<DegreeVector> monomialOrder)
    {
        return poly.ordering == monomialOrder ? poly : poly.SetOrdering(monomialOrder);
    }


    private MultivariatePolynomial<E> SetOrdering(MultivariatePolynomial<E> poly)
    {
        return SetOrdering(poly, ordering);
    }


    private MultivariatePolynomial<E> Mod0(MultivariatePolynomial<E> poly)
    {
        return MultivariateDivision.PseudoRemainder(SetOrdering(poly), groebnerBasis);
    }


    public MultivariatePolynomial<E> NormalForm(MultivariatePolynomial<E> poly)
    {
        Comparator<DegreeVector> originalOrder = poly.ordering;
        return SetOrdering(Mod0(poly), originalOrder);
    }


    public List<MultivariatePolynomial<E>> GetOriginalGenerators()
    {
        return originalGenerators;
    }


    public ImmutableList<MultivariatePolynomial<E>> GetGroebnerBasis()
    {
        return groebnerBasis.ToImmutableList();
    }


    public int NBasisGenerators()
    {
        return groebnerBasis.Count;
    }


    public MultivariatePolynomial<E> GetBasisGenerator(int i)
    {
        return groebnerBasis[i];
    }


    public bool IsTrivial()
    {
        return NBasisGenerators() == 1 && GetBasisGenerator(0).IsConstant() && !GetBasisGenerator(0).IsZero();
    }


    public bool IsProper()
    {
        return !IsTrivial();
    }


    public bool IsEmpty()
    {
        return NBasisGenerators() == 1 && GetBasisGenerator(0).IsZero();
    }


    public bool IsPrincipal()
    {
        return NBasisGenerators() == 1;
    }


    public bool IsHomogeneous()
    {
        return IsHomogeneousIdeal(groebnerBasis);
    }


    public bool IsMonomial()
    {
        return IsMonomialIdeal(groebnerBasis);
    }


    public bool IsMaximal()
    {
        return (factory.IsOverZ() || factory.IsOverField()) && Dimension() == 0 &&
               groebnerBasis.Count == factory.nVariables &&
               groebnerBasis.Stream().AllMatch(AMultivariatePolynomial.IsLinearExactly());
    }


    public Ideal<E> LtIdeal()
    {
        if (IsMonomial())
            return this;
        return new Ideal<E>(groebnerBasis.Select(m => m.LtAsPoly()).ToList());
    }


    public bool Contains(MultivariatePolynomial<E> poly)
    {
        return Mod0(poly).IsZero();
    }


    public bool Contains(Ideal<E> oth)
    {
        return Quotient(oth).IsTrivial();
    }


    // lazy Hilbert-Poincare series
    private HilbertSeries hilbertSeries = null;


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


    public int Dimension()
    {
        return HilbertSeries().Dimension();
    }


    public int Degree()
    {
        return HilbertSeries().Degree();
    }


    public bool ContainsProduct(Ideal<E> a, Ideal<E> b)
    {
        if (a.NBasisGenerators() > b.NBasisGenerators())
            return ContainsProduct(b, a);
        return Quotient(a).Contains(b);
    }


    public bool RadicalContains(MultivariatePolynomial<E> poly)
    {
        // adjoin new variable to all generators (convert to F[X][y])
        List<MultivariatePolynomial<E>> yGenerators = groebnerBasis.Select(m => m.JoinNewVariable()).ToList();
        MultivariatePolynomial<E> yPoly = poly.JoinNewVariable();

        // add 1 - y*poly
        yGenerators.Add(yPoly.CreateOne().Subtract(yPoly.CreateMonomial(yPoly.nVariables - 1, 1).Multiply(yPoly)));
        return Create(yGenerators).IsTrivial();
    }


    public Ideal<E> Union(MultivariatePolynomial<E> oth)
    {
        factory.AssertSameCoefficientRingWith(oth);
        if (oth.IsZero())
            return this;
        if (oth.IsOne())
            return Trivial(factory);
        List<MultivariatePolynomial<E>> l = new List<MultivariatePolynomial<E>>(groebnerBasis);
        l.Add(oth);
        return Create(l, ordering);
    }


    public Ideal<E> Union(Ideal<E> oth)
    {
        AssertSameDomain(oth);
        if (IsEmpty() || oth.IsTrivial())
            return oth;
        if (oth.IsEmpty() || IsTrivial())
            return this;
        List<MultivariatePolynomial<E>> l = new List<MultivariatePolynomial<E>>();
        l.AddRange(groebnerBasis);
        l.AddRange(oth.groebnerBasis);
        return Create(l, ordering);
    }


    public Ideal<E> Multiply(Ideal<E> oth)
    {
        AssertSameDomain(oth);
        if (IsTrivial() || oth.IsEmpty())
            return oth;
        if (oth.IsTrivial() || this.IsEmpty())
            return this;
        List<MultivariatePolynomial<E>> generators = new List<MultivariatePolynomial<E>>();
        foreach (MultivariatePolynomial<E> a in groebnerBasis)
        foreach (MultivariatePolynomial<E> b in oth.groebnerBasis)
            generators.Add(a.Clone().Multiply(b));
        return Create(generators, ordering);
    }


    public Ideal<E> Square()
    {
        return Multiply(this);
    }


    public Ideal<E> Pow(int exponent)
    {
        if (exponent < 0)
            throw new ArgumentException();
        if (exponent == 1)
            return this;
        Ideal<E> result = Trivial(factory);
        Ideal<E> k2p = this;
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


    public Ideal<E> Multiply(MultivariatePolynomial<E> oth)
    {
        factory.AssertSameCoefficientRingWith(oth);
        if (IsTrivial())
            return Create([oth], ordering);
        if (oth.IsZero())
            return Trivial(oth, ordering);
        if (oth.IsOne() || this.IsEmpty())
            return this;
        return new Ideal<E>(Canonicalize(groebnerBasis.Select((p) => p.Clone().Multiply(oth)).ToList()));
    }


    public Ideal<E> Intersection(Ideal<E> oth)
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
        MultivariatePolynomial<E> t = factory.InsertVariable(0).CreateMonomial(0, 1);
        List<MultivariatePolynomial<E>> tGenerators = new List<MultivariatePolynomial<E>>();
        foreach (MultivariatePolynomial<E> gI in this.groebnerBasis)
            tGenerators.Add(gI.InsertVariable(0).Multiply(t));
        MultivariatePolynomial<E> omt = t.Clone().Negate().Increment(); // 1 - t
        foreach (MultivariatePolynomial<E> gJ in oth.groebnerBasis)
            tGenerators.Add(gJ.InsertVariable(0).Multiply(omt));

        // elimination
        List<MultivariatePolynomial<E>> result = GroebnerMethods.Eliminate(tGenerators, 0).Stream().Map((p) => p.DropVariable(0))
            .Map((p) => p.SetOrdering(ordering)).Collect(Collectors.ToList());
        return Create(result, ordering);
    }


    public Ideal<E> Quotient(MultivariatePolynomial<E> oth)
    {
        if (oth.IsZero())
            return Trivial(factory);
        if (oth.IsConstant())
            return this;
        return Create(Intersection(Create(oth)).groebnerBasis.Select((p) => ring.Quotient(p, oth)).ToList());
    }


    public Ideal<E> Quotient(Ideal<E> oth)
    {
        if (oth.IsEmpty())
            return Trivial(factory);
        if (oth.IsTrivial())
            return this;
        return oth.groebnerBasis.Select(this.Quotient).Aggregate(Trivial(factory), (agg, b) => agg.Intersection(b));
    }


    Ideal<E> InsertVariable(int variable)
    {
        return new Ideal<E>(groebnerBasis.Select((p) => p.InsertVariable(variable)).ToList());
    }


    private void AssertSameDomain(Ideal<E> oth)
    {
        factory.AssertSameCoefficientRingWith(oth.factory);
    }


    public override bool Equals(object? o)
    {
        if (this == o)
            return true;
        if (o == null || GetType() != o.GetType())
            return false;
        Ideal <E> ideal = (Ideal <E>)o;
        return ordering.Equals(ideal.ordering) && groebnerBasis.Equals(ideal.groebnerBasis);
    }


    public override int GetHashCode()
    {
        return groebnerBasis.GetHashCode();
    }


    public static Ideal<E> Create(List<MultivariatePolynomial<E>> generators)
    {
        return Create(generators, MonomialOrder.GREVLEX);
    }


    public static Ideal<E> Create(params MultivariatePolynomial<E>[] generators)
    {
        return Create(generators.ToList());
    }


    public static Ideal<E> Create(List<MultivariatePolynomial<E>> generators, IComparer<DegreeVector> monomialOrder)
    {
        return new Ideal<E>(generators, GroebnerBasis(generators, monomialOrder));
    }


    public static Ideal<E> Trivial(MultivariatePolynomial<E> factory)
    {
        return Trivial(factory, MonomialOrder.GREVLEX);
    }


    public static Ideal<E> Trivial(MultivariatePolynomial<E> factory, IComparer<DegreeVector> monomialOrder)
    {
        return new Ideal<E>([factory.CreateOne().SetOrdering(monomialOrder)]);
    }


    public static Ideal<E> Empty(MultivariatePolynomial<E> factory)
    {
        return Empty(factory, MonomialOrder.GREVLEX);
    }


    public static Ideal<E> Empty(MultivariatePolynomial<E> factory, IComparer<DegreeVector> monomialOrder)
    {
        return new Ideal<E>([factory.CreateZero().SetOrdering(monomialOrder)]);
    }



}