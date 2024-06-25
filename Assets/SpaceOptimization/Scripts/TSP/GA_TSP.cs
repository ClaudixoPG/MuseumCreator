using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GA_TSP
{
    public int n = 20;
    public int populationSize = 20;
    public float mutationProbability = 0.1f;
    public int maxGeneration = 100;

    private int[,] distance;
    private int[,] population;
    private System.Random random = new System.Random();

    public GA_TSP(int numCities, int numIndividuals, float mutationRate, int maxGen)
    {
        n = numCities;
        populationSize = numIndividuals;
        mutationProbability = mutationRate;
        maxGeneration = maxGen;
    }

    public Tuple<string, int, List<int>, double> Solver(int[,] distanceMatrix)
    {
        //distance = DistanceMatrix(n);
        distance = distanceMatrix;

        //print distance matrix
        /*for (int i = 0; i < n; i++)
        {
            string line = "";
            for (int j = 0; j < n; j++)
            {
                line += distance[i, j] + " ";
            }
            Debug.Log(line);
        }*/

        population = InitialPopulation(n, populationSize);
        int bestFitness = int.MaxValue;
        int[] bestIndividual = new int[n];

        //double timeStart = Time.realtimeSinceStartup;
        var time = System.DateTime.Now;

        for (int i = 0; i < maxGeneration; i++)
        {
            population = Selection(n, distance, population, populationSize);
            population = Crossover(n, population, populationSize);
            population = Mutation(n, population, populationSize, mutationProbability);

            for (int j = 0; j < populationSize; j++)
            {
                int[] individual = GetRow(population, j);
                int currentFitness = Fitness(n, distance, individual);
                if (currentFitness < bestFitness)
                {
                    bestFitness = currentFitness;
                    bestIndividual = (int[])individual.Clone();
                }
            }
        }

        var time2 = System.DateTime.Now;

        //double timeEnd = Time.realtimeSinceStartup;

        //Debug.Log("The best individual is: " + string.Join(",", bestIndividual));
        //Debug.Log("The fitness of the best individual is: " + bestFitness);
        //Debug.Log("The time cost is: " + (timeEnd - timeStart));

        var timeElapsed = (time2 - time).TotalMilliseconds;

        return Tuple.Create("GA", bestFitness, bestIndividual.ToList(), timeElapsed);

    }

    int[,] DistanceMatrix(int n)
    {
        int[,] distance = new int[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                distance[i, j] = distance[j, i] = random.Next(1, 101);
            }
        }
        return distance;
    }

    int[,] InitialPopulation(int n, int populationSize)
    {
        int[,] population = new int[populationSize, n];
        for (int i = 0; i < populationSize; i++)
        {
            List<int> perm = new List<int>(Enumerable.Range(0, n));
            for (int j = 0; j < n; j++)
            {
                int index = random.Next(perm.Count);
                population[i, j] = perm[index];
                perm.RemoveAt(index);
            }
        }
        return population;
    }

    int Fitness(int n, int[,] distance, int[] individual)
    {
        int length = 0;
        for (int i = 0; i < n - 1; i++)
        {
            length += distance[individual[i], individual[i + 1]];
        }
        length += distance[individual[n - 1], individual[0]];
        return length;
    }

    int[,] Selection(int n, int[,] distance, int[,] population, int populationSize)
    {
        int[] fitnessValues = new int[populationSize];
        for (int i = 0; i < populationSize; i++)
        {
            fitnessValues[i] = Fitness(n, distance, GetRow(population, i));
        }

        float[] selectionProbability = new float[populationSize];
        float sumFitness = 0;
        for (int i = 0; i < populationSize; i++)
        {
            sumFitness += fitnessValues[i];
        }
        for (int i = 0; i < populationSize; i++)
        {
            selectionProbability[i] = fitnessValues[i] / sumFitness;
        }

        float[] cumulativeProbability = new float[populationSize];
        cumulativeProbability[0] = selectionProbability[0];
        for (int i = 1; i < populationSize; i++)
        {
            cumulativeProbability[i] = cumulativeProbability[i - 1] + selectionProbability[i];
        }

        int[,] newPopulation = new int[populationSize, n];
        for (int i = 0; i < populationSize; i++)
        {
            float randomValue = (float)random.NextDouble();
            for (int j = 0; j < populationSize; j++)
            {
                if (randomValue <= cumulativeProbability[j])
                {
                    for (int k = 0; k < n; k++)
                    {
                        newPopulation[i, k] = population[j, k];
                    }
                    break;
                }
            }
        }
        return newPopulation;
    }

    int[,] Crossover(int n, int[,] population, int populationSize)
    {
        int[,] newPopulation = new int[populationSize, n];
        for (int i = 0; i < populationSize; i += 2)
        {
            int[] parent1 = GetRow(population, i);
            int[] parent2 = GetRow(population, i + 1);
            int[] child1 = new int[n];
            int[] child2 = new int[n];

            int crossoverPoint = random.Next(1, n);
            for (int j = 0; j < crossoverPoint; j++)
            {
                child1[j] = parent1[j];
                child2[j] = parent2[j];
            }

            int k1 = crossoverPoint, k2 = crossoverPoint;
            for (int j = 0; j < n; j++)
            {
                if (!Array.Exists(child1, element => element == parent2[j]))
                {
                    child1[k1++] = parent2[j];
                }
                if (!Array.Exists(child2, element => element == parent1[j]))
                {
                    child2[k2++] = parent1[j];
                }
            }

            SetRow(newPopulation, i, child1);
            SetRow(newPopulation, i + 1, child2);
        }
        return newPopulation;
    }

    int[,] Mutation(int n, int[,] population, int populationSize, float mutationProbability)
    {
        int[,] newPopulation = new int[populationSize, n];
        for (int i = 0; i < populationSize; i++)
        {
            int[] individual = GetRow(population, i);
            float randomValue = (float)random.NextDouble();
            if (randomValue < mutationProbability)
            {
                int mutationPoint1 = random.Next(n);
                int mutationPoint2 = random.Next(n);
                while (mutationPoint2 == mutationPoint1)
                {
                    mutationPoint2 = random.Next(n);
                }
                int temp = individual[mutationPoint1];
                individual[mutationPoint1] = individual[mutationPoint2];
                individual[mutationPoint2] = temp;
            }
            SetRow(newPopulation, i, individual);
        }
        return newPopulation;
    }

    int[] GetRow(int[,] matrix, int row)
    {
        int cols = matrix.GetLength(1);
        int[] result = new int[cols];
        for (int i = 0; i < cols; i++)
        {
            result[i] = matrix[row, i];
        }
        return result;
    }

    void SetRow(int[,] matrix, int row, int[] rowData)
    {
        int cols = matrix.GetLength(1);
        for (int i = 0; i < cols; i++)
        {
            matrix[row, i] = rowData[i];
        }
    }
}
