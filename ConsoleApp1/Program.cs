
using System.Diagnostics;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Parser;

static void print(object? x)
{
    Console.WriteLine(x);
}

string input = "-1.e-2";
var test = Parser.GetFloat(input);
print(test is null? "null" : test.Value);