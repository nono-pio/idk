
using System.Diagnostics;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Core.Sets;
using ConsoleApp1.Parser;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

static void print(object? x)
{
    Console.WriteLine(x);
}

var set = Set.CreateInterval(1, 2, true, false);
var set2 = Set.CreateFiniteSet(3, 4, 5, 2, 1, 3);

print(set);
print(set2);
var test = set.Contains("x");
print(test);
