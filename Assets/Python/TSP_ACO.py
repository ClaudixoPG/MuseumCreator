
#Implement Ant Colony Optimization to Solve TSP problem

import numpy as np
import random
import math
import copy
import matplotlib.pyplot as plt

# Define the distance matrix
def distance_matrix(n):
    distance = np.zeros((n,n))
    for i in range(n):
        for j in range(i+1,n):
            distance[i][j] = distance[j][i] = random.randint(1,100)
    return distance

# Define the pheromone matrix
def pheromone_matrix(n):
    pheromone = np.zeros((n,n))
    for i in range(n):
        for j in range(i+1,n):
            pheromone[i][j] = pheromone[j][i] = 1.0
    return pheromone

# Define the probability matrix
def probability_matrix(n,distance,pheromone,alpha,beta):
    probability = np.zeros((n,n))
    for i in range(n):
        for j in range(i+1,n):
            if distance[i][j] == 0:
                probability[i][j] = probability[j][i] = 0.0
            else:
                probability[i][j] = probability[j][i] = math.pow(pheromone[i][j],alpha)*math.pow(1.0/distance[i][j],beta)
    return probability

# Define the ant tour
def ant_tour(n,probability):
    tour = []
    visited = np.zeros(n)
    start = random.randint(0,n-1)
    tour.append(start)
    visited[start] = 1
    for i in range(n-1):
        current = tour[-1]
        next = 0
        max_probability = 0.0
        for j in range(n):
            if visited[j] == 0 and probability[current][j] > max_probability:
                max_probability = probability[current][j]
                next = j
        tour.append(next)
        visited[next] = 1
    return tour

# Define the tour length
def tour_length(n,distance,tour):
    length = 0.0
    for i in range(n-1):
        length += distance[tour[i]][tour[i+1]]
    length += distance[tour[n-1]][tour[0]]
    return length

# Define the update pheromone matrix
def update_pheromone(n,pheromone,distance,ant_list):
    pheromone_temp = np.zeros((n,n))
    for i in range(n):
        for j in range(i+1,n):
            for ant in ant_list:
                length = tour_length(n,distance,ant)
                pheromone_temp[i][j] += 1.0/length
            pheromone_temp[j][i] = pheromone_temp[i][j]
    for i in range(n):
        for j in range(i+1,n):
            pheromone[i][j] = pheromone[j][i] = 0.5*pheromone[i][j] + 0.5*pheromone_temp[i][j]
    return pheromone

# Define the main function
def main():
    n = 20
    m = 20
    alpha = 1.0
    beta = 2.0
    distance = distance_matrix(n)
    #print the distance matrix to the console
    print(distance)

    pheromone = pheromone_matrix(n)
    best_tour = []
    best_length = float('inf')
    for i in range(m):
        probability = probability_matrix(n,distance,pheromone,alpha,beta)
        ant_list = []
        for j in range(m):
            ant_list.append(ant_tour(n,probability))
        for ant in ant_list:
            length = tour_length(n,distance,ant)
            if length < best_length:
                best_length = length
                best_tour = copy.deepcopy(ant)
        pheromone = update_pheromone(n,pheromone,distance,ant_list)
    print('The best tour is:',best_tour)
    print('The best length is:',best_length)

    plt.figure()
    x = []
    y = []
    for i in range(n):
        x.append(i)
        y.append(i)
    x.append(best_tour[0])
    y.append(best_tour[0])
    plt.plot(x,y)
    for i in range(n):
        plt.text(i,i,str(i))
    plt.show()

if __name__ == '__main__':
    main()

# End of TSP_ACO.py