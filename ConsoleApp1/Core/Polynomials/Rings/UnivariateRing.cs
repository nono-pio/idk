using ConsoleApp1.Core.Polynomials.UnivariatePolynomials;

namespace ConsoleApp1.Core.Polynomials.Rings;

public class UnivariateRing<TPoly> : Ring<TPoly> where TPoly : IUnivariatePolynomial<TPoly>
{

    public TPoly Factory;
    public UnivariateRing(TPoly factory)
    {
        Factory = factory;
    }
    
    public override TPoly Zero => Factory.Zero;
    public override TPoly One => Factory.One;
    
    public override TPoly ValueOf(int value) => Factory.ValueOf(value);
    
    public override bool IsZero(TPoly e) => Factory.IsZero(e);
    public override bool IsOne(TPoly e) => Factory.IsOne(e);
    public override bool IsInt(TPoly e, int value) => Factory.IsInt(e, value);

    public override TPoly Add(TPoly a, TPoly b) => Factory.Add(a, b);
    public override TPoly MAdd(TPoly a, TPoly b) => Factory.MAdd(a, b);
    public override TPoly Sub(TPoly a, TPoly b) => Factory.Sub(a, b);
    public override TPoly MSub(TPoly a, TPoly b) => Factory.MSub(a, b);
    public override TPoly Neg(TPoly a) => Factory.Neg(a);
    public override TPoly Mul(TPoly a, TPoly b) => Factory.Mul(a, b);
    public override TPoly MulInt(TPoly a, int b) => Factory.MulInt(a, b);
    public override TPoly MMul(TPoly a, TPoly b) => Factory.MMul(a, b);
    public override (bool isNull, TPoly Value) SafeDiv(TPoly a, TPoly b) => Factory.SafeDiv(a, b);
    public override TPoly Rem(TPoly a, TPoly b) => Factory.Rem(a, b);
    public override (TPoly Quotient, TPoly Remainder) DivRem(TPoly a, TPoly b) => Factory.DivRem(a, b);

    public override TPoly Gcd(TPoly a, TPoly b) => Factory.Gcd(a, b);
}