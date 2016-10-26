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
            FirstTime = DateTime.Now;
            LastTime = DateTime.Now;
            ItemTypes = new BMyDynamicDictionary<string, string>("undefined");
            ItemTypes["MyObjectBuilder_AmmoMagazine"] = "ammo";
            ItemTypes["MyObjectBuilder_Component"] = "component";
            ItemTypes["MyObjectBuilder_GasContainerObject"] = "hbottle";
            ItemTypes["MyObjectBuilder_OxygenContainerObject"] = "obottle";
            ItemTypes["MyObjectBuilder_Ingot"] = "ingot";
            ItemTypes["MyObjectBuilder_Ore"] = "ore";
            ItemTypes["MyObjectBuilder_PhysicalGunObject"] = "handtool";

            coutItems(out ItemAmountsFirst, out oxyLevelFirst, out HLevelFirst);
            coutItems(out ItemAmountsLast, out oxyLevelLast, out HLevelLast);            
        }
        BMyDynamicDictionary<string, string> ItemTypes;

        float oxyLevelFirst;
        float HLevelFirst;
        DateTime FirstTime;
        BMyDynamicDictionary<string, long> ItemAmountsFirst;

        float oxyLevelLast;
        float HLevelLast;
        DateTime LastTime;
        BMyDynamicDictionary<string, long> ItemAmountsLast;

        float oxyLevelCurrent;
        float HLevelCurrent;
        DateTime CurrentTime;
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

        }

        public void load()
        {
            CurrentTime = DateTime.Now;
            coutItems(out ItemAmountsCurrent, out oxyLevelCurrent, out HLevelCurrent);
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
        void coutItems(out BMyDynamicDictionary<string, long> itemAmounts, out float levelOxy, out float levelHydro)
        {
            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(Blocks, (b => b.HasInventory()));
            itemAmounts = new BMyDynamicDictionary<string, long>(0);


            float sumH = 0;
            float sumO = 0;
            float countH = 0;
            float countO = 0;

            foreach (IMyTerminalBlock Block in Blocks)
            {
                for (int i = 0; i < Block.GetInventoryCount(); i++)
                {
                    IMyInventory Inventory = Block.GetInventory(i);
                    foreach (IMyInventoryItem Item in Inventory.GetItems())
                    {
                        itemAmounts[ItemTypes[Item.Content.TypeId.ToString()] + "/" + Item.Content.SubtypeName] += Item.Amount.RawValue;
                    }
                }
                if(Block is IMyOxygenTank)
                {
                    if (Block.BlockDefinition.SubtypeName.Contains("Hydrogen"))
                    {
                        sumH += (Block as IMyOxygenTank).GetOxygenLevel();
                        countH++;
                    } else
                    {
                        sumO += (Block as IMyOxygenTank).GetOxygenLevel();
                        countO++;
                    }
                }
                
            }
            levelHydro = (countH > 0) ? (sumH / countH) : 0;
            levelOxy = (countO > 0) ? (sumO / countO) : 0;
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