using ConsoleApp1.core;
using ConsoleApp1.core.atoms;
using ConsoleApp1.utils;

static void print(object x)
{
    Console.WriteLine(x);
}


var x = Var("x");
var poly1 = new Poly(1.Expr(), 2.Expr(), 1.Expr());

var ans = Poly.YunSquareFree(poly1);
foreach (var a in ans)
{
    print(a);
}


//
//
// // (0,0) - (1,0), (1,1), (0,1) - (2,0), (2,1), (2,2), (1,2), (0,2) - ...
// static IEnumerable<(int, int)> Map2DInCircle(int max)
// {
//     int counter = 0;
//     
//     for (int n = 0; n < 20; n++)
//     {
//         int k = n * n + n;
//         for (int i = n*n; i <= n*n + 2*n; i++)
//         {
//
//             if (counter >= max)
//             {
//                 yield break;
//             }
//             
//             int dif = n - Math.Abs(k - i);
//
//             int x, y;
//             if (i <= k)
//             {
//                 x = n;
//                 y = dif;
//             }
//             else
//             {
//                 x = dif;
//                 y = n;
//             }
//
//             yield return (x, y);
//         }
//     }
// }
//
// static void print(object x)
// {
//     Console.WriteLine(x);    
// }
//
// var vars = Vars("a", "b", "c");
//
// var a = vars[0];
// var b = vars[1];
// var c = vars[2];
//
//
// (Expr, Expr)[] equations =
// {
//     (x+3, Zero),
//     (Pow(x, 2.Expr()), 2.Expr()),
//     (x*x-9, Zero),
//     (x, 3.Expr()),
//     (x*x, Zero)
// };
//
// foreach (var (expr, y) in equations)
// {
//     print($"{expr} = {y}");
//     var sol = Solve.SolveFor(expr, y, "x");
//     print($"> x = {sol}");
//     
// }

/*
// 1+x=a
print(((Addition) (1 + x)).Inverse(a, 1));
print(((Addition) (1 + x)).Inverse(a, 0));
*/

//print(1 + 2 + Sqrt(3.Expr()));


/* Power Eval Test
print(Pow(a,b)); // Normal
print(Pow(Pow(a,b),c)); // Power Tower
print(Pow(a,Pow(b,c))); // Not Power Tower
print(Pow(a,0.Expr()));
print(Pow(a,1.Expr()));
print(Pow(1.Expr(), a));
print(Pow(0.Expr(),a));
print(Pow(5.Expr(),2.Expr()));
print(Pow(1e60.Expr(),1e60.Expr())); // Error
*/
/* Multiplication Eval Test
print(a * b); // Normal
print(a * b * c); // Flatting
print(a * a); // Grouping
print(a * 0); // Zero return
print(a * 1); // Id 1
print(1 * a); // Id 2
print(a * 2); // Sorting 1
print(2 * a); // Sorting 2
print(2.Expr() * 2); // Grouping Constant
print(2.Expr() * 2 * b * b * c); // Grouping Constant + Grouping + Flatting
*/
/* Addition Eval Test
print(a + b); // Normal
print(a + b + c); // Flatting
print(a + a); // Grouping
print(a + 0); // Id
print(0 + a); // Id
print(a + 1); // Sorting 1
print(1 + a); // Sorting 2
print(2.Expr() + 2); // Grouping Constant
print(2.Expr() + 2 + b + b + c); // Grouping Constant + Grouping + Flatting
*/