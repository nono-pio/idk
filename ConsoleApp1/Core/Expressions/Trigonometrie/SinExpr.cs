namespace ConsoleApp1.Core.Expressions.Trigonometrie;

public class SinExpr : FonctionExpr
{
    public SinExpr(Expr x) : base(x)
    {
    }

    public override string Name => "sin";

    protected override Expr BaseDerivee()
    {
        return Cos(X);
    }

    public override Expr Reciproque(Expr y) => ASin(y);
    
    public override double N()
    {
        return Math.Sin(X.N());
    }
}