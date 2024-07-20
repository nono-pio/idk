
using System.Diagnostics;
using ConsoleApp1.Core.Classes;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Base;
using ConsoleApp1.Core.Integrals;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Core.Sets;
using ConsoleApp1.Core.TestDir;
using ConsoleApp1.Parser;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

static void print(object? x)
{
    Console.WriteLine(x);
}

// Optimistaion of the addition

var x = Var("x");
var y = Var("y");

Expr[] exprs = [ 2*x, x, y, 2, 3, 0, Pow(x, 2), x*y, 3*y, x*y ];

var time = Stopwatch.StartNew();
Expr result = null;
for (int i = 0; i < 100_000; i++)
{
    result = CreateAdd(exprs);
}
time.Stop();

print($"CreateAdd : {result} ({time.ElapsedMilliseconds})");

Expr[] exprs2 = [ 2*x, x, y, 2, 3, 0, Pow(x, 2), x*y, 3*y, x*y ];
time = Stopwatch.StartNew();
for (int i = 0; i < 100_000; i++)
{
    result = Add(exprs2);
}
time.Stop();

print($"Add : {result} ({time.ElapsedMilliseconds})");

static Expr CreateAdd(Expr[] exprs)
{
    // Rules
    // 1. x + x = 2x
    // 2. x + 0 = x
    // 3. x + (y + z) = x + y + z TODO
    // 4. Sort the terms

    NumberStruct numbersSum = 0;
    var exprsSum = new List<Expr>();
    foreach (var expr in exprs)
    {
        if (expr is Number num)
        {
            numbersSum += num.Num;
            continue;
        }

        var hasCombined = false;
        for (int i = 0; i < exprsSum.Count; i++)
        {
            Expr? combine = Combine(expr, exprsSum[i]);
            if (combine is not null)
            {
                exprsSum[i] = combine;
                hasCombined = true;
                break;
            }
        }
    
        if (!hasCombined)
            exprsSum.Add(expr);
    }

    if (!numbersSum.IsZero)
        exprsSum.Add(Num(numbersSum));

    return exprsSum.Count switch
    {
        0 => 0,
        1 => exprsSum[0],
        _ => new Addition(Sorting.BubbleSort(exprsSum.ToArray()))
    };
}
static Expr? Combine(Expr a, Expr b)
{
    if (a is not Multiplication && b is not Multiplication)
        return a == b ? 2 * a : null;

    if (a is Multiplication aMul && b is Multiplication bMul)
    {
        var hasANum = aMul.Factors[0] is Number ? 1 : 0;
        var hasBNum = bMul.Factors[0] is Number ? 1 : 0;

        if (aMul.Factors.Length - hasANum != bMul.Factors.Length - hasBNum)
            return null;
        
        var length = aMul.Factors.Length - hasANum;
        for (int i = 0; i < length; i++)
        {
            if (aMul.Factors[i + hasANum] != bMul.Factors[i + hasBNum])
                return null;
        }
        
        NumberStruct sum = (hasANum == 1 ? ((Number)aMul.Factors[0]).Num : 1) + (hasBNum == 1 ? ((Number)bMul.Factors[0]).Num : 1);

        if (sum == 0)
            return 0;

        if (sum == 1)
            return length == 1 ? aMul.Factors[hasANum] : Multiplication.MulNotEval(aMul.Factors[hasANum..]);
        
        var exprs = new Expr[length + 1];

        exprs[0] = Num(sum);
        Array.Copy(aMul.Factors, hasANum, exprs, 1, length);
        return Multiplication.MulNotEval(exprs);
    }

    Multiplication mul;
    Expr other;
    if (a is Multiplication aMult)
    {
        mul = aMult;
        other = b;
    }
    else
    {
        mul = (Multiplication)b;
        other = a;
    }

    if (mul.Factors.Length != 2 || mul.Factors[0] is not Number)
        return null;

    if (mul.Factors[1] == other)
    {
        var factor = ((Number)mul.Factors[0]).Num + 1;
        return factor == 0 ? 0 : Multiplication.MulNotEval([Num(factor), other]);
    }
    
    return null;
} 