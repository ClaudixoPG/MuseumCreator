using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceOptimization{

    //create class to save data
    public class Data
    {
        public bool haveElitism;
        public string crossover;
        public string mutation;
        public string selection;
        public int validChromosomes;
        public int invalidChromosomes;
        public float time;
        public float bestFitness;
        public Genetic.Individual individual;
        //constructor of the class values
        public Data()
        {
            haveElitism = false;
            crossover = "";
            mutation = "";
            selection = "";
            validChromosomes = 0;
            invalidChromosomes = 0;
            time = 0;
            bestFitness = 0;
            individual = null;
        }
        public Data(bool haveElitism, string crossover, 
        string mutation, string selection, 
        int validChromosomes, int invalidChromosomes, 
        float time, float bestFitness, Genetic.Individual individual)
        {
            this.haveElitism = haveElitism;
            this.crossover = crossover;
            this.mutation = mutation;
            this.selection = selection;
            this.validChromosomes = validChromosomes;
            this.invalidChromosomes = invalidChromosomes;
            this.time = time;
            this.bestFitness = bestFitness;
            this.individual = individual;
        }
    }
}