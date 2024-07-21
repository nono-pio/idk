namespace ConsoleApp1.Core.Expressions.Trigonometrie;

public static class ConstructorTrigo
{
    public static Expr Cos(Expr expr) => CosExpr.Construct(expr);

    public static Expr Sin(Expr expr) => SinExpr.Construct(expr);

    public static Expr Tan(Expr expr) => TanExpr.Construct(expr);
    
    public static Expr ACos(Expr expr) => ACosExpr.Construct(expr);

    public static Expr ASin(Expr expr) => ASinExpr.Construct(expr);

    public static Expr ATan(Expr expr) => ATanExpr.Construct(expr);
}