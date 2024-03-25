namespace ConsoleApp1.Utils;

public class ListTheory
{

    /// [1,2,3] -> [[1,2,3], [1,3,2], [2,1,3], [2,3,1], [3,1,2], [3,2,1]]
    public static IEnumerable<T[]> Permutations<T>(T[] list)
    {
        throw new NotImplementedException();
    }
    
    /// 6:sum, 2:list length -> [[0, 6], [1, 5], [2, 4], [3, 3], [4, 2], [5, 1], [6, 0]]
    public static IEnumerable<int[]> SumAt(int sum, int n_length)
    {
        return SumAt(sum, n_length, n_length);
    }
    public static IEnumerable<int[]> SumAt(int sum, int n_values, int n_length_max)
    {
        if (n_values == 1)
        {
            var list = new int[n_length_max];
            list[^1] = sum;
            yield return list;
            yield break;
        }
        
        
        for (int i = 0; i <= sum; i++)
        {
            var lists = SumAt(sum - i, n_values - 1, n_length_max);
            foreach (var list in lists)
            {
                list[^n_values] = i;
                yield return list;
            }
        }
    } 
    
}