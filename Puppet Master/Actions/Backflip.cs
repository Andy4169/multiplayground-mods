//     ___                        _                    _     
//    / _ \_   _ _ __  _ __   ___| |_  /\/\   __ _ ___| |_ ___ _ __ 
//   / /_)/ | | | '_ \| '_ \ / _ \ __|/    \ / _` / __| __/ _ \ '__|
//  / ___/| |_| | |_) | |_) |  __/ |_/ /\/\ \ (_| \__ \ ||  __/ |  
//  \/     \__,_| .__/| .__/ \___|\__\/    \/\__,_|___/\__\___|_|  
//              |_|   |_|                
//                                           
using UnityEngine;
namespace PuppetMaster
{
    public enum BackflipState
    {
        ready,
        start,
        flipping,
        landed,
    };

    public class Backflip
    {
        public BackflipState state = BackflipState.ready;
        public Puppet Puppet       = null;
        public int frame           = 0;
        public float force         = 0;
        private float facing       = 1;
        private bool handPlant     = false;
        private bool Boost         = true;
        private float avgSpeed     = 0;
        private bool keepFlipping  = true;

        private MoveSet AP_Backflip1;

        //
        // ─── Backflip INIT ────────────────────────────────────────────────────────────────
        //
        public void Init()
        {
            if (state == BackflipState.ready)
            {
                if (!Puppet.LB["Foot"].IsOnFloor && !Puppet.LB["FootFront"].IsOnFloor) return;
                if (Puppet.JumpLocked || Puppet.DisabledMoves) return;

                AP_Backflip1 = new MoveSet("backflip_1", false);

                AP_Backflip1.Ragdoll.ShouldStandUpright       = true;
                AP_Backflip1.Ragdoll.State                    = PoseState.Rest;
                AP_Backflip1.Ragdoll.Rigidity                 = 6.3f;
                AP_Backflip1.Ragdoll.ShouldStumble            = false;
                AP_Backflip1.Ragdoll.AnimationSpeedMultiplier = 11.5f;
                AP_Backflip1.Ragdoll.UprightForceMultiplier   = 2f;
                AP_Backflip1.Import();

                state        = BackflipState.start;
                force        = 0;
                frame        = 0;
                Boost        = false;
                keepFlipping = true;


                handPlant = false;

                facing = Puppet.Facing;

                if (KB.Modifier)
                {
                    Util.MaxPayne(true);
                }

                Puppet.RunLimbs(Puppet.LimbImmune, true);

                Puppet.DisableMoves = Time.time + 2.5f;
                Puppet.JumpLocked   = true;

                if (Puppet.HoldingF != null && (bool)Puppet.HoldingF?.isEnergySword) Puppet.HoldingF?.TurnItemOff();
                Puppet.pauseAiming = true;

            }
        }

        //
        // ─── Backflip GO ────────────────────────────────────────────────────────────────
        //
        public void Go()
        {
            frame++;
            if (KB.Up) force++;

            if (state == BackflipState.start)
            {
                if (frame > 10)
                {
                }
                if (frame >= 10 && frame <= 14)
                {
                    Puppet.RB2["LowerArm"]?.AddForce(Vector2.right * facing * (20 * Puppet.TotalWeight));
                    Puppet.RB2["LowerArmFront"]?.AddForce(Vector2.right * facing * (20 * Puppet.TotalWeight));
                    Puppet.RB2["UpperArm"]?.AddForce(Vector2.up * (8 * Puppet.TotalWeight));
                    Puppet.RB2["UpperArmFront"]?.AddForce(Vector2.up * (8 * Puppet.TotalWeight));

                    if (Puppet.HoldingF != null) Puppet.RB2["LowerBody"]?.AddForce(Vector2.up * (5 * Puppet.TotalWeight));

                }
                if (frame < 20) AP_Backflip1.RunMove();

                if (frame >= 20 && frame <= 25)
                {
                    Puppet.RB2["LowerArm"]?.AddForce(Vector2.right * facing * (20 * Puppet.TotalWeight));
                    Puppet.RB2["LowerArmFront"]?.AddForce(Vector2.right * facing * (20 * Puppet.TotalWeight));
                }

                if (frame >= 23)
                {
                    Puppet.RB2["UpperArm"]?.AddForce(Vector2.right * -facing * (5 * Puppet.TotalWeight));
                    Puppet.RB2["UpperArmFront"]?.AddForce(Vector2.right * -facing * (5 * Puppet.TotalWeight));
                    Puppet.RB2["UpperBody"]?.AddForce(((Vector2.right * -facing) + Vector2.up) * (20 * Puppet.TotalWeight));

                    if (frame >= 27)
                    {
                        state    = BackflipState.flipping;
                        frame    = 0;
                        avgSpeed = 0;
                    }

                    return;
                }
            }

            if (frame > 200 && !KB.Right && !KB.Left) state = BackflipState.landed;

            if (state == BackflipState.flipping)
            {
                avgSpeed += Puppet.RB2["Head"].velocity.magnitude;

                if (Puppet.LB["LowerArm"].IsOnFloor || Puppet.LB["LowerArmFront"].IsOnFloor)
                {
                    if (keepFlipping && handPlant && (KB.Right || KB.Left || KB.Up || Boost))
                    {
                        Puppet.RB2["UpperBody"].AddForce(((Vector2.right * facing) + Vector2.up) * 15 * Puppet.TotalWeight);
                        Puppet.RB2["LowerBody"].AddTorque(-facing * Puppet.TotalWeight * 20);
                    }

                    if (!handPlant)
                    {
                        Vector2 tempY = Puppet.RB2["UpperBody"].velocity;
                        tempY.y *= 0f;

                        Puppet.RB2["UpperBody"].velocity = tempY;

                        handPlant = Boost = true;

                        Puppet.RB2["UpperBody"].AddForce(((Vector2.right * facing) + Vector2.up) * 15 * Puppet.TotalWeight);
                        Puppet.RB2["LowerBody"].AddTorque(-facing * Puppet.TotalWeight * 15);
                    }
                }

                if (Puppet.LB["UpperBody"].IsOnFloor || Puppet.LB["LowerBody"].IsOnFloor)
                {
                    keepFlipping = false;
                }

                if (Puppet.LB["Foot"].IsOnFloor || Puppet.LB["FootFront"].IsOnFloor)
                {
                    Boost = false;

                    if (keepFlipping && (KB.Right || KB.Left || KB.Up || force > 0))
                    {
                        if (Puppet.RB2["Head"].velocity.magnitude < ((avgSpeed / frame) * 0.5f))
                        {
                            state = BackflipState.landed;
                            return;
                        }

                        if (force > 0 && !KB.Up)
                        {
                            AP_Backflip1.ClearMove();
                            Puppet.PBO.OverridePoseIndex = (int)PoseState.Protective;
                            
                            force *= 0.5f;

                            if (force > 100f) force = 100f;
                            
                            Puppet.RB2["MiddleBody"].AddForce(Vector2.up * force * 10.5f * Puppet.TotalWeight);
                            
                            Puppet.Actions.jump.state = JumpState.goingUp;
                            
                            state = BackflipState.ready;
                            force = 0;
                            return;
                        }

                        Puppet.RB2["UpperBody"].AddForce(((Vector2.right * facing) + (Vector2.up)) * 15 * Puppet.TotalWeight);
                        Puppet.RB2["LowerBody"].AddTorque(-facing * Puppet.TotalWeight * 15);
                    }
                    else
                    {
                        Puppet.RB2["UpperBody"].velocity *= 0.0001f;
                        state = BackflipState.landed;
                    }
                }
            }


            if (state == BackflipState.landed)
            {
                Puppet.JumpLocked = false;

                PuppetMaster.Master.AddTask(Util.MaxPayne, 0.5f, false);

                Puppet.RB2["UpperBody"].velocity *= 0.0001f;
                Puppet.RB2["MiddleBody"]?.AddTorque(50f * facing * Puppet.TotalWeight);

                state = BackflipState.landed;

                Puppet.RunRigids(Puppet.RigidDrag, 0.5f);

                Actions.AddTask(Finale, 1.5f);

                AP_Backflip1.ClearMove();

                state = BackflipState.ready;

                if (Puppet.HoldingF != null && (bool)Puppet.HoldingF?.isEnergySword) Puppet.HoldingF?.TurnItemOn(true);
            }
        }

        public void Finale()
        {
            Puppet.RunLimbs(Puppet.LimbImmune, false);
            Puppet.RunLimbs(Puppet.LimbHeal);
            Puppet.RunRigids(Puppet.RigidReset);
            AP_Backflip1.ClearMove();
            Puppet.pauseAiming = false;
        }
        
    }

}
