namespace ConsoleApp1.core.expr.fonctions.trigonometrie;

public class Tan : Fonction
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
        return Pow(Sec(x), Deux);
    }

    public override Expr Reciproque(Expr y)
    {
        throw new NotImplementedException();
    }
}