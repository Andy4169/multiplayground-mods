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
    public class Garage
    {
        [SkipSerialisation] public static Puppet Puppet;
        [SkipSerialisation] public static PuppetBike Bike;

        public static bool CanPuppetDrive(PhysicalBehaviour PB)
        {

            //  Check for Bike
            if ("Frame,Pedal,Dinges,Wheel1,Wheel2".Contains(PB?.name))
            {
                //  Bicycle was clicked
                if (Puppet != null && Puppet.IsActive)
                {

                    PuppetBike clickedBike = PB.transform.root.Find("Frame")?.gameObject.GetOrAddComponent<PuppetBike>();

                    if (Bike != null && Bike != clickedBike)
                    {
                        Bike.enabled = false;
                        Util.DestroyNow(Bike);
                    }

                    Bike = clickedBike;
                    Bike.Puppet = Puppet;

                    if (Bike.RegisterBike()) Util.Notify("You have a new <color=yellow>bike</color>");

                    return true;
                }
            }


            return false;
        }
    }

}
