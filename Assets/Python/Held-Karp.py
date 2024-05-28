"""import numpy as np
import itertools
import time

def held_karp(distances):
    n = len(distances)
    # Initialize the memoization table
    memo = {}
    # Initialize the base case
    for k in range(1, n):
        memo[(1 << k, k)] = (distances[0][k], 0)
    # Iterate over the subproblem size
    for subproblem_size in range(2, n):
        for subset in itertools.combinations(range(1, n), subproblem_size):
            bits = 0
            for bit in subset:
                bits |= 1 << bit
            for k in subset:
                prev = bits & ~(1 << k)
                res = []
                for m in subset:
                    if m == 0 or m == k:
                        continue
                    res.append((memo[(prev, m)][0] + distances[m][k], m))
                memo[(bits, k)] = min(res)
    bits = (2**n - 1) - 1
    res = []
    for k in range(1, n):
        res.append((memo[(bits, k)][0], k))
    opt, parent = min(res)
    path = []
    for i in range(n - 1):
        path.append(parent)
        new_bits = bits & ~(1 << parent)
        _, parent = memo[(bits, parent)]
        bits = new_bits
    path.append(0)
    path = path[::-1]
    return opt, path

def main():
    distances = np.array([[0, 29, 20, 21],
                           [29, 0, 15, 26],
                           [20, 15, 0, 16],
                           [21, 26, 16, 0]])
    start = time.time()
    opt, path = held_karp(distances)
    end = time.time()
    print('Optimal cost:', opt)
    print('Optimal path:', path)
    print('Time taken:', end - start)

if __name__ == '__main__':
    main()"""

import numpy as np
import itertools
import time

def held_karp(distances, start_city=0, end_city=None):
    n = len(distances)
    if end_city is None:
        end_city = start_city

    # Initialize the memoization table
    memo = {}
    # Initialize the base case
    for k in range(n):
        if k != start_city and k != end_city:
            memo[(1 << k, k)] = (distances[start_city][k], start_city)

    # Iterate over the subproblem size
    for subproblem_size in range(2, n):
        for subset in itertools.combinations(range(n), subproblem_size):
            if start_city in subset or end_city in subset:
                continue
            bits = 0
            for bit in subset:
                bits |= 1 << bit
            for k in subset:
                if k == start_city or k == end_city:
                    continue
                prev = bits & ~(1 << k)
                res = []
                for m in subset:
                    if m == start_city or m == k:
                        continue
                    res.append((memo[(prev, m)][0] + distances[m][k], m))
                memo[(bits, k)] = min(res)

    # Calculate the optimal cost
    bits = (1 << n) - 1 - (1 << start_city) - (1 << end_city)
    res = []
    for k in range(n):
        if k != start_city and k != end_city:
            res.append((memo[(bits, k)][0] + distances[k][end_city], k))
    opt, parent = min(res)

    # Reconstruct the optimal path
    path = [end_city]
    while parent != start_city:
        path.append(parent)
        new_bits = bits & ~(1 << parent)
        _, parent = memo[(bits, parent)]
        bits = new_bits
    path.append(start_city)
    path = path[::-1]

    return opt, path

def main():
    distances = np.array([[0, 29, 20, 21, 17, 30, 10, 15],
                      [29, 0, 15, 26, 12, 24, 18, 25],
                      [20, 15, 0, 16, 28, 13, 22, 27],
                      [21, 26, 16, 0, 18, 23, 30, 19],
                      [17, 12, 28, 18, 0, 22, 25, 14],
                      [30, 24, 13, 23, 22, 0, 16, 20],
                      [10, 18, 22, 30, 25, 16, 0, 15],
                      [15, 25, 27, 19, 14, 20, 15, 0]])

    start_city = 0  # Change as needed
    end_city = 7    # Change as needed
    start = time.time()
    opt, path = held_karp(distances, start_city, end_city)
    end = time.time()
    print('Optimal cost:', opt)
    print('Optimal path:', path)
    print('Time taken:', end - start)

if __name__ == '__main__':
    main()
