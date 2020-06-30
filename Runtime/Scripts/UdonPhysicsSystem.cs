/* MIT License

Copyright (c) 2020 FORNO

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
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
            CancelGravity();
            Move2Forward();
            Move2UpDownByUserInput();
            AffectDrag();
            return;
        }
        if (t == "fly")
        {
            DoFly();
            return;
        }
    }

    private void DoFly()
    {
        drag = flyDrag;
        CancelGravity();
        Move2Forward();
        Move2UpDownByUserInput();
        AffectDrag();
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
        dv = (p.GetWalkSpeed() * Time.fixedDeltaTime) * dv.normalized;
        p.SetVelocity(lastVelocity + dv);
    }

    public void Move2UpDownByUserInput()
    {
        var down = Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftControl);
        var up = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space);
        if (!down && !up)
            return;
        var s = p.GetWalkSpeed() * Time.fixedDeltaTime;
        var v = p.GetVelocity();
        if (down)
            v += s * Vector3.down;
        if (up)
            v += s * Vector3.up;
        p.SetVelocity(v);
    }

    public void AffectDrag()
    {
        var t = 1 - drag * Time.fixedDeltaTime;
        p.SetVelocity(t * p.GetVelocity());
    }
}
