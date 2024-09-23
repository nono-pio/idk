
using System.Collections;
using System.Diagnostics;
using System.Runtime;
using ConsoleApp1.Algorithms;
using ConsoleApp1.Core.Classes;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Base;
using ConsoleApp1.Core.Expressions.Others;
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
using static ConsoleApp1.Core.Alphabet;

static void print(object? x)
{
    Console.WriteLine(x is null ? "null" : x.ToString());
}

var x = new Variable("x", dummy:true);
var x2 = x;

print(x == x2);