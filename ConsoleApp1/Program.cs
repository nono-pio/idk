
using ConsoleApp1.Core.Models;

static void print(object? x)
{
    Console.WriteLine(x);
}

var x = Var("x");
var y = Var("y");

Poly poly = new Poly(1, 1);
print(poly);
print(poly.Pow(2));
print(poly.Pow(3));
print(poly.Pow(4));

print(poly * poly);

// Expr expr = 1/x;
// for (int d = 0; d < 10; d++)
// {
//     expr += Pow(x, d.Expr());
// }
//
// var watch = System.Diagnostics.Stopwatch.StartNew();
//
// var isPoly = Poly.IsPolynomial(expr, "x");
//
// watch.Stop();
// var elapsedMs = watch.ElapsedMilliseconds;
//
// print(expr);
// print(isPoly);
// print(elapsedMs);