//     ___                        _                    _     
//    / _ \_   _ _ __  _ __   ___| |_  /\/\   __ _ ___| |_ ___ _ __ 
//   / /_)/ | | | '_ \| '_ \ / _ \ __|/    \ / _` / __| __/ _ \ '__|
//  / ___/| |_| | |_) | |_) |  __/ |_/ /\/\ \ (_| \__ \ ||  __/ |  
//  \/     \__,_| .__/| .__/ \___|\__\/    \/\__,_|___/\__\___|_|  
//              |_|   |_|                
//                                           
using System.Collections.Generic;
using UnityEngine;

namespace PuppetMaster
{
    public enum AttackState
    {
        ready,
        windup,
        hit,
        final,
        wait,
    }

    public class Attack
    {
        public Puppet Puppet = null;
        public AttackState state      = AttackState.ready;
        public AttackIds CurrentAttack;
        public MoveTypes CurrentMove;
        public Thing thing;

        public PersonBehaviour Enemy;
        public Vector2 EnemyTarget;
        public Vector2 TargetPos;
        public float TargetAngle;
        public Dictionary<string, Rigidbody2D> XRB2 = new Dictionary<string, Rigidbody2D>();

        private float floatemp1;
        private float floatemp2;
        private float power      = 0f;
        private float facing     = 1f;
        private int frame        = 0;

        public MoveSet combatPose;
        private MoveSet MS_Attack_1;

        public enum AttackIds
        {
            club,
            thrust,
            thrustx,
            dislodgeKick,
            dislodgeBack,
            dislodgeIchi,
        }

        public enum MoveTypes
        {
            stab,
            slash,
        }

        private Vector2 Vectemp1;
        // private Vector2 Vectemp2;




        //
        // ─── TARGET ENEMY ────────────────────────────────────────────────────────────────
        //
        public void TargetEnemy()
        {
            PersonBehaviour TheChosen = (PersonBehaviour)null;
            PersonBehaviour[] people  = GameObject.FindObjectsOfType<PersonBehaviour>();
            floatemp1                 = float.MaxValue;

            bool lastKnockedOut       = false;

            foreach (PersonBehaviour person in people)
            {
                if (!person.isActiveAndEnabled || !person.IsAlive() || person == Puppet.PBO) continue;

                Vectemp1 = person.transform.GetComponentInChildren<Rigidbody2D>().position;

                if (Puppet.FacingLeft  && Vectemp1.x > Puppet.RB2["Head"].position.x) continue;
                if (!Puppet.FacingLeft && Vectemp1.x < Puppet.RB2["Head"].position.x) continue;

                floatemp2 = (thing.R.position - Vectemp1).sqrMagnitude;

                if (floatemp2 < floatemp1)
                {
                    floatemp1 = floatemp2;
                    TheChosen = person;

                    if (person.Consciousness < 1 || person.ShockLevel > 0.3f)
                    {
                        lastKnockedOut = true;
                    }
                } else if(lastKnockedOut)
                {
                    if (person.Consciousness >= 1 && person.ShockLevel < 0.3f)
                    {
                        if (floatemp2 - floatemp1 < 1f)
                        {
                            lastKnockedOut = false;
                            floatemp1      = floatemp2;
                            TheChosen      = person;
                        }
                    }
                }
            }

            Enemy = TheChosen;

            if (TheChosen == null) {
                EnemyTarget = Puppet.RB2["UpperBody"].position + (Vector2.right * facing * 1.1f);
                return;

            }

            string[] ValidTargets = new string[]
            {
                "Head",
                "UpperBody",
                "MiddleBody",
                "LowerBody",
                "UpperArm",
                "UpperArmFront",
                "LowerArm",
                "LowerArmFront",
            };

            string randomTarget = ValidTargets[UnityEngine.Random.Range(0,ValidTargets.Length - 1)];  
            

            XRB2.Clear();

            Rigidbody2D[] RBs = Enemy.transform.GetComponentsInChildren<Rigidbody2D>();


            foreach (Rigidbody2D rb in RBs)
            {
                XRB2.Add(rb.name, rb);
            }

            EnemyTarget = XRB2[ randomTarget ].position;

        }

        //
        // ─── ATTACK INIT ────────────────────────────────────────────────────────────────
        //
        public void Init(AttackIds attackId)
        {
            if (state != AttackState.ready) return;

            if (!Puppet.CanAttack) return;

            thing                   = Puppet.HoldingF;
            thing.P.ForceContinuous = true;
            thing.AttackDamage      = 0f;
            frame                   = 0;
            power                   = 0;
            state                   = AttackState.windup;
            facing                  = Puppet.FacingLeft ? -1 : 1;

            thing.AttackedList.Clear();

            CurrentAttack = attackId;

            if (attackId == AttackIds.club)
            {
                TargetEnemy();
            }

            else if (attackId == AttackIds.thrust)
            {
                TargetEnemy();
            }
            else if (attackId == AttackIds.dislodgeIchi)
            {
                
            }
           
        }


        //
        // ─── ATTACK GO ────────────────────────────────────────────────────────────────
        //
        public void Go()
        {
            frame++;
            if (KB.ActionHeld || KB.Action2Held || KB.MouseDown || KB.Mouse2Down) power++;

            if (CurrentAttack == AttackIds.club) MeleeClub();
            if (CurrentAttack == AttackIds.thrust) MeleeThrust();
            if (CurrentAttack == AttackIds.dislodgeBack) DislodgeBack();
            if (CurrentAttack == AttackIds.dislodgeKick) DislodgeKick();
            if (CurrentAttack == AttackIds.dislodgeIchi) DislodgeIchi();
        }



        // - - - - - - - - - - - - - - - - - -
        // : Identify X
        // - - - - - - - - - - - - - - - - - -
        public void DoCombatPose(bool enableCombat=true)
        {
            if (enableCombat) {
                if (combatPose == null) combatPose = new MoveSet("UpperArmFront:-11.68893:4, LowerArmFront:-96.55749:4");

                combatPose.CombineMove();
                if (!KB.isMouseDisabled)  KB.DisableMouse();
            } 
            else
            {
                combatPose.ClearMove();
                Puppet.Actions.combatMode = false;

                if (KB.isMouseDisabled) KB.EnableMouse();
            }

        }



        // - - - - - - - - - - - - - - - - - -
        //  MELEE:  CLUB
        // - - - - - - - - - - - - - - - - - -
        public void MeleeClub()
        {
            if (state == AttackState.windup)
            {
                if (frame <= 1)
                {
                    if (thing.isShiv) {
                        Puppet.LB["LowerArmFront"].SendMessage("Use", (object)new ActivationPropagation(), SendMessageOptions.DontRequireReceiver);
                        thing.ResetPosition(false, true);
                    } else
                    if (thing.IsFlipped != Puppet.IsFlipped)
                    {
                        Puppet.LB["LowerArmFront"].SendMessage("Use", (object)new ActivationPropagation(), SendMessageOptions.DontRequireReceiver);
                        thing.ResetPosition();
                    }

                    MS_Attack_1 = new MoveSet(@"
                        UpperArmFront:169.3435,
                        LowerArmFront:-43.21984", false);
                
                    MS_Attack_1.Ragdoll.ShouldStandUpright       = true;
                    MS_Attack_1.Ragdoll.State                    = PoseState.Rest;
                    MS_Attack_1.Ragdoll.Rigidity                 = 6.2f;
                    MS_Attack_1.Ragdoll.ShouldStumble            = false;
                    MS_Attack_1.Ragdoll.AnimationSpeedMultiplier = 11.5f;
                    MS_Attack_1.Ragdoll.UprightForceMultiplier   = 0f;
                    MS_Attack_1.Import();
                    if (!Puppet.IsWalking && !Puppet.IsCrouching && Puppet.Actions.prone.state == ProneState.ready) MS_Attack_1.RunMove();
                }
                
                if (Puppet.IsWalking || Puppet.IsCrouching || Puppet.Actions.prone.state != ProneState.ready) MS_Attack_1.CombineMove();

                float distance = Mathf.Abs(EnemyTarget.x - Puppet.RB2["UpperBody"].position.x);

                if (!Puppet.IsCrouching && !Puppet.IsWalking && Puppet.Actions.prone.state == ProneState.ready && distance < 3.5f && distance > 1.4f)
                {
                    Puppet.RB2["LowerLeg"].AddForce(Vector2.up + (Vector2.right * facing) * Puppet.TotalWeight * distance * 1.6f);
                    Puppet.RB2["UpperBody"].AddForce(Vector2.up + (Vector2.right * facing) * Puppet.TotalWeight * distance * 1.5f);
                    Puppet.RB2["Head"].AddForce(Vector2.up * Puppet.TotalWeight * distance * 2f);
                }

                if (!Puppet.IsWalking && distance < 1.9f && distance > 1.2f)
                {
                    Puppet.RB2["UpperBody"].velocity *= 0.001f;
                }

                if (!Puppet.IsCrouching && !Puppet.IsWalking && Puppet.Actions.prone.state == ProneState.ready && distance < 1.5f && distance > 1.2f)
                {
                    Puppet.RB2["LowerLeg"].AddForce(Vector2.up + (Vector2.right * -facing) * Puppet.TotalWeight * distance * 2f);
                    Puppet.RB2["UpperBody"].AddForce(Vector2.up + (Vector2.right * -facing) * Puppet.TotalWeight * distance * 2f);
                    Puppet.RB2["Head"].AddForce(Vector2.up * Puppet.TotalWeight * distance * 2f);
                    //Puppet.RB2["LowerArmFront"].inertia = 0.001f;
                    //Puppet.RB2["LowerArmFront"].inertia = 0.001f;
                    //thing.R.inertia = 0.01f;

                    thing.P.Temperature         = 30f;
                }


                if (frame == 1)
                {
                    CurrentMove = MoveTypes.slash;
                }


                if (!KB.ActionHeld && !KB.Action2Held && !KB.MouseDown && !KB.Mouse2Down)
                {
                    state = AttackState.hit;
                    frame = 0;
                }

                return;
            }

            if (state == AttackState.hit)
            {
                if (frame == 1)
                {
                    Puppet.RunRigids(Puppet.RigidInertia, -1f);

                    MS_Attack_1.ClearMove();

                    thing.AttackDamage = power;

                    //  Set attack damage power curve
                    if (power > 40) thing.AttackDamage = 40 - (power % 20);

                    Util.Notify("Damage: " + thing.AttackDamage, Util.VerboseLevels.Full);

                    Puppet.RunRigids(BodyInertia);

                    thing.R.AddForce(Vector2.up * Puppet.TotalWeight * 5f);

                    if (Puppet.LB["LowerArmFront"].SpeciesIdentity == "Android" && Puppet.IsCrouching)
                    {
                        Puppet.LB["LowerArmFront"].Broken = true;
                        Puppet.LB["UpperArmFront"].Broken = true;
                        thing.R.velocity *= 3;
                    }

                }

                if (frame == 2)
                {
                    if (!Puppet.IsCrouching) { 
                        thing.R.AddForce(((Vector2.right * facing) + (Vector2.down * 0.5f)) * Puppet.TotalWeight * thing.AttackDamage * 2);
                        thing.R.AddTorque(thing.AttackDamage * Puppet.TotalWeight * -facing);
                    }

                    Puppet.RB2["LowerArmFront"].AddForce(Vector2.right * facing * Puppet.TotalWeight * 10f);
                    Puppet.RB2["UpperBody"].AddForce(Vector2.right * -facing * Puppet.TotalWeight * 15f);
                    Puppet.RB2["UpperArmFront"].AddTorque(20f * -facing * Puppet.TotalWeight);
                }

                if (frame > 2)
                {
                    float distance = thing.R.position.x - EnemyTarget.x;

                    if (Mathf.Abs(distance) < 2f)
                    {
                        thing.R.mass    = 0.1f;
                        Vector2 target1 = (EnemyTarget - thing.R.position);
                        float force     = 0.5f;

                        thing.R.AddForceAtPosition(target1 * force, (Vector2)((Vector2)thing.tr.localPosition + (Vector2)(thing.R.transform.right * facing * thing.size)), ForceMode2D.Impulse);

                        Puppet.RB2["LowerArmFront"].AddForce(target1 * thing.AttackDamage * Puppet.TotalWeight);

                    } else
                    {
                        thing.R.mass    = 0.1f;
                        //Vector2 target1 = (EnemyTarget - thing.R.position).normalized;
                        Vector2 target1 = Vector2.right * facing;
                        float force     = 0.1f;

                        thing.R.AddForceAtPosition(target1 * force, (Vector2)((Vector2)thing.tr.localPosition + (Vector2)(thing.R.transform.right * thing.size)), ForceMode2D.Force);

                        Puppet.RB2["LowerArmFront"].AddForce(target1 * thing.AttackDamage * Puppet.TotalWeight);

                    }

                    if (Puppet.IsCrouching)
                    {
                        thing.P.MakeWeightful();
                        Puppet.RB2["LowerLeg"].velocity      *= 0.001f;
                        Puppet.RB2["LowerLegFront"].velocity *= 0.001f;
                        Puppet.RB2["Foot"].velocity          *= 0.001f;
                        Puppet.RB2["FootFront"].velocity     *= 0.001f;
                        thing.R.AddForce(Vector2.down * Puppet.TotalWeight * 15f);
                        Puppet.RB2["UpperBody"].AddForce((Vector2.down * 2) + (Vector2.right * facing) * Puppet.TotalWeight * 2f);
                        
                        Puppet.LB["UpperBody"].Broken  = true;
                        Puppet.LB["MiddleBody"].Broken = true;
                        Puppet.LB["LowerBody"].Broken  = true;
                    }
                    else
                    {
                        Puppet.RB2["UpperBody"].velocity *= 0.001f;
                        Puppet.RB2["LowerBody"].velocity *= 0.001f;
                    }

                }

                //if (frame == 4) thing.R.mass = 0.05f;

                if (frame > 10)
                {
                    state = AttackState.final;
                    frame = 0;
                }

                return;
            }

            if (state == AttackState.final)
            {
                thing.AttackDamage = 0f;
                thing.P.MakeWeightless();
                thing.P.Temperature = 30f;

                state = AttackState.ready;
                Puppet.RunRigids(BodyInertiaFix);

                if (!Puppet.IsCrouching && Puppet.Actions.prone.state == ProneState.ready) Puppet.PBO.OverridePoseIndex = -1;

                Puppet.LB["UpperBody"].Broken      = false;
                Puppet.LB["MiddleBody"].Broken     = false;
                Puppet.LB["LowerArmFront"].Broken  = false;
                Puppet.LB["UpperArmFront"].Broken  = false;
                Puppet.LB["LowerBody"].Broken      = false;

            }
        }

        // - - - - - - - - - - - - - - - - - -
        //  MELEE:  THRUST
        // - - - - - - - - - - - - - - - - - -
        public void MeleeThrust()
        {
            if (state == AttackState.windup)
            {
                if (frame <= 1)
                {
                    if (thing.isShiv)
                    {
                        Puppet.LB["LowerArmFront"].SendMessage("Use", (object)new ActivationPropagation(), SendMessageOptions.DontRequireReceiver);
                        thing.ResetPosition(false, false);
                    } else
                    if (thing.IsFlipped != Puppet.IsFlipped) {
                        Puppet.LB["LowerArmFront"].SendMessage("Use", (object)new ActivationPropagation(), SendMessageOptions.DontRequireReceiver);
                        thing.ResetPosition();
                    }

                    MS_Attack_1 = new MoveSet(@"
                            UpperArmFront:112.2979,
                            LowerArmFront:-69.02728", false);

                    MS_Attack_1.Ragdoll.ShouldStandUpright       = true;
                    MS_Attack_1.Ragdoll.State                    = PoseState.Rest;
                    MS_Attack_1.Ragdoll.Rigidity                 = 6.2f;
                    MS_Attack_1.Ragdoll.ShouldStumble            = false;
                    MS_Attack_1.Ragdoll.AnimationSpeedMultiplier = 11.5f;
                    MS_Attack_1.Ragdoll.UprightForceMultiplier   = 0f;
                    MS_Attack_1.Import();
                    if (!Puppet.IsWalking && !Puppet.IsCrouching) MS_Attack_1.RunMove();
                }

                if (Puppet.IsWalking || Puppet.IsCrouching || Puppet.Actions.prone.state != ProneState.ready) MS_Attack_1.CombineMove();

                if (frame == 1) CurrentMove = MoveTypes.stab;

                float distance = Mathf.Abs(EnemyTarget.x - Puppet.RB2["UpperBody"].position.x);

                if (!Puppet.IsCrouching && !Puppet.IsWalking && Puppet.Actions.prone.state == ProneState.ready && distance < 3.5f && distance > 1.1f)
                {
                    Puppet.RB2["LowerLeg"].AddForce(Vector2.up  + (Vector2.right * facing) * Puppet.TotalWeight * distance * 1.2f);
                    Puppet.RB2["UpperBody"].AddForce(Vector2.up + (Vector2.right * facing) * Puppet.TotalWeight * distance * 1.2f);
                    Puppet.RB2["Head"].AddForce(Vector2.up * Puppet.TotalWeight * distance * 2f);
                }
                if (!Puppet.IsWalking && distance < 1.1f)
                {
                    Puppet.RB2["UpperBody"].velocity *= 0.001f;
                }


                if (!KB.ActionHeld && !KB.Action2Held && !KB.MouseDown && !KB.Mouse2Down)
                {
                    state = AttackState.hit;
                    frame = 0;
                }

                return;
            }

            if (state == AttackState.hit)
            {
                if (frame == 1)
                {
                    MS_Attack_1.ClearMove();

                    thing.AttackDamage = power;

                    if (power > 30) thing.AttackDamage = 30 - (power % 20);

                    Util.Notify("Damage: " + thing.AttackDamage, Util.VerboseLevels.Full);

                    TargetPos    = (EnemyTarget - thing.R.position);
                    TargetAngle  = Vector2.SignedAngle(Vector2.right, TargetPos) - (thing.angleAim + (thing.angleOffset * facing));
                    floatemp1    = Mathf.Abs(EnemyTarget.x - Puppet.RB2["UpperBody"].position.x);
                    thing.R.mass = (floatemp1 > 3f || thing.canStab) ? 0.1f : 0.5f;
                }

                //Vector2 v = R.velocity;
                //float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
                //R.MoveRotation(angle + ((angleAim - (angleOffset * facing)) * -1f));

                thing.R.MoveRotation(TargetAngle);

                int frameCount = (Enemy == null || floatemp1 > 3f) ? 2 : UnityEngine.Random.Range(6, 8);

                if (frame >= 2 && frame <= 4)
                {
                    Puppet.RB2["UpperArmFront"].AddTorque(facing * Puppet.TotalWeight * 1.5f);
                    Puppet.RB2["LowerArmFront"].AddForce(Vector2.down * Puppet.TotalWeight * 1.5f);
                }

                if (frame >= 6 && frame <= 6 + frameCount)
                {
                    thing.R.AddForce((TargetPos.normalized + (Vector2.right * facing * Puppet.TotalWeight)) * thing.AttackDamage);
                    
                    Puppet.RB2["LowerArmFront"].AddForce((Vector2.down + (Vector2.right * facing)) * Puppet.TotalWeight);
                    Puppet.RB2["UpperBody"].velocity     *= 0.5f;

                    if (frame >= 8)
                    {
                        Vector2 vec      = thing.R.velocity;
                        vec.x           *= 1.5f;
                        thing.R.velocity = vec;
                    }
                }

                

                if (frame > 2)
                {
                    Puppet.RB2["LowerLeg"].velocity      *= 0f;
                    Puppet.RB2["LowerLegFront"].velocity *= 0f;
                }

                if (frame > 30)
                {
                    state = AttackState.final;
                    frame = 0;
                }

                return;
            }

            if (state == AttackState.final)
            {
                thing.AttackDamage = 0f;
                thing.P.MakeWeightless();

                state = AttackState.ready;
                Puppet.RunRigids(BodyInertiaFix);

                if (!Puppet.IsCrouching && Puppet.Actions.prone.state == ProneState.ready) Puppet.PBO.OverridePoseIndex = 0;
            }
        }


        // - - - - - - - - - - - - - - - - - -
        //  DISLODGE:  BACK
        // - - - - - - - - - - - - - - - - - -
        public void DislodgeBack()
        {
            if (state == AttackState.windup)
            {

                if (frame == 1)
                {
                    CurrentMove = MoveTypes.stab;
                }

                if ((!KB.ActionHeld && !KB.Action2Held && !KB.MouseDown && !KB.Mouse2Down) || power > 40)
                {
                    state = AttackState.hit;
                    frame = 0;
                }

                return;
            }

            if (state == AttackState.hit)
            {

                if (frame == 1)
                {
                    if (power < 20) power = 20;
                    thing.AttackDamage = power;

                    //  Set attack damage power curve
                    if (power > 40) thing.AttackDamage = 30 - (power % 40);
                }

                if (frame >= 2)
                {
                    thing.R.AddForce(Vector2.right * -facing * thing.AttackDamage * 2 * Puppet.TotalWeight);
                    
                    thing.CheckLodged();
                    if (!thing.isLodged) state = AttackState.final;
                }

                if (frame > 15)
                {
                    state = AttackState.final;
                    frame = 0;
                }

                return;
            }

            if (state == AttackState.final)
            {
                thing.AttackDamage = 0f;
                thing.P.MakeWeightless();

                state = AttackState.ready;

            }
        }


        // - - - - - - - - - - - - - - - - - -
        //  DISLODGE:  Kick
        // - - - - - - - - - - - - - - - - - -
        public void DislodgeKick()
        {
            if (state == AttackState.windup)
            {
                CurrentMove = MoveTypes.stab;

                if ((!KB.ActionHeld && !KB.Action2Held && !KB.MouseDown && !KB.Mouse2Down) || power > 40)
                {
                    state = AttackState.hit;
                    frame = 0;
                }

                return;
            }

            if (state == AttackState.hit)
            {

                if (frame == 1)
                {
                    TargetPos = (thing.StabVictim.rigidbody.position - Puppet.RB2["FootFront"].position);

                    if (power < 20) power = 20;

                    thing.AttackDamage = power;

                    if (power > 40) thing.AttackDamage = 30 - (power % 40);
                }

                Puppet.LB["UpperLegFront"].Broken = true;

                if (frame >= 2 && frame <= 7) Puppet.RB2["UpperLegFront"].AddTorque(30f * facing);
                

                if (frame == 10)
                {
                    if (thing.StabVictim != null)
                    {
                        thing.StabVictim.rigidbody.AddForce(Vector2.right * facing * thing.AttackDamage * 20);
                        Puppet.RB2["LowerBody"].AddForce(Vector2.right * -facing * thing.AttackDamage * 10);
                    }
                }

                if (frame > 15)
                {
                    state = AttackState.final;
                    frame = 0;
                }

                return;
            }

            if (state == AttackState.final)
            {
                thing.AttackDamage = 0f;
                thing.P.MakeWeightless();
                
                Puppet.LB["UpperLegFront"].Broken          = false;
                Puppet.LB["UpperLegFront"].Joint.useLimits = true;
                Puppet.LB["LowerLegFront"].Broken          = false;
                Puppet.LB["LowerLegFront"].Joint.useLimits = true;

                state = AttackState.ready;
            }
        }


        // - - - - - - - - - - - - - - - - - -
        //  DISLODGE:  Ichi The Killer
        // - - - - - - - - - - - - - - - - - -
        public void DislodgeIchi()
        {
            if (state == AttackState.windup)
            {
                if (frame <= 1)
                {
                    MS_Attack_1 = new MoveSet(@"
                                    LowerArmFront:-51.0253,
                                    UpperArmFront:-130.0736,
                                    LowerArm:-76.43639,
                                    UpperArm:-40.85823", true);
                }

                MS_Attack_1.RunMove();

                CurrentMove = MoveTypes.stab;

                if ((!KB.ActionHeld && !KB.Action2Held && !KB.MouseDown && !KB.Mouse2Down) || power > 20)
                {
                    state = AttackState.hit;
                    frame = 0;
                }

                return;
            }

            if (state == AttackState.hit)
            {
                MS_Attack_1.RunMove();
                if (frame == 1)
                {
                    PersonBehaviour Vic = thing.StabVictim.GetComponentInParent<PersonBehaviour>();
                    
                    if (Vic != null)
                    {
                        for (int i = Vic.Limbs.Length; --i >= 0;)
                        {
                            //  Make it rain body parts
                            if (Vic.Limbs[i].transform.position.y >= thing.HitLimb.transform.position.y)
                            {
                                Vic.Limbs[i].Slice();
                                Vic.Limbs[i].IsDismembered = true;

                                float x      = Random.Range(-1.0f,2.0f);
                                float y      = Random.Range(1.0f, 2.0f);
                                float force  = Random.Range(10f, 50f);

                                Vic.Limbs[i].PhysicalBehaviour.rigidbody.AddForce(new Vector2(x,y)*force);
                            }
                            
                            if (Vic.Limbs[i].gameObject != null) Vic.Limbs[i].gameObject.SetLayer(10);

                        }
                    }

                    if (power < 20) power = 20;

                    thing.AttackDamage = power;

                    if (power > 40) thing.AttackDamage = 30 - (power % 40);
                }

               
                if (frame > 15)
                {
                    MS_Attack_1.ClearMove();
                    state = AttackState.final;
                    frame = 0;
                }

                return;
            }

            if (state == AttackState.final)
            {
                thing.AttackDamage = 0f;
                thing.P.MakeWeightless();
                MS_Attack_1.ClearMove();

                state = AttackState.ready;
            }
        }

        public void BodyInertia(Rigidbody2D rb)
        {
            string exclude = "LowerArmFront UpperArmFront";
            if (exclude.Contains(rb.name)) return;
            rb.inertia = 0.5f;
        }

        public void BodyInertiaFix(Rigidbody2D rigid)
        {
            Puppet.SetRigidOriginal(rigid.name);
        }
    }
}
