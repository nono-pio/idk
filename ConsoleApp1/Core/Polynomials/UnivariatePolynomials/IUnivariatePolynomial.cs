namespace ConsoleApp1.Core.Polynomials.UnivariatePolynomials;

public interface IUnivariatePolynomial<TSelf> where TSelf : IUnivariatePolynomial<TSelf>
{
    
    public TSelf Zero { get; }
    public TSelf One { get; }
    public TSelf ValueOf(int value);
    public bool IsZero(TSelf e);
    public bool IsOne(TSelf e);
    public bool IsInt(TSelf e, int value);
    public TSelf Add(TSelf a, TSelf b);
    public TSelf MAdd(TSelf a, TSelf b);
    public TSelf Sub(TSelf a, TSelf b);
    public TSelf MSub(TSelf a, TSelf b);
    public TSelf Neg(TSelf a);
    public TSelf Mul(TSelf a, TSelf b);
    public TSelf MulInt(TSelf a, int b);
    public TSelf MMul(TSelf a, TSelf b);
    public TSelf Div(TSelf a, TSelf b);
    public TSelf Rem(TSelf a, TSelf b);
    public (TSelf Quotient, TSelf Remainder) DivRem(TSelf a, TSelf b);
    public TSelf Gcd(TSelf a, TSelf b);
}