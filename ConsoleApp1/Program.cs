
using System.Diagnostics;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Integrals;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Core.Sets;
using ConsoleApp1.Parser;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

static void print(object? x)
{
    Console.WriteLine(x);
}

Expr x = "x";
var expr1 = 1 + 2*x + Pow(x, 2);
var expr2 = Pow(x+1, 2);
print(expr1 == expr2);
print(expr1.IsEqualStrong(expr2));