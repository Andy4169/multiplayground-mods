//     ___                        _                    _     
//    / _ \_   _ _ __  _ __   ___| |_  /\/\   __ _ ___| |_ ___ _ __ 
//   / /_)/ | | | '_ \| '_ \ / _ \ __|/    \ / _` / __| __/ _ \ '__|
//  / ___/| |_| | |_) | |_) |  __/ |_/ /\/\ \ (_| \__ \ ||  __/ |  
//  \/     \__,_| .__/| .__/ \___|\__\/    \/\__,_|___/\__\___|_|  
//              |_|   |_|                
//                                           
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PuppetMaster
{
    public class Effects
    {
        public static List<Collider2D> DoNotKill = new List<Collider2D>();
        public static int DoNotKillLayer         = 0;

        public static TrailRenderer Trail;

        public static TextMeshProUGUI Text;

        public static Canvas Canvas;

        public static TextMeshProUGUI speedometer;
        public static GameObject speedometerObj;
        public static Vector3 ScreenSize => Canvas.pixelRect.size;


        public enum SpeedometerTypes
        {
            Off,
            Bike,
            Car,
            Hovercraft,
        }

        public static void Speedometer(SpeedometerTypes type, float speed)
        {
            if (speedometer == null)
            {
                if (Canvas == null) Canvas = Global.FindObjectOfType<Canvas>();

                speedometerObj = new GameObject("speedo");

                speedometerObj.transform.SetParent(Canvas.transform, false);

                Vector2 pos                       = new Vector2((ScreenSize.x / 2) + 100f,32f);
                speedometerObj.transform.position = pos;
                RectTransform rectText            = speedometerObj.AddComponent<RectTransform>();
                
                speedometer                       = speedometerObj.AddComponent<TextMeshProUGUI>();
                speedometer.fontSize              = 24;
                speedometer.alignment             = TextAlignmentOptions.TopLeft;
                speedometer.raycastTarget         = false;
                speedometer.text                  = "0";
                speedometer.color                 = new Color(0.8f,0.8f,0.8f,0.8f);
                speedometer.faceColor             = new Color(0.8f,0.8f,0.8f,0.8f);
                speedometer.outlineColor          = new Color(0.2f,0.2f,0.2f);
                speedometer.outlineWidth          = 0.1f;
                speedometer.alignment             = TextAlignmentOptions.Left;
                speedometer.fontStyle             = FontStyles.Bold;
                
                GameObject imageObj               = new GameObject("speedoImage");

                imageObj.transform.SetParent(speedometerObj.transform, false);

                imageObj.transform.position       = new Vector2(ScreenSize.x/2 - 40 , 32f);
                RectTransform rectImage           = imageObj.AddComponent<RectTransform>();
                rectImage.sizeDelta               = new Vector2(45,25);
                Image image                       = imageObj.AddComponent<Image>();
                image.sprite                      = JTMain.SpeedometerSprite;
                Color c                           = new Color(1,1,1,0.5f);
            }

            if (Time.frameCount % 100 == 0)
            {
                if (type == SpeedometerTypes.Off || JTMain.Verbose == Util.VerboseLevels.Off || KB.NotPlaying)
                {
                    speedometerObj.SetActive(false);

                } else
                {
                    if (!speedometerObj.activeSelf) speedometerObj.SetActive(true);
                }
            }

            speedometer.text = String.Format("{0}", Mathf.RoundToInt( speed ));
        }


        public static TrailRenderer DoTrail(Rigidbody2D rbody, bool kill=false)
        {
            if (kill)
            {
                if (rbody.gameObject.TryGetComponent<TrailRenderer>(out TrailRenderer trailRenderer))
                {
                    Util.Destroy(trailRenderer);
                }
                return (TrailRenderer)null;

            }

            Trail                      = rbody.gameObject.GetOrAddComponent<TrailRenderer>();
            //Trail.endColor             = Color.red;
            //Trail.startColor           = Color.white;
            Trail.startWidth           = 0.5f;
            Trail.endWidth             = 0.1f;
            Trail.generateLightingData = true;
            Trail.time                 = 0.5f;
            Trail.material             = Resources.Load<Material>("Materials/PhaseLink");

            return Trail;
        }
        public static void DoPulseExplosion(
            Vector3 position,
            float force,
            float range,
            bool soundAndEffects,
            bool breakObjects = true)
        {
            CameraShakeBehaviour.main.Shake((float)((double)force * (double)range * 2.0), (Vector2)position);
            
            //if (soundAndEffects)
            //    UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/RaygunExplosion"), position, Quaternion.identity);
            
            Vector2 point = (Vector2)position;
            double num1   = (double)range;
            int mask      = LayerMask.GetMask("Objects");

            foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(point, (float)num1, mask))
            {
                if (DoNotKill.Contains(collider2D)) continue;
                if (DoNotKillLayer == mask) continue;

                if ((bool)(UnityEngine.Object)collider2D.attachedRigidbody)
                {
                    Vector3 vector3 = collider2D.transform.position - position;
                    float num2      = (float)((-(double)vector3.magnitude + (double)range) * ((double)force / (double)range));
                    
                    if (breakObjects && (double)UnityEngine.Random.Range(0, 10) > (double)force)
                        collider2D.BroadcastMessage("Break", (object)(Vector2)(vector3 * num2 * -1f), SendMessageOptions.DontRequireReceiver);
                    
                    collider2D.attachedRigidbody.AddForce((Vector2)(num2 * vector3.normalized), ForceMode2D.Impulse);
                }
            }
        }
    }
}
