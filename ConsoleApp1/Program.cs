
using System.Diagnostics;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Parser;

static void print(object? x)
{
    Console.WriteLine(x);
}

string input = "xn";

var test = Parser.GetExpr(input);
print(test is null? "null" : test.Value);