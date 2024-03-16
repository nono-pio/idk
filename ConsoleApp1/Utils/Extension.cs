namespace ConsoleApp1.Utils;

public static class Extension
{
    public static string Join<T>(this List<T> exprs)
    {
        if (exprs.Count == 0)
        {
            return "[]";
        }

        var str = exprs[0].ToString();
        for (int i = 1; i < exprs.Count; i++)
        {
            str += "," + exprs[i].ToString();
        }

        return "[" + str + "]";
    }
    public static Expr[] Exprs(this object[] objs)
    {
        return objs.Map(obj => (Expr) obj);
    }
    
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