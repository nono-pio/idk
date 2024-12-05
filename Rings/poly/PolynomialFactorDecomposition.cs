using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;

namespace Rings.poly;

/**
 * {@inheritDoc}
 *
 * @since 1.0
 * @since 2.2 FactorDecomposition renamed to PolynomialFactorDecomposition
 */
public sealed class PolynomialFactorDecomposition<Poly> : FactorDecomposition<Poly> where Poly: IPolynomial<Poly> {
    private static readonly long serialVersionUID = 1L;

    private PolynomialFactorDecomposition(Poly unit, List<Poly> factors, TIntArrayList exponents) : base(PolynomialRing(unit), unit, factors, exponents){
    }

    private PolynomialFactorDecomposition(FactorDecomposition<Poly> factors) : base(factors.ring, factors.unit, factors.factors, factors.exponents){
        
    }

    
    public new bool isUnit(Poly element) {
        return element.isConstant();
    }

    
    public new PolynomialFactorDecomposition<Poly> setUnit(Poly unit) {
        base.setUnit(unit);
        return this;
    }

    
    public new PolynomialFactorDecomposition<Poly> addUnit(Poly unit) {
        base.addUnit(unit);
        return this;
    }

    
    public new PolynomialFactorDecomposition<Poly> addFactor(Poly factor, int exponent) {
        base.addFactor(factor, exponent);
        return this;
    }

    
    public new PolynomialFactorDecomposition<Poly> addAll(FactorDecomposition<Poly> other) {
        base.addAll(other);
        return this;
    }

    
    public new PolynomialFactorDecomposition<Poly> canonical() {
        if (factors.Count == 0)
            return this;
        reduceUnitContent();
        Poly[] fTmp = factors.ToArray();
        int[] eTmp = exponents.toArray();
        for (int i = fTmp.Length - 1; i >= 0; --i) {
            Poly poly = fTmp[i];
            if (poly.isMonomial() && eTmp[i] != 1) {
                poly = PolynomialMethods.polyPow(poly, eTmp[i], true);
                Debug.Assert( poly.isMonomial());
            }
            if (poly.signumOfLC() < 0) {
                poly.negate();
                if (eTmp[i] % 2 == 1)
                    unit.negate();
            }
        }

        ArraysUtil.quickSort(fTmp, eTmp);
        for (int i = 0; i < fTmp.Length; i++) {
            factors[i] = fTmp[i];
            exponents.set(i, eTmp[i]);
        }
        return this;
    }

    /**
     * Makes the lead coefficient of this factorization equal to the l.c. of specified poly via multiplication of this
     * by appropriate unit
     */
    public PolynomialFactorDecomposition<Poly> setLcFrom(Poly poly) {
        Poly u = ring.getOne();
        for (int i = 0; i < size(); i++)
            u = u.multiply(PolynomialMethods.polyPow(get(i).lcAsPoly(), getExponent(i)));
        return setUnit(PolynomialMethods.divideExact(poly.lcAsPoly(), u));
    }

    /**
     * Resulting lead coefficient
     */
    public Poly lc() {
        Poly u = unit.clone();
        for (int i = 0; i < size(); i++)
            u = u.multiply(PolynomialMethods.polyPow(get(i).lcAsPoly(), getExponent(i)));
        return u;
    }

    /**
     * Calculates the signum of the polynomial constituted by this decomposition
     *
     * @return the signum of the polynomial constituted by this decomposition
     */
    public int signum() {
        int signum = unit.signumOfLC();
        for (int i = 0; i < factors.Count; i++)
            signum *= exponents[i] % 2 == 0 ? 1 : factors[i].signumOfLC();
        return signum;
    }

    /**
     * Makes each factor monic (moving leading coefficients to the {@link #unit})
     */
    public PolynomialFactorDecomposition<Poly> monic() {
        for (int i = 0; i < factors.Count; i++) {
            Poly factor = factors[i];
            addUnit(polyPow(factor.lcAsPoly(), exponents[i], false));
            factor = factor.monic();
            Debug.Assert(factor != null);
        }
        return this;
    }

    /**
     * Makes each factor primitive (moving contents to the {@link #unit})
     */
    public PolynomialFactorDecomposition<Poly> primitive() {
        for (int i = 0; i < factors.Count; i++) {
            Poly factor = factors[i];
            Poly content = factor.contentAsPoly();
            addUnit(polyPow(content, exponents[i], false));
            factor = factor.divideByLC(content);
            Debug.Assert(factor != null);
            if (factor.signumOfLC() < 0) {
                factor.negate();
                if (exponents[i] % 2 == 1)
                    unit.negate();
            }
        }
        return this;
    }

    public PolynomialFactorDecomposition<OthPoly> mapTo<OthPoly>(Func<Poly, OthPoly> mapper) where OthPoly: IPolynomial<OthPoly> {
        return of(mapper(unit), factors.Select(mapper).ToList(), exponents);
    }

    /**
     * Calls {@link #monic()} if the coefficient ring is field and {@link #primitive()} otherwise
     */
    public PolynomialFactorDecomposition<Poly> reduceUnitContent() {
        return unit.isOverField() ? monic() : primitive();
    }

    
    public new PolynomialFactorDecomposition<Poly> clone() {
        return new PolynomialFactorDecomposition<Poly>(unit.clone(), factors.Select(p => p.clone()).ToList(), new TIntArrayList(exponents));
    }

    /** Unit factorization */
    public new static  PolynomialFactorDecomposition<Poly> unit(Poly unit) {
        if (!unit.isConstant())
            throw new ArgumentException();
        return empty(unit).addUnit(unit);
    }

    /** Empty factorization */
    public static PolynomialFactorDecomposition<Poly> empty(Poly factory) {
        return new PolynomialFactorDecomposition<Poly>(factory.createOne(), new List<Poly>(), new TIntArrayList());
    }

    /**
     * Factor decomposition with specified factors and exponents
     *
     * @param unit      the unit coefficient
     * @param factors   the factors
     * @param exponents the exponents
     */
    public static PolynomialFactorDecomposition<Poly>
    of(Poly unit, List<Poly> factors, TIntArrayList exponents) {
        if (factors.Count != exponents.size())
            throw new ArgumentException();
        PolynomialFactorDecomposition<Poly> r = empty(unit).addUnit(unit);
        for (int i = 0; i < factors.Count; i++)
            r.addFactor(factors[i], exponents.get(i));
        return r;
    }

    /**
     * Factor decomposition with specified factors and exponents
     *
     * @param factors factors
     */
    public static PolynomialFactorDecomposition<Poly>
    of(params Poly[] factors) {
        if (factors.Length == 0)
            throw new ArgumentException();
        return of(factors.ToList());
    }

    public static PolynomialFactorDecomposition<Poly>
    of(Poly a) {
        return of([a]);
    }

    public static PolynomialFactorDecomposition<Poly>
    of(Poly a, Poly b) {
        return of([a, b]);
    }

    public static PolynomialFactorDecomposition<Poly>
    of(Poly a, Poly b, Poly c) {
        return of([a, b, c]);
    }

    /**
     * Factor decomposition with specified factors and exponents
     *
     * @param factors factors
     */
    public static PolynomialFactorDecomposition<Poly> of(IEnumerable<Poly> factors) {
        TObjectIntHashMap<Poly> map = new TObjectIntHashMap<>();
        var facs = factors as Poly[] ?? factors.ToArray();
        foreach (Poly e in facs)
            map.adjustOrPutValue(e, 1, 1);
        List<Poly> l = new List<Poly>();
        TIntArrayList e = new TIntArrayList();
        map.forEachEntry((a, b) => {
            l.Add(a);
            e.Add(b);
            return true;
        });
        return of(facs[0].createOne(), l, e);
    }
}
