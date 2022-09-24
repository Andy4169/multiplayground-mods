//     ___                        _                    _     
//    / _ \_   _ _ __  _ __   ___| |_  /\/\   __ _ ___| |_ ___ _ __ 
//   / /_)/ | | | '_ \| '_ \ / _ \ __|/    \ / _` / __| __/ _ \ '__|
//  / ___/| |_| | |_) | |_) |  __/ |_/ /\/\ \ (_| \__ \ ||  __/ |  
//  \/     \__,_| .__/| .__/ \___|\__\/    \/\__,_|___/\__\___|_|  
//              |_|   |_|         
//                                           
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace PuppetMaster
{
    [SkipSerialisation]
    public class PuppetMaster : MonoBehaviour
    {
        [SkipSerialisation] public static bool Activated                      = false;
        [SkipSerialisation] public static bool LastPauseState                 = false;
        [SkipSerialisation] public static bool IsGamePaused                   = false;
        [SkipSerialisation] public static bool CheckDisabledCollisions        = false;

        [SkipSerialisation] public static int ActivePuppet                    = 0;
        
        [SkipSerialisation] public static PhysicalBehaviour LastClickedItem   = null;
        [SkipSerialisation] public static PersonBehaviour LastClickedPerson   = null;
        [SkipSerialisation] public static Puppet Puppet                       = new Puppet();
        [SkipSerialisation] public static ChaseCamX ChaseCam                  = null;

        
        [SkipSerialisation] public static bool isMaxPayne                     = false;
        [SkipSerialisation] public static PuppetMaster Master;

        [SkipSerialisation] public static Dictionary<int, Puppet> PuppetList  = new Dictionary<int, Puppet>();
        
        private readonly List<Task> Tasks  = new List<Task>();
        private bool hasTasks     = false;

        public static bool CanBePuppet(PersonBehaviour person) => (bool) (person.isActiveAndEnabled && person.IsAlive());


        public void Start()
        {
            
            
            KB.SwapBindings(Activated);
            Master = this;


        }


        //
        // ─── UNITY UPDATE ────────────────────────────────────────────────────────────────
        //
        public void Update()
        {
            if (InputSystem.Down("PM-TogglePM")) TogglePuppetMode();

            if (!Activated) return;

            if (hasTasks) RunTasks();
            if (KB.NotPlaying) 
            {
                if (LastPauseState == false)
                {
                    KB.ReplaceAllKeys();
                    LastPauseState = true;
                    
                    KB.EnableMouse();
                    KB.EnableNumberKeys(); 
                }
            }
            else if (LastPauseState == true) { 

                LastPauseState = false;
                KB.SwapBindings(true);
                
            }

            KB.CheckKeys();

            if (KB.PClick) CheckNewClicks();

            if (CheckDisabledCollisions && Time.frameCount % 10 == 0) Util.FixDisabledCollisions();

            if ((bool)Puppet?.IsActive) {
                Puppet.CheckControls();

                if (Puppet.RunUpdate) Puppet.Update();
            }
            else if (Time.frameCount % 100 == 0)
            {
                if (Puppet?.PBO == null)
                {
                    if (Puppet?.HoldingF != null)
                    {
                        Puppet.HoldingF.P.beingHeldByGripper = false;
                    }
                }
            }

        }

        public void FixedUpdate()
        {
            if (!Activated) return;

            if ((bool)Puppet?.IsActive) Puppet.FixedUpdate();
        }


        private void RunTasks()
        {
            hasTasks = false;

            for (int i = Tasks.Count; --i >= 0;)
            {
                hasTasks = true;
                if (Tasks[i].Run()) Tasks.RemoveAt(i);
            }
        }


        //
        // ─── TOGGLE PUPPET MODE ────────────────────────────────────────────────────────────────
        //
        public static void TogglePuppetMode()
        {

            //(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt));
            if (KB.Alt)
            {
                // Set verbose mode
                int verbose = ((int)JTMain.Verbose + 1) % 3;

                JTMain.Verbose = (Util.VerboseLevels)verbose;

                Util.Notify("<color=yellow>VERBOSE LEVEL:</color> " + Enum.GetName(typeof(Util.VerboseLevels), verbose), Util.VerboseLevels.Off);

                return;

            }

            Activated = !Activated;

            KB.SwapBindings(Activated);

            if (Activated )
            {
                Util.cam = Global.main.CameraControlBehaviour;

                if (ChaseCam == null)
                {
                    //  Init the Chase cam
                    Global.main.gameObject.GetOrAddComponent<ChaseCamX>();
                } 
                else
                {
                    //  re-enable the chase cam
                    ChaseCam.enabled = true;
                }

                Util.LoadHoldingPositions();
            } 
            else
            {
                //  Disable the chase cam
                ChaseCam?.Stop();

            }

            Util.Notify("Puppet Master: <color=" + (Activated ? "yellow>On" : "red>Off") + "</color>", Util.VerboseLevels.Minimal);

        }


        //
        // ─── CHECK NEW CLICKS ────────────────────────────────────────────────────────────────
        //
        public static void CheckNewClicks()
        {
            PersonBehaviour LCP = Util.GetClickedPerson();
            if (LCP != null)
            {

                if (LCP == LastClickedPerson)
                {
                    //  This person was clicked twice, so make them the puppet
                    if (CanBePuppet(LCP)) ActivatePuppet(LCP);
                    
                    LCP = null;

                    return;

                }

                LastClickedPerson = LCP;
            }
            else
            {
                PhysicalBehaviour LCI = Util.GetClickedItem();

                if (LCI != null && (bool)Puppet?.IsActive && !Puppet.SpecialMode)
                {
                    if (LCI == LastClickedItem)
                    {
                        //  Item was clicked twice, so pick it up automatically
                        Puppet.HoldThing(LCI.gameObject.GetOrAddComponent<Thing>());

                        return;
                    }

                    //  Validate clicked item
                    if (CanHoldItem(LCI)) LastClickedItem = LCI;
                    
                    else if (Garage.CanPuppetDrive(LCI)) return;
                }

                
            }
            
        }


        public void AddTask(Action<bool> action, float time, bool option)
        {
            Task task = new Task();
            task.AddTask(action, time, option);
            Tasks.Add(task);
            hasTasks = true;
        }

        public void AddTask(Action action, float time)
        {
            Task task = new Task();
            task.AddTask(action, time);
            Tasks.Add(task);
            hasTasks = true;
        }

        public void SetBool(ref bool VarItem, bool option)
        {
            VarItem = option;
        }

        //
        // ─── ACTIVATE PUPPET ────────────────────────────────────────────────────────────────
        //
        public static void ActivatePuppet(PersonBehaviour person)
        {
            List<string>ForcedRemoves = new List<string>()
            {
                "ACTIVEHUMAN",
                "EVALTHREAT",
            };
            
            int puppetId = person.GetHashCode();
            
            if (PuppetList.TryGetValue(puppetId, out Puppet puppet))
            {
                Puppet       = puppet;

                if (ChaseCam == null)
                {
                    Global.main.gameObject.GetOrAddComponent<ChaseCamX>();
                }
                
                ChaseCam.SetPuppet(Puppet, true);
            }
            else 
            {
                Puppet = new Puppet();
                Puppet.Init(person);

                PuppetList.Add(puppetId, Puppet);
            }

            //  Disable ActiveHumans for this puppet
            MonoBehaviour[] components = Puppet.PBO.GetComponents<MonoBehaviour>();

            if (components.Length > 0)
            {
                for (int i = components.Length; --i >= 0;)
                {
                    if (ForcedRemoves.Contains(components[i].GetType().ToString().ToUpper()))
                    {
                        Util.Notify("Removed component: " + components[i].GetType().ToString());

                        UnityEngine.Object.DestroyImmediate((UnityEngine.Object)components[i]);
                    }
                }
            }

            Puppet.IsActive        = true;
            ActivePuppet           = puppetId;

        }


        //
        // ─── CAN HOLD ITEM ────────────────────────────────────────────────────────────────
        //
        public static bool CanHoldItem(PhysicalBehaviour item)
        {
            if (!item.Selectable || item.HoldingPositions.Length == 0) return false;

            if (item.TryGetComponent<FreezeBehaviour>(out _)) return false;

            return true;
        }

        public class Task
        {
            public float Xtime;
            public object TaskAction;
            public bool hasParam = false;
            public bool boolParam;

            public void AddTask(Action action, float time)
            {
                Xtime      = Time.time + time;
                TaskAction = action;
            }
            
            public void AddTask(Action<bool> action, float time, bool param) 
            {
                hasParam        = true;
                Xtime           = Time.time + time;
                TaskAction      = action;
                boolParam       = param;
            }

            public bool Run()
            {
                if (Time.time >= Xtime) {
                    if (hasParam) { 
                        Action<bool> action = (Action<bool>)TaskAction;
                        action(boolParam);
                    } else {
                        Action action = (Action)TaskAction;
                        action();
                    }
                    return true;
                }

                return false;
            }
        }
    }
}