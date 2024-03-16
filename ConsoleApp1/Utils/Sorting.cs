namespace ConsoleApp1.Utils;

public class Sorting
{
    public static Expr[] BubbleSort(Expr[] exprs)
    {

        var n = exprs.Length;
        bool swapped;
        for (int i = 0; i < n - 1; i++) {
            swapped = false;
            for (int j = 0; j < n - i - 1; j++)
            {
                if (exprs[j] > exprs[j + 1]) 
                {
                    (exprs[j], exprs[j + 1]) = (exprs[j + 1], exprs[j]);
                    swapped = true;
                }
            }
            
            if (swapped == false)
                break;
        }

        return exprs;
    } 
}