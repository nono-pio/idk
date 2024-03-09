namespace ConsoleApp1.Core.Expr.Fonction.Trigonometrie;

public class Tan : FonctionExpr
{
    public Tan(Core.Expr.Expr x) : base(x)
    {
    }

    public override string Name()
    {
        return "tan";
    }

    protected override Core.Expr.Expr BaseDerivee()
    {
        return Pow(Sec(x), Deux);
    }

    public override Core.Expr.Expr Reciproque(Core.Expr.Expr y)
    {
        throw new NotImplementedException();
    }
    
    public override double N()
    {
        return Math.Tan(x.N());
    }
}