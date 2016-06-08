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

namespace BaconSort
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        BaconSort
        ==============
        Copyright 2016 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE

        Description
        ===========
        Inventory Sorting
            - Sort Items by tags in the Blockname
                    Tags: #ammo, #component, #bottle, #ingot, #ore, #handtool
            - Ignores all Reactors by default
            - Ignores all containers with "#!BaconSort" in their Name
 
            How it works: 
            * Add any tag to a Container with an Inventory to pull items of this type in it. 
            * Script is lazy, it will sort a item only if it is not in a Container with a matching tag. (Example: It will not pull an SteelPlate from an container called "Large Cargo Container #component") 
            * Source & Target Block must match some requirements: 1. Same Grid as PB, 2. Must be connected throug Conveyor-System, 4. Must be Enabled aka turned ON 
             
             
            Known Bugs: 
            * not working with Hydrogen Tanks. (will fix this as soon as i know why.) 
                                 
           Example 
           ------------------------------ 
            Blocks: "Cargo 1 #ingot #ammo", "Cargo 2 #component", Cargo 3" 
            This will try to sort all Ingots and ammo in Cargo 1 and all components in Cargo 2. 


        */

        // BEGIN - settings
        const bool IGNORE_REACTOR = true; // when set to true, this script will ignore all reactors to prevent power loss.
        const string IGNORE_TAG = "#!BaconSort"; // every Block with this in its name will be ignores
        const double preventExecutionForSeconds = 0; // scipt will not run twice in this timespan. (This is mostly a workaround to the weird behavior of timers on Servers.)
        // END - settings

        // BEGIN - LOG settings
        const bool LOG = true; // enabel disable Logging at all
        const string LOG_SCREEN = null; // the log will be displayed on any LCD of the PBs Grid with this in name. set to `null` to disable it.
        const int LOG_LEVEL = 1; // can be any of LOG_LEVEL_*, where higher number means more verbose Log.
        const string LOG_MODE = ""; // can be used to log funcion calls asn return values, this WILL cause massive lag and is for development purpose only.
        // BEGIN - LOG settings

        const int LOG_LEVEL_ECHO = 1;
        const int LOG_LEVEL_INFO = 2;
        const int LOG_LEVEL_ERROR = 3;
        const int LOG_LEVEL_DEBUG = 4;

        const string LOG_MODE_FUNCTIONCALL = "[FUNCTIONCALL]";
        const string LOG_MODE_RETURN = "[RETURN]";

        private int logIndex = 0;

        static public Dictionary<string, string> s_TypeTagMap = null;
        static public List<string> s_Tags = null;
        private List<IMyTextPanel> LogPanels = null;
        static public double timeToWait = 0;


        public Program()
        {
            timeToWait = preventExecutionForSeconds;
        }

        public void Save()
        {
        }

        public void Main(string argument)
        {
            bootStrapLog();
            Log("Script START", LOG_LEVEL_ECHO);
            if (doExecute())
            {
                Log_FunctionCall("Program", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("IGNORE_TAG", IGNORE_TAG) });
                Log_FunctionCall("Main", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("argument", argument) });
                List<IMyTerminalBlock> SourceBlocks = getSourceBlocks(IGNORE_TAG);
                Dictionary<string, string> TypeMap = getTypeTagMap();
                List<string> Tags = getTags(TypeMap);
                Dictionary<string, List<IMyTerminalBlock>> DestinationBlocksMap = getDestinationBlocksMap(Tags, IGNORE_TAG);
                doSortAll(SourceBlocks, DestinationBlocksMap, TypeMap);
                Log_Return("Main", "void");
            }
            Log("Script END (" + Runtime.LastRunTimeMs.ToString() + "ms)", LOG_LEVEL_ECHO);
        }

        private bool doExecute()
        {
            Log_FunctionCall("doExecute", new KeyValuePair<string, string>[] { });
            bool result = false;

            timeToWait = timeToWait - Runtime.TimeSinceLastRun.TotalSeconds;
            Log("Time to Wait: " + timeToWait.ToString(), LOG_LEVEL_ECHO);
            if (timeToWait > 0)
            {
                result = false;
            } else
            {                
                result = true;
                timeToWait = preventExecutionForSeconds;
            }            

            Log_Return("doExecute", result.ToString());
            return result;
        }

        private void doSortAll(List<IMyTerminalBlock> SourceBlocks, Dictionary<string, List<IMyTerminalBlock>> DestinationBlocksMap, Dictionary<string, string> TypeMap)
        {
            Log_FunctionCall("doSortAll", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("SourceBlocks", SourceBlocks.ToString()), new KeyValuePair<string, string>("DestinationBlocksMap", DestinationBlocksMap.ToString()), new KeyValuePair<string, string>("TypeMap", TypeMap.ToString()) });
            for(int i_SourceBLocks = 0; i_SourceBLocks < SourceBlocks.Count; i_SourceBLocks++)
            {
                IMyTerminalBlock SourceBlock = SourceBlocks[i_SourceBLocks];
                int sourceInventoryCount = SourceBlock.GetInventoryCount();
                Log(SourceBlock.CustomName + " with " + sourceInventoryCount.ToString() + " Inventories.", LOG_LEVEL_INFO);
                if (sourceInventoryCount > 0)
                {
                    for (int i_sourceInventory = 0; i_sourceInventory < sourceInventoryCount; i_sourceInventory++)
                    {
                        Log("Sorting Inventory " + (i_sourceInventory+1).ToString() + " of " + sourceInventoryCount.ToString() + " from Block " + SourceBlock.CustomName, LOG_LEVEL_INFO);
                        IMyInventory SourceInventory = SourceBlock.GetInventory(i_sourceInventory);
                        doSortInventory(SourceBlock, SourceInventory, DestinationBlocksMap, TypeMap);
                    }
                }
            }
            Log_Return("doSortAll", "void");
        }

        private void doSortInventory(IMyTerminalBlock SourceBlock, IMyInventory SourceInventory, Dictionary<string, List<IMyTerminalBlock>> DestinationBlocksMap, Dictionary<string, string> TypeMap)
        {
            Log_FunctionCall("doSortInventory", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("SourceBlock", SourceBlock.CustomName), new KeyValuePair<string, string>("SourceInventory", SourceInventory.ToString()), new KeyValuePair<string, string>("DestinationBlocksMap", DestinationBlocksMap.ToString()), new KeyValuePair<string, string>("TypeMap", TypeMap.ToString()) });
            List<IMyInventoryItem> SourceItems = SourceInventory.GetItems();
            for (int i_SourceItems = SourceItems.Count - 1; i_SourceItems >= 0; i_SourceItems--)
            {
                IMyInventoryItem SourceItem = SourceItems[i_SourceItems];
                Log("sorting " + SourceItem.Content.SubtypeName + " from " + SourceBlock.CustomName, LOG_LEVEL_INFO);
                doSortItem(i_SourceItems, SourceItem, SourceBlock, SourceInventory, DestinationBlocksMap, TypeMap);
            }
            Log_Return("doSortInventory", "void");
        }

        private void doSortItem(int i_SourceItems, IMyInventoryItem SourceItem, IMyTerminalBlock SourceBlock, IMyInventory SourceInventory, Dictionary<string, List<IMyTerminalBlock>> DestinationBlocksMap, Dictionary<string, string> TypeMap)
        {
            Log_FunctionCall("doSortItem", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("i_SourceItem", i_SourceItems.ToString()), new KeyValuePair<string, string>("SourceItem", SourceItem.Content.SubtypeName), new KeyValuePair<string, string>("SourceInventory", SourceInventory.ToString()), new KeyValuePair<string, string>("DestinationBlocksMap", DestinationBlocksMap.ToString()), new KeyValuePair<string, string>("TypeMap", TypeMap.ToString()) });
            bool itemIsPending = true;
            string tag = getTag(SourceItem, TypeMap);
            if (!SourceBlock.CustomName.Contains(tag) && DestinationBlocksMap.ContainsKey(tag))
            {
                List<IMyTerminalBlock> DestinationBlocks = DestinationBlocksMap[tag];
                Log("Tag '" + tag + "' has " + DestinationBlocks.Count.ToString() + " possible Destinations", LOG_LEVEL_INFO);

                for (int i_DestinationBlocks = 0; itemIsPending && i_DestinationBlocks < DestinationBlocks.Count; i_DestinationBlocks++)
                {
                    IMyTerminalBlock DestinationBlock = DestinationBlocks[i_DestinationBlocks];
                    int destinationInventoryCount = DestinationBlock.GetInventoryCount();
                    Log("Try Destination " + DestinationBlock.CustomName + " with " +destinationInventoryCount.ToString() + " Inventories.", LOG_LEVEL_INFO);
                    if (destinationInventoryCount > 0)
                    {
                        for (int i_DestinationInventory = 0; itemIsPending && i_DestinationInventory < destinationInventoryCount; i_DestinationInventory++)
                        {
                            VRage.MyFixedPoint lastAmmount = SourceItem.Amount;
                            IMyInventory DestinationInventory = DestinationBlock.GetInventory(i_DestinationInventory);
                            Log(DestinationBlock.CustomName + " Inventory #" + (i_DestinationInventory + 1).ToString(), LOG_LEVEL_INFO); 
                            if (SourceInventory.IsConnectedTo(DestinationInventory))
                            {
                                Log("is connected", LOG_LEVEL_INFO);
                                if (SourceInventory.TransferItemTo(DestinationInventory, i_SourceItems, null, true))
                                {
                                    VRage.MyFixedPoint movedAmmount = lastAmmount - SourceItem.Amount;
                                    itemIsPending = (SourceItem.Amount > 0);
                                    EchoSort(SourceBlock, DestinationBlock, SourceItem, movedAmmount);
                                }    
                            } else
                            {
                                Log("is not connected", LOG_LEVEL_INFO);
                            }
                        }
                    }
                }
            } else
            {
                Log("Tag '" + tag + "' has no Destination.", LOG_LEVEL_INFO);
            }
            Log_Return("doSortItem", "void");
        }

        private List<IMyTerminalBlock> getSourceBlocks(string ignoreTag)
        {
            Log_FunctionCall("getSourceBlocks", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("ignoreTag", ignoreTag) });
            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(Blocks, (x => !x.CustomName.Contains(ignoreTag) && x.CubeGrid.Equals(Me.CubeGrid) && (!(x is IMyReactor) || !IGNORE_REACTOR)));

            Log_Return("getSourceBlocks", Blocks.Count.ToString());
            return Blocks;
        }

        private Dictionary<string, List<IMyTerminalBlock>> getDestinationBlocksMap(List<string> Tags, string ignoreTag)
        {
            Log_FunctionCall("getDestinationBlocksMap", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("Tags", Tags.ToString()), new KeyValuePair<string, string>("ignoreTag", ignoreTag) });
            Dictionary<string, List<IMyTerminalBlock>> DestinationBlockList = new Dictionary<string, List<IMyTerminalBlock>>();
            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(Blocks, (x => (!(x is IMyReactor) || !IGNORE_REACTOR) && isDestinationBlock(x, Tags, ignoreTag)));
            Log("Found " + Blocks.Count.ToString() + " possible Destinations.", LOG_LEVEL_INFO);
            for(int iBlocks = 0; iBlocks < Blocks.Count; iBlocks++)
            {
                IMyTerminalBlock Block = Blocks[iBlocks];
                for(int iTags = 0; iTags < Tags.Count; iTags++)
                {
                    string tag = Tags[iTags];
                    Log("For Tag '" + tag + "' in '" + Block.CustomName + "'", LOG_LEVEL_INFO);
                    if (Block.CustomName.Contains(tag))
                    {
                        Log("Found TAG '" + tag + "' in " + Block.CustomName);
                        if (!DestinationBlockList.ContainsKey(tag))
                        {
                            DestinationBlockList.Add(tag, new List<IMyTerminalBlock>());
                            Log("new List for Tag '" + tag + "'");
                        }
                        if (!DestinationBlockList[tag].Contains(Block))
                        {
                            Log("Add '" + Block.CustomName + "' to List");
                            DestinationBlockList[tag].Add(Block);
                        }                         
                    } else
                    {
                        Log("NO TAG '" + tag + "' in " + Block.CustomName);
                    }
                }
            }

            Log_Return("getDestinationBlocksMap", DestinationBlockList.Count.ToString());
            return DestinationBlockList;
        }
        

        private bool isDestinationBlock(IMyTerminalBlock Block, List<string> Tags, string ignoreTag)
        {
            Log_FunctionCall("isDestinationBlock", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("Block", Block.CustomName), new KeyValuePair<string, string>("ignoreTag", ignoreTag) });
            bool result = false;
            if (!Block.CustomName.Contains(ignoreTag))
            {
                bool hasTag = false;
                for (int i = 0; !hasTag && i < Tags.Count; i++)
                {
                    hasTag = hasTag || Block.CustomName.Contains(Tags[i]);
                }

                result = hasTag;
            }

            Log_Return("isDestinationBlock", result.ToString());
            return result;
        }
        
        private List<string> getTags(Dictionary<string, string> TypeMap)
        {
            Log_FunctionCall("getTags", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("TypeMap", TypeMap.ToString()) });
            if (s_Tags == null)
            {
                s_Tags = new List<string>();
                s_Tags.AddRange(TypeMap.Values);
            }
            Log_Return("getTags", s_Tags.Count.ToString());
            return s_Tags;
        }

        private Dictionary<string, string> getTypeTagMap()
        {
            Log_FunctionCall("getTypeTagMap", new KeyValuePair<string, string>[] { });
            if (s_TypeTagMap == null)
            {
                s_TypeTagMap = new Dictionary<string, string>();
                s_TypeTagMap.Add("MyObjectBuilder_AmmoMagazine", "#ammo");
                s_TypeTagMap.Add("MyObjectBuilder_Component", "#component");
                s_TypeTagMap.Add("MyObjectBuilder_GasContainerObject", "#bottle");
                s_TypeTagMap.Add("MyObjectBuilder_OxygenContainerObject", "#bottle");
                s_TypeTagMap.Add("MyObjectBuilder_Ingot", "#ingot");
                s_TypeTagMap.Add("MyObjectBuilder_Ore", "#ore");
                s_TypeTagMap.Add("MyObjectBuilder_PhysicalGunObject", "#handtool");
            }

            Log_Return("getTypeTagMap", s_TypeTagMap.Count.ToString());
            return s_TypeTagMap;
        }

        private string getTag(IMyInventoryItem InventoryItem, Dictionary<string, string> TypeMap)
        {
            Log_FunctionCall("getTag", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("InvetoryItem", InventoryItem.Content.SubtypeName), new KeyValuePair<string, string>( "TypeMap", TypeMap.ToString()) });
            string tag;
            string key = InventoryItem.Content.TypeId.ToString();
            if (TypeMap.ContainsKey(key))
            {
                tag = TypeMap[key];
            } else
            {
                tag = key;
            }

            Log_Return("getTag", tag);
            return tag;
        }

        private void EchoSort(IMyTerminalBlock SourceBlock, IMyTerminalBlock DestinationBlock, IMyInventoryItem Item, VRage.MyFixedPoint ammount)
        {
            Log_FunctionCall("EchoSort", new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("SourceBlock", SourceBlock.CustomName), new KeyValuePair<string, string>("DestinationBlock", DestinationBlock.CustomName), new KeyValuePair<string, string>("Item", Item.Content.SubtypeName), new KeyValuePair<string, string>("ammount", ammount.ToString()) });
            Log(SourceBlock.CustomName + ">>(" + ammount.ToString() + "x" + Item.Content.SubtypeName + ")>>" + DestinationBlock.CustomName, LOG_LEVEL_ECHO);
            Log_Return("EchoSort", "void");
        }

        // DEBUG/LOG STUFF 

        private void bootStrapLog()
        {
            LogPanels = null;
            Log_FunctionCall("bootSTrapLog", new KeyValuePair<string, string>[] { });
            Log("Debug Settings: Debuglevel=" + LOG_LEVEL.ToString() + " DEBUGMODE='" + LOG_MODE.ToString() + "' DEBUGSCREEN='" + LOG_SCREEN + "'");
            Log_Return("bootStrapLog", "void");
        }

        private List<IMyTextPanel> getLogPanels()
        {
            if(LogPanels == null)
            {
                List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();

                if (LOG_SCREEN != null)
                {
                    GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(Blocks, (x => x.CustomName.Contains(LOG_SCREEN) && x.CubeGrid.Equals(Me.CubeGrid)));
                }

                LogPanels = Blocks.ConvertAll<IMyTextPanel>(( x => x as IMyTextPanel));
                for(int i = 0; i < LogPanels.Count; i++)
                {
                    LogPanels[i].WritePublicText("");
                    LogPanels[i].ShowPublicTextOnScreen();
                }
            }
            
            return LogPanels;
        }

        private void Log_AppendToPanels(string data)
        {
            List<IMyTextPanel> Panels = getLogPanels();
            for(int i = 0; i < Panels.Count; i++)
            {
                Panels[i].WritePublicText(data, true);
            }
        }

        private void Log(string msg, int logLevel = LOG_LEVEL_DEBUG)
        {
            if (LOG && logLevel <= LOG_LEVEL)
            {
                string data = "[LOG:" + logLevel.ToString() + "]" + msg;
                Echo(data);
                Log_AppendToPanels(data + "\n");
            }
        }

        private void Log_Return(string funcname, string value, int debugLevel = LOG_LEVEL_DEBUG)
        {
            if (LOG && LOG_MODE.Contains(LOG_MODE_RETURN))
            {
                Log("return " + funcname + " => " + value);
            }
        }

        private void Log_FunctionCall(string funcname, KeyValuePair<string,string>[] argv, int debugLevel = LOG_LEVEL_DEBUG)
        {
            if (LOG && LOG_MODE.Contains(LOG_MODE_FUNCTIONCALL))
            {
                StringBuilder SB = new StringBuilder();
                SB.Append(funcname);
                SB.Append("(");
                for(int i = 0; i < argv.Length; i++)
                {
                    SB.Append(argv[i].Key);
                    SB.Append(": ");
                    SB.Append(argv[i].Value);
                    if(i < argv.Length - 1)
                    {
                        SB.Append(", ");
                    }
                }
                SB.Append(");");
                Log(SB.ToString());
            }
        }
                
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}