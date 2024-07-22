
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

double f(double x) => Math.Cos(x);
double f_ddx(double x) => -Math.Sin(x);

var a = 0;
var b = Math.PI;
var x0 = 0;
var x1 = Math.PI / 4;
double x;
var n = 10_000_000;

var time = Stopwatch.StartNew();
for (int i = 0; i < n; i++)
{
    x = RootFinding.Bisection(a, b, f);
}
time.Stop();
print($"{time.ElapsedMilliseconds}ms");
 
time = Stopwatch.StartNew();
for (int i = 0; i < n; i++)
{
    x = RootFinding.FalsePosition(a, b, f);
}
time.Stop();
print($"{time.ElapsedMilliseconds}ms");

time = Stopwatch.StartNew();
for (int i = 0; i < n; i++)
{
    x = RootFinding.ITPMethod(b, a, f);
}
time.Stop();
print($"{time.ElapsedMilliseconds}ms");

time = Stopwatch.StartNew();
for (int i = 0; i < n; i++)
{
    x = RootFinding.SteffensenMethod(x0, f);
}
time.Stop();
print($"{time.ElapsedMilliseconds}ms");

time = Stopwatch.StartNew();
for (int i = 0; i < n; i++)
{
    x = RootFinding.SecantMethod(x0, x1, f);
}
time.Stop();
print($"{time.ElapsedMilliseconds}ms");

time = Stopwatch.StartNew();
for (int i = 0; i < n; i++)
{
    x = RootFinding.NewtonMethod(x1, f, f_ddx);
}
time.Stop();
print($"{time.ElapsedMilliseconds}ms");

// cos(x) = x
// x = RootFinding.FixedPointIteration(x0, f);
// print($"x = {x}, f(x) = {f(x)}");