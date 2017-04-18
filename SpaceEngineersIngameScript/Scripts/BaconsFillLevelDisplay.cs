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

namespace BaconsFillLevelDisplay
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        BaconsFillLevelDisplay
        ==============
        Copyright 2017 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE

        Description
        ===========
        Adds Fill level bars to containers and other blocks with inventories by simply adding an LCD.

        Prepare:
            1. Load this script to a Programmable Block
            2. run this Programmable block with a timer (the the time to your needs)
            
        How to add a fill level bar to any container:
            1. build an TextPanel next to this container.
            2. add "[FLD]" to the TextPanel's name.
            - textpanel should show a fill bar

        Customization:
            You can change the TAG ([FLD]) to your needs by adding another one as the argument.

        */

        Dictionary<IMyTextPanel, IMyTerminalBlock> PanelToCargoMap = new Dictionary<IMyTextPanel, IMyTerminalBlock>();
        Queue<IMyTextPanel> PanelQueue = new Queue<IMyTextPanel>();
        

        const int LOAD_LIMIT = 35000;
        string tag;
        string defaultTag = "[FLD]";
        char color_red = (char)(0xe100 + (4 << 6) + (0 << 3) + 0);
        char color_orange = (char)(0xe100 + (4 << 6) + (2 << 3) + 0);
        char color_green = (char)(0xe100 + (0 << 6) + (4 << 3) + 0);
        string divider = new String((char)(0xe100 + (4 << 6) + (4 << 3) + 4), 100);
        float fontSizeFuelBar = 0.18f;
        float fontSizeText = 1f;
        long fontIdMonospaced = 1147350002;
        long fontIdRed = -795103743;
        int panelProgressCount = 0;
        DateTime Start;

        public void Main(string argument)
        {
            try
            {
                Start = DateTime.Now;
                panelProgressCount = 0;
                tag = (argument.Trim().Length > 0) ? argument.Trim() : defaultTag;
                enqueuePanels();
                progressPanels();
                statisics();
            }
            catch (Exception e)
            {
                Echo(string.Format(@"Exception: {0}", e.Message));
            }
        }

        public void statisics()
        {
            Echo(string.Format(@"used Tag: {0}", tag));
            Echo(string.Format(@"Load: {0}% (Limit: {1}%)", (Runtime.CurrentInstructionCount * 100) / Runtime.MaxInstructionCount, (LOAD_LIMIT * 100) / Runtime.MaxInstructionCount));
            Echo(string.Format(@"Instructions: {0}/{1}", Runtime.CurrentInstructionCount, Runtime.MaxInstructionCount));
            Echo(string.Format(@"Runtime (ms): {0}", (DateTime.Now - Start).TotalMilliseconds));
            
            Echo(string.Format(@"Progressed: {0} Panels", panelProgressCount));
            Echo(string.Format(@"Queued: {0} Panels",PanelQueue.Count));
        }       

        public void enqueuePanels()
        {
            List<IMyTextPanel> Matches = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(Matches, (p => p.CubeGrid.Equals(Me.CubeGrid) && p.CustomName.Contains(tag) && !PanelQueue.Contains(p)));
            foreach(IMyTextPanel Panel in Matches)
            {
                PanelQueue.Enqueue(Panel);
            }
        }

        public void progressPanels()
        {
            while(Runtime.CurrentInstructionCount < LOAD_LIMIT && PanelQueue.Count > 0)
            {
                panelProgressCount++;
                long panelId = PanelQueue.Dequeue().EntityId;
                IMyTextPanel Panel = GridTerminalSystem.GetBlockWithId(panelId) as IMyTextPanel;

                IMyTerminalBlock Cargo = getCargo(Panel);
                if (Panel != null && Panel is IMyTextPanel && Cargo != null && Panel.IsFunctional && Panel.IsWorking)
                {
                    int fillLevel0 = 0;
                    int fillLevel1 = 0;
                    string fillBar = getFillLevelBarForBlock(Cargo, out fillLevel0, out fillLevel1);
                    Panel.CustomData = string.Format("= Bacon's autoamted Fill Level Display =\nInventory Block: {0}\nLast Update: {1}\nLevel Inventory #0: ~{2}%\nLevel Inventory #1: ~{3}%", Cargo.CustomName, DateTime.Now, fillLevel0, fillLevel1);
                    Panel.WritePublicText(fillBar);
                    Panel.SetValueFloat("FontSize", fontSizeFuelBar);
                    Panel.SetValue<long>("Font", fontIdMonospaced);
                    Panel.ShowPublicTextOnScreen();
                } else
                {
                    Panel.WritePublicText("Container not found. (Mount this Panel to\na Block with a Inventory.)");
                    Panel.SetValueFloat("FontSize", fontSizeText);
                    Panel.SetValue<long>("Font", fontIdRed);
                    Panel.ShowPublicTextOnScreen();
                }
            }
        }

        public IMyTerminalBlock getCargo(IMyTextPanel Panel)
        {
            if (PanelToCargoMap.ContainsKey(Panel))
            {
                IMyTerminalBlock buffer = GridTerminalSystem.GetBlockWithId(PanelToCargoMap[Panel].EntityId);
                if (buffer != null && buffer is IMyButtonPanel)
                {
                    return buffer as IMyButtonPanel;
                }
                else
                {
                    PanelToCargoMap.Remove(Panel);
                }
            }
            IMyTerminalBlock Cargo = getBlockBehind(Panel);
            if (Cargo != null && Cargo.HasInventory)
            {
                PanelToCargoMap.Add(Panel, Cargo);
                return Cargo;
            }

            return null;
        }

        public string getFillLevelBarForBlock(IMyTerminalBlock Block, out int fillLevel0, out int fillLevel1)
        {
            fillLevel0 = 0;
            fillLevel1 = 0;

            if (!Block.HasInventory)
            {
                return "";
            }
            StringBuilder slug = new StringBuilder();

            string bar0 = getFillLevelBarForInventory(Block.GetInventory(0), out fillLevel0);
            switch (Block.InventoryCount)
            {
                case 1:
                    
                    for(int i = 0; i <= 15; i++)
                    {
                        slug.AppendLine(bar0);
                    }
                    break;
                case 2:
                    string bar1 = getFillLevelBarForInventory(Block.GetInventory(1), out fillLevel1);
                    for (int i = 0; i <= 7; i++)
                    {
                        slug.AppendLine(bar0);
                    }
                    slug.AppendLine(divider);
                    for (int i = 0; i <= 7; i++)
                    {
                        slug.AppendLine(bar1);
                    }
                    break;
            }

            return slug.ToString();
        }

        public string getFillLevelBarForInventory(IMyInventory Inventory, out int fillLevel)
        {
            fillLevel = getPercentage(Inventory);
            if(fillLevel == 0)
            {
                return "";
            }
            if(fillLevel < 75)
            {
                return new string(color_green, fillLevel);
            } else if(75 <= fillLevel && fillLevel < 90)
            {
                return new string(color_orange, fillLevel);
            } else if(90 <= fillLevel)
            {
                return new string(color_red, fillLevel);
            } else
            {
                return "";
            }
        }



        public int getPercentage(IMyInventory Inventory)
        {
            long current = Inventory.CurrentVolume.RawValue;
            long max = Inventory.MaxVolume.RawValue;
            if(max == 0)
            {
                return 100;
            }

            int level = Convert.ToInt32((current * 100) / max);
            if(level == 0 && current > 0)
            {
                level = 1;
            }

            return level;
        }


        public IMyTerminalBlock getBlockBehind(IMyTextPanel Panel)
        {
            IMyTerminalBlock Container;
            if(tryFindCargoAtPosition(Panel.Position + getMountPointVector(Panel), Panel.CubeGrid, out Container)){
                return Container;
            } else
            {
                return null;
            }
        }

        public Vector3I getMountPointVector(IMyTerminalBlock Block)
        {
            Matrix localMatrix;
            Block.Orientation.GetMatrix(out localMatrix);
            Vector3 buffer;
            switch (Block.BlockDefinition.SubtypeName)
            {
                case "LargeBlockCorner_LCD_Flat_1":
                case "LargeBlockCorner_LCD_Flat_2":
                case "LargeBlockCorner_LCD_1":
                case "LargeBlockCorner_LCD_2":
                case "SmallBlockCorner_LCD_Flat_1":
                case "SmallBlockCorner_LCD_Flat_2":
                case "SmallBlockCorner_LCD_1":
                case "SmallBlockCorner_LCD_2":
                    buffer = localMatrix.Down;
                    break;
                default:
                    buffer = localMatrix.Forward;
                    break;               
            }

            return new Vector3I(buffer);
        }
        
        public bool tryFindCargoAtPosition(Vector3I Position, IMyCubeGrid Grid, out IMyTerminalBlock Match)
        {
            IMySlimBlock SlimBlock = Grid.GetCubeBlock(Position);
            if (SlimBlock != null && SlimBlock.FatBlock != null && SlimBlock.FatBlock is IMyTerminalBlock && SlimBlock.FatBlock.HasInventory)
            {
                Match = SlimBlock.FatBlock as IMyTerminalBlock;
                return true;
            } else
            {
                Match = null;
                return false;
            }
        }
        
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}