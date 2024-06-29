
static void print(object? x)
{
    Console.WriteLine(x);
}

var x = Var("x");
var y = Var("y");

Expr expr = 1 + Pow(1 + x, 2.Expr());

print(expr);

