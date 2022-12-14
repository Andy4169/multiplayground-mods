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
    //
    // ─── MOVESET CLASS ────────────────────────────────────────────────────────────────
    //
    public class MoveSet
    {
        public Puppet Puppet;
        public int poseID        = -1;
        public string MovesList  = "";
        public bool doReset      = false;
        public bool autoStart    = false;
        public bool comboMove    = false;

        public readonly RagdollPose Ragdoll;

        public Dictionary<string, LimbMove> moves = new Dictionary<string, LimbMove>();

        public string MoveHash => "PM" + MovesList.GetHashCode();

        public struct LimbMove
        {
            public string limbName;
            public float angle;
            public float rigid;
            public float force;
       }

        //
        // ─── NEW MOVESET ────────────────────────────────────────────────────────────────
        //
        public MoveSet(string movesList, bool autoStart=true, RagdollPose presetPose=null)
        {
            MovesList = movesList;
            Puppet    = PuppetMaster.Puppet;
            Ragdoll   = new RagdollPose()
            {
                Name                     = MoveHash,
                Rigidity                 = 4,
                ShouldStandUpright       = true,
                DragInfluence            = 1,
                UprightForceMultiplier   = 1,
                ShouldStumble            = true,
                State                    = 0,
                AnimationSpeedMultiplier = 1,
                Angles                   = new List<RagdollPose.LimbPose>(),
            };

            if (presetPose != null)
            {
                Ragdoll      = presetPose.ShallowClone();
                Ragdoll.Name = MoveHash;
                comboMove    = true;
            }

            if (autoStart) Import();
        }


        public string[] GetPoseData(string movesList)
        {
            if (movesList.Contains(",")) return MovesList.Trim().Split(',');

            if (MoveData.ContainsKey(movesList)) return MoveData[movesList].Split(',');

            return null;

        }

        //
        // ─── IMPORT MOVESET ────────────────────────────────────────────────────────────────
        //
        public void Import()
        {
            string[] mvItem = GetPoseData(MovesList);

            for (int i = mvItem.Length; --i >= 0;)
            {
                string[] parts  = mvItem[i].Trim().Split(':');
                if (parts.Length < 2) continue;
                
                //  Skip commented out limbs
                if ("#/".Contains("" + parts[0][0])) continue;

                LimbMove move   = new LimbMove()
                {
                    limbName    = parts[0],
                    angle       = 0f,
                    force       = 4f,
                    rigid       = 0f,
                };

                if (float.TryParse(parts[1], out float angle)) move.angle = angle;

                if (parts.Length > 2) { 
                    for (int n = parts.Length; --n >= 2;)
                    {
                        string item = parts[n].Trim();

                        if (item.StartsWith("f")      && float.TryParse(parts[n].Substring(1), out float force)) move.force = force;
                        else if (item.StartsWith("r") && float.TryParse(parts[n].Substring(1), out float rigid)) move.rigid = rigid;
                    }
                }

                moves.Add(parts[0], move);
            }

            RagdollPose.LimbPose limbPose;

            bool includeMove;

            foreach (LimbBehaviour limb in Puppet.LB.Values)
            {
                includeMove = false;

                if (limb.HasJoint)
                {
                    if (moves.ContainsKey(limb.name))
                    {
                        includeMove = true;
                    }
                }

                if (includeMove)
                {
                    limbPose = new RagdollPose.LimbPose(limb, moves[limb.name].angle)
                    {
                        Name                 = MoveHash,
                        Limb                 = limb,
                        Animated             = false,
                        PoseRigidityModifier = moves[limb.name].rigid,
                        StartAngle           = 0f,
                        EndAngle             = 0f,
                        AnimationDuration    = 0f,
                        RandomInfluence      = 0f,
                        RandomSpeed          = 0f,
                        TimeOffset           = 0f,
                        AnimationCurve       = new AnimationCurve(),
                    };
                }
                else if(!comboMove)
                {
                    limbPose = new RagdollPose.LimbPose(limb,0f)
                    {
                        Name                 = MoveHash,
                        Limb                 = limb,
                        Animated             = false,
                        PoseRigidityModifier = 0f,
                        Angle                = 0f,
                        StartAngle           = 0f,
                        EndAngle             = 0f,
                        AnimationDuration    = 0f,
                        RandomInfluence      = 0f,
                        RandomSpeed          = 0f,
                        TimeOffset           = 0f,
                        AnimationCurve       = new AnimationCurve(),
                    };
                } else continue;

                if (comboMove)
                {
                    for (int i = Ragdoll.Angles.Count; --i >= 0;)
                    {
                        if (Ragdoll.Angles[i].Limb == limb)
                        {
                            Ragdoll.Angles[i] = limbPose;
                        }
                    }
                } else 
                {
                    Ragdoll.Angles.Add(limbPose);
                }
            }

            Ragdoll.ConstructDictionary();

            for (int pnum = Puppet.PBO.Poses.Count; --pnum >= 0;)
            {
                if (Puppet.PBO.Poses[pnum].Name == MoveHash)
                {
                    poseID = pnum;

                    Puppet.PBO.Poses[pnum] = Ragdoll;

                    return;
                }
            }

            Puppet.PBO.Poses.Add(Ragdoll);
            
            poseID = Puppet.PBO.Poses.Count -1;

        }


        public bool RunMove()
        {
            Puppet.PBO.OverridePoseIndex = poseID;
            return true;
        }


        //
        // ─── COMBINE MOVESET ────────────────────────────────────────────────────────────────
        //
        public void CombineMove()
        {
            //  Only pose applicable limbs (continue previous actions)
            foreach (KeyValuePair<string, LimbMove> pair in moves)
            {
                Puppet.LB[pair.Key].InfluenceMotorSpeed(Mathf.DeltaAngle(
                    Puppet.LB[pair.Key].Joint.jointAngle, pair.Value.angle * Puppet.PBO.AngleOffset) * pair.Value.force);

            }

        }

        public void ClearMove()
        {
            foreach (string limbName in moves.Keys)
            {
                if (Puppet.LB?[limbName])
                {
                    Puppet.LB[limbName].Health                = Puppet.LB[limbName].InitialHealth;
                    Puppet.LB[limbName].Numbness              = 0.0f;
                    Puppet.LB[limbName].HealBone();

                    Puppet.LB[limbName].CirculationBehaviour.BleedingRate  *= 0.5f;
                    Puppet.LB[limbName].PhysicalBehaviour.BurnProgress     *= 0.5f;
                    Puppet.LB[limbName].SkinMaterialHandler.AcidProgress   *= 0.5f;
                    Puppet.LB[limbName].SkinMaterialHandler.RottenProgress *= 0.5f;
                }
            }

            if (!Puppet.IsCrouching && Puppet.Actions.prone.state == ProneState.ready) Puppet.PBO.OverridePoseIndex = -1;
        }

        //
        // ─── POSE DATA ────────────────────────────────────────────────────────────────
        //
        private static readonly Dictionary<string, string> MoveData = new Dictionary<string, string>()
        {
            { "throw_1",
              @"UpperArmFront:53.87996,
                UpperArm:0.1090901,
                LowerLegFront:-0.01318056,
                LowerArmFront:-36.86349,
                LowerArm:-0.4033567,
                UpperLegFront:3.002661,
                UpperLeg:-0.003749454,
                LowerBody:-0.05592484,
                LowerLeg:-0.007670409"
            },
            { "throw_2",
              @"LowerArm:0.1230614,
                UpperArm:0.117004,
                LowerLeg:0.001100474,
                LowerLegFront:0.001414623,
                UpperArmFront:174.287,
                LowerArmFront:86.93764,
                UpperLeg:0.007332622,
                UpperLegFront:0.00374265"
            },
            { "crouch",
              @"Head:-0.0002471675,
                UpperArm:0.1,
                #UpperArmFront:0.1,
                UpperLegFront:-75.54669,
                UpperLeg:-0.03494752,
                LowerLeg:106.0683,
                LowerLegFront:78.07973,
                Foot:-24.21083,
                FootFront:0.4032795"
            },
            { "dive",
              @"UpperArm:-136.7963,
                LowerBody:-11.74754,
                UpperArmFront:-136.7963,
                UpperBody:-8.516563,
                LowerArm:-34.3288,
                UpperLeg:-74.00098,
                UpperLegFront:-67.9553,
                LowerArmFront:-33.82881,
                LowerLegFront:63.65865,
                LowerLeg:60.26822"
            },
            { "backflip_1",
              @"Foot:-34.77201,
                FootFront:-27.44459,
                LowerArm:-30.00827,
                LowerArmFront:-38.24268,
                LowerLeg:95.33749,
                LowerLegFront:100.3498,
                UpperArm:-157.7551,
                UpperArmFront:-149.6151,
                UpperLeg:-28.73837,
                UpperLegFront:-27.9621"
            },
            { "backflip_2",
              @"Foot:51.61726,
                FootFront:50.23609,
                Head:0.8721468,
                LowerArm:-30.56421,
                LowerArmFront:-22.44059,
                LowerBody:-1.73008,
                LowerLeg:39.59584,
                LowerLegFront:52.36701,
                UpperArm:155.1894,
                UpperArmFront:-196.8672,
                UpperBody:-0.8053715,
                UpperLeg:0.4376444,
                UpperLegFront:-0.03437976"
            },
            { "jump",
              @"LowerArm:-95.09151,
                LowerArmFront:-85.78412,
                UpperArm:-376.2525,
                UpperArmFront:-369.4041,
                UpperLegFront:-111.5341,
                UpperLeg:-112.034,
                LowerLeg:133.9088,
                LowerLegFront:134.5014"
            },
            { "jump_spin",
              @"LowerBody:4.073051,
                UpperBody:1.87907,
                UpperArm:-77.37722,
                UpperArmFront:-82.49118,
                LowerLeg:98.53986,
                Foot:52.00001,
                LowerLegFront:105.6584,
                FootFront:50.32719,
                LowerArmFront:-122.3479,
                LowerArm:-118.9032,
                UpperLeg:-110.6554,
                UpperLegFront:-112"
            },
            { "prone",
              @"Head:-6.500257, 
                LowerBody:17.68937,
                UpperArmFront:-18.54307,
                UpperArm:-16.35537,
                LowerLegFront:9.899184,
                LowerLeg:11.33566,
                Foot:0.02769642, 
                FootFront:0.04737419,
                UpperLegFront:16.45826,
                UpperLeg:15.87339,
                LowerArmFront:-103.8956,
                UpperBody:-17.44694,
                LowerArm:-102.0114"
            },
            { "jumpsword_1",
              @"Foot:-30.16558,
                FootFront:37.30453,
                #Head:-0.004139095,
                LowerArm:-72.21709,
                LowerArmFront:-85.00078,
                LowerBody:0.5152695,
                LowerLeg:-248.4472,
                LowerLegFront:-257.0258,
                UpperArm:-100.6666,
                UpperArmFront:-92.74561,
                UpperBody:0.00395468,
                UpperLeg:46.62198,
                UpperLegFront:46.22927"
            },
            { "jumpsword_2",
              @"Foot:-31.99515,
                FootFront:50.46266,
                #Head:0.008920227,
                LowerArm:-102.5917,
                LowerArmFront:-110.0015,
                LowerBody:42.94868,
                LowerLeg:-0.1923655,
                LowerLegFront:13.52737,
                UpperArm:387.0154,
                UpperArmFront:382.127,
                UpperBody:-5.140564,
                UpperLeg:-94.6385,
                UpperLegFront:45.05071"
            },
            { "spearhold",
              @"Foot:-30.16564,
                FootFront:37.3046,
                Head:0.0003141887,
                LowerArm:-430.0918,
                LowerArmFront:-443.1978,
                LowerBody:-3.46585,
                LowerLeg:-249.7297,
                LowerLegFront:-258.3062,
                UpperArm:-89.24717,
                UpperArmFront:-81.00411,
                UpperBody:-17.57518,
                UpperLeg:47.90446,
                UpperLegFront:47.50976"
            },
            { "spear_1",
              @"Foot:0.3362804,
                FootFront:-0.4490192,
                Head:-0.0003978585,
                LowerArm:-0.0007299231,
                LowerArmFront:-121.741,
                LowerBody:-0.004964107,
                LowerLeg:-0.002408175,
                LowerLegFront:-0.000129987,
                UpperArm:-0.01419425,
                UpperArmFront:-7.900223,
                UpperBody:0.001294588,
                UpperLeg:-0.00224964,
                UpperLegFront:1.56886"
            },
            { "spear_2",
              @"Foot:-0.2711532,
                FootFront:0.3674452,
                Head:0.04528368,
                LowerArm:-1.056267,
                LowerArmFront:-64.89429,
                LowerBody:-0.07540518,
                LowerLeg:0.1310087,
                LowerLegFront:-0.1006831,
                UpperArm:-11.41162,
                UpperArmFront:117.8358,
                UpperBody:-0.01748134,
                UpperLeg:0.3105718,
                UpperLegFront:-0.4763209"  
            },
            { "spear_3",
              @"Foot:-30.59307,
                FootFront:-19.17831,
                Head:-0.01713011,
                LowerArm:-1.355939,
                LowerArmFront:-64.62589,
                LowerBody:-17.71288,
                LowerLeg.29605,
                LowerLegFront:67.17188,
                UpperArm:-96.83207,
                UpperArmFront:31.8473,
                UpperBody:-0.1174417,
                UpperLeg:0.1126486,
                UpperLegFront:-54.36752"  
            },
            { "groundpound_1",
              @"Foot:51.70714,
                FootFront:-31.99269,
                #Head:-0.07597391,
                LowerArm:-36.66118,
                LowerArmFront:-19.87204,
                LowerBody:-3.997653,
                LowerLeg:146.4712,
                LowerLegFront:150.5883,
                UpperArm:-308.9179,
                UpperArmFront:38.9634,
                UpperBody:0.06812089,
                UpperLeg:22.58445,
                UpperLegFront:25.51438"
            },
            { "groundpound_2",
              @"Foot:51.99865,
                FootFront:-30.64739,
                LowerArm:277.8946,
                LowerArmFront:-0.2756784,
                LowerBody:0.7950341,
                LowerLeg:137.9989,
                LowerLegFront:88.16898,
                UpperArm:-211.8853,
                UpperArmFront:-73.02646,
                UpperBody:-2.796829,
                UpperLeg:-104.0163,
                UpperLegFront:-21.80765"
            },
            { "groundpound_2x",
              @"Foot:-30.07862,
                FootFront:-8.974759,
                #Head:-0.2506458,
                LowerArm:-86.63663,
                LowerArmFront5.9857,
                LowerBody:-10.36328,
                LowerLeg:78.862,
                LowerLegFront:83.92636,
                UpperArm:-91.01971,
                UpperArmFront:-132.8579,
                UpperBody:0.63482,
                UpperLeg:-78.86197,
                UpperLegFront:0.1315119"
            },
            { "groundpound_3",
              @"Foot:6.143962,
                FootFront:51.7829,
                #Head:14.52312,
                LowerArm:-21.96695,
                LowerArmFront:-42.34736,
                #LowerBody:-13.23629,
                LowerLeg:79.28992,
                LowerLegFront:73.83433,
                UpperArm:-41.73515,
                UpperArmFront:-43.72218,
                #UpperBody:13.89472,
                UpperLeg:-110.0218,
                UpperLegFront:-3.436492"
            },
            { "chainsaw",
              @"LowerArm:-28.53935,
                LowerArmFront:-76.81657,
                UpperArm:-47.884,
                UpperArmFront:-3.689704"
            },
            { "bicycle_1",
              @"#Foot:-16.25298,
                #FootFront:7.71407,
                Head:-0.003797585,
                LowerArm:-0.04865827,
                LowerArmFront:-30.08054,
                LowerBody:-2.096376,
                UpperArm:-66.23631,
                UpperArmFront:-52.03539,
                UpperBody:0.001038189,
                UpperLeg:-97.02609:r4,
                UpperLegFront:-70.46504:r4,
                LowerLeg:93.72361:r4,
                LowerLegFront:67.38856:r4"
            },
            { "bicycle_2",
              @"#Foot:-16.25306,
                #FootFront:7.715409,
                Head:-8.253792,
                LowerArm:0.4919102,
                LowerArmFront:-30.06942,
                LowerBody:-1.764511,
                UpperArm:-66.15874,
                UpperArmFront:-51.97304,
                UpperBody:-0.2303413,
                UpperLegFront:-97.02609:r4,
                UpperLeg:-70.46504:r4,
                LowerLegFront:93.72361:r4,
                LowerLeg:67.38856:r4"
            },
        };
    }
}
