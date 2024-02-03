namespace ConsoleApp1.core.expr.fonctions.trigonometrie;

public class Sin : Fonction
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
}