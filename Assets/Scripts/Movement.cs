using UnityEngine;

public class Movement : MonoBehaviour
{
    // mq''(t) + b q'(t) = k(p(t)-q(t))
    [Header("Physical Constants")]
    public float m = 1.0f;
    public float b = 0.5f;
    public float k = 10.0f;
    
    private Vector2 _q = Vector2.zero;
    private Vector2 _v = Vector2.zero;
    
    public Transform sphereParent;
    
    [Header("Graph Settings")]
    public float maxT = 2.0f;
    private float[] _graph;
    

    void Update()
    {
        transform.position = SecondOrderSolution(sphereParent);
    }

    private Vector3 SecondOrderSolution(Transform parent)
    {
        Vector2 p = new Vector2(parent.position.x, parent.position.y);
        Vector2 vNew = _v + (-b * _v + k * (p - _q)) / m * Time.deltaTime;

        _q += vNew * Time.deltaTime;

        _v = vNew;

        Vector3 newPosition = new Vector3(_q.x, _q.y, transform.position.z);
        return newPosition;
    }
    
    public float[] GetSinGraphValues()
    {
        int numPoints = 100;
        float[] values = new float[numPoints];

        float timeSim;
        float xSim = 0.5f;
        float vSimNew;
        float qSim = 0.0f;
        float vSim = 0.0f;
        float dt = 0.02f;

        float maxTSim = maxT;

        for (int i = 0; i < numPoints; i++)
        {
            timeSim = i * maxTSim / (numPoints - 1);
            vSimNew = vSim + (-b * vSim + k * (xSim - qSim)) / m * timeSim;
            qSim += vSim * dt;
            vSim = vSimNew;
            values[i] = qSim;
        }

        return values;
    }
    
    
}
