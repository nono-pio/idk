namespace Polynomials.Utils;

using System;
using System.Collections.Generic;

public static class Combinatorics
{
    public static IEnumerable<int[]> GetCombinations(int n, int k)
    {
        if (k > n || k < 0)
        {
            throw new ArgumentException("k must be in the range [0, n].");
        }

        int[] combination = new int[k];
        foreach (var c in GenerateCombinations(n, k, 0, combination, 0))
        {
            yield return c;
        }
    }

    private static IEnumerable<int[]> GenerateCombinations(int n, int k, int start, int[] combination, int index)
    {
        if (index == k)
        {
            yield return (int[])combination.Clone();
            yield break;
        }

        for (int i = start; i < n; i++)
        {
            combination[index] = i;
            foreach (var c in GenerateCombinations(n, k, i + 1, combination, index + 1))
            {
                yield return c;
            }
        }
    }
}
