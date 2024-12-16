using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement3D : MonoBehaviour
{
     // mq''(t) + b q'(t) = k(p(t)-q(t))
        [Header("Physical Constants")]
        public float m = 1.0f;
        public float b = 0.5f;
        public float k = 10.0f;
    
        private Vector3 _q = Vector3.zero;
        private Vector3 _v = Vector3.zero;
    
        public Transform sphereParent;
    
        [Header("Graph Settings")]
        public float maxT = 2.0f;
        private float[] _graph;
    
        void Update()
        {
            transform.position = SecondOrderSolution(sphereParent);
            DrawLineBetweenSpheres();
        }
        
        private void DrawLineBetweenSpheres()
        {
            Debug.DrawLine(transform.position, sphereParent.position, Color.red);
        }

        Vector3 F(Vector3 x, Vector3 v)
        {
            Vector3 first_component = b*v;
            Vector3 second_component = -k*x;
            return -(first_component + second_component)/m;
        }
        private Vector3 SecondOrderSolution(Transform parent)
        {
            Vector3 p = parent.position;
            Vector3 f = F(p - _q, _v);
            Vector3 vNew = _v + (-b * _v + k * (p - _q)) / m * Time.deltaTime;
    
            _q += vNew * Time.deltaTime;
    
            _v = vNew;
    
            return _q;
        }
    
        public float[] GetSinGraphValues()
        {
            int numPoints = 100;
            float[] values = new float[numPoints];
    
            float timeSim;
            Vector3 xSim = new Vector3(0.5f, 0.5f, 0.5f);
            Vector3 vSimNew;
            Vector3 qSim = Vector3.zero;
            Vector3 vSim = Vector3.zero;
            float dt = 0.02f;
    
            float maxTSim = maxT;
    
            for (int i = 0; i < numPoints; i++)
            {
                timeSim = i * maxTSim / (numPoints - 1);
                vSimNew = vSim + (-b * vSim + k * (xSim - qSim)) / m * timeSim;
                qSim += vSim * dt;
                vSim = vSimNew;
                values[i] = qSim.magnitude; // Store the magnitude as a single float value.
            }
    
            return values;
        }
}
