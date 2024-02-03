namespace ConsoleApp1.utils;

public static class Extension
{
    public static Expr Expr(this double d)
    {
        return Num(d);
    }

    public static Expr Expr(this float d)
    {
        return Num(d);
    }


    public static Expr Expr(this int d)
    {
        return Num(d);
    }


    public static Expr Expr(this long d)
    {
        return Num(d);
    }
}