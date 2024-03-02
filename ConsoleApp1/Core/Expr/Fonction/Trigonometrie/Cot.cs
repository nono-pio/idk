using ConsoleApp1.Core.Expr.Fonction;

namespace ConsoleApp1.core.expr.fonctions.trigonometrie;

public class Cot : FonctionExpr
{
    public Cot(Expr x) : base(x)
    {
    }

    public override string Name()
    {
        return "cot";
    }

    protected override Expr BaseDerivee()
    {
        return -Pow(Csc(x), Deux);
    }

    public override Expr Reciproque(Expr y)
    {
        throw new NotImplementedException();
    }
    
    public override double N()
    {
        return 1/Math.Tan(x.N());
    }
}