using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PSO_TSP
{
    private int numCities = 10;
    private int numParticles = 10;
    private int numIterations = 100;
    private double maxVelocity = 0.5;
    private double w = 0.7;
    private double c1 = 1.5;
    private double c2 = 1.5;

    private int GetNumCities()
    {
        return numCities;
    }

    public PSO_TSP(int numCities, int numParticles, int numIterations, float maxVelocity, float inertia, float cognitive, float social)
    {
        this.numCities = numCities;
        this.numParticles = numParticles;
        this.numIterations = numIterations;
        this.maxVelocity = maxVelocity;
        this.w = inertia;
        this.c1 = cognitive;
        this.c2 = social;
    }
    class Particle
    {
        public int[] Position { get; set; }
        public double[] Velocity { get; set; }
        public int[] PBestPosition { get; set; }
        public double PBestValue { get; set; }
        public double Value { get; set; }

        public Particle(int numCities)
        {
            Position = Enumerable.Range(0, numCities).OrderBy(x => Guid.NewGuid()).ToArray();
            Velocity = new double[numCities];
            PBestPosition = (int[])Position.Clone();
            PBestValue = double.MaxValue;
        }

        public void Evaluate(int[,] distanceMatrix)
        {
            Value = 0;
            for (int i = 0; i < Position.Length - 1; i++)
            {
                Value += distanceMatrix[Position[i], Position[i + 1]];
            }
            Value += distanceMatrix[Position[Position.Length - 1], Position[0]];

            if (Value < PBestValue)
            {
                PBestValue = Value;
                PBestPosition = (int[])Position.Clone();
            }
        }
    }

    class Swarm
    {
        public List<Particle> Particles { get; set; }
        public double GBestValue { get; set; }
        public int[] GBestPosition { get; set; }

        public Swarm(int numParticles, int numCities)
        {
            Particles = new List<Particle>();
            GBestValue = double.MaxValue;
            GBestPosition = new int[numCities];

            for (int i = 0; i < numParticles; i++)
            {
                Particles.Add(new Particle(numCities));
            }
        }

        public void Evaluate(int[,] distanceMatrix)
        {
            foreach (var particle in Particles)
            {
                particle.Evaluate(distanceMatrix);
                if (particle.Value < GBestValue)
                {
                    GBestValue = particle.Value;
                    GBestPosition = (int[])particle.Position.Clone();
                }
            }
        }

        public void Update(double w, double c1, double c2, double maxVelocity, int numCities)
        {
            System.Random random = new System.Random();
            foreach (var particle in Particles)
            {
                for (int i = 0; i < particle.Position.Length; i++)
                {
                    double r1 = random.NextDouble();
                    double r2 = random.NextDouble();

                    particle.Velocity[i] = w * particle.Velocity[i] + c1 * r1 * (particle.PBestPosition[i] - particle.Position[i]) + c2 * r2 * (GBestPosition[i] - particle.Position[i]);

                    if (particle.Velocity[i] > maxVelocity)
                        particle.Velocity[i] = maxVelocity;
                    if (particle.Velocity[i] < -maxVelocity)
                        particle.Velocity[i] = -maxVelocity;
                }

                for (int i = 0; i < particle.Position.Length; i++)
                {
                    particle.Position[i] += (int)particle.Velocity[i];
                    if (particle.Position[i] < 0)
                        particle.Position[i] = 0;
                    if (particle.Position[i] >= numCities)
                        particle.Position[i] = numCities - 1;
                }
            }
        }
    }
    public Tuple<string,int,List<int>,double> Solver(int[,] distanceMatrix)
    {
        //distanceMatrix = DistanceMatrix(num_cities);

        /*Debug.Log("Distance Matrix");
        for (int i = 0; i < GetNumCities(); i++)
        {
            string row = "";
            for (int j = 0; j < GetNumCities(); j++)
            {
                row += distanceMatrix[i, j] + " ";
            }
            Debug.Log(row);
        }*/

        var time = System.DateTime.Now;

        Swarm swarm = new Swarm(numParticles, numCities);

        for (int i = 0; i < numIterations; i++)
        {
            swarm.Evaluate(distanceMatrix);
            swarm.Update(w, c1, c2, maxVelocity,numCities);
        }

        //Debug.Log("The minimum distance is: " + swarm.GBestValue);
        //Debug.Log("The best route is: " + string.Join(" -> ", swarm.GBestPosition));

        var time2 = System.DateTime.Now;

        var timeElapsed = (time2 - time).TotalMilliseconds;

        return Tuple.Create("PSO",(int)swarm.GBestValue, swarm.GBestPosition.ToList(),timeElapsed);
    }

}
