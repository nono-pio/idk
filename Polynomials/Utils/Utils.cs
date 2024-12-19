namespace Polynomials.Utils;

public static class Utils
{
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
        }
    }

    public static T[] GetRow<T>(this T[,] matrix, int row)
    {
        var result = new T[matrix.GetLength(1)];
        for (int i = 0; i < matrix.GetLength(1); i++)
        {
            result[i] = matrix[row, i];
        }

        return result;
    }
    
    public static int[] IntSetDifference(int[] main, int[] delete)
    {
        int bPointer = 0, aPointer = 0;
        int counter = 0;
        while (aPointer < delete.Length && bPointer < main.Length)
            if (delete[aPointer] == main[bPointer])
            {
                aPointer++;
                bPointer++;
            }
            else if (delete[aPointer] < main[bPointer])
                aPointer++;
            else if (delete[aPointer] > main[bPointer])
            {
                counter++;
                bPointer++;
            }

        counter += main.Length - bPointer;
        int[] result = new int[counter];
        counter = 0;
        aPointer = 0;
        bPointer = 0;
        while (aPointer < delete.Length && bPointer < main.Length)
            if (delete[aPointer] == main[bPointer])
            {
                aPointer++;
                bPointer++;
            }
            else if (delete[aPointer] < main[bPointer])
                aPointer++;
            else if (delete[aPointer] > main[bPointer])
                result[counter++] = main[bPointer++];
        Array.Copy(main, bPointer, result, counter, main.Length - bPointer);
        return result;
    }
    
    public static bool NextBoolean(this Random rnd)
    {
        return rnd.Next(2) == 1;
    }
}