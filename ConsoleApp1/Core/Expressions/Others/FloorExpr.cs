using System.Data;

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