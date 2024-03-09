namespace ConsoleApp1.RandomAlgo;

public class NewtonMethode
{
    private const int MAX_ITER = 1_000;
    private const double PRECISION = 1e-50d;
    
    /// x_{n+1} = x_n - f(x_n)/f'(x_n)
    public static double RootOf(double x0, Func<double, double> f, Func<double, double> f_ddx, 
        int max_iter=MAX_ITER, double precision=PRECISION)
    {
        var x = x0;
        var y = f(x0);
        var y_ddx = f_ddx(x0);
        var i = 0;
        while (double.Abs(y) > precision && i < max_iter)
        {
            if (y_ddx == 0)
            {
                throw new Exception("Error Newton Methode : Division By Zero");
            }

            var div = y / y_ddx;
            if (!double.IsNormal(div))
            {
                throw new Exception("Error Newton Methode : " + $"Wrong Value f(x):{y}, f'(x):{y_ddx}, f(x)/f'(x):{div}");
            }
            
            x -= div;
            y = f(x);
            y_ddx = f_ddx(x);

            i++;
        }

        if (i == max_iter)
        {
            throw new Exception("Error Newton Methode : Max Iteration");
        }

        return x;
    }

    public static double RootOf(Expr function, string variable, int max_iter = MAX_ITER, double precision = PRECISION)
    {
        var f = function.AsFonction(variable);
        var f_ddx = f.Derivee();
        return RootOf(0d, f.N, f_ddx.N, precision: precision, max_iter: max_iter);
    }
}