//     ___                        _                    _     
//    / _ \_   _ _ __  _ __   ___| |_  /\/\   __ _ ___| |_ ___ _ __ 
//   / /_)/ | | | '_ \| '_ \ / _ \ __|/    \ / _` / __| __/ _ \ '__|
//  / ___/| |_| | |_) | |_) |  __/ |_/ /\/\ \ (_| \__ \ ||  __/ |  
//  \/     \__,_| .__/| .__/ \___|\__\/    \/\__,_|___/\__\___|_|  
//              |_|   |_|                
//                                           
using UnityEngine;
using System.Collections.Generic;

namespace PuppetMaster
{
    public enum JumpState
    {
        ready,
        start,
        launch,
        goingUp,
        goingDown,
        gPoundStart,
        gPoundDown,
        sonicBoom,
        swordDown,
        swordKill,
        attackDown,
        landed,
    };

    public class Jump
    {
        public ParticleSystem SmokeParticleSystem;

        public GameObject SmokeParticlePrefab;
        public GameObject Glow;
        public int frame              = 0;
        public JumpState state        = JumpState.ready;
        public Puppet Puppet          = null;
        public float jumpStrength     = 0;
        public float torque           = 0;
        public float facing           = 1;
        public float sonicForce       = 0;
        public float power            = 0;
        public float timeRecover      = 0;
        private MoveSet ActionPose;
        private float bodyRotation    = 0f;
        private Vector3 euler;
        private Vector3 diff;
        private Vector3 angleVelocity;
        private bool gripAltHand      = false;
        private Collider2D altHandCollider;
        private TrailRenderer trailRenderer;
        private float miscFloat = 0;
        private List<PersonBehaviour> Enemies = new List<PersonBehaviour>();
        private List<Rigidbody2D> Skulls      = new List<Rigidbody2D>();

        public void Init()
        {
            if (state == JumpState.ready)
            {
                Enemies.Clear();
                if (!Puppet.LB["Foot"].IsOnFloor && !Puppet.LB["FootFront"].IsOnFloor) return;
                if (Puppet.JumpLocked || Puppet.DisabledMoves) return;

                //  Make sure player is not holding up from something previously executed
                if (Time.time - KB.KeyTimes.Up > 1f) return;

                ActionPose = new MoveSet("jump", false);

                ActionPose.Ragdoll.ShouldStandUpright       = true;
                ActionPose.Ragdoll.State                    = PoseState.Rest;
                ActionPose.Ragdoll.Rigidity                 = 1.3f;
                ActionPose.Ragdoll.ShouldStumble            = false;
                ActionPose.Ragdoll.AnimationSpeedMultiplier = 0.5f;
                ActionPose.Ragdoll.UprightForceMultiplier   = 2f;
                ActionPose.Import();

                ActionPose.RunMove();

                jumpStrength                 = 0;
                state                        = JumpState.start;
                frame                        = 0;
                torque                       = 0;
                facing                       = Puppet.FacingLeft ? -1 : 1;

                Puppet.BlockMoves  = true;
                Puppet.pauseAiming = true;
            }
        }



        //
        // ─── JUMP GO ────────────────────────────────────────────────────────────────
        //
        public void Go()
        {

            if (Puppet.Actions.attack.CurrentAttack == Attack.AttackIds.dislodgeIchi)
            {
                state = JumpState.ready;
                Puppet.BlockMoves = false;
                return;
            }

            frame++;

            Vector2 direction  = new Vector2(KB.Right ? 3 : KB.Left ? -3 : 1 * facing, 10);
            float keyDirection = KB.Left ? 1f : (KB.Right ? -1f : 0);

            if ((KB.Left && facing < 0) || (KB.Right && facing > 0)) direction.x *= 2;
            //else if  direction.x *= 2;

            //  = = = = = = = = = = = = =
            //  START
            //  - - - - - - - - - - - - -
            if (state == JumpState.start)
            {

                if (KB.Up)
                {
                    jumpStrength += 0.01f;
                }
                else
                {
                    if (jumpStrength <= 0f)
                    {
                        state             = JumpState.ready;
                        Puppet.BlockMoves = false;

                        return;
                    }

                    if (jumpStrength > 3.5f) jumpStrength = 3.5f;

                    state = JumpState.launch;
                    frame = 0;
                    Puppet.Invincible(true);
                    return;

                }
            }

            //  = = = = = = = = = = = = =
            //  LAUNCH
            //  - - - - - - - - - - - - -
            if (state == JumpState.launch)
            {

                if (!Puppet.LB["Foot"].IsOnFloor && !Puppet.LB["FootFront"].IsOnFloor)
                {
                    ActionPose.ClearMove();
                    Puppet.BlockMoves            = false;
                    state                        = JumpState.ready;

                    return;
                }


                // Puppet.PBO.OverridePoseIndex = -1;

                if (KB.Left || KB.Right)
                {
                    torque = keyDirection * 10 * Puppet.TotalWeight * jumpStrength * 2f;
                    Puppet.RB2["MiddleBody"].AddTorque(torque, ForceMode2D.Force);
                    Puppet.RB2["UpperBody"].AddTorque(torque, ForceMode2D.Force);
                    Puppet.RB2["LowerBody"].AddTorque(torque, ForceMode2D.Force);
                }

                Puppet.RB2["Head"].AddRelativeForce(direction * (20 * (jumpStrength * Puppet.TotalWeight)));
                Puppet.RB2["UpperBody"].AddRelativeForce(direction * (20 * (jumpStrength * Puppet.TotalWeight)));
                Puppet.RB2["LowerBody"].AddRelativeForce(direction * (20 * (jumpStrength * Puppet.TotalWeight)));
                Puppet.RB2["Foot"].AddRelativeForce(direction * (20 * (jumpStrength * Puppet.TotalWeight)));
                Puppet.RB2["FootFront"].AddRelativeForce(direction * (20 * (jumpStrength * Puppet.TotalWeight)));

                if (KB.Left)
                {
                    Puppet.RB2["Head"].AddForce(Vector2.left * jumpStrength * Puppet.TotalWeight * 50);
                    Puppet.RB2["Foot"].AddForce(Vector2.right * jumpStrength * Puppet.TotalWeight * 50);
                    Puppet.RB2["FootFront"].AddForce(Vector2.right * jumpStrength * Puppet.TotalWeight * 50);
                }
                if (KB.Right)
                {
                    Puppet.RB2["Head"].AddForce(Vector2.right * jumpStrength * Puppet.TotalWeight * 50);
                    Puppet.RB2["Foot"].AddForce(Vector2.left * jumpStrength * Puppet.TotalWeight * 50);
                    Puppet.RB2["FootFront"].AddForce(Vector2.left * jumpStrength * Puppet.TotalWeight * 50);
                }
                state = JumpState.goingUp;
                frame = 0;
                return;
            }

            //  = = = = = = = = = = = = =
            //  GOING UP
            //  - - - - - - - - - - - - -
            if (state == JumpState.goingUp)
            {
                if (KB.Down)
                {
                    frame = 0;
                    state = JumpState.gPoundStart;
                    return;
                }

                if (frame == 1)
                {
                    ActionPose = new MoveSet("jump_spin", false);
                    ActionPose.Ragdoll.ShouldStandUpright       = false;
                    ActionPose.Ragdoll.State                    = PoseState.Protective;
                    ActionPose.Ragdoll.Rigidity                 = 2.3f;
                    ActionPose.Ragdoll.ShouldStumble            = false;
                    ActionPose.Ragdoll.AnimationSpeedMultiplier = 2.5f;
                    ActionPose.Ragdoll.UprightForceMultiplier   = 0f;
                    ActionPose.Import();
                }

                if (KB.Action2Held || (Puppet.Actions.combatMode && KB.Mouse2Down))
                {
                    if ((bool)Puppet?.HoldingF.canStab)
                    {
                        frame = 0;
                        state = JumpState.swordDown;
                        return;
                    }
                }
                if (KB.ActionHeld || (Puppet.Actions.combatMode && KB.MouseDown))
                {
                    if ((bool)Puppet?.HoldingF.canStab)
                    {
                        frame = 0;
                        state = JumpState.attackDown;
                        return;
                    }
                }

                ActionPose.RunMove();

                if (frame < 10)
                {
                    Puppet.RB2["LowerBody"].AddForce(direction * (jumpStrength * Puppet.TotalWeight));
                }

                if (frame == 10) {
                    state = JumpState.goingDown;
                    frame = 0;
                    return;
                }

                if (KB.Left || KB.Right)
                {
                    torque = keyDirection * 10 * Puppet.TotalWeight * jumpStrength * 0.32f;
                    Puppet.RB2["MiddleBody"].AddTorque(torque, ForceMode2D.Force);
                    Puppet.RB2["UpperBody"].AddTorque(torque, ForceMode2D.Force);
                    Puppet.RB2["LowerBody"].AddTorque(torque, ForceMode2D.Force);
                }
            }

            //  = = = = = = = = = = = = =
            //  GOING DOWN
            //  - - - - - - - - - - - - -
            if (state == JumpState.goingDown)
            {
                if (KB.Down)
                {
                    frame = 0;
                    state = JumpState.gPoundStart;
                    return;
                }
                
                if (KB.Action2Held || (Puppet.Actions.combatMode && KB.Mouse2Down))
                {
                    if ((bool)Puppet?.HoldingF.canStab)
                    {
                        frame = 0;
                        state = JumpState.swordDown;
                        return;
                    }
                }
                if (KB.ActionHeld || (Puppet.Actions.combatMode && KB.MouseDown))
                {
                    if ((bool)Puppet?.HoldingF.canStab)
                    {
                        frame = 0;
                        state = JumpState.attackDown;
                        return;
                    }
                }

                if (Puppet.PBO.IsTouchingFloor)
                {
                    state = JumpState.landed;
                    return;
                }

                if (KB.Left || KB.Right)
                {
                    if (jumpStrength == 0) jumpStrength = 1f;

                    keyDirection = KB.Left ? 1f : (KB.Right ? -1f : 0);
                    torque       = keyDirection * 10 * Puppet.TotalWeight * jumpStrength * 0.32f;

                    Puppet.RB2["MiddleBody"].AddTorque(torque, ForceMode2D.Force);
                    Puppet.RB2["UpperBody"].AddTorque(torque, ForceMode2D.Force);
                    Puppet.RB2["LowerBody"].AddTorque(torque, ForceMode2D.Force);
                }
            }

            //  = = = = = = = = = = = = =
            //  GROUND POUND START
            //  - - - - - - - - - - - - -
            if (state == JumpState.gPoundStart)
            {
                sonicForce += Time.fixedDeltaTime * 2;

                if (frame == 1)
                {
                    SetupSmoke();
                    sonicForce = 0;
                    euler      = new Vector3(0,0,0);
                    ActionPose = new MoveSet("groundpound_1", false);

                    ActionPose.Ragdoll.ShouldStandUpright       = false;
                    ActionPose.Ragdoll.State                    = PoseState.Rest;
                    ActionPose.Ragdoll.Rigidity                 = 2.3f;
                    ActionPose.Ragdoll.ShouldStumble            = false;
                    ActionPose.Ragdoll.AnimationSpeedMultiplier = 1.5f;
                    ActionPose.Ragdoll.UprightForceMultiplier   = 0f;
                    ActionPose.Import();

                    ActionPose.RunMove();

                    Puppet.RB2["MiddleBody"].inertia = 0.11125f;
                    //Global.main.CameraControlBehaviour.SendMessage()
                    //Global.main.CameraControlBehaviour.SendMessage("ZoomCamera", -1, SendMessageOptions.DontRequireReceiver);

                    //if (Global.main.camera.orthographicSize < 40f) Global.main.camera.orthographicSize *= 1.01f;
                    //ChaseCamX.lockedOn    = true;
                    //ChaseCamX.isMoving    = true;
                    //ChaseCamX.yOffsetTemp = -1.5f;
                    //ChaseCamX.yOffsetTemp = ChaseCamX.yOffset < 0f ? 0f : (ChaseCamX.yOffset / 2f);


                }

                if (!KB.Down)
                {
                    ChaseCamX.shakePower = 0.0f;
                    ActionPose.ClearMove();
                    state = JumpState.goingDown;
                    frame = 0;
                    Effects.DoTrail(Puppet.RB2["UpperBody"], true);
                    return;
                }


                Vector2 ubody = Puppet.RB2["UpperBody"].velocity;
                ubody.x *= 0.1f;
                Puppet.RB2["UpperBody"].velocity = ubody;

                if (ubody.y < -0.5f)
                {
                    frame = 0;
                    state = JumpState.gPoundDown;
                    return;
                } 
                else if (ubody.y < 0.5f)
                {
                    diff = Vector3.right;
                    angleVelocity = new Vector3(0f, 0f, (Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg));
                }
                else
                {
                    diff = Vector3.right;
                    angleVelocity = new Vector3(0f, 0f, (Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg));
                }

                Puppet.RB2["MiddleBody"].MoveRotation(
                    Quaternion.RotateTowards(
                        Puppet.RB2["MiddleBody"].transform.rotation, Quaternion.Euler(angleVelocity * facing), 5f));

                if (Puppet.PBO.IsTouchingFloor)
                {
                    state = JumpState.landed;
                    return;
                }

            }


            //  = = = = = = = = = = = = =
            //  GROUND POUND DOWN
            //  - - - - - - - - - - - - -
            if (state == JumpState.gPoundDown)
            {
                if (frame == 1)
                {
                    ChaseCamX.shakePower  = 0.01f;
                    ChaseCamX.CustomMode  = ChaseCamX.CustomModes.GroundPound;

                    Puppet.Invincible(true);

                    ActionPose = new MoveSet("groundpound_2", false);

                    ActionPose.Ragdoll.ShouldStandUpright       = false;
                    ActionPose.Ragdoll.State                    = PoseState.Rest;
                    ActionPose.Ragdoll.Rigidity                 = 2.3f;
                    ActionPose.Ragdoll.ShouldStumble            = false;
                    ActionPose.Ragdoll.AnimationSpeedMultiplier = 2.5f;
                    ActionPose.Ragdoll.UprightForceMultiplier   = 0f;
                    ActionPose.Import();

                    ActionPose.RunMove();

                    Puppet.HoldingF?.DisableSelfDamage(true);
                }


                if (frame == 10) trailRenderer = Effects.DoTrail(Puppet.RB2["UpperBody"]);

                if (!KB.Down)
                {
                    ChaseCamX.shakePower = 0.0f;
                    ActionPose.ClearMove();
                    state = JumpState.goingDown;
                    frame = 0;
                    Effects.DoTrail(Puppet.RB2["UpperBody"], true);
                    return;

                }

                ChaseCamX.shakePower = Mathf.Clamp(ChaseCamX.shakePower * 1.022f, 0f, 0.2f);

                Vector2 ubody = Puppet.RB2["UpperBody"].velocity;
                

                sonicForce += Time.fixedDeltaTime * 2;

                Puppet.RunRigids(Puppet.RigidAddMass, 1.01f);

                diff          = Vector3.right + Vector3.down;
                
                //miscFloat = Mathf.Lerp(miscFloat, )

                angleVelocity = new Vector3(0f, 0f, (Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg * facing ));

                Puppet.RB2["MiddleBody"].MoveRotation(Quaternion.RotateTowards(
                 Puppet.RB2["MiddleBody"].transform.rotation, Quaternion.Euler(angleVelocity), 3f));

                if (Puppet.PBO.IsTouchingFloor)
                {
                    if (ubody.y > -1f)
                    {
                        ChaseCamX.shakePower = 0.0f;
                        state                = JumpState.sonicBoom;
                        frame                = 0;
                        Puppet.RunRigids(Puppet.RigidStop);
                        return;
                    }
                }

                ubody.y = Mathf.Clamp(ubody.y *= 1.25f, -100f, 100.5f);

                Puppet.RB2["UpperBody"].velocity = ubody;
            }

            //  = = = = = = = = = = = = =
            //  SONIC BOOM
            //  - - - - - - - - - - - - -
            if (state == JumpState.sonicBoom)
            {
                if (frame == 1)
                {
                    ChaseCamX.CustomMode = ChaseCamX.CustomModes.SonicBoom;
                    Puppet.RunLimbs(Puppet.LimbGhost, true);

                    trailRenderer.emitting = false;

                    Vector2 effectPosition = Puppet.RB2["LowerLegFront"].transform.position;
                    effectPosition.y -= 0.5f;

                    ModAPI.CreateParticleEffect("IonExplosion", effectPosition);

                    Effects.DoNotKill.AddRange(Puppet.PBO.transform.root.GetComponentsInChildren<Collider2D>());

                    Effects.DoPulseExplosion(Puppet.RB2["LowerLegFront"].transform.position, sonicForce, sonicForce * 2, true,true);

                    SmokeParticleSystem.Emit(5 * (int)sonicForce);

                    ChaseCamX.shakePower = 0.0f;
                    ActionPose           = new MoveSet("groundpound_3", true);

                    //ActionPose.Ragdoll.ShouldStandUpright       = false;
                    //ActionPose.Ragdoll.State                    = PoseState.Rest;
                    //ActionPose.Ragdoll.Rigidity                 = 2.3f;
                    //ActionPose.Ragdoll.ShouldStumble            = true;
                    //ActionPose.Ragdoll.AnimationSpeedMultiplier = 1.5f;
                    //ActionPose.Ragdoll.UprightForceMultiplier   = 0f;
                    //ActionPose.Import();

                    ActionPose.RunMove();
                    timeRecover        = Time.time + 2f;

                    PuppetMaster.Master.AddTask(Puppet.HoldingF.DisableSelfDamage,2f,false);

                    //ChaseCamX.lockedOn = false;
                }

                if (Time.time > timeRecover)
                {
                    Puppet.RunLimbs(Puppet.LimbGhost, false);

                    Effects.DoTrail(Puppet.RB2["UpperBody"], true);

                    state = JumpState.landed;
                    return;
                }
            }


            //  = = = = = = = = = = = = =
            //  SWORD DOWN
            //  - - - - - - - - - - - - -
            if (state == JumpState.swordDown)
            {
                if (frame == 1)
                {
                    if (Puppet.GripB == null) {
                        gripAltHand     = true;
                        if (!Puppet.RB2["LowerHand"].TryGetComponent<Collider2D>(out altHandCollider))
                            altHandCollider = null;
                    }

                    Puppet.Actions.attack.CurrentMove           = Attack.MoveTypes.stab;

                    power                                       = 0;
                    ActionPose                                  = new MoveSet("jumpsword_1", false);
                    ActionPose.Ragdoll.ShouldStandUpright       = false;
                    ActionPose.Ragdoll.State                    = PoseState.Rest;
                    ActionPose.Ragdoll.Rigidity                 = 2.3f;
                    ActionPose.Ragdoll.ShouldStumble            = false;
                    ActionPose.Ragdoll.AnimationSpeedMultiplier = 2.5f;
                    ActionPose.Ragdoll.UprightForceMultiplier   = 0f;
                    ActionPose.Import();
                    ActionPose.RunMove();

                    Puppet.RB2["MiddleBody"].inertia = 0.11125f;

                    Puppet.LB["LowerArmFront"].SendMessage("Use", (object)new ActivationPropagation(), SendMessageOptions.DontRequireReceiver);

                    Puppet.HoldingF.ResetPosition(false, true);

                    //ChaseCamX.lockedOn    = true;
                    //ChaseCamX.isMoving    = true;
                    ChaseCamX.yOffsetTemp = -5.5f;


                }

                if (frame == 2)
                {
                    if (Puppet.HoldingF.P == null) Puppet.DropThing();
                }

                if (gripAltHand) 
                {
                    if (altHandCollider != null)
                    {
                        for (int i=Puppet.HoldingF.ItemColliders.Length; --i >= 0;) 
                        { 
                            if (gripAltHand && altHandCollider.IsTouching(Puppet.HoldingF.ItemColliders[i]))
                            {
                                gripAltHand = false;
                                Puppet.HoldingF.P.MakeWeightful();
                                Puppet.LB["LowerArm"].SendMessage("Use", (object)new ActivationPropagation(), SendMessageOptions.DontRequireReceiver);

                            }

                        }
                    } else gripAltHand = false;
                }

                power                       += 1f;
                Puppet.HoldingF.AttackDamage = power;

                diff          = Vector3.down;
                angleVelocity = new Vector3(0f, 0f, (Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg));
                
                
                Puppet.RB2["MiddleBody"].MoveRotation(
                    Quaternion.RotateTowards(
                        Puppet.RB2["MiddleBody"].transform.rotation, Quaternion.Euler(angleVelocity * facing), 10f));

                if (Puppet.PBO.IsTouchingFloor)
                {
                    state = JumpState.landed;
                    return;
                }

                if (KB.Left) Puppet.RB2["UpperBody"].AddForce(Vector2.left * Puppet.TotalWeight * 5f);
                if (KB.Right) Puppet.RB2["UpperBody"].AddForce(Vector2.right * Puppet.TotalWeight * 5f);

            }


            //  = = = = = = = = = = = = =
            //  ATTACK DOWN
            //  - - - - - - - - - - - - -
            if (state == JumpState.attackDown)
            {
                if (frame == 1)
                {
                    if (Puppet.HoldingF.IsFlipped != Puppet.IsFlipped)
                    {
                        Puppet.LB["LowerArmFront"].SendMessage("Use", (object)new ActivationPropagation(), SendMessageOptions.DontRequireReceiver);
                        Puppet.HoldingF.ResetPosition();
                    }
                    if (Puppet.HoldingF.isShiv)
                    {
                        frame = 0;
                        state = JumpState.swordDown;
                        return;
                    }
                    Puppet.HoldingF.HitLimb = null;
                    FindEnemies();
                    if (Puppet.GripB == null)
                    {
                        gripAltHand = true;
                        if (!Puppet.RB2["LowerHand"].TryGetComponent<Collider2D>(out altHandCollider))
                            altHandCollider = null;
                    }

                    Puppet.Actions.attack.CurrentMove = Attack.MoveTypes.stab;

                    power = 0;
                    ActionPose = new MoveSet("jumpsword_2", false);
                    ActionPose.Ragdoll.ShouldStandUpright = false;
                    ActionPose.Ragdoll.State = PoseState.Rest;
                    ActionPose.Ragdoll.Rigidity = 2.3f;
                    ActionPose.Ragdoll.ShouldStumble = false;
                    ActionPose.Ragdoll.AnimationSpeedMultiplier = 2.5f;
                    ActionPose.Ragdoll.UprightForceMultiplier = 0f;
                    ActionPose.Import();
                    ActionPose.RunMove();

                    Puppet.RB2["MiddleBody"].inertia = 0.11125f;

                    //Puppet.LB["LowerArmFront"].SendMessage("Use", (object)new ActivationPropagation(), SendMessageOptions.DontRequireReceiver);

                    //Puppet.HoldingF.ResetPosition(false, true);

                    //ChaseCamX.lockedOn = true;
                    //ChaseCamX.isMoving = true;
                    //ChaseCamX.yOffsetTemp = -5.5f;


                }


                if (gripAltHand)
                {
                    if (altHandCollider != null)
                    {
                        for (int i = Puppet.HoldingF.ItemColliders.Length; --i >= 0;)
                        {
                            if (gripAltHand && altHandCollider.IsTouching(Puppet.HoldingF.ItemColliders[i]))
                            {
                                gripAltHand = false;
                                Puppet.HoldingF.P.MakeWeightful();
                                Puppet.LB["LowerArm"].SendMessage("Use", (object)new ActivationPropagation(), SendMessageOptions.DontRequireReceiver);

                            }

                        }
                    }
                    else gripAltHand = false;
                }

                power += 1f;
                Puppet.HoldingF.AttackDamage = power;

                diff = Vector3.left;
                angleVelocity = new Vector3(0f, 0f, (Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg));


                Puppet.RB2["MiddleBody"].MoveRotation(
                    Quaternion.RotateTowards(
                        Puppet.RB2["MiddleBody"].transform.rotation, Quaternion.Euler(angleVelocity * facing), 10f));

                if (Puppet.PBO.IsTouchingFloor)
                {
                    state = JumpState.landed;
                    return;
                }

                if (KB.Left)  Puppet.RB2["UpperBody"].AddForce(Vector2.left * Puppet.TotalWeight * 5f);
                else if (KB.Right) Puppet.RB2["UpperBody"].AddForce(Vector2.right * Puppet.TotalWeight * 5f);
                else
                {
                    Rigidbody2D enemyRB = ClosestSkull();

                    if (enemyRB != null)
                    {
                        Vector2 toEnemy = Puppet.HoldingF.R.position - enemyRB.position;
                        toEnemy.y       = 0f;
                        Puppet.RB2["UpperBody"].AddForce(toEnemy * -facing * Puppet.TotalWeight * 1.5f * toEnemy.magnitude * Time.deltaTime);
                        //Puppet.HoldingF.R.AddForce(toEnemy * facing * Puppet.TotalWeight * 3f * toEne  );
                        //if (toEnemy.magnitude < 0.05f) Puppet.RB2["UpperBody"].velocity *= 0.1f;
                        //if (toEnemy.magnitude < 0.1f) Puppet.RB2["UpperBody"].velocity *= 0.1f;
                    }
                }

                if (Puppet.HoldingF.HitLimb != null)
                {
                    Puppet.HoldingF.HitLimb.Slice();
                    state = JumpState.swordKill;
                    frame = 0;
                    return;
                }

            }


            //  = = = = = = = = = = = = =
            //  SWORD KILL
            //  - - - - - - - - - - - - -
            if (state == JumpState.swordKill)
            {

                if (frame == 1)
                {

                    ChaseCamX.shakePower  = 0.0f;
                    ChaseCamX.yOffsetTemp = 0.0f;

                    Puppet.JumpLocked = false;
                    state             = JumpState.ready;
                    frame             = 0;
                    Puppet.BlockMoves = false;

                    ActionPose.ClearMove();

                    Puppet.RunRigids(Puppet.BodyInertiaFix);
                    Puppet.RunRigids(Puppet.RigidMass, -1f);

                    if (jumpStrength < 0.2f) jumpStrength = 0.2f;

                    Puppet.DisableMoves = Time.time + jumpStrength / 2;

                    PuppetMaster.Master.AddTask(Puppet.Invincible,1f,false);

                    Util.MaxPayne(false);

                }

                diff = Vector3.up;
                angleVelocity = new Vector3(0f, 0f, (Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg));


                Puppet.RB2["MiddleBody"].MoveRotation(
                    Quaternion.RotateTowards(
                        Puppet.RB2["MiddleBody"].transform.rotation, Quaternion.Euler(angleVelocity * facing), 10f));

                if (frame >= 20)
                {
                    state = JumpState.landed;
                }

            }

            //  = = = = = = = = = = = = =
            //  LANDED
            //  - - - - - - - - - - - - -
            if (state == JumpState.landed)
            {
                ChaseCamX.shakePower  = 0.0f;
                ChaseCamX.yOffsetTemp = 0.0f;

                Puppet.JumpLocked    = false;
                state                = JumpState.ready;
                frame                = 0;
                Puppet.BlockMoves    = false;

                ActionPose.ClearMove();

                Puppet.RunRigids(Puppet.BodyInertiaFix);
                Puppet.RunRigids(Puppet.RigidMass,-1f);

                if (jumpStrength < 0.2f) jumpStrength = 0.2f;

                Puppet.DisableMoves = Time.time + jumpStrength / 2;

                PuppetMaster.Master.AddTask(Puppet.Invincible, 1f, false);

                Util.MaxPayne(false);

                Puppet.pauseAiming = false;
            }
        }

        public void SetupSmoke()
        {
            SmokeParticlePrefab = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/SmokeParticle"), Puppet.RB2["LowerLegFront"].transform.position, Quaternion.identity, Puppet.RB2["LowerLegFront"].transform);
            SmokeParticleSystem = SmokeParticlePrefab.GetComponent<ParticleSystem>();
            SmokeParticlePrefab.AddComponent<Optout>();
            
            ParticleSystem.ShapeModule shape = SmokeParticleSystem.shape;
            shape.radiusSpeedMultiplier      = 5f;
            shape.radiusThickness            = 0.51f;
            shape.spriteRenderer             = Puppet.RB2["LowerLegFront"].GetComponent<SpriteRenderer>();
            shape.radiusSpread               = 15f;
            shape.radiusSpeed                = 0.001f;
            shape.radiusMode                 = ParticleSystemShapeMultiModeValue.Random;
            shape.shapeType                  = ParticleSystemShapeType.Hemisphere;
            shape.radius                    *= 6f;
            
        }
        

        //
        // ─── CHECK FOR ENEMY ────────────────────────────────────────────────────────────────
        //
        public void FindEnemies()
        {
            Skulls.Clear();
            Enemies.AddRange(GameObject.FindObjectsOfType<PersonBehaviour>());

            for (int i = Enemies.Count; --i >= 0;)
            {
                if (Enemies[i].IsAlive() && Enemies[i] != Puppet.PBO) {

                    Skulls.Add(Enemies[i].transform.GetChild(5).GetComponent<Rigidbody2D>());

                } else
                {
                    Enemies.RemoveAt(i);

                }
            }
        }

        public Rigidbody2D ClosestSkull()
        {
            if (Skulls.Count == 0) return null;

            float closestDistance = float.MaxValue;

            Rigidbody2D rb = null;

            for (int i = Skulls.Count; --i >= 0;)
            {
                float distance = Mathf.Abs((Skulls[i].position - Puppet.HoldingF.R.position).sqrMagnitude);

                if (distance < closestDistance)
                {
                    rb              = Skulls[i];
                    closestDistance = distance;
                }
            }

            return rb;

        }

        public bool CheckForEnemy()
        {
            //PersonBehaviour TheChosen   = (PersonBehaviour)null;
            PersonBehaviour[] people    = GameObject.FindObjectsOfType<PersonBehaviour>();



            //float floatemp1 = float.MaxValue;
            //
            //bool lastKnockedOut = false;

            foreach (PersonBehaviour person in people)
            {
                if (!person.isActiveAndEnabled || !person.IsAlive() || person == Puppet.PBO) continue;

                Vector2 Vectemp1 = person.transform.GetComponentInChildren<Rigidbody2D>().position;

                if (Vectemp1.y > Puppet.RB2["Head"].position.y) continue;
                if (Puppet.RB2["Head"].position.y - Vectemp1.y < 1f) continue;
                if (Mathf.Abs(Vectemp1.x - Puppet.RB2["Head"].position.x) > 1f) continue;

                return true;

                //float floatemp2 = (thing.R.position - Vectemp1).sqrMagnitude;

                //if (floatemp2 < floatemp1)
                //{
                //    floatemp1 = floatemp2;
                //    TheChosen = person;

                //    if (person.Consciousness < 1 || person.ShockLevel > 0.3f)
                //    {
                //        lastKnockedOut = true;
                //    }
                //}
                //else if (lastKnockedOut)
                //{
                //    if (person.Consciousness >= 1 && person.ShockLevel < 0.3f)
                //    {
                //        if (floatemp2 - floatemp1 < 1f)
                //        {
                //            lastKnockedOut = false;
                //            floatemp1 = floatemp2;
                //            TheChosen = person;
                //        }
                //    }
            }

            return false;

            //Enemy = TheChosen;

            //if (TheChosen == null)
            //{
            //    EnemyTarget = Puppet.RB2["UpperBody"].position + (Vector2.right * facing * 1.5f);
            //    return;

            }
        }
}
