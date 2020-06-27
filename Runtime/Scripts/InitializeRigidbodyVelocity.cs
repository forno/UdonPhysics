using UdonSharp;
using UnityEngine;

public class InitRigidbodyVelocity : UdonSharpBehaviour
{
    public Rigidbody target;
    public Vector3 linerDirection = Vector3.forward;
    public float linerSpeed = 0;
    public Vector3 angularDirection = Vector3.up;
    public float angularSpeed = 0;

    public void Init()
    {
        target.AddRelativeForce(linerDirection.normalized * linerSpeed, ForceMode.VelocityChange);
        target.AddRelativeTorque(angularDirection * angularSpeed, ForceMode.VelocityChange);
    }
}
