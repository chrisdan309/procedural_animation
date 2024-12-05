using UnityEngine;

public class Player : MonoBehaviour
{
    public float raycastDistance = 1f;
    public float moveOffset = 1f;
    public Leg legR;
    public Leg legL;

    public LayerMask raycastLayer;

    private Vector2 targetR;
    private Vector2 targetL;
    private bool isMovingLeg = false;
    private bool direction = true;
    private Color rayRColor = Color.green;
    private Color rayLColor = Color.red;

    void Update()
    {

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

    public void OnLegMoved(bool isLeftLeg)
    {
        isMovingLeg = false;
        if (isLeftLeg)
            StartWalkingR();
        else
            StartWalkingL();
    }
}
