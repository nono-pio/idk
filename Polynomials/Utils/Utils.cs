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
}