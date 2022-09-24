//     ___                        _                    _     
//    / _ \_   _ _ __  _ __   ___| |_  /\/\   __ _ ___| |_ ___ _ __ 
//   / /_)/ | | | '_ \| '_ \ / _ \ __|/    \ / _` / __| __/ _ \ '__|
//  / ___/| |_| | |_) | |_) |  __/ |_/ /\/\ \ (_| \__ \ ||  __/ |  
//  \/     \__,_| .__/| .__/ \___|\__\/    \/\__,_|___/\__\___|_|  
//              |_|   |_|                
//                                           
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuppetMaster
{
    [SkipSerialisation]
    public class Puppet
    {
        [SkipSerialisation] public bool IsActive                 = false;
        [SkipSerialisation] public bool IsWalking                = false;
        [SkipSerialisation] public bool IsCrouching              = false;
        [SkipSerialisation] public bool IsAiming                 = false;
        [SkipSerialisation] public bool IsPointing               = false;
        [SkipSerialisation] public bool IsFiring                 = false;
        [SkipSerialisation] public bool JumpLocked               = false;
        [SkipSerialisation] public bool IsEmote                  = false;
        [SkipSerialisation] public bool FacingLeft               = false;
        [SkipSerialisation] public bool BlockMoves               = false;
        [SkipSerialisation] public bool checkFlipColliders       = false;
        [SkipSerialisation] public bool pauseAiming              = false;
        [SkipSerialisation] public bool resetFlipControl         = false;
        [SkipSerialisation] public bool IsInVehicle              = false;
        [SkipSerialisation] public bool RunUpdate                = false;
        
        [SkipSerialisation] public int EmoteId                   = -1;
        
        [SkipSerialisation] public float Facing                  = 1f;
        [SkipSerialisation] public float TotalWeight             = 0;
        [SkipSerialisation] public float DisableMoves            = 0;
        
        [SkipSerialisation] public PersonBehaviour PBO           = null;
        [SkipSerialisation] public Inventory Inventory           = null;
        [SkipSerialisation] public Actions Actions               = null;
        [SkipSerialisation] public Collider2D[] Colliders        = null;

        [SkipSerialisation] public Thing HoldingF                = null;
        [SkipSerialisation] public Thing HoldingB                = null;

        [SkipSerialisation] public GripBehaviour GripF           = null;
        [SkipSerialisation] public GripBehaviour GripB           = null;

        [SkipSerialisation] public KeyCode IgnoreKey             = KeyCode.None;
        [SkipSerialisation] public bool FireProof                = false;

        [SkipSerialisation] public Dictionary<string, int> PuppetPose     = new Dictionary<string, int>();
        [SkipSerialisation] public Dictionary<string, Rigidbody2D> RB2    = new Dictionary<string, Rigidbody2D>();
        [SkipSerialisation] public Dictionary<string, LimbBehaviour> LB   = new Dictionary<string, LimbBehaviour>();
        private readonly Dictionary<string, LimbSnapshot> LimbOriginals   = new Dictionary<string, LimbSnapshot>();
        private readonly Dictionary<string, RigidSnapshot> RigidOriginals = new Dictionary<string, RigidSnapshot>();
        
        [SkipSerialisation] public List<Collider2D> FlipColliders = new List<Collider2D>();

        [SkipSerialisation]
        public struct LimbSnapshot
        {
            public float Health;
            public float Numbness;
            public float BreakingThreshold;
            public float Vitality;
            public float RegenerationSpeed;
            public float BaseStrength;
            public bool Broken;
            public bool Frozen;
            public ushort BruiseCount;
            public float jLimitMin;
            public float jLimitMax;
            public PhysicalProperties Properties;
        }

        [SkipSerialisation]
        public class RigidSnapshot
        {
            [SkipSerialisation] public float inertia;
            [SkipSerialisation] public float mass;
            [SkipSerialisation] public float drag;
        }

        [SkipSerialisation] public enum Emotes {
            None,
            Waving,
            Victory,
            Prone,
            Dive1,
            Dive2,
        };



        [SkipSerialisation] public bool IsReady    => (bool)(PuppetMaster.Activated 
                                                   && PBO.isActiveAndEnabled 
                                                   && PBO.IsAlive() 
                                                   && PBO.Consciousness >= 1 
                                                   && PBO.ShockLevel < 0.3f );

        [SkipSerialisation] public bool IsUpright  => (bool)(!LB["LowerBody"].IsOnFloor
                                                   && !LB["UpperBody"].IsOnFloor
                                                   && !LB["Head"].IsOnFloor
                                                   && LB["FootFront"].IsOnFloor
                                                   && LB["Foot"].IsOnFloor)
                                                   && !IsInVehicle;

        [SkipSerialisation] public bool CanWalk    => (bool)(PBO.IsTouchingFloor 
                                                   && !IsWalking 
                                                   && !IsCrouching
                                                   && !KB.Up 
                                                   && !IsInVehicle //&& Actions.attack.state == AttackState.ready
                                                   && Actions.prone.state == ProneState.ready);
        [SkipSerialisation] public bool CanAttack  => (bool)(!IsInVehicle && Actions.jump.state == JumpState.ready);
        [SkipSerialisation] public bool IsFlipped     => (bool)(PBO.transform.localScale.x < 0.0f);
        [SkipSerialisation] public bool DisabledMoves => (bool)(IsInVehicle || BlockMoves || DisableMoves > Time.time);
        [SkipSerialisation] public bool SpecialMode => (bool)(IsAiming || IsPointing || Actions.combatMode );


        //
        // ─── INITIALIZE PUPPET ────────────────────────────────────────────────────────────────
        //
        public void Init(PersonBehaviour _pbo)
        {
            TotalWeight       = 0f;
            PBO               = _pbo;

            //  Cache Rigidbody Maps
            Rigidbody2D[] RBs = PBO.transform.GetComponentsInChildren<Rigidbody2D>();
            
            foreach (Rigidbody2D rb in RBs)
            {

                //  Take snapshots of the current values for drag, intertia & mass
                //  Since these are modified during actions, we can set it back proper
                //
                RB2.Add(rb.name, rb);

                RigidSnapshot RBOG = new RigidSnapshot()
                {
                    drag    = rb.drag,
                    inertia = rb.inertia,
                    mass    = rb.mass,
                };

                RigidOriginals.Add(rb.name, RBOG);

                TotalWeight += rb.mass;

                GripBehaviour GB;

                //  @TODO: This shit is broken
                switch (rb.name)
                {
                    case "LowerArmFront":
                        if (rb.TryGetComponent<GripBehaviour>(out GB)) GripF = GB;
                        if (GripF.CurrentlyHolding != null) HoldingF = GripF.CurrentlyHolding.gameObject.GetOrAddComponent<Thing>();
                        break;

                    case "LowerArm":
                        if (rb.TryGetComponent<GripBehaviour>(out GB)) GripB = GB;
                        if (GripB.CurrentlyHolding != null) HoldingB = GripB.CurrentlyHolding.gameObject.GetOrAddComponent<Thing>();
                        break;
                }
            }

            //  Cache LimbBeheaviour Maps
            LimbBehaviour[] LBs = PBO.GetComponentsInChildren<LimbBehaviour>();

            foreach (LimbBehaviour limb in LBs) 
            {
                LB.Add(limb.name, limb);

                LimbSnapshot LBOG = new LimbSnapshot()
                {
                    BaseStrength      = limb.BaseStrength,
                    BreakingThreshold = limb.BreakingThreshold,
                    Broken            = limb.Broken,
                    BruiseCount       = limb.BruiseCount,
                    Frozen            = limb.Frozen,
                    Health            = limb.Health,
                    Numbness          = limb.Numbness,
                    RegenerationSpeed = limb.RegenerationSpeed,
                    Vitality          = limb.Vitality,
                    Properties        = limb.GetComponent<PhysicalBehaviour>().Properties.ShallowClone(),
                };

                if (limb.HasJoint)
                {
                    LBOG.jLimitMax = limb.Joint.limits.max;
                    LBOG.jLimitMin = limb.Joint.limits.min;
                }
                LimbOriginals.Add(limb.name, LBOG);
            }

            if (PBO.OverridePoseIndex != (int)PoseState.Rest || PBO.OverridePoseIndex != (int)PoseState.Sitting)
            {
                PBO.OverridePoseIndex = -1;
            }

            Actions        = new Actions(this);
            Inventory      = new Inventory(this);
            FacingLeft     = IsFlipped;
            Facing         = FacingLeft ? 1f : -1f;

            Colliders      = PBO.transform.root.GetComponentsInChildren<Collider2D>(); 

            Strengthen();

            Util.Notify("Puppet set: <color=yellow>" + PBO.name + "</color>", Util.VerboseLevels.Minimal);

            PuppetMaster.ChaseCam?.SetPuppet(this, true);

            Garage.Puppet  = this;

        }


        //
        // ─── UNITY FIXED UPDATE ────────────────────────────────────────────────────────────────
        //
        public void FixedUpdate()
        {
            if (Time.frameCount % 100 == 0) 
            {
                if (PBO == null) 
                { 
                    this.IsActive = false;
                    PuppetMaster.ActivePuppet = 0;
                    return;
                }

                if (resetFlipControl && !KB.Left && !KB.Right && (bool)!HoldingF?.isLodged) {
                    
                    resetFlipControl = false;

                }
            }

            Actions.RunActions();


            if (IsAiming) { 
                //if (!IsReady) IsAiming = false;
                //else AimWeapon();
                if (IsReady) AimWeapon();
            } else if (IsPointing)
            {
                if (IsReady) PointWeapon();
            }

            if (IsWalking)
            {
                if (KB.Left && !FacingLeft && (Time.time - KB.KeyTimes.Left > 0.5f) && (Time.time - KB.KeyTimes.Left < 1.0f) && Flip())        FacingLeft = true;
                else if (KB.Right && FacingLeft && (Time.time - KB.KeyTimes.Right > 0.5f) && (Time.time - KB.KeyTimes.Right < 1.0f) && Flip())  FacingLeft = false;
                else
                {
                    IsWalking = false;
                    StopPerson();
                }
            }

            if (IsCrouching)
            {

                
            }

            if (Inventory.TriggerPickup)
            {
                Inventory.TriggerPickup = false;

                HoldThing(Inventory.Clone);
            }

            if (KB.Inventory) Inventory.DoInventory();

            if (Inventory.InventoryTriggered && !KB.Inventory) Inventory.RunInventory();

            if (checkFlipColliders && Time.frameCount % 5 == 0) CheckClearedCollisions();
        }


        //
        // ─── UNITY UPDATE ────────────────────────────────────────────────────────────────
        //
        public void Update()
        {
            RunUpdate = false;
            if (FireProof)
            {
                RunUpdate           = true;
                PBO.PainLevel       = 0.0f;
                PBO.ShockLevel      = 0.0f;
                PBO.AdrenalineLevel = 1.0f;
            }
        }

        public void RefreshInventory(Inventory inventory)
        {
            Inventory = inventory;
        }


        //
        // ─── FLIP ────────────────────────────────────────────────────────────────
        //
        public bool Flip(bool forced=false)
        {
            if (!forced) { 
                if (
                    DisabledMoves || 
                    !IsUpright || 
                    (KB.Left && KB.Right) || 
                    Actions.jump.state != JumpState.ready || 
                    Actions.backflip.state != BackflipState.ready ||
                    !PBO.isActiveAndEnabled || 
                    !PBO.IsAlive() || 
                    !PBO.IsTouchingFloor || 
                    PBO.Consciousness < 1) return false;

                if ((HoldingF != null && HoldingF.isLodged) || resetFlipControl) {
                    resetFlipControl = true;
                    return false;

                }
            }

           //if (CheckFlipCollision()) return false;

           const string limbNamesList = "LowerBody,MiddleBody,UpperBody,UpperArm,UpperArmFront,Head";

            //  Take position snapshot of currently help items
            //ValidateHeldItems();

            //HoldingF?.SetHoldingPosition();
            //HoldingB?.SetHoldingPosition();

            Vector3 flipScale = PBO.transform.localScale;
            Vector3 moveA;
            Vector3 moveB;
            Vector2 moveDif;


            flipScale.x *= -1;

            moveB = RB2["Head"].transform.position;

            foreach (LimbBehaviour limb in LB.Values)
            {
                if (limb.HasJoint)
                {
                    limb.BreakingThreshold *= 8;

                    if (!limbNamesList.Contains(limb.name))
                    {
                        JointAngleLimits2D t = limb.Joint.limits;
                        t.min *= -1f;
                        t.max *= -1f;
                        limb.Joint.limits = t;
                        limb.OriginalJointLimits = new Vector2(limb.OriginalJointLimits.x * -1f, limb.OriginalJointLimits.y * -1f);
                    }
                }
            }

            PBO.transform.localScale = flipScale;

            moveA   = RB2["Head"].transform.position;
            moveDif = moveB - moveA;

            PBO.AngleOffset *= -1f;
            PBO.transform.position = new Vector2(PBO.transform.position.x + moveDif.x, PBO.transform.position.y);

            foreach (LimbBehaviour limb in PBO.Limbs) if (limb.HasJoint) limb.Broken = false;

            if (HoldingF != null)
            {
                if (LB["LowerArm"].GripBehaviour.isHolding)
                {
                    LB["LowerArm"].SendMessage("Use", (object)new ActivationPropagation(), SendMessageOptions.DontRequireReceiver);
                }
                LB["LowerArmFront"].SendMessage("Use", (object)new ActivationPropagation(), SendMessageOptions.DontRequireReceiver);
                HoldingF.ResetPosition();
            }

            FacingLeft = IsFlipped;
            Facing     = FacingLeft ? 1f : -1f;

            
            //HoldingB?.ResetPosition();

            CheckFlipCollision();

            return true;
        }


        public void CheckFlipCollision()
        {
            Collider2D[] noCollide = PBO.transform.root.GetComponentsInChildren<Collider2D>();
            bool isClear;
            Collider2D[] tmpCol;

            if (HoldingF != null)
            {
                Collider2D[] itemcollide = HoldingF.P.GetComponents<Collider2D>();

                foreach (Collider2D coll in itemcollide)
                {
                    coll.enabled = false;
                    isClear = true;

                    tmpCol = Physics2D.OverlapBoxAll(coll.transform.position, (Vector2)coll.bounds.size, coll.transform.eulerAngles.y);
                    if (tmpCol.Length > 0)
                    {
                        foreach (Collider2D col in tmpCol)
                        {
                            PersonBehaviour hitperson = col.attachedRigidbody.GetComponentInParent<PersonBehaviour>();
                            if (hitperson != null && hitperson != PBO)
                            {
                                checkFlipColliders = true;
                                FlipColliders.Add(col);
                                col.enabled = false;
                               // isClear = false;

                            }
                        }
                    }
                    coll.enabled = isClear;
                }

            }

            foreach (Collider2D coll in noCollide) {
                coll.enabled = false;
                isClear = true;

                tmpCol = Physics2D.OverlapBoxAll(coll.transform.position, (Vector2)coll.bounds.size, 0 );
                if (tmpCol.Length > 0) 
                {
                    foreach (Collider2D col in tmpCol) 
                    { 
                        PersonBehaviour hitperson = col.attachedRigidbody.GetComponentInParent<PersonBehaviour>();
                        if (hitperson != null && hitperson != PBO)
                        {
                            checkFlipColliders = true;
                            FlipColliders.Add(coll);
                            isClear = false;
                        }
                    }
                }
                coll.enabled = isClear;
            }

            return;
        }

        public bool CheckClearedCollisions()
        {
            Collider2D tmpCol;

            for (int i = FlipColliders.Count; --i >= 0;)
            {
                Collider2D coll = (Collider2D)FlipColliders[i];

                if (coll == null)
                {
                    FlipColliders.RemoveAt(i);
                    continue;
                }
                if (tmpCol = Physics2D.OverlapBox(coll.transform.position, (Vector2)coll.bounds.size, 0))
                {
                    PersonBehaviour hitperson = tmpCol.attachedRigidbody.GetComponentInParent<PersonBehaviour>();
                    if (hitperson == null || hitperson == PBO)
                    {
                        coll.enabled = true;
                        FlipColliders.RemoveAt(i);
                    }
                }
            }

            checkFlipColliders = FlipColliders.Count > 0;
            return checkFlipColliders;
        }

        //
        // ─── CHECK CONNTROLS ────────────────────────────────────────────────────────────────
        //
        public void CheckControls()
        {
            if (!KB.ActionHeld && !KB.MouseDown) IsFiring = false;


            if (KB.Activate && HoldingF != null)
            {
                //  ---
                //  Activate Items
                //  ---
                HoldingF.Activate(HoldingF.isAutomatic);
            }
            if(KB.Action2 || KB.Action || ( (Actions.combatMode || IsPointing) && (KB.MouseDown || KB.Mouse2Down)))
            {
                //  ---
                //  Action Keys
                //  ---
                if (HoldingF != null) { 
                    if (HoldingF.isChainSaw)
                    {
                        HoldingF.Activate(true);
                    }
                    if (HoldingF.canStrike ) {

                        if (HoldingF.isLodged)
                        {
                            // Sword or knife is stuck in enemy
                            if (KB.Left || KB.Right)
                            {
                                if (KB.Left == FacingLeft && KB.Right != FacingLeft) Actions.attack.Init(Attack.AttackIds.dislodgeKick);
                                else Actions.attack.Init(Attack.AttackIds.dislodgeBack);
                            } 
                            else if (KB.Down)
                            {
                                if (HoldingF.canSlice) Actions.attack.Init(Attack.AttackIds.dislodgeIchi);
                                else Actions.attack.Init(Attack.AttackIds.dislodgeKick);
                            }
                        } else
                        {
                            if (KB.Action || (Actions.combatMode && KB.MouseDown)) Actions.attack.Init(Attack.AttackIds.club);
                            if (KB.Action2 || (Actions.combatMode && KB.Mouse2Down)) Actions.attack.Init(Attack.AttackIds.thrust);
                        }
                    }
                    
                }
            }
            if (( KB.Left || KB.Right) && !DisabledMoves)
            {
                //  ---
                //  Keys LEFT + RIGHT
                //  ---
                if ( CanWalk )
                {

                    if ((KB.KeyCombos.DoubleRight || KB.KeyCombos.DoubleLeft))
                    {
                        if (FacingLeft == KB.Left)
                        {
                            Actions.dive.Init();
                        } else
                        {
                            Actions.backflip.Init();
                        }

                        KB.KeyCombos.DoubleRight = KB.KeyCombos.DoubleLeft = false;
                        
                        
                    } else { 
                        if (!IsWalking)
                        {

                            if (FacingLeft != KB.Left) {
                                
                                if (KB.Left && (Time.time - KB.KeyTimes.Left > 0.5f) && (Time.time - KB.KeyTimes.Left < 1.0f))   IsWalking   = Flip();
                                if (KB.Right && (Time.time - KB.KeyTimes.Right > 0.5f) && (Time.time - KB.KeyTimes.Right < 1.0f)) IsWalking   = Flip();

                            }
                            else IsWalking  = true;
                            
                            if (IsWalking) PBO.OverridePoseIndex = (int)PoseState.Walking;

                        }

                    }
                } 
                else  if (IsCrouching)
                {
                    if (FacingLeft != KB.Left) Flip();
                    //FacingLeft = IsFlipped;
                }
            } 

            else if (KB.Down && !IsCrouching && Actions.prone.state == ProneState.ready && PBO.IsTouchingFloor)
            {
                //  ---
                //  Keys DOWN
                //  ---
                if (KB.KeyCombos.DoubleDown)
                {
                    if (!DisabledMoves) Actions.prone.Init();
                    KB.KeyCombos.DoubleDown = false;
                } 
                else if (!DisabledMoves && Actions.prone.state == ProneState.ready) { 

                    if (KB.Modifier) PBO.OverridePoseIndex = (int)PoseState.Sitting;
                    else
                    {
                        //PBO.OverridePoseIndex  = PuppetPose["JT_Duck"];
                        if (!IsCrouching) Actions.crouch.Init();
                    }

                }

            }

            else if (KB.Up)
            {
                //  ---
                //  Keys UP
                //  ---
                if (!IsInVehicle && Actions.prone.state != ProneState.ready)
                {
                    StopPerson();
                    JumpLocked = true;
                } 
                else if (!IsReady)
                {
                    Recover();
                }
                else if (!DisabledMoves && !IsInVehicle) Actions.jump.Init();
                
            }

            
            else if (KB.Emote)
            {
                //Vector3 diff = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - (Vector3)RB2["MiddleBody"].position);
                //Vector2 wdirection = RB2["MiddleBody"].position;// - Vector2.down;
                //wdirection.y -= 2f;
                //wdirection.x = 0f;
                //float angle = Vector2.Angle(RB2["MiddleBody"].position, wdirection);
                ////angle = 90f;
                ////RB2["MiddleBody"].MoveRotation(Mathf.LerpAngle(RB2["MiddleBody"].rotation * Mathf.Rad2Deg, angle, 10f * Time.deltaTime));
                //RB2["MiddleBody"].MoveRotation(angle);
                //Util.Notify("TAngle: " + angle + " -- CAngle: " + RB2["MiddleBody"].rotation * Mathf.Rad2Deg);


                //    DoEmote();
                //} else if (IsEmote) {
                //    IsEmote = false;
                //    KB.EnableNumberKeys();
                //    if (EmoteId == -1) Util.Notify("HOLD THIS KEY & CHOOSE EMOTE #", Util.VerboseLevels.Minimal);
            }



            else if (KB.Throw)
            {
                //  ---
                //  Keys Throw
                //  ---
                if (IsReady && Actions.throwItem.state == ThrowState.ready) Actions.throwItem.Init();

            }


            else {
                
                if (JumpLocked) JumpLocked = false;

            }

            if (KB.Aim)
            {
                //  ---
                //  Keys AIM
                //  ---
                IsAiming = !IsAiming;

                if (IsAiming) AimingInit();

                else AimingStop();

                if (HoldingF != null && !HoldingF.canShoot)
                {
                    if (HoldingF.isChainSaw)
                    {
                        Actions.combatMode = false;
                        IsPointing         = !IsPointing;
                        if (IsPointing) KB.DisableMouse();
                        else KB.EnableMouse();
                    } else
                    {
                        IsPointing = false;
                        Actions.combatMode = !Actions.combatMode;
                        Util.Notify("<color=red>COMBAT MODE: </color>" + (Actions.combatMode ? "ON" : "OFF"), Util.VerboseLevels.Minimal);
                    }
                }

            }

            if (KB.ActionHeld || (IsAiming && KB.MouseDown))
            {
                //  ---
                //  Keys ACTION
                //  ---
                if (!IsFiring)
                {
                    // Check if held item is something aimable
                    if (!LB["LowerArmFront"].isActiveAndEnabled || !LB["LowerArmFront"].GripBehaviour.isHolding || !HoldingF.canShoot)
                    {
                        IsFiring = false;
                        return;
                    }

                    IsFiring = true;

                    FireWeapon();
                }
                else if (HoldingF.isAutomatic) FireWeapon();
            }

            //if (KB.Alt && HoldingF != null) HoldingF.PrintPosition();
            
        }


        //
        // ─── DO EMOTE ────────────────────────────────────────────────────────────────
        //
        public void DoEmote()
        {
            if (!IsEmote)
            {
                EmoteId = -1;

                KB.DisableNumberKeys();

                IsEmote = true;
            }

            EmoteId = KB.CheckNumberKey();

            if (EmoteId == -1) return;

        }

        
        //
        // ─── FIRE WEAPON ────────────────────────────────────────────────────────────────
        //
        private void FireWeapon()
        {
            if (!PBO.IsAlive()) return;
            HoldingF.Activate(HoldingF.isAutomatic);
        }


        //
        // ─── STOP PERSON ────────────────────────────────────────────────────────────────
        //
        public void StopPerson()
        {
            if (!IsWalking && !IsCrouching) { 

                if (PBO.OverridePoseIndex != (int)PoseState.Rest || PBO.OverridePoseIndex != (int)PoseState.Sitting)
                {
                    PBO.OverridePoseIndex = -1;
                }
            }
        }


        //
        // ─── AIMING INIT ────────────────────────────────────────────────────────────────
        //
        public void AimingInit()
        {
            // Check if held item is something that puppet can point
            if (!LB["LowerArmFront"].isActiveAndEnabled || !LB["LowerArmFront"].GripBehaviour.isHolding || HoldingF == null || !HoldingF.canShoot)
            {
                //IsAiming = false;
                AimingStop();
                return;
            }

            IsAiming = HoldingF.canShoot;
            
            if (IsAiming) KB.DisableMouse();


            

            if (HoldingF.holdToSide)
            {
                //HoldingF.SetHoldingPosition();

                LB["LowerArmFront"].SendMessage("Use", (object)new ActivationPropagation(), SendMessageOptions.DontRequireReceiver);
                
                HoldingF.ResetPosition(true);
            }


        }

        //
        // ─── AIMING STOP ────────────────────────────────────────────────────────────────
        //
        public void AimingStop()
        {
            IsAiming = false;
            
            LB["LowerArmFront"].Broken = false;
            LB["UpperArmFront"].Broken = false;

            KB.EnableMouse();

            //Actions.dive.Finale();

            if (HoldingF != null && HoldingF.holdToSide) {

                LB["LowerArmFront"].SendMessage("Use", (object)new ActivationPropagation(), SendMessageOptions.DontRequireReceiver);
                HoldingF.ResetPosition();

            }

        }


        //
        // ─── AIM WEAPON ────────────────────────────────────────────────────────────────
        //
        public void AimWeapon()
        {
            //
            //  Reference: https://gamedevbeginner.com/make-an-object-follow-the-mouse-in-unity-in-2d/
            //
            Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);


            if (mouse.x * Facing > (RB2["Head"].position.x * Facing))
            {
                LB["LowerArmFront"].Broken = false;
                LB["UpperArmFront"].Broken = false;

                SetRigidOriginal("LowerArmFront", "inertia");
                SetRigidOriginal("UpperArmFront", "inertia");
                return;

            }

            LB["UpperArmFront"].Broken   = true;
            RB2["UpperArmFront"].inertia = 0.00125f;
            RB2["LowerArmFront"].inertia = 0.00125f;
            
            Vector3 diff = ((Vector3)RB2["LowerArmFront"].position - Camera.main.ScreenToWorldPoint(Input.mousePosition));
            
            //  Prevent spasticated behavior
            float sqrmag = diff.sqrMagnitude;

            if( sqrmag < 105f ) { 
                if (pauseAiming) return;
                if( diff.sqrMagnitude < 101f ) {
                    pauseAiming = true;
                    return;
                }
            }

            pauseAiming = false;
            
            Vector3 angleVelocity;
            if (FacingLeft) angleVelocity = new Vector3(0f, 0f, (Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg) - (85.5f + (16.5f * Facing)));
            else angleVelocity = new Vector3(0f, 0f, (Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg) - (85.5f + (10.5f * Facing)));
            RB2["LowerArmFront"].MoveRotation(Quaternion.Euler(angleVelocity));

        }

        public void PointWeapon()
        {
            if (HoldingF == null || HoldingF.isChainSaw == false)
            {
                IsPointing                 = false;
                LB["UpperArm"].Broken      = false;
                LB["UpperArmFront"].Broken = false;
                LB["LowerArm"].Broken      = false;
                LB["LowerArmFront"].Broken = false;
                RunRigids(RigidReset);
                return;
            }
            Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (mouse.x * Facing > (RB2["Head"].position.x * Facing))
            {
                LB["UpperArm"].Broken       = false;
                LB["UpperArmFront"].Broken  = false;
                LB["LowerArm"].Broken       = false;
                LB["LowerArmFront"].Broken  = false;
                RunRigids(RigidReset);
                return;
            }

            LB["UpperArm"].Broken           = (IsWalking || IsCrouching);
            LB["UpperArmFront"].Broken      = (IsWalking || IsCrouching);
            LB["LowerArm"].Broken           = (IsWalking || IsCrouching);
            LB["LowerArmFront"].Broken      = (IsWalking || IsCrouching);

            HoldingF.R.inertia                 = 0.01f;
            HoldingF.R.mass                    = HoldingF.P.InitialMass / 2;

            if (pauseAiming) return;

            Vector3 TargetPos = (mouse - HoldingF.tr.position);

            float TargetAngle = Vector2.SignedAngle(Vector2.right, TargetPos) - (HoldingF.angleAim + (HoldingF.angleOffset * Facing));

            HoldingF.R.AddForce(TargetPos * TotalWeight * 3.5f );

            RB2["UpperBody"].AddForce(TargetPos * TotalWeight * 3.5f * -1);
        }



        //
        // ─── INVINCIBLE ────────────────────────────────────────────────────────────────
        //
        public void Invincible(bool doit)
        {
            foreach (LimbBehaviour limb in LB.Values) { limb.ImmuneToDamage = doit; }
        }

        //
        // ─── RUN LIMBS ────────────────────────────────────────────────────────────────
        //
        public void RunLimbs(Action<LimbBehaviour> action)
        {
            foreach (LimbBehaviour limb in LB.Values) { action(limb); }
        }

        public void RunLimbs<t>(Action<LimbBehaviour, t> action, t option)
        {
            foreach (LimbBehaviour limb in LB.Values) { action(limb, option); }
        }

        public void LimbFireProof(LimbBehaviour limb, bool option) {  
            
            PhysicalProperties propys           = limb.GetComponent<PhysicalBehaviour>().Properties.ShallowClone();
            propys.Flammability                 = option ? 0f : LimbOriginals[limb.name].Properties.Flammability;
            //propys.BurningTemperatureThreshold  = option ? 5000f * 100f : LimbOriginals[limb.name].Properties.BurningTemperatureThreshold;
            propys.BurningTemperatureThreshold  = float.MaxValue;
            propys.Burnrate = 0.00001f;

            limb.PhysicalBehaviour.Properties = propys;
            limb.PhysicalBehaviour.Extinguish();
            limb.PhysicalBehaviour.SimulateTemperature = !option;

            limb.PhysicalBehaviour.ChargeBurns  = false;
            limb.PhysicalBehaviour.BurnProgress = 0f;

            limb.DiscomfortingHeatTemperature = float.MaxValue;
        }
        
        public void LimbImmune(LimbBehaviour limb, bool option)     => limb.ImmuneToDamage = option;
        public void LimbHeal(LimbBehaviour limb) { limb.BruiseCount = 0; }
        public void LimbGhost (LimbBehaviour limb, bool option) { limb.gameObject.layer = LayerMask.NameToLayer(option ? "Debris" : "Objects"); }




        //
        // ─── RUN RIGIDS ────────────────────────────────────────────────────────────────
        //
        public void RunRigids(Action<Rigidbody2D> action)
        {
            foreach (Rigidbody2D rigid in RB2.Values) { action(rigid); }
        }

        public void RunRigids<t>(Action<Rigidbody2D, t> action, t option)
        {
            foreach (Rigidbody2D rigid in RB2.Values) { action(rigid, option); }
        }

        public void RigidInertia(Rigidbody2D rb, float option) => rb.inertia = (option == -1) ? RigidOriginals[rb.name].inertia : option;
        public void RigidMass(Rigidbody2D rb, float option) => rb.mass = (option == -1) ? RigidOriginals[rb.name].mass : option;
        public void RigidAddMass(Rigidbody2D rb, float option) => rb.mass *= option;
        public void RigidDrag(Rigidbody2D rb, float option) => rb.drag = (option == -1) ? RigidOriginals[rb.name].drag : option;
        public void RigidReset(Rigidbody2D rb)
        {
            rb.mass    = RigidOriginals[rb.name].mass;
            rb.drag    = RigidOriginals[rb.name].drag;
            rb.inertia = RigidOriginals[rb.name].inertia;
        }
        public void RigidStop(Rigidbody2D rb)
        {
            rb.velocity        = Vector2.zero;
            rb.angularVelocity = 0f;
        }


        public enum HoldingPositions
        {
            Auto,
            PointingDown,
            PointingForward,
            BothHands,
        }

        //
        // ─── HOLD THING ────────────────────────────────────────────────────────────────
        //
        public void HoldThing(Thing thing)
        {
            ValidateHeldItems();

            if (HoldingF != null)
            {
                DropThing();
                return;
            }

            HoldingF = thing;
            HoldingF.AttachPuppetHand(this, true);

            if (Actions.combatMode && HoldingF.canShoot) Actions.combatMode = false;
            if (IsAiming && !HoldingF.canShoot) AimingStop();

        }

        //
        // ─── VALIDATE HELD ITEMS ────────────────────────────────────────────────────────────────
        //
        public void ValidateHeldItems()
        {
        }


        public void DropThing()
        {
            //ValidateHeldItems();

            if (HoldingF != null)
            {
                if (HoldingF.P == null)
                {
                    HoldingF = (Thing)null;
                    return;
                }

                if (GripF.isHolding)
                {
                    LB["LowerArmFront"].SendMessage("Use", (object)new ActivationPropagation(), SendMessageOptions.DontRequireReceiver);
                    if (LB["LowerArm"].GripBehaviour.isHolding)
                    {
                        LB["LowerArm"].SendMessage("Use", (object)new ActivationPropagation(), SendMessageOptions.DontRequireReceiver);
                    }
                    Util.DisableCollision(PBO.transform, HoldingF.P, true);

                    //Collider2D[] noCollide = GripF.transform.root.GetComponentsInChildren<Collider2D>();

                    //foreach (Collider2D col1 in HoldingF.tr.root.GetComponentsInChildren<Collider2D>())
                    //{
                    //    foreach (Collider2D col2 in noCollide)
                    //    {
                    //        if ((bool)(UnityEngine.Object)col2 && (bool)(UnityEngine.Object)col1)

                    //        Physics2D.IgnoreCollision(col1, col2);
                    //    }
                    //}

                    PuppetMaster.CheckDisabledCollisions = true;

                    HoldingF.Dropped();
                    HoldingF = (Thing)null;
                }
            }
        }

        //
        // ─── RECOVER ────────────────────────────────────────────────────────────────
        //
        public void Recover()
        {
            PBO.Consciousness   = 1f;
            PBO.ShockLevel      = 0.0f;
            PBO.PainLevel       = 0.0f;
            PBO.OxygenLevel     = 1f;
            PBO.AdrenalineLevel = 1f;

            if (KB.Modifier)
            {
                Liquid liquid = Liquid.GetLiquid("LIFE SERUM");

                foreach (LimbBehaviour limb in LB.Values)
                {
                    if (!limb.isActiveAndEnabled) return;

                    limb.HealBone();
                    limb.CirculationBehaviour.HealBleeding();

                    limb.Health                                 = limb.InitialHealth;
                    limb.Numbness                               = 0.0f;
                    limb.BruiseCount                            = 0;
                    limb.PhysicalBehaviour.BurnProgress         = 0.0f;
                    limb.SkinMaterialHandler.AcidProgress       = 0.0f;
                    limb.SkinMaterialHandler.RottenProgress     = 0.0f;
                    limb.CirculationBehaviour.IsPump            = limb.CirculationBehaviour.WasInitiallyPumping;
                    limb.CirculationBehaviour.BloodFlow         = 1f;
                    limb.CirculationBehaviour.StabWoundCount    = 0;
                    limb.CirculationBehaviour.GunshotWoundCount = 0;

                    limb.CirculationBehaviour.ForceSetAllLiquid(0f);
                    limb.CirculationBehaviour.AddLiquid(limb.GetOriginalBloodType(), 1f);
                    limb.CirculationBehaviour.AddLiquid(liquid, 0.1f);
                }
            }

        }

        public void LimbLimits(string LimbList = "", bool setActive = true)
        {
            HingeJoint2D Joint;

            if (LimbList == "")
            {
                foreach (KeyValuePair<string, LimbBehaviour> pair in LB)
                {
                    if (pair.Value.HasJoint) 
                    {
                        Joint               = pair.Value.Joint;
                        Joint.useLimits     = setActive;
                        LB[pair.Key].Joint  = Joint;
                    }
                }
            } 
            else
            {
                string[] limbNames = LimbList.Split(',');

                for (int i = limbNames.Length; --i >= 0;)
                {
                    if (LB[limbNames[i]].HasJoint) 
                    { 
                        Joint                  = LB[limbNames[i]].Joint;
                        Joint.useLimits        = setActive;
                        LB[limbNames[i]].Joint = Joint;
                    }
                }
            }
        }


        //
        // ─── STRENGTHEN ────────────────────────────────────────────────────────────────
        //
        public void Strengthen()
        {
            foreach (KeyValuePair<string, LimbBehaviour> kvp in LB)
            {
                LB[kvp.Key].Vitality             *= 0.1f;
                LB[kvp.Key].RegenerationSpeed    += 5f;
                LB[kvp.Key].ImpactPainMultiplier *= 0.1f;
                LB[kvp.Key].InitialHealth        += 150f;
                LB[kvp.Key].BreakingThreshold    += 10f;
                LB[kvp.Key].ShotDamageMultiplier *= 0.1f;
                LB[kvp.Key].Health               += LB[kvp.Key].InitialHealth;
                LB[kvp.Key].BaseStrength          = Mathf.Min(15f, LB[kvp.Key].BaseStrength + 5f);
            }
        }


        //
        // ─── SET RIGID ORIGINAL ────────────────────────────────────────────────────────────────
        //
        public void SetRigidOriginal(string rigidName, string propName="")
        {
            List<string> props = new List<string>() { "mass", "drag", "inertia" };

            if (propName != "") {
                props.Clear();
                props.Add(propName);
            }

            foreach (string pname in props)
            {
                if (RigidOriginals.TryGetValue(rigidName, out RigidSnapshot rigidOG))
                {
                    switch(pname)
                    {
                        case "mass":
                            RB2[ rigidName ].mass = rigidOG.mass;
                            break;

                        case "drag":
                            RB2[rigidName].drag = rigidOG.drag;
                            break;

                        case "inertia":
                            RB2[rigidName].inertia = rigidOG.inertia;
                            break;
                    }

                }
            }
        }

        public void FixBody()
        {
            RunRigids(BodyInertiaFix);
            foreach (KeyValuePair<string, LimbBehaviour> pair in LB)
            {
                if (LimbOriginals.TryGetValue(pair.Key, out LimbSnapshot limbSnapshot))
                {
                    pair.Value.Broken = limbSnapshot.Broken;

                    if (pair.Value.HasJoint)
                    {
                        JointAngleLimits2D jal = pair.Value.Joint.limits;

                        jal.min = limbSnapshot.jLimitMin;
                        jal.max = limbSnapshot.jLimitMax;


                    }

                    pair.Value.Broken = limbSnapshot.Broken;
                }
            }

        }

        public void BodyInertiaFix(Rigidbody2D rigid) => SetRigidOriginal(rigid.name);

        
    }
}
