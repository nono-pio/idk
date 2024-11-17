using ConsoleApp1.Core.Evaluators;
using ConsoleApp1.Core.Sets;
using Sdcb.Arithmetic.Mpfr;

namespace ConsoleApp1.Core.Expressions.Others;

public class CeilExpr(Expr x) : FonctionExpr(x)
{
    
    public override bool IsNatural => IsPositive;
    public override bool IsInteger => true;
   
    public static readonly FunctionEval Evaluator = new FunctionEval(
        e => new CeilExpr(e),
        specialValueRule: e => e.IsInteger ? e : null,
        numFunc: Math.Ceiling,
        ifNotIntegerReturnNaN: true
    );

    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Evaluator.Eval(exprs, objects);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => Evaluator.Eval(exprs, objects);
    public override double N() => Evaluator.N(X);


    public override string Name => "ceil";
    
    public override MpfrFloat NPrec(int precision = 333, MpfrRounding rnd = MpfrRounding.ToEven)
    {
        return MpfrFloat.Ceiling(X.NPrec(precision, rnd), precision);
    }
}