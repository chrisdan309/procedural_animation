using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class Leg : MonoBehaviour
{
    [SerializeField] private Player player;
    [Header("Joint Angles")]
    [SerializeField] private float q1 = 0;
    [SerializeField] private float q2 = 0;
    private float initialQ1;
    private float initialQ2;

    [Header("Segment Lengths")]
    [SerializeField] private float l1 = 2;
    [SerializeField] private float l2 = 2;

    [Header("Mirroring")]
    [SerializeField] private bool isMirrored = false;

    private Transform root;
    private Transform knee;
    private Transform hip;
    private Transform thigh;
    private Transform foot;

    void Awake()
    {
        InitializeTransforms();
        ConfigureLegSegments();
    }

    void Start()
    {
        initialQ1 = q1;
        initialQ2 = q2;
    }

    private void InitializeTransforms()
    {
        if (transform.childCount < 1) return;

        root = transform;
        hip = root.GetChild(0);
        thigh = hip?.GetChild(0);
        knee = hip?.GetChild(1);
        foot = knee?.GetChild(0);

        if (hip == null || thigh == null || knee == null || foot == null)
        {
            Debug.LogError("Leg structure is missing one or more necessary child objects.");
        }
    }

    private void ConfigureLegSegments()
    {
        float mirrorFactor = isMirrored ? -1 : 1;

        root.rotation = Quaternion.Euler(0, 0, q1 * mirrorFactor);

        hip.localPosition = new Vector3(l1 / 2 * mirrorFactor, 0, 0);
        thigh.localScale = new Vector3(l1, 0.5f, 1);

        knee.localPosition = new Vector3((l1 / 2 - 0.15f) * mirrorFactor, 0, 0);
        knee.rotation = Quaternion.Euler(0, 0, q2 * mirrorFactor);

        foot.localPosition = new Vector3(l2 / 2 * mirrorFactor, 0, 0);
        foot.localScale = new Vector3(l2, 0.3f, 1);
    }

    private void OnValidate()
    {
        if (!Application.isPlaying && hip != null)
        {
            ConfigureLegSegments();
        }
    }

    public Vector2 GetFootPosition()
    {
        float mirrorFactor = isMirrored ? -1 : 1;
        return (Vector2)foot.position + (l2 * 0.5f * mirrorFactor * (Vector2)foot.right);
    }

    IEnumerator MoveToRoutine(Vector2 target)
    {
        float duration = 2f;
        float time = 0;
        Debug.Log("Moving leg " + (isMirrored ? "Left" : "Right") + " to " + target);
        while (time < duration)
        {
            time += Time.deltaTime;

            float distance = Vector2.Distance(root.transform.position, target);
            Vector2 direction = target - (Vector2)root.transform.position;
            float atan = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            if (l1 + l2 < distance)
            {
                q1 = Mathf.Lerp(q1, atan, time / duration);
                q2 = 0;
            }
            else
            {
                float cosAngle0 = (distance * distance + l1 * l1 - l2 * l2) / (2 * distance * l1);
                float angle0 = Mathf.Acos(cosAngle0) * Mathf.Rad2Deg;

                float cosAngle1 = (l2 * l2 + l1 * l1 - distance * distance) / (2 * l2 * l1);
                float angle1 = Mathf.Acos(cosAngle1) * Mathf.Rad2Deg;

                q1 = Mathf.Lerp(q1, (atan - angle0) * (isMirrored ? -1 : 1), time / duration);
                q2 = Mathf.Lerp(q2, (180f - angle1) * (isMirrored ? -1 : 1), time / duration);
            }

            ConfigureLegSegments();
            yield return null;
        }
        NotifyPlayer();
    }

    public void MoveTo(Vector2 target)
    {
        StartCoroutine(MoveToRoutine(target));
    }

    private void NotifyPlayer()
    {
        player?.OnLegMoved(isMirrored);
    }
}
