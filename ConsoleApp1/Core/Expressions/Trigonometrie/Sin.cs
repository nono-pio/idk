namespace ConsoleApp1.Core.Expressions.Trigonometrie;

public class Sin : FonctionExpr
{
    public Sin(Expr x) : base(x)
    {
    }

    public override string Name()
    {
        return "sin";
    }

    protected override Expr BaseDerivee()
    {
        return Cos(X);
    }

    public override Expr Reciproque(Expr y)
    {
        throw new NotImplementedException();
    }
    
    public override double N()
    {
        return Math.Sin(X.N());
    }
}