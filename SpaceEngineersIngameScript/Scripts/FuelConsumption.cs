using System;
using System.Collections.Generic;
using VRageMath; // VRage.Math.dll
using VRage.Game; // VRage.Game.dll
using System.Text;
using Sandbox.ModAPI.Interfaces; // Sandbox.Common.dll
using Sandbox.ModAPI.Ingame; // Sandbox.Common.dll
using Sandbox.Game.EntityComponents; // Sandbox.Game.dll
using VRage.Game.Components; // VRage.Game.dll
using VRage.Collections; // VRage.Library.dll
using VRage.Game.ObjectBuilders.Definitions; // VRage.Game.dll
using VRage.Game.ModAPI.Ingame; // VRage.Game.dll
using SpaceEngineers.Game.ModAPI.Ingame; // SpacenEngineers.Game.dll

namespace FuelConsumption
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        FuelConsumption
        ==============
        Copyright 2016 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE

        Description
        ===========

        */

        public Program()
        {
            ItemAmountsLast = coutItems();
            LastTime = DateTime.Now;
        }

        DateTime LastTime;
        DateTime CurrentTime;

        BMyDynamicDictionary<string, long> ItemAmountsLast;
        BMyDynamicDictionary<string, long> ItemAmountsCurrent;

        char[] progressIndicator = new char[] {'-', '\\', '|', '/'};
        int progressIndicatorCount = 0;

        public void Main(string argument)
        {
            load();
            run(argument);
            unload();
        }

        public void run(string argument)
        {
            IMyTextPanel Panel = GridTerminalSystem.GetBlockWithName(argument) as IMyTextPanel;
            if(Panel != null)
            {
                Echo("Panel found");
                Panel.WritePrivateText("");
                foreach(var Item in ItemAmountsCurrent)
                {
                    if (ItemAmountsLast.ContainsKey(Item.Key))
                    {
                        var Last = ItemAmountsLast[Item.Key];
                        Panel.WritePrivateText(string.Format("{0}: {1} / {2}\n", Item.Key, Item.Value, Last), true);
                    } else
                    {
                        Echo("no record for " + Item.Key);
                    }
                }
            } else
            {
                Echo("Panel not found");
            }
        }

        public void load()
        {
            ItemAmountsCurrent = coutItems();
            CurrentTime = DateTime.Now;
        }

        public void unload()
        {
            ItemAmountsLast.Clear();
            foreach(var Item in ItemAmountsCurrent)
            {
                ItemAmountsLast.Add(Item.Key,Item.Value);
            }
            LastTime = CurrentTime;
            progressIndicatorCount++;
            if(progressIndicatorCount < 0 || progressIndicator.Length <= progressIndicatorCount)
            {
                progressIndicatorCount = 0;
            }
        }

        BMyDynamicDictionary<string, long> coutItems()
        {
            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(Blocks, (b => b.HasInventory()));
            BMyDynamicDictionary<string, long> itemAmounts = new BMyDynamicDictionary<string, long>(0);
            foreach (IMyTerminalBlock Block in Blocks)
            {
                for (int i = 0; i < Block.GetInventoryCount(); i++)
                {
                    IMyInventory Inventory = Block.GetInventory(i);
                    foreach (IMyInventoryItem Item in Inventory.GetItems())
                    {
                        itemAmounts[Item.Content.TypeId.ToString() + "/" + Item.Content.SubtypeName] += Item.Amount.RawValue;
                    }
                }
            }

            return itemAmounts;
        }

        class BMyDynamicDictionary<TKey, TValue> : Dictionary<TKey, TValue>
        {
            private TValue _default;

            public BMyDynamicDictionary(TValue defaultValue) : base()
            {
                _default = defaultValue;
            }

            new public TValue this[TKey key] {
                get {
                    return ContainsKey(key) ? base[key] : _default;
                }
                set
                {
                    if (ContainsKey(key))
                    {
                        base[key] = value;
                    } else
                    {
                        Add(key, value);
                    }
                }
            }
        }
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}