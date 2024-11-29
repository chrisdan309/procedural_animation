using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Movement))]
public class MovementEditor : Editor
{
    private Movement _movementScript;

    private void OnEnable()
    {
        _movementScript = (Movement)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        float newMaxX = EditorGUILayout.FloatField("Max T Value", _movementScript.maxT);

        if (Mathf.Abs(newMaxX - _movementScript.maxT) > Mathf.Epsilon)
        {
            _movementScript.maxT = newMaxX;
            Repaint();
        }
        
        float[] sinValues = _movementScript.GetSinGraphValues();
        
        Rect rect = GUILayoutUtility.GetRect(200, 150);
        
        DrawGraph(rect, sinValues);
    }

    private void DrawGraph(Rect rect, float[] values)
    {
        Handles.BeginGUI();
        Handles.color = Color.green;

        DrawAxes(rect);

        Vector2 prevPoint = new Vector2(0, values[0]);

        for (int i = 0; i < values.Length; i++)
        {
            float x = Mathf.Lerp(0, _movementScript.maxT, i / (float)(values.Length - 1));
            float y = Mathf.Lerp(0, 1, values[i] * 0.5f + 0.5f);
            y = 1 - y;

            Vector2 newPoint = new Vector2(x, y);
            
            Handles.DrawLine(new Vector3(prevPoint.x * rect.width / _movementScript.maxT + rect.x, prevPoint.y * rect.height + rect.y, 0), 
                             new Vector3(newPoint.x * rect.width / _movementScript.maxT + rect.x, newPoint.y * rect.height + rect.y, 0));
            prevPoint = newPoint;
        }

        Handles.EndGUI();
    }

    private void DrawAxes(Rect rect)
    {
        Handles.color = Color.white;

        Handles.DrawLine(new Vector3(rect.x, rect.y + rect.height / 2, 0), 
                         new Vector3(rect.x + rect.width, rect.y + rect.height / 2, 0));

        Handles.DrawLine(new Vector3(rect.x + rect.width / 2, rect.y, 0), 
                         new Vector3(rect.x + rect.width / 2, rect.y + rect.height, 0));
        
        DrawAxisLabels(rect);
    }

    private void DrawAxisLabels(Rect rect)
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 12;

        Handles.Label(new Vector3(rect.x + rect.width - 10, rect.y + rect.height / 2, 0), _movementScript.maxT.ToString(), style);
        Handles.Label(new Vector3(rect.x, rect.y + rect.height / 2, 0), "0", style);

        Handles.Label(new Vector3(rect.x + rect.width / 2, rect.y + rect.height - 10, 0), "1", style);
        Handles.Label(new Vector3(rect.x + rect.width / 2, rect.y + 10, 0), "0", style);
    }
}
