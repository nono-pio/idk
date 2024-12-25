using Polynomials.Poly.Univar;

namespace Polynomials.Poly;

public abstract class Polynomial<Poly> where Poly : Polynomial<Poly> 
{
 
    public abstract bool IsConstant();
    public abstract bool IsMonomial();
    public abstract int SignumOfLC();
    public abstract Poly Negate();
    public abstract Poly? DivideByLC(Poly other);
    public abstract Poly ContentAsPoly();
    public abstract Poly LcAsPoly();
    public abstract Poly Monic();
    public abstract Poly Clone();
    public abstract Poly Multiply(Poly other);
    public abstract Poly Add(Poly other);
    public abstract bool IsOverField();
    public abstract bool IsOne();
    public abstract Poly CreateOne();
    public abstract Poly Square();
    public abstract Poly DivideExact(Poly other);
    public abstract Poly Gcd(Poly other);

    public abstract PolynomialRing<Poly> AsRing();
}
