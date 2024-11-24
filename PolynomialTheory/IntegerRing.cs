namespace PolynomialTheory;

public class IntegerRing : IRing<int>
{
    public int Zero => 0;
    public int One => 1;
    
    public int Add(int a, int b) => a + b;
    public int Subtract(int a, int b) => a - b;
    public int Negate(int a) => -a;
    
    public int Multiply(int a, int b) => a * b;

    public int Inverse(int a) => a == 1 ? 1 : a == -1 ? -1 : throw new ArgumentException("L'inverse de " + a + " n'est pas dans l'anneau.");

    public int Divide(int a, int b) => a % b == 0 ? a / b : throw new ArgumentException("La division de " + a + " par " + b + " n'est pas dans l'anneau.");

    public bool IsZero(int a) => a == 0;

    public bool IsInversible(int a) => a == 1 || a == -1;
    public bool IsDivisible(int a, int b) => a % b == 0;
    public bool IsField() => false;
}