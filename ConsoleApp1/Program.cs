
using System.Diagnostics;
using ConsoleApp1.Core.Classes;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Base;
using ConsoleApp1.Core.Integrals;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Core.NumericalAnalysis;
using ConsoleApp1.Core.Sets;
using ConsoleApp1.Core.TestDir;
using ConsoleApp1.Parser;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

static void print(object? x)
{
    Console.WriteLine(x);
}

static void eval(Func<double> f, int n = 100_000)
{
    var time = Stopwatch.StartNew();
    double x = 0;
    for (int i = 0; i < n; i++)
    {
        x = f();
    }
    time.Stop();
    
    print($"x = {x} ({(double)time.ElapsedMilliseconds/n}ms)");
}

double f(double x) => Math.Cos(x); // I cosx dx = sinx (sinb - sina) a=0 b=pi/2 I cosx dx = 1
double f_inf(double x) => Math.Exp(-x * x); // I e^-x^2 dx 0->INF = sqrt{pi}/2
double a = 0;
double b = Math.PI / 2;

int n = 500;
double epsilon = 1e-10;
var sqrtpi = Math.Sqrt(Math.PI);
var sqrtpi_2 = Math.Sqrt(Math.PI)/2;

// eval(() => IntegralApproximation.RectangleUp(a, b, f, n: n));
// eval(() => IntegralApproximation.RectangleDown(a, b, f, n: n));
// eval(() => IntegralApproximation.Trapezoidal(a, b, f, n: n));
// eval(() => IntegralApproximation.SimpsonOneThird(a, b, f, n: n));
eval(() => IntegralApproximation.NegInfToInfIntegral(f_inf, epsilon: epsilon, n: n) - sqrtpi);
eval(() => IntegralApproximation.AToInfIntegral(0, f_inf, epsilon: epsilon, n: n) - sqrtpi_2);
eval(() => IntegralApproximation.NegInfToBIntegral(0, f_inf, epsilon: epsilon, n: n) - sqrtpi_2);