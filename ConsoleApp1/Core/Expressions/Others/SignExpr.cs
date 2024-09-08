using System.Data;
using ConsoleApp1.Core.Sets;

namespace ConsoleApp1.Core.Expressions.Others;

public class SignExpr(Expr x) : FonctionExpr(x)
{
    public override bool IsNatural => IsPositive;
    public override bool IsInteger => true;

    public static Expr Eval(Expr x)
    {
        if (x.IsZero)
            return 0;
        if (x.IsPositive)
            return 1;
        if (x.IsNegative)
            return -1;
        
        return new SignExpr(x);
    }

    public override Set AsSet()
    {
        return ArithmeticOnSet.FunctionOnSet(Eval, X.AsSet(), 
            ArithmeticOnSet.FunctionBasicNumber(
                natural: Set.CreateFiniteSet([0, 1]), 
                integer: Set.CreateFiniteSet([-1, 0, 1]), 
                rational: Set.CreateFiniteSet([-1, 0, 1]), 
                real: Set.CreateFiniteSet([-1, 0, 1])
                )
            );
    }

    public override Expr Eval(Expr[] exprs, object[]? objects = null)
    {
        return Eval(exprs[0]);
    }

    public override Expr NotEval(Expr[] exprs, object[]? objects = null)
    {
        return new SignExpr(exprs[0]);
    }

    public override double N()
    {
        return Math.Sign(X.N());
    }

    public override string Name => "sign";
}