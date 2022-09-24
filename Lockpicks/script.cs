    using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;


namespace Caxap
{
        public class Caxap
    {

        public static void Main()
        {
              CategoryBuilder.Create("Lockpicks", "<color=blue>Andrew's Lockpicks", ModAPI.LoadSprite("lockpick.png"));


            ModAPI.Register(
                new Modification()
                {
                    OriginalItem = ModAPI.FindSpawnable("Stick"),
                    NameOverride = "Lockpick",
                    NameToOrderByOverride = "Lockpick",
                    DescriptionOverride = "A lockpick prop",
                    CategoryOverride = ModAPI.FindCategory("Lockpicks"),
                    ThumbnailOverride = ModAPI.LoadSprite("lockpick.png", 5f),
                    AfterSpawn = (Instance) =>
                    {
                        Instance.GetComponent<SpriteRenderer>().sprite = ModAPI.LoadSprite("lockpick.png", 0.7f);
                        Instance.FixColliders();
                    }
                }
            );
        }
    }
}