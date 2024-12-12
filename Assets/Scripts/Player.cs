using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float raycastDistance = 1f;
    public float moveOffset = 1f;
    public Leg legR;
    public Leg legL;
    public Transform body;

    public LayerMask raycastLayer;

    private Vector2 targetR;
    private Vector2 targetL;
    private bool isMovingLeg = false;
    private bool direction = true;
    private Color rayRColor = Color.green;
    private Color rayLColor = Color.red;

    void Start()
    {
        body = transform;
    }

    void Update()
    {
        if (isMovingLeg) return;
        UpdateTargets();
        CastRayAndDebug(targetR, false);
        CastRayAndDebug(targetL, true);
        Vector3 rayR = (Vector3)targetR - transform.position;
        Debug.DrawRay(transform.position, rayR.normalized * raycastDistance, rayRColor);

        Vector3 rayL = (Vector3)targetL - transform.position;
        Debug.DrawRay(transform.position, rayL.normalized * raycastDistance, rayLColor);

        if (Input.GetKey(KeyCode.Space) && !isMovingLeg)
        {
            StartWalkingR();
        }
    }
    public void MoveBody(float x)
    {
        body.position = new Vector3(x, body.position.y, body.position.z);
    }

    private void StartWalkingR()
    {
        isMovingLeg = true;
        legR.MoveTo(targetR);
    }

    private void StartWalkingL()
    {
        isMovingLeg = true;
        legL.MoveTo(targetL);
    }

    private void UpdateTargets()
    {
        Vector2 offset = Vector2.right * (direction ? moveOffset : -moveOffset);
        targetR = legR.GetFootPosition() + offset;
        targetL = legL.GetFootPosition() + offset;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetR, 0.1f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(targetL, 0.1f);
    }

    private bool CastRayAndDebug(Vector3 target, bool isLeftLeg)
    {
        Vector3 direction = target - transform.position;
        if (Physics.Raycast(transform.position, direction.normalized, out RaycastHit hit, raycastDistance, raycastLayer))
        {
            if (isLeftLeg) rayLColor = Color.green;
            else rayRColor = Color.green;
            return true;
        }
        if (isLeftLeg) rayLColor = Color.red;
        else rayRColor = Color.red;
        return false;
    }

    IEnumerator MoveBodyRoutine(Vector3 target)
    {
        float duration = .05f;
        float time = 0;
        Vector3 initialPosition = body.position;
        while (time < duration)
        {
            time += Time.deltaTime;
            body.position = Vector3.Lerp(initialPosition, target, time / duration);
            yield return null;
        }
        yield return new WaitForSeconds(1f);
        StartWalkingR();
    }

    public void OnLegMoved(bool isLeftLeg)
    {
        isMovingLeg = false;
        if (isLeftLeg){
            legR.RotateLeg();
            legL.RotateLeg();
            StartCoroutine(MoveBodyRoutine(body.position + Vector3.right * (direction ? moveOffset : -moveOffset)));
        }
        else
            StartWalkingL();
    }
}
