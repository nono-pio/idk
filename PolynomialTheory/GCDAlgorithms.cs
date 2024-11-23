namespace PolynomialTheory;

public static class GCDAlgorithms
{
    public static int GCD(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }

        return a;
    }
}