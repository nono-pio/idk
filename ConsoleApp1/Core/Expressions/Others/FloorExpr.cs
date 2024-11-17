using System.Data;
using ConsoleApp1.Core.Evaluators;
using ConsoleApp1.Core.Sets;
using Sdcb.Arithmetic.Mpfr;

namespace ConsoleApp1.Core.Expressions.Others;

public class FloorExpr(Expr x) : FonctionExpr(x)
{
    
    public override bool IsNatural => IsPositive;
    public override bool IsInteger => true;

    public static readonly FunctionEval Evaluator = new FunctionEval(
        e => new FloorExpr(e),
        specialValueRule: e => e.IsInteger ? e : null,
        numFunc: Math.Floor,
        ifNotIntegerReturnNaN: true
        );

    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Evaluator.Eval(exprs, objects);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => Evaluator.Eval(exprs, objects);
    public override double N() => Evaluator.N(X);

    public override string Name => "floor";
    
    public override MpfrFloat NPrec(int precision = 333, MpfrRounding rnd = MpfrRounding.ToEven)
    {
        return MpfrFloat.Floor(X.NPrec(precision, rnd), precision);
    }
}