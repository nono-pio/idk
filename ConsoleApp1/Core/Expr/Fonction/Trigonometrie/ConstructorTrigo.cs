using ConsoleApp1.Core.Expr.Fonctions.Trigonometrie;

namespace ConsoleApp1.Core.Expr.Fonction.Trigonometrie;

public static class ConstructorTrigo
{
    public static Expr Cos(Expr expr)
    {
        return new Cos(expr);
    }

    public static Expr Sin(Expr expr)
    {
        return new Sin(expr);
    }

    public static Expr Csc(Expr expr)
    {
        return new Csc(expr);
    }

    public static Expr Sec(Expr expr)
    {
        return new Sec(expr);
    }

    public static Expr Tan(Expr expr)
    {
        return new Tan(expr);
    }

    public static Expr Cot(Expr expr)
    {
        return new Cot(expr);
    }
}