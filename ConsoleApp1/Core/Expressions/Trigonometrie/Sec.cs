namespace ConsoleApp1.Core.Expressions.Trigonometrie;

public class Sec : FonctionExpr
{
    public Sec(Expr x) : base(x)
    {
    }

    public override string Name()
    {
        return "sec";
    }

    protected override Expr BaseDerivee()
    {
        return Tan(X) * Sec(X);
    }

    public override Expr Reciproque(Expr y)
    {
        throw new NotImplementedException();
    }
    
    public override double N()
    {
        return 1/Math.Cos(X.N());
    }
}