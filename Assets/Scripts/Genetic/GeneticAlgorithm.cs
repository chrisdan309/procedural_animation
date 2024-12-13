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
    public int steps = 50;
    private List<Individual> population;
    public GameObject Robot;
    public int batchSize = 10;
    void Start()
    {
        StartCoroutine(RunGeneticAlgorithm());
    }

    IEnumerator RunGeneticAlgorithm()
    {
        
        population = InitializePopulation(populationSize, steps);

        for (int generation = 0; generation < generations; generation++)
        {
            Debug.Log($"Generation {generation + 1}:");

            // Evaluar la población en paralelo
            yield return StartCoroutine(FitnessEvaluator.EvaluatePopulationParallel(population, jointController, cycleDuration, Robot, batchSize));

            // Ordenar la población por fitness
            population.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));

            Debug.Log($"Generation {generation + 1}, Best Fitness: {population[0].Fitness}");

            // Crear nueva generación
            population = CreateNewGeneration(population);
        }

        Debug.Log("Genetic Algorithm Complete.");
    }

    List<Individual> InitializePopulation(int size, int steps)
    {
        List<Individual> population = new List<Individual>();

        // Obtén las rotaciones iniciales del HipRight y KneeRight en grados
        float initialHipRightAngle = HipRotationToDegrees(jointController.HipRight.localRotation);
        float initialKneeRightAngle = HipRotationToDegrees(jointController.KneeRight.localRotation);
        float initialHipLeftAngle = HipRotationToDegrees(jointController.HipLeft.localRotation);
        float initialKneeLeftAngle = HipRotationToDegrees(jointController.KneeLeft.localRotation);

        for (int i = 0; i < size; i++)
        {
            Individual individual = new Individual
            {
                HipAnglesRight = new float[steps],
                KneeAnglesRight = new float[steps],
                HipAnglesLeft = new float[steps],
                KneeAnglesLeft = new float[steps],
                Fitness = 0
            };

            for (int j = 0; j < steps; j++)
            {
                if (j == 0)
                {
                    // El primer ángulo es igual a las rotaciones iniciales del GameObject
                    individual.HipAnglesRight[j] = initialHipRightAngle;
                    individual.KneeAnglesRight[j] = initialKneeRightAngle;
                    individual.HipAnglesLeft[j] = initialHipLeftAngle;
                    individual.KneeAnglesLeft[j] = initialKneeLeftAngle;

                    
                }
                else
                {
                    // Generar valores aleatorios para el resto de los pasos
                    individual.HipAnglesRight[j] = Random.Range(-45f, 45f);
                    individual.KneeAnglesRight[j] = Random.Range(-90f, 0f);
                    individual.HipAnglesLeft[j] = Random.Range(-45f, 45f);;
                    individual.KneeAnglesLeft[j] = Random.Range(0f, 90f);
                }
            }

            population.Add(individual);
        }
        return population;
    }

    float HipRotationToDegrees(Quaternion localRotation)
    {
        return localRotation.eulerAngles.z;
    }

    
    // IEnumerator EvaluatePopulation(List<Individual> population)
    // {
    //     foreach (var individual in population)
    //     {
    //         bool isComplete = false;
    //
    //         yield return StartCoroutine(FitnessEvaluator.EvaluatePopulationParallel(
    //             individual,
    //             jointController,
    //             cycleDuration,
    //             Robot,
    //             fitness =>
    //             {
    //                 individual.Fitness = fitness;
    //                 isComplete = true;
    //             }
    //         ));
    //
    //         while (!isComplete)
    //             yield return null;
    //     }
    //
    //     population.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));
    // }

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
        int steps = parent1.HipAnglesRight.Length; // Todos los arrays tienen la misma longitud
        Individual offspring = new Individual
        {
            HipAnglesRight = new float[steps],
            KneeAnglesRight = new float[steps],
            HipAnglesLeft = new float[steps],
            KneeAnglesLeft = new float[steps]
        };

        for (int i = 0; i < steps; i++)
        {
            // Combina valores de los padres para las articulaciones derechas
            offspring.HipAnglesRight[i] = Random.value < 0.5f ? parent1.HipAnglesRight[i] : parent2.HipAnglesRight[i];
            offspring.KneeAnglesRight[i] = Random.value < 0.5f ? parent1.KneeAnglesRight[i] : parent2.KneeAnglesRight[i];

            // Combina valores de los padres para las articulaciones izquierdas
            offspring.HipAnglesLeft[i] = Random.value < 0.5f ? parent1.HipAnglesLeft[i] : parent2.HipAnglesLeft[i];
            offspring.KneeAnglesLeft[i] = Random.value < 0.5f ? parent1.KneeAnglesLeft[i] : parent2.KneeAnglesLeft[i];
        }

        return offspring;
    }


    void Mutate(Individual individual)
    {
        for (int i = 0; i < individual.HipAnglesRight.Length; i++)
        {
            if (Random.value < mutationRate)
            {
                individual.HipAnglesRight[i] += Random.Range(-10f, 10f);
            }
            if (Random.value < mutationRate)
            {
                individual.KneeAnglesRight[i] += Random.Range(-10f, 10f);
            }

            if (Random.value < mutationRate)
            {
                individual.HipAnglesLeft[i] += Random.Range(-10f, 10f);
            }
            if (Random.value < mutationRate)
            {
                individual.KneeAnglesLeft[i] += Random.Range(-10f, 10f);
            }
        }
    }

}
