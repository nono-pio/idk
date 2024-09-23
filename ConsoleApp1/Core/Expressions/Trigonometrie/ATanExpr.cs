namespace ConsoleApp1.Core.Expressions.Trigonometrie;

public class ATanExpr(Expr x) : TrigonometrieExpr(x)
{

    public static Expr Construct(Expr x) => new ATanExpr(x);
    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Construct(exprs[0]);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => new ATanExpr(exprs[0]);
    
    public override string Name => "atan";
    public override string LatexName => "tan^{-1}";

    public override Expr fDerivee() => 1 / (1 + Pow(X, 2));

    public override Expr Reciproque(Expr y) => Tan(y);

    public override double N() => Math.Atan(X.N());
}