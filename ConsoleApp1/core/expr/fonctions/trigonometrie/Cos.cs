namespace ConsoleApp1.core.expr.fonctions.trigonometrie;

public class Cos : Fonction
{
    public Cos(Expr x) : base(x)
    {
    }

    public override string Name()
    {
        return "cos";
    }

    protected override Expr BaseDerivee()
    {
        return -Sin(x);
    }

    public override Expr Reciproque(Expr y)
    {
        throw new NotImplementedException();
    }
}