using ConsoleApp1.Core.Expressions.Atoms;

namespace ConsoleApp1.Core.Expressions.Others;

public class FactorialExpr(Expr x) : FonctionExpr(x)
{
    
    public const int MAX_FACTORIAL_EVAL = 6;
    
    public static Expr Eval(Expr x)
    {
        if (x is Number num && num.IsNatural && num.Num <= MAX_FACTORIAL_EVAL)
        {
            return new Number(Factorial(num.ToInt()));
        }
        return new FactorialExpr(x);
    }
    
    public override Expr Eval(Expr[] exprs, object[]? objects = null)
    {
        return Eval(exprs[0]);
    }

    public override Expr NotEval(Expr[] exprs, object[]? objects = null)
    {
        return new FactorialExpr(exprs[0]);
    }

    public override double N()
    {
        var x = X.N();
        
        if (x < 0)
            throw new ArgumentException("x must be positive");
        if (x % 1 != 0)
            throw new ArgumentException("x must be an integer");
        
        return Factorial((long) x);
    }
    
    public static int Factorial(long n)
    {
        if (n < 0)
            throw new ArgumentException("n must be positive");
        
        var result = 1;
        for (int i = 1; i <= n; i++)
        {
            result *= i;
        }
        return result;
    }

    public override OrderOfOperation GetOrderOfOperation() => OrderOfOperation.Multiplication;
    public override string Name => "Factorial";
    public override string ToString()
    {
        return $"{ParenthesisIfNeeded(x)}!";
    }
    public override string ToLatex()
    {
        return $"{ParenthesisLatexIfNeeded(x)}!";
    }
}