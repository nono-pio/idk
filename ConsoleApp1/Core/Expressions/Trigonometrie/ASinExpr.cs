namespace ConsoleApp1.Core.Expressions.Trigonometrie;

public class ASinExpr(Expr x) : TrigonometrieExpr(x)
{

    public static Expr Construct(Expr x) => new ASinExpr(x);
    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Construct(exprs[0]);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => new ASinExpr(exprs[0]);
    
    public override string Name => "asin";
    public override string LatexName => "sin^{-1}";

    protected override Expr BaseDerivee() => 1/Sqrt(1 - Pow(x, 2));

    public override Expr Reciproque(Expr y) => Sin(y);

    public override double N() => Math.Asin(X.N());
}