using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FitnessEvaluator
{
    public static IEnumerator EvaluatePopulationParallel(List<Individual> population,
        JointController jointControllerPrefab, float cycleDuration, GameObject robotPrefab, int batchSize)
    {
        for (int i = 0; i < population.Count; i += batchSize)
        {
            int end = Mathf.Min(i + batchSize, population.Count);
            List<Individual> batch = population.GetRange(i, end - i);

            List<Coroutine> evaluations = new List<Coroutine>();
            for (int j = 0; j < batch.Count; j++)
            {
                Individual individual = batch[j];
                float zPosition = j * 3f;
                Coroutine evaluation = CoroutineManager.Instance.StartCoroutine(EvaluateIndividual(individual,
                    jointControllerPrefab, robotPrefab, cycleDuration, zPosition));
                evaluations.Add(evaluation);
            }

            foreach (Coroutine evaluation in evaluations)
            {
                yield return evaluation;
            }
        }
    }

    private static IEnumerator EvaluateIndividual(Individual individual, JointController jointControllerPrefab,
        GameObject robotPrefab, float cycleDuration, float zPosition)
    {
        // Instancia al robot
        GameObject robotInstance = GameObject.Instantiate(robotPrefab);
        robotInstance.transform.position = new Vector3(0, 0, zPosition);

        JointController jointControllerInstance = robotInstance.GetComponent<JointController>();

        if (jointControllerInstance == null)
        {
            Debug.LogError("El prefab del robot debe contener un componente JointController.");
            GameObject.Destroy(robotInstance);
            yield break;
        }

        int steps = individual.HipAnglesRight.Length;
        float stepDuration = cycleDuration / steps;

        ResetRobotState(jointControllerInstance, robotInstance);

        Vector3 initialPosition = jointControllerInstance.transform.position;
        float smoothnessPenalty = 0f;
        float verticalPenalty = 0f;

        float previousYPosition = initialPosition.y;

        for (int i = 0; i < steps; i++)
        {
            float hipAngleRight = individual.HipAnglesRight[i];
            float kneeAngleRight = individual.KneeAnglesRight[i];
            float hipAngleLeft = individual.HipAnglesLeft[i];
            float kneeAngleLeft = individual.KneeAnglesLeft[i];
            jointControllerInstance.ApplyAngles(hipAngleRight, kneeAngleRight, hipAngleLeft, kneeAngleLeft);

            if (i > 0)
            {
                float hipChangeRight = Mathf.Abs(individual.HipAnglesRight[i] - individual.HipAnglesRight[i - 1]);
                float kneeChangeRight = Mathf.Abs(individual.KneeAnglesRight[i] - individual.KneeAnglesRight[i - 1]);
                float hipChangeLeft = Mathf.Abs(individual.HipAnglesLeft[i] - individual.HipAnglesLeft[i - 1]);
                float kneeChangeLeft = Mathf.Abs(individual.KneeAnglesLeft[i] - individual.KneeAnglesLeft[i - 1]);

                if (hipChangeRight > 10f) smoothnessPenalty += hipChangeRight - 10f;
                if (kneeChangeRight > 10f) smoothnessPenalty += kneeChangeRight - 10f;
                if (hipChangeLeft > 10f) smoothnessPenalty += hipChangeLeft - 10f;
                if (kneeChangeLeft > 10f) smoothnessPenalty += kneeChangeLeft - 10f;
            }

            float currentYPosition = jointControllerInstance.transform.position.y;
            float yChange = Mathf.Abs(currentYPosition - previousYPosition);

            if (yChange > 1f)
            {
                verticalPenalty += yChange * 10f;
            }

            previousYPosition = currentYPosition;

            yield return new WaitForSeconds(stepDuration);
        }

        Vector3 finalPosition = jointControllerInstance.transform.position;
        float distanceTraveled = Mathf.Abs(finalPosition.x - initialPosition.x);

        float fitness = 30 * distanceTraveled - 0.01f * smoothnessPenalty - 0.1f * verticalPenalty;
        if (distanceTraveled < 3f)
        {
            fitness /= 3;
        }

        individual.Fitness = fitness;

        Debug.Log(
            $"Individual Fitness: {fitness} (Smoothness Penalty: {smoothnessPenalty}, Vertical Penalty: {verticalPenalty})");

        GameObject.Destroy(robotInstance);
    }


    private static void ResetRobotState(JointController jointController, GameObject robotInstance)
    {
        jointController.ApplyAngles(30f, -90f, -30f, -90f);

        robotInstance.transform.position = new Vector3(0, 0, robotInstance.transform.position.z);

        Rigidbody rb = robotInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}