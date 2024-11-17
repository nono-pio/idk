using System.Data;
using ConsoleApp1.Core.Evaluators;
using ConsoleApp1.Core.Sets;
using Sdcb.Arithmetic.Mpfr;

namespace ConsoleApp1.Core.Expressions.Others;

public class SignExpr(Expr x) : FonctionExpr(x)
{
    public override bool IsNatural => IsPositive;
    public override bool IsInteger => true;

    public static readonly FunctionEval Evaluator = new FunctionEval(
        e => new AbsExpr(e),
        specialValueRule: x =>
        {
            if (x.IsZero)
                return 0;
            if (x.IsPositive)
                return 1;
            if (x.IsNegative)
                return -1;

            return null;
        },
        numFunc: x => Math.Sign(x),
        isEven: true
    );

    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Evaluator.Eval(exprs, objects);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => Evaluator.NotEval(exprs, objects);
    public override double N() => Evaluator.N(X);

    public override string Name => "sign";
    
    public override MpfrFloat NPrec(int precision = 333, MpfrRounding rnd = MpfrRounding.ToEven)
    {
        var r = new MpfrFloat(precision);
        var x = X.NPrec(precision, rnd);
        r.Assign(x.IsZero ? 0 : x.Sign);

        return r;
    }
}