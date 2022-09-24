//     ___                        _                    _     
//    / _ \_   _ _ __  _ __   ___| |_  /\/\   __ _ ___| |_ ___ _ __ 
//   / /_)/ | | | '_ \| '_ \ / _ \ __|/    \ / _` / __| __/ _ \ '__|
//  / ___/| |_| | |_) | |_) |  __/ |_/ /\/\ \ (_| \__ \ ||  __/ |  
//  \/     \__,_| .__/| .__/ \___|\__\/    \/\__,_|___/\__\___|_|  
//              |_|   |_|                                    
//                                           
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PuppetMaster
{
    [SkipSerialisation]
    public class Thing : MonoBehaviour
    {
        [SkipSerialisation] public GameObject G;
        [SkipSerialisation] public PhysicalBehaviour P;
        [SkipSerialisation] public Rigidbody2D R;
        [SkipSerialisation] public Transform tr;
        [SkipSerialisation] public Puppet Puppet;
        [SkipSerialisation] public GripBehaviour PuppetGrip;
        [SkipSerialisation] public Rigidbody2D PuppetArm;
        [SkipSerialisation] public LimbBehaviour HitLimb;

        [SkipSerialisation] public string Name;
        [SkipSerialisation] public int Hash;
        [SkipSerialisation] public static int ThingCounter  = 0;
        [SkipSerialisation] public int MyId                 = 0;

        [SkipSerialisation] public bool canAim              = false;
        [SkipSerialisation] public bool canShoot            = false;
        [SkipSerialisation] public bool canStab             = false;
        [SkipSerialisation] public bool canStrike           = false;
        [SkipSerialisation] public bool canSlice            = false;
        [SkipSerialisation] public bool isAutomatic         = false;
        [SkipSerialisation] public bool holdToSide          = false;
        [SkipSerialisation] public bool hasDetails          = false;
        [SkipSerialisation] public bool isFlashlight        = false;
        [SkipSerialisation] public bool isEnergySword       = false;
        [SkipSerialisation] public bool isChainSaw          = false;
        [SkipSerialisation] public bool isSyringe           = false;
        [SkipSerialisation] public bool twoHands            = false;
        [SkipSerialisation] public bool isLodged            = false;
        [SkipSerialisation] public bool isShiv              = false;
        [SkipSerialisation] public bool isSpear             = false;
        [SkipSerialisation] public bool isPersistant        = false;
        [SkipSerialisation] public bool isThrown            = false;
        [SkipSerialisation] public bool doCollisionCheck    = false;
        [SkipSerialisation] public bool protectFromFire     = false;
        [SkipSerialisation] public bool canBeDamaged        = false;
        [SkipSerialisation] public bool doSecondHand        = false;
        [SkipSerialisation] public bool doNotDispose        = false;

        private float secondHandDelay                       = 0f;
        
        [SkipSerialisation] public string HoldingMove       = "";

        private bool itemActiveOGSetting = false;

        
        [SkipSerialisation] public PhysicalBehaviour StabVictim;
        [SkipSerialisation] public LodgedIntoTypes LodgedIntoType;
        

        [SkipSerialisation] public float AttackDamage       = 0f;
        [SkipSerialisation] public float angleHold          = 0f;
        [SkipSerialisation] public float angleAim           = 0f;
        [SkipSerialisation] public float angleOffset        = 0f;
        [SkipSerialisation] public float size               = 0f;
        [SkipSerialisation] public float originalInertia    = 0f;

        [SkipSerialisation] public static Dictionary<int, Vector2> ManualPositions = new Dictionary<int, Vector2>();
        [SkipSerialisation] public List<int> AttackedList                          = new List<int>();


        [SkipSerialisation] public float ImpactForceThreshold;

        [SkipSerialisation] public Collider2D Collider;
        [SkipSerialisation] public UnityEvent Actions;
        [SkipSerialisation] public Collider2D[] ItemColliders;
        [SkipSerialisation] public Collider2D[] Collisions = new Collider2D[3];
        [SkipSerialisation] public int CollisionCount      = 0;


        [SkipSerialisation] public Vector2 HoldingPosition;
        [SkipSerialisation] public Vector3 HoldingOffset;
        
        [SkipSerialisation] public MoveSet ActionPose;


        [SkipSerialisation] public bool IsFlipped => (bool)(tr.localScale.x < 0.0f);
        public bool doDestroyCheck = true;
        private string sortingLayerName;
        private int sortingOrder;

        private float facing          = 1f;
        private float throwTimeout    = 0f;
        public static float delayCollisions = 0f;


        public void Awake()
        {
            MyId = ++ThingCounter;
            G    = gameObject;
            P    = gameObject.GetComponent<PhysicalBehaviour>();
            R    = gameObject.GetComponent<Rigidbody2D>();
            tr   = P.transform;

            isPersistant = false;

            Name = P.name;
            Hash = P.GetHashCode();

            originalInertia  = R.inertia;

            sortingLayerName = P.GetComponent<SpriteRenderer>().sortingLayerName;
            sortingOrder     = P.GetComponent<SpriteRenderer>().sortingOrder;

            Collider2D[] collider = new Collider2D[5];
            int colCount = R.GetAttachedColliders(collider);

            ItemColliders = new Collider2D[colCount];

            for (int i = 0; i < colCount; i++) ItemColliders[i] = collider[i];

            GetDetails();
        }

        public void Update()
        {
            if (isPersistant) return;
            if (doSecondHand) GrabSecondHand();

            //  Check on this periodically
            if (Time.frameCount % 100 == 0)
            {
                if (isLodged) CheckLodged();

                if (!P.beingHeldByGripper) { 
                    
                    if (P.IsWeightless) P.MakeWeightful();


                    //if (!checkingActivity && !isPersistant)
                    if (!isPersistant) 
                    {
                        if (doDestroyCheck) 
                        { 
                            doDestroyCheck  = false;

                            InvokeRepeating("CheckIfUsed", 10f, 10f);
                        } else
                        {
                           if (!doNotDispose) Dispose(true);
                        }
                    }
                }
            }

            if (doCollisionCheck)
            {
                if (CheckCollisions() == LodgedIntoTypes.Nothing)
                {
                    doCollisionCheck = false;
                    if (G.layer == 10) G.SetLayer(9);
                }
            }
        }

        public void FixedUpdate()
        {
            if (isThrown) AnimateThrow();
        }

        public void JustThrown()
        {
            if (canStab || isSyringe)
            {
                isThrown        = true;
                facing          = IsFlipped ? 1f : -1f;
                throwTimeout    = Time.time + 30f;
                delayCollisions = Time.time + 2f;
            }
        }

        private void GrabSecondHand()
        {
            Puppet.LB["UpperArm"].Broken      = false;
            Puppet.LB["UpperArmFront"].Broken = false;
            Puppet.LB["LowerArm"].Broken      = false;
            Puppet.LB["LowerArmFront"].Broken = false;

            Puppet.pauseAiming = true;

            if (HoldingMove != "") ActionPose.RunMove();

            if (Time.time < secondHandDelay) return;

            Collider2D[] colliders = new Collider2D[5];

            ContactFilter2D filter2D = new ContactFilter2D();

            int colCount = ItemColliders[0].OverlapCollider(filter2D, colliders);

            if (Time.time > secondHandDelay + 1f)
            {
                Puppet.pauseAiming = false;
                doSecondHand = false;
                Puppet.LB["LowerArm"].SendMessage("Use", (object)new ActivationPropagation(), SendMessageOptions.DontRequireReceiver);
                Puppet.PBO.OverridePoseIndex = -1;
                P.MakeWeightless();
                float thingRotation = Puppet.IsAiming ? angleAim : angleHold;

                thingRotation += angleOffset;

                tr.rotation = Quaternion.Euler(0.0f, 0.0f, IsFlipped ? PuppetArm.rotation + thingRotation : PuppetArm.rotation - thingRotation);

                tr.position += PuppetGrip.transform.TransformPoint(PuppetGrip.GripPosition) - tr.TransformPoint((Vector3)HoldingPosition);

                return;
            }

            for (int i = 0; i < colCount; i++)
            {
                if (colliders[i].attachedRigidbody.name == "LowerArm")
                {
                    Puppet.pauseAiming = false;
                    doSecondHand = false;
                    Puppet.LB["LowerArm"].SendMessage("Use", (object)new ActivationPropagation(), SendMessageOptions.DontRequireReceiver);
                    Puppet.PBO.OverridePoseIndex = -1;
                    P.MakeWeightless();
                    float thingRotation = Puppet.IsAiming ? angleAim : angleHold;

                    thingRotation += angleOffset;

                    tr.rotation = Quaternion.Euler(0.0f, 0.0f, IsFlipped ? PuppetArm.rotation + thingRotation : PuppetArm.rotation - thingRotation);

                    tr.position += PuppetGrip.transform.TransformPoint(PuppetGrip.GripPosition) - tr.TransformPoint((Vector3)HoldingPosition);

                    return;
                }
            }
        }

        private void AnimateThrow()
        {
            Vector2 v = R.velocity;

            float angle  = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;

            R.MoveRotation(angle + ((angleAim - (angleOffset * facing)) * -1f));

            if (Time.time > throwTimeout) isThrown = false;
        }

        public void MakePersistant()
        {
            isPersistant = true;
            CancelInvoke();
        }

        public enum LodgedIntoTypes
        {
            Nothing,
            Person,
            Wall,
            Item,
        }

        public LodgedIntoTypes CheckCollisions()
        {
            CollisionCount             = 0;
            LodgedIntoTypes touchingType = LodgedIntoTypes.Nothing;

            List<Collider2D> col2dList = new List<Collider2D>();
            
            Rigidbody2D rb;

            foreach (Collider2D collider in tr.root.GetComponentsInChildren<Collider2D>())
            {
                if (!collider || collider == null) continue;

                ContactFilter2D contactFilter = new ContactFilter2D(); 
                contactFilter                 = contactFilter.NoFilter();
                CollisionCount                = (int)collider?.OverlapCollider(contactFilter, col2dList);

                if (CollisionCount > 0)
                {
                    foreach (Collider2D colliderOther in col2dList)
                    {
                        if (!colliderOther || colliderOther == null) continue;

                        rb = colliderOther.attachedRigidbody;

                        if (rb == R || rb.name == R.name) continue;
                        if (Puppet.RB2.ContainsValue(rb)) continue;

                        if (rb.name.Contains("Wall") || rb.name == "Root" )      touchingType = LodgedIntoTypes.Wall;
                        else if (rb.GetComponentInParent<PersonBehaviour>() != null) touchingType = LodgedIntoTypes.Person;
                        else touchingType = LodgedIntoTypes.Item;
                        
                        //Util.Notify("DoCollisionCheck: " + doCollisionCheck + " and Touching: " + touchingType.ToString() + " :" + rb.name);

                        return touchingType;

                    }
                }
            }

            doCollisionCheck = false;
            //Util.Notify("DoCollisionCheckX: " + doCollisionCheck);

            return touchingType;
        }

        public bool CheckLodged()
        {
            isLodged = false;
            bool notLodged = true;
            foreach (PhysicalBehaviour.Penetration penetration in P.penetrations)
            {
                if (penetration.Active) {
                    isLodged   = true;
                    StabVictim                  = penetration.Victim;
                    notLodged                   = false;

                    if (StabVictim.name.Contains("Wall") || StabVictim.name == "Root" ) LodgedIntoType = LodgedIntoTypes.Wall;
                    else if (StabVictim.TryGetComponent<PersonBehaviour>(out _)) LodgedIntoType = LodgedIntoTypes.Person;
                    else LodgedIntoType = LodgedIntoTypes.Item;
                }
                
            }
            
            if (notLodged) LodgedIntoType = LodgedIntoTypes.Nothing;

            return isLodged;
        }


        //
        // ─── COLLISIONS ────────────────────────────────────────────────────────────────
        //
        private void OnCollisionEnter2D(Collision2D coll=null)
        {
            if (isThrown) { isThrown = false; }
            //  if AttackDamage is not set, then we're not attacking
            if (AttackDamage <= 0f) return;

            if (coll == null) return;
            LimbBehaviour theHitLimb  = (LimbBehaviour)null;
            PersonBehaviour hitperson = coll.rigidbody.GetComponentInParent<PersonBehaviour>();
            
            if (hitperson != null && hitperson != Puppet.PBO)
            {
                if (AttackedList.Contains(hitperson.GetHashCode())) return;
                if (canStab)
                {
                    if (coll.rigidbody.TryGetComponent<LimbBehaviour>(out LimbBehaviour limbStabbed))
                    {
                        HitLimb = limbStabbed;

                        
                        //  Check if power was enough to do a finisher
                        if (Puppet.Actions.attack.CurrentMove == Attack.MoveTypes.slash && canSlice)
                        {
                            if (limbStabbed.TryGetComponent<CirculationBehaviour>(out CirculationBehaviour circulation))
                            {
                                circulation.CreateBleedingParticle((Vector2)limbStabbed.gameObject.transform.position, Vector2.left);
                            }
                            if (AttackDamage > 30 && AttackDamage < 40)
                            {
                                int health = Util.GetOverallHealth(hitperson);
                                if (health > 75) health = 75;
                                if (UnityEngine.Random.Range(1, 100) > health)
                                {
                                    //Util.Detach(hitperson, limbStabbed);
                                    limbStabbed.Slice();
                                    Util.Notify("Critical", Util.VerboseLevels.Minimal);
                                }
                            }
                        }

                        else if (Puppet.Actions.attack.CurrentMove == Attack.MoveTypes.stab)
                        {

                            if (AttackDamage > 30 && AttackDamage < 40)
                            {
                                int health = Util.GetOverallHealth(hitperson);
                                if (health > 75) health = 75;
                                if (UnityEngine.Random.Range(1, 100) > health)
                                {
                                    if (coll.rigidbody.TryGetComponent<PhysicalBehaviour>(out PhysicalBehaviour PB))
                                    {
                                        Stabbing stabbing = new Stabbing(P, PB, (Vector2)(tr.position - PB.transform.position).normalized, coll.collider.ClosestPoint(tr.position));

                                        limbStabbed.Stabbed(stabbing);
                                        Util.Notify("Critical", Util.VerboseLevels.Minimal);
                                    }
                                }
                            }

                            Invoke("CheckLodged", 0.5f);

                        }
                    }
                }
                else
                {
                    if (coll.rigidbody.TryGetComponent<LimbBehaviour>(out LimbBehaviour limbStabbed))
                    {
                        theHitLimb = limbStabbed;
                        //  Check if power was enough to do a finisher
                        if (Puppet.Actions.attack.CurrentMove == Attack.MoveTypes.slash)
                        {
                            if (AttackDamage > 30 && AttackDamage < 40)
                            {
                                int health = Util.GetOverallHealth(hitperson);
                                if (health > 75) health = 75;
                                if (UnityEngine.Random.Range(1, 100) > health)
                                {
                                    //Util.Detach(hitperson, limbStabbed);
                                    limbStabbed.Crush();
                                    Util.Notify("Critical", Util.VerboseLevels.Minimal);
                                }
                            }
                        }

                        else if (Puppet.Actions.attack.CurrentMove == Attack.MoveTypes.stab)
                        {
                            if (AttackDamage > 30 && AttackDamage < 40)
                            {
                                int health = Util.GetOverallHealth(hitperson);
                                if (health > 75) health = 75;
                                if (UnityEngine.Random.Range(1, 100) > health)
                                {
                                    if (coll.rigidbody.TryGetComponent<PhysicalBehaviour>(out _))
                                    {
                                        limbStabbed.BreakBone();
                                        Util.Notify("Critical", Util.VerboseLevels.Minimal);
                                    }
                                }
                            }

                        }

                    }
                }

                AttackedList.Add(hitperson.GetHashCode());

                hitperson.PainLevel  = 0f;
                hitperson.ShockLevel = 0f;
                if (theHitLimb != null)
                {
                    if (theHitLimb.Health  < theHitLimb.InitialHealth)  theHitLimb.Health += (theHitLimb.InitialHealth - theHitLimb.Health) / 2f;
                    if (theHitLimb.BruiseCount > 2) theHitLimb.BruiseCount--;
                }
            }
        }

        //
        // ─── HELD BY ────────────────────────────────────────────────────────────────
        //
        public PersonBehaviour HeldBy()
        {
            if (!P.beingHeldByGripper) return (PersonBehaviour) null;

            GripBehaviour[] Grips = Global.FindObjectsOfType<GripBehaviour>();

            foreach (GripBehaviour grip in Grips)
            {
                if (grip != null && grip.CurrentlyHolding == P)
                {
                    return grip.GetComponentInParent<PersonBehaviour>();
                }
            }

            return (PersonBehaviour) null;
        }


        //
        // ─── BREAK CONNECTIONS ────────────────────────────────────────────────────────────────
        //
        public void BreakConnections(bool clearPuppet=false)
        {

            GripBehaviour[] Grips = Global.main.gameObject.GetComponentsInChildren<GripBehaviour>();

            foreach (GripBehaviour grip in Grips)
            {
                if (grip.CurrentlyHolding != P) continue;

                // Break joint
                FixedJoint2D ItemJoint;
                        
                while (ItemJoint = grip.gameObject.GetComponent<FixedJoint2D>())
                {
                    UnityEngine.Object.DestroyImmediate(ItemJoint);
                }

            }

            foreach (PhysicalBehaviour.Penetration penetration in P.penetrations)
            {
                if (penetration.Active)
                {
                    penetration.Victim = null;
                    penetration.Active = false;
                    isLodged           = false;
                    StabVictim         = null;
                    LodgedIntoType     = LodgedIntoTypes.Nothing;
                }

            }

            P.beingHeldByGripper = false;


            if (clearPuppet)
            {
                if (Puppet?.GripF?.CurrentlyHolding == P) { Puppet.GripF.CurrentlyHolding = null; Puppet.GripF.isHolding = false; }
                if (Puppet?.GripB?.CurrentlyHolding == P) { Puppet.GripB.CurrentlyHolding = null; Puppet.GripF.isHolding = false; }
                    
                if (Puppet != null)
                {
                    Puppet.IsAiming           = false;
                    Puppet.IsPointing         = false;
                    Puppet.Actions.combatMode = false;
                    Puppet                    = null;
                }
            }
            
        }


        //
        // ─── DETACH ────────────────────────────────────────────────────────────────
        //
        public void Dropped()
        {
            if (Puppet != null)
            {
                Puppet     = null;
                PuppetGrip = null;

                LayerFix(false);
            }

            P.MakeWeightful();
            if (!isPersistant && !doNotDispose) Dispose(true);
            
        }

        //
        // ─── ATTACH PUPPET HAND ────────────────────────────────────────────────────────────────
        //
        public bool AttachPuppetHand(Puppet puppet, bool frontHand = true)
        {
            if (!hasDetails) GetDetails();

            P.MakeWeightless();
            
            Puppet                = puppet;
            PuppetArm             = frontHand ? Puppet.RB2["LowerArmFront"] : Puppet.RB2["LowerArm"];
            PuppetGrip            = frontHand ? Puppet.GripF : Puppet.GripB;
            GripBehaviour AltGrip = frontHand ? Puppet.GripB : Puppet.GripF;
            HoldingPosition       = new Vector3( float.MaxValue, 0.0f );

            foreach (Vector3 HPos in P.HoldingPositions) if (HPos.x < HoldingPosition.x) HoldingPosition = HPos;
            //SetHoldingPosition(HoldingPosition);

            if (AltGrip.isHolding)
            {
                PhysicalBehaviour AltItem = AltGrip.CurrentlyHolding;

                foreach (Collider2D col1 in AltItem.transform.root.GetComponentsInChildren<Collider2D>())
                {
                    foreach (Collider2D col2 in tr.root.GetComponentsInChildren<Collider2D>())
                    {
                        if ((bool)(UnityEngine.Object)col2 && (bool)(UnityEngine.Object)col1)
                            Physics2D.IgnoreCollision(col1, col2);
                    }
                }
            }

            ResetPosition();

            //  Adjust layers of the hand and the held object so object is over body but under hand and arms
            LayerFix(true);

            if (frontHand) Puppet.HoldingF         = this;

            if (protectFromFire != Puppet.FireProof) { 
                Puppet.RunLimbs(Puppet.LimbFireProof, protectFromFire);
                Puppet.RunUpdate = true;
                Puppet.FireProof = protectFromFire;
            }

            return true;

        }

        public void LayerFix(bool inHand)
        {
            if (inHand)
            {
                int SOrder = Puppet.RB2["LowerArmFront"].GetComponent<SpriteRenderer>().sortingOrder;
                if (SOrder <= 1)
                {
                    SOrder = 2;
                    Puppet.RB2["LowerArmFront"].GetComponent<SpriteRenderer>().sortingOrder = SOrder;
                    Puppet.RB2["UpperArmFront"].GetComponent<SpriteRenderer>().sortingOrder = SOrder;
                }

                P.GetComponent<SpriteRenderer>().sortingLayerName = Puppet.RB2["LowerArmFront"].GetComponent<SpriteRenderer>().sortingLayerName;
                P.GetComponent<SpriteRenderer>().sortingOrder     = SOrder + -1;
            }
            else
            {
                P.GetComponent<SpriteRenderer>().sortingLayerName = sortingLayerName;
                P.GetComponent<SpriteRenderer>().sortingOrder     = sortingOrder;
            }
        }


        //
        // ─── RESET POSITION ────────────────────────────────────────────────────────────────
        //
        public void ResetPosition(bool norotate=false, bool extraFlip=false)
        {
            bool thingFlipped           = IsFlipped;
            if (extraFlip) thingFlipped = !thingFlipped;
            
            if (Puppet.IsFlipped != thingFlipped) Flip();

            if (angleHold == 0f)
            {
                angleHold = (holdToSide && !norotate) ? 5.0f : 95.0f;
                angleAim  = 95.0f;
            }

            if (HoldingMove != "")
            {
                ActionPose.RunMove();
            }

            //if (P.Properties.SharpAxes.Length == 1) angleHold = angleAim = P.Properties.SharpAxes[0].Axis.x;

            float thingRotation = Puppet.IsAiming ? angleAim : angleHold;

            thingRotation += angleOffset;

            tr.rotation = Quaternion.Euler(0.0f, 0.0f, IsFlipped ? PuppetArm.rotation + thingRotation : PuppetArm.rotation - thingRotation);

            tr.position += PuppetGrip.transform.TransformPoint(PuppetGrip.GripPosition) - tr.TransformPoint((Vector3)HoldingPosition);

            PuppetArm.SendMessage("Use", (object)new ActivationPropagation(), SendMessageOptions.DontRequireReceiver);

            if (angleAim != angleHold)
            {
                tr.rotation = Quaternion.Euler(0.0f, 0.0f, IsFlipped ? PuppetArm.rotation + thingRotation : PuppetArm.rotation - thingRotation);
            }

            Util.DisableCollision(Puppet.PBO.transform, P, false);

            G.SetLayer(10);

            doCollisionCheck = true;

            if (twoHands) {
                P.MakeWeightless();
                doSecondHand    = true;
                secondHandDelay = Time.time + 1f;
            }


        }


        

        //
        // ─── FLIP ────────────────────────────────────────────────────────────────
        //
        public void Flip()
        {
            Vector3 scale = tr.localScale;
            scale.x      *= -1;
            tr.localScale = scale;
        }
        

        //
        // ─── ACTIVATE ────────────────────────────────────────────────────────────────
        //
        public void Activate(bool continuous=false)
        {
            P.SendMessage(continuous ? "UseContinuous" : "Use", (object)new ActivationPropagation(), SendMessageOptions.DontRequireReceiver);
        }


        //
        // ─── SET HOLDING POSITION ────────────────────────────────────────────────────────────────
        //
        //public void SetHoldingPosition(Vector3 newPosition, bool save=false)
        //{
        //    //  We call this when its detected player has repositioned an item
        //    //int hoPo = P.name.GetHashCode();

        //    //if (!save && ManualPositions.ContainsKey(hoPo))
        //    //{
        //    //    HoldingOffset = ManualPositions[hoPo];
        //    //    return;
        //    //}

        //    //HoldingOffset = newPosition;


        //    //if (save) {
                

        //    //    if (ManualPositions.ContainsKey(hoPo)) ManualPositions[hoPo] = HoldingOffset;
        //    //    else ManualPositions.Add(hoPo, HoldingOffset);

        //    //    Util.SaveHoldingPositions();

        //    //}
            
        //}


        //public void PrintPosition()
        //{
        //    ////Vector2 worldPoint = (Vector2)PuppetGrip.transform.TransformPoint(PuppetGrip.GripPosition);

        //    ////Vector2 localHoldingPoint;// = P.GetNearestLocalHoldingPoint(worldPoint, out float distance);

        //    //Vector3 offset;

        //    //offset  = tr.localPosition;

        //    //if (HoldingOffset.x == 0) {
                
        //    //    HoldingOffset = offset;

        //    //} else
        //    //{
        //    //    //Util.Notify("x:" + HoldingOffset.x + " y:" + HoldingOffset.y + " z:" + HoldingOffset.z);
        //    //    //Util.Notify("x2:" + offset.x + " y2:" + offset.y + " z2:" + offset.z);

        //    //}




        //    //HoldingOffset = tr.localPosition - (PuppetGrip.GripPosition - tr.TransformPoint((Vector3)localHoldingPoint));



        //    //ParentConstraint PC = G.GetOrAddComponent<ParentConstraint>();

        //    //PC.AddSource(new ConstraintSource() { sourceTransform = PuppetGrip.transform, weight = 1 });

        //    //PC.locked = true;

        //    //Vector3 offset = PC.GetTranslationOffset(0);


        //    return;

        //    //tr.parent = PuppetGrip.transform;

        //    //tr.localEulerAngles = new Vector3(0,0,PuppetArm.rotation + (Puppet.Facing * (angleHold + angleOffset)));
            
            
            
        //    //float thingRotation = Puppet.IsAiming ? angleAim : angleHold;

        //    //thingRotation += angleOffset;

        //    //Quaternion quack = Quaternion.Euler(0.0f, 0.0f, PuppetArm.rotation + (Puppet.Facing * thingRotation));
            
        //    //Vector3 handPo   = tr.position + (PuppetGrip.transform.TransformPoint(PuppetGrip.GripPosition) - tr.TransformPoint((Vector3)HoldingPosition));

        //    //float rotOffset   = quack.z - tr.rotation.z;
        //    //Vector3 posOffset = handPo - tr.position;
            
        //    //Util.Notify("Angle: " + quack.z   + " - X: " + handPo.x +    " - Y: " + handPo.y);
        //    //Util.Notify("AnOff: " + rotOffset + " - X: " + posOffset.x + " - Y: " + posOffset.y);


        //}

        public void GetPositionChange()
        {
            //  fuckthisfuckingfuckitybullshitnonworkingconfusingassholecodebolloxeddicklickingtubbybitchdontusedrugsreadafuckingbook
            //float fdirec = IsFlipped ? 1.0f : -1.0f;

            //Vector3 t2 = new Vector3(HoldingPosition.x, HoldingPosition.y);

            //Vector3 nhp = new Vector3(float.MaxValue, 0.0f);
            //foreach (Vector3 HPos in P.HoldingPositions) if (HPos.x < nhp.x) nhp = HPos;

            //float rotation = PuppetArm.rotation + (95.0f * fdirec);
            
            //Util.Notify("hpx: " + nhp.x + " hpY: " + nhp.y);
            //Util.Notify("Rotation: " + rotation);
            
            //Vector3 mypos = PuppetArm.transform.InverseTransformPoint(PuppetArm.transform.position) - tr.InverseTransformPoint(R.transform.position);
            //Util.Notify("X1: " + mypos.x + " Y1: " + mypos.y);

            ////tr.position += PuppetArm.transform.TransformPoint(PuppetArm.position) - tr.TransformPoint((Vector3)mypos);
            //tr.position += PuppetArm.transform.TransformPoint(PuppetArm.position) - tr.TransformPoint((Vector3)mypos);

            //// x: -0.4675879   y: -0.04876827
            //Vector3 offsetp = new Vector3(-0.4752571f, -1.795648f);


            //tr.position = PuppetGrip.transform.position - offsetp;


            //tr.position = PuppetGrip.transform.position + PuppetGrip.transform.TransformPoint(PuppetGrip.GripPosition) - tr.TransformPoint((Vector3)offsetp);

            //tr.rotation = Quaternion.Euler(0.0f, 0.0f, IsFlipped ? PuppetArm.rotation + 95.0f : PuppetArm.rotation - 95.0f);


            //Vector3 t3 = new Vector3(tr.TransformPoint(HoldingPosition).x, tr.TransformPoint(HoldingPosition).y);
            //Vector3 t3 = tr.TransformPoint((Vector3)HoldingPosition);
            //Util.Notify("X1: " + t3.x + " Y1: " + t3.y);

            //Vector3 t5 = PuppetGrip.GripPosition - tr.position;
            //Util.Notify("X2: " + t5.x + " Y2: " + t5.y);

            //Vector3 t4 = tr.InverseTransformPoint(t5);
            //Util.Notify("X2: " + t4.x + " Y2: " + t4.y);



            //tr.rotation = Quaternion.Euler(0.0f, 0.0f, IsFlipped ? PuppetArm.rotation + thingRotation : PuppetArm.rotation - thingRotation);

            //tr.position += PuppetGrip.transform.TransformPoint(PuppetGrip.GripPosition) - tr.TransformPoint((Vector3)HoldingPosition);


            //HoldingPosition = new Vector3(float.MaxValue, 0.0f);
            //foreach (Vector3 HPos in P.HoldingPositions) if (HPos.x < HoldingPosition.x) HoldingPosition = HPos;
            //SetHoldingPosition(HoldingPosition);


        }

        public void TurnItemOff()
        {
            if (isEnergySword && P.TryGetComponent<EnergySwordBehaviour>(out EnergySwordBehaviour esb))
            {
                itemActiveOGSetting = esb.Activated;
                if (esb.Activated) Activate();
            }
        }

        public void TurnItemOn(bool ifWasOn = false)
        {
            if (ifWasOn && !itemActiveOGSetting) return;

            if (isEnergySword && P.TryGetComponent<EnergySwordBehaviour>(out EnergySwordBehaviour esb))
            {
                
                if (!esb.Activated) Activate();
            }
        }

        public void DisableSelfDamage(bool option)
        {
            if (canBeDamaged && P.TryGetComponent<DamagableMachineryBehaviour>(out DamagableMachineryBehaviour dmb))
            {
                dmb.enabled = option;
            }
        }

        //
        // ─── GET DETAILS ────────────────────────────────────────────────────────────────
        //
        public void GetDetails()
        {
            const float LargeItemLength = 1.1f;
            const float SmallShivLength = 1.25f;

            string itemName = P.name.ToLower();


            hasDetails = true;

            

            //  Loop through items components and check for behaviour classes that identify if its auto or manual

            string AutoFire = @"ACCELERATORGUNBEHAVIOUR,FLAMETHROWERBEHAVIOUR,PHYSICSGUNBEHAVIOUR,MINIGUNBEHAVIOUR,TEMPERATURERAYGUNBEHAVIOUR";
            
            string ManualFire   = @"ARCHELIXCASTERBEHAVIOUR,BEAMFORMERBEHAVIOUR,ROCKETLAUNCHERBEHAVIOUR,GENERICSCIFIWEAPON40BEHAVIOUR,
                                    LIGHTNINGGUNBEHAVIOUR,PULSEDRUMBEHAVIOUR,HEALTHGUNBEHAVIOUR,FREEZEGUNBEHAVIOUR,SINGLEFLOODLIGHTBEHAVIOUR";

            string FireProtection = @"ENERGYSWORDBEHAVIOUR,FLAMETHROWERBEHAVIOUR";
            
            string Damageable = @"ENERGYSWORDBEHAVIOUR";
            string EnergySword = @"ENERGYSWORDBEHAVIOUR";

            MonoBehaviour[] Components = P.GetComponents<MonoBehaviour>();

            if (Components.Length > 0)
            {
                for (int i = Components.Length; --i >= 0;)
                {
                    string compo = Components[i].GetType().ToString().ToUpper();

                    if (Damageable.Contains(compo)) canBeDamaged        = true;
                    if (EnergySword.Contains(compo)) isEnergySword      = true;
                    if (FireProtection.Contains(compo)) protectFromFire = true;

                    if (AutoFire.Contains(compo))
                    {
                        canShoot    = true;
                        isAutomatic = true;
                    }

                    if (ManualFire.Contains(compo))
                    {
                        canShoot    = true;
                        isAutomatic = false;
                    }
                }
            }

            //  @Todo: need to return modified values to their OG after they're dropped

            //  Determine if we can do auto firing

            if (P.TryGetComponent(out FirearmBehaviour FBH))
            {
                canShoot             = true;
                isAutomatic          = FBH.Automatic;
                FBH.Cartridge.Recoil = 0.1f;
            }
            else if (P.TryGetComponent(out ProjectileLauncherBehaviour PLB))
            {
                canShoot             = true;
                isAutomatic          = PLB.IsAutomatic;
                PLB.recoilMultiplier = 0.1f;
            }
            else if (P.TryGetComponent(out BlasterBehaviour BB))
            {
                canShoot             = true;
                isAutomatic          = BB.Automatic;
                BB.Recoil            = 0.1f;
            }
            else if (P.TryGetComponent(out BeamformerBehaviour BB2))
            {
                BB2.RecoilForce      = 0.1f;
            }
            else if (P.TryGetComponent(out GenericScifiWeapon40Behaviour GWB))
            {
                GWB.RecoilForce = 0.1f;
            }
            //else if (P.TryGetComponent(out AcceleratorGunBehaviour AB))
            //{
            //    AB.RecoilIntensity = 0.1f;
            //    canShoot = true;
            //    isAutomatic = true;
            //}
            else if (P.TryGetComponent<ArchelixCasterBehaviour>(out _))
            {
                canShoot    = true;
                isAutomatic = false;
            }
            else if (P.TryGetComponent<SingleFloodlightBehaviour>(out _))
            {
                isFlashlight = true;
                angleHold    = 95f;
                angleAim     = 180f;
                canAim       = true;
            } 
            
            //  Flag Larger Items since they interfere with walking
            holdToSide = isFlashlight || (canShoot && P.ObjectArea > LargeItemLength);
            size       = Mathf.Max(tr.lossyScale.x, tr.lossyScale.y);

            //  Check for clubs/bats
            if (!canShoot)
            {
                canStab     = (P.Properties.Sharp && P.StabCausesWound) || isSyringe;
                canStrike   = true;
                //twoHands    = size > 1.1f;
                canSlice    = P.TryGetComponent<SharpOnAllSidesBehaviour>(out _);

                if (canStab) {
                    if (P.ObjectArea < SmallShivLength) isShiv = true;
                }

            } else
            {
                canAim      = true;
            }

            if (itemName.Contains("syringe"))
            {
                angleOffset = -95f;
                isSyringe   = true;
                isShiv      = true;
            }
            else if (itemName.Contains("crystal"))
            {
                angleOffset = -95f;
            }
            else if (itemName.Contains("bulb"))
            {
                isShiv      = true;
                angleOffset = 180f;
            }
            else if (itemName.Contains("stick"))
            {
                canStrike    = true;
                angleOffset  = 180f;
            }
            else if (itemName.Contains("rod"))
            {
                canStrike   = true;
                angleOffset = 180f;
            }
            else if (itemName.Contains("bolt"))
            {
                isShiv      = true;
                angleOffset = -90f;
            }

            if (P.TryGetComponent<ChainsawBehaviour>(out _))
            {
                isChainSaw  = true;
                twoHands    = true;
                HoldingMove = "chainsaw";
                canStab     = false;
                canSlice    = false;
                canStrike   = false;
            }

            if (HoldingMove != "")
            {
                ActionPose                                  = new MoveSet(HoldingMove, false);
                ActionPose.Ragdoll.ShouldStandUpright       = true;
                ActionPose.Ragdoll.State                    = PoseState.Rest;
                ActionPose.Ragdoll.Rigidity                 = 2.3f;
                ActionPose.Ragdoll.ShouldStumble            = false;
                ActionPose.Ragdoll.AnimationSpeedMultiplier = 2.5f;
                ActionPose.Ragdoll.UprightForceMultiplier   = 1f;
                ActionPose.Import();
            }

        }


        protected virtual void Dispose(bool disposing)
        {
            Destroy(false);
        }

        public void Destroy(bool deleteThing=true)
        {
            Puppet?.DropThing();
            
            BreakConnections();

            if (deleteThing && (bool)P?.Deletable)
            {
                G.SendMessage("OnUserDelete", SendMessageOptions.DontRequireReceiver);

                UnityEngine.Object.Destroy((UnityEngine.Object)G);
            }

            Util.Destroy(this);

        }
        
    }
}
