using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticAlgorithm : MonoBehaviour
{
    public int populationSize = 50;
    public int generations = 100;
    public float mutationRate = 0.1f;
    public float cycleDuration = 2.0f;
    public JointController jointController;

    private List<Individual> population;

    void Start()
    {
        StartCoroutine(RunGeneticAlgorithm());
    }

    IEnumerator RunGeneticAlgorithm()
    {
        population = InitializePopulation(populationSize, 10);

        for (int generation = 0; generation < generations; generation++)
        {
            yield return StartCoroutine(EvaluatePopulation(population));
            Debug.Log($"Generation {generation + 1}, Best Fitness: {population[0].Fitness}");
            population = CreateNewGeneration(population);
        }
    }

    List<Individual> InitializePopulation(int size, int steps)
    {
        List<Individual> population = new List<Individual>();
        for (int i = 0; i < size; i++)
        {
            Individual individual = new Individual
            {
                HipAngles = new float[steps],
                KneeAngles = new float[steps],
                Fitness = 0
            };

            for (int j = 0; j < steps; j++)
            {
                individual.HipAngles[j] = Random.Range(-45f, 45f);
                individual.KneeAngles[j] = Random.Range(-90f, 0f);
            }

            population.Add(individual);
        }
        return population;
    }

    IEnumerator EvaluatePopulation(List<Individual> population)
    {
        foreach (var individual in population)
        {
            bool isComplete = false;

            yield return StartCoroutine(FitnessEvaluator.Evaluate(
                individual,
                jointController,
                cycleDuration,
                fitness =>
                {
                    individual.Fitness = fitness;
                    isComplete = true;
                }
            ));

            while (!isComplete)
                yield return null;
        }

        population.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));
    }

    List<Individual> CreateNewGeneration(List<Individual> population)
    {
        List<Individual> newPopulation = new List<Individual>();
        for (int i = 0; i < populationSize; i++)
        {
            Individual parent1 = SelectParent(population);
            Individual parent2 = SelectParent(population);
            Individual offspring = Crossover(parent1, parent2);
            Mutate(offspring);
            newPopulation.Add(offspring);
        }
        return newPopulation;
    }

    Individual SelectParent(List<Individual> population)
    {
        int index = Random.Range(0, populationSize / 2);
        return population[index];
    }

    Individual Crossover(Individual parent1, Individual parent2)
    {
        int steps = parent1.HipAngles.Length;
        Individual offspring = new Individual
        {
            HipAngles = new float[steps],
            KneeAngles = new float[steps]
        };

        for (int i = 0; i < steps; i++)
        {
            offspring.HipAngles[i] = Random.value < 0.5f ? parent1.HipAngles[i] : parent2.HipAngles[i];
            offspring.KneeAngles[i] = Random.value < 0.5f ? parent1.KneeAngles[i] : parent2.KneeAngles[i];
        }

        return offspring;
    }

    void Mutate(Individual individual)
    {
        for (int i = 0; i < individual.HipAngles.Length; i++)
        {
            if (Random.value < mutationRate)
            {
                individual.HipAngles[i] += Random.Range(-10f, 10f);
                individual.KneeAngles[i] += Random.Range(-10f, 10f);
            }
        }
    }
}
