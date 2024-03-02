using ConsoleApp1.Core.Expr.Fonction;

namespace ConsoleApp1.core.expr.fonctions.trigonometrie;

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
        return Tan(x) * Sec(x);
    }

    public override Expr Reciproque(Expr y)
    {
        throw new NotImplementedException();
    }
    
    public override double N()
    {
        return 1/Math.Cos(x.N());
    }
}