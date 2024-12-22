using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Text;
using Polynomials.Poly.Univar;
using Polynomials.Utils;
using MultivariatePolynomialZp64 = Polynomials.Poly.Multivar.MultivariatePolynomial<long>;

namespace Polynomials.Poly.Multivar;

public class MultivariatePolynomial<E> : Polynomial<MultivariatePolynomial<E>>, MonomialSetView<E>
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
        int[] newVariables = Utils.Utils.Sequence(poly.nVariables);
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
    public MultivariatePolynomial<E> Create(MonomialSet<E> terms)
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
        foreach (var term in terms)
            newData.Add(term);
        
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
        return terms.DescendingIterator();
    }


    public virtual Monomial<E> First()
    {
        return ((MonomialSetView<E>) terms).First();
    }


    public virtual Monomial<E> Last()
    {
        return ((MonomialSetView<E>) terms).Last();
    }


    public Collection<Monomial<E>> Collection()
    {
        return new Collection<Monomial<E>>(terms.Values.ToList());
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
        if (Equals(oth, this))
            return this;
        AssertSameCoefficientRingWith(oth);
        return LoadFrom(oth.terms);
    }

    public void AssertSameCoefficientRingWith(MultivariatePolynomial<E> oth)
    {
        if (!SameCoefficientRingWith(oth))
            throw new ArgumentException();
    }

    public MultivariatePolynomial<E> LoadFrom(MonomialSet<E> map)
    {
        terms.Clear();
        foreach (var term in map)
            terms.Add(term);
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
        int newNVars = mapping.Max() + 1;
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


    public MultivariatePolynomial<E> JoinNewVariables(int newNVariables, int[] mapping)
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
        return DegreesRef().Max();
    }


    public int Degree(int variable)
    {
        return DegreesRef()[variable];
    }


    private int[]? cachedDegrees = null;


    public virtual int[] DegreesRef()
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
        HashSet<int>[] degrees = new HashSet<int>[nVariables];
        for (int i = 0; i < nVariables; i++)
        {
            degrees[i] = new HashSet<int>();
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
        HashSet<int> degrees = new HashSet<int>();
        foreach (var db in terms)
            degrees.Add(db.exponents[variable]);
        return degrees.ToArray();
    }


    public int DegreeSum()
    {
        return DegreesRef().Sum();
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
        HashSet<int> distinctTotalDegrees = new HashSet<int>();
        terms.Iterator().Select((dv) => dv.totalDegree).ForEach(d => distinctTotalDegrees.Add(d));
        using var it = distinctTotalDegrees.GetEnumerator();
        double nDenseTerms = 0;
        while (it.MoveNext())
        {
            int deg = it.Current;
            double d = (double) BigIntegerUtils.Binomial(deg + nVariables - 1, deg);
            nDenseTerms += d;
            if (d == Double.MaxValue)
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
        foreach (var e in terms)
        {
            for (int i = 0; i < variables.Length; i++)
                if (e.exponents[variables[i]] != exponents[i])
                    goto next;
            result.Add(e.SetZero(variables));
            next : ;
        }

        return result;
    }


    public MultivariatePolynomial<E> DropCoefficientOf(int[] variables, int[] exponents)
    {
        if (variables.Length != exponents.Length)
            throw new ArgumentException();
        MultivariatePolynomial<E> result = CreateZero();
        using var it = terms.Iterator().GetEnumerator();
        while (it.MoveNext())
        {
            var e = it.Current;
            for (int i = 0; i < variables.Length; i++)
                if (e.exponents[variables[i]] != exponents[i])
                    goto next;
            result.Add(e.SetZero(variables));
            terms.Remove(e);
            
            next : ;
        }

        return result;
    }

    public UnivariatePolynomial<MultivariatePolynomial<E>> AsUnivariate(int variable)
    {
        MultivariateRing<E> ring = new MultivariateRing<E>(this);
        MultivariatePolynomial<E>[] univarData = ring.CreateZeroesArray(Degree(variable) + 1);
        foreach (var e in terms)
            univarData[e.exponents[variable]].Add(e.Set(variable, 0));
        return UnivariatePolynomial<MultivariatePolynomial<E>>.CreateUnsafe(ring, univarData);
    }
    

    public UnivariatePolynomial<MultivariatePolynomial<E>> AsUnivariateEliminate(int variable)
    {
        MultivariateRing<E> ring = new MultivariateRing<E>(CreateZero().DropVariable(variable));
        MultivariatePolynomial<E>[] univarData = ring.CreateZeroesArray(Degree(variable) + 1);
        foreach (var e in terms)
            univarData[e.exponents[variable]].Add(e.Without(variable));
        return UnivariatePolynomial<MultivariatePolynomial<E>>.CreateUnsafe(ring, univarData);
    }


    public static MultivariatePolynomial<E> AsMultivariate(
        UnivariatePolynomial<MultivariatePolynomial<E>> univariate, int uVariable, bool join)
    {
        var factory = univariate[0];
        if (join)
            factory = factory.InsertVariable(uVariable);
        var result = factory.CreateZero();
        for (int i = 0; i <= univariate.Degree(); i++)
        {
            var cf = univariate[i];
            if (join)
                cf = cf.InsertVariable(uVariable);
            result.Add(cf.Multiply(factory.CreateMonomial(uVariable, i)));
        }

        return result;
    }

    public MultivariatePolynomial<MultivariatePolynomial<E>> AsOverMultivariateEliminate(params int[] variables)
    {
        return AsOverMultivariateEliminate(variables, ordering);
    }


    public static MultivariatePolynomial<E> AsMultivariate(
        UnivariatePolynomial<MultivariatePolynomial<E>> uPoly, int variable)
    {
        var result = uPoly.ring.GetZero();
        for (int i = uPoly.Degree(); i >= 0; --i)
        {
            if (uPoly.IsZeroAt(i))
                continue;
            result.Add(result.CreateMonomial(variable, i).Multiply(uPoly[i]));
        }

        return result;
    }


    public MultivariatePolynomial<E> Content(int variable)
    {
        return AsMultivariate(ContentUnivariate(variable), nVariables, variable, ordering);
    }

    public MultivariatePolynomial<E> ContentExcept(int variable)
    {
        return AsUnivariate(variable).Content();
    }


    public MultivariatePolynomial<E> MultiplyByMonomial(int variable, int exponent)
    {
        if (exponent == 0)
            return this;
        var oldData = new List<Monomial<E>>(terms.Values);
        terms.Clear();
        foreach (var term in oldData)
            terms.Add(term.Set(variable, term.exponents[variable] + exponent));
        Release();
        return this;
    }


    public MultivariatePolynomial<E> Lc(int variable)
    {
        int degree = Degree(variable);
        var result = CreateZero();
        foreach (var term in terms)
            if (term.exponents[variable] == degree)
                result.Add(term.Set(variable, 0));
        return result;
    }


    public MultivariatePolynomial<E> SetLC(int variable, MultivariatePolynomial<E> lc)
    {
        int degree = Degree(variable);
        lc = lc.Clone().MultiplyByMonomial(variable, degree);
        using var it = terms.EntryIterator().GetEnumerator();
        while (it.MoveNext())
        {
            var term = it.Current;
            if (term.Value.exponents[variable] == degree)
                terms.Remove(term.Key);
        }
        
        foreach (var term in lc.terms)
            terms.Add(term);
        Release();
        return this;
    }


    public Monomial<E> Lt(IComparer<DegreeVector> ordering)
    {
        if (ordering.Equals(this.ordering))
            return Lt();
        if (Size() == 0)
            return monomialAlgebra.GetZeroTerm(nVariables);
        return (Monomial<E>) terms.Values.Max(ordering);
    }


    public Monomial<E> Lt()
    {
        return Size() == 0 ? monomialAlgebra.GetZeroTerm(nVariables) : ((MonomialSetView<E>)terms).Last();
    }


    public Monomial<E> Mt()
    {
        return Size() == 0 ? monomialAlgebra.GetZeroTerm(nVariables) : ((MonomialSetView<E>)terms).First();
    }


    public MultivariatePolynomial<E> LtAsPoly()
    {
        return Create(Lt());
    }


    public Monomial<E> MonomialContent()
    {
        return CommonContent(null);
    }


    public Monomial<E> CommonContent(Monomial<E>? monomial)
    {
        if (!CcAsPoly().IsZero())
            return monomialAlgebra.GetUnitTerm(nVariables);
        int[]? exponents = monomial == null ? null : (int[])monomial.exponents.Clone();
        int totalDegree = -1;
        foreach (Monomial<E> degreeVector in terms)
            if (exponents == null)
            {
                exponents = (int[])degreeVector.exponents.Clone();
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


    public static int SetMin(int[] dv, int[] exponents)
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


    public MultivariatePolynomial<E>? DivideDegreeVectorOrNull(DegreeVector monomial)
    {
        if (monomial.IsZeroVector())
            return this;
        MonomialSet<E> map = new MonomialSet<E>(ordering);
        foreach (Monomial<E> term in terms)
        {
            var dv = term.DivideOrNull(monomial);
            if (dv == null)
                return null;
            map.Add(dv);
        }

        return LoadFrom(map);
    }


    public void CheckSameDomainWith(Monomial<E> oth)
    {
        if (nVariables != oth.exponents.Length)
            throw new ArgumentException("Combining multivariate polynomials from different fields: this.nVariables = " +
                                        nVariables + " oth.nVariables = " + oth.NVariables());
    }


    public MultivariatePolynomial<E> Add(MultivariatePolynomial<E> oth)
    {
        if (terms == oth.terms)
            return Multiply(2);
        AssertSameCoefficientRingWith(oth);
        if (oth.IsZero())
            return this;
        foreach (Monomial<E> term in oth.terms)
            Add(terms, term);
        Release();
        return this;
    }


    public MultivariatePolynomial<E> Subtract(MultivariatePolynomial<E> oth)
    {
        if (terms == oth.terms)
            return ToZero();
        AssertSameCoefficientRingWith(oth);
        if (oth.IsZero())
            return this;
        foreach (Monomial<E> term in oth.terms)
            Subtract(terms, term);
        Release();
        return this;
    }


    public MultivariatePolynomial<E> Subtract(Monomial<E> cf, MultivariatePolynomial<E> oth)
    {
        if (monomialAlgebra.IsZero(cf))
            return this;
        if (terms == oth.terms && monomialAlgebra.IsOne(cf))
            return ToZero();
        AssertSameCoefficientRingWith(oth);
        if (oth.IsZero())
            return this;
        foreach (Monomial<E> term in oth.terms)
            Subtract(terms, monomialAlgebra.Multiply(cf, term));
        Release();
        return this;
    }


    public MultivariatePolynomial<E> Add(Monomial<E> monomial)
    {
        CheckSameDomainWith(monomial);
        Add(terms, monomial);
        Release();
        return this;
    }


    public MultivariatePolynomial<E> Put(Monomial<E> monomial)
    {
        CheckSameDomainWith(monomial);
        terms.Add(monomial);
        Release();
        return this;
    }


    public MultivariatePolynomial<E> Subtract(Monomial<E> monomial)
    {
        CheckSameDomainWith(monomial);
        Subtract(terms, monomial);
        Release();
        return this;
    }


    public override MultivariatePolynomial<E> Negate()
    {
        foreach (var entry in terms.EntryIterator())
        {
            Monomial<E> term = entry.Value;
            terms[entry.Key] = monomialAlgebra.Negate(term);
        }

        Release();
        return this;
    }


    public MultivariatePolynomial<E> Add(IEnumerable<Monomial<E>> monomials)
    {
        foreach (Monomial<E> term in monomials)
            Add(term);
        return this;
    }


    public MultivariatePolynomial<E> Add(params Monomial<E>[] monomials)
    {
        return Add(monomials.ToList());
    }


    public MultivariatePolynomial<E> SubtractLt()
    {
        terms.Remove(terms.Keys.Last());
        Release();
        return this;
    }


    public MultivariatePolynomial<E> MultiplyByDegreeVector(DegreeVector dv)
    {
        if (dv.IsZeroVector())
            return this;
        return Multiply(monomialAlgebra.Create(dv));
    }


    public ImmutableList<DegreeVector> GetSkeleton()
    {
        return terms.Keys.ToImmutableList();
    }


    public MultivariatePolynomial<E> SetAllCoefficientsToUnit()
    {
        Monomial<E> unit = monomialAlgebra.GetUnitTerm(nVariables);
        foreach (var entry in terms.EntryIterator())
            terms[entry.Key] = unit.SetDegreeVector(entry.Key);
        Release();
        return this;
    }


    public ImmutableList<DegreeVector> GetSkeleton(params int[] variables)
    {
        return terms.Keys.Select((dv) => dv.DvSelect(variables)).ToImmutableList();
    }


    public ImmutableList<DegreeVector> GetSkeletonDrop(params int[] variables)
    {
        int[] variablesSorted = (int[])variables.Clone();
        Array.Sort(variablesSorted);
        return terms.Keys.Select((dv) => dv.DvDropSelect(variablesSorted)).ToImmutableList();
    }


    public ImmutableList<DegreeVector> GetSkeletonExcept(params int[] variables)
    {
        return terms.Keys.Select((dv) => dv.DvSetZero(variables)).ToImmutableList();
    }


    public bool SameSkeletonQ<T>(MultivariatePolynomial<T> oth)
    {
        return GetSkeleton().Equals(oth.GetSkeleton());
    }


    public bool SameSkeletonQ<T>(MultivariatePolynomial<T> oth, params int[] variables)
    {
        return GetSkeleton(variables).Equals(oth.GetSkeleton(variables));
    }


    public bool SameSkeletonExceptQ<T>(MultivariatePolynomial<T> oth, params int[] variables)
    {
        return GetSkeletonExcept(variables).Equals(oth.GetSkeletonExcept(variables));
    }


    public MultivariatePolynomial<E> Derivative(int variable)
    {
        return Derivative(variable, 1);
    }


    public MultivariatePolynomial<E> EvaluateAtZero(int variable)
    {
        MonomialSet<E> newData = new MonomialSet<E>(ordering);
        foreach (Monomial<E> el in terms)
            if (el.exponents[variable] == 0)
                newData.Add(el);
        return Create(newData);
    }


    public MultivariatePolynomial<E> EvaluateAtZero(int[] variables)
    {
        if (variables.Length == 0)
            return Clone();
        MonomialSet<E> newData = new MonomialSet<E>(ordering);
        foreach (Monomial<E> el in terms)
        {
            foreach (int variable in variables)
                if (el.exponents[variable] != 0)
                    goto next;
            newData.Add(el);
            next : ;
        }

        return Create(newData);
    }


    public MultivariatePolynomial<E>[] Derivative()
    {
        var result = new MultivariatePolynomial<E>[nVariables];
        for (int i = 0; i < nVariables; ++i)
            result[i] = Derivative(i);
        return result;
    }

    public MultivariatePolynomial<MultivariatePolynomial<E>> AsOverPoly(MultivariatePolynomial<E> factory)
    {
        MonomialSet<MultivariatePolynomial<E>> newTerms = new MonomialSet<MultivariatePolynomial<E>>(ordering);
        foreach (Monomial<E> term in terms)
            newTerms.Add(new Monomial<MultivariatePolynomial<E>>(term, factory.CreateConstantFromTerm(term)));
        return new MultivariatePolynomial<MultivariatePolynomial<E>>(nVariables, Rings.MultivariateRing(factory),
            ordering, newTerms);
    }
    
    public MultivariatePolynomial<E> Composition(params MultivariatePolynomial<E>[] values)
    {
        if (values.Length != nVariables)
            throw new ArgumentException();
        MultivariatePolynomial<E> factory = values[0];
        return AsOverPoly(factory).Evaluate(values);
    }


    public UnivariatePolynomial<E> Composition(params UnivariatePolynomial<E>[] values)
    {
        if (values.Length != nVariables)
            throw new ArgumentException();
        return Composition(Rings.UnivariateRing(values[0]), values);
    }


    public UnivariatePolynomial<E> Composition(UnivariateRing<E> uRing, params UnivariatePolynomial<E>[] values)
    {
        if (values.Length != nVariables)
            throw new ArgumentException();
        var factory = values[0];

        return MapCoefficients(uRing, cf => factory.CreateConstant(cf)).Evaluate(values);
    }

    public MultivariatePolynomial<E> Composition(List<MultivariatePolynomial<E>> values)
    {
        if (nVariables == 0)
            return this;
        return Composition(values.ToArray());
    }

    public MultivariatePolynomial<E> Composition(int variable, MultivariatePolynomial<E> value)
    {
        AssertSameCoefficientRingWith(value);
        return AsUnivariate(variable).Evaluate(value);
    }

    public MultivariatePolynomial<E> Composition(int[] variables, MultivariatePolynomial<E>[] values)
    {
        if (variables.Length == 0)
            throw new ArgumentException();
        if (variables.Length != values.Length)
            throw new ArgumentException();
        AssertSameCoefficientRingWith(values[0]);
        variables = (int[])variables.Clone();
        values = (MultivariatePolynomial<E>[])values.Clone();
        Array.Sort(variables, values);
    
        // R[variables][other_variables] => R[other_variables][variables]
        int[] mainVariables = Utils.Utils.IntSetDifference(Utils.Utils.Sequence(0, nVariables), variables);
        MultivariatePolynomial<MultivariatePolynomial<E>> r = AsOverMultivariate(mainVariables)
            .Evaluate(variables, values);
        return r.Cc();
    }


    // R[variables][other_variables] => R[other_variables][variables]
    public override bool Equals(object? o)
    {
        if (this == o)
            return true;
        if (o == null || GetType() != o.GetType())
            return false;
        MultivariatePolynomial<E> that = (MultivariatePolynomial<E>)o;
        if (nVariables != that.nVariables)
            return false;
        return terms.Equals(that.terms);
    }


    public override int GetHashCode()
    {
        return terms.GetHashCode();
    }


    public virtual int SkeletonHashCode()
    {
        return terms.SkeletonHashCode();
    }





    public static long[]? KroneckerMap(int[] degrees)
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


    /* ============================================ Factory methods ============================================ */
    static void Add(SortedDictionary<DegreeVector, Monomial<E>> polynomial, Monomial<E> term, Ring<E> ring)
    {
        if (ring.IsZero(term.coefficient))
            return;

        if (polynomial.TryGetValue(term, out Monomial<E>? value))
        {
            E r = ring.Add(value.coefficient, term.coefficient);
            if (ring.IsZero(r))
                polynomial.Remove(term);
            else
                polynomial[term] = value.SetCoefficient(r);
        }
        else
            polynomial[term] = term;
    }


    /* ============================================ Factory methods ============================================ */
    static void Subtract(SortedDictionary<DegreeVector, Monomial<E>> polynomial, Monomial<E> term, Ring<E> ring)
    {
        Add(polynomial, term.SetCoefficient(ring.Negate(term.coefficient)), ring);
    }


    public static MultivariatePolynomial<E> Create(int nVariables, Ring<E> ring,
        IComparer<DegreeVector> ordering, IEnumerable<Monomial<E>> terms)
    {
        MonomialSet<E> map = new MonomialSet<E>(ordering);
        foreach (Monomial<E> term in terms)
            Add(map, term.SetCoefficient(ring.ValueOf(term.coefficient)), ring);
        return new MultivariatePolynomial<E>(nVariables, ring, ordering, map);
    }


    public static MultivariatePolynomial<E> Create(int nVariables, Ring<E> ring,
        IComparer<DegreeVector> ordering, params Monomial<E>[] terms)
    {
        return Create(nVariables, ring, ordering, terms.ToList());
    }


    public static MultivariatePolynomial<E> Zero(int nVariables, Ring<E> ring, IComparer<DegreeVector> ordering)
    {
        return new MultivariatePolynomial<E>(nVariables, ring, ordering, new MonomialSet<E>(ordering));
    }


    public static MultivariatePolynomial<E> One(int nVariables, Ring<E> ring, IComparer<DegreeVector> ordering)
    {
        return Create(nVariables, ring, ordering, new Monomial<E>(nVariables, ring.GetOne()));
    }
    
    public static MultivariatePolynomialZp64 AsOverZp64(MultivariatePolynomial<BigInteger> poly)
    {
        if (!(poly.ring is IntegersZp))
            throw new ArgumentException("Poly is not over modular ring: " + poly.ring);
        IntegersZp ring = (IntegersZp)poly.ring;
        MonomialSet<long> terms = new MonomialSet<long>(poly.ordering);
        foreach (Monomial<BigInteger> term in poly.terms)
            terms.Add(new Monomial<long>(term.exponents, term.totalDegree, (long)term.coefficient));
        return MultivariatePolynomialZp64.Create(poly.nVariables, ring.AsMachineRing(), poly.ordering, terms.Iterator());
    }


    public static MultivariatePolynomialZp64 AsOverZp64(MultivariatePolynomial<BigInteger> poly, IntegersZp64 ring)
    {
        MonomialSet<long> terms = new MonomialSet<long>(poly.ordering);
        BigInteger modulus = new BigInteger(ring.modulus);
        foreach (Monomial<BigInteger> term in poly.terms)
            terms.Add(new Monomial<long>(term.exponents, term.totalDegree,
                (long)(term.coefficient % modulus)));
        return MultivariatePolynomialZp64.Create(poly.nVariables, ring, poly.ordering, terms.Iterator());
    }


    public static MultivariatePolynomial<E> AsMultivariate(UnivariatePolynomial<E> poly, int nVariables,
        int variable, IComparer<DegreeVector> ordering)
    {
        MonomialSet<E> map = new MonomialSet<E>(ordering);
        for (int i = poly.Degree(); i >= 0; --i)
        {
            if (poly.IsZeroAt(i))
                continue;
            int[] degreeVector = new int[nVariables];
            degreeVector[variable] = i;
            map.Add(new Monomial<E>(degreeVector, i, poly[i]));
        }

        return new MultivariatePolynomial<E>(nVariables, poly.ring, ordering, map);
    }


    public UnivariatePolynomial<E> AsUnivariate()
    {
        if (IsConstant())
            return UnivariatePolynomial<E>.Constant(ring, Lc());
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
        return UnivariatePolynomial<E>.CreateUnsafe(ring, univarData);
    }


    public MultivariatePolynomial<UnivariatePolynomial<E>> AsOverUnivariate(int variable)
    {
        UnivariatePolynomial<E> factory = UnivariatePolynomial<E>.Zero(ring);
        Ring<UnivariatePolynomial<E>> pDomain = new UnivariateRing<E>(factory);
        MonomialSet<UnivariatePolynomial<E>> newData = new MonomialSet<UnivariatePolynomial<E>>(ordering);
        foreach (Monomial<E> e in terms)
        {
            MultivariatePolynomial<UnivariatePolynomial<E>>.Add(newData,
                new Monomial<UnivariatePolynomial<E>>(e.DvSetZero(variable), factory.CreateMonomial(e.coefficient, e.exponents[variable])),
                pDomain);
        }

        return new MultivariatePolynomial<UnivariatePolynomial<E>>(nVariables - 1, pDomain, ordering, newData);
    }


    public MultivariatePolynomial<UnivariatePolynomial<E>> AsOverUnivariateEliminate(int variable)
    {
        UnivariatePolynomial<E> factory = UnivariatePolynomial<E>.Zero(ring);
        UnivariateRing<E> pDomain = new UnivariateRing<E>(factory);
        MonomialSet<UnivariatePolynomial<E>> newData = new MonomialSet<UnivariatePolynomial<E>>(ordering);
        foreach (Monomial<E> e in terms)
        {
            MultivariatePolynomial<UnivariatePolynomial<E>>.Add(newData,
                new Monomial<UnivariatePolynomial<E>>(e.DvWithout(variable), factory.CreateMonomial(e.coefficient, e.exponents[variable])),
                pDomain);
        }

        return new MultivariatePolynomial<UnivariatePolynomial<E>>(nVariables - 1, pDomain, ordering, newData);
    }

    public MultivariatePolynomial<MultivariatePolynomial<E>> AsOverMultivariate(params int[] variables)
    {
        Ring<MultivariatePolynomial<E>> ring = new MultivariateRing<E>(this);
        MonomialSet<MultivariatePolynomial<E>> terms = new MonomialSet<MultivariatePolynomial<E>>(ordering);
        foreach (Monomial<E> term in this.terms)
        {
            int[] coeffExponents = new int[nVariables];
            foreach (int var in variables)
                coeffExponents[var] = term.exponents[var];
            Monomial<MultivariatePolynomial<E>> newTerm = new Monomial<MultivariatePolynomial<E>>(term.DvSetZero(variables),
                Create(new Monomial<E>(coeffExponents, coeffExponents.Sum(), term.coefficient)));
            MultivariatePolynomial<MultivariatePolynomial<E>>.Add(terms, newTerm, ring);
        }
    
        return new MultivariatePolynomial<MultivariatePolynomial<E>>(nVariables, ring, ordering, terms);
    }


    public MultivariatePolynomial<MultivariatePolynomial<E>> AsOverMultivariateEliminate(int[] variables,
        IComparer<DegreeVector> ordering)
    {
        variables = (int[])variables.Clone();
        Array.Sort(variables);
        int[] restVariables = Utils.Utils.IntSetDifference(Utils.Utils.Sequence(nVariables), variables);
        Ring<MultivariatePolynomial<E>> ring =
            new MultivariateRing<E>(Create(variables.Length, new MonomialSet<E>(ordering)));
        MonomialSet<MultivariatePolynomial<E>> terms = new MonomialSet<MultivariatePolynomial<E>>(ordering);
        foreach (Monomial<E> term in this.terms)
        {
            int i = 0;
            int[] coeffExponents = new int[variables.Length];
            foreach (int var in variables)
                coeffExponents[i++] = term.exponents[var];
            i = 0;
            int[] termExponents = new int[restVariables.Length];
            foreach (int var in restVariables)
                termExponents[i++] = term.exponents[var];
            Monomial<MultivariatePolynomial<E>> newTerm = new Monomial<MultivariatePolynomial<E>>(termExponents,
                Create(variables.Length, this.ring, this.ordering, new Monomial<E>(coeffExponents, term.coefficient)));
            MultivariatePolynomial<MultivariatePolynomial<E>>.Add(terms, newTerm, ring);
        }
    
        return new MultivariatePolynomial<MultivariatePolynomial<E>>(restVariables.Length, ring, ordering, terms);
    }


    public static MultivariatePolynomial<E> AsNormalMultivariate(
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
                result.Add(new Monomial<E>(dv.DvSet(variable, i), uPoly[i]));
            }
        }

        return result;
    }


    public static MultivariatePolynomial<E> AsNormalMultivariate(
        MultivariatePolynomial<MultivariatePolynomial<E>> poly)
    {
        Ring<E> ring = poly.ring.GetZero().ring;
        int nVariables = poly.nVariables;
        MultivariatePolynomial<E> result = Zero(nVariables, ring, poly.ordering);
        foreach (Monomial<MultivariatePolynomial<E>> term in poly.terms)
        {
            MultivariatePolynomial<E> uPoly = term.coefficient;
            result.Add(uPoly.Clone().Multiply(new Monomial<E>(term.exponents, term.totalDegree, ring.GetOne())));
        }

        return result;
    }


    public static MultivariatePolynomial<E> AsNormalMultivariate(
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
            result.Add(coefficient.Multiply(new Monomial<E>(t.exponents, t.totalDegree, ring.GetOne())));
        }

        return result;
    }


    public static MultivariatePolynomial<BigInteger> AsPolyZ(MultivariatePolynomial<BigInteger> poly, bool copy)
    {
        return new MultivariatePolynomial<BigInteger>(poly.nVariables, Rings.Z, poly.ordering,
            copy ? (MonomialSet<BigInteger>)poly.terms.Clone() : poly.terms);
    }


    public static MultivariatePolynomial<BigInteger> AsPolyZSymmetric(MultivariatePolynomial<BigInteger> poly)
    {
        if (!(poly.ring is IntegersZp))
            throw new ArgumentException("Not a modular ring: " + poly.ring);
        IntegersZp ring = (IntegersZp)poly.ring;
        MonomialSet<BigInteger> newTerms = new MonomialSet<BigInteger>(poly.ordering);
        foreach (Monomial<BigInteger> term in poly.terms)
            newTerms.Add(term.SetCoefficient(ring.SymmetricForm(term.coefficient)));
        return new MultivariatePolynomial<BigInteger>(poly.nVariables, Rings.Z, poly.ordering, newTerms);
    }
    
    public static MultivariatePolynomial<BigInteger> AsPolyZSymmetric(MultivariatePolynomialZp64 poly)
    {
        MonomialSet<BigInteger> bTerms = new MonomialSet<BigInteger>(poly.ordering);
        foreach (var t in poly.terms)
            bTerms.Add(new Monomial<BigInteger>(t.exponents, t.totalDegree,
                new BigInteger(((IntegersZp64)poly.ring).SymmetricForm(t.coefficient))));
        return new MultivariatePolynomial<BigInteger>(poly.nVariables, Rings.Z, poly.ordering, bTerms);
    }

    public MultivariatePolynomial<BigInteger> ToBigPoly()
    {
        if (typeof(E) == typeof(BigInteger))
            return this as MultivariatePolynomial<BigInteger>;

        if (ring is IntegersZp64 zp64)
            return AsZp64().MapCoefficients(zp64.AsGenericRing(), el => new BigInteger(el));
        
        if (ring is Integers64 z64)
            return AsT<long>().MapCoefficients(Rings.Z, el => new BigInteger(el));

        throw new Exception();
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


    public MultivariatePolynomial<E> LcAsPoly(IComparer<DegreeVector> ordering)
    {
        return CreateConstant(Lc(ordering));
    }


    public MultivariatePolynomial<E> CcAsPoly()
    {
        return CreateConstant(Cc());
    }


    MultivariatePolynomial<E> Create(int nVariables, IComparer<DegreeVector> ordering,
        MonomialSet<E> monomialTerms)
    {
        return new MultivariatePolynomial<E>(nVariables, ring, ordering, monomialTerms);
    }


    public override bool IsOverField()
    {
        return ring.IsField();
    }


    public bool IsOverFiniteField()
    {
        return ring.IsFiniteField();
    }


    public bool IsOverZ()
    {
        return ring.Equals(Rings.Z);
    }


    public BigInteger? CoefficientRingCardinality()
    {
        return ring.Cardinality();
    }


    public BigInteger CoefficientRingCharacteristic()
    {
        return ring.Characteristic();
    }


    public bool IsOverPerfectPower()
    {
        return ring.IsPerfectPower();
    }


    public BigInteger? CoefficientRingPerfectPowerBase()
    {
        return ring.PerfectPowerBase();
    }


    public BigInteger? CoefficientRingPerfectPowerExponent()
    {
        return ring.PerfectPowerExponent();
    }


    public MultivariatePolynomial<E>[] CreateArray(int length)
    {
        return new MultivariatePolynomial<E>[length];
    }

    public bool SameCoefficientRingWith(MultivariatePolynomial<E> oth)
    {
        return nVariables == oth.nVariables && ring.Equals(oth.ring);
    }


    public MultivariatePolynomial<E> SetCoefficientRingFrom(MultivariatePolynomial<E> poly)
    {
        return SetRing(poly.ring);
    }





    public MultivariatePolynomial<E> SetRing(Ring<E> newRing)
    {
        if (ring == newRing)
            return Clone();
        MonomialSet<E> newData = new MonomialSet<E>(ordering);
        foreach (Monomial<E> e in terms)
            Add(newData, e.SetCoefficient(newRing.ValueOf(e.coefficient)));
        return new MultivariatePolynomial<E>(nVariables, newRing, ordering, newData);
    }


    public MultivariatePolynomial<E> SetRingUnsafe(Ring<E> newRing)
    {
        return new MultivariatePolynomial<E>(nVariables, newRing, ordering, terms);
    }


    public MultivariatePolynomial<E> CreateConstant(E val)
    {
        MonomialSet<E> data = new MonomialSet<E>(ordering);
        if (!ring.IsZero(val))
            data.Add(new Monomial<E>(nVariables, val));
        return new MultivariatePolynomial<E>(nVariables, ring, ordering, data);
    }


    public MultivariatePolynomial<E> CreateConstantFromTerm(Monomial<E> monomial)
    {
        return CreateConstant(monomial.coefficient);
    }


    public MultivariatePolynomial<E> CreateZero()
    {
        return CreateConstant(ring.GetZero());
    }


    public override MultivariatePolynomial<E> CreateOne()
    {
        return CreateConstant(ring.GetOne());
    }


    public MultivariatePolynomial<E> CreateLinear(int variable, E cc, E lc)
    {
        MonomialSet<E> data = new MonomialSet<E>(ordering);
        if (!ring.IsZero(cc))
            data.Add(new Monomial<E>(nVariables, cc));
        if (!ring.IsZero(lc))
        {
            int[] lcDegreeVector = new int[nVariables];
            lcDegreeVector[variable] = 1;
            data.Add(new Monomial<E>(lcDegreeVector, 1, lc));
        }

        return new MultivariatePolynomial<E>(nVariables, ring, ordering, data);
    }


    public bool IsMonic()
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
        Monomial<E> lt = ((MonomialSetView<E>)terms).First();
        return lt.IsZeroVector() && ring.IsOne(lt.coefficient);
    }


    public bool IsUnitCC()
    {
        return ring.IsOne(Cc());
    }

    public override bool IsConstant()
    {
        return Size() == 0 || (Size() == 1 && ((MonomialSetView<E>)terms).First().IsZeroVector());
    }


    public E MaxAbsCoefficient()
    {
        return Stream().Select(ring.Abs).Max(ring) ?? (ring.GetZero());
    }


    public E Lc()
    {
        return Lt().coefficient;
    }


    public E Lc(IComparer<DegreeVector> ordering)
    {
        return Lt(ordering).coefficient;
    }


    public MultivariatePolynomial<E> SetLC(E val)
    {
        if (IsZero())
            return Add(val);
        terms.Add(Lt().SetCoefficient(ring.ValueOf(val)));
        Release();
        return this;
    }


    public E Cc()
    {
        Monomial<E> zero = new Monomial<E>(nVariables, ring.GetZero());
        return terms.GetValueOrDefault(zero, zero).coefficient;
    }


    public E Content()
    {
        return IsOverField() ? Lc() : ring.Gcd(Coefficients());
    }


    public IEnumerable<E> Coefficients()
    {
        return terms.Values.Select((m) => m.coefficient);
    }


    public E[] CoefficientsArray()
    {
        if (IsZero())
            return ring.CreateZeroesArray(1);
        E[] array = new E[Size()];
        int i = 0;
        foreach (Monomial<E> term in terms)
            array[i++] = term.coefficient;
        return array;
    }

    public MultivariatePolynomial<E> PrimitivePart(int variable)
    {
        return AsNormalMultivariate(AsOverUnivariateEliminate(variable).PrimitivePart(), variable);
    }


    public UnivariatePolynomial<E> ContentUnivariate(int variable)
    {
        return AsOverUnivariate(variable).Content();
    }


    public MultivariatePolynomial<E> PrimitivePart()
    {
        if (IsZero())
            return this;
        E content = Content();
        if (SignumOfLC() < 0 && ring.Signum(content) > 0)
            content = ring.Negate(content);
        MultivariatePolynomial<E> r = DivideOrNull(content);
        return r;
    }


    public MultivariatePolynomial<E> PrimitivePartSameSign()
    {
        if (IsZero())
            return this;
        E c = Content();
        if (SignumOfLC() < 0)
            c = ring.Negate(c);
        MultivariatePolynomial<E> r = DivideOrNull(c);
        return r;
    }


    public override MultivariatePolynomial<E>? DivideByLC(MultivariatePolynomial<E> other)
    {
        return DivideOrNull(other.Lc());
    }


    public MultivariatePolynomial<E>? DivideOrNull(E factor)
    {
        if (ring.IsOne(factor))
            return this;
        if (ring.IsMinusOne(factor))
            return Negate();
        if (ring.IsField())
            return Multiply(ring.Reciprocal(factor)); // <- this is typically faster than the division
        foreach (var entry in terms.EntryIterator())
        {
            Monomial<E> term = entry.Value;
            var quot = ring.DivideOrNull(term.coefficient, factor);
            if (quot.IsNull)
                return null;
            terms[entry.Key] = term.SetCoefficient(quot.Value);
        }

        Release();
        return this;
    }


    public MultivariatePolynomial<E> DivideExact(E factor)
    {
        var r = DivideOrNull(factor);
        if (r == null)
            throw new ArithmeticException("not divisible " + this + " / " + factor);
        return r;
    }


    public MultivariatePolynomial<E>? DivideOrNull(Monomial<E> monomial)
    {
        if (monomial.IsZeroVector())
            return DivideOrNull(monomial.coefficient);
        MonomialSet<E> map = new MonomialSet<E>(ordering);
        foreach (Monomial<E> term in terms)
        {
            var dv = monomialAlgebra.DivideOrNull(term, monomial);
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


    public MultivariatePolynomial<E> Monic(IComparer<DegreeVector> ordering)
    {
        if (IsZero())
            return this;
        return DivideOrNull(Lc(ordering));
    }


    public MultivariatePolynomial<E> Monic(E factor)
    {
        E lc = Lc();
        return Multiply(factor).DivideOrNull(lc);
    }


    public MultivariatePolynomial<E> Monic(IComparer<DegreeVector> ordering, E factor)
    {
        E lc = Lc(ordering);
        return Multiply(factor).DivideOrNull(lc);
    }

    public MultivariatePolynomial<E> MonicWithLC(MultivariatePolynomial<E> other)
    {
        if (Lc().Equals(other.Lc()))
            return this;
        return Monic(other.Lc());
    }


    public MultivariatePolynomial<E> MonicWithLC(IComparer<DegreeVector> ordering,
        MultivariatePolynomial<E> other)
    {
        E lc = Lc(ordering);
        E olc = other.Lc(ordering);
        if (lc.Equals(olc))
            return this;
        return Monic(ordering, olc);
    }

    // TODO
    // public UnivariatePolynomial ToDenseRecursiveForm()
    // {
    //     if (nVariables == 0)
    //         throw new ArgumentException("#variables = 0");
    //     return ToDenseRecursiveForm(nVariables - 1);
    // }
    //
    //
    // private UnivariatePolynomial ToDenseRecursiveForm(int variable)
    // {
    //     if (variable == 0)
    //         return AsUnivariate();
    //     UnivariatePolynomial<MultivariatePolynomial<E>> result = AsUnivariateEliminate(variable);
    //     IUnivariatePolynomial[] data = new IUnivariatePolynomial[result.Degree() + 1];
    //     for (int j = 0; j < data.Length; ++j)
    //         data[j] = result[j].ToDenseRecursiveForm(variable - 1);
    //     return UnivariatePolynomial.Create(Rings.PolynomialRing(data[0]), data);
    // }
    //
    //
    // public static MultivariatePolynomial<E> FromDenseRecursiveForm(UnivariatePolynomial<E> recForm, int nVariables,
    //     IComparer<DegreeVector> ordering)
    // {
    //     return FromDenseRecursiveForm(recForm, nVariables, ordering, nVariables - 1);
    // }
    //
    //
    // private static MultivariatePolynomial<E> FromDenseRecursiveForm(UnivariatePolynomial recForm, int nVariables,
    //     IComparer<DegreeVector> ordering, int variable)
    // {
    //     if (variable == 0)
    //         return (MultivariatePolynomial<E>)AsMultivariate(recForm, nVariables, 0, ordering);
    //     UnivariatePolynomial<UnivariatePolynomial> _recForm = (UnivariatePolynomial<UnivariatePolynomial>)recForm;
    //     MultivariatePolynomial<E>[] data = new MultivariatePolynomial[_recForm.Degree() + 1];
    //     for (int j = 0; j < data.Length; ++j)
    //         data[j] = FromDenseRecursiveForm(_recForm[j], nVariables, ordering, variable - 1);
    //     return AsMultivariate(UnivariatePolynomial.Create(Rings.MultivariateRing(data[0]), data), variable);
    // }
    //
    //
    // public static E EvaluateDenseRecursiveForm(UnivariatePolynomial recForm, int nVariables, E[] values)
    // {
    //     // compute number of variables
    //     UnivariatePolynomial p = recForm;
    //     int n = nVariables - 1;
    //     while (n > 0)
    //     {
    //         p = (UnivariatePolynomial)((UnivariatePolynomial)p).Cc();
    //         --n;
    //     }
    //
    //     if (nVariables != values.Length)
    //         throw new ArgumentException();
    //     return EvaluateDenseRecursiveForm(recForm, values, ((UnivariatePolynomial<E>)p).ring, nVariables - 1);
    // }
    //
    //
    // private static E EvaluateDenseRecursiveForm(UnivariatePolynomial recForm, E[] values, Ring<E> ring,
    //     int variable)
    // {
    //     if (variable == 0)
    //         return ((UnivariatePolynomial<E>)recForm).Evaluate(values[0]);
    //     UnivariatePolynomial<UnivariatePolynomial> _recForm = (UnivariatePolynomial<UnivariatePolynomial>)recForm;
    //     E result = ring.GetZero();
    //     for (int i = _recForm.degree(); i >= 0; --i)
    //         result = ring.Add(ring.Multiply(values[variable], result),
    //             EvaluateDenseRecursiveForm(_recForm[i], values, ring, variable - 1));
    //     return result;
    // }
    //
    //
    // public AMultivariatePolynomial ToSparseRecursiveForm()
    // {
    //     if (nVariables == 0)
    //         throw new ArgumentException("#variables = 0");
    //     return ToSparseRecursiveForm(nVariables - 1);
    // }
    //
    //
    // private AMultivariatePolynomial ToSparseRecursiveForm(int variable)
    // {
    //     if (variable == 0)
    //     {
    //         return this.SetNVariables(1);
    //     }
    //
    //     MultivariatePolynomial<MultivariatePolynomial<E>> result =
    //         AsOverMultivariateEliminate(ArraysUtil.Sequence(0, variable), MonomialOrder.GRLEX);
    //     Monomial<AMultivariatePolynomial>[] data = new Monomial[result.Count == 0 ? 1 : result.Count];
    //     int j = 0;
    //     foreach (Monomial<MultivariatePolynomial<E>> term in result.Count == 0
    //                  ? Collections.SingletonList(result.Lt())
    //                  : result)
    //         data[j++] = new Monomial(term, term.coefficient.ToSparseRecursiveForm(variable - 1));
    //     return MultivariatePolynomial.Create(1, Rings.MultivariateRing(data[0].coefficient), MonomialOrder.GRLEX,
    //         data);
    // }
    //
    //
    // public static MultivariatePolynomial<E> FromSparseRecursiveForm<E>(AMultivariatePolynomial recForm,
    //     int nVariables, IComparer<DegreeVector> ordering)
    // {
    //     return FromSparseRecursiveForm(recForm, nVariables, ordering, nVariables - 1);
    // }
    //
    //
    // private static MultivariatePolynomial<E> FromSparseRecursiveForm<E>(AMultivariatePolynomial recForm,
    //     int nVariables, IComparer<DegreeVector> ordering, int variable)
    // {
    //     if (variable == 0)
    //     {
    //         return ((MultivariatePolynomial<E>)recForm).SetNVariables(nVariables).SetOrdering(ordering);
    //     }
    //
    //     MultivariatePolynomial<AMultivariatePolynomial> _recForm =
    //         (MultivariatePolynomial<AMultivariatePolynomial>)recForm;
    //     Monomial<MultivariatePolynomial<E>>[] data = new Monomial[_recForm.Count == 0 ? 1 : _recForm.Count];
    //     int j = 0;
    //     foreach (Monomial<AMultivariatePolynomial> term in _recForm.Count == 0
    //                  ? Collections.SingletonList(_recForm.Lt())
    //                  : _recForm)
    //     {
    //         int[] exponents = new int[nVariables];
    //         exponents[variable] = term.totalDegree;
    //         data[j++] = new Monomial(exponents, term.totalDegree,
    //             FromSparseRecursiveForm(term.coefficient, nVariables, ordering, variable - 1));
    //     }
    //
    //     MultivariatePolynomial<MultivariatePolynomial<E>> result =
    //         MultivariatePolynomial.Create(nVariables, Rings.MultivariateRing(data[0].coefficient), ordering, data);
    //     return AsNormalMultivariate(result);
    // }
    //
    //
    // public static E EvaluateSparseRecursiveForm(AMultivariatePolynomial recForm, int nVariables, E[] values)
    // {
    //     // compute number of variables
    //     AMultivariatePolynomial p = recForm;
    //     TIntArrayList degrees = new TIntArrayList();
    //     int n = nVariables - 1;
    //     while (n > 0)
    //     {
    //         p = (AMultivariatePolynomial)((MultivariatePolynomial)p).Cc();
    //         degrees.Add(p.Degree());
    //         --n;
    //     }
    //
    //     degrees.Add(p.Degree());
    //     if (nVariables != values.Length)
    //         throw new ArgumentException();
    //     Ring<E> ring = ((MultivariatePolynomial<E>)p).ring;
    //     PrecomputedPowers<E>[] pp = new PrecomputedPowers[nVariables];
    //     for (int i = 0; i < nVariables; ++i)
    //         pp[i] = new PrecomputedPowers(Math.Min(degrees[i], MAX_POWERS_CACHE_SIZE), values[i], ring);
    //     return EvaluateSparseRecursiveForm(recForm, new PrecomputedPowersHolder(ring, pp), nVariables - 1);
    // }
    //
    //
    // static E EvaluateSparseRecursiveForm(AMultivariatePolynomial recForm, PrecomputedPowersHolder ph,
    //     int variable)
    // {
    //     Ring<E> ring = ph.ring;
    //     if (variable == 0)
    //     {
    //         MultivariatePolynomial<E> _recForm = (MultivariatePolynomial<E>)recForm;
    //         IEnumerator<Monomial<E>> it = _recForm.terms.DescendingIterator();
    //         int previousExponent = -1;
    //         E result = ring.GetZero();
    //         while (it.HasNext())
    //         {
    //             Monomial<E> m = it.Next();
    //             result = ring.Add(
    //                 ring.Multiply(result,
    //                     ph.Pow(variable, previousExponent == -1 ? 1 : previousExponent - m.totalDegree)),
    //                 m.coefficient);
    //             previousExponent = m.totalDegree;
    //         }
    //
    //         if (previousExponent > 0)
    //             result = ring.Multiply(result, ph.Pow(variable, previousExponent));
    //         return result;
    //     }
    //
    //     MultivariatePolynomial<AMultivariatePolynomial> _recForm =
    //         (MultivariatePolynomial<AMultivariatePolynomial>)recForm;
    //     IEnumerator<Monomial<AMultivariatePolynomial>> it = _recForm.terms.DescendingIterator();
    //     int previousExponent = -1;
    //     E result = ring.GetZero();
    //     while (it.HasNext())
    //     {
    //         Monomial<AMultivariatePolynomial> m = it.Next();
    //         result = ring.Add(
    //             ring.Multiply(result,
    //                 ph.Pow(variable, previousExponent == -1 ? 1 : previousExponent - m.totalDegree)),
    //             EvaluateSparseRecursiveForm(m.coefficient, ph, variable - 1));
    //         previousExponent = m.totalDegree;
    //     }
    //
    //     if (previousExponent > 0)
    //         result = ring.Multiply(result, ph.Pow(variable, previousExponent));
    //     return result;
    // }

    // TODO
    // public HornerForm GetHornerForm(int[] evaluationVariables)
    // {
    //     int[] evalDegrees = ArraysUtil.Select(DegreesRef(), evaluationVariables);
    //     MultivariatePolynomial<MultivariatePolynomial<E>> p = AsOverMultivariateEliminate(evaluationVariables);
    //     Ring<AMultivariatePolynomial> newRing = Rings.PolynomialRing(p.Cc().ToSparseRecursiveForm());
    //     return new HornerForm(ring, evalDegrees, evaluationVariables.Length,
    //         p.MapCoefficients(newRing, MultivariatePolynomial.ToSparseRecursiveForm()));
    // }

    // TODO
    // public sealed class HornerForm
    // {
    //     private readonly Ring<E> ring;
    //     private readonly int nEvalVariables;
    //     private readonly int[] evalDegrees;
    //     private readonly MultivariatePolynomial<AMultivariatePolynomial> recForm;
    //
    //     private HornerForm(Ring<E> ring, int[] evalDegrees, int nEvalVariables,
    //         MultivariatePolynomial<AMultivariatePolynomial> recForm)
    //     {
    //         this.ring = ring;
    //         this.evalDegrees = evalDegrees;
    //         this.nEvalVariables = nEvalVariables;
    //         this.recForm = recForm;
    //     }
    //
    //
    //     public MultivariatePolynomial<E> Evaluate(E[] values)
    //     {
    //         if (values.Length != nEvalVariables)
    //             throw new ArgumentException();
    //         PrecomputedPowers[] pp = new PrecomputedPowers[nEvalVariables];
    //         for (int i = 0; i < nEvalVariables; ++i)
    //             pp[i] = new PrecomputedPowers(Math.Min(evalDegrees[i], MAX_POWERS_CACHE_SIZE), values[i], ring);
    //         return recForm.MapCoefficients(ring,
    //             (p) => EvaluateSparseRecursiveForm(p, new PrecomputedPowersHolder(ring, pp), nEvalVariables - 1));
    //     }
    // }


    public MultivariatePolynomial<E> Evaluate(int variable, E value)
    {
        value = ring.ValueOf(value);
        if (ring.IsZero(value))
            return EvaluateAtZero(variable);
        PrecomputedPowers powers = new PrecomputedPowers(value, ring);
        return Evaluate(variable, powers);
    }


    MultivariatePolynomial<E> Evaluate(int variable, PrecomputedPowers powers)
    {
        if (Degree(variable) == 0)
            return Clone();
        if (ring.IsZero(powers.value))
            return EvaluateAtZero(variable);
        MonomialSet<E> newData = new MonomialSet<E>(ordering);
        foreach (Monomial<E> el in terms)
        {
            E val = ring.Multiply(el.coefficient, powers.Pow(el.exponents[variable]));
            Add(newData, el.SetZero(variable).SetCoefficient(val));
        }

        return new MultivariatePolynomial<E>(nVariables, ring, ordering, newData);
    }


    UnivariatePolynomial<E> EvaluateAtZeroAllExcept(int variable)
    {
        E[] uData = new E[Degree(variable) + 1];
        foreach (Monomial<E> el in terms)
        {
            if (el.totalDegree != 0 && el.exponents[variable] == 0)
                continue;
            for (int i = 0; i < nVariables; ++i)
                if (i != variable && el.exponents[i] != 0)
                    goto next;
            int uExp = el.exponents[variable];
            uData[uExp] = ring.Add(uData[uExp], el.coefficient);
            
            next: ;
        }

        return UnivariatePolynomial<E>.CreateUnsafe(ring, uData);
    }


    public E Evaluate(params E[] values)
    {
        if (values.Length != nVariables)
            throw new ArgumentException();
        return Evaluate(Utils.Utils.Sequence(0, nVariables), values).Cc();
    }


    public MultivariatePolynomial<E> Evaluate(int[] variables, E[] values)
    {
        foreach (E value in values)
            if (!ring.IsZero(value))
                return Evaluate(MkPrecomputedPowers(variables, values), variables);

        // <- all values are zero
        return EvaluateAtZero(variables);
    }


    public MultivariatePolynomial<E> Evaluate(PrecomputedPowersHolder powers, int[] variables)
    {
        MonomialSet<E> newData = new MonomialSet<E>(ordering);
        foreach (Monomial<E> el in terms)
        {
            Monomial<E> r = el;
            E value = el.coefficient;
            foreach (int variable in variables)
                value = ring.Multiply(value, powers.Pow(variable, el.exponents[variable]));
            r = r.SetZero(variables).SetCoefficient(value);
            Add(newData, r);
        }

        return new MultivariatePolynomial<E>(nVariables, ring, ordering, newData);
    }


    public MultivariatePolynomial<E>[] Evaluate(int variable, params E[] values)
    {
        return values.Select((p) => Evaluate(variable, p)).ToArray();
    }


    public MultivariatePolynomial<E> Evaluate(int variable, long value)
    {
        return Evaluate(variable, ring.ValueOfLong(value));
    }


    public MultivariatePolynomial<E> Eliminate(int variable, E value)
    {
        value = ring.ValueOf(value);
        MonomialSet<E> newData = new MonomialSet<E>(ordering);
        PrecomputedPowers powers = new PrecomputedPowers(value, ring);
        foreach (Monomial<E> el in terms)
        {
            E val = ring.Multiply(el.coefficient, powers.Pow(el.exponents[variable]));
            Add(newData, el.Without(variable).SetCoefficient(val));
        }

        return new MultivariatePolynomial<E>(nVariables - 1, ring, ordering, newData);
    }


    public MultivariatePolynomial<E> Eliminate(int variable, long value)
    {
        return Eliminate(variable, ring.ValueOfLong(value));
    }


    public MultivariatePolynomial<E> Eliminate(int[] variables, E[] values)
    {
        foreach (E value in values)
            if (!ring.IsZero(value))
                return Eliminate(MkPrecomputedPowers(variables, values), variables);

        // <- all values are zero
        return EvaluateAtZero(variables).DropVariables(variables);
    }


    MultivariatePolynomial<E> Eliminate(PrecomputedPowersHolder powers, int[] variables)
    {
        MonomialSet<E> newData = new MonomialSet<E>(ordering);
        foreach (Monomial<E> el in terms)
        {
            Monomial<E> r = el;
            E value = el.coefficient;
            foreach (int variable in variables)
                value = ring.Multiply(value, powers.Pow(variable, el.exponents[variable]));
            r = r.Without(variables).SetCoefficient(value);
            Add(newData, r);
        }

        return new MultivariatePolynomial<E>(nVariables - variables.Length, ring, ordering, newData);
    }

    private const int DEFAULT_POWERS_CACHE_SIZE = 64;
    public const int MAX_POWERS_CACHE_SIZE = 1014;

    public sealed class PrecomputedPowers
    {
        public readonly E value;
        private readonly Ring<E> ring;
        public readonly E[] precomputedPowers;

        public PrecomputedPowers(E value, Ring<E> ring) : this(DEFAULT_POWERS_CACHE_SIZE, value, ring)
        {
        }

        public PrecomputedPowers(int cacheSize, E value, Ring<E> ring)
        {
            this.value = ring.ValueOf(value);
            this.ring = ring;
            this.precomputedPowers = new E[cacheSize];
        }

        public E Pow(int exponent)
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


    public PrecomputedPowersHolder MkPrecomputedPowers(int variable, E value)
    {
        PrecomputedPowers[] pp = new PrecomputedPowers[nVariables];
        pp[variable] = new PrecomputedPowers(Math.Min(Degree(variable), MAX_POWERS_CACHE_SIZE), value, ring);
        return new PrecomputedPowersHolder(ring, pp);
    }


    public PrecomputedPowersHolder MkPrecomputedPowers(int[] variables, E[] values)
    {
        int[] degrees = DegreesRef();
        PrecomputedPowers[] pp = new PrecomputedPowers[nVariables];
        for (int i = 0; i < variables.Length; ++i)
            pp[variables[i]] = new PrecomputedPowers(Math.Min(degrees[variables[i]], MAX_POWERS_CACHE_SIZE),
                values[i], ring);
        return new PrecomputedPowersHolder(ring, pp);
    }


    public static PrecomputedPowersHolder MkPrecomputedPowers(int nVariables, Ring<E> ring, int[] variables,
        E[] values)
    {
        PrecomputedPowers[] pp = new PrecomputedPowers[nVariables];
        for (int i = 0; i < variables.Length; ++i)
            pp[variables[i]] = new PrecomputedPowers(MAX_POWERS_CACHE_SIZE, values[i], ring);
        return new PrecomputedPowersHolder(ring, pp);
    }


    public PrecomputedPowersHolder MkPrecomputedPowers(E[] values)
    {
        if (values.Length != nVariables)
            throw new ArgumentException();
        int[] degrees = DegreesRef();
        PrecomputedPowers[] pp = new PrecomputedPowers[nVariables];
        for (int i = 0; i < nVariables; ++i)
            pp[i] = new PrecomputedPowers(Math.Min(degrees[i], MAX_POWERS_CACHE_SIZE), values[i], ring);
        return new PrecomputedPowersHolder(ring, pp);
    }


    public sealed class PrecomputedPowersHolder
    {
        public readonly Ring<E> ring;
        readonly PrecomputedPowers[] powers;

        public PrecomputedPowersHolder(Ring<E> ring, PrecomputedPowers[] powers)
        {
            this.ring = ring;
            this.powers = powers;
        }

        public void Set(int i, E point)
        {
            if (powers[i] == null || !powers[i].value.Equals(point))
                powers[i] = new PrecomputedPowers(
                    powers[i] == null ? DEFAULT_POWERS_CACHE_SIZE : powers[i].precomputedPowers.Length, point,
                    ring);
        }

        public E Pow(int variable, int exponent)
        {
            return powers[variable].Pow(exponent);
        }

        public PrecomputedPowersHolder Clone()
        {
            return new PrecomputedPowersHolder(ring, (PrecomputedPowers[])powers.Clone());
        }
    }


    public MultivariatePolynomial<E> Substitute(int variable, MultivariatePolynomial<E> poly)
    {
        if (poly.IsConstant())
            return Evaluate(variable, poly.Cc());
        PrecomputedSubstitution subsPowers;
        if (poly.IsEffectiveUnivariate())
            subsPowers = new USubstitution(poly.AsUnivariate(), poly.UnivariateVariable(), nVariables, ordering);
        else
            subsPowers = new MSubstitution(poly);
        MultivariatePolynomial<E> result = CreateZero();
        foreach (Monomial<E> term in terms)
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


    public MultivariatePolynomial<E> Shift(int variable, long shift)
    {
        return Shift(variable, ring.ValueOfLong(shift));
    }


    public MultivariatePolynomial<E> Shift(int variable, E shift)
    {
        if (ring.IsZero(shift))
            return Clone();
        shift = ring.ValueOf(shift);
        USubstitution shifts =
            new USubstitution(UnivariatePolynomial<E>.CreateUnsafe(ring, [shift, ring.GetOne()]),
                variable, nVariables, ordering);
        MultivariatePolynomial<E> result = CreateZero();
        foreach (Monomial<E> term in terms)
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


    public MultivariatePolynomial<E> Shift(int[] variables, E[] shifts)
    {
        PrecomputedSubstitution[] powers = new PrecomputedSubstitution[nVariables];
        bool allShiftsAreZero = true;
        for (int i = 0; i < variables.Length; ++i)
        {
            if (!ring.IsZero(shifts[i]))
                allShiftsAreZero = false;
            powers[variables[i]] =
                new USubstitution(UnivariatePolynomial<E>.Create(ring, [shifts[i], ring.GetOne()]),
                    variables[i], nVariables, ordering);
        }

        if (allShiftsAreZero)
            return Clone();
        PrecomputedSubstitutions calculatedShifts = new PrecomputedSubstitutions(powers);
        MultivariatePolynomial<E> result = CreateZero();
        foreach (Monomial<E> _term in terms)
        {
            var term = _term;
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


    sealed class PrecomputedSubstitutions
    {
        readonly PrecomputedSubstitution[] subs;

        public PrecomputedSubstitutions(PrecomputedSubstitution[] subs)
        {
            this.subs = subs;
        }

        public MultivariatePolynomial<E> GetSubstitutionPower(int var, int exponent)
        {
            if (subs[var] == null)
                throw new ArgumentException();
            return subs[var].Pow(exponent);
        }
    }


    interface PrecomputedSubstitution
    {
        MultivariatePolynomial<E> Pow(int exponent);
    }


    sealed class USubstitution : PrecomputedSubstitution
    {
        readonly int variable;
        readonly int nVariables;
        readonly IComparer<DegreeVector> ordering;
        readonly UnivariatePolynomial<E> @base;
        readonly Dictionary<int, UnivariatePolynomial<E>> uCache = new Dictionary<int, UnivariatePolynomial<E>>();
        readonly Dictionary<int, MultivariatePolynomial<E>> mCache = new Dictionary<int, MultivariatePolynomial<E>>();

        public USubstitution(UnivariatePolynomial<E> @base, int variable, int nVariables,
            IComparer<DegreeVector> ordering)
        {
            this.nVariables = nVariables;
            this.variable = variable;
            this.ordering = ordering;
            this.@base = @base;
        }

        public MultivariatePolynomial<E> Pow(int exponent)
        {
            if (mCache.TryGetValue(exponent, out var cached))
                return cached.Clone();
            UnivariatePolynomial<E> r = PolynomialMethods.PolyPow(@base, exponent, true, uCache);
            mCache[exponent] = (cached = AsMultivariate(r, nVariables, variable, ordering));
            return cached.Clone();
        }
    }


    sealed class MSubstitution : PrecomputedSubstitution
    {
        readonly MultivariatePolynomial<E> @base;
        readonly Dictionary<int, MultivariatePolynomial<E>> cache = new Dictionary<int, MultivariatePolynomial<E>>();

        public MSubstitution(MultivariatePolynomial<E> @base)
        {
            this.@base = @base;
        }

        public MultivariatePolynomial<E> Pow(int exponent)
        {
            return PolynomialMethods.PolyPow(@base, exponent, true, cache);
        }
    }


    void Add(MonomialSet<E> terms, Monomial<E> term)
    {
        Add(terms, term, ring);
    }


    void Subtract(MonomialSet<E> terms, Monomial<E> term)
    {
        Subtract(terms, term, ring);
    }


    public MultivariatePolynomial<E> Add(E oth)
    {
        oth = ring.ValueOf(oth);
        if (ring.IsZero(oth))
            return this;
        Add(terms, new Monomial<E>(nVariables, oth));
        Release();
        return this;
    }


    public MultivariatePolynomial<E> Subtract(E oth)
    {
        return Add(ring.Negate(ring.ValueOf(oth)));
    }


    public MultivariatePolynomial<E> Increment()
    {
        return Add(ring.GetOne());
    }


    public MultivariatePolynomial<E> Decrement()
    {
        return Subtract(ring.GetOne());
    }


    public MultivariatePolynomial<E> Multiply(E factor)
    {
        factor = ring.ValueOf(factor);
        if (ring.IsOne(factor))
            return this;
        if (ring.IsZero(factor))
            return ToZero();
        using var it = terms.EntryIterator().GetEnumerator();
        while (it.MoveNext())
        {
            Monomial<E> term = it.Current.Value;
            E val = ring.Multiply(term.coefficient, factor);
            if (ring.IsZero(val))
                terms.Remove(term);
            else
                terms[it.Current.Key] = term.SetCoefficient(val);
        }

        Release();
        return this;
    }


    public MultivariatePolynomial<E> MultiplyByLC(MultivariatePolynomial<E> other)
    {
        return Multiply(other.Lc());
    }


    public MultivariatePolynomial<E> Multiply(Monomial<E> monomial)
    {
        CheckSameDomainWith(monomial);
        if (monomial.IsZeroVector())
            return Multiply(monomial.coefficient);
        if (ring.IsZero(monomial.coefficient))
            return ToZero();
        MonomialSet<E> newMap = new MonomialSet<E>(ordering);
        foreach (Monomial<E> thisElement in terms)
        {
            Monomial<E> mul = monomialAlgebra.Multiply(thisElement, monomial);
            if (!ring.IsZero(mul.coefficient))
                newMap.Add(mul);
        }

        return LoadFrom(newMap);
    }


    public MultivariatePolynomial<E> Multiply(long factor)
    {
        return Multiply(ring.ValueOfLong(factor));
    }


    public MultivariatePolynomial<E> MultiplyByBigInteger(BigInteger factor)
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
        if (oth.Size() == 1)
            return Multiply(oth.Lt());
        if (Size() > KRONECKER_THRESHOLD && oth.Size() > KRONECKER_THRESHOLD)
            return MultiplyKronecker(oth);
        else
            return MultiplyClassic(oth);
    }


    private MultivariatePolynomial<E> MultiplyClassic(MultivariatePolynomial<E> oth)
    {
        MonomialSet<E> newMap = new MonomialSet<E>(ordering);
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
        if (threshold > long.MaxValue)
            return MultiplyClassic(oth);
        return FromKronecker(MultiplySparseUnivariate(ring, ToKronecker(map), oth.ToKronecker(map)), map);
    }


    private Dictionary<long, E> ToKronecker(long[] kroneckerMap)
    {
        var result = new Dictionary<long, E>(Size());
        foreach (Monomial<E> term in terms)
        {
            long exponent = term.exponents[0];
            for (int i = 1; i < term.exponents.Length; i++)
                exponent += term.exponents[i] * kroneckerMap[i];
            result[exponent] = term.coefficient;
        }

        return result;
    }


    private static Dictionary<long, CfHolder> MultiplySparseUnivariate(Ring<E> ring,
        Dictionary<long, E> a, Dictionary<long, E> b)
    {
        Dictionary<long, CfHolder> result = new Dictionary<long, CfHolder>(a.Count + b.Count);
        using var ait = a.GetEnumerator();
        while (ait.MoveNext())
        {
            using var bit = b.GetEnumerator();
            while (bit.MoveNext())
            {
                long deg = ait.Current.Key + bit.Current.Key;
                E av = ait.Current.Value;
                E bv = bit.Current.Value;
                E val = ring.Multiply(av, bv);
                if (result.TryGetValue(deg, out var r))
                    r.coefficient = ring.Add(r.coefficient, val);
                else
                    result[deg] = new CfHolder(val);
            }
        }

        return result;
    }


    private MultivariatePolynomial<E> FromKronecker(Dictionary<long, CfHolder> p, long[] kroneckerMap)
    {
        terms.Clear();
        using var it = p.GetEnumerator();
        while (it.MoveNext())
        {
            if (ring.IsZero(it.Current.Value.coefficient))
                continue;
            long exponent = it.Current.Key;
            int[] exponents = new int[nVariables];
            for (int i = 0; i < nVariables; i++)
            {
                long div = exponent / kroneckerMap[nVariables - i - 1];
                exponent = exponent - (div * kroneckerMap[nVariables - i - 1]);
                exponents[nVariables - i - 1] = MachineArithmetic.SafeToInt(div);
            }

            terms.Add(new Monomial<E>(exponents, it.Current.Value.coefficient));
        }

        Release();
        return this;
    }


    sealed class CfHolder
    {
        public E coefficient;

        public CfHolder(E coefficient)
        {
            this.coefficient = coefficient;
        }
    }


    public override MultivariatePolynomial<E> Square()
    {
        return Multiply(this);
    }

    public override MultivariatePolynomial<E> DivideExact(MultivariatePolynomial<E> other)
    {
        return MultivariateDivision.DivideExact(this, other);
    }

    public override PolynomialRing<MultivariatePolynomial<E>> AsRing()
    {
        return Rings.MultivariateRing(this);
    }


    public MultivariatePolynomial<E> EvaluateAtRandom(int variable, Random rnd)
    {
        return Evaluate(variable, ring.RandomElement(rnd));
    }


    public MultivariatePolynomial<E> EvaluateAtRandomPreservingSkeleton(int variable, Random rnd)
    {
        if (Degree(variable) == 0)
            return Clone();

        //desired skeleton
        var skeleton = GetSkeletonExcept(variable);
        MultivariatePolynomial<E> tmp;
        do
        {
            E randomPoint = ring.RandomElement(rnd);
            tmp = Evaluate(variable, randomPoint);
        } while (!skeleton.Equals(tmp.GetSkeleton()));

        return tmp;
    }


    public MultivariatePolynomial<E> Derivative(int variable, int order)
    {
        MonomialSet<E> newTerms = new MonomialSet<E>(ordering);
        foreach (Monomial<E> term in terms)
        {
            int exponent = term.exponents[variable];
            if (exponent < order)
                continue;
            E newCoefficient = term.coefficient;
            for (int i = 0; i < order; ++i)
                newCoefficient = ring.Multiply(newCoefficient, ring.ValueOfLong(exponent - i));
            int[] newExponents = (int[])term.exponents.Clone();
            newExponents[variable] -= order;
            Add(newTerms, new Monomial<E>(newExponents, term.totalDegree - order, newCoefficient));
        }

        return new MultivariatePolynomial<E>(nVariables, ring, ordering, newTerms);
    }


    static BigInteger SeriesCoefficientFactor0(int exponent, int order, IntegersZp ring)
    {
        if (!ring.modulus.IsInt() || order < (long)ring.modulus)
            return MultivariatePolynomial<BigInteger>.SeriesCoefficientFactor1(exponent, order, ring);
        return new BigInteger(
            MultivariatePolynomialZp64.SeriesCoefficientFactor(exponent, order, ring.AsZp64()));
    }


    static E SeriesCoefficientFactor1(int exponent, int order, Ring<E> ring)
    {
        E factor = ring.GetOne();
        for (int i = 0; i < order; ++i)
            factor = ring.Multiply(factor, ring.ValueOfLong(exponent - i));
        factor = ring.DivideExact(factor, ring.Factorial(order));
        return factor;
    }


    static E SeriesCoefficientFactor2(int exponent, int order, Ring<E> ring)
    {
        BigInteger factor = BigInteger.One;
        for (int i = 0; i < order; ++i)
            factor *= exponent - 1;
        factor /= Rings.Z.Factorial(order);
        return ring.ValueOfBigInteger(factor);
    }


    static E SeriesCoefficientFactor(int exponent, int order, Ring<E> ring)
    {
        if (ring is IntegersZp rZp)
            return (E)(object)SeriesCoefficientFactor0(exponent, order, rZp);
        BigInteger characteristics = ring.Characteristic();
        if (characteristics == null || !characteristics.IsInt() || (int)characteristics > order)
            return SeriesCoefficientFactor1(exponent, order, ring);
        return SeriesCoefficientFactor2(exponent, order, ring);
    }


    public MultivariatePolynomial<E> SeriesCoefficient(int variable, int order)
    {
        if (order == 0)
            return Clone();
        if (IsConstant())
            return CreateZero();
        MonomialSet<E> newTerms = new MonomialSet<E>(ordering);
        foreach (Monomial<E> term in terms)
        {
            int exponent = term.exponents[variable];
            if (exponent < order)
                continue;
            var newExponents = (int[])term.exponents.Clone();
            newExponents[variable] -= order;
            E newCoefficient = ring.Multiply(term.coefficient, SeriesCoefficientFactor(exponent, order, ring));
            Add(newTerms, new Monomial<E>(newExponents, term.totalDegree - order, newCoefficient));
        }

        return new MultivariatePolynomial<E>(nVariables, ring, ordering, newTerms);
    }


    public IEnumerable<E> Stream()
    {
        return terms.Values.Select(m => m.coefficient);
    }


    public MultivariatePolynomial<T> MapTerms<T>(Ring<T> newRing, Func<Monomial<E>, Monomial<T>> mapper)
    {
        var newTerms = terms.Values.Select(mapper);
        return MultivariatePolynomial<T>.Create(nVariables, newRing, ordering, newTerms);
    }


    public MultivariatePolynomial<T> MapCoefficients<T>(Ring<T> newRing, Func<E, T> mapper)
    {
        return MapTerms(newRing, (t) => new Monomial<T>(t.exponents, t.totalDegree, mapper(t.coefficient)));
    }


    public MultivariatePolynomialZp64 MapCoefficientsZp64(IntegersZp64 newDomain, Func<E, long> mapper)
    {
        return MapCoefficients(newDomain, (cf) => mapper(cf));
    }
    
    public MultivariatePolynomial<T> MapCoefficientsAsPolys<T>(Ring<T> ring, Func<MultivariatePolynomial<E>, T> mapper)
    {
        return MapCoefficients(ring, (cf) => mapper(CreateConstant(cf)));
    }
    
    public int CompareTo(MultivariatePolynomial<E> oth)
    {
        int c = Size().CompareTo(oth.Size());
        if (c != 0)
            return c;
        using var thisIt = Iterator().GetEnumerator();
        using var othIt = oth.Iterator().GetEnumerator();
        while (thisIt.MoveNext() && othIt.MoveNext())
        {
            Monomial<E> a = thisIt.Current, b = othIt.Current;
            if ((c = ordering.Compare(a, b)) != 0)
                return c;
            if ((c = ring.Compare(a.coefficient, b.coefficient)) != 0)
                return c;
        }

        return 0;
    }
    
    public override MultivariatePolynomial<E> Clone()
    {
        return new MultivariatePolynomial<E>(nVariables, ring, ordering, (MonomialSet<E>)terms.Clone());
    }
    
    public override string ToString()
    {
        if (IsConstant())
            return Cc().ToString();
        string[] varStrings = new string[nVariables];
        for (int i = 0; i < nVariables; ++i)
            varStrings[i] = new string[]{"x", "y", "z", "w"}[i];
        StringBuilder sb = new StringBuilder();
        foreach (Monomial<E> term in terms)
        {
            E cf = term.coefficient;
            string cfString;
            if (ring.IsMinusOne(cf) && term.totalDegree != 0)
                cfString = "-";
            else if (!ring.IsOne(cf) || term.totalDegree == 0)
                cfString = cf.ToString();
            else
                cfString = "";
            if (term.totalDegree != 0 && (cfString.Contains("+") || cfString.Contains("-")))
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
    
    public MultivariatePolynomial<BigInteger> AsZ()
    {
        if (ring is not Integers && ring is not IntegersZp)
            throw new InvalidOperationException("Not a Zp64 ring");

        return this as MultivariatePolynomial<BigInteger>;
    }
    
    public MultivariatePolynomialZp64 AsZp64()
    {
        if (ring is not IntegersZp64 rZp64)
            throw new InvalidOperationException("Not a Zp64 ring");

        return this as MultivariatePolynomialZp64;
    }
    
    public MultivariatePolynomial<T> AsT<T>()
    {
        if (typeof(E) != typeof(T))
            throw new InvalidOperationException("Not a Zp64 ring");

        return this as MultivariatePolynomial<T>;
    }
}