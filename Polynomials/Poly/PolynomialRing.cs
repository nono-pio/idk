namespace Polynomials.Poly.Univar;

public abstract class PolynomialRing<Poly> : Ring<Poly> where Poly : Polynomial<Poly>
{
    public abstract int NVariables();


    public abstract Poly Factory();


    public abstract Poly Variable(int variable);


    new int Signum(Poly element)
    {
        return element.SignumOfLC();
    }
}