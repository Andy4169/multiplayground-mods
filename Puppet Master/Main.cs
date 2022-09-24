//     ___                        _                    _     
//    / _ \_   _ _ __  _ __   ___| |_  /\/\   __ _ ___| |_ ___ _ __ 
//   / /_)/ | | | '_ \| '_ \ / _ \ __|/    \ / _` / __| __/ _ \ '__|
//  / ___/| |_| | |_) | |_) |  __/ |_/ /\/\ \ (_| \__ \ ||  __/ |  
//  \/     \__,_| .__/| .__/ \___|\__\/    \/\__,_|___/\__\___|_|  
//              |_|   |_|                   
//
//  Inspired from great mods & modders
//  
//  "AutoAim" by Puppyguard & ofc the Legendary "Active Humans!" by quaq
//
//  Nothing here can be considered original or mine. It has already been
//  thought of and done better by someone much more creative than meself.
//  Any credit goes to all of those who paved the way providing sample code.
//
//  For these coders below I've learned from the mods they have contributed
//  and am very grateful and lucky to be able to do that.
//
// -= Aspa102 Azule Blocksify jimmyl morzz1c Pump3d Pumpkin weirdo Woowz11 =-
//
//  (+ many more) and ofc the one & only...         -= zooi =- 
//
//  Thanks
//
//  If for any reason anybody wants to use any of this code, please do.
//
//  If you have any tips on how to do things more efficiently,
//  I would appreciate any tips I can get.
//
using UnityEngine;
namespace PuppetMaster
{
    public class JTMain
    {
        public static bool EnableChaseCam          = true;
        public static bool AllowMultiplePuppets    = false;
        public static bool DoBindingSwaps          = true;
        public static int MaxThingsOnGround        = 20;

        [SkipSerialisation] public static Sprite SpeedometerSprite;
        [SkipSerialisation] public static SpriteRenderer SpeedometerSR;

        public static Util.VerboseLevels Verbose   = Util.VerboseLevels.Minimal;

        public static void OnLoad()
        {
            KB.InitControls();
        }

        public static void Main()
        {
            SpeedometerSprite  = ModAPI.LoadSprite("shtuff/speedometer.png", 1f, false);

            ModAPI.Register<PuppetMaster>();
        }
    }
}