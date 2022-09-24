//     ___                        _                    _     
//    / _ \_   _ _ __  _ __   ___| |_  /\/\   __ _ ___| |_ ___ _ __ 
//   / /_)/ | | | '_ \| '_ \ / _ \ __|/    \ / _` / __| __/ _ \ '__|
//  / ___/| |_| | |_) | |_) |  __/ |_/ /\/\ \ (_| \__ \ ||  __/ |  
//  \/     \__,_| .__/| .__/ \___|\__\/    \/\__,_|___/\__\___|_|  
//              |_|   |_|                
//                                           
namespace PuppetMaster
{
    public enum CrouchState
    {
        ready,
        crouching,
        getup,
    };

    public class Crouch
    {
        public CrouchState state = CrouchState.ready;
        public Puppet Puppet     = null;
        
        private MoveSet ActionPose;


        //
        // ─── CROUCH INIT ────────────────────────────────────────────────────────────────
        //
        public void Init()
        {
            if (state == CrouchState.ready )
            {
                state      = CrouchState.crouching;
                ActionPose = new MoveSet("crouch", false);

                ActionPose.Ragdoll.ShouldStandUpright       = true;
                ActionPose.Ragdoll.State                    = PoseState.Rest;
                ActionPose.Ragdoll.Rigidity                 = 2.2f;
                ActionPose.Ragdoll.ShouldStumble            = false;
                ActionPose.Ragdoll.AnimationSpeedMultiplier = 10.5f;
                

                ActionPose.Import();
                ActionPose.RunMove();

            }
        }


        //
        // ─── PRONE GO ────────────────────────────────────────────────────────────────
        //
        public void Go()
        {
            if (state == CrouchState.crouching)
            {

                Puppet.IsCrouching = KB.Down;

                if (Puppet.IsCrouching)
                {
                    if (KB.Left || KB.Right)
                    {
                        if (Puppet.FacingLeft != KB.Left && Puppet.IsReady) Puppet.Flip();
                    }
                }
                else
                {
                    state = CrouchState.ready;

                    ActionPose.ClearMove();
                    Puppet.PBO.OverridePoseIndex = 0;
                }
            }
        }
    }
}
