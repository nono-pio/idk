namespace ConsoleApp1.Core.Expressions.Trigonometrie;

public class TanExpr(Expr x) : FonctionExpr(x)
{

    public static Expr Construct(Expr x) => new TanExpr(x);
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