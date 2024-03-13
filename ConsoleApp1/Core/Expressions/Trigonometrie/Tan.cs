namespace ConsoleApp1.Core.Expressions.Trigonometrie;

public class Tan : FonctionExpr
{
    public Tan(Expr x) : base(x)
    {
    }

    public override string Name()
    {
        return "tan";
    }

    protected override Expr BaseDerivee()
    {
        return Pow(Sec(X), Deux);
    }

    public override Expr Reciproque(Expr y)
    {
        throw new NotImplementedException();
    }
    
    public override double N()
    {
        return Math.Tan(X.N());
    }
}