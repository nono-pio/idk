using ConsoleApp1.Core.Evaluators;
using ConsoleApp1.Core.Expressions.Atoms;
using Sdcb.Arithmetic.Mpfr;

namespace ConsoleApp1.Core.Expressions.Others;

public class FactorialExpr(Expr x) : FonctionExpr(x)
{
    
    public const int MAX_FACTORIAL_EVAL = 6;
    public override string Name => "Factorial";


    public static readonly FunctionEval Evaluator = new FunctionEval(
        e => new FactorialExpr(e),
        specialValueRule: e => e is Number num && num.Num.IsInt  && num.Num <= MAX_FACTORIAL_EVAL ? Factorial(num.Num.Numerator) : null,
        ifNotIntegerReturnNaN:true,
        numFunc: n => Factorial((long) Math.Floor(n))
        );

    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Evaluator.Eval(exprs, objects);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => Evaluator.NotEval(exprs, objects);
    public override double N() => Evaluator.N(X);


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
    public override string ToString() => $"{ParenthesisIfNeeded(x)}!";
    public override string ToLatex() => $"{ParenthesisLatexIfNeeded(x)}!";
    
    public override MpfrFloat NPrec(int precision = 333, MpfrRounding rnd = MpfrRounding.ToEven)
    {
        var x = X.NPrec(precision, rnd);
        if (!x.FitsUInt32(rnd))
            return double.NaN;
        
        return MpfrFloat.Factorial(x.ToUInt32(), precision, rnd);
    }
}