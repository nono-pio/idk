
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
using ConsoleApp1.Core.Polynomials;
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

// var poly = new Polynomial([1, 1])*new Polynomial([1, 1])*new Polynomial([2, 1])*2;//new Polynomial([0, 0, 1, 1]);
// foreach (var (p, i) in poly.YunSquareFree().Select((p, i)=>(p, i)))
// {
//     print($"({p})^{i+1}\t{string.Join(";", p.Coefs.Select(c=>c.ToString()))}");
// }
// print(poly.YunSquareFree().Aggregate("", (acc, p) => acc + ";" + p));
// print(poly);

print(new Polynomial([1,4,6,4,1]).SolveDegree().Aggregate("", (acc, p) => acc + ";" + p.ToString()));