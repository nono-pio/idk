using ConsoleApp1.Core.Complexes;
using ConsoleApp1.Core.Sets;

namespace ConsoleApp1.Core.Expressions.Others;

public class AbsExpr(Expr x) : FonctionExpr(x)
{

    public override bool IsNatural => X.IsInteger;
    public override bool IsInteger => X.IsInteger;
    public override bool IsReal => X.IsReal;

    public override bool IsPositive => true;
    public override bool IsNegative => false;

    public override Set AsSet()
    {
        return ArithmeticOnSet.FunctionOnSet(Eval, X.AsSet(),
            (abs_start, abs_end) => Set.CreateInterval(Min(0, abs_start, abs_end), Max(abs_start, abs_end)),
            ArithmeticOnSet.FunctionBasicNumber(natural: Set.N, integer: Set.N, rational: Set.Q.Positive, real: Set.R.Positive)
            );
    }

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

    public override Expr fDerivee()
    {
        return Sign(X);
    }
}