/*
FORNO's Software License v1 (https://github.com/forno/FORNOsSoftwareLicense/blob/v1/LICENSE.md)

Copyright (c) 2020 FORNO
*/

using UdonSharp;
using VRC.SDKBase;
using UnityEngine;
using System;

public class UdonPhysicsSystem : UdonSharpBehaviour
{
    [Header("Global physics data")]
    public float normalDrag = 0;
    public float normalAngularDrag = 0.5f;

    private const int PlayerModeStandard = 0;
    private const int PlayerModeAlwaysNormal = 1;
    private const int PlayerModeAlwaysFly = 2;
    [Header("Player config")]
    [Tooltip("0: standard, 1: always normal, 2: always fly")]
    public int playerMode = PlayerModeStandard;
    public VRCPlayerApi.TrackingDataType trackingTarget = VRCPlayerApi.TrackingDataType.Head;
    public float flyDrag = 1;

    [Header("UdonPhysics config")]
    public string volumeTagName = "UdonPhysics";

    [NonSerialized]
    public float drag;

    private bool isEditor;
    private Vector3 lastVelocity;
    private VRCPlayerApi p;
    private Vector3 velocity;

    private void Start()
    {
        p = Networking.LocalPlayer;
        isEditor = p == null;
    }

    private void Update()
    {
        if (isEditor)
            return;
        velocity = p.GetVelocity();
        switch (playerMode)
        {
            case PlayerModeStandard:
                DoMode();
                break;
            case PlayerModeAlwaysNormal:
                // nothing to do
                break;
            case PlayerModeAlwaysFly:
                DoFly();
                break;
        }
        lastVelocity = velocity;
    }

    private void DoMode()
    {
        var t = p.GetPlayerTag(volumeTagName);
        if (t == "swim")
        {
            DoFly();
            return;
        }
        if (t == "fly")
        {
            drag = flyDrag;
            DoFly();
            return;
        }
    }

    private void DoFly()
    {
        CancelGravity();
        Move2Forward();
        Move2UpDownByUserInput();
        AffectDragAfterCancelGravity();
        p.SetVelocity(velocity);
    }

    public void AffectDrag()
    {
        velocity *= 1 - drag * Time.deltaTime;
    }

    public void CancelGravity()
    {
        velocity -= (p.GetGravityStrength() * Time.deltaTime) * Physics.gravity;
    }

    public void Move2Forward()
    {
        var dv = velocity - lastVelocity;
        var trackingData = p.GetTrackingData(trackingTarget);
        if (trackingData.Equals(null))
            return;
        var h2v = trackingData.rotation * Quaternion.Inverse(p.GetRotation()); // horizon to view
        dv = h2v * dv;
        //dv = ((p.GetWalkSpeed() - 0.1f) * Mathf.Max(drag, 1) * Time.deltaTime) * dv.normalized; // cancel drag effect
        velocity = lastVelocity + dv;
    }

    public void Move2UpDownByUserInput()
    {
        var down = Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftControl);
        var up = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space);
        if (!down && !up)
            return;
        var s = p.GetWalkSpeed() * Time.deltaTime * Mathf.Max(drag, 1); // cancel drag effect
        if (down)
            velocity += s * Vector3.down;
        if (up)
            velocity += s * Vector3.up;
    }

    public void AffectDragAfterCancelGravity()
    {
        var dragFactor = 1 - drag * Time.deltaTime;
        //var fixed4Gravity = (p.GetGravityStrength() * drag * Time.deltaTime * Time.deltaTime) * Physics.gravity;
        //velocity = dragFactor * velocity - fixed4Gravity;
        velocity = dragFactor * velocity;
    }
}
