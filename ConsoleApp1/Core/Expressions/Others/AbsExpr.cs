using ConsoleApp1.Core.Complexes;
using ConsoleApp1.Core.Evaluators;
using ConsoleApp1.Core.Sets;
using ConsoleApp1.Latex;

namespace ConsoleApp1.Core.Expressions.Others;

public class AbsExpr(Expr x) : FonctionExpr(x)
{

    public override bool IsNatural => X.IsInteger;
    public override bool IsInteger => X.IsInteger;
    public override bool IsReal => X.IsReal;

    public override bool IsPositive => true;
    public override bool IsNegative => false;

    public override string Name => "abs";

    public static readonly FunctionEval Evaluator = new FunctionEval(
        e => new AbsExpr(e),
        specialValueRule: x =>
        {
            if (x.IsZero)
                return x;
            if (x.IsPositive)
                return x;
            if (x.IsNegative)
                return -x;

            return null;
        },
        numFunc: Math.Abs,
        isEven: true
        );

    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Evaluator.Eval(exprs, objects);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => Evaluator.NotEval(exprs, objects);
    public override double N() => Evaluator.N(X);

    public override Complex AsComplex()
    {
        var complex = x.AsComplex();

        if (complex.Imaginary.IsZero)
            return new(this, 0);
        
        return new(complex.Norm, 0);
    }

    public override Expr fDerivee()
    {
        return Sign(X);
    }

    public override string ToLatex() => $@"\left|{X.ToLatex()}\right|";
    public override string ToString() => $"|{X}|";
}