using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceOptimization{

    public class Genetic : MonoBehaviour
    {
        public class Individual{
            private Vector3Int[] cromosome;
            private float fitness;
            private float probability;
            private float accumulatedProbability;
            private int rank;
            public Individual(Vector3Int[] cromosome){
                this.cromosome = cromosome;
                this.fitness = 0;
            }
            public Vector3Int[] GetCromosome(){
                return cromosome;
            }
            public float GetFitness(){
                return fitness;
            }
            public void SetFitness(float fitness){
                this.fitness = fitness;
            }
            public float GetProbability(){
                return probability;
            }
            public void SetProbability(float probability){
                this.probability = probability;
            }
            public float GetAccumulatedProbability(){
                return accumulatedProbability;
            }
            public void SetAccumulatedProbability(float accumulatedProbability){
                this.accumulatedProbability = accumulatedProbability;
            }
            public int GetRank(){
                return rank;
            }
            public void SetRank(int rank){
                this.rank = rank;
            }
        }
        public enum CrossoverType{
            SinglePoint,
            TwoPoint,
            Uniform
        }
        public enum MutationType{
            RandomResetting,
            Swap,
            Scramble,
            Inversion
        }
        public enum SelectionType{
            Roulette,
            Tournament,
            Rank
        }

        //step by step
        public virtual void InitializePopulation()
        {
            //Debug.Log("InitializePopulation");
            // Generar genes aleatorios para crear la población inicial
            // Puedes utilizar bucles y generación aleatoria para crear los genes
        }
        public virtual void EvaluatePopulation(List<Individual> population)
        {
            //Debug.Log("EvaluatePopulation");
            // Evaluar la aptitud de cada gen en la población
            // Puedes utilizar bucles para recorrer la población y calcular la aptitud de cada gen
        }
        public virtual float CalculateFitness(Vector3Int[] individual)
        {
            //Debug.Log("CalculateFitness");
            // Calcular la aptitud de un gen específico
            // Basado en algún criterio específico del problema
            // Retorna un valor numérico que representa la aptitud del gen
            return 0;
        }
        public virtual List<Individual> Selection(List<Individual> population)
        {
            //Debug.Log("Selection");
            // Implementar el operador de selección para seleccionar los padres que se reproducirán
            // Puedes utilizar diferentes métodos de selección, como selección por ruleta, torneo, etc.
            // Retorna los genes seleccionados como padres
            return null;
        }
        public virtual List<Individual> Crossover(Vector3Int[] parent1, Vector3Int[] parent2)
        {
            Debug.Log("Crossover");
            // Implementar el operador de cruce para combinar la información genética de los padres
            // Puedes utilizar diferentes métodos de cruce, como cruce en un punto, cruce uniforme, etc.
            // Retorna los genes hijos generados a partir de los padres
            return null;
        }
        public virtual void Mutation(Vector3Int[] cromosome)
        {
            Debug.Log("Mutation");
            // Implementar la función de mutación para introducir cambios aleatorios en un gen
            // Puedes seleccionar aleatoriamente genes específicos y aplicar alguna modificación
        }
        public virtual void ReplacePopulation(List<Individual> offspring)
        {
            Debug.Log("ReplacePopulation");
            // Implementar la estrategia de reemplazo para actualizar la población actual con los nuevos genes (hijos y mutados)
            // Puedes utilizar diferentes métodos de reemplazo, como reemplazo generacional o reemplazo elitista
        }
        //end conditions
        public virtual bool IsTerminationConditionMet(int generation, int maxGenerations)
        {
            Debug.Log("IsTerminationConditionMet");
            // Implementar el criterio de terminación para el algoritmo genético
            // Puedes utilizar diferentes criterios de terminación, como número máximo de generaciones, aptitud mínima, etc.
            return false;
        }
        public virtual bool IsFitnessMet(double fitness, double maxFitness)
        {
            Debug.Log("IsFitnessMet");
            // Implementar el criterio de terminación para el algoritmo genético
            // Puedes utilizar diferentes criterios de terminación, como número máximo de generaciones, aptitud mínima, etc.
            return false;
        }
        public virtual bool IsConvergenceMet(int[][] population)
        {
            Debug.Log("IsConvergenceMet");
            // Implementar el criterio de terminación para el algoritmo genético
            // Puedes utilizar diferentes criterios de terminación, como número máximo de generaciones, aptitud mínima, etc.
            return false;
        }
        //genetic algorithm
        public virtual Data GeneticAlgorithm(int populationSize,int maxGenerations, bool useElitism, CrossoverType crossoverType,MutationType mutationType, SelectionType selectionType, float crossoverRate, float mutationRate, int tournamentSize, int elitismSize)
        {
            Debug.Log("GeneticAlgorithm");

            for (int generation = 0; generation < maxGenerations; generation++)
            {
                // Calcular la aptitud para cada gen en la población

                // Selección de padres

                // Reproducción y cruce

                // Mutación

                // Reemplazo

                // Evaluar si se cumple algún criterio de terminación
            }
            return null;
        }
    }
}
