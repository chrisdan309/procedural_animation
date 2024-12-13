using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FitnessEvaluator
{
    public static IEnumerator EvaluatePopulationParallel(List<Individual> population, JointController jointControllerPrefab, float cycleDuration, GameObject robotPrefab, int batchSize)
    {
        // Divide la población en lotes (batches)
        for (int i = 0; i < population.Count; i += batchSize)
        {
            int end = Mathf.Min(i + batchSize, population.Count);
            List<Individual> batch = population.GetRange(i, end - i);

            // Crea una lista de corutinas para evaluar el lote actual
            List<Coroutine> evaluations = new List<Coroutine>();
            for (int j = 0; j < batch.Count; j++)
            {
                Individual individual = batch[j];
                float zPosition = j * 3f; // Asigna posiciones en el eje z con un salto de 3
                Coroutine evaluation = CoroutineManager.Instance.StartCoroutine(EvaluateIndividual(individual, jointControllerPrefab, robotPrefab, cycleDuration, zPosition));
                evaluations.Add(evaluation);
            }

            // Espera a que todas las evaluaciones del lote terminen
            foreach (Coroutine evaluation in evaluations)
            {
                yield return evaluation;
            }
        }
    }

    private static IEnumerator EvaluateIndividual(Individual individual, JointController jointControllerPrefab, GameObject robotPrefab, float cycleDuration, float zPosition)
    {
        // Instancia un clon del robot
        GameObject robotInstance = GameObject.Instantiate(robotPrefab);
        robotInstance.transform.position = new Vector3(0, 0, zPosition); // Establece la posición inicial con z personalizada

        JointController jointControllerInstance = robotInstance.GetComponent<JointController>();

        if (jointControllerInstance == null)
        {
            Debug.LogError("El prefab del robot debe contener un componente JointController.");
            GameObject.Destroy(robotInstance);
            yield break;
        }

        int steps = individual.HipAnglesRight.Length;
        float stepDuration = cycleDuration / steps;

        // Reinicia el estado del robot antes de evaluar
        ResetRobotState(jointControllerInstance, robotInstance);

        Vector3 initialPosition = jointControllerInstance.transform.position;
        float smoothnessPenalty = 0f; // Penalización acumulativa por movimientos bruscos

        for (int i = 0; i < steps; i++)
        {
            // Aplica los ángulos del individuo para ambas patas
            float hipAngleRight = individual.HipAnglesRight[i];
            float kneeAngleRight = individual.KneeAnglesRight[i];
            float hipAngleLeft = individual.HipAnglesLeft[i];
            float kneeAngleLeft = individual.KneeAnglesLeft[i];
            jointControllerInstance.ApplyAngles(hipAngleRight, kneeAngleRight, hipAngleLeft, kneeAngleLeft);

            // Evalúa suavidad si no es el primer paso
            if (i > 0)
            {
                float hipChangeRight = Mathf.Abs(individual.HipAnglesRight[i] - individual.HipAnglesRight[i - 1]);
                float kneeChangeRight = Mathf.Abs(individual.KneeAnglesRight[i] - individual.KneeAnglesRight[i - 1]);
                float hipChangeLeft = Mathf.Abs(individual.HipAnglesLeft[i] - individual.HipAnglesLeft[i - 1]);
                float kneeChangeLeft = Mathf.Abs(individual.KneeAnglesLeft[i] - individual.KneeAnglesLeft[i - 1]);

                if (hipChangeRight > 20f) smoothnessPenalty += hipChangeRight - 20f;
                if (kneeChangeRight > 20f) smoothnessPenalty += kneeChangeRight - 20f;
                if (hipChangeLeft > 20f) smoothnessPenalty += hipChangeLeft - 20f;
                if (kneeChangeLeft > 20f) smoothnessPenalty += kneeChangeLeft - 20f;
            }

            yield return new WaitForSeconds(stepDuration);
        }

        // Calcula el desplazamiento horizontal
        Vector3 finalPosition = jointControllerInstance.transform.position;
        float distanceTraveled = Mathf.Abs(finalPosition.x - initialPosition.x);

        // Ajusta la fitness combinando desplazamiento y penalización
        float fitness = 10 * distanceTraveled;
        fitness = Mathf.Max(0f, fitness);

        individual.Fitness = fitness;

        Debug.Log($"Individual Fitness: {fitness}");

        // Destruye el clon del robot después de la evaluación
        GameObject.Destroy(robotInstance);
    }

    private static void ResetRobotState(JointController jointController, GameObject robotInstance)
    {
        jointController.ApplyAngles(30f, -90f, -30f, -90f);

        robotInstance.transform.position = new Vector3(0,0, robotInstance.transform.position.z);

        Rigidbody rb = robotInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
