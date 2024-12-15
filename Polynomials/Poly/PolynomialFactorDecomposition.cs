using Polynomials.Poly.Univar;
using Polynomials.Utils;

namespace Polynomials.Poly;

public sealed class PolynomialFactorDecomposition<Poly> : FactorDecomposition<Poly> where Poly : Polynomial<Poly>
{

    private PolynomialFactorDecomposition(Poly unit, List<Poly> factors, List<int> exponents) : base(
        Rings.PolynomialRing(unit), unit, factors, exponents)
    {
    }

    private PolynomialFactorDecomposition(FactorDecomposition<Poly> factors) : base(factors.Ring, factors.Unit,
        factors.Factors, factors.Exponents)
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
        if (Factors.Count == 0)
            return this;
        ReduceUnitContent();
        Poly[] fTmp = Factors.ToArray();
        int[] eTmp = Exponents.ToArray();
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
                    base.Unit.Negate();
            }
        }

        Array.Sort(fTmp, eTmp);
        for (int i = 0; i < fTmp.Length; i++)
        {
            Factors[i] = fTmp[i];
            Exponents[i] = eTmp[i];
        }

        return this;
    }


    public PolynomialFactorDecomposition<Poly> SetLcFrom(Poly poly)
    {
        Poly u = Ring.GetOne();
        for (int i = 0; i < Factors.Count; i++)
            u = u.Multiply(PolynomialMethods.PolyPow(Factors[i].LcAsPoly(), Exponents[i]));
        return SetUnit(PolynomialMethods.DivideExact(poly.LcAsPoly(), u));
    }


    public Poly Lc()
    {
        Poly u = base.Unit.Clone();
        for (int i = 0; i < Factors.Count; i++)
            u = u.Multiply(PolynomialMethods.PolyPow(Factors[i].LcAsPoly(), Exponents[i]));
        return u;
    }


    public int Signum()
    {
        int signum = base.Unit.SignumOfLC();
        for (int i = 0; i < Factors.Count; i++)
            signum *= Exponents[i] % 2 == 0 ? 1 : Factors[i].SignumOfLC();
        return signum;
    }


    public PolynomialFactorDecomposition<Poly> Monic()
    {
        for (int i = 0; i < Factors.Count; i++)
        {
            Poly factor = Factors[i];
            AddUnit(PolynomialMethods.PolyPow(factor.LcAsPoly(), Exponents[i], false));
            factor = factor.Monic();
        }

        return this;
    }


    public PolynomialFactorDecomposition<Poly> Primitive()
    {
        for (int i = 0; i < Factors.Count; i++)
        {
            Poly factor = Factors[i];
            Poly content = factor.ContentAsPoly();
            AddUnit(PolynomialMethods.PolyPow(content, Exponents[i], false));
            factor = factor.DivideByLC(content);
            if (factor.SignumOfLC() < 0)
            {
                factor.Negate();
                if (Exponents[i] % 2 == 1)
                    base.Unit.Negate();
            }
        }

        return this;
    }


    public PolynomialFactorDecomposition<OthPoly> MapTo<OthPoly>(Func<Poly, OthPoly> mapper) where OthPoly : Polynomial<OthPoly>
    {
        return PolynomialFactorDecomposition<OthPoly>.Of(mapper(base.Unit), Factors.Select(mapper).ToList(), Exponents);
    }


    public PolynomialFactorDecomposition<Poly> ReduceUnitContent()
    {
        return base.Unit.IsOverField() ? Monic() : Primitive();
    }

    public PolynomialFactorDecomposition<Poly> Clone()
    {
        return new PolynomialFactorDecomposition<Poly>(base.Unit.Clone(), Factors.Select(f => f.Clone()).ToList(),
            new List<int>(Exponents));
    }


    public static PolynomialFactorDecomposition<Poly> Unit(Poly unit)
    {
        if (!unit.IsConstant())
            throw new ArgumentException();
        return Empty(unit).AddUnit(unit);
    }


    public static PolynomialFactorDecomposition<Poly> Empty(Poly factory)
    {
        return new PolynomialFactorDecomposition<Poly>(factory.CreateOne(), [], new List<int>());
    }


    public static PolynomialFactorDecomposition<Poly> Of(Poly unit, List<Poly> factors, List<int> exponents)
    {
        if (factors.Count != exponents.Count)
            throw new ArgumentException();
        PolynomialFactorDecomposition<Poly> r = Empty(unit).AddUnit(unit);
        for (int i = 0; i < factors.Count; i++)
            r.AddFactor(factors[i], exponents[i]);
        return r;
    }


    public static PolynomialFactorDecomposition<Poly> Of(params Poly[] factors)
    {
        if (factors.Length == 0)
            throw new ArgumentException();
        return Of(factors.ToList());
    }


    public static PolynomialFactorDecomposition<Poly> Of(IEnumerable<Poly> factors)
    {
        Dictionary<Poly, int> map = new Dictionary<Poly, int>();
        var polynomials = factors as Poly[] ?? factors.ToArray();
        foreach (Poly e in polynomials)
            if (!map.TryAdd(e, 1))
                map[e]++;
        List<Poly> l = [];
        List<int> _e = new List<int>();
        map.ForEach(ab =>
        {
            l.Add(ab.Key);
            _e.Add(ab.Value);
        });
        return Of(polynomials[0].CreateOne(), l, _e);
    }
}