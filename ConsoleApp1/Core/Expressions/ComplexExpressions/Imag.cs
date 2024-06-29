using ConsoleApp1.Latex;

namespace ConsoleApp1.Core.Expressions.ComplexExpressions;

public class Imag : Expr
{
    public Expr X => Args[0];
    
    public Imag(Expr x) : base(x) {}

    public Expr Eval()
    {
        return this;
    }

    public override string ToLatex()
    {
        return LatexUtils.Fonction(Symbols.ImaginaryPart, X.ToLatex());
    }

    public override string ToString()
    {
        return ToLatex();
    }

    public override double N()
    {
        throw new NotImplementedException();
    }

    public override Expr Inverse(Expr y, int argIndex)
    {
        throw new Exception("Cannot take Inverse of the function Im(z)");
    }

    public override Expr Derivee(string variable)
    {
        return Im(X.Derivee(variable));
    }
}