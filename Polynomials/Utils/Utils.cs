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
    
    public static int[] GetSortedDistinct(int[] values)
    {
        if (values.Length == 0)
            return values;
        Array.Sort(values);
        int shift = 0;
        int i = 0;
        while (i + shift + 1 < values.Length)
            if (values[i + shift] == values[i + shift + 1])
                ++shift;
            else
            {
                values[i] = values[i + shift];
                ++i;
            }

        values[i] = values[i + shift];
        return values[..(i + 1)];// Array.CopyOf(values, i + 1);
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
    
    public static E Pop<E>(this List<E> list, int index)
    {
        var result = list[index];
        list.RemoveAt(index);
        return result;
    }
    
    public static int[] Sequence(int size)
    {
        return Sequence(0, size);
    }

    public static int[] Sequence(int from, int to)
    {
        int[] ret = new int[to - from];
        for (int i = ret.Length - 1; i >= 0; --i)
            ret[i] = from + i;
        return ret;
    }
    
    public static int[] Max(int[] a, int[] b)
    {
        int[] r = new int[a.Length];
        for (int i = 0; i < a.Length; i++)
            r[i] = Math.Max(a[i], b[i]);
        return r;
    }
    
    public static T[] Swap<T>(T[] a, int i, int j)
    {
        var t = a[i];
        a[i] = a[j];
        a[j] = t;
        return a;
    }
    
    public static T[] AddAll<T>(T[] array1, params T[] array2)
    {
        T[] r = new T[array1.Length + array2.Length];
        Array.Copy(array1, 0, r, 0, array1.Length);
        Array.Copy(array2, 0, r, array1.Length, array2.Length);
        return r;
    }
    
    public static T[] Remove<T>(T[] array, int i)
    {
        if (i >= array.Length)
            throw new IndexOutOfRangeException();
        if (array.Length == 1)
        {
            return new T[0];
        }
        else if (array.Length == 2)
            return new T[]
            {
                array[1 ^ i]
            };
        T[] newArray = new T[array.Length - 1];
        Array.Copy(array, 0, newArray, 0, i);
        if (i != array.Length - 1)
            Array.Copy(array, i + 1, newArray, i, array.Length - i - 1);
        return newArray;
    }
    public static int[] Negate(int[] arr)
    {
        for (int i = 0; i < arr.Length; i++)
            arr[i] = -arr[i];
        return arr;
    }
    
}