using System.Data;
using ConsoleApp1.Core.Sets;

namespace ConsoleApp1.Core.Expressions.Others;

public class FloorExpr(Expr x) : FonctionExpr(x)
{
    
    public override bool IsNatural => IsPositive;
    public override bool IsInteger => true;
    public static Expr Eval(Expr x)
    {
        if (x.IsInteger)
            return x;
        
        return new FloorExpr(x);
    }

    public override Set AsSet()
    {
        return ArithmeticOnSets.FunctionOnSet(Eval, X.AsSet(), ArithmeticOnSets.FunctionBasicNumber(natural: ConstructorSets.N, integer: Z, rational: Z, real: Z));
    }

    public override Expr Eval(Expr[] exprs, object[]? objects = null)
    {
        return Eval(exprs[0]);
    }

    public override Expr NotEval(Expr[] exprs, object[]? objects = null)
    {
        return new FloorExpr(exprs[0]);
    }

    public override double N()
    {
        return Math.Floor(x.N());
    }

    public override string Name => "floor";
}