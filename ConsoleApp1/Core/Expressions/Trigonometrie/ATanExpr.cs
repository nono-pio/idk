using Sdcb.Arithmetic.Mpfr;

namespace ConsoleApp1.Core.Expressions.Trigonometrie;

public class ATanExpr(Expr x) : TrigonometrieExpr(x)
{

    public static Expr Construct(Expr x) => new ATanExpr(x);
    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Construct(exprs[0]);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => new ATanExpr(exprs[0]);
    
    public override string Name => "atan";
    public override string LatexName => "tan^{-1}";

    public override Expr fDerivee() => 1 / (1 + Pow(X, 2));

    public override Expr Reciproque(Expr y) => Tan(y);

    public override double N() => Math.Atan(X.N());
    
    public override MpfrFloat NPrec(int precision = 333, MpfrRounding rnd = MpfrRounding.ToEven)
    {
        return MpfrFloat.Atan(X.NPrec(precision, rnd), precision, rnd);
    }
}