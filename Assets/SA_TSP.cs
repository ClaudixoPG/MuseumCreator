using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SA_TSP : MonoBehaviour
{
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

    //Define the total distance of the path
    public float totalDistance;
    private float TotalDistance(int[,] distanceMatrix, int[] path)
    {
        totalDistance = 0;
        for (int i = 0; i < path.Length - 1; i++)
        {
            totalDistance += distanceMatrix[path[i], path[i + 1]];
        }
        totalDistance += distanceMatrix[path[path.Length - 1], path[0]];
        return totalDistance;
    }

    //Define the initial path
    public List<int> initialPath;
    private List<int> InitialPath(int n)
    {
        initialPath = new List<int>();
        for (int i = 0; i < n; i++)
        {
            initialPath.Add(i);
        }
        //Shuffle the initial path
        for (int i = 0; i < n; i++)
        {
            int temp = initialPath[i];
            int randomIndex = UnityEngine.Random.Range(i, n);
            initialPath[i] = initialPath[randomIndex];
            initialPath[randomIndex] = temp;
        }

        return initialPath;
    }

    //Define the acceptance probability
    public float acceptanceProbability;
    private float AcceptanceProbability(float currentDistance, float newDistance, float temperature)
    {
        if (newDistance < currentDistance)
        {
            acceptanceProbability = 1.0f;
        }
        else
        {
            acceptanceProbability = Mathf.Exp((currentDistance - newDistance) / temperature);
        }
        return acceptanceProbability;
    }

    //Define the simulated annealing algorithm
    public List<int> simulatedAnnealingPath;

    private Tuple<List<int>, float> SimulatedAnnealing(int[,] distanceMatrix, int maxIteration, float initialTemperature, float coolingRate )
    {
        var n = distanceMatrix.GetLength(0);
        var currentPath = InitialPath(n);
        var currentDistance = TotalDistance(distanceMatrix, currentPath.ToArray());
        var bestPath = new List<int>(currentPath);
        var bestDistance = currentDistance;
        var temperature = initialTemperature;

        for (int i = 0; i < maxIteration; i++)
        {
            var newPath = new List<int>(currentPath);

            //Get two random indices
            int randomIndex1 = UnityEngine.Random.Range(0, n);
            int randomIndex2 = UnityEngine.Random.Range(0, n);

            //Swap the two indices
            int temp = newPath[randomIndex1];
            newPath[randomIndex1] = newPath[randomIndex2];
            newPath[randomIndex2] = temp;

            //Calculate the new distance
            var newDistance = TotalDistance(distanceMatrix, newPath.ToArray());

            if (AcceptanceProbability(currentDistance, newDistance, temperature) > UnityEngine.Random.Range(0.0f, 1.0f))
            {
                currentPath = new List<int>(newPath);
                currentDistance = newDistance;
            }
            if(currentDistance < bestDistance)
            {
                bestPath = new List<int>(currentPath);
                bestDistance = currentDistance;
            }
            temperature *= 1 - coolingRate;
        }
        return new Tuple<List<int>, float>(bestPath, bestDistance);
    }

    private void Start()
    {
        var n = 5;
        distanceMatrix = DistanceMatrix(n);
        //print distance matrix
        for (int i = 0; i < n; i++)
        {
            string row = "";
            for (int j = 0; j < n; j++)
            {
                row += distanceMatrix[i, j] + " ";
            }
            Debug.Log(row);
        }

        var maxIteration = 1000;
        var initialTemperature = 1.0f;
        var coolingRate = 0.001f;
        var result = SimulatedAnnealing(distanceMatrix, maxIteration, initialTemperature, coolingRate);
        simulatedAnnealingPath = result.Item1;
        totalDistance = result.Item2;
        Debug.Log("Simulated Annealing Path: " + string.Join("->", simulatedAnnealingPath.ToArray()) + " Total Distance: " + totalDistance);
    }

}
