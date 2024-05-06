
#Implement Particle Swarm Optimization to Solve TSP problem

import numpy as np
import random
import math
import copy
import matplotlib.pyplot as plt

# Define the number of cities
num_cities = 10

# Define the number of particles
num_particles = 10

# Define the number of iterations
num_iterations = 100

# Define the maximum velocity
max_velocity = 0.5

# Define the inertia weight
w = 0.7

# Define the cognitive weight
c1 = 1.5

# Define the social weight
c2 = 1.5

# Define the distance matrix
distance_matrix = np.array([[0, 29, 20, 21, 16, 31, 100, 12, 4, 31],
                             [29, 0, 15, 29, 28, 40, 72, 21, 29, 41],
                             [20, 15, 0, 15, 14, 25, 81, 9, 23, 27],
                             [21, 29, 15, 0, 4, 12, 92, 12, 25, 13],
                             [16, 28, 14, 4, 0, 16, 94, 9, 20, 16],
                             [31, 40, 25, 12, 16, 0, 95, 24, 36, 3],
                             [100, 72, 81, 92, 94, 95, 0, 90, 101, 99],
                             [12, 21, 9, 12, 9, 24, 90, 0, 15, 25],
                             [4, 29, 23, 25, 20, 36, 101, 15, 0, 35],
                             [31, 41, 27, 13, 16, 3, 99, 25, 35, 0]])

# Define the class for the particle
class Particle:
    def __init__(self, num_cities):
        self.num_cities = num_cities
        self.position = np.random.permutation(num_cities)
        self.velocity = np.random.rand(num_cities)
        self.pbest_position = self.position
        self.pbest_value = float('inf')

    def evaluate(self, distance_matrix):
        self.value = 0
        for i in range(self.num_cities - 1):
            self.value += distance_matrix[self.position[i]][self.position[i + 1]]
        self.value += distance_matrix[self.position[self.num_cities - 1]][self.position[0]]

        if self.value < self.pbest_value:
            self.pbest_value = self.value
            self.pbest_position = copy.copy(self.position)

# Define the class for the swarm
class Swarm:
    def __init__(self, num_particles, num_cities):
        self.num_particles = num_particles
        self.num_cities = num_cities
        self.particles = []
        self.gbest_value = float('inf')
        self.gbest_position = np.random.permutation(num_cities)

    def evaluate(self, distance_matrix):
        for particle in self.particles:
            particle.evaluate(distance_matrix)
            if particle.value < self.gbest_value:
                self.gbest_value = particle.value
                self.gbest_position = copy.copy(particle.position)

    def update(self, w, c1, c2, max_velocity):
        for particle in self.particles:
            for i in range(self.num_cities):
                r1 = random.random()
                r2 = random.random()

                particle.velocity[i] = w * particle.velocity[i] + c1 * r1 * (particle.pbest_position[i] - particle.position[i]) + c2 * r2 * (self.gbest_position[i] - particle.position[i])

                if particle.velocity[i] > max_velocity:
                    particle.velocity[i] = max_velocity
                if particle.velocity[i] < -max_velocity:
                    particle.velocity[i] = -max_velocity

            for i in range(self.num_cities):
                particle.position[i] += int(particle.velocity[i])
                if particle.position[i] < 0:
                    particle.position[i] = 0
                if particle.position[i] >= self.num_cities:
                    particle.position[i] = self.num_cities - 1

# Define the main function
def main():
    swarm = Swarm(num_particles, num_cities)
    for i in range(num_particles):
        swarm.particles.append(Particle(num_cities))

    for i in range(num_iterations):
        swarm.evaluate(distance_matrix)
        swarm.update(w, c1, c2, max_velocity)

    print("The minimum distance is: ", swarm.gbest_value)
    print("The best route is: ", swarm.gbest_position)

    plt.plot(swarm.gbest_position)
    plt.show()

if __name__ == "__main__":
    main()