using ConsoleApp1.Core.Complexes;

namespace ConsoleApp1.Core.Expressions.Others;

public class AbsExpr(Expr x) : FonctionExpr(x)
{

    public override string Name => "abs";

    public static Expr Eval(Expr x)
    {
        if (x.IsZero)
            return x;
        if (x.IsPositive)
            return x;
        if (x.IsNegative)
            return -x;
        
        return new AbsExpr(x);
    }
    public override Expr Eval(Expr[] exprs, object[]? objects = null)
    {
        return Eval(exprs[0]);
    }

    public override Expr NotEval(Expr[] exprs, object[]? objects = null)
    {
        return new AbsExpr(exprs[0]);
    }

    public override Complex AsComplex()
    {
        var complex = x.AsComplex();

        if (complex.Imaginary.IsZero)
            return new(this, 0);
        
        return new(complex.Norm, 0);
    }

    public override double N()
    {
        return Math.Abs(x.N());
    }

    public override Expr Derivee(string variable)
    {
        return Sign(x);
    }
}