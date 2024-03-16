using ConsoleApp1.Core.Expressions.Atoms;

static void print(object? x)
{
    Console.WriteLine(x);
}

var x = Var("x");
var y = Var("y");
print(x*y*y*x*Deux);


