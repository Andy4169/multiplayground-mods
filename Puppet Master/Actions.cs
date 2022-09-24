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
    public class Actions
    {
        public Puppet Puppet                      = (Puppet)null;
        public Crouch crouch                      = new Crouch();
        public Dive dive                          = new Dive();
        public Backflip backflip                  = new Backflip();
        public Prone prone                        = new Prone();
        public Jump jump                          = new Jump();
        public Attack attack                      = new Attack();
        public ThrowItem throwItem                = new ThrowItem();

        public bool combatMode                    = false;
        public static bool hasTasks               = false;
        public static List<Task> Tasks            = new List<Task>();

        public struct Task {
            public float time;
            public Action task;
        }


        public Actions(Puppet puppet)
        {
            Puppet = crouch.Puppet = backflip.Puppet = dive.Puppet = prone.Puppet = jump.Puppet = attack.Puppet = throwItem.Puppet = puppet;

        }


        //
        // ─── UNITY FIXED UPDATE ────────────────────────────────────────────────────────────────
        //
        public void RunActions()
        {
            if (dive.state            != DiveState.ready)       dive.Go();
            else if (backflip.state   != BackflipState.ready)   backflip.Go();
            else if (jump.state       != JumpState.ready)       jump.Go();
            else if (throwItem.state  != ThrowState.ready)      throwItem.Go();
            else if (attack.state     != AttackState.ready)     attack.Go();
            else if (combatMode) { Puppet.Actions.attack.DoCombatPose(); }

            if (prone.state != ProneState.ready) prone.Go();
            if (crouch.state != CrouchState.ready) crouch.Go();

            if (hasTasks) RunTasks();

        }

        public void Start()
        {
            attack.Puppet = crouch.Puppet = jump.Puppet = dive.Puppet = prone.Puppet = throwItem.Puppet = this.Puppet;
        }


        //
        // ─── ACTION TASK SYSTEM ────────────────────────────────────────────────────────────────
        //
        public static void RunTasks()
        {
            for (int i = Tasks.Count; --i >= 0;)
            {
                if (Tasks[i].time > Time.time)
                {
                    Tasks[i].task();

                    Tasks.RemoveAt(i);
                }
            }
        }

         
        public static void AddTask(Action task, float seconds)
        {
            Task taskItem = new Task()
            {
                time = seconds + Time.time,
                task = task
            };

            Tasks.Add(taskItem);

            hasTasks = true;    
        }
    }
}