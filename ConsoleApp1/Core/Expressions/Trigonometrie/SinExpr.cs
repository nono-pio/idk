namespace ConsoleApp1.Core.Expressions.Trigonometrie;

public class SinExpr(Expr x) : FonctionExpr(x)
{

    public static Expr Construct(Expr x) => new SinExpr(x);
    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Construct(exprs[0]);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => new SinExpr(exprs[0]);
    
    public override string Name => "sin";

    public override Expr fDerivee() => Cos(X);

    public override Expr Reciproque(Expr y) => ASin(y);
    
    public override double N()
    {
        return Math.Sin(X.N());
    }
}