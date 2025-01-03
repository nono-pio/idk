using System.Diagnostics;
using System.Numerics;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Polynomials;
using Polynomials;
using Polynomials.Poly.Multivar;
using Polynomials.Poly.Univar;

namespace ConsoleApp1.Core.Integrals;

public class RischUtils
{
    
}

// DiffField is a field with a differentiation operator over rational or univariate polynomials over a field K
public abstract class UniDiffField<K>
{

    public Variable t { get; }
    public UnivariatePolynomial<K> tPoly;
    public Rational<UnivariatePolynomial<K>> Dt;
    public UnivariatePolynomial<K> DtPoly;
    

    public UniDiffField(Variable t)
    {
        this.t = t;
        tPoly = PolynomialFactory.Uni(Ring, [0, 1]);
        Dt = D(tPoly);
        DtPoly = Dt.NumeratorExact();
    }
    
    public abstract Rational<UnivariatePolynomial<K>> Derivative(UnivariatePolynomial<K> f);

    public virtual Rational<UnivariatePolynomial<K>> Derivative(Rational<UnivariatePolynomial<K>> f)
    {
        var newNumerator = Derivative(f.Numerator()) * f.Denominator() - f.Numerator() * Derivative(f.Denominator());
        var newDenominator = f.Denominator().Square();
        
        return PolynomialFactory.RationalPoly(newNumerator.Numerator(), newDenominator * newNumerator.Denominator());
    }

    public Rational<UnivariatePolynomial<K>> D(Rational<UnivariatePolynomial<K>> f) => Derivative(f);
    public Rational<UnivariatePolynomial<K>> D(UnivariatePolynomial<K> f) => Derivative(f);
    public UnivariatePolynomial<K> DPoly(Rational<UnivariatePolynomial<K>> f) => Derivative(f).NumeratorExact();
    public UnivariatePolynomial<K> DPoly(UnivariatePolynomial<K> f) => Derivative(f).NumeratorExact();

    public K DKinK(K f)
    {
        var fPoly = UnivariatePolynomial<K>.Constant(Ring, f);
        var d = DPoly(fPoly);
        Debug.Assert(d.Degree() == 0);
        return d.Lc();
    }

    
    public abstract Expr FieldToExpr(K element);
    
    public Expr ToExpr(K element) => FieldToExpr(element);

    public Expr ToExpr(UnivariatePolynomial<K> poly)
    {
        var result = Zero;
        for (int i = 0; i <= poly.Degree(); i++)
        {
            result += FieldToExpr(poly[i]) * Pow(t, i);
        }
        
        return result;
    }
    
    public Expr ToExpr(Rational<UnivariatePolynomial<K>> poly) => ToExpr(poly.Numerator()) / ToExpr(poly.Denominator());

    public abstract K FromExpr(Expr expr);
    
    public Polynomial ToPolynomial(UnivariatePolynomial<K> poly) => new Polynomial(poly.data.Select(FieldToExpr).ToArray());

    public abstract Ring<K> Ring { get; }
    public UnivariateRing<K> RingPoly => Rings.UnivariateRing(Ring);
    public Rationals<UnivariatePolynomial<K>> RingRatPoly => Rings.Frac(RingPoly);

}

// K = Q
public class UniDiffFieldQ : UniDiffField<Rational<BigInteger>>
{
    
    public UniDiffFieldQ(Variable t) : base(t) { }
    
    public override Rational<UnivariatePolynomial<Rational<BigInteger>>> Derivative(UnivariatePolynomial<Rational<BigInteger>> f)
    {
        return PolynomialFactory.RationalPoly(f.Derivative());
    }

    public override Expr FieldToExpr(Rational<BigInteger> element)
    {
        return Num((long)element.Numerator(), (long)element.Denominator());
    }

    public override Rational<BigInteger> FromExpr(Expr expr)
    {
        if (expr is Number num && num.Num.IsFraction)
        {
            return Rings.Q.Mk(num.Num.Numerator, num.Num.Denominator);
        }

        throw new Exception();
    }
    
    public override Ring<Rational<BigInteger>> Ring => Rings.Q;
}

// K = Q(t0, .., tn) 
public class UniDiffFieldMulti : UniDiffField<Rational<MultivariatePolynomial<Rational<BigInteger>>>>
{
    public Risch.DiffField DiffField;
    public int i;
    
    public UniDiffFieldMulti(Risch.DiffField D, int i) : base(D.t[i])
    {
        DiffField = D;
        this.i = i;
    }

    public override Rational<UnivariatePolynomial<Rational<MultivariatePolynomial<Rational<BigInteger>>>>> Derivative(UnivariatePolynomial<Rational<MultivariatePolynomial<Rational<BigInteger>>>> f)
    {
        throw new NotImplementedException();
    }

    public override Expr FieldToExpr(Rational<MultivariatePolynomial<Rational<BigInteger>>> element)
    {
        throw new NotImplementedException();
    }

    public override Rational<MultivariatePolynomial<Rational<BigInteger>>> FromExpr(Expr expr)
    {
        throw new NotImplementedException();
    }

    public override Ring<Rational<MultivariatePolynomial<Rational<BigInteger>>>> Ring 
        => Rings.Frac(Rings.MultivariateRing(DiffField.t.Length, Rings.Q));
}