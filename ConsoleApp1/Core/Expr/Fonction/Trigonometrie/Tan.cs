namespace ConsoleApp1.Core.Expr.Fonction.Trigonometrie;

public class Tan : FonctionExpr
{
    public Tan(core.expr.Expr x) : base(x)
    {
    }

    public override string Name()
    {
        return "tan";
    }

    protected override core.expr.Expr BaseDerivee()
    {
        return Pow(Sec(x), Deux);
    }

    public override core.expr.Expr Reciproque(core.expr.Expr y)
    {
        throw new NotImplementedException();
    }
    
    public override double N()
    {
        return Math.Tan(x.N());
    }
}