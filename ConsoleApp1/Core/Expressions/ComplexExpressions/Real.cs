namespace ConsoleApp1.Core.Expressions.ComplexExpressions;

public class Real : Expr
{

    public Expr X => Args[0];
    
    public Real(Expr x) : base(x) {}

    public Expr Eval()
    {
        return this;
    }

    public override string ToLatex()
    {
        return $"\\RE({X.ToLatex()})";
    }

    public override string ToString()
    {
        return $"Re({X.ToLatex()})";
    }

    public override double N()
    {
        return X.N();
    }

    public override Expr Inverse(Expr y, int argIndex)
    {
        throw new Exception("Cannot take Inverse of the function Re(z)");
    }

    public override Expr Derivee(string variable)
    {
        return Re(X.Derivee(variable));
    }
}