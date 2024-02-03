namespace ConsoleApp1.core.expr.fonctions.trigonometrie;

public class Sec : Fonction
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
}