namespace ConsoleApp1.Core.Expressions.ComplexExpressions;

public static class ConstructorComplex
{

    public static Expr Re(Expr x)
    {
        return new Real(x).Eval();
    }
    
    public static Expr Im(Expr x)
    {
        return new Imag(x).Eval();
    }
}