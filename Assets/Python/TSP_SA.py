
#Implement Simulated Annealing to Solve TSP problem

import numpy as np
import random
import math
import copy
import matplotlib.pyplot as plt

# Define the distance matrix
def distance_matrix(n):
    # Create a random distance matrix
    np.random.seed(0)
    matrix = np.random.rand(n, n)
    matrix = (matrix + matrix.T) / 2
    np.fill_diagonal(matrix, 0)
    return matrix

# Define the total distance of the path
def total_distance(matrix, path):
    distance = 0
    for i in range(len(path) - 1):
        distance += matrix[path[i]][path[i + 1]]
    distance += matrix[path[-1]][path[0]]
    return distance

# Define the initial path
def initial_path(n):
    path = list(range(n))
    random.shuffle(path)
    return path

# Define the acceptance probability
def acceptance_probability(old_distance, new_distance, temperature):
    if new_distance < old_distance:
        return 1
    else:
        return math.exp((old_distance - new_distance) / temperature)
    
# Define the simulated annealing algorithm
def simulated_annealing(matrix, max_iteration, initial_temperature, cooling_rate):
    n = len(matrix)
    path = initial_path(n)
    old_distance = total_distance(matrix, path)
    best_path = path
    best_distance = old_distance
    temperature = initial_temperature
    for i in range(max_iteration):
        new_path = copy.copy(path)
        # Generate two random indices
        index1 = random.randint(0, n - 1)
        index2 = random.randint(0, n - 1)
        # Swap the cities at the two indices
        new_path[index1], new_path[index2] = new_path[index2], new_path[index1]
        new_distance = total_distance(matrix, new_path)
        if acceptance_probability(old_distance, new_distance, temperature) > random.random():
            path = new_path
            old_distance = new_distance
        if old_distance < best_distance:
            best_path = path
            best_distance = old_distance
        temperature *= 1 - cooling_rate
    return best_path, best_distance

# Define the main function
def main():
    n = 10
    max_iteration = 10000
    initial_temperature = 1.0
    cooling_rate = 0.001
    matrix = distance_matrix(n)
    best_path, best_distance = simulated_annealing(matrix, max_iteration, initial_temperature, cooling_rate)
    print("Best path:", best_path)
    print("Best distance:", best_distance)
    plt.imshow(matrix, cmap='hot', interpolation='nearest')
    plt.show()

if __name__ == "__main__":
    main()