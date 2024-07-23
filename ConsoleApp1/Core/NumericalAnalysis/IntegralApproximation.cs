namespace ConsoleApp1.Core.NumericalAnalysis;

public static class IntegralApproximation
{
    public const int N = 100;

    /// number of f call : n
    public static double RectangleUp(double a, double b, Func<double, double> f, int n = N)
    {
        var dx = (b - a) / n;
        double I = 0;
        for (int i = 0; i < n; i++)
        {
            var xi = a + dx * i;
            I += f(xi);
        }

        return I * dx;
    }
    
    /// number of f call : n
    public static double RectangleDown(double a, double b, Func<double, double> f, int n = N)
    {
        var dx = (b - a) / n;
        double I = 0;
        for (int i = 1; i <= n; i++)
        {
            var xi = a + dx * i;
            I += f(xi);
        }

        return I * dx;
    }
    
    /// number of f call : n + 1
    public static double Trapezoidal(double a, double b, Func<double, double> f, int n = N)
    {
        var dx = (b - a) / n;
        double I = 0;
        for (int i = 1; i < n; i++)
        {
            var xi = a + dx * i;
            I += f(xi);
        }

        return (f(a) / 2 + I + f(b) / 2) * dx;
    }
    
    /// number of f call : n + 1
    public static double SimpsonOneThird(double a, double b, Func<double, double> f, int n = N)
    {
        var dx = (b - a) / n;
        double I1 = 0;
        double I2 = 0;
        for (int i = 1; i <= n / 2; i++)
        {
            var xi = a + dx * (2 * i - 1);
            I1 += f(xi);
        }
        
        for (int i = 1; i <= n / 2 - 1; i++)
        {
            var xi = a + dx * 2 * i;
            I2 += f(xi);
        }

        return (f(a) + 4 * I1  + 2 * I2 + f(b)) * dx / 3;
    }

    public static double Integral(double a, double b, Func<double, double> f, int n = N, double epsilon = 1e-10)
    {
        if (double.IsNegativeInfinity(a) && double.IsInfinity(b))
            return NegInfToInfIntegral(f, epsilon: epsilon, n: n);
        if (double.IsNegativeInfinity(a))
            return NegInfToBIntegral(b, f, epsilon: epsilon, n: n);
        if (double.IsInfinity(b))
            return AToInfIntegral(a, f, epsilon: epsilon, n: n);

        return SimpsonOneThird(a, b, f, n: n);
    }

    public static double NegInfToInfIntegral(Func<double, double> f, double epsilon = 1e-10, int n = N)
    {
        return Integral(-1+epsilon, 1-epsilon, x => f(x / (1 - x * x)) * (1 + x * x) / ((1 - x * x) * (1 - x * x)), n: n);
    }

    public static double AToInfIntegral(double a, Func<double, double> f, double epsilon = 1e-10, int n = N)
    {
        return Integral(0, 1-epsilon, x => f(a + x / (1 - x)) / ((1 - x) * (1 - x)), n: n);
    }

    
    public static double NegInfToBIntegral(double b, Func<double, double> f, double epsilon = 1e-10, int n = N)
    {
        return Integral(0+epsilon, 1, x => f(b - (1 - x) / x) / (x * x), n: n);
    }
}




















