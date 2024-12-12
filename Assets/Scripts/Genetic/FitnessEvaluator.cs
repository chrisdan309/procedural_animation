using System.Collections;
using UnityEngine;

public static class FitnessEvaluator
{
    public static IEnumerator Evaluate(Individual individual, JointController jointController, float cycleDuration, System.Action<float> onComplete)
    {
        int steps = individual.HipAngles.Length;
        float stepDuration = cycleDuration / steps;
        Vector3 initialPosition = jointController.transform.position;

        for (int i = 0; i < steps; i++)
        {
            jointController.ApplyAngles(individual.HipAngles[i], individual.KneeAngles[i]);
            yield return new WaitForSeconds(stepDuration);
        }

        Vector3 finalPosition = jointController.transform.position;
        float fitness = finalPosition.x - initialPosition.x;
        onComplete?.Invoke(fitness);
    }
}