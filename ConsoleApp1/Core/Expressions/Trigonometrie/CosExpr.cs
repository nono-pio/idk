using ConsoleApp1.Core.Expressions.Atoms;
using Sdcb.Arithmetic.Mpfr;

namespace ConsoleApp1.Core.Expressions.Trigonometrie;

public class CosExpr(Expr x) : TrigonometrieExpr(x)
{
    public override Expr? BasePeriod => 2 * PI;

    public static Expr Construct(Expr x)
    {
        var n = x.AsMulIndependent(Atoms.Constant.PI);
        if (n is Number num)
        {
            if (MapValues.TryGetValue(num, out var values))
            {
                return values.Cos;
            }
        }
        
        
        return new CosExpr(x);
    }
    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Construct(exprs[0]);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => new CosExpr(exprs[0]);

    public override string Name => "cos";

    public override Expr fDerivee() => -Sin(X);

    public override Expr Reciproque(Expr y) => ACos(y);

    public override double N() => Math.Cos(X.N());
    
    public override MpfrFloat NPrec(int precision = 333, MpfrRounding rnd = MpfrRounding.ToEven)
    {
        return MpfrFloat.Cos(X.NPrec(precision, rnd), precision, rnd);
    }
}