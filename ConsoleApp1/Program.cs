
using System.Collections;
using System.Diagnostics;
using System.Runtime;
using System.Text;
using System.Text.Json.Serialization;
using ConsoleApp1.Algorithms;
using ConsoleApp1.Core.Classes;
using ConsoleApp1.Core.Equations;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Base;
using ConsoleApp1.Core.Expressions.Others;
using ConsoleApp1.Core.Expressions.Trigonometrie;
using ConsoleApp1.Core.Integrals;
using ConsoleApp1.Core.Limits;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Core.NumericalAnalysis;
using ConsoleApp1.Core.Polynomials;
using ConsoleApp1.Core.Series;
using ConsoleApp1.Core.Sets;
using ConsoleApp1.Core.Simplifiers;
using ConsoleApp1.Core.Solvers;
using ConsoleApp1.Core.TestDir;
using ConsoleApp1.Parser;
using Sdcb.Arithmetic.Mpfr;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;
using static ConsoleApp1.Core.Alphabet;

static void print(object? x)
{
    if (x is Array arr)
    {
        print(string.Join(" ; ", Enumerable.Range(0, arr.Length).Select(i => arr.GetValue(i).ToString())));
        return;
    }
    
    Console.WriteLine(x is null ? "null" : x.ToString());
}

// var f = Sin(x);
// var a = 0;
// var n = 5;
//
// print(TaylorSeries.TaylorSeriesOf(f, x, a, n));

// /// n = c*sqrt^2
// O(sqrt(n/2))
static (int, int) sqrt(int n)
{
    var sqrt = (int) Math.Floor(Math.Sqrt(n));

    if (n % (sqrt * sqrt) == 0) // c = 1 case
        return (1, sqrt);

    sqrt = (int) Math.Ceiling(sqrt / 1.41421356237); // c is min 2
    
    for (;n % (sqrt * sqrt) != 0; sqrt--) 
    {}

    return (n / (sqrt * sqrt), sqrt);
}

// Domain : ASin(Log(x) - 1)


// Good functions for domains
// ASin(Ln(x)) -> [1/e, e]
// Ln(x) -> ]0, +inf[
// Asin(Ln(x) - 1) -> [1, e^2]
// Exp(x)/Ln(x) -> ]1, +inf[ U [0, 1[

// var D = new Risch.DiffField(Exp(x) + Exp(Exp(x)) * Exp(x) ,x);
// print(D.t);
// print(D.T);
// print(D.D);
// print(D.Cases);
// print(Risch.Derivative(D.f, D));

var D = new Risch.DiffField(1, x)
{
    t = [x, t],
    T = [x, double.NaN],
    D = [1, -Pow(t, 2) - Num(3, 2) * t / x + 1/(2*x)]
};
var p = 4*Pow(x, 4)*Pow(t, 5) - 4*Pow(x, 3)*(x+1)*Pow(t, 4) + Pow(x, 2)*(2*x-3)*Pow(t, 3) + x*(2*Pow(x, 2)+7*x+2)*Pow(t, 2) -(4*Pow(x,2) +4*x-1)*t + 2*x - 1;
print(Risch.SplitFactor(p, D, 1));
