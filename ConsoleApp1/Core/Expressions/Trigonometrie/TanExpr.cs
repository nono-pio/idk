using ConsoleApp1.Core.Expressions.Atoms;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Expressions.Trigonometrie;

public class TanExpr(Expr x) : TrigonometrieExpr(x)
{
    public override Expr? BasePeriod => PI;

    public override Boolean DomainCondition => Boolean.NotEqual(X, PI/2 + new Variable("n", dummy: true, domain: Z) * PI);

    public static Expr Construct(Expr x)
    {
        var n = x.AsMulIndependent(Atoms.Constant.PI);
        if (n is Number num)
        {
            if (MapValues.TryGetValue(num, out var values))
            {
                return values.Tan;
            }
        }
        
        
        return new TanExpr(x);
    }
    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Construct(exprs[0]);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => new TanExpr(exprs[0]);
    
    public override string Name => "tan";

    public override Expr fDerivee() => 1 - Pow(Tan(X), 2);

    public override Expr Reciproque(Expr y) => ATan(y);
    
    public override double N()
    {
        return Math.Tan(X.N());
    }
}