namespace ConsoleApp1.Core.Expressions.Trigonometrie;

public static class ConstructorTrigo
{
    public static Expr Cos(Expr expr)
    {
        return new CosExpr(expr);
    }

    public static Expr Sin(Expr expr)
    {
        return new SinExpr(expr);
    }

    public static Expr Tan(Expr expr)
    {
        return new TanExpr(expr);
    }
    
    public static Expr ACos(Expr expr)
    {
        return new ACosExpr(expr);
    }

    public static Expr ASin(Expr expr)
    {
        return new ASinExpr(expr);
    }

    public static Expr ATan(Expr expr)
    {
        return new ATanExpr(expr);
    }
}