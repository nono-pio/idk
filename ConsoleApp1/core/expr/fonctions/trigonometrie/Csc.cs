namespace ConsoleApp1.core.expr.fonctions.trigonometrie;

public class Csc : Fonction
{
    public Csc(Expr x) : base(x)
    {
    }

    public override string Name()
    {
        return "csc";
    }

    protected override Expr BaseDerivee()
    {
        return Cot(x) * Csc(x);
    }

    public override Expr Reciproque(Expr y)
    {
        throw new NotImplementedException();
    }
}