using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SpaceOptimization{

public class FloorSolver : Genetic
{
    [Header("Map Parameters")]
    public Map map;
    List<Individual> population = new List<Individual>();
    List<Individual> newPopulation = new List<Individual>();

    [Header("Best Individual Parameters")]
    Individual bestIndividual;
    float bestFitness = 0;
    int bestGeneration = 0;
    int bestIndividualOfgeneration = 0;

    [Header("Generation Parameters")]
    int populationSize = 10;
    int actualGeneration = 0;
    int maxGenerations = 10;
    int thresholdBetweenGenerations = 10;
    int tournamentSize = 5;
    int elitismSize = 10;
    float mutationRate = 0.01f;
    float crossoverRate = 0.7f;
    bool useElitism;
    private CrossoverType crossoverType;
    private MutationType mutationType;
    private SelectionType selectionType;

    [Header("Analyzer")]
    public Analyzer analyzer;
    private int legalChildrenCount = 0;
    private int illegalChildrenCount = 0;
    private System.Diagnostics.Stopwatch timer;

//crossover alto, mutacion baja
// sacar tabla comparativa de algotritmos con diferentes aproximaciones de crossover y mutacion y seleccion(implementar selecciones)
// comparar tiempo de ejecucion y fitness
    public override Data GeneticAlgorithm(int populationSize,
    int maxGenerations,
    bool useElitism, 
    CrossoverType crossoverType,
    MutationType mutationType, 
    SelectionType selectionType, 
    float crossoverRate = 0.9f, float mutationRate = 0.01f, int tournamentSize = 5, int elitismSize = 2 ){
        this.populationSize = populationSize;
        this.maxGenerations = maxGenerations;
        this.crossoverRate = crossoverRate;
        this.mutationRate = mutationRate;
        this.crossoverType = crossoverType;
        this.mutationType = mutationType;
        this.selectionType = selectionType;
        this.tournamentSize = tournamentSize;
        this.elitismSize = elitismSize;
        this.useElitism = useElitism;
        
        RestartData();

        //Start Genetic Algorithm

        //Initialize Population
        InitializePopulation();
        
        timer = System.Diagnostics.Stopwatch.StartNew();

        //repeat until better fitness wasn´t better and difference between actual generation and better generation was less than 10
        do
        {
            EvaluatePopulation(population);

            if(useElitism && population.Count != 0) newPopulation.AddRange(ElitismSelection(population,elitismSize));

            List<Individual> parents = new List<Individual>();
            while(newPopulation.Count < populationSize){

                List<Individual> copyPopulation = population.ToList();
                parents.Clear();
                parents.AddRange(Selection(copyPopulation));
                var children = Crossover(parents[0].GetCromosome(),parents[1].GetCromosome());
                newPopulation.AddRange(children);
            }

            for(int i = 0; i < newPopulation.Count; i++){
                Mutation(newPopulation[i].GetCromosome());
            }

            //Replace population with new population
            population.Clear();
            population.AddRange(newPopulation);
            newPopulation.Clear();
            
            //Check if best fitness is better than before, if it is, set best fitness and best generation
            actualGeneration++;
        }while(actualGeneration < maxGenerations);//  || actualGeneration - bestGeneration < thresholdBetweenGenerations || bestFitness < 100);

        EvaluatePopulation(population);

        for(int i = 0; i < population.Count; i++)
        {
            Debug.Log("Element " + i + "; is legal?: " + analyzer.CheckIfIndividualIsLegal(population[i].GetCromosome()) + "; fitness: " + population[i].GetFitness() );
        }

        //CAREFUL
        foreach(var individual in population){
            //Check if individual is legal
            if(!analyzer.CheckIfIndividualIsLegal(individual.GetCromosome())) continue;
            //Check if individual is better than the best individual
            if(individual.GetFitness() > bestFitness)
            {
                //bestFitness = CalculateFitness(individual.GetCromosome());
                bestFitness = individual.GetFitness();
                bestIndividual = individual;
                Debug.Log("Change candidate by: " + bestIndividual + " with fitness : " + bestFitness);
            }
        }

        Debug.Log("Legal: " + legalChildrenCount + "; Illegal: " + illegalChildrenCount);
        
        //Debug.Log("is Legal?: " + analyzer.CheckIfIndividualIsLegal(bestIndividual.GetCromosome()) +  " Fitness: " + bestIndividual.GetFitness() + "Infactibles: " + infact );

        //Restart data
        var time = timer.ElapsedMilliseconds;

        var dataSave = new Data(useElitism, crossoverType.ToString(), 
        mutationType.ToString(), selectionType.ToString(),
        legalChildrenCount, illegalChildrenCount,
        time,bestFitness,bestIndividual);

        return dataSave;
    }

    //Initialize Population
    public override void InitializePopulation(){
        for(int i = 0; i < populationSize; i++){
            var individual = new Individual(map.GenerateMap());
            population.Add(individual);
        }
        bestIndividual = population[0];
        //PrintPopulation();
    }

    //Restart Data
    public void RestartData(){
        actualGeneration = 0;
        bestGeneration = 0;
        bestIndividualOfgeneration = 0;
        bestFitness = 0;
        legalChildrenCount = 0;
        illegalChildrenCount = 0;
        bestIndividual = null;
        population.Clear();
        newPopulation.Clear();
    }

    //https://www.tutorialspoint.com/genetic_algorithms/genetic_algorithms_parent_selection.htm
    //Selection
    public override List<Individual> Selection(List<Individual> population){
        //select selection type
        switch(selectionType){
            case SelectionType.Roulette:
                return RouletteSelection(population);
            case SelectionType.Tournament:
                return TournamentSelection(population,tournamentSize);
            case SelectionType.Rank:
                return RankSelection(population);
        }
        //if no selection type was selected, return roulette selection
        return RouletteSelection(population);
    }
    //roulette selection
    private List<Individual> RouletteSelection(List<Individual> population){

        List<Individual> parents = new List<Individual>();
        //sum of all fitness
        float sumFitness = 0;
        //calculate sum of all fitness
        for(int i = 0; i < population.Count; i++){
            sumFitness += population[i].GetFitness();
        }
        //calculate probability of each individual
        for(int i = 0; i < population.Count; i++){
            population[i].SetProbability(population[i].GetFitness()/sumFitness);
        }
        //calculate cumulative probability of each individual
        for(int i = 0; i < population.Count; i++){
            if(i == 0) population[i].SetAccumulatedProbability(0);
            else population[i].SetAccumulatedProbability(population[i].GetProbability() + population[i-1].GetAccumulatedProbability());
        }

        //select parents
        foreach(var individual in population){
            
            //check if parents list is full
            if(parents.Count == 2) break;
            //generate random number between 0 and 1
            float random = Random.Range(0f,1f);
            //select parent
            for(int i = 0; i < population.Count; i++)
            {
                //if random number is less than accumulated probability, select individual as parent
                if(population[i].GetAccumulatedProbability() > random && !parents.Contains(population[i])){
                    parents.Add(population[i]);
                    break;
                }
                else if(i == population.Count - 1){
                    parents.Add(population[i]);
                    break;
                }
            }
        }

        return parents;
    }
    //tournament selection
    private List<Individual> TournamentSelection(List<Individual> population,int tournamentSize){
        //parent list should be the same size as the population
        List<Individual> parents = new List<Individual>();
        //select parents until the parent list is the same size as the population
        while(parents.Count < 2){
            //select random individuals from the population
            List<Individual> tournament = new List<Individual>();
            for(int i = 0; i < tournamentSize; i++){
                var random = Random.Range(0,population.Count);
                tournament.Add(population[random]);
            }
            //select the individual with the best fitness
            var bestIndividual = tournament[0];
            for(int i = 1; i < tournament.Count; i++){
                if(tournament[i].GetFitness() > bestIndividual.GetFitness()){
                    bestIndividual = tournament[i];
                }
            }
            parents.Add(bestIndividual);
        }

        return parents;
    }
    //rank selection
    private List<Individual> RankSelection(List<Individual> population){
        //parent list should be the same size as the population
        List<Individual> parents = new List<Individual>();
        //sort population by fitness (best to worst)
        population.Sort((x,y) => x.GetFitness().CompareTo(y.GetFitness()));
        //calculate probability of each individual
        var sum = (population.Count * (population.Count + 1))/2;
        for(int i = 0; i < population.Count; i++){
            //population[i].SetProbability((i+1)/(float)population.Count);
            population[i].SetProbability(population.Count - i); // 
        }

        //calculate cumulative probability of each individual
        for(int i = 0; i < population.Count; i++){
            if(i == 0) population[i].SetAccumulatedProbability(population[i].GetProbability());
            else population[i].SetAccumulatedProbability(population[i].GetProbability() + population[i-1].GetAccumulatedProbability());
        }
        //select parents until the parent list is the same size as the population
        while(parents.Count < 2){
            //generate random number between 0 and 1
            var random = Random.Range(0f,sum);
            //select the individual that has the accumulated probability greater than the random number
            for(int i = 0; i < population.Count; i++){
                if(population[i].GetAccumulatedProbability() > random){
                    if(!parents.Contains(population[i]))
                        parents.Add(population[i]);
                    break;
                }
            }
        }
        return parents;
    }

    //elitism selection
    private List<Individual> ElitismSelection(List<Individual> population, int elitismSize){
        //parent list should be the same size as the population
        List<Individual> parents = new List<Individual>();
        //sort population by fitness
        population.Sort((x,y) => x.GetFitness().CompareTo(y.GetFitness()));
        //select the best individuals
        for(int i = 0; i < elitismSize; i++){
            parents.Add(population[i]);
        }
        return parents;
    }

    //https://www.geeksforgeeks.org/crossover-in-genetic-algorithm/
    //Crossover
    public override List<Individual> Crossover(Vector3Int[] parent1, Vector3Int[] parent2)
    {
        //select crossover type
        switch(crossoverType){
            case CrossoverType.SinglePoint:
                return SinglePointCrossover(parent1,parent2);
            case CrossoverType.TwoPoint:
                return TwoPointCrossover(parent1,parent2);
            case CrossoverType.Uniform:
                return UniformCrossover(parent1,parent2);
        }
        //if no crossover type was selected, return single point crossover
        return SinglePointCrossover(parent1,parent2);
    }
    private List<Individual> SinglePointCrossover(Vector3Int[] parent1, Vector3Int[] parent2)
    {
        List<Individual> children = new List<Individual>();
        var randomIndex = Random.Range(0,parent1.Length);

        //cut the genes of the parents in the random index, the mix them to create the children
        var child1Gen = new List<Vector3Int>();
        child1Gen.AddRange(new List<Vector3Int>(parent1).GetRange(0,randomIndex).ToArray());
        child1Gen.AddRange(new List<Vector3Int>(parent2).GetRange(randomIndex,parent1.Length - randomIndex).ToArray());
        var child1 =  new Individual(child1Gen.ToArray());

        var child2Gen = new List<Vector3Int>();
        child2Gen.AddRange(new List<Vector3Int>(parent2).GetRange(0,randomIndex).ToArray());
        child2Gen.AddRange(new List<Vector3Int>(parent1).GetRange(randomIndex, parent1.Length - randomIndex).ToArray());
        var child2 =  new Individual(child2Gen.ToArray());

        children.Add(child1);
        children.Add(child2);

        return children;
    }
    private List<Individual> TwoPointCrossover(Vector3Int[] parent1, Vector3Int[] parent2)
    {
        var randomIndex1 = 0;
        var randomIndex2 = 0;

        List<Individual> children = new List<Individual>();
        do
        {
            randomIndex1 = Random.Range(0,population[0].GetCromosome().Length);
            randomIndex2 = Random.Range(randomIndex1,population[0].GetCromosome().Length);
        }while(randomIndex1 == randomIndex2);

        //cut the genes of the parents in the random index, the mix them to create the children
        var child1Gen = new List<Vector3Int>();
        child1Gen.AddRange(new List<Vector3Int>(parent1).GetRange(0,randomIndex1).ToArray());
        child1Gen.AddRange(new List<Vector3Int>(parent2).GetRange(randomIndex1,randomIndex2 - randomIndex1).ToArray());
        child1Gen.AddRange(new List<Vector3Int>(parent1).GetRange(randomIndex2,parent1.Length - randomIndex2).ToArray());
        var child1 =  new Individual(child1Gen.ToArray());

        var child2Gen = new List<Vector3Int>();
        child2Gen.AddRange(new List<Vector3Int>(parent2).GetRange(0,randomIndex1).ToArray());
        child2Gen.AddRange(new List<Vector3Int>(parent1).GetRange(randomIndex1, randomIndex2 - randomIndex1).ToArray());
        child2Gen.AddRange(new List<Vector3Int>(parent2).GetRange(randomIndex2, parent1.Length - randomIndex2).ToArray());
        var child2 =  new Individual(child2Gen.ToArray());

        children.Add(child1);
        children.Add(child2);

        return children;
    }
    private List<Individual> UniformCrossover(Vector3Int[] parent1, Vector3Int[] parent2)
    {
        List<Individual> children = new List<Individual>();

        //cut the genes of the parents in the random index, the mix them to create the children
        var child1Gen = new List<Vector3Int>();
        var child2Gen = new List<Vector3Int>();
        for(int i = 0; i < parent1.Length; i++){
            if(Random.Range(0,2) == 0){
                child1Gen.Add(parent1[i]);
                child2Gen.Add(parent2[i]);
            }else{
                child1Gen.Add(parent2[i]);
                child2Gen.Add(parent1[i]);
            }
        }
        var child1 =  new Individual(child1Gen.ToArray());
        var child2 =  new Individual(child2Gen.ToArray());

        children.Add(child1);
        children.Add(child2);

        return children;

    }

    // https://www.tutorialspoint.com/genetic_algorithms/genetic_algorithms_mutation.htm
    //Mutation
    public override void Mutation(Vector3Int[] cromosome)
    {
        //select mutation type
        switch(mutationType){
            case MutationType.RandomResetting:
                BitFlip(cromosome);
                break;
            case MutationType.Swap:
                Swap(cromosome);
                break;
            case MutationType.Scramble:
                Scramble(cromosome);
                break;
            case MutationType.Inversion:
                Inversion(cromosome);
                break;
        }
    }
    private void BitFlip(Vector3Int[] cromosome)
    {
        //select random index and random value to replace
        var randomIndex = Random.Range(0,cromosome.Length);

        //bit flip
        if(cromosome[randomIndex].x == 0) cromosome[randomIndex].x = 1;
        else cromosome[randomIndex].x = 0;
    }
    private void Swap(Vector3Int[] cromosome)
    {
        //select random index and random value to replace
        var randomIndex1 = 0;
        var randomIndex2 = 0;
        do
        {
            randomIndex1 = Random.Range(0,cromosome.Length);
            randomIndex2 = Random.Range(0,cromosome.Length);
        }while(randomIndex1 == randomIndex2);

        //Swap values

        var aux = cromosome[randomIndex1];
        cromosome[randomIndex1] = cromosome[randomIndex2];
        cromosome[randomIndex2] = aux;
    }
    private void Scramble(Vector3Int[] cromosome)
    {
        //select random index and random value to replace
        var randomIndex1 = 0;
        var randomIndex2 = 0;
        do
        {
            randomIndex1 = Random.Range(0,cromosome.Length);
            randomIndex2 = Random.Range(randomIndex1,cromosome.Length);
        }while(randomIndex1 == randomIndex2);

        //create a sublist with the values between the random indexes
        var subList = new List<Vector3Int>();
        subList.AddRange(new List<Vector3Int>(cromosome).GetRange(randomIndex1,randomIndex2 - randomIndex1).ToArray());
        //shuffle the sublist
        subList = subList.OrderBy(x => Random.value).ToList();

        //replace the values in the cromosome
        for(int i = randomIndex1; i < randomIndex2; i++){
            cromosome[i] = subList[i - randomIndex1];
        }
    }
    private void Inversion(Vector3Int[] cromosome)
    {
        //select random index and random value to replace
        var randomIndex1 = 0;
        var randomIndex2 = 0;
        do
        {
            randomIndex1 = Random.Range(0,cromosome.Length);
            randomIndex2 = Random.Range(randomIndex1,cromosome.Length);
        }while(randomIndex1 == randomIndex2);

        //create a sublist with the values between the random indexes
        var subList = new List<Vector3Int>();
        subList.AddRange(new List<Vector3Int>(cromosome).GetRange(randomIndex1,randomIndex2 - randomIndex1).ToArray());
        //reverse the sublist
        subList.Reverse();

        //replace the values in the cromosome
        for(int i = randomIndex1; i < randomIndex2; i++){
            cromosome[i] = subList[i - randomIndex1];
        }
    }
    





    //DEMAS
    public override float CalculateFitness(Vector3Int[] genes)
    {
        //Calculate fitness based on the map
        return analyzer.CalculateEfficiency(genes);
    }
    public override void EvaluatePopulation(List<Individual> population)
    {
        //Calculate Fitness, set illegal children fitness to 0
        for(int i = 0; i < population.Count; i++){
            if(!analyzer.CheckIfIndividualIsLegal(population[i].GetCromosome())){
                //population[i].SetFitness(0);
                var penalty = bestFitness;
                population[i].SetFitness(CalculateFitness(population[i].GetCromosome()) - penalty);
                illegalChildrenCount++;
            }else{
                population[i].SetFitness(CalculateFitness(population[i].GetCromosome()));
                legalChildrenCount++;
                
                if(CalculateFitness(population[i].GetCromosome()) > bestFitness){
                    bestFitness = CalculateFitness(population[i].GetCromosome());
                    //bestIndividual = population[i];
                    //bestIndividualOfgeneration = actualGeneration;
                }
            }
        }
    }
}


}

