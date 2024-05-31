
#Implement Genetic Algorithms to Solve TSP problem

import numpy as np
import random
import matplotlib.pyplot as plt
import time

# Define the distance matrix
def distance_matrix(n):
    distance = np.zeros((n,n))
    for i in range(n):
        for j in range(i+1,n):
            distance[i][j] = distance[j][i] = random.randint(1,100)
    return distance

# Define the initial population, use permutation to generate the initial population
def initial_population(n,population_size):
    population = np.zeros((population_size,n),dtype=int)
    for i in range(population_size):
        population[i] = np.random.permutation(n)
    return population

# Define the fitness function, the fitness is the total length of the tour
def fitness(n,distance,individual):
    length = 0.0
    for i in range(n-1):
        length += distance[individual[i]][individual[i+1]]
    length += distance[individual[n-1]][individual[0]]
    return length

# Define the selection function, use roulette wheel selection to select the new population
def selection(n,distance,population,population_size):
    fitness_values = np.zeros(population_size)
    for i in range(population_size):
        fitness_values[i] = fitness(n,distance,population[i])
    selection_probability = np.zeros(population_size)
    sum_fitness = np.sum(fitness_values)
    for i in range(population_size):
        selection_probability[i] = fitness_values[i]/sum_fitness
    cumulative_probability = np.zeros(population_size)
    cumulative_probability[0] = selection_probability[0]
    for i in range(1,population_size):
        cumulative_probability[i] = cumulative_probability[i-1] + selection_probability[i]
    new_population = np.zeros((population_size,n),dtype=int)
    for i in range(population_size):
        random_value = random.random()
        for j in range(population_size):
            if random_value <= cumulative_probability[j]:
                new_population[i] = population[j]
                break
    return new_population

# Define the crossover function, use one-point crossover to generate the new population
def crossover(n,population,population_size):
    new_population = np.zeros((population_size,n),dtype=int)
    for i in range(0,population_size,2):
        parent1 = population[i]
        parent2 = population[i+1]
        child1 = np.zeros(n,dtype=int)
        child2 = np.zeros(n,dtype=int)
        crossover_point = random.randint(1,n-1)
        for j in range(crossover_point):
            child1[j] = parent1[j]
            child2[j] = parent2[j]
        k1 = 0
        k2 = 0
        for j in range(n):
            if parent2[j] not in child1:
                child1[crossover_point+k1] = parent2[j]
                k1 += 1
            if parent1[j] not in child2:
                child2[crossover_point+k2] = parent1[j]
                k2 += 1
        new_population[i] = child1
        new_population[i+1] = child2
    return new_population

# Define the mutation function, use swap mutation to generate the new population
def mutation(n,population,population_size,mutation_probability):
    new_population = np.zeros((population_size,n),dtype=int)
    for i in range(population_size):
        individual = population[i]
        random_value = random.random()
        if random_value < mutation_probability:
            mutation_point1 = random.randint(0,n-1)
            mutation_point2 = random.randint(0,n-1)
            while mutation_point2 == mutation_point1:
                mutation_point2 = random.randint(0,n-1)
            individual[mutation_point1],individual[mutation_point2] = individual[mutation_point2],individual[mutation_point1]
        new_population[i] = individual
    return new_population

# Define the main function
def main():
    n = 20
    population_size = 20
    mutation_probability = 0.1
    max_generation = 100
    distance = distance_matrix(n)
    population = initial_population(n,population_size)
    best_fitness = float('inf')
    best_individual = np.zeros(n,dtype=int)
    time_start = time.time()
    for i in range(max_generation):
        population = selection(n,distance,population,population_size)
        population = crossover(n,population,population_size)
        population = mutation(n,population,population_size,mutation_probability)
        for individual in population:
            current_fitness = fitness(n,distance,individual)
            if current_fitness < best_fitness:
                best_fitness = current_fitness
                best_individual = individual
    time_end = time.time()
    print('The best individual is:',best_individual)
    print('The fitness of the best individual is:',best_fitness)
    print('The time cost is:',time_end-time_start)

# Execute the main function
if __name__ == '__main__':
    main()
