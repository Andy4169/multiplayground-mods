//     ___                        _                    _     
//    / _ \_   _ _ __  _ __   ___| |_  /\/\   __ _ ___| |_ ___ _ __ 
//   / /_)/ | | | '_ \| '_ \ / _ \ __|/    \ / _` / __| __/ _ \ '__|
//  / ___/| |_| | |_) | |_) |  __/ |_/ /\/\ \ (_| \__ \ ||  __/ |  
//  \/     \__,_| .__/| .__/ \___|\__\/    \/\__,_|___/\__\___|_|  
//              |_|   |_|                
//                                           
using System;
using UnityEngine;

namespace PuppetMaster
{
    public enum ThrowState
    {
        ready,
        aim,
        windup,
        fire,
    }

    public class ThrowItem
    {
        public ThrowState state       = ThrowState.ready;
        public Puppet Puppet = null;
        public Thing heldThing        = null;
        
        public int frame              = 0;

        public float power            = 2.2f;
        public float facing           = 1f;
        private float initGravity;

        protected Material lineMaterial;

        private LineRenderer lr;

        private Vector2 ThrowStartPos;
        private Vector2 ThrowEndPos;

        private MoveSet AP_Throw1;
        private MoveSet AP_Throw2;

        private bool CustomCamera = false;

        


        //
        // ─── THROW INIT ────────────────────────────────────────────────────────────────
        //
        public void Init()
        {
            frame = 0;

            CustomCamera = false;

            if (state == ThrowState.ready)
            {

                if (Puppet.HoldingF == null) return;

                if (Puppet.IsAiming) Puppet.AimingStop();
                if (Puppet.Actions.combatMode) {
                    Puppet.Actions.combatMode = false;
                    Puppet.Actions.attack.combatPose.ClearMove();
                }

                heldThing = Puppet.HoldingF;

                heldThing.doNotDispose = true;

                frame = 0;
                state     = ThrowState.aim;
                facing    = Puppet.FacingLeft ? -1 : 1;

                AP_Throw1                                  = new MoveSet("throw_1", false);
                AP_Throw1.Ragdoll.ShouldStandUpright       = true;
                AP_Throw1.Ragdoll.State                    = PoseState.Rest;
                AP_Throw1.Ragdoll.Rigidity                 = 2.2f;
                AP_Throw1.Ragdoll.ShouldStumble            = false;
                AP_Throw1.Ragdoll.AnimationSpeedMultiplier = 10.5f;
                AP_Throw1.Import();

                AP_Throw2                                  = new MoveSet("throw_2", false);
                AP_Throw2.Ragdoll.ShouldStandUpright       = true;
                AP_Throw2.Ragdoll.State                    = PoseState.Rest;
                AP_Throw2.Ragdoll.Rigidity                 = 2.2f;
                AP_Throw2.Ragdoll.ShouldStumble            = false;
                AP_Throw2.Ragdoll.AnimationSpeedMultiplier = 10.5f;
                AP_Throw2.Import();

                InitLine();

                Puppet.BlockMoves = true;

                initGravity = heldThing.P.InitialGravityScale;
                
            }
        }


        //
        // ─── THROW GO ────────────────────────────────────────────────────────────────
        //
        public void Go()
        {
            if (heldThing == null)
            {
                Puppet.PBO.OverridePoseIndex = 0;
                lr.enabled                   = false;
                state                        = ThrowState.ready;
                Puppet.BlockMoves            = false;
                AP_Throw2.ClearMove();
                AP_Throw1.ClearMove();
                return;
            }

            frame++;

            if (state == ThrowState.aim)
            {
                AP_Throw1.RunMove();

                if (KB.MouseDown || KB.Action) heldThing.P.SendMessage("Use", (object)new ActivationPropagation(), SendMessageOptions.DontRequireReceiver);

                if (!KB.Throw)
                {
                    frame = 0;
                    state = ThrowState.windup;
                    return;
                }

                if (frame > 10)
                {
                    if (!lr.enabled)
                    {
                        lr.enabled    = true;
                        ThrowStartPos = Puppet.RB2["Head"].position;
                    }

                    lr.enabled = true;

                    Vector2 mousePo = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    if (facing == 1f  && mousePo.x < Puppet.RB2["Head"].position.x) { lr.enabled = false; return; }
                    if (facing == -1f && mousePo.x > Puppet.RB2["Head"].position.x) { lr.enabled = false; return; }

                    //  If mouse is at edge of screen lets zoom out
                    if (Input.mousePosition.y >= Screen.height * 0.95f ||
                        Input.mousePosition.x >= Screen.width * 0.95f  ||
                        Input.mousePosition.x <= 10f || Input.mousePosition.y <= 10f)
                    {
                        if (!CustomCamera) {
                            Global.main.CameraControlBehaviour.enabled = false;
                            CustomCamera = true;
                        }

                        
                        if (Global.main.camera.orthographicSize < 40f ) Global.main.camera.orthographicSize *= 1.01f;
                        
                        Vector3 camTarget = Global.main.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Global.CameraPosition.z));
                        Vector3 oldCamPos = Global.main.camera.transform.position;

                        camTarget.z = 0f;
                        Vector3 direction = camTarget - oldCamPos;

                        Global.CameraPosition += direction * Time.deltaTime;
                        Global.CameraPosition = Util.ClampPos(Global.CameraPosition);


                    }

                    ThrowEndPos          = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 _velocity    = (ThrowEndPos - ThrowStartPos)* power;

                    Vector2[] trajectory = Plot(ThrowStartPos, _velocity, 1000);

                    lr.positionCount     = trajectory.Length;

                    Vector3[] positions  = new Vector3[trajectory.Length];

                    int il               = positions.Length;

                    for (int i = 0; i < il; i++)
                    {
                        positions[i] = trajectory[i];
                    }

                    lr.SetPositions(positions);
                }

                return;
            }

            if (state == ThrowState.windup)
            {
                if (frame == 1)
                {
                    if (CustomCamera) Global.main.CameraControlBehaviour.enabled = true;
                    AP_Throw2.RunMove();
                }
                if (frame > 4)
                {
                    float armForce = (ThrowEndPos - ThrowStartPos).magnitude;

                    Puppet.PBO.OverridePoseIndex = -1;

                    Puppet.RB2["UpperArmFront"].AddTorque(Mathf.Clamp(armForce, 5f, 10f) * Puppet.TotalWeight * -facing);
                }
                if (frame > 9)
                {
                    Puppet.DropThing();
                }
                if (frame > 10)
                {
                    heldThing.R.interpolation = RigidbodyInterpolation2D.Extrapolate;
                    heldThing.R.velocity      = Vector2.zero;

                    frame = 0;
                    state = ThrowState.fire;
                }
                return;
            }


            if (state == ThrowState.fire)
            {
                if (KB.Modifier) {
                    Util.MaxPayne(true);
                    PuppetMaster.isMaxPayne = true;
                }

                PuppetMaster.ChaseCam.QuickChase(heldThing.P);

                Puppet.PBO.OverridePoseIndex    = 0;
                lr.enabled                      = false;
                state                           = ThrowState.ready;
                Puppet.BlockMoves               = false;

                Vector2 _velocity               = (ThrowEndPos - ThrowStartPos) * power;

                heldThing.R.velocity            = _velocity;

                heldThing.JustThrown();
            }
        }

        //
        // ─── THROW Plot ────────────────────────────────────────────────────────────────
        //
        public Vector2[] Plot(Vector2 pos, Vector2 velocity, int steps)
        {
            Vector2[] results    = new Vector2[steps];
            float timestep       = Time.fixedDeltaTime / Physics2D.velocityIterations;
            Vector2 gravityAccel = Physics2D.gravity * initGravity * timestep * timestep;
            //float drag           = 1f - timestep * initDrag;
            Vector2 moveStep     = velocity * timestep;
            RaycastHit2D hit;
            int i;
            for (i = 0; i < steps; i++)
            {
                moveStep += gravityAccel;

                pos += moveStep;
                if (i > 70) results[i - 71] = pos;
                if (i > 120)
                {
                    hit = Physics2D.Raycast(pos, (pos + moveStep).normalized, 0.5f);

                    if (hit.transform)
                    {

                        Array.Resize(ref results, i - 71);

                        return results;
                    }

                }



            }
            Array.Resize(ref results, i - 71);
            return results;
        }

        private void InitLine()
        {
            if (lr != null) return;

            lr = Puppet.PBO.gameObject.GetOrAddComponent<LineRenderer>();

            lr.enabled           = false;
            lr.startColor        = new Color(1f, 0.3f, 0f, 0.05f);
            lr.endColor          = new Color(1f, 0.4f, 0f, 0.05f);
            lr.startWidth        = 0.03f;
            lr.endWidth          = 0.01f;
            lr.numCornerVertices = 0;
            lr.numCapVertices    = 0;
            lr.useWorldSpace     = true;
            lr.alignment         = LineAlignment.View;
            lr.sortingOrder      = 2;
            lr.material          = ModAPI.FindMaterial("Sprites-Default");
            lr.textureMode       = LineTextureMode.RepeatPerSegment;
            lr.hideFlags         = HideFlags.HideAndDontSave;
        }
    }
}
