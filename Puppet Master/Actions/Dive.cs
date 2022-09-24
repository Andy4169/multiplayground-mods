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
    public enum DiveState
    {
        ready,
        start,
        diving,
        landed,
    };

    public class Dive
    {
        public DiveState state        = DiveState.ready;
        public Puppet Puppet          = null;
        public bool DoMaxPayne        = false;
        public int frame              = 0;
        public float force            = 0;
        public float facing           = 1;
        private bool handPlant        = false;
        private bool keepFlipping     = true;
        
        private MoveSet ActionPose;

        //
        // ─── DIVE INIT ────────────────────────────────────────────────────────────────
        //
        public void Init()
        {
            if (state == DiveState.ready)
            {
                if (!Puppet.LB["Foot"].IsOnFloor && !Puppet.LB["FootFront"].IsOnFloor) return;
                if (Puppet.JumpLocked || Puppet.DisabledMoves) return;

                ActionPose = new MoveSet("dive", false);

                ActionPose.Ragdoll.ShouldStandUpright       = false;
                ActionPose.Ragdoll.State                    = PoseState.Rest;
                ActionPose.Ragdoll.Rigidity                 = 6.2f;
                ActionPose.Ragdoll.ShouldStumble            = false;
                ActionPose.Ragdoll.AnimationSpeedMultiplier = 11.5f;
                ActionPose.Ragdoll.UprightForceMultiplier   = 0f;

                ActionPose.Import();

                facing       = Puppet.Facing;
                state        = DiveState.start;
                force        = 0;
                frame        = 0;
                handPlant    = false;
                keepFlipping = true;

                if (KB.Modifier)
                {
                    Util.MaxPayne(true);
                    DoMaxPayne = true;
                }

                Puppet.Invincible(true);
                Puppet.DisableMoves = Time.time + 2.5f;
                Puppet.JumpLocked   = true;

                if (Puppet.HoldingF != null && (bool)Puppet.HoldingF?.isEnergySword) Puppet.HoldingF?.TurnItemOff();
                Puppet.pauseAiming = true;

            }
        }

        //
        // ─── DIVE GO ────────────────────────────────────────────────────────────────
        //
        public void Go()
        {
            frame++;
            if (KB.Up) force++;

            if (state == DiveState.start)
            {
                if (frame < 20) ActionPose.RunMove();

                if (frame >= 20 && frame <= 25) {
                    Puppet.RB2["LowerArm"]?.AddForce(Vector2.right * -facing * (20 * Puppet.TotalWeight));
                    Puppet.RB2["LowerArmFront"]?.AddForce(Vector2.right * -facing * (20 * Puppet.TotalWeight));
                    Puppet.RB2["UpperBody"]?.AddForce(Vector2.up * (5 * Puppet.TotalWeight));
                }
                if (frame == 20) {
                    ActionPose.ClearMove();
                    Puppet.RunRigids(Puppet.RigidDrag, 0.3f);

                }

                if (frame >= 23)
                {
                    Puppet.RB2["UpperArm"]?.AddForce(Vector2.right * -facing  * (5 * Puppet.TotalWeight));
                    Puppet.RB2["UpperArmFront"]?.AddForce(Vector2.right * -facing * (5 * Puppet.TotalWeight));
                    Puppet.RB2["UpperBody"]?.AddForce(((Vector2.right * -facing) + Vector2.up)  * (25 * Puppet.TotalWeight));

                    if (frame >= 27) {
                        state = DiveState.diving;
                        frame = 0;
                    }

                    return;
                }
            }


            if (state == DiveState.diving)
            {
                if (frame == 10) Puppet.PBO.OverridePoseIndex = -1;

                if (frame > 10)
                {
                    frame = 0;
                    state = DiveState.landed;
                    return;
                }
            }


            if (state == DiveState.landed)
            {
                Puppet.DisableMoves = Time.time + 0.5f;

                if (Puppet.LB["LowerArm"].IsOnFloor || Puppet.LB["LowerArmFront"].IsOnFloor)
                {
                    if (keepFlipping && handPlant && (KB.Right || KB.Left))
                    {
                        Puppet.RB2["UpperBody"].AddForce(Vector2.up * 5 * Puppet.TotalWeight);
                        Puppet.RB2["LowerBody"].AddTorque(facing * Puppet.TotalWeight * 15);
                    }

                    if (!handPlant)
                    {
                        Vector2 tempY = Puppet.RB2["UpperBody"].velocity;
                        tempY.y *= 0f;

                        Puppet.RB2["UpperBody"].velocity = tempY;

                        handPlant = true;
                    }
                }

                if (Puppet.LB["Foot"].IsOnFloor || Puppet.LB["FootFront"].IsOnFloor)
                {
                    if (keepFlipping && (KB.Right || KB.Left))
                    {
                            Puppet.RB2["UpperBody"].AddForce(Vector2.up * 25 * Puppet.TotalWeight);
                            Puppet.RB2["LowerBody"].AddTorque(facing * Puppet.TotalWeight * 15);
                            return;
                    }

                    if (DoMaxPayne) PuppetMaster.Master.AddTask(Util.MaxPayne, 0.5f, false); 

                    Puppet.RB2["UpperBody"].velocity *= 0.0001f;
                    Puppet.RB2["LowerBody"].velocity *= 0.0001f;
                    Puppet.RB2["MiddleBody"]?.AddTorque(50f * -facing * Puppet.TotalWeight);
                    
                    Puppet.PBO.OverridePoseIndex = 0;
                    state                        = DiveState.ready;
                    Puppet.JumpLocked            = false;

                    ActionPose.ClearMove();

                    Actions.AddTask(Finale, 1.5f);

                } else if (Puppet.LB["Head"].IsOnFloor) keepFlipping = false;
            }
        }

        public void Finale()
        {
            Puppet.Invincible(false);
            Puppet.RunLimbs(Puppet.LimbHeal);
            Puppet.RunRigids(Puppet.RigidReset);
            ActionPose.ClearMove();
            if (Puppet.HoldingF != null && (bool)Puppet.HoldingF?.isEnergySword) Puppet.HoldingF?.TurnItemOn(true);
            Puppet.pauseAiming = false;
        }
        
    }

}
