using ConsoleApp1.RandomAlgo;

static void print(object x)
{
    Console.WriteLine(x);
}

var f = (double x) => Math.Log(x);
var f_ddx = (double x) => 1/x;

var x = NewtonMethode.RootOf(0, f, f_ddx, precision:1e-300d);
print((x, f(x)));