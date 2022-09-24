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
    public class ChaseCamX : MonoBehaviour
    {
        [SkipSerialisation] public static GameObject ChaseObj;
        [SkipSerialisation] public static PhysicalBehaviour ChasePB;
        [SkipSerialisation] public static PhysicalBehaviour ChaseTarget;

        [SkipSerialisation] public static Puppet Puppet;

        [SkipSerialisation] public static float yOffset;
        [SkipSerialisation] public static float yOffsetTemp  = 0f;
        [SkipSerialisation] public static float LookAheadX   = 2.0f;
        [SkipSerialisation] public static float ItemTimeout  = 2.0f;
        [SkipSerialisation] public static float yThreshold   = 1.0f;
        [SkipSerialisation] public static float xThreshold   = 2.0f;

        [SkipSerialisation] public static bool isMoving      = false;
        [SkipSerialisation] public static bool lockedOn      = false;
        [SkipSerialisation] public static float shakePower   = 0f;
        private static bool setCamera          = false;
        private static bool checkedMouse       = false;
        private static bool targetClicked      = false;
        private static bool skipFrame          = false;
        private static bool yMoving            = false;

        private static bool miscBool   = false;
        private static float miscFloat = 0f;

        private static readonly float maxSpeed = 5.0f;

        private static int notMoving2          = 0;
        private static float notMoving         = 0f;
        private static float slerpSpeed        = 5;

        private static ChaseModes ChaseMode    = ChaseModes.Idle;

        private static Vector3 velocity        = Vector3.zero;
        private static Vector3 cTarget;
        private static Vector3 cItem;
        private static Vector3 cPuppet;
        private static Vector3 cItemStill;
        private static Vector3 shakeVector;

        private static Camera GCamera;
        private static bool CustomCameraInitialized = false;
        public static CustomModes CustomMode = CustomModes.Off;

        public enum ChaseModes
        {
            Idle,
            Puppet,
            Item,
            Action,
        }

        public enum CustomModes
        {
            Off,
            GroundPound,
            SonicBoom,
        }


        //
        // ─── UNITY AWAKE ────────────────────────────────────────────────────────────────
        //
        public void Awake()
        {
            CreateChaseObject();
            GCamera = Global.main.camera;
        }


        //
        // ─── UNITY LATE UPDATE ────────────────────────────────────────────────────────────────
        //
        public void LateUpdate()
        {
            if (ChaseMode == ChaseModes.Idle || (ChaseTarget == null && ChaseMode != ChaseModes.Item) ) return;

            if (skipFrame)
            {
                skipFrame = false;
                return;
            }

            if (setCamera)
            {
                setCamera = false;
                if (!Global.main.CameraControlBehaviour.CurrentlyFollowing.Contains(ChasePB))
                {
                    Global.main.CameraControlBehaviour.CurrentlyFollowing.Clear();
                    Global.main.CameraControlBehaviour.CurrentlyFollowing.Add(ChasePB);
                }

                return;
            }

            if (CustomMode != CustomModes.Off)
            {
                MoveCustomCamera();
                return;
            }

            if (CustomCameraInitialized)
            {
                //  Disable custom cam, go back to default
                GCamera                                    = (Camera)null;
                Global.main.CameraControlBehaviour.enabled = true;
                CustomCameraInitialized                    = false;
                SetPuppet(Puppet,false);
                Time.timeScale = 1f;
            }

            //  Prevent chasecam following puppet when its being dragged around the screen
            if (KB.MouseDown)
            {
                if (!checkedMouse)
                {
                    checkedMouse = true;

                    Collider2D[] NoCollide = Puppet.PBO.transform.root.GetComponentsInChildren<Collider2D>();

                    foreach (Collider2D collider in NoCollide)
                    {
                        if (!collider || collider == null) continue;

                        if ((bool)collider?.OverlapPoint((Vector2)Global.main.MousePosition))
                        {
                            targetClicked = true;
                            break;
                        }
                    }
                }

                if (targetClicked) return;
            }
            else checkedMouse = targetClicked = false;

            //  Chase funcs
            if      (ChaseMode == ChaseModes.Puppet) TrackPuppet();
            else if (ChaseMode == ChaseModes.Item)   TrackItem();

            if (shakePower > 0.0f)
            {
                shakeVector = Vector3.zero;

                shakeVector.x += Random.Range(-shakePower, shakePower);
                shakeVector.y += Random.Range(-shakePower, shakePower);

                ChaseObj.transform.Translate(shakeVector);

                //Global.CameraPosition = shakeVector;
            }
            
        }

        //
        // ─── TRACK PUPPET ────────────────────────────────────────────────────────────────
        //
        private void TrackPuppet()
        {
            if (Puppet == null)
            {
                ChaseMode = ChaseModes.Idle;
                return;
            }

            cTarget     = ChaseObj.transform.position;
            cPuppet     = ChaseTarget.transform.position;

            //  have camera look ahead the direction puppet is facing
            cPuppet.x += (Puppet.IsInVehicle ? LookAheadX * 1.5f : LookAheadX) * -Puppet.Facing;

            //  have camera Keep the chosen Y offset from the players position
            //if (!lockedOn) cPuppet.y += yOffset;
            cPuppet.y += (yOffset + yOffsetTemp);

            //cPuppet.z = -10f;

            float diffX   = Mathf.Abs(cTarget.x - cPuppet.x);
            float diffY   = Mathf.Abs(cTarget.y - cPuppet.y);
            
            //Util.Notify("diffX:" + diffX + " - diffY:" + diffY + " - notMoving:" + notMoving + " - lockedOn:" + lockedOn);

            if (isMoving) 
            {  
                if (diffX < 0.2f && diffY < (yMoving ? 0.7f : 1.5f)) {
                
                    if (++notMoving > 15) {
                    
                        notMoving  = 0;
                        lockedOn   = false;
                        isMoving   = false;
                        yMoving    = false;

                    }

                } else notMoving = 0;

            }

            if (isMoving || diffX > xThreshold || diffY > yThreshold)
            {
                float vDistance = Vector3.SqrMagnitude(cTarget - cPuppet);
                float vMaxSpeed = Mathf.Max(maxSpeed, vDistance);


                velocity *= vDistance * Time.deltaTime;

                if (lockedOn) {
                    if (slerpSpeed++ > 25f) slerpSpeed = 25f;
                    ChaseObj.transform.position = Vector3.Slerp(Util.ClampPos(cTarget), cPuppet, slerpSpeed * Time.deltaTime);
                    float mago = ChaseTarget.rigidbody.velocity.magnitude;
                    //Util.Notify("Magnitude: " + mago);
                    if ( mago > 0.01f ) notMoving2 = 0;
                    if (++notMoving2 > 15)
                    {
                        notMoving2 = 0;
                        lockedOn  = false;
                        isMoving  = false;
                        yMoving   = false;
                        return;
                    }
                }
                else 
                { 
                    if (vMaxSpeed > 20f && diffY > yThreshold * 3) {
                        slerpSpeed = 5f;
                        lockedOn   = true;
                        notMoving2 = 0;
                    }

                    if (diffY < yThreshold) cPuppet.y = cTarget.y;
                    else yMoving = true;

                    isMoving = true;

                    //if (Puppet.IsInVehicle) vMaxSpeed *= 20f;


                    ChaseObj.transform.position = Vector3.SmoothDamp(
                        Util.ClampPos(cTarget),
                        cPuppet,
                        ref velocity, 
                        Time.deltaTime,
                        vMaxSpeed
                    );
                }

            }
        }

        //
        // ─── TRACK ITEM ────────────────────────────────────────────────────────────────
        //
        private void TrackItem()
        {
            if (ChaseTarget == null)
            {
                if (notMoving == 0) notMoving = Time.time;
                if (Time.time - notMoving > 2f) StopQuickChase();
                return;
            }

            if (KB.AnyKey)
            {
                StopQuickChase();
                return;
            }

            cTarget = ChaseObj.transform.position;
            cItem   = Util.ClampPos(ChaseTarget.transform.position);

            float vDistance = Vector3.SqrMagnitude(cItemStill - cItem);

            if (vDistance > 0.01f) {
                notMoving  = Time.time;
                cItemStill = cTarget;
            }
            else if (Time.time - notMoving > 2f)
            {
                StopQuickChase();
                return;
            }

            ChaseObj.transform.position = cItem;

        }


        //
        // ─── QUICK CHASE ────────────────────────────────────────────────────────────────
        //
        public void QuickChase(PhysicalBehaviour target)
        {
            ChaseTarget = target;
            ChaseMode   = ChaseModes.Item;
            cItemStill  = Vector3.zero;
        }


        public void StopQuickChase()
        {
            if (KB.Modifier) return;
            if (PuppetMaster.isMaxPayne) Util.MaxPayne(false);

            notMoving = 0;
            lockedOn  = false;

            if (Puppet != null)
            {
                SetPuppet(Puppet);
                return;
            }

            ChaseMode = ChaseModes.Idle;
        }

        //
        // ─── MOVE CUSTOM CAMERA ────────────────────────────────────────────────────────────────
        //
        private void MoveCustomCamera()
        {
            if (!CustomCameraInitialized)
            {
                if (!Global.main.CameraControlBehaviour.CurrentlyFollowing.Contains(ChasePB))
                {
                    CustomMode = CustomModes.Off;
                    ChaseMode  = ChaseModes.Idle;
                    return;
                }
                
                GCamera                                    = Global.main.camera;
                Global.main.CameraControlBehaviour.enabled = false;
                CustomCameraInitialized                    = true;
                miscBool                                   = false;

                miscFloat = Time.time + 10f;

            }
            cTarget = ChaseObj.transform.position;
            cPuppet = ChaseTarget.transform.position;

            ChaseObj.transform.position = Vector3.Slerp(cTarget, cPuppet, slerpSpeed * Time.deltaTime);
            Global.CameraPosition       = Util.ClampPos(cTarget);

            float zoomOut            = Mathf.Clamp(Puppet.Actions.jump.sonicForce * 3, GCamera.orthographicSize, 40);
            GCamera.orthographicSize = Mathf.Lerp(GCamera.orthographicSize, zoomOut, Time.deltaTime * 2);
            
            if (CustomMode == CustomModes.SonicBoom) {
                Time.timeScale = 0.3f;
                if (!miscBool)
                {
                    miscBool  = true;
                    miscFloat = Time.time + 2f;
                }

                if (KB.AnyKey && !KB.Down) CustomMode = CustomModes.Off;
            }

            if (Time.time > miscFloat)
            {
                CustomMode = CustomModes.Off;
            }

        }

        //
        // ─── SET PUPPET ────────────────────────────────────────────────────────────────
        //
        public void SetPuppet(Puppet puppet, bool resetY=false)
        {
            if (puppet == null) return;

            CustomMode = CustomModes.Off;
            Puppet     = puppet;

            if (ChaseObj == null) CreateChaseObject();

            ChaseTarget = Puppet.LB?["Head"].PhysicalBehaviour;

            //  Set the constant Yoffset
            if (resetY) {
                
                yOffset = Global.main.CameraControlBehaviour.transform.position.y - ChaseTarget.transform.position.y;

            }

            //  Start invisible tracker at puppet position
            Vector3 cPuppet = ChaseTarget.transform.position;

            //  have camera look ahead the direction puppet is facing
            cPuppet.x += LookAheadX * -Puppet.Facing;

            //  have camera Keep the chosen Y offset from the players position
            cPuppet.y += yOffset;
            
            ChaseObj.transform.position = cPuppet;

            ChaseMode = ChaseModes.Puppet;
            setCamera = true;
            lockedOn  = false;
            skipFrame = true;
        }


        //
        // ─── CREATE CHASE OBJECT ────────────────────────────────────────────────────────────────
        //
        public void CreateChaseObject()
        {
            //  Create a hidden chase-cam object that the camera follows
            Texture2D tex   = new Texture2D(0, 0);
            Sprite mySprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.1f, 0.1f), 0.1f);
            ChaseObj        = ModAPI.CreatePhysicalObject("ChaseCam", mySprite);

            // Initialize chase-cam object
            ChaseObj.transform.position = Global.CameraPosition;

            ChasePB = ChaseObj.GetOrAddComponent<PhysicalBehaviour>();

            ChasePB.MakeWeightless();
            ChasePB.Selectable = false;
            ChasePB.Deletable  = false;

            //  Prevent all collisions with invisible chase object
            ChaseObj.SetLayer(10);

            //  Prevent this from feking up anyones contraption
            ChaseObj.GetOrAddComponent<Optout>();

            PuppetMaster.ChaseCam = this;

            DontDestroyOnLoad(ChaseObj);
        }

        //
        // ─── STOP (self-destruct) ────────────────────────────────────────────────────────────────
        //
        public void Stop()
        {
            if (Global.main.CameraControlBehaviour.CurrentlyFollowing.Contains(ChasePB))
            {
                Global.main.CameraControlBehaviour.CurrentlyFollowing.Remove(ChasePB);
            }

            if (ChasePB  != null) UnityEngine.Object.Destroy(ChasePB);    
            if (ChaseObj != null) UnityEngine.Object.Destroy(ChaseObj);    

            this.enabled = false;
            UnityEngine.Object.Destroy(this);
        }
    }
}