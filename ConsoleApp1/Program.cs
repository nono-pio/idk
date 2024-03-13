using ConsoleApp1.Core.Expressions.Atoms;

static void print(object? x)
{
    Console.WriteLine(x);
}

var a = Var("a", new ScalarVar(6));
var x = Var("x", new ExprVar( Pow(a, Deux) ));
print(x.N());