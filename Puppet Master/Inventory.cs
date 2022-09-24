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
    [SkipSerialisation]
    public class Inventory
    {
        [SkipSerialisation] 
        public Puppet Puppet;
        [SkipSerialisation]
        public Thing Clone;

        public bool InventoryTriggered           = false;
        public bool InventoryDone                = false;
        public bool TriggerPickup                = false;

        public int CurrentSpot                   = -1;
        private int LastItemPos                  = -1;

        [SkipSerialisation] 
        private readonly List<Thing> MyInventory = new List<Thing>();

        public Inventory(Puppet puppet)
        {
            Puppet = puppet;
        }



        //
        // ─── INIT INVENTORY ────────────────────────────────────────────────────────────────
        //
        public void InitInventory()
        {
            KB.DisableNumberKeys(); 

            InventoryTriggered  = true;
            InventoryDone       = false;
            CurrentSpot         = -1;
        }

        public void DoInventory()
        {
            if (!InventoryTriggered) InitInventory();
            else
            {
                int spot = KB.CheckNumberKey();

                if (spot > -1 && CurrentSpot != spot) CurrentSpot = spot;

                if (KB.MouseDown)
                {
                    PhysicalBehaviour clickedPB = Util.GetClickedItem();

                    if (clickedPB != null && PuppetMaster.CanHoldItem(clickedPB))
                    {
                        Thing thing = clickedPB.gameObject.GetOrAddComponent<Thing>();
                        if (thing != null) StoreItem(thing);
                    }
                    else
                    {
                        PersonBehaviour clickedPerson = Util.GetClickedPerson();
                        if (clickedPerson != null && clickedPerson != Puppet.PBO)
                        {
                            int puppetId = clickedPerson.GetHashCode();

                            if (PuppetMaster.PuppetList.TryGetValue(puppetId, out Puppet oldPuppet))
                            {
                                int beforeCount = MyInventory.Count;

                                foreach (Thing othing in oldPuppet.Inventory.MyInventory)
                                {
                                    if (!MyInventory.Contains(othing)) MyInventory.Add(othing);
                                }

                                int afterCount = MyInventory.Count;

                                if (afterCount == beforeCount) return;

                                this.LastItemPos = this.CurrentSpot = 1;

                                Util.Notify("Transferred <color=yellow>[" + MyInventory.Count + "] Inventory</color> from past puppet", Util.VerboseLevels.Minimal);

                                return;
                            }
                        }
                    }


                }
            }
        }


        //
        // ─── RUN INVENTORY ────────────────────────────────────────────────────────────────
        //
        public void RunInventory()
        {
            InventoryTriggered  = false;

            KB.EnableNumberKeys();

            if (InventoryDone) return;

            InventoryDone       = true;



            if ((Puppet.HoldingF == null || Puppet.HoldingF.Name == null) && CurrentSpot == -1)
            {
                if (LastItemPos > -1)
                {
                    SelectItem(LastItemPos);
                    return;
                }

                 Util.Notify("<color=orange>Hold </color>[ I ]<color=orange> and press </color>#[ 1-9 ]<color=orange> to store/retrieve Items</color>", Util.VerboseLevels.Minimal);
                 return;
            }

            if (Puppet.HoldingF != null) StoreItem(Puppet.HoldingF, -1);
            
            if (CurrentSpot > -1) SelectItem(CurrentSpot);
            else if (Puppet.HoldingF == null && !TriggerPickup)
            {
                Puppet.Actions.combatMode = false;
                if (Puppet.IsAiming) {
                    Puppet.AimingStop();
                }
            }
            
        }

        public void PutAwayItems()
        {
            if (Puppet.HoldingF != null) StoreItem(Puppet.HoldingF, -1);
        }


        public void RefreshPuppet()
        {
            for (int I = MyInventory.Count; --I > -1;)
            {
                MyInventory[I].Puppet                = Puppet;
                MyInventory[I].PuppetArm             = Puppet.RB2["LowerArmFront"];
                MyInventory[I].PuppetGrip            = Puppet.GripF;
                //MyInventory[I].AltGrip               = Puppet.GripB;
                MyInventory[I].HoldingPosition       = new Vector3(float.MaxValue, 0.0f);
            }
            Puppet = PuppetMaster.Puppet;

            Puppet.RefreshInventory(this);
        }


        //
        // ─── STORE ITEM ────────────────────────────────────────────────────────────────
        //
        public bool StoreItem(Thing thing, int spot = -1)
        {
            //  Check if we already have blueprint of this item
            int curpos = LocateItem(thing);

            if (curpos > -1)
            {
                //  We already have the item blueprint, so destroy this one
                thing.Destroy();

                curpos += KB.Modifier ? -1 : 1;

                if (curpos >= MyInventory.Count)
                {
                    LastItemPos = 0;
                    return true;
                } else if(curpos <= -1)
                {
                    LastItemPos = MyInventory.Count - 1;
                    return true;
                }

                LastItemPos = curpos;

                SelectItem(LastItemPos);

                return true;
            }


            if(spot == -1) spot = MyInventory.Count;

            thing.MakePersistant();

            CleanItem(thing);
            
            MyInventory.Add(thing);

            LastItemPos   = spot;
            InventoryDone = true;


            Util.Notify(thing.Name + " <color=orange>stored in position #:</color> " + (spot+1), Util.VerboseLevels.Minimal);

            return true;

        }

        


        //
        // ─── LOCATE ITEM ────────────────────────────────────────────────────────────────
        //
        public int LocateItem(Thing thing)
        {
            for (int i = MyInventory.Count; --i >= 0;)
            {
                if (MyInventory[i].Hash == thing.Hash || MyInventory[i].Name == thing.Name)
                {
                    if (MyInventory[i].P != null) return i;
                    MyInventory.RemoveAt(i);   
                }
            }

            return -1;
        }


        //
        // ─── CLEAN ITEM ────────────────────────────────────────────────────────────────
        //
        public void CleanItem(Thing thing)
        {
            if (thing.PuppetGrip)
            {
                Puppet.DropThing();
            } 
            
            thing.BreakConnections();

            thing.G.layer              = 10;
            thing.G.transform.position = (new Vector2(-100, -100));
            thing.R.position           = (new Vector2(1, 1));
            thing.R.bodyType           = RigidbodyType2D.Static;
            

        }


        //
        // ─── SELECT ITEM ────────────────────────────────────────────────────────────────
        //
        public bool SelectItem(int spotChk)
        {
            Thing thing = (Thing)null;

            if (spotChk < MyInventory.Count && spotChk >= 0)
            {
                if (MyInventory[spotChk] != null) thing = MyInventory[spotChk];

                if (thing.P == null)
                {
                    MyInventory.RemoveAt(spotChk);
                    thing = (Thing)null;
                }
            }
            
            if (thing == null) return false;

            GameObject instance = UnityEngine.Object.Instantiate(thing.G, thing.G.transform.position, Quaternion.identity);
            
            instance.name       = thing.Name;
            
            Clone               = instance.GetOrAddComponent<Thing>();
            Clone.isPersistant  = false;
            Clone.R.bodyType    = RigidbodyType2D.Dynamic;
            Clone.G.layer       = 9;
            Clone.Name          = thing.Name;

            if (Puppet.IsAiming && !thing.canAim) Puppet.AimingStop();
            if (Puppet.Actions.combatMode && !thing.canStrike) Puppet.Actions.attack.DoCombatPose(false);

            PuppetMaster.LastClickedItem = Clone.P;

            TriggerPickup       = true;
            InventoryDone       = true;
            LastItemPos         = spotChk;

            return true;

        }
       
    }

}