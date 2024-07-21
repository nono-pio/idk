using ConsoleApp1.Latex;

namespace ConsoleApp1.Core.Expressions.ComplexExpressions;

public class ComplexImag : Expr
{
    public Expr X => Args[0];
    
    public ComplexImag(Expr x) : base(x) {}
    
    public static Expr Construct(Expr x) => new ComplexImag(x);
    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Construct(exprs[0]);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => new ComplexImag(exprs[0]);

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