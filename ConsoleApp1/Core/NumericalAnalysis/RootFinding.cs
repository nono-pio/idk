namespace ConsoleApp1.Core.NumericalAnalysis;

// https://en.wikipedia.org/wiki/Root-finding_algorithm
public static class RootFinding
{

    public const double PRECISION = 1e-8;

    /// find a root of a continuous function f in [a, b] with sign(f(a)) = -sign(f(b))
    public static double Bisection(double a, double b, Func<double, double> f, double precision = PRECISION, int maxIter = 1_000)
    {
        var fa = f(a);
        var fb = f(b);
        
        for (int i = 0; i < maxIter; i++)
        {
            if (b - a < 2 * precision)
                return (a + b) / 2;

            var c = (a + b) / 2;
            var fc = f(c);
            if (Math.Sign(fc) == -Math.Sign(fa))
            {
                b = c;
                fb = fc;
            }
            else
            {
                a = c;
                fa = fc;
            }

        }

        return double.NaN;
    } 
    
    /// find a root of a continuous function f in [a, b] with sign(f(a)) = -sign(f(b))
    public static double FalsePosition(double a, double b, Func<double, double> f, double precision = PRECISION, int maxIter = 1_000)
    {
        var fa = f(a);
        var fb = f(b);
        
        for (int i = 0; i < maxIter; i++)
        {
            if (b - a < 2 * precision)
                return (a + b) / 2;

            var c = (a * fb - b * fa) / (fb - fa);
            var fc = f(c);
            if (Math.Sign(fc) == -Math.Sign(fa))
            {
                b = c;
                fb = fc;
            }
            else
            {
                a = c;
                fa = fc;
            }

        }

        return double.NaN;
    }

    // find a root of a continuous function f in [a, b]
    // k1 (0, INF) k2 (1, 1 + golden ratio) n0 (0, INF)
    // f(a) < 0 < f(b)
    // Fastest
    // https://en.wikipedia.org/wiki/ITP_method
    public static double ITPMethod(double a, double b, Func<double, double> f, double precision = PRECISION, double k1 = 0.1, double k2 = 2, int n0 = 1, int maxIter = 1_000)
    {

        var ya = f(a);
        var yb = f(b);
        
        var n12 = (int)Math.Ceiling(Math.Log2((b - a) / (2 * precision)));
        var n_max = n12 + n0;
        var j = 0;
        while (b - a > 2 * precision)
        {
            if (j > maxIter)
                return double.NaN;
            
            var x12 = (b + a) / 2;
            var r = precision * (2 << (n_max - j)) - (b - a) / 2;
            var delta = k1 * Math.Pow(b - a, k2);

            var xf = (yb * a - ya * b) / (yb - ya);

            var sigma = Math.Sign(x12 - xf);
            double xt;
            if (delta <= Math.Abs(x12 - xf))
                xt = xf + sigma * delta;
            else
                xt = x12;

            double xITP;
            if (Math.Abs(xt - x12) <= r)
                xITP = xt;
            else
                xITP = x12 - sigma * r;

            var yITP = f(xITP);
            if (yITP > 0)
            {
                b = xITP;
                yb = yITP;
            } 
            else if (yITP < 0)
            {
                a = xITP;
                ya = yITP;
            }
            else
            {
                a = xITP;
                b = xITP;
            }

            j++;
        }

        return (b + a) / 2;
    }



    public static double NewtonMethod(double x0, Func<double, double> f, Func<double, double> f_ddx,
        double precision = PRECISION, int maxIter = 1_000)
    {
        var x = x0;
        var fx = f(x);
        var dfx = f_ddx(x);

        var i = 0;
        while (Math.Abs(fx) > precision)
        {
            if (i > maxIter)
                return double.NaN;

            x = x - fx / dfx;
            fx = f(x);
            dfx = f_ddx(x);

            i++;
        }

        return x;
    }

    public static double SecantMethod(double x0, double x1, Func<double, double> f, double precision = PRECISION, int maxIter = 1_000)
    {
        
        var fx0 = f(x0);
        var fx1 = f(x1);

        var i = 0;
        while (Math.Abs(fx1) > precision)
        {
            if (i > maxIter)
                return double.NaN;

            var new_x = x1 - fx1 * (x1 - x0) / (fx1 - fx0);

            x0 = x1;
            fx0 = fx1;
            x1 = new_x;
            fx1 = f(x1);

            i++;
        }

        return x1;
    }
    
    public static double SteffensenMethod(double x0, Func<double, double> f, double precision = PRECISION, int maxIter = 1_000)
    {
        double g(double x, double fx) => f(x + fx) / fx - 1;
        
        var x = x0;
        var fx = f(x);

        var i = 0;
        while (Math.Abs(fx) > precision)
        {
            if (i > maxIter)
                return double.NaN;

            x = x - fx / g(x, fx);
            fx = f(x);

            i++;
        }

        return x;
    }

    /// f(x) = x
    public static double FixedPointIteration(double x0, Func<double, double> f, double precision = PRECISION,
        int maxIter = 1_000)
    {
        var x = x0;
        var fx = f(x);

        var i = 0;
        while (Math.Abs(fx - x) > precision)
        {
            if (i > maxIter)
                return double.NaN;

            x = fx;
            fx = f(x);

            i++;
        }

        return x;
    }
}























