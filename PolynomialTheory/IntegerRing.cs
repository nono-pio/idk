namespace PolynomialTheory;

public class IntegerRing : IRing<int>
{
    public int Add(int a, int b) => a + b;

    public int Subtract(int a, int b) => a - b;

    public int Multiply(int a, int b) => a * b;

    public int Negate(int a) => -a;

    public int Zero => 0;
    public int One => 1;
}