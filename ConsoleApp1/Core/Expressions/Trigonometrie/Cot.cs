namespace ConsoleApp1.Core.Expressions.Trigonometrie;

public class Cot : FonctionExpr
{
    public Cot(Expr x) : base(x)
    {
    }

    public override string Name()
    {
        return "cot";
    }

    protected override Expr BaseDerivee()
    {
        return -Pow(Csc(X), Deux);
    }

    public override Expr Reciproque(Expr y)
    {
        throw new NotImplementedException();
    }
    
    public override double N()
    {
        return 1/Math.Tan(X.N());
    }
}