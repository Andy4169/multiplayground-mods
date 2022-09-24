//     ___                        _                    _     
//    / _ \_   _ _ __  _ __   ___| |_  /\/\   __ _ ___| |_ ___ _ __ 
//   / /_)/ | | | '_ \| '_ \ / _ \ __|/    \ / _` / __| __/ _ \ '__|
//  / ___/| |_| | |_) | |_) |  __/ |_/ /\/\ \ (_| \__ \ ||  __/ |  
//  \/     \__,_| .__/| .__/ \___|\__\/    \/\__,_|___/\__\___|_|  
//              |_|   |_|                
//                                           
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuppetMaster
{
    [SkipSerialisation]
    public class PuppetBike : MonoBehaviour
    {
        [SkipSerialisation] public Puppet Puppet;
        [SkipSerialisation] public Transform BikeT;
        [SkipSerialisation] public PhysicalBehaviour Frame;
        [SkipSerialisation] public PhysicalBehaviour Pedal;
        [SkipSerialisation] public PhysicalBehaviour Dinges;
        [SkipSerialisation] public PhysicalBehaviour Wheel1;
        [SkipSerialisation] public PhysicalBehaviour Wheel2;

        [SkipSerialisation] public bool Registered = false;

        [SkipSerialisation] public FixedJoint2D seatJoint;
        [SkipSerialisation] public FixedJoint2D hbarJoint;
        [SkipSerialisation] public FixedJoint2D pedalJoint;
        [SkipSerialisation] public WheelJoint2D pedalJointF;
        [SkipSerialisation] public SpringJoint2D pedalJointB;

        [SkipSerialisation] public EBikeBehaviour EBike;
        [SkipSerialisation] public HingeJoint2D Hinge;
        [SkipSerialisation] public JointMotor2D Motor;

        [SkipSerialisation] public float Facing => (float)BikeT.localScale.x < 0.0f ? 1f : -1f;

        [SkipSerialisation] public float SpeedupCounter = 0f;
        [SkipSerialisation] public float TopSpeed       = 100f;
        [SkipSerialisation] public float WheelSpan      = 0f;

        private float animationSpeed = 0f;

        //private List<GameObject> CustomBikeParts = new List<GameObject>();


        private float bikeTimer      = 0f;
        private float FrameLife      = 0f;

        private bool pullPopped      = false;
        private bool downFixed       = false;

        private int skipFrames       = 0;

        private int currentFrame     = 0;
        private int counter          = 0;

        private bool doFlip          = false;
        private bool wasTurbo        = false;


        [SkipSerialisation] public float TotalWeight     = 0f;


        [SkipSerialisation] public MoveSet[] APBike = new MoveSet[2];

        [SkipSerialisation]
        public enum BikeStates
        {
            Ready,
            Summon,
            GettingOn,
            Riding,
            JumpedOff,
            SendOff,
        }

        [SkipSerialisation] public BikeStates state = BikeStates.Ready;


        

        //
        // ─── UNITY FIXED UPDATE ────────────────────────────────────────────────────────────────
        //
        [SkipSerialisation]
        public void FixedUpdate()
        {
            if (skipFrames > 0)
            {
                --skipFrames; 
                return;
            }
            if (doFlip) {
                FlipBike(true);
                skipFrames = 3;
                return;
            }



                if (Time.frameCount % 100 == 0)
                {
                    CheckBikeStatus();
                }

            if (state == BikeStates.GettingOn)
            {
                Wheel1.rigidbody.angularVelocity *= 0.001f;
                Wheel2.rigidbody.angularVelocity *= 0.001f;

                GetOnBike();
            }
            else

            if (state == BikeStates.Riding)
            {
                UpdateSpeedView();

                if (KB.Alt)
                {
                    PhysicalBehaviour Wheel;
                    if (KB.Down)
                    {
                        Wheel = Facing < 0 ? Wheel2 : Wheel1;
                        
                        if (Wheel != null)
                        {
                            Wheel.rigidbody.angularVelocity = 0f;
                            Vector2 force = Wheel.rigidbody.velocity;
                            Wheel.rigidbody.AddForceAtPosition(-force * 2.5f, Wheel.rigidbody.position);
                        }
                    }
                    else if (KB.Up)
                    {
                        Wheel = Facing < 0 ? Wheel1 : Wheel2;

                        if (Wheel != null)
                        {
                            Wheel.rigidbody.angularVelocity = 0f;
                            Vector2 force = Wheel.rigidbody.velocity;
                            Wheel.rigidbody.AddForceAtPosition((-force) + (Vector2.down * 5f), Wheel.rigidbody.position);
                            if (Mathf.Abs(force.magnitude) > 5f) Wheel.Sizzle();
                        }
                    }
                    else if (KB.Left && KB.Right)
                    {
                        GetOffBike();
                        return;
                    }
                    else 
                    {
                        Wheel = (Facing != Mathf.Sign(Frame.rigidbody.velocity.x)) ? Wheel1 : Wheel2;
                        if (Wheel != null)
                        {
                            Wheel.rigidbody.angularVelocity = 0f;
                            Vector2 force = Wheel.rigidbody.velocity;
                            Wheel.rigidbody.AddForceAtPosition((-force) + (Vector2.down * 5f), Wheel.rigidbody.position);
                            if (Mathf.Abs(force.magnitude) > 5f) Wheel.Sizzle();
                        }
                    }
                }
                else if ((KB.Right && !Puppet.FacingLeft) || (KB.Left && Puppet.FacingLeft))
                {
                    // going forward
                    SetSpeed((KB.Shift ? 10 : 1) * Facing);
                    AnimatePedal();
                }
                else if ((KB.Right && Puppet.FacingLeft) || (KB.Left && !Puppet.FacingLeft))
                {
                    //  going backward
                    SetSpeed((KB.Shift ? 10 : 1) * -Facing);
                    AnimatePedal();
                }
                else
                {
                    // COASTING
                    SetSpeed(Frame.rigidbody.velocity.magnitude * 190f * -Mathf.Sign(Frame.rigidbody.velocity.x),false,false);

                    if (KB.Alt)
                    {
                        
                    }
                }

                if (KB.Up)
                {
                    if (pullPopped)
                    {
                        Frame.rigidbody.AddTorque(200f * -Facing * TotalWeight * Puppet.TotalWeight * Time.fixedDeltaTime, ForceMode2D.Force);
                    }
                    else
                    {
                        Frame.rigidbody.AddTorque(25f * -Facing * TotalWeight * Puppet.TotalWeight * Time.fixedDeltaTime, ForceMode2D.Impulse);
                        pullPopped = true;
                    }
                } else pullPopped = false;

                if (KB.Down)
                {
                    if (downFixed)
                    {
                        Frame.rigidbody.AddTorque(200f * Facing * TotalWeight * Puppet.TotalWeight * Time.fixedDeltaTime, ForceMode2D.Force);
                    }
                    else
                    {
                        Frame.rigidbody.AddTorque(25f * Facing * TotalWeight * Puppet.TotalWeight * Time.fixedDeltaTime, ForceMode2D.Impulse);
                        downFixed = true;
                    }

                } else downFixed = false;
            }

            if (state == BikeStates.JumpedOff)
            {
                if (Time.time > bikeTimer)
                {
                    if (!Util.IsColliding(BikeT, Puppet.RB2["Head"].transform.root, false))
                    {
                        Util.ToggleCollisions(Frame.transform, Puppet.RB2["LowerBody"].transform, true,false);
                        state = BikeStates.Ready;
                    }
                }
            }
        }
        private void UpdateSpeedView()
        {
            //SpeedView.text = string.Format("{0} km/h", Math.Round(Frame.rigidbody.velocity.magnitude, 1));
            Effects.Speedometer(Effects.SpeedometerTypes.Bike, Frame.rigidbody.velocity.magnitude);
        }
        private void AnimatePedal()
        {
            if (animationSpeed == 0 || Time.frameCount % 100 == 0 || KB.Shift != wasTurbo)
            {
                wasTurbo        = KB.Shift && Mathf.Abs(Motor.motorSpeed) > 1000;
                animationSpeed  = wasTurbo ? 0.3f : 1f;
            }
            
            int animationFrame   = (Mathf.RoundToInt(Time.time / animationSpeed) % 2);

            if (animationFrame   == currentFrame) return;

            currentFrame = animationFrame;
            APBike[animationFrame].RunMove();
        }


        public void SetSpeed(float speed, bool delta=true, bool turnOn=true) 
        {
            if (!delta) Motor.motorSpeed = speed;
            else Motor.motorSpeed += speed;

            Hinge.motor    = Motor;
            Hinge.useMotor = turnOn;
        }



        public void CheckBikeStatus()
        {
            if (BikeT == null || Frame == null || Hinge == null)
            {
                Util.DestroyNow(hbarJoint);
                Util.DestroyNow(pedalJointF);
                Util.DestroyNow(pedalJointB);
                Util.DestroyNow(seatJoint);

                Puppet.IsInVehicle = false;

                APBike[0].ClearMove();
                APBike[1].ClearMove();

                Puppet.RunRigids(Puppet.RigidReset);

                Util.Destroy(this);
            }
        }

        //
        // ─── REGISTER BIKE ────────────────────────────────────────────────────────────────
        //
        public bool RegisterBike()
        {
            if (state == BikeStates.Riding) {
                GetOffBike();
                return false;
            }

            TotalWeight = 0;
            Frame       = gameObject.GetComponent<PhysicalBehaviour>();
            BikeT       = Frame.transform.root;
            
            foreach (PhysicalBehaviour pb in BikeT.gameObject.GetComponentsInChildren<PhysicalBehaviour>())
            {
                if (pb.name == "Pedal")  Pedal  = pb;
                if (pb.name == "Dinges") Dinges = pb;
                if (pb.name == "Wheel1") Wheel1 = pb;
                if (pb.name == "Wheel2") Wheel2 = pb;
                TotalWeight += pb.rigidbody.mass;
            }

            if (gameObject.TryGetComponent<EBikeBehaviour>(out EBike))
            {
                Hinge = EBike.Hinge;
                Motor = Hinge.motor;
            };

            DisableCollisions();
            Util.ToggleCollisions(Frame.transform, Puppet.RB2["LowerBody"].transform,true,false);

            if (Registered) return false;

            Registered = true;
            return true;


        }

        public void DisableCollisions()
        {
            Util.ToggleCollisions(BikeT, Puppet.RB2["Head"].transform.root, false, true);
        }

        public void ResetBike()
        {
            Util.DestroyNow(hbarJoint);
            Util.DestroyNow(pedalJointF);
            Util.DestroyNow(pedalJointB);
            Util.DestroyNow(seatJoint);

            bikeTimer = Time.time + 2f;
            counter   = -2;

            Puppet.IsInVehicle = false;

            APBike[0].ClearMove();
            APBike[1].ClearMove();

            Puppet.RunRigids(Puppet.RigidReset);
            GetOnBike();
        }

        //
        // ─── GET OFF BIKE ────────────────────────────────────────────────────────────────
        //
        public void GetOffBike()
        {
            SetSpeed(0f,false,false);
            DisableCollisions();

            Util.Destroy(hbarJoint);
            Util.Destroy(pedalJointF);
            Util.Destroy(pedalJointB);
            Util.Destroy(seatJoint);

            Puppet.PBO.OverridePoseIndex = -1;

            Puppet.RunRigids(Puppet.RigidReset);

            state              = BikeStates.JumpedOff;
            bikeTimer          = Time.time + 1f;
            Puppet.IsInVehicle = false;

            Action offBike     = () => { 
                Puppet.IsInVehicle           = false; 
                Puppet.PBO.OverridePoseIndex = -1;
            };

            Effects.Speedometer(Effects.SpeedometerTypes.Off, 0);

            PuppetMaster.Master.AddTask(offBike, 1.0f);

        }


        //
        // ─── GET ON BIKE ────────────────────────────────────────────────────────────────
        //
        [SkipSerialisation]
        public void GetOnBike()
        {
            Util.Notify("Getting on Bike", Util.VerboseLevels.Full);
            
            Puppet.IsInVehicle = true;
            
            if (++counter <= 0) return;

            if (counter == 1)
            {
                Puppet.Inventory.PutAwayItems();

                DisableCollisions();

                for (int i = 0; i <= 1; i++)
                {
                    string moveName = string.Format("bicycle_{0}", i + 1);
                    APBike[i]                                  = new MoveSet(moveName, false);
                    APBike[i].Ragdoll.ShouldStandUpright       = false;
                    APBike[i].Ragdoll.State                    = PoseState.Rest;
                    APBike[i].Ragdoll.Rigidity                 = 2.2f;
                    APBike[i].Ragdoll.ShouldStumble            = false;
                    APBike[i].Ragdoll.AnimationSpeedMultiplier = 1.5f;
                    APBike[i].Import();
                }

                Puppet.RB2["UpperLegFront"].mass = 0.001f;
                Puppet.RB2["UpperLeg"].mass      = 0.001f;
                Puppet.RB2["LowerLeg"].mass      = 0.001f;
                Puppet.RB2["LowerLegFront"].mass = 0.001f;
                Puppet.RB2["Foot"].mass          = 0.001f;
                Puppet.RB2["FootFront"].mass     = 0.001f;

                bikeTimer = Time.time + 2f;


                return;
            }

            //  -------------------------------------------------

            APBike[0].RunMove();

            Vector2 seatPos  = Frame.rigidbody.position;
            seatPos.y       += 0.65f;
            seatPos.x       -= 0.35f * -Facing;

            Vector2 hbarsPos = Frame.rigidbody.position;
            hbarsPos.y      += 0.65f;
            hbarsPos.x      += 0.37f * -Facing;

            if (Time.time > bikeTimer)
            {
                seatJoint                              = Frame.gameObject.AddComponent<FixedJoint2D>();
                seatJoint.anchor                       = Frame.transform.InverseTransformPoint(seatPos);
                seatJoint.connectedBody                = Puppet.RB2["LowerBody"];
                seatJoint.connectedAnchor              = Puppet.RB2["LowerBody"].transform.InverseTransformPoint(Puppet.RB2["LowerBody"].position);
                seatJoint.enableCollision              = false;
                seatJoint.autoConfigureConnectedAnchor = true;

                hbarJoint                              = Frame.gameObject.AddComponent<FixedJoint2D>();
                hbarJoint.anchor                       = Frame.transform.InverseTransformPoint(hbarsPos);
                hbarJoint.connectedBody                = Puppet.RB2["LowerArm"];
                hbarJoint.connectedAnchor              = Puppet.RB2["LowerArm"].transform.InverseTransformPoint(Puppet.RB2["LowerArm"].position);
                hbarJoint.enableCollision              = false;
                hbarJoint.autoConfigureConnectedAnchor = true;
                hbarJoint.frequency                    = 100f;
                hbarJoint.dampingRatio                 = 100f;

                state  = BikeStates.Riding;
                Util.ToggleCollisions(Frame.transform, Puppet.RB2["LowerBody"].transform, true, false);

                //Effects.ShowText("bike", "0 km/h", new Vector2(2,2));   
                //ToggleCollisions(true);

            }

            Puppet.RB2["LowerBody"].MoveRotation(-17f * Puppet.Facing);
            Puppet.RB2["LowerBody"].MovePosition(seatPos);

            Puppet.RB2["LowerArm"].MovePosition(hbarsPos);
            Puppet.RB2["LowerArmFront"].MovePosition(hbarsPos);

            //  --------------------------------------------------

        }


        void FlipBike(bool AfterFrame=false)
        {
            if (!AfterFrame)
            {
                Transform bicycle = Frame.transform.root;
                bicycle.position  = Frame.transform.position;
                doFlip            = true;
                return;
            }

            Vector3 flipper = Frame.transform.root.localScale;
            flipper.x *= -1;
            
            Vector3 pos = Frame.transform.position;

            Frame.transform.root.localScale = flipper;
            Frame.transform.root.position   = pos;

            doFlip  = false;
        }


        //
        // ─── COLLISIONS ────────────────────────────────────────────────────────────────
        //
        private void OnCollisionEnter2D(Collision2D coll = null)
        {
            if (coll == null) return;

            if (coll.gameObject.name == "LowerBody" && state == BikeStates.Ready)
            {
                DisableCollisions();

                if (Facing != Puppet.Facing) FlipBike();

                Puppet.Actions.jump.state     = JumpState.ready;
                Puppet.Actions.dive.state     = DiveState.ready;
                Puppet.Actions.backflip.state = BackflipState.ready;
                Puppet.PBO.OverridePoseIndex  = -1;

                Puppet.RunRigids(Puppet.RigidReset);

                counter = 0;
                state   = BikeStates.GettingOn;
            }
        }
    }
}
