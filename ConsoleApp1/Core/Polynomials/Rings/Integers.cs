namespace ConsoleApp1.Core.Polynomials.Rings;

public class Integers : Ring<int>
{
    public override int Zero => 0;
    public override int One => 1;
    
    public override int ValueOf(int value)
    {
        return value;
    }

    public override bool IsInt(int e, int value) => e == value;
    public override bool IsZero(int e) => e == 0;
    public override bool IsOne(int e) => e == 1;

    public override int Add(int a, int b)
    {
        return a + b;
    }

    public override int Sub(int a, int b)
    {
        return a - b;
    }

    public override int Neg(int e)
    {
        return -e;
    }

    public override int Mul(int a, int b)
    {
        return a * b;
    }

    public override int MulInt(int a, int b)
    {
        return a * b;
    }

    public override int Div(int a, int b)
    {
        return a / b;
    }

    public override int Rem(int a, int b)
    {
        return a % b;
    }

    public override (int Quotient, int Remainder) DivRem(int a, int b)
    {
        return (a / b, a % b);
    }
    
}