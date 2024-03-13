namespace ConsoleApp1.Utils;

public static class NumberUtils
{
    
    public static int Gcd(int a, int b)
    {
        if (a < b)
            (a, b) = (b, a);
        
        while (b > 0)
            (a, b) = (b, a % b);

        return a;
    }
    
    public static int Lcm(int a, int b)
    {
        return a * b / Gcd(a, b);
    }

    public static (int, int) AsNumDen(double x, double precision=1e-10)
    {

        int n = (int) x;
        x -= n;
        if (x < precision)
            return (n, 1);
        else if (1 - precision < x)
            return (n + 1, 1);

        var lower_n = 0;
        var lower_d = 1;

        var upper_n = 1;
        var upper_d = 1;
        
        int middle_n;
        int middle_d;
        while (true)
        {
            middle_n = lower_n + upper_n;
            middle_d = lower_d + upper_d;
            
            // x + precision < middle
            if (middle_d * (x + precision) < middle_n)
            {
                upper_n = middle_n;
                upper_d = middle_d;
            } // middle < x - precision
            else if (middle_n < (x - precision) * middle_d)
            {
                lower_n = middle_n;
                lower_d = middle_d;
            }
            else // middle between  x +- precision
                break;

        }
        
        var result_n = n * middle_d + middle_n;
        var result_d = middle_d;
        
        // simplify
        var gcd = Gcd(result_n, result_d);
        result_n /= gcd;
        result_d /= gcd;
        
        return (result_n, result_d);

    }

    public static IEnumerable<(int, int)> AsFactorExp(double x)
    {

        var (n, d) = AsNumDen(x);

        using var n_iter = AsFactorExp(n).GetEnumerator();
        using var d_iter = AsFactorExp(d).GetEnumerator();
        
        var is_d_empty = !d_iter.MoveNext();
        var (d_fac, d_exp) = d_iter.Current;
        while (n_iter.MoveNext())
        {
            var (n_fac, n_exp) = n_iter.Current;
        
            if (is_d_empty)
            {
                yield return (n_fac, n_exp);
                continue;
            }
            
            while (d_fac < n_fac)
            {
                yield return (d_fac, -d_exp);
                if (!d_iter.MoveNext())
                {
                    is_d_empty = true;
                    yield return (n_fac, n_exp);
                    break;
                }
        
                (d_fac, d_exp) = d_iter.Current;
            }
        
            if (is_d_empty)
                continue;
            
            
            if (d_fac == n_fac)
            {
                if (n_exp != d_exp) 
                    yield return (n_fac, n_exp - d_exp);
                
                if (!d_iter.MoveNext())
                    is_d_empty = true;
                else
                    (d_fac, d_exp) = d_iter.Current;
                
                continue;
            }
            
            if (n_fac < d_fac)
                yield return (n_fac, n_exp);
            
        }

        do
            yield return (d_iter.Current.Item1, -d_iter.Current.Item2);
        while (d_iter.MoveNext());
    }
    
    public static IEnumerable<(int, int)> AsFactorExp(int n)
    {
        
        var bound = (int) Math.Sqrt(n) + 1;
        for (int i = 2; i <= bound; i++)
        {
            var exp = 0;
            while (n % i == 0)
            {
                n /= i;
                exp++;
            }
            yield return (i, exp);

            if (n == 1)
            {
                yield break;
            }
        }
        
    }
    
    
}