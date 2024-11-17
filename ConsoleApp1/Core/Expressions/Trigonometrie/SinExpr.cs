using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Others;
using Sdcb.Arithmetic.Mpfr;

namespace ConsoleApp1.Core.Expressions.Trigonometrie;

public class SinExpr(Expr x) : TrigonometrieExpr(x)
{
    public override Expr? BasePeriod => 2 * PI;

    public static Expr Construct(Expr x)
    {
        var n = x.AsMulIndependent(Atoms.Constant.PI);
        if (n is Number num)
        {
            if (MapValues.TryGetValue(num, out var values))
            {
                return values.Sin;
            }
        }
        
        
        return new SinExpr(x);
    }
    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Construct(exprs[0]);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => new SinExpr(exprs[0]);
    
    public override string Name => "sin";

    public override Expr fDerivee() => Cos(X);

    public override Expr Reciproque(Expr y) => ASin(y);
    
    public override double N()
    {
        return Math.Sin(X.N());
    }

    public override MpfrFloat NPrec(int precision = 333, MpfrRounding rnd = MpfrRounding.ToEven)
    {
        return MpfrFloat.Sin(X.NPrec(precision, rnd), precision, rnd);
    }
}