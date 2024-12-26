﻿using System.Collections;
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

// Domain : ASin(Log(x) - 1)


// Good functions for domains
// ASin(Ln(x)) -> [1/e, e]
// Ln(x) -> ]0, +inf[
// Asin(Ln(x) - 1) -> [1, e^2]
// Exp(x)/Ln(x) -> ]1, +inf[ U [0, 1[

// Hermite
// var D = new Risch.DiffField((x-Tan(x))/Pow(Tan(x), 2) ,x);
// print(D.Dtemp);
//
// var f = D.ExprToRMPoly(D.f).ToUniPolynomialOfRational(1);
// print(Risch.HermiteReduce(f, D, 1));

// Poly reduce
// var D = new Risch.DiffField(1 + x * Tan(x) + Pow(Tan(x), 2), x);
// print(D.Dtemp);
//
// var f = D.ExprToRMPoly(D.f).ToUniPolynomialOfRational(1);
// print(Risch.PolynomialReduce(f.Numerator, D, 1));

// --- Univar test
// var polyA = new UnivariatePolynomial<Rational<BigInteger>>(Rings.Q, Rings.Q.FromArray([2, 0, 2, 0, 2]));
// var polyB = new UnivariatePolynomial<Rational<BigInteger>>(Rings.Q, Rings.Q.FromArray([1, 3, 3, 1]));
//
// print(polyA);
// print(polyB);
// print(IrreduciblePolynomials.IrreducibleQ(polyA));
// print(IrreduciblePolynomials.IrreducibleQ(polyB));



// x^2 + y^2 + z^2 + 2xy + 2xz + 2yz = (x + y + z)^2
var monomial1 = new Monomial<BigInteger>([2, 0, 0], 1);
var monomial2 = new Monomial<BigInteger>([1, 1, 0], -1);
var monomial3 = new Monomial<BigInteger>([1, 0, 1], 1);
var monomial4 = new Monomial<BigInteger>([0, 1, 1], -1);
// var monomial5 = new Monomial<BigInteger>([1, 0, 1], 2);
// var monomial6 = new Monomial<BigInteger>([0, 1, 1], 2);

var poly = MultivariatePolynomial<BigInteger>.Create(3, Rings.Z, MonomialOrder.GRLEX, [monomial1, monomial2, monomial3, monomial4]);

print(poly);
print(MultivariateFactorization.Factor(poly));
