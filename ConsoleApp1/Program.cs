using ConsoleApp1.Core.Expressions.Base;

static void print(object? x)
{
    Console.WriteLine(x);
}

var a = Var("a");
var b = Var("b");
var c = Var("c");

var expr = Power.NewtonMultinomial(new Expr[]{a,b,c}, 6);
print(expr);

