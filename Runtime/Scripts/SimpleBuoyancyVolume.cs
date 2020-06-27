using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class SimpleBuoyancyVolume : UdonSharpBehaviour
{
    public Collider waterCollider;
    public float density = 1000;
    public PhysicsData physicsData;
    public string swimTag = "swim";

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetType() == typeof(CharacterController))
        {
            Networking.LocalPlayer.SetPlayerTag(swimTag, "true");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        var type = other.GetType();
        if (type == typeof(SphereCollider))
        {
            var radius = other.bounds.extents.y;
            var t = Mathf.Clamp(waterCollider.bounds.max.y - other.bounds.center.y, -radius, radius);
            var v = -t * t * t / 3f + radius * radius * t + radius * radius * radius * 2f / 3f; // Volume of sphere
            var r = other.attachedRigidbody;
            r.drag = physicsData.dragOnWater;
            r.angularDrag = physicsData.angularDragOnWater;
            r.AddForceAtPosition(-Physics.gravity * (v * density), other.bounds.center);
        }
        else if (type == typeof(CharacterController)) // only local player
        {
            var player = Networking.LocalPlayer;
            var p = player.GetPosition();
            var t = Mathf.Clamp(waterCollider.bounds.max.y - p.y + physicsData.playerHeight / 2, 0, physicsData.playerHeight);
            var v = physicsData.playerRadius * physicsData.playerRadius * Mathf.PI * t; // Volume of cylinder
            var velocity = player.GetVelocity() - Physics.gravity * (v * density / physicsData.playerMass * Time.fixedDeltaTime);
            velocity *= 1 - physicsData.dragOnWater * Time.fixedDeltaTime;
            player.SetVelocity(velocity);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var type = other.GetType();
        if (type == typeof(SphereCollider))
        {
            var r = other.attachedRigidbody;
            r.drag = physicsData.dragOnAir;
            r.angularDrag = physicsData.angularDragOnAir;
        }
        else if (type == typeof(CharacterController))
        {
            Networking.LocalPlayer.SetPlayerTag(swimTag, "false");
        }
    }
}
