using UnityEngine;

public class JointController : MonoBehaviour
{
    public Transform HipRight;
    public Transform KneeRight;
    public Transform HipLeft;
    public Transform KneeLeft;

    public void ApplyAngles(float hipAngleRight, float kneeAngleRight, float hipAngleLeft, float kneeAngleLeft)
    {
        if (HipRight != null)
            HipRight.rotation = Quaternion.Euler(0, 0, hipAngleRight);
        if (KneeRight != null)
            KneeRight.rotation = Quaternion.Euler(0, 0, kneeAngleRight);
        if (HipLeft != null)
            HipLeft.rotation = Quaternion.Euler(0, 0, hipAngleLeft);
        if (KneeLeft != null)
            KneeLeft.rotation = Quaternion.Euler(0, 0, kneeAngleLeft);
    }

    public void UpdatePhysics()
    {
        GetComponent<Rigidbody>().WakeUp();
    }
}