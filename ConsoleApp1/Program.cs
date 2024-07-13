
using System.Diagnostics;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Parser;

static void print(object? x)
{
    Console.WriteLine(x);
}

/*
   \begin{pmatrix} ... \\ ... \end{pmatrix} -> Vecteur
   \begin{bmatrix} ... & ... \\ ... & ... \end{bmatrix} -> Matrice
*/

var inputs = new string[]
{
     "1",
     "-1.2e4",
     "1/2",
     "3*4",
     "x",
     "x+2",
     "2*x",
     "x/2",
     "x^2",
     "x^2+2*x+1",
     "2*(x+1)",
     "\\sqrt{2}",
     "\\sqrt[3]{2}+1",
     "f(x)",
     "\\pi",
     "\\pi(x)",
     "cos^2(x)",
     "sin^n(1)",
     "sin^2(x)+cos^2(x)",
     "x^{n+1}",
    @"\begin{pmatrix}x\\y\\z\end{pmatrix}",
    @"\begin{bmatrix}a&b\\c&d\end{bmatrix}"
};
 
foreach (var input in inputs)
{
    try
    {
        var test = Parser.GetExpr(input);
        print($"{input} : " + (test is null ? "null" : test.Value.Expr.ToLatex() + $" ({test.Value.Length} == {input.Length})"));
    }
    catch (Exception e)
    {
        Console.WriteLine($"{input} : Error {e.Message}");
    }
}