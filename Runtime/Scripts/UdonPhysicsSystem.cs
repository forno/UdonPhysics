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
    public float flyDrag = 1;

    [Header("UdonPhysics config")]
    public string volumeTagName = "UdonPhysics";

    [NonSerialized]
    public float drag;

    private bool isEditor;
    private Vector3 lastVelocity;
    private VRCPlayerApi p;

    private void Start()
    {
        p = Networking.LocalPlayer;
        isEditor = p == null;
    }

    private void FixedUpdate()
    {
        if (isEditor)
            return;
        switch (playerMode)
        {
            case PlayerModeStandard:
                var t = p.GetPlayerTag(volumeTagName);
                if (t == "swim" || t == "fly")
                {
                    AffectDrag();
                    CancelGravity();
                }
                break;
            case PlayerModeAlwaysNormal:
                // nothing to do
                break;
            case PlayerModeAlwaysFly:
                AffectDrag();
                CancelGravity();
                break;
        }
    }

    private void Update()
    {
        if (isEditor)
            return;
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
        lastVelocity = p.GetVelocity();
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
        Move2Forward();
        Move2UpDownByUserInput();
    }

    public void AffectDrag()
    {
        var t = 1 - drag * Time.fixedDeltaTime;
        p.SetVelocity(t * p.GetVelocity());
    }

    public void CancelGravity()
    {
        var g = (p.GetGravityStrength() * Time.fixedDeltaTime) * Physics.gravity;
        p.SetVelocity(p.GetVelocity() - g);
    }

    public void Move2Forward()
    {
        var dv = p.GetVelocity() - lastVelocity;
        var h2v = p.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation * Quaternion.Inverse(p.GetRotation()); // horizon to view
        dv = h2v * dv;
        dv = (p.GetWalkSpeed() * Time.deltaTime * Mathf.Max(drag, 1)) * dv.normalized; // cancel drag effect
        p.SetVelocity(lastVelocity + dv);
    }

    public void Move2UpDownByUserInput()
    {
        var down = Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftControl);
        var up = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space);
        if (!down && !up)
            return;
        var s = p.GetWalkSpeed() * Time.deltaTime * Mathf.Max(drag, 1); // cancel drag effect
        var v = p.GetVelocity();
        if (down)
            v += s * Vector3.down;
        if (up)
            v += s * Vector3.up;
        p.SetVelocity(v);
    }
}
