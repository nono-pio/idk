namespace ConsoleApp1.Core.Expressions.Trigonometrie;

public class CosExpr(Expr x) : TrigonometrieExpr(x)
{

    public override string Name => "cos";

    protected override Expr BaseDerivee() => -Sin(X);

    public override Expr Reciproque(Expr y) => ACos(y);

    public override double N() => Math.Cos(X.N());
}