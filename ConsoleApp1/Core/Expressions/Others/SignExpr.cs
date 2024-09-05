using System.Data;

namespace ConsoleApp1.Core.Expressions.Others;

public class SignExpr(Expr x) : FonctionExpr(x)
{
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