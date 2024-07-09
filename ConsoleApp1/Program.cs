
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Models;

static void print(object? x)
{
    Console.WriteLine(x);
}

Expr x = "x";

var expr = Pow(4f, Num(1, 2));
print(expr);
print(expr.GetType());
