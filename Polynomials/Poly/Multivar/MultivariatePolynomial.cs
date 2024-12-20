using System.Collections.ObjectModel;
using System.Reflection.PortableExecutable;
using Polynomials.Poly.Univar;

namespace Polynomials.Poly.Multivar;

public abstract class MultivariatePolynomial<E> : Polynomial<MultivariatePolynomial<E>>, MonomialSetView<E>
{
    public readonly int nVariables;

    public readonly IComparer<DegreeVector> ordering;

    public readonly MonomialAlgebra<E> monomialAlgebra;

    public readonly MonomialSet<E> terms;

    public readonly Ring<E> ring;

    public MultivariatePolynomial(int nVariables, Ring<E> ring, IComparer<DegreeVector> ordering,
        MonomialSet<E> terms)
    {
        this.nVariables = nVariables;
        this.ordering = ordering;
        this.ring = ring;
        this.monomialAlgebra = new MonomialAlgebra<E>(ring);
        this.terms = terms;
    }


    public static MultivariatePolynomial<E> SwapVariables(MultivariatePolynomial<E> poly, int i, int j)
    {
        if (i == j)
            return poly.Clone();
        int[] newVariables = ArraysUtil.Sequence(poly.nVariables);
        newVariables[i] = j;
        newVariables[j] = i;
        return RenameVariables(poly, newVariables, poly.ordering);
    }


    public static MultivariatePolynomial<E> RenameVariables(MultivariatePolynomial<E> poly, int[]
        newVariables)
    {
        return RenameVariables(poly, newVariables, poly.ordering);
    }


    public static Monomial<E> RenameVariables(Monomial<E> e, int[] newVariables)
    {
        return e.SetDegreeVector(Map(e.exponents, newVariables), e.totalDegree);
    }


    public static MultivariatePolynomial<E> RenameVariables(MultivariatePolynomial<E> poly, int[]
        newVariables, IComparer<DegreeVector> newOrdering)
    {
        // NOTE: always return a copy of poly, even if order of variables is unchanged
        var data = new MonomialSet<E>(newOrdering);
        foreach (var e in poly.terms)
            data.Add(RenameVariables(e, newVariables));
        return poly.Create(data);
    }


    private static int[] Map(int[] degrees, int[] mapping)
    {
        int[] newDegrees = new int[degrees.Length];
        for (int i = 0; i < degrees.Length; i++)
            newDegrees[i] = degrees[mapping[i]];
        return newDegrees;
    }


    /* private factory */
    MultivariatePolynomial<E> Create(MonomialSet<E> terms)
    {
        return Create(nVariables, ordering, terms);
    }


    /* private factory */
    MultivariatePolynomial<E> Create(int nVariables, MonomialSet<E> terms)
    {
        return Create(nVariables, ordering, terms);
    }

    public MultivariatePolynomial<E> Create(params Monomial<E>[] terms)
    {
        return Create(terms.ToList());
    }


    public MultivariatePolynomial<E> Create(IEnumerable<Monomial<E>> terms)
    {
        MonomialSet<E> monomials = new MonomialSet<E>(ordering);
        foreach (var term in terms)
        {
            if (term.exponents.Length != nVariables)
                throw new ArgumentException();
            Add(monomials, term);
        }

        return Create(monomials);
    }


    public MultivariatePolynomial<E> Create(Monomial<E> term)
    {
        if (term.exponents.Length != nVariables)
            throw new ArgumentException();
        MonomialSet<E> monomials = new MonomialSet<E>(ordering);
        Add(monomials, term);
        return Create(monomials);
    }


    public MultivariatePolynomial<E> Create(DegreeVector term)
    {
        return Create(monomialAlgebra.Create(term));
    }


    public MultivariatePolynomial<E> CreateMonomial(int variable, int degree)
    {
        int[] degreeVector = new int[nVariables];
        degreeVector[variable] = degree;
        return Create(monomialAlgebra.Create(degreeVector));
    }


    public MultivariatePolynomial<E> SetOrdering(IComparer<DegreeVector> newOrdering)
    {
        if (ordering.Equals(newOrdering))
            return Clone();
        MonomialSet<E> newData = new MonomialSet<E>(newOrdering);
        newData.PutAll(terms);
        return Create(nVariables, newOrdering, newData);
    }


    MultivariatePolynomial<E> SetOrderingUnsafe(IComparer<DegreeVector> newOrdering)
    {
        if (ordering.Equals(newOrdering))
            return this;
        return SetOrdering(newOrdering);
    }


    protected virtual void Release()
    {
        cachedDegrees = null;
        cachedDegree = -1;
    }


    public int Size()
    {
        return terms.Count;
    }


    public bool IsZero()
    {
        return terms.Count == 0;
    }


    public virtual bool IsLinearOrConstant()
    {
        if (Size() > 2)
            return false;
        if (IsConstant())
            return true;
        if (IsZeroCC())
            return Size() == 1;
        else
            return Size() == 2;
    }


    public virtual bool IsLinearExactly()
    {
        if (Size() > 2)
            return false;
        if (IsConstant())
            return false;
        if (IsZeroCC())
            return Size() == 1;
        else
            return Size() == 2;
    }


    public virtual bool IsZeroCC()
    {
        return !terms.ContainsKey(new DegreeVector(new int[nVariables], 0));
    }


    public IEnumerable<Monomial<E>> Iterator()
    {
        return terms.Iterator();
    }


    public virtual IEnumerable<Monomial<E>> AscendingIterator()
    {
        return terms.Values;
    }


    public virtual IEnumerable<Monomial<E>> DescendingIterator()
    {
        return terms.DescendingMap().Values().Iterator();
    }


    public virtual Monomial<E> First()
    {
        return terms.First();
    }


    public virtual Monomial<E> Last()
    {
        return terms.Last();
    }


    public Collection<Monomial<E>> Collection()
    {
        return terms.Values;
    }


    public Monomial<E>[] ToArray()
    {
        return terms.Values.ToArray();
    }


    public override bool IsMonomial()
    {
        return Size() <= 1;
    }


    public bool IsVariable()
    {
        return IsMonomial() && IsEffectiveUnivariate() && LcAsPoly().IsOne() && !IsConstant();
    }


    public MultivariatePolynomial<E> ToZero()
    {
        terms.Clear();
        Release();
        return this;
    }


    public MultivariatePolynomial<E> Set(MultivariatePolynomial<E> oth)
    {
        if (oth == this)
            return this;
        AssertSameCoefficientRingWith(oth);
        return LoadFrom(oth.terms);
    }


    public MultivariatePolynomial<E> LoadFrom(MonomialSet<E> map)
    {
        terms.Clear();
        terms.PutAll(map);
        Release();
        return this;
    }


    public MultivariatePolynomial<E> DropVariable(int variable)
    {
        MonomialSet<E> newData = new MonomialSet<E>(ordering);
        foreach (var term in terms)
            newData.Add(term.Without(variable));
        return Create(nVariables - 1, newData);
    }


    public MultivariatePolynomial<E> DropVariables(int[] variables)
    {
        MonomialSet<E> newData = new MonomialSet<E>(ordering);
        foreach (var term in terms)
            newData.Add(term.Without(variables));
        return Create(nVariables - variables.Length, newData);
    }


    public MultivariatePolynomial<E> DropSelectVariables(params int[] variables)
    {
        MonomialSet<E> newData = new MonomialSet<E>(ordering);
        foreach (var term in terms)
            newData.Add(term.DropSelect(variables));
        return Create(variables.Length, newData);
    }


    public MultivariatePolynomial<E> InsertVariable(int variable)
    {
        MonomialSet<E> newData = new MonomialSet<E>(ordering);
        foreach (var term in terms)
            newData.Add(term.Insert(variable));
        return Create(nVariables + 1, newData);
    }


    public MultivariatePolynomial<E> InsertVariable(int variable, int count)
    {
        MonomialSet<E> newData = new MonomialSet<E>(ordering);
        foreach (var term in terms)
            newData.Add(term.Insert(variable, count));
        return Create(nVariables + count, newData);
    }


    public MultivariatePolynomial<E> SetNVariables(int newNVariables)
    {
        if (newNVariables == nVariables)
            return this;
        MonomialSet<E> newData = new MonomialSet<E>(ordering);
        foreach (var term in terms)
            newData.Add(term.SetNVariables(newNVariables));
        return Create(newNVariables, newData);
    }


    public MultivariatePolynomial<E> MapVariables(int[] mapping)
    {
        int newNVars = ArraysUtil.Max(mapping) + 1;
        MonomialSet<E> newData = new MonomialSet<E>(ordering);
        foreach (var term in terms)
            newData.Add(term.Map(newNVars, mapping));
        return Create(newNVars, newData);
    }


    public MultivariatePolynomial<E> JoinNewVariable()
    {
        return JoinNewVariables(1);
    }


    public MultivariatePolynomial<E> JoinNewVariables(int n)
    {
        MonomialSet<E> newData = new MonomialSet<E>(ordering);
        foreach (var term in terms)
            newData.Add(term.JoinNewVariables(n));
        return Create(nVariables + n, newData);
    }


    MultivariatePolynomial<E> JoinNewVariables(int newNVariables, int[] mapping)
    {
        MonomialSet<E> newData = new MonomialSet<E>(ordering);
        foreach (var term in terms)
            newData.Add(term.JoinNewVariables(newNVariables, mapping));
        return Create(newNVariables, newData);
    }


    public int NUsedVariables()
    {
        int[] degrees = DegreesRef();
        int r = 0;
        foreach (int d in degrees)
            if (d != 0)
                ++r;
        return r;
    }


    private int cachedDegree = -1;


    public virtual int Degree()
    {
        // fixme replace with degreeSum ?
        if (cachedDegree == -1)
        {
            int max = 0;
            foreach (var db in terms)
                max = Math.Max(max, db.totalDegree);
            cachedDegree = max;
        }

        return cachedDegree;
    }


    public virtual int Degree(params int[] variables)
    {
        int max = 0;
        foreach (var db in terms)
            max = Math.Max(max, db.DvTotalDegree(variables));
        return max;
    }


    public virtual int DegreeMax()
    {
        return ArraysUtil.Max(DegreesRef());
    }


    public int Degree(int variable)
    {
        return DegreesRef()[variable];
    }


    private int[] cachedDegrees = null;


    protected virtual int[] DegreesRef()
    {
        if (cachedDegrees == null)
        {
            int[] degrees = new int[nVariables];
            foreach (var db in terms)
                for (int i = 0; i < nVariables; i++)
                    if (db.exponents[i] > degrees[i])
                        degrees[i] = db.exponents[i];
            return cachedDegrees = degrees;
        }

        return cachedDegrees;
    }


    public int[] Degrees()
    {
        return (int[])DegreesRef().Clone();
    }


    public int[] Occurrences()
    {
        int[] occurrences = new int[nVariables];
        foreach (var t in terms)
        {
            for (int i = 0; i < nVariables; i++)
            {
                if (t.exponents[i] > 0)
                    ++occurrences[i];
            }
        }

        return occurrences;
    }


    public int[] UniqueOccurrences()
    {
        TIntHashSet[] degrees = new TIntHashSet[nVariables];
        for (int i = 0; i < nVariables; i++)
        {
            degrees[i] = new TIntHashSet();
        }

        int[] occurrences = new int[nVariables];
        foreach (var t in terms)
        {
            for (int i = 0; i < nVariables; i++)
            {
                int exp = t.exponents[i];
                if (exp > 0 && !degrees[i].Contains(exp))
                {
                    degrees[i].Add(exp);
                    ++occurrences[i];
                }
            }
        }

        return occurrences;
    }


    public int[] Multidegree()
    {
        return Lt().exponents;
    }


    public int[] Degrees(int variable)
    {
        TIntHashSet degrees = new TIntHashSet();
        foreach (var db in terms)
            degrees.Add(db.exponents[variable]);
        return degrees.ToArray();
    }


    public int DegreeSum()
    {
        return ArraysUtil.Sum(DegreesRef());
    }


    public int TotalDegree()
    {
        return DegreeSum();
    }


    public virtual double Sparsity()
    {
        double sparsity = Size();
        foreach (int d in DegreesRef())
        {
            if (d != 0)
                sparsity /= (d + 1);
        }

        return sparsity;
    }


    public virtual double Sparsity2()
    {
        TIntHashSet distinctTotalDegrees = new TIntHashSet();
        terms.KeySet().Stream().MapToInt((dv) => dv.totalDegree).ForEach(distinctTotalDegrees.Add());
        TIntIterator it = distinctTotalDegrees.Iterator();
        double nDenseTerms = 0;
        while (it.HasNext())
        {
            int deg = it.Next();
            double d = BigIntegerUtil.Binomial(deg + nVariables - 1, deg).DoubleValue();
            nDenseTerms += d;
            if (d == Double.MAX_VALUE)
                return Size() / d;
        }

        return Size() / nDenseTerms;
    }


    public int Ecart()
    {
        return DegreeSum() - Lt().totalDegree;
    }


    public bool IsHomogeneous()
    {
        int deg = -1;
        foreach (var term in terms)
            if (deg == -1)
                deg = term.totalDegree;
            else if (term.totalDegree != deg)
                return false;
        return true;
    }


    public MultivariatePolynomial<E> Homogenize(int variable)
    {
        int deg = TotalDegree();
        MonomialSet<E> result = new MonomialSet<E>(ordering);
        foreach (var term in terms)
        {
            DegreeVector dv = term.DvInsert(variable);
            dv = dv.DvSet(variable, deg - dv.totalDegree);
            result.Add(term.SetDegreeVector(dv));
        }

        return Create(nVariables + 1, result);
    }


    public bool IsEffectiveUnivariate()
    {
        return UnivariateVariable() != -1;
    }


    public int UnivariateVariable()
    {
        if (IsConstant())
            return 0;
        if (nVariables == 1)
            return 0;
        int[] degrees = DegreesRef();
        int var = -1;
        for (int i = 0; i < nVariables; i++)
        {
            if (degrees[i] != 0)
            {
                if (var != -1)
                    return -1;
                else
                    var = i;
            }
        }

        return var;
    }


    public MultivariatePolynomial<E> CoefficientOf(int variable, int exponent)
    {
        MultivariatePolynomial<E> result = CreateZero();
        foreach (var e in terms)
        {
            if (e.exponents[variable] != exponent)
                continue;
            result.Add(e.SetZero(variable));
        }

        return result;
    }


    public MultivariatePolynomial<E> CoefficientOf(int[] variables, int[] exponents)
    {
        if (variables.Length != exponents.Length)
            throw new ArgumentException();
        MultivariatePolynomial<E> result = CreateZero();
            out:
        foreach (var e in terms)
        {
            for (int i = 0; i < variables.Length; i++)
                if (e.exponents[variables[i]] != exponents[i])
                    continue;
            result.Add(e.SetZero(variables));
        }

        return result;
    }


    public MultivariatePolynomial<E> DropCoefficientOf(int[] variables, int[] exponents)
    {
        if (variables.Length != exponents.Length)
            throw new ArgumentException();
        MultivariatePolynomial<E> result = CreateZero();
        var it = terms.Iterator();
            out:
        while (it.HasNext())
        {
            Term e = it.Next();
            for (int i = 0; i < variables.Length; i++)
                if (e.exponents[variables[i]] != exponents[i])
                    continue;
            result.Add(e.SetZero(variables));
            it.Remove();
        }

        return result;
    }


    public UnivariatePolynomial<Poly> AsUnivariate(int variable)
    {
        MultivariateRing<Poly> ring = new MultivariateRing(this);
        Poly[] univarData = ring.CreateZeroesArray(Degree(variable) + 1);
        foreach (Term e in terms)
            univarData[e.exponents[variable]].Add(e[variable] = 0);
        return UnivariatePolynomial.CreateUnsafe(ring, univarData);
    }


    public UnivariatePolynomial<Poly> AsUnivariateEliminate(int variable)
    {
        MultivariateRing<Poly> ring = new MultivariateRing(CreateZero().DropVariable(variable));
        Poly[] univarData = ring.CreateZeroesArray(Degree(variable) + 1);
        foreach (Term e in terms)
            univarData[e.exponents[variable]].Add(e.Without(variable));
        return UnivariatePolynomial.CreateUnsafe(ring, univarData);
    }


    public static Poly AsMultivariate<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(
        UnivariatePolynomial<Poly> univariate, int uVariable, bool join)
    {
        Poly factory = univariate[0];
        if (join)
            factory = factory.InsertVariable(uVariable);
        Poly result = factory.CreateZero();
        for (int i = 0; i <= univariate.Degree(); i++)
        {
            Poly cf = univariate[i];
            if (join)
                cf = cf.InsertVariable(uVariable);
            result.Add(cf.Multiply(factory.CreateMonomial(uVariable, i)));
        }

        return result;
    }


    public abstract MultivariatePolynomial<TWildcardTodoIUnivariatePolynomial> AsOverUnivariate(int variable);


    public abstract MultivariatePolynomial<TWildcardTodoIUnivariatePolynomial> AsOverUnivariateEliminate(int variable);


    public abstract MultivariatePolynomial<Poly> AsOverMultivariate(params int[] variables);


    public MultivariatePolynomial<Poly> AsOverMultivariateEliminate(params int[] variables)
    {
        return AsOverMultivariateEliminate(variables, ordering);
    }


    public abstract MultivariatePolynomial<Poly> AsOverMultivariateEliminate(int[] variables,
        IComparer<DegreeVector> ordering);


    public static Poly AsMultivariate<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(
        UnivariatePolynomial<Poly> uPoly, int variable)
    {
        Poly result = uPoly.ring.GetZero();
        for (int i = uPoly.degree(); i >= 0; --i)
        {
            if (uPoly.IsZeroAt(i))
                continue;
            result.Add(result.CreateMonomial(variable, i).Multiply(uPoly[i]));
        }

        return result;
    }


    public abstract Poly PrimitivePart(int variable);


    public abstract IUnivariatePolynomial ContentUnivariate(int variable);


    public abstract Poly Monic(IComparer<DegreeVector> ordering);


    public abstract Poly MonicWithLC(IComparer<DegreeVector> ordering, Poly oth);


    public Poly Content(int variable)
    {
        return AsMultivariate(ContentUnivariate(variable), nVariables, variable, ordering);
    }


    public Poly ContentExcept(int variable)
    {
        return AsUnivariate(variable).Content();
    }


    public Poly MultiplyByMonomial(int variable, int exponent)
    {
        if (exponent == 0)
            return this;
        Collection<Term> oldData = new List(terms.Values());
        terms.Clear();
        foreach (Term term in oldData)
            terms.Add(term[variable] = term.exponents[variable] + exponent);
        Release();
        return this;
    }


    public Poly Lc(int variable)
    {
        int degree = Degree(variable);
        Poly result = CreateZero();
        foreach (Term term in this)
            if (term.exponents[variable] == degree)
                result.Add(term[variable] = 0);
        return result;
    }


    public Poly SetLC(int variable, Poly lc)
    {
        int degree = Degree(variable);
        lc = lc.Clone().MultiplyByMonomial(variable, degree);
        Iterator<Map.Entry<DegreeVector, Term>> it = terms.EntrySet().Iterator();
        while (it.HasNext())
        {
            Term term = it.Next().GetValue();
            if (term.exponents[variable] == degree)
                it.Remove();
        }

        terms.PutAll(lc.terms);
        Release();
        return this;
    }


    public Term Lt(IComparer<DegreeVector> ordering)
    {
        if (ordering.Equals(this.ordering))
            return Lt();
        if (Size() == 0)
            return monomialAlgebra.GetZeroTerm(nVariables);
        return terms.Values().Stream().Max(ordering).Get();
    }


    public Term Lt()
    {
        return Size() == 0 ? monomialAlgebra.GetZeroTerm(nVariables) : terms.Last();
    }


    public Term Mt()
    {
        return Size() == 0 ? monomialAlgebra.GetZeroTerm(nVariables) : terms.First();
    }


    public abstract Poly LcAsPoly(IComparer<DegreeVector> ordering);


    public Poly LtAsPoly()
    {
        return Create(Lt());
    }


    public Term MonomialContent()
    {
        return CommonContent(null);
    }


    Term CommonContent(Term monomial)
    {
        if (!CcAsPoly().IsZero())
            return monomialAlgebra.GetUnitTerm(nVariables);
        int[] exponents = monomial == null ? null : monomial.exponents.Clone();
        int totalDegree = -1;
        foreach (Term degreeVector in terms)
            if (exponents == null)
            {
                exponents = degreeVector.exponents.Clone();
                totalDegree = degreeVector.totalDegree;
            }
            else
            {
                totalDegree = SetMin(degreeVector.exponents, exponents);
                if (totalDegree == 0)
                    break;
            }

        if (exponents == null)
            return monomialAlgebra.GetUnitTerm(nVariables);
        return monomialAlgebra.Create(new DegreeVector(exponents, totalDegree));
    }


    static int SetMin(int[] dv, int[] exponents)
    {
        int sum = 0;
        for (int i = 0; i < exponents.Length; ++i)
        {
            if (dv[i] < exponents[i])
                exponents[i] = dv[i];
            sum += exponents[i];
        }

        return sum;
    }


    public Poly DivideDegreeVectorOrNull(DegreeVector monomial)
    {
        if (monomial.IsZeroVector())
            return this;
        MonomialSet<Term> map = new MonomialSet(ordering);
        foreach (Term term in terms)
        {
            Term dv = term.DivideOrNull(monomial);
            if (dv == null)
                return null;
            map.Add(dv);
        }

        return LoadFrom(map);
    }


    public void CheckSameDomainWith(Term oth)
    {
        if (nVariables != oth.exponents.Length)
            throw new ArgumentException("Combining multivariate polynomials from different fields: this.nVariables = " +
                                        nVariables + " oth.nVariables = " + oth.NVariables());
    }


    public abstract Poly DivideOrNull(Term monomial);


    abstract void Add(MonomialSet<Term> terms, Term term);


    abstract void Subtract(MonomialSet<Term> terms, Term term);


    public Poly Add(Poly oth)
    {
        if (terms == oth.terms)
            return Multiply(2);
        AssertSameCoefficientRingWith(oth);
        if (oth.IsZero())
            return this;
        foreach (Term term in oth.terms)
            Add(terms, term);
        Release();
        return this;
    }


    public Poly Subtract(Poly oth)
    {
        if (terms == oth.terms)
            return ToZero();
        AssertSameCoefficientRingWith(oth);
        if (oth.IsZero())
            return this;
        foreach (Term term in oth.terms)
            Subtract(terms, term);
        Release();
        return this;
    }


    public Poly Subtract(Term cf, Poly oth)
    {
        if (monomialAlgebra.IsZero(cf))
            return this;
        if (terms == oth.terms && monomialAlgebra.IsOne(cf))
            return ToZero();
        AssertSameCoefficientRingWith(oth);
        if (oth.IsZero())
            return this;
        foreach (Term term in oth.terms)
            Subtract(terms, monomialAlgebra.Multiply(cf, term));
        Release();
        return this;
    }


    public Poly Add(Term monomial)
    {
        CheckSameDomainWith(monomial);
        Add(terms, monomial);
        Release();
        return this;
    }


    public Poly Put(Term monomial)
    {
        CheckSameDomainWith(monomial);
        terms.Add(monomial);
        Release();
        return this;
    }


    public Poly Subtract(Term monomial)
    {
        CheckSameDomainWith(monomial);
        Subtract(terms, monomial);
        Release();
        return this;
    }


    public Poly Negate()
    {
        foreach (Map.Entry<DegreeVector, Term> entry in terms.EntrySet())
        {
            Term term = entry.GetValue();
            entry.SetValue(monomialAlgebra.Negate(term));
        }

        Release();
        return this;
    }


    public Poly Add(Iterable<Term> monomials)
    {
        foreach (Term term in monomials)
            Add(term);
        return this;
    }


    public Poly Add(params Term[] monomials)
    {
        return Add(Arrays.AsList(monomials));
    }


    // todo rename to tail
    // todo move to IPolynomial
    public Poly SubtractLt()
    {
        terms.PollLastEntry();
        Release();
        return this;
    }


    public abstract Poly Multiply(Term monomial);


    public Poly MultiplyByDegreeVector(DegreeVector dv)
    {
        if (dv.IsZeroVector())
            return this;
        return Multiply(monomialAlgebra.Create(dv));
    }


    public HashSet<DegreeVector> GetSkeleton()
    {
        return Collections.UnmodifiableSet(terms.KeySet());
    }


    public Poly SetAllCoefficientsToUnit()
    {
        Term unit = monomialAlgebra.GetUnitTerm(nVariables);
        foreach (Map.Entry<DegreeVector, Term> entry in terms.EntrySet())
            entry.SetValue(unit.SetDegreeVector(entry.GetKey()));
        Release();
        return this;
    }


    public HashSet<DegreeVector> GetSkeleton(params int[] variables)
    {
        return terms.KeySet().Stream().Map((dv) => dv.DvSelect(variables))
            .Collect(Collectors.ToCollection(() => new TreeSet(ordering)));
    }


    public HashSet<DegreeVector> GetSkeletonDrop(params int[] variables)
    {
        int[] variablesSorted = variables.Clone();
        Arrays.Sort(variablesSorted);
        return terms.KeySet().Stream().Map((dv) => dv.DvDropSelect(variablesSorted))
            .Collect(Collectors.ToCollection(() => new TreeSet(ordering)));
    }


    public HashSet<DegreeVector> GetSkeletonExcept(params int[] variables)
    {
        return terms.KeySet().Stream().Map((dv) => dv.DvSetZero(variables))
            .Collect(Collectors.ToCollection(() => new TreeSet(ordering)));
    }


    public bool SameSkeletonQ(AMultivariatePolynomial oth)
    {
        return GetSkeleton().Equals(oth.GetSkeleton());
    }


    public bool SameSkeletonQ(AMultivariatePolynomial oth, params int[] variables)
    {
        return GetSkeleton(variables).Equals(oth.GetSkeleton(variables));
    }


    public bool SameSkeletonExceptQ(AMultivariatePolynomial oth, params int[] variables)
    {
        return GetSkeletonExcept(variables).Equals(oth.GetSkeletonExcept(variables));
    }


    public Poly Derivative(int variable)
    {
        return Derivative(variable, 1);
    }


    public abstract Poly Derivative(int variable, int order);


    public abstract Poly SeriesCoefficient(int variable, int order);


    public Poly EvaluateAtZero(int variable)
    {
        MonomialSet<Term> newData = new MonomialSet(ordering);
        foreach (Term el in terms)
            if (el.exponents[variable] == 0)
                newData.Add(el);
        return Create(newData);
    }


    public Poly EvaluateAtZero(int[] variables)
    {
        if (variables.Length == 0)
            return Clone();
        MonomialSet<Term> newData = new MonomialSet(ordering);
            out:
        foreach (Term el in terms)
        {
            foreach (int variable in variables)
                if (el.exponents[variable] != 0)
                    continue;
            newData.Add(el);
        }

        return Create(newData);
    }


    public Poly[] Derivative()
    {
        Poly[] result = CreateArray(nVariables);
        for (int i = 0; i < nVariables; ++i)
            result[i] = Derivative(i);
        return result;
    }


    public MultivariatePolynomial<Poly> AsOverPoly(Poly factory)
    {
        MonomialSet<Monomial<Poly>> newTerms = new MonomialSet(ordering);
        foreach (Term term in terms)
            newTerms.Add(new Monomial(term, factory.CreateConstantFromTerm(term)));
        return new MultivariatePolynomial(nVariables, Rings.MultivariateRing(factory), ordering, newTerms);
    }


    public Poly Composition(params Poly[] values)
    {
        if (values.Length != nVariables)
            throw new ArgumentException();
        Poly factory = values[0];
        return AsOverPoly(factory).Evaluate(values);
    }


    public sPoly Composition<sPoly extends IUnivariatePolynomial<sPoly>>(params sPoly[] values)
    {
        if (values.Length != nVariables)
            throw new ArgumentException();
        return Composition(Rings.UnivariateRing(values[0]), values);
    }


    public sPoly Composition<sPoly extends IUnivariatePolynomial<sPoly>>(Ring<sPoly> uRing, params sPoly[] values)
    {
        if (values.Length != nVariables)
            throw new ArgumentException();
        sPoly factory = values[0];
        if (this is MultivariatePolynomialZp64)
            return ((MultivariatePolynomialZp64)this).MapCoefficients(uRing, uRing.ValueOf()).Evaluate(values);
        else
            return (sPoly)((MultivariatePolynomial)this)
                .MapCoefficients(uRing, (cf) => ((UnivariatePolynomial)factory).CreateConstant(cf)).Evaluate(values);
    }


    public Poly Composition(IList<Poly> values)
    {
        if (nVariables == 0)
            return this;
        return Composition(values.ToArray(values[0].CreateArray(values.Count)));
    }


    public Poly Composition(int variable, Poly value)
    {
        AssertSameCoefficientRingWith(value);
        return AsUnivariate(variable).Evaluate(value);
    }


    public Poly Composition(int[] variables, Poly[] values)
    {
        if (variables.Length == 0)
            throw new ArgumentException();
        if (variables.Length != values.Length)
            throw new ArgumentException();
        AssertSameCoefficientRingWith(values[0]);
        variables = variables.Clone();
        values = values.Clone();
        ArraysUtil.QuickSort(variables, values);

        // R[variables][other_variables] => R[other_variables][variables]
        int[] mainVariables = ArraysUtil.IntSetDifference(ArraysUtil.Sequence(0, nVariables), variables);
        MultivariatePolynomial<Poly> r = AsOverMultivariate(mainVariables).Evaluate(variables, values);
        return r.Cc();
    }


    // R[variables][other_variables] => R[other_variables][variables]
    public virtual bool Equals(object o)
    {
        if (this == o)
            return true;
        if (o == null || GetType() != o.GetType())
            return false;
        AMultivariatePolynomial < ?, ?> that = (AMultivariatePolynomial < ?,  ?>)o;
        if (nVariables != that.nVariables)
            return false;
        return terms.Equals(that.terms);
    }


    public virtual int GetHashCode()
    {
        return terms.GetHashCode();
    }


    public virtual int SkeletonHashCode()
    {
        return terms.SkeletonHashCode();
    }


    public abstract Poly Clone();


    public abstract Poly EvaluateAtRandom(int variable, RandomGenerator rnd);


    public abstract Poly EvaluateAtRandomPreservingSkeleton(int variable, RandomGenerator rnd);


    public abstract MultivariatePolynomial<E> MapCoefficientsAsPolys<E>(Ring<E> ring, Func<Poly, E> mapper);


    public sealed class PolynomialCollector<Term, Poly> : Collector<Term, Poly, Poly>
    {
        readonly Supplier<Poly> supplier;
        readonly BiConsumer<Poly, Term> accumulator = Poly.Add();

        readonly BinaryOperator<Poly> combiner = (l, r) =>
        {
            l.Add(r);
            return l;
        };

        readonly Func<Poly, Poly> finisher = Func.Identity();

        public PolynomialCollector(Supplier<Poly> supplier)
        {
            this.supplier = supplier;
        }

        public Supplier<Poly> Supplier()
        {
            return supplier;
        }

        public BiConsumer<Poly, Term> Accumulator()
        {
            return accumulator;
        }

        public BinaryOperator<Poly> Combiner()
        {
            return combiner;
        }

        public Func<Poly, Poly> Finisher()
        {
            return finisher;
        }

        public HashSet<Characteristics> Characteristics()
        {
            return EnumSet.Of(Characteristics.IDENTITY_FINISH);
        }
    }


    public string ToString()
    {
        return ToString(IStringifier.Dummy());
    }


    public static long[] KroneckerMap(int[] degrees)
    {
        long[] result = new long[degrees.Length];
        result[0] = 1;
        for (int i = 1; i < degrees.Length; i++)
        {
            result[i] = 1;
            double check = 1;
            for (int j = 0; j < i; j++)
            {
                long b = 2 * degrees[j] + 1;
                result[i] *= b;
                check *= b;
            }

            if (check > long.MaxValue)
            {
                // long overflow -> can't use Kronecker's trick
                return null;
            }
        }

        return result;
    }


    public const int KRONECKER_THRESHOLD = 256;
}