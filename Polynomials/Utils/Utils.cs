using System.Collections;

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
        return values[..(i + 1)]; // Array.CopyOf(values, i + 1);
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

    public static T[,] Swap<T>(T[,] a, int i, int j)
    {
        for (int k = 0; k < a.GetLength(1); k++)
        {
            var t = a[i, k];
            a[i, k] = a[j, k];
            a[j, k] = t;
        }

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

    public static T[] Remove<T>(T[] array, int[] positions)
    {
        if (array == null)
            throw new NullReferenceException();
        int[] p = GetSortedDistinct(positions);
        if (p.Length == 0)
            return array;
        int size = p.Length, pointer = 0, s = array.Length;
        for (; pointer < size; ++pointer)
            if (p[pointer] >= s)
                throw new IndexOutOfRangeException();
        T[] r = new T[array.Length - p.Length];
        pointer = 0;
        int i = -1;
        for (int j = 0; j < s; ++j)
        {
            if (pointer < size - 1 && j > p[pointer])
                ++pointer;
            if (j == p[pointer])
                continue;
            else
                r[++i] = array[j];
        }

        return r;
    }

    public static int[] Negate(int[] arr)
    {
        for (int i = 0; i < arr.Length; i++)
            arr[i] = -arr[i];
        return arr;
    }

    public static T[,] AsArray2D<T>(this T[][] array)
    {
        var result = new T[array.Length, array[0].Length];
        for (int i = 0; i < array.Length; i++)
        {
            for (int j = 0; j < array[i].Length; j++)
            {
                result[i, j] = array[i][j];
            }
        }

        return result;
    }

    public static int Cardinality(this BitArray arr)
    {
        int count = 0;
        for (int i = 0; i < arr.Length; i++)
            if (arr[i])
                count++;
        return count;
    }

    public static int NextSetBit(this BitArray bitArray, int fromIndex)
    {
        if (bitArray == null)
            throw new ArgumentNullException(nameof(bitArray));

        if (fromIndex < 0 || fromIndex >= bitArray.Length)
            throw new ArgumentOutOfRangeException(nameof(fromIndex), "Index hors des limites de la BitArray.");

        for (int i = fromIndex; i < bitArray.Length; i++)
        {
            if (bitArray[i])
                return i;
        }

        return -1; // Retourne -1 si aucun bit `true` n'est trouvé
    }

    public static void Set(this BitArray bitArray, int from, int to, bool value)
    {
        for (int i = from; i < to; i++)
        {
            bitArray[i] = value;
        }
    }

    public class ComparerBy<T> : IComparer<T>
    {
        private readonly Func<T, int> _comparer;

        public ComparerBy(Func<T, int> comparer)
        {
            _comparer = comparer;
        }

        public int Compare(T? x, T? y)
        {
            return _comparer(x).CompareTo(_comparer(y));
        }
    }

    public static void RemoveAll<T>(this List<T> list, List<T> toRemove)
    {
        foreach (var item in toRemove)
        {
            list.Remove(item);
        }
    }

    public static T[] ArrayOf<T>(T value, int length)
    {
        var array = new T[length];
        Array.Fill(array, value);
        return array;
    }

    public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> toCheck)
    {
        return toCheck.All(source.Contains);
    }

    public static int FirstIndexOf<T>(this IEnumerable<T> source, T element)
    {
        int i = 0;
        foreach (var item in source)
        {
            if (Equals(item, element))
                return i;
            i++;
        }

        return -1;
    }

    public static void InsertionSort<T, B>(T[] target, B[] coSort) where T : IComparable<T>
    {
        InsertionSort(target, 0, target.Length, coSort);
    }

    public static void InsertionSort<T, B>(T[] target, int fromIndex, int toIndex, B[] coSort) where T : IComparable<T>
    {
        int i, j;
        T key;
        B keyC;
        for (i = fromIndex + 1; i < toIndex; i++)
        {
            key = target[i];
            keyC = coSort[i];
            for (j = i; j > fromIndex && target[j - 1].CompareTo(key) > 0; j--)
            {
                target[j] = target[j - 1];
                coSort[j] = coSort[j - 1];
            }

            target[j] = key;
            coSort[j] = keyC;
        }
    }
}