using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class SimpleBuoyancyVolume : UdonSharpBehaviour
{
    public float density = 1000;
    public float drag = 1;
    public float angularDrag = 1;
    public UdonPhysicsSystem udonPhysics;

    private Collider waterCollider;

    private void Start()
    {
        waterCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetType() == typeof(CharacterController))
        {
            Networking.LocalPlayer.SetPlayerTag(udonPhysics.volumeTagName, "swim");
            udonPhysics.drag = drag;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetType() == typeof(SphereCollider))
        {
            var radius = other.bounds.extents.y;
            var t = Mathf.Clamp(waterCollider.bounds.max.y - other.bounds.center.y, -radius, radius);
            var v = -t * t * t / 3f + radius * radius * t + radius * radius * radius * 2f / 3f; // Volume of sphere
            var r = other.attachedRigidbody;
            if (r == null)
                return;
            r.drag = drag;
            r.angularDrag = angularDrag;
            r.AddForceAtPosition(-Physics.gravity * (v * density), other.bounds.center);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var type = other.GetType();
        if (type == typeof(SphereCollider))
        {
            var r = other.attachedRigidbody;
            r.drag = udonPhysics.normalDrag;
            r.angularDrag = udonPhysics.normalAngularDrag;
        }
        else if (type == typeof(CharacterController))
        {
            Networking.LocalPlayer.SetPlayerTag(udonPhysics.volumeTagName, "");
        }
    }
}
