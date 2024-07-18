namespace ConsoleApp1.Core.Expressions.Trigonometrie;

public class TanExpr : FonctionExpr
{
    public TanExpr(Expr x) : base(x)
    {
    }

    public override string Name => "tan";

    protected override Expr BaseDerivee()
    {
        return 1 - Pow(Tan(X), 2);
    }

    public override Expr Reciproque(Expr y) => ATan(y);
    
    public override double N()
    {
        return Math.Tan(X.N());
    }
}