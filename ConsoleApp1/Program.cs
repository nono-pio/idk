
using System.Collections;
using System.Diagnostics;
using System.Runtime;
using ConsoleApp1.Core.Classes;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Base;
using ConsoleApp1.Core.Expressions.Trigonometrie;
using ConsoleApp1.Core.Integrals;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Core.NumericalAnalysis;
using ConsoleApp1.Core.Sets;
using ConsoleApp1.Core.Solvers;
using ConsoleApp1.Core.TestDir;
using ConsoleApp1.Parser;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

static void print(object? x)
{
    Console.WriteLine(x is null ? "null" : x.ToString());
}

static void eval<T>(Func<T> f, int n = 1_000)
{
    var time = Stopwatch.StartNew();
    T? x = default;
    for (int i = 0; i < n; i++)
    {
        x = f();
    }
    time.Stop();
    
    print($"x = {x} ({(double)time.ElapsedMilliseconds/n}ms)");
}

var x = Var("x");
var y = Var("y");

var a = new MultiPoly( [x],
    new MultiNomial(1, [1]),
    new MultiNomial(1, [0])
);
var b = new MultiPoly( [x],
    new MultiNomial(1, [1]),
    new MultiNomial(-1, [0])
);
print(a);
print(b);
print(a + b);
print(a - b);
print(a * b);
print(a / b);
