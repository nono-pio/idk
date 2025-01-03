using System.Collections;
using System.Diagnostics;
using System.Numerics;
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
using Polynomials;
using Polynomials.Poly.Multivar;
using Polynomials.Poly.Univar;
using Polynomials.Primes;
using Polynomials.Utils;
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
    var sqrt = (int)Math.Floor(Math.Sqrt(n));

    if (n % (sqrt * sqrt) == 0) // c = 1 case
        return (1, sqrt);

    sqrt = (int)Math.Ceiling(sqrt / 1.41421356237); // c is min 2

    for (; n % (sqrt * sqrt) != 0; sqrt--)
    {
    }

    return (n / (sqrt * sqrt), sqrt);
}


// Hermite
// var D = new Risch.DiffField((x - Tan(x)) / Pow(Tan(x), 2), x);
// var Diff = Risch.DefaultDiff(1, D);
// print(D.Dtemp);
//
// var f = D.ExprToRMPoly(D.f).ToRUPoly(1);
// print(Risch.HermiteReduce(f, Diff));

// Poly reduce
// var D = new Risch.DiffField(1 + x * Tan(x) + Pow(Tan(x), 2), x);
// var Diff = Risch.DefaultDiff(1, D);
// print(D.Dtemp);
//
// var f = D.ExprToRMPoly(D.f).ToRUPoly(1);
// print(Risch.PolynomialReduce(f.Numerator(), Diff));

// var expr = (2 * Pow(Ln(x), 2) - Ln(x) - Pow(x, 2)) / (Pow(Ln(x), 3) - Pow(x, 2) * Ln(x));
// var D = new Risch.DiffField(expr, x);
// var Diff = Risch.DefaultDiff(1, D);
// print(D.Dtemp);
//
// var f = D.ExprToRMPoly(D.f).ToRUPoly(1);
// var residue = Risch.ResidueReduce(f, Diff);
// print(residue);

var A = PolynomialFactory.Uni(Rings.Q, [1]);
var D = PolynomialFactory.Uni(Rings.Q, [0, 1]);

print(A);
print(D);
print(RationalPolynomialIntegration.IntegrateRationalPolynomial(A, D, new UniDiffFieldQ(x)));
// var residue = RationalPolynomialIntegration.IntRationalLogPart(A, D); // A/D = d/dx(g) + h