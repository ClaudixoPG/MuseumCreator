using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PSO_TSP : MonoBehaviour
{
    int num_cities = 10;
    int num_particles = 10;
    int num_iterations = 1000;

    float max_velocity = 1.0f;
    float w = 0.5f;
    float c1 = 0.8f;
    float c2 = 0.9f;


    //Define the distance matrix
    public int[,] distanceMatrix;
    private int[,] DistanceMatrix(int n)
    {
        //Create a new distance matrix
        distanceMatrix = new int[n, n];

        //Initialize matrix with 0 values
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                distanceMatrix[i, j] = 0;
            }
        }

        //Fill the distance matrix with random values
        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                distanceMatrix[i, j] = distanceMatrix[j, i] = UnityEngine.Random.Range(1, 10);
            }
        }
        return distanceMatrix;
    }

    public class Particle
    {
        public int numCities;
        public List<int> position;
        public List<double> velocity;
        public List<int> bestPosition;
        public float pBest = float.MaxValue;
        public float currentDistance;

        public Particle(int numCities)
        {
            this.numCities = numCities;
            var random = new System.Random();
            position = Enumerable.Range(0, numCities).OrderBy(x => random.Next()).ToList();
            velocity = new List<double>(numCities);
            for (int i = 0; i < numCities; i++)
            {
                velocity.Add(random.NextDouble());
            }
            bestPosition = new List<int>();
            currentDistance = 0;
        }
        public void Evaluate(int[,] distanceMatrix)
        {
            currentDistance = 0;
            for (int i = 0; i < numCities - 1; i++)
            {
                currentDistance += distanceMatrix[position[i], position[i + 1]];
            }
            currentDistance += distanceMatrix[position[numCities - 1], position[0]];
            
            if (currentDistance < pBest)
            {
                pBest = currentDistance;
                bestPosition = new List<int>(position);
                //position.CopyTo(bestPosition.ToArray());
            }
        }
    }

    public class Swarm
    {
        public int particleCount;
        public int numCities;
        public List<Particle> particles;
        public float gBest = float.MaxValue;
        public List<int> gBestPosition;

        public Swarm(int particleCount, int numCities)
        {
            this.particleCount = particleCount;
            this.numCities = numCities;
            particles = new List<Particle>();
            gBest = float.MaxValue;
            var random = new System.Random();
            gBestPosition = Enumerable.Range(0, numCities).OrderBy(x => random.Next()).ToList();
        }

        public void Evaluate(int[,] distanceMatrix)
        {
            foreach (Particle particle in particles)
            {
                particle.Evaluate(distanceMatrix);
                if (particle.pBest < gBest)
                {
                    gBest = particle.currentDistance;
                    particle.bestPosition = new List<int>(particle.position);
                }
            }
        }

        public void Update(float w, float c1, float c2, float maxVelocity)
        {
            foreach (Particle particle in particles)
            {
                for (int i = 0; i < numCities; i++)
                {
                    float r1 = UnityEngine.Random.Range(0.0f, 1.0f);
                    float r2 = UnityEngine.Random.Range(0.0f, 1.0f);
                    particle.velocity[i] = (w * particle.velocity[i] + c1 * r1 * (particle.bestPosition[i] - particle.position[i]) + c2 * r2 * (gBestPosition[i] - particle.position[i]));
                    
                    if (particle.velocity[i] > maxVelocity)
                    {
                        particle.velocity[i] = (int)maxVelocity;
                    }
                    else if (particle.velocity[i] < -maxVelocity)
                    {
                        particle.velocity[i] = (int)-maxVelocity;
                    }
                }

                for (int i = 0; i < numCities; i++)
                {
                    particle.position[i] += (int)(particle.velocity[i]);
                    if (particle.position[i] >= numCities)
                    {
                        particle.position[i] = numCities - 1;
                    }
                    else if (particle.position[i] < 0)
                    {
                        particle.position[i] = 0;
                    }
                }

            }
        }
    }

    private void Start()
    {
        distanceMatrix = DistanceMatrix(num_cities);

        print("Distance Matrix");
        for (int i = 0; i < num_cities; i++)
        {
            string row = "";
            for (int j = 0; j < num_cities; j++)
            {
                row += distanceMatrix[i, j] + " ";
            }
            print(row);
        }

        Swarm swarm = new Swarm(num_particles, num_cities);

        for(int i = 0; i < num_particles;i++)
        {
            swarm.particles.Add(new Particle(num_cities));
        }

        for (int i = 0; i < num_iterations; i++)
        {
            swarm.Evaluate(distanceMatrix);
            swarm.Update(w, c1, c2, max_velocity);
        }

        Debug.Log("Best distance: " + swarm.gBest);
        string bestPath = "";
        foreach (int city in swarm.gBestPosition)
        {
            bestPath += city + " ";
        }
        Debug.Log("Best path: " + bestPath);

    }

}
