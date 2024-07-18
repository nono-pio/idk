namespace ConsoleApp1.Core.Expressions.Trigonometrie;

public class ATanExpr(Expr x) : TrigonometrieExpr(x)
{
    public override string Name => "atan";
    public override string LatexName => "tan^{-1}";

    protected override Expr BaseDerivee() => 1 / (1 + Pow(X, 2));

    public override Expr Reciproque(Expr y) => Tan(y);

    public override double N() => Math.Atan(X.N());
}