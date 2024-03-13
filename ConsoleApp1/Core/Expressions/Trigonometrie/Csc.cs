namespace ConsoleApp1.Core.Expressions.Trigonometrie;

public class Csc : FonctionExpr
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
        return Cot(X) * Csc(X);
    }

    public override Expr Reciproque(Expr y)
    {
        throw new NotImplementedException();
    }
    
    public override double N()
    {
        return 1/Math.Sin(X.N());
    }
}