﻿using System;
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
                green -> below 75%
                orange -> between 75% and 90%
                red -> above 90%
        Display state of Production Blocks (Reffineries, Assemblers, Arc) 
                green border -> on and working
                orange border -> on but idle
                red border -> off

        Prepare:
            1. Load this script to a Programmable Block
            2. run this Programmable block with a timer (the the time to your needs)
            
        How to add a fill level bar to any container:^/
            1. build an TextPanel next to this container.
            2. add "[FLD]" to the TextPanel's name.
            - textpanel should show a fill bar
    
        Configuratoin: 
            Arguments:
                You can change the TAG ([FLD]) to your needs by adding another one as the argument.

            CustomData:
                You can configure the Script via the Programmable Block's CustomData.
                Deleta all from CustomData and recompile script to reset Configuration.

                Available options :
                (all without quotes)

                "lcd.tag:TAG":
                    defaults to [FLD]
                    this tag must be in the name of LCDs that should be used to display fill bars
                    this tag might be overwritten by the Programmable Block's argument
                    where TAG must not be empty

                "bar.color.max:r,g,b"
                    defaukts to red (4,0,0)
                    color for the bar when filled over 90%
                    where r,g, and b must be between 0 and 7 (including)

                "bar.color.high:r,g,b"
                    defaults to orange (4,2,0)
                    color for the bar when filled over between 75% and 90%
                    where r,g, and b must be between 0 and 7 (including)

                "bar.color.default:r,g,b"
                    defaults to green (0,4,0)
                    color for the bar when filled lower than 75%
                    where r,g, and b must be between 0 and 7 (including)

                "color.background:r,g,b"
                    defaults to black (0,0,0)
                    backgorund color
                    where r,g, and b must be between 0 and 7 (including)

                "state.show:bool"
                    defaults to true
                    enables/disables the border indicating the block state for production blocks
                    where bool must be any of [true,on,yes,y,1] to enable, anything else will disable this option.

                "state.color.off:r,g,b"
                    defaults to red(4,,00)
                    color for the border when the block is turned OFF
                    where r,g, and b must be between 0 and 7 (including)

                "state.color.idle:r,g,b"
                    defaults to orange (4,2,0)
                    color for the border when the block is ON but Idling (like not precessing ore)
                    where r,g, and b must be between 0 and 7 (including)

                "state.color.on:r,g,b"
                    defaults to green (0,4,0)
                    color for the border when the block is ON and WORKING
                    where r,g, and b must be between 0 and 7 (including)

        */

        Dictionary<IMyTextPanel, IMyTerminalBlock> PanelToCargoMap = new Dictionary<IMyTextPanel, IMyTerminalBlock>();
        Queue<IMyTextPanel> PanelQueue = new Queue<IMyTextPanel>();


        const int LOAD_LIMIT = 35000;
        float fontSizeFuelBar = 0.178f;
        float fontSizeText = 1f;
        long fontIdMonospaced = 1147350002;
        long fontIdRed = -795103743;
        int panelProgressCount = 0;
        DateTime Start;

        const int LCD_WIDTH = 100;

        int CustomDataHash = 0;

        #region CustomData config Keys
        const string CFG_LCD_TAG = "lcd.tag";

        const string CFG_BAR_COLOR_MAX = "bar.color.max";
        const string CFG_BAR_COLOR_HIGH = "bar.color.high";
        const string CFG_BAR_COLOR_DEFAULT = "bar.color.default";
        const string CFG_BAR_COLOR_DIVIDER = "bar.color.divider";

        const string CFG_COLOR_BACKGROUND = "color.background";

        const string CFG_STATE_SHOW = "state.show";
        const string CFG_STATE_COLOR_OFF = "state.color.off";
        const string CFG_STATE_COLOR_IDLE = "state.color.idle";
        const string CFG_STATE_COLOR_ON = "state.color.on";
        
        #endregion CustomData config Keys

        #region config
        string lcdTag; // default to [FLD]

        char barColorMax;
        char barColorHigh;
        char barColorDefault;
        string barDivider;

        char colorBackground;

        bool showBlockState;

        char stateColorOff;
        char stateColorIdle;
        char stateColorOn;

        #endregion config

        public Program()
        {
            if(Me.CustomData.Trim().Length == 0)
            {
                writeDefaultConfigToCustomData();
            }
            updateConfigFromCustomData(true);
        }

        public void Main(string argument)
        {
            try
            {
                Start = DateTime.Now;
                panelProgressCount = 0;
                updateConfigFromCustomData();
                if (argument.Trim().Length > 0) {
                    lcdTag = argument.Trim();
                }
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
            Echo(string.Format(@"used Tag: {0}", lcdTag));
            Echo(string.Format(@"Load: {0}% (Limit: {1}%)", (Runtime.CurrentInstructionCount * 100) / Runtime.MaxInstructionCount, (LOAD_LIMIT * 100) / Runtime.MaxInstructionCount));
            Echo(string.Format(@"Instructions: {0}/{1}", Runtime.CurrentInstructionCount, Runtime.MaxInstructionCount));
            Echo(string.Format(@"Runtime (ms): {0}", (DateTime.Now - Start).TotalMilliseconds));
            
            Echo(string.Format(@"Progressed: {0} Panels", panelProgressCount));
            Echo(string.Format(@"Queued: {0} Panels",PanelQueue.Count));
        }       

        public void enqueuePanels()
        {
            List<IMyTextPanel> Matches = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(Matches, (p => p.CubeGrid.Equals(Me.CubeGrid) && p.CustomName.Contains(lcdTag) && !PanelQueue.Contains(p)));
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

            bool showBorderForThisBlock = showBlockState && Block is IMyProductionBlock;

            char stateColor = showBlockState?getStateColor(Block):'\0';

            List<string> slug = new List<string>();

            string bar0 = getFillLevelBarForInventory(Block.GetInventory(0), out fillLevel0);
            if (showBorderForThisBlock)
            {
                bar0 = stateColor + bar0.Substring(2) + stateColor;
            }

            switch (Block.InventoryCount)
            {
                case 1:
                    
                    for(int i = 0; i <= 15; i++)
                    {
                        slug.Add(bar0);
                    }
                    break;
                case 2:
                    string bar1 = getFillLevelBarForInventory(Block.GetInventory(1), out fillLevel1);
                    if (showBorderForThisBlock)
                    {
                        bar1 = stateColor + bar1.Substring(2) + stateColor;
                    }
                    for (int i = 0; i <= 7; i++)
                    {
                        slug.Add(bar0);
                    }
                    slug.Add(showBorderForThisBlock?stateColor + barDivider.Substring(0,LCD_WIDTH-1)+stateColor:barDivider);
                    for (int i = 0; i <= 7; i++)
                    {
                        slug.Add(bar1);
                    }
                    break;
            }
            if (showBorderForThisBlock && slug.Count > 0)
            {
                slug[0] = new String(stateColor, LCD_WIDTH);
                slug[slug.Count - 1] = slug[0];
            }


            return string.Join("\n", slug.ToArray());
        }

        public char getStateColor(IMyTerminalBlock Block)
        {
            char stateColor;
            if (Block.IsWorking)
            {
                if (Block is IMyProductionBlock)
                {
                    if ((Block as IMyProductionBlock).IsProducing)
                    {
                        stateColor = stateColorOn;
                    }
                    else
                    {
                        stateColor = stateColorIdle;
                    }
                }
                else
                {
                    stateColor = stateColorOn;
                }
            }
            else
            {
                stateColor = stateColorOff;
            }
            return stateColor;
        }

        public string getFillLevelBarForInventory(IMyInventory Inventory, out int fillLevel)
        {
            string buffer;
            fillLevel = getPercentage(Inventory);
            if(fillLevel == 0)
            {
                buffer = "";
            }
            if(fillLevel < 75)
            {
                buffer = new string(barColorDefault, fillLevel);
            } else if(75 <= fillLevel && fillLevel < 90)
            {
                buffer = new string(barColorHigh, fillLevel);
            } else if(90 <= fillLevel)
            {
                buffer = new string(barColorMax, fillLevel);
            } else
            {
                buffer = "";
            }

            buffer = buffer + new String(colorBackground, LCD_WIDTH - buffer.Length);

            return buffer;
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

        public void updateConfigFromCustomData(bool forceUpdate = false)
        {
            int currentCustomDataHash = Me.CustomData.GetHashCode();
            if (forceUpdate ||!CustomDataHash.Equals(currentCustomDataHash))
            {
                applyDefaultConfigValues();
                readConfig();
                CustomDataHash = currentCustomDataHash;
            }
        }

        public void readConfig()
        {
            string[] CustomData = Me.CustomData.Split(new Char[] {'\n','\r'}, StringSplitOptions.RemoveEmptyEntries);
            char colorBuffer;
            foreach (string configLine in CustomData)
            {
                string[] KeyValue = configLine.Split(new Char[] {':'}, 2);
                if(KeyValue.Length == 2)
                {
                    switch (KeyValue[0])
                    {
                        case CFG_LCD_TAG:
                            if (KeyValue[1].Trim().Length > 0)
                            {
                                lcdTag = KeyValue[1].Trim();
                            }
                            break;
                        case CFG_BAR_COLOR_DEFAULT:
                            if(TryParseColor(KeyValue[1], out colorBuffer))
                            {
                                barColorDefault = colorBuffer;
                            }
                            break;
                        case CFG_BAR_COLOR_DIVIDER:
                           if (TryParseColor(KeyValue[1], out colorBuffer))
                            {
                                barDivider = new String(colorBuffer, LCD_WIDTH);
                            }
                            break;
                        case CFG_BAR_COLOR_HIGH:
                       
                            if (TryParseColor(KeyValue[1], out colorBuffer))
                            {
                                barColorHigh = colorBuffer;
                            }
                            break;
                        case CFG_BAR_COLOR_MAX:
                            if (TryParseColor(KeyValue[1], out colorBuffer))
                            {
                                barColorMax = colorBuffer;
                            }
                            break;
                        case CFG_COLOR_BACKGROUND:
                            if (TryParseColor(KeyValue[1], out colorBuffer))
                            {
                                colorBackground = colorBuffer;
                            }
                            break;
                        case CFG_STATE_COLOR_IDLE:
                            if (TryParseColor(KeyValue[1], out colorBuffer))
                            {
                                stateColorIdle = colorBuffer;
                            }
                            break;
                        case CFG_STATE_COLOR_OFF:
                            if (TryParseColor(KeyValue[1], out colorBuffer))
                            {
                                stateColorOff = colorBuffer;
                            }
                            break;
                        case CFG_STATE_COLOR_ON:
                            if (TryParseColor(KeyValue[1], out colorBuffer))
                            {
                                stateColorOn = colorBuffer;
                            }
                            break;
                        case CFG_STATE_SHOW:
                            switch (KeyValue[1].Trim().ToLowerInvariant())
                            {
                                case "true":
                                case "on":
                                case "yes":
                                case "y":
                                case "1":
                                    showBlockState = true;
                                    break;
                                default:
                                    showBlockState = false;
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public bool TryParseColor(string value, out char color)
        {
            string[] rgbValues = value.Trim().Split(new Char[] {','}, 3);
            byte R, G, B;
            if(rgbValues.Length == 3 && byte.TryParse(rgbValues[0].Trim(), out R) && 0 <= R && R <= 7 && byte.TryParse(rgbValues[1].Trim(), out G) && 0 <= G && G <= 7 &&  byte.TryParse(rgbValues[2].Trim(), out B) && 0 <= B && B <= 7)
            {
                color = (char)(0xe100 + (R << 6) + (G << 3) + B);
                return true;
            }
            color = '\0';
            return false;            
        }


        public void applyDefaultConfigValues()
        {
            lcdTag = "[FLD]";

            barColorMax = (char)(0xe100 + (4 << 6) + (0 << 3) + 0); //red
            barColorHigh = (char)(0xe100 + (4 << 6) + (2 << 3) + 0); //orange
            barColorDefault = (char)(0xe100 + (0 << 6) + (4 << 3) + 0); //green
            barDivider = new String((char)(0xe100 + (4 << 6) + (4 << 3) + 4), LCD_WIDTH);

            colorBackground = (char)(0xe100 + (0 << 6) + (0 << 3) + 0); //black

            showBlockState = true;

            stateColorOff = (char)(0xe100 + (4 << 6) + (0 << 3) + 0); //red
            stateColorIdle = (char)(0xe100 + (4 << 6) + (2 << 3) + 0); //orange
            stateColorOn = (char)(0xe100 + (0 << 6) + (4 << 3) + 0); //green
        }

        public void writeDefaultConfigToCustomData()
        {
            StringBuilder slug = new StringBuilder();
            slug.AppendLine(string.Format(@"{0}:{1}", CFG_LCD_TAG, "[FLD]"));

            slug.AppendLine(string.Format(@"{0}:{1}", CFG_BAR_COLOR_MAX, "4,0,0"));
            slug.AppendLine(string.Format(@"{0}:{1}", CFG_BAR_COLOR_HIGH, "4,2,0"));
            slug.AppendLine(string.Format(@"{0}:{1}", CFG_BAR_COLOR_DEFAULT, "0,4,0"));
            slug.AppendLine(string.Format(@"{0}:{1}", CFG_BAR_COLOR_DIVIDER, "4,4,4"));

            slug.AppendLine(string.Format(@"{0}:{1}", CFG_COLOR_BACKGROUND, "0,0,0"));

            slug.AppendLine(string.Format(@"{0}:{1}", CFG_STATE_SHOW, "on"));
            slug.AppendLine(string.Format(@"{0}:{1}", CFG_STATE_COLOR_OFF, "4,0,0"));
            slug.AppendLine(string.Format(@"{0}:{1}", CFG_STATE_COLOR_IDLE, "4,2,0"));
            slug.AppendLine(string.Format(@"{0}:{1}", CFG_STATE_COLOR_ON, "0,4,0"));


            Me.CustomData = slug.ToString();
        }
        
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}