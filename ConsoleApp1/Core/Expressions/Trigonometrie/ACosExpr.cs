namespace ConsoleApp1.Core.Expressions.Trigonometrie;

public class ACosExpr(Expr x) : TrigonometrieExpr(x)
{

    public static Expr Construct(Expr x) => new ACosExpr(x);
    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Construct(exprs[0]);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => new ACosExpr(exprs[0]);
    
    public override string Name => "acos";
    public override string LatexName => "cos^{-1}";

    protected override Expr BaseDerivee() => -1/Sqrt(1 - Pow(x, 2));

    public override Expr Reciproque(Expr y) => Cos(y);

    public override double N() => Math.Acos(X.N());
}