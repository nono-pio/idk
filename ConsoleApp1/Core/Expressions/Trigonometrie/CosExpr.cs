namespace ConsoleApp1.Core.Expressions.Trigonometrie;

public class CosExpr(Expr x) : TrigonometrieExpr(x)
{
    
    public static Expr Construct(Expr x) => new CosExpr(x);
    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Construct(exprs[0]);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => new CosExpr(exprs[0]);

    public override string Name => "cos";

    protected override Expr BaseDerivee() => -Sin(X);

    public override Expr Reciproque(Expr y) => ACos(y);

    public override double N() => Math.Cos(X.N());
}