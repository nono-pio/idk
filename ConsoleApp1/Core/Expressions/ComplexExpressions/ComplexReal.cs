using ConsoleApp1.Core.Complexes;
using ConsoleApp1.Latex;

namespace ConsoleApp1.Core.Expressions.ComplexExpressions;

public class ComplexReal : Expr
{

    public Expr X => Args[0];
    
    public ComplexReal(Expr x) : base(x) {}

    public static Expr Construct(Expr x) => new ComplexReal(x);
    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Construct(exprs[0]);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => new ComplexReal(exprs[0]);

    public override Complex AsComplex()
    {
        return new(X.AsComplex().Real, 0);
    }
    
    public override string ToLatex()
    {
        return LatexUtils.Fonction(Symbols.RealPart, X.ToLatex());
    }

    public override string ToString()
    {
        return ToLatex();
    }

    public override double N()
    {
        return X.N();
    }

    public override Expr Reciprocal(Expr y, int argIndex)
    {
        throw new Exception("Cannot take Inverse of the function Re(z)");
    }

    public override Expr Derivee(string variable)
    {
        return Re(X.Derivee(variable));
    }
}