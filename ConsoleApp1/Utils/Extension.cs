namespace ConsoleApp1.Utils;

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
    public static Expr Expr(this uint d)
    {
        return Num(d);
    }

    public static Expr Expr(this long d)
    {
        return Num(d);
    }

    public static TOut[] Map<TIn, TOut>(this TIn[] array, Func<TIn, TOut> func)
    {
        var newArray = new TOut[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            newArray[i] = func(array[i]);
        }

        return newArray;
    }
}