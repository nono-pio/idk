namespace ConsoleApp1.Core;

public static class Constructor
{
    

    // Misc
    public static int Gcd(int a, int b)
    {
        while (a != b)
            if (a > b)
                a -= b;
            else
                b -= a;

        return a;
    }

    public static int Ppmc(int a, int b)
    {
        return a * b / Gcd(a, b);
    }
}