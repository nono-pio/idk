using ConsoleApp1.Core.Expr.Fonction;

namespace ConsoleApp1.Core.Expr.Fonctions.Trigonometrie;

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
        return Cos(x);
    }

    public override Expr Reciproque(Expr y)
    {
        throw new NotImplementedException();
    }
    
    public override double N()
    {
        return Math.Sin(x.N());
    }
}