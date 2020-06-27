using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class PlayerRigidbody : UdonSharpBehaviour
{
    public float mass = 50;
    public ForceMode forceMode = ForceMode.Force;
    public Vector3 force;

    public void AddForce()
    {
        var v = Networking.LocalPlayer.GetVelocity();
        switch (forceMode)
        {
            case ForceMode.Force:
                Networking.LocalPlayer.SetVelocity(v + mass * force * Time.fixedDeltaTime);
                break;
            case ForceMode.Acceleration:
                Networking.LocalPlayer.SetVelocity(v + force * Time.fixedDeltaTime);
                break;
            case ForceMode.Impulse:
                Networking.LocalPlayer.SetVelocity(v + mass * force);
                break;
            case ForceMode.VelocityChange:
                Networking.LocalPlayer.SetVelocity(v + force);
                break;
        }
        forceMode = ForceMode.Force;
        force = Vector3.zero;
    }
}
