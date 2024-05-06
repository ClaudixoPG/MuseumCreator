using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACO_TSP : MonoBehaviour
{
    //Use Ant Colony Optimization to solve the Traveling Salesman Problem

    //Parameters
    public int n = 5; //Number of cities
    public int m = 5; //Number of ants
    public float alpha = 1.0f; //Pheromone factor
    public float beta = 2.0f; //Distance factor

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
            for (int j = i+1; j < n; j++)
            {
                distanceMatrix[i, j] = distanceMatrix[j,i] = Random.Range(1, 10);
            }
        }
        return distanceMatrix;
    }

    //Define the pheromone matrix
    public float[,] pheromoneMatrix;
    private float[,] PheromoneMatrix(int n)
    {
        //Create a new pheromone matrix
        pheromoneMatrix = new float[n, n];

        //Initialize matrix with 0 values
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                pheromoneMatrix[i, j] = 0;
            }
        }

        //Fill the pheromone matrix with random values
        for (int i = 0; i < n; i++)
        {
            for (int j = i+1; j < n; j++)
            {
                pheromoneMatrix[i, j] = pheromoneMatrix[j, i] = 1.0f;//Random.Range(0.1f, 1.0f);
            }
        }
        return pheromoneMatrix;
    }

    //Define the probability matrix
    public float[,] probabilityMatrix;
    private float[,] ProbabilityMatrix(int n, int[,] distanceMatrix, float alpha, float beta)
    {
        //Create a new probability matrix
        probabilityMatrix = new float[n, n];

        //Initialize matrix with 0 values
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                probabilityMatrix[i, j] = 0;
            }
        }

        //Fill the probability matrix with values
        for (int i = 0; i < n; i++)
        {
            for (int j = i+1; j < n; j++)
            {
                if (distanceMatrix[i,j] == 0)
                {
                    probabilityMatrix[i, j] = probabilityMatrix[j, i] = 0;
                }
                else
                {
                    probabilityMatrix[i, j] = probabilityMatrix[j, i] = Mathf.Pow(pheromoneMatrix[i, j], alpha) * Mathf.Pow(1.0f / distanceMatrix[i, j], beta);
                }
            }
        }
        return probabilityMatrix;
    }

    //Define Ant Tour
    public List<int> antTour;
    private List<int> Ant_Tour(int n, float[,] probabilityMatrix)
    {
        antTour = new List<int>();
        bool[] visited = new bool[n];
        //Initialize visited array with false values
        for (int i = 0; i < n; i++)
        {
            visited[i] = false;
        }

        var Start = Random.Range(0, n);
        antTour.Add(Start);
        visited[Start] = true;

        for (int i = 0; i < n-1; i++)
        {
            //set currentCity to the last city in the antTour
            var currentCity = antTour[antTour.Count - 1];
            var nextCity = 0;
            var maxProbability = 0.0f;
            for (int j = 0; j <n; j++)
            {
                if (visited[j] == false && probabilityMatrix[currentCity, j] > maxProbability)
                {
                    nextCity = j;
                    maxProbability = probabilityMatrix[currentCity, j];
                }
            }
            antTour.Add(nextCity);
            visited[nextCity] = true;
        }
        return antTour;
    }

    //Define Tour Length
    private float TourLength(int n, List<int> antTour, int[,] distanceMatrix)
    {
        float tourLength = 0.0f;
        for (int i = 0; i < n-1; i++)
        {
            tourLength += distanceMatrix[antTour[i], antTour[i + 1]];
        }
        tourLength += distanceMatrix[antTour[n - 1], antTour[0]];
        return tourLength;
    }

    //Define Update Pheromone
    private float[,] UpdatePheromone(int n, float[,] pheromoneMatrix, int[,] distance,  List<List<int>> antList)
    {
        float[,] temporalPheromoneMatrix = new float[n, n];
        //Initialize matrix with 0 values
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                temporalPheromoneMatrix[i, j] = 0;
            }
        }

        for (int i = 0;i< n; i++)
        {
            for(int j = i+1;j < n; j++)
            {
                foreach (var ant in antList)
                {
                    var lenght = TourLength(n, ant, distance);
                    temporalPheromoneMatrix[i, j] += 1.0f / lenght;
                }
                temporalPheromoneMatrix[j, i] = temporalPheromoneMatrix[i, j];
            }
        }

        for (int i = 0; i < n; i++)
        {
            for (int j = i+1; j < n; j++)
            {
                pheromoneMatrix[i, j] = pheromoneMatrix[i, j] = 0.5f * pheromoneMatrix[i, j] + temporalPheromoneMatrix[i, j];
            }
        }

        return pheromoneMatrix;
    }

    private void Start()
    {
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

        pheromoneMatrix = PheromoneMatrix(n);
        List<int> bestTour = new List<int>();
        int bestTourLength = int.MaxValue;

        for (int i = 0; i < m; i++)
        {
            probabilityMatrix = ProbabilityMatrix(n, distanceMatrix, alpha, beta);
            var antList = new List<List<int>>();
            for (int j = 0; j < m; j++)
            {
                antList.Add(Ant_Tour(n, probabilityMatrix));
            }
            foreach (var ant in antList)
            {
                var tourLength = TourLength(n, ant, distanceMatrix);
                if (tourLength < bestTourLength)
                {
                    bestTourLength = (int)tourLength;
                    bestTour = ant;
                }
            }
            pheromoneMatrix = UpdatePheromone(n, pheromoneMatrix, distanceMatrix, antList);
        }
        Debug.Log("Best Tour: " + string.Join(",", bestTour.ToArray()));
        Debug.Log("Best Tour Length: " + bestTourLength);

    }
}
