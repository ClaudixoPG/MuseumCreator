using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HeldKarp
{
    public static Tuple<int,List<int>> CalculateValue(int[,] distances)
    {
        /*float[,] distances = {
            { 0, 29, 20, 21, 17, 30, 10, 15 },
            { 29, 0, 15, 26, 12, 24, 18, 25 },
            { 20, 15, 0, 16, 28, 13, 22, 27 },
            { 21, 26, 16, 0, 18, 23, 30, 19 },
            { 17, 12, 28, 18, 0, 22, 25, 14 },
            { 30, 24, 13, 23, 22, 0, 16, 20 },
            { 10, 18, 22, 30, 25, 16, 0, 15 },
            { 15, 25, 27, 19, 14, 20, 15, 0 }
        };*/

        //int startCity = 0;  // Change as needed
        //int endCity = 7;    // Change as needed
        int startCity = 0;
        int endCity = distances.GetLength(0) - 1;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        var (opt, path) = HeldKarpAlgorithm(distances, startCity, endCity);
        watch.Stop();

        /*Debug.Log("Optimal cost: " + opt);
        Debug.Log("Optimal path: " + string.Join(", ", path));
        Debug.Log("Time taken: " + watch.Elapsed.TotalMilliseconds + " ms");
        */
        return new Tuple<int, List<int>>(opt, path);
    }

    static (int, List<int>) HeldKarpAlgorithm(int[,] distances, int startCity = 0, int endCity = 0)
    {
        int n = distances.GetLength(0);

        // Initialize the memoization table
        var memo = new Dictionary<(int, int), (int, int)>();
        // Initialize the base case
        for (int k = 0; k < n; k++)
        {
            if (k != startCity && k != endCity)
            {
                memo[(1 << k, k)] = ((int)distances[startCity, k], startCity);
            }
        }

        // Iterate over the subproblem size
        for (int subproblemSize = 2; subproblemSize < n; subproblemSize++)
        {
            var subsets = Combinations(Enumerable.Range(0, n).Where(i => i != startCity && i != endCity), subproblemSize);
            foreach (var subset in subsets)
            {
                int bits = 0;
                foreach (int bit in subset)
                {
                    bits |= 1 << bit;
                }
                foreach (int k in subset)
                {
                    if (k == startCity || k == endCity)
                    {
                        continue;
                    }
                    int prev = bits & ~(1 << k);
                    var res = new List<(int, int)>();
                    foreach (int m in subset)
                    {
                        if (m == startCity || m == k)
                        {
                            continue;
                        }
                        res.Add((memo[(prev, m)].Item1 + (int)distances[m, k], m));
                    }
                    memo[(bits, k)] = res.Min();
                }
            }
        }

        // Calculate the optimal cost
        int allBits = (1 << n) - 1 - (1 << startCity) - (1 << endCity);
        var results = new List<(int, int)>();
        for (int k = 0; k < n; k++)
        {
            if (k != startCity && k != endCity)
            {
                results.Add((memo[(allBits, k)].Item1 + (int)distances[k, endCity], k));
            }
        }
        var (opt, parent) = results.Min();

        // Reconstruct the optimal path
        var path = new List<int> { endCity };
        while (parent != startCity)
        {
            path.Add(parent);
            int newBits = allBits & ~(1 << parent);
            (int cost, int prev) = memo[(allBits, parent)];
            parent = prev;
            allBits = newBits;
        }
        path.Add(startCity);
        path.Reverse();

        return (opt, path);
    }

    static IEnumerable<IEnumerable<T>> Combinations<T>(IEnumerable<T> elements, int k)
    {
        return k == 0 ? new[] { new T[0] } :
            elements.SelectMany((e, i) =>
                Combinations(elements.Skip(i + 1), k - 1).Select(c => (new[] { e }).Concat(c)));
    }
}
