
using System.Collections;
using System.Diagnostics;
using System.Runtime;
using ConsoleApp1.Algorithms;
using ConsoleApp1.Core.Classes;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Base;
using ConsoleApp1.Core.Expressions.Trigonometrie;
using ConsoleApp1.Core.Integrals;
using ConsoleApp1.Core.Limits;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Core.NumericalAnalysis;
using ConsoleApp1.Core.Polynomials.Rings;
using ConsoleApp1.Core.Polynomials.UnivariatePolynomials;
using ConsoleApp1.Core.Sets;
using ConsoleApp1.Core.Solvers;
using ConsoleApp1.Core.TestDir;
using ConsoleApp1.Parser;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

static void print(object? x)
{
    Console.WriteLine(x is null ? "null" : x.ToString());
}

Expr x = "x";
var expr = x + Pow(x + 1, -2); // x + 1/x^2

print(Limit.LimitOf(expr, "x", Expr.Inf));
