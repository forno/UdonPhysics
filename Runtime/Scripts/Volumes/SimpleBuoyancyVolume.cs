/*
FORNO's Software License v1 (https://github.com/forno/FORNOsSoftwareLicense/blob/v1/LICENSE.md)

Copyright (c) 2020 FORNO
*/
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class SimpleBuoyancyVolume : UdonSharpBehaviour
{
    public float density = 1000;
    public float drag = 3;
    public float angularDrag = 1;
    public bool isPlayerEffect = true;
    public UdonPhysicsSystem udonPhysics;

    private Collider volumeCollider;

    private void Start()
    {
        volumeCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPlayerEffect && other.GetType() == typeof(CharacterController))
        {
            Networking.LocalPlayer.SetPlayerTag(udonPhysics.volumeTagName, "swim");
            udonPhysics.drag = drag;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetType() == typeof(SphereCollider))
        {
            var r = other.attachedRigidbody;
            if (r == null)
                return;
            var radius = other.bounds.extents.y; // Shpere collider
            const float overWrap = 1.5f;
            var top_point = other.bounds.center + (overWrap * radius) * Vector3.up;
            RaycastHit hit;
            var t = (volumeCollider.Raycast(new Ray(top_point, Vector3.down), out hit, 2 * radius * overWrap) ? Mathf.Clamp(hit.point.y - other.bounds.center.y, -radius, radius) : radius);
            var v = -t * t * t / 3f + t * radius * radius + radius * radius * radius * 2f / 3f; // Volume of sphere
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
        else if (isPlayerEffect && type == typeof(CharacterController))
        {
            Networking.LocalPlayer.SetPlayerTag(udonPhysics.volumeTagName, "");
        }
    }
}
