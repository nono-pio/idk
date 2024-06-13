namespace ConsoleApp1.Core.Expressions.Trigonometrie;

public class Cos : FonctionExpr
{
    public Cos(Expr x) : base(x)
    {
    }

    public override string Name() => "cos";

    protected override Expr BaseDerivee() => -Sin(X);

    public override Expr Reciproque(Expr y)
    {
        throw new NotImplementedException();
    }

    public override double N() => Math.Cos(X.N());
}