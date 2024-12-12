using UnityEngine;

public class JointController : MonoBehaviour
{
    public Transform HipRight;
    public Transform KneeRight;
    public Transform HipLeft;
    public Transform KneeLeft;

    public void ApplyAngles(float hipAngle, float kneeAngle)
    {
        if (HipRight != null)
            HipRight.localRotation = Quaternion.Euler(0, 0, hipAngle);
        if (KneeRight != null)
            KneeRight.localRotation = Quaternion.Euler(0, 0, kneeAngle);
        if (HipLeft != null)
            HipLeft.localRotation = Quaternion.Euler(0, 0, -hipAngle);
        if (KneeLeft != null)
            KneeLeft.localRotation = Quaternion.Euler(0, 0, -kneeAngle);
    }

    public void UpdatePhysics()
    {
        GetComponent<Rigidbody>().WakeUp();
    }
}