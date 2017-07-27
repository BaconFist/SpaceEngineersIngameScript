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

                to use default values just put rhe key with no value to the config. (like "lcd.tag:" to use the default one
                

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

                "language: L"
                    defaults to english (en)
                    display language for all text
                    where L must be an available language code
                    available Language codes:
                        en: english
                        de: german     
            
                "opt.scanlimit: i"
                    defaults to 1
                    sets how far the script should look behind a panel to find something
                    where i must be integer greater "0"
                    example: set to "2" if you have a layer of armor between LCD and cargo container.

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

        const string CFG_LANGUAGE = "language";

        const string CFG_OPT_SCANLIMIT = "opt.scanlimit";
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

        string language;

        int optScanLimit = 1;
        #endregion config

        #region config defaults
        string defaultLcdTag = "[FLD]";

        char defaultBarColorMax = (char) (0xe100 + (4 << 6) + (0 << 3) + 0); //red
        char defaultBarColorHigh = (char) (0xe100 + (4 << 6) + (2 << 3) + 0); //orange
        char defaultBarColorDefault = (char) (0xe100 + (0 << 6) + (4 << 3) + 0); //green
        string defaultBarDivider = new String((char)(0xe100 + (4 << 6) + (4 << 3) + 4), LCD_WIDTH);

        char defaultColorBackground = (char) (0xe100 + (0 << 6) + (0 << 3) + 0); //black

        bool defaultShowBlockState = true;

        char defaultStateColorOff = (char) (0xe100 + (4 << 6) + (0 << 3) + 0); //red
        char defaultStateColorIdle = (char) (0xe100 + (4 << 6) + (2 << 3) + 0); //orange
        char defaultStateColorOn = (char) (0xe100 + (0 << 6) + (4 << 3) + 0); //green

        string defaultLanguage = "en";

        int defaultOptScanLimit = 1;
        #endregion config defaults

        #region text
        Dictionary<string, Dictionary<string, string>> Text = new Dictionary<string, Dictionary<string, string>>() {
            {"en", new Dictionary<string, string>(){
                    {"statUsedTag","used Tag: {0}"}, // {0} => LCD Tag
                    {"statLoad","Load: {0}% (Limit: {1}%)"}, // {0} => current load, {1} => max load
                    {"statInstructions","Instructions: {0}/{1}"}, // {0} => current instructions, {1} => max instructions (50.000)
                    {"statRuntime","Runtime (ms): {0}"}, //{0} => script runtime (realtime) in milliseconds
                    {"statProgressed","Progressed: {0} Panels"}, // {0} => number of panels progressed in this run
                    {"statQueued","Queued: {0} Panels"}, // {0} => number of panels delayed to the next run
                    {"lcdStatSingleInventory","= Bacon's autoamted Fill Level Display =\nInventory Block: {0}\nLast Update: {1}\nLevel Inventory: ~{2}%"}, // {0} => Name of the Block with the inventory, {1} => datetime of last update, {2} => fill level of the inventory in percent
                    {"lcdStatDoubleInventory","= Bacon's autoamted Fill Level Display =\nInventory Block: {0}\nLast Update: {1}\nLevel Inventory #0: ~{2}%\nLevel Inventory #1: ~{3}%"}, // {0} => Name of the Block with the inventory, {1} => datetime of last update, {2} => fill level of 1st the inventory in percent, {3} => fill level of 2nd the inventory in percent
                    {"warnNoInventory","Container not found. (Mount this Panel to\na Block with an Inventory.)"}
                }
            },
            {"de", new Dictionary<string, string>(){
                    {"statUsedTag","Verwendeter Tag: {0}"}, // {0} => LCD Tag
                    {"statLoad","Last: {0}% (Grenze: {1}%)"}, // {0} => current load, {1} => max load
                    {"statInstructions","Befehle: {0}/{1}"}, // {0} => current instructions, {1} => max instructions (50.000)
                    {"statRuntime","Laufzeit (ms): {0}"}, //{0} => script runtime (realtime) in milliseconds
                    {"statProgressed","Verabeitet: {0} Panels"}, // {0} => number of panels progressed in this run
                    {"statQueued","In der Warteschlange: {0} Panels"}, // {0} => number of panels delayed to the next run
                    {"lcdStatSingleInventory","= Bacon's automatische Füllstandsanzeige =\nInventar Block: {0}\nletzte Aktualisierung: {1}\nInventar Füllstand: ~{2}%"}, // {0} => Name of the Block with the inventory, {1} => datetime of last update, {2} => fill level of the inventory in percent
                    {"lcdStatDoubleInventory","= Bacon's automatische Füllstandsanzeige =\nInventar Block: {0}\nletzte Aktualisierung: {1}\nInventar #0 Füllstand: ~{2}%\nInventar #1 Füllstand: ~{3}%"}, // {0} => Name of the Block with the inventory, {1} => datetime of last update, {2} => fill level of 1st the inventory in percent, {3} => fill level of 2nd the inventory in percent
                    {"warnNoInventory","Frachtblock nicht gefunden. (Bau das LCD an einen Block mit Inventar.)"}
                }
            }
        };

        #endregion text

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
            Echo(getText("statUsedTag", lcdTag));
            Echo(getText("statLoad", (Runtime.CurrentInstructionCount * 100) / Runtime.MaxInstructionCount, (LOAD_LIMIT * 100) / Runtime.MaxInstructionCount));
            Echo(getText("statInstructions", Runtime.CurrentInstructionCount, Runtime.MaxInstructionCount));
            Echo(getText("statRuntime", (DateTime.Now - Start).TotalMilliseconds));
            
            Echo(getText("statProgressed", panelProgressCount));
            Echo(getText("statQueued",PanelQueue.Count));
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
                    switch (Cargo.InventoryCount)
                    {
                        case 1:
                            Panel.CustomData = getText("lcdStatSingleInventory", Cargo.CustomName, DateTime.Now, fillLevel0);
                            break;
                        case 2:
                            Panel.CustomData = getText("lcdStatDoubleInventory", Cargo.CustomName, DateTime.Now, fillLevel0, fillLevel1);
                            break;
                    }
                    Panel.WritePublicText(fillBar);
                    Panel.SetValueFloat("FontSize", fontSizeFuelBar);
                    Panel.SetValue<long>("Font", fontIdMonospaced);
                    Panel.ShowPublicTextOnScreen();
                } else
                {
                    Panel.WritePublicText(getText("warnNoInventory"));
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
            Vector3I MPV = getMountPointVector(Panel);
            Vector3I LastPos = Panel.Position;

            for(int i = 0; i < optScanLimit; i++)
            {
                LastPos = LastPos + MPV;
                if (tryFindCargoAtPosition(LastPos, Panel.CubeGrid, out Container))
                {
                    return Container;
                }                
            }

            return null;
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
                            } else
                            {
                                lcdTag = defaultLcdTag;
                            }
                            break;
                        case CFG_BAR_COLOR_DEFAULT:
                            if(KeyValue[1].Trim().Length == 0)
                            {
                                barColorDefault = defaultBarColorDefault;
                            } else if(TryParseColor(KeyValue[1], out colorBuffer))
                            {
                                barColorDefault = colorBuffer;
                            }
                            break;
                        case CFG_BAR_COLOR_DIVIDER:
                            if (KeyValue[1].Trim().Length == 0)
                            {
                                barDivider = defaultBarDivider;
                            }
                            else if (TryParseColor(KeyValue[1], out colorBuffer))
                            {
                                barDivider = new String(colorBuffer, LCD_WIDTH);
                            }
                            break;
                        case CFG_BAR_COLOR_HIGH:

                            if (KeyValue[1].Trim().Length == 0)
                            {
                                barColorHigh = defaultBarColorHigh;
                            }
                            else if (TryParseColor(KeyValue[1], out colorBuffer))
                            {
                                barColorHigh = colorBuffer;
                            }
                            break;
                        case CFG_BAR_COLOR_MAX:
                            if (KeyValue[1].Trim().Length == 0)
                            {
                                barColorMax = defaultBarColorMax;
                            }
                            else if (TryParseColor(KeyValue[1], out colorBuffer))
                            {
                                barColorMax = colorBuffer;
                            }
                            break;
                        case CFG_COLOR_BACKGROUND:
                            if (KeyValue[1].Trim().Length == 0)
                            {
                                colorBackground = defaultColorBackground;
                            }
                            else if (TryParseColor(KeyValue[1], out colorBuffer))
                            {
                                colorBackground = colorBuffer;
                            }
                            break;
                        case CFG_STATE_COLOR_IDLE:
                            if (KeyValue[1].Trim().Length == 0)
                            {
                                stateColorIdle = defaultStateColorIdle;
                            }
                            else if (TryParseColor(KeyValue[1], out colorBuffer))
                            {
                                stateColorIdle = colorBuffer;
                            }
                            break;
                        case CFG_STATE_COLOR_OFF:
                            if (KeyValue[1].Trim().Length == 0)
                            {
                                stateColorOff = defaultStateColorOff;
                            }
                            else if (TryParseColor(KeyValue[1], out colorBuffer))
                            {
                                stateColorOff = colorBuffer;
                            }
                            break;
                        case CFG_STATE_COLOR_ON:
                            if (KeyValue[1].Trim().Length == 0)
                            {
                                stateColorOn = defaultStateColorOn;
                            }
                            else if (TryParseColor(KeyValue[1], out colorBuffer))
                            {
                                stateColorOn = colorBuffer;
                            }
                            break;
                        case CFG_STATE_SHOW:
                            if (KeyValue[1].Trim().Length == 0)
                            {
                                showBlockState = defaultShowBlockState;
                            }
                            else
                            {
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
                            }
                            break;
                        case CFG_LANGUAGE:
                            if (isLanguageAvailable(KeyValue[1].Trim().ToLowerInvariant()))
                            {
                                language = KeyValue[1].Trim().ToLowerInvariant();
                            }
                            break;
                        case CFG_OPT_SCANLIMIT:
                            int buffer = 0;
                            if(int.TryParse(KeyValue[1].Trim(), out buffer) && buffer > 0)
                            {
                                optScanLimit = buffer;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        string getText(string key, params object[] values)
        {
            string format = Text[language]?[key];
            string result = string.Format("Error: No translation for '{0}' available.", key);
            if(format == null)
            {
                format = Text[defaultLanguage]?[key];
            }
            if (format != null)
            {
                try
                {
                    result = string.Format(format, values);
                }
                catch (Exception e)
                {
                    result = "Error: " + e.Message;
                }
            }

            return result;
        }

        bool isLanguageAvailable(string key)
        {
            return Text.ContainsKey(key);
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
            lcdTag = defaultLcdTag;
            barColorMax = defaultBarColorMax;
            barColorHigh = defaultBarColorHigh;
            barColorDefault = defaultBarColorDefault;
            barDivider = defaultBarDivider;
            colorBackground = defaultColorBackground;
            showBlockState = defaultShowBlockState;
            stateColorOff = defaultStateColorOff;
            stateColorIdle = defaultStateColorIdle;
            stateColorOn = defaultStateColorOn;
            language = defaultLanguage;
            optScanLimit = defaultOptScanLimit;
        }

        public void writeDefaultConfigToCustomData()
        {
            StringBuilder slug = new StringBuilder();
            slug.AppendLine(string.Format(@"{0}:", CFG_LCD_TAG));

            slug.AppendLine(string.Format(@"{0}:", CFG_BAR_COLOR_MAX));
            slug.AppendLine(string.Format(@"{0}:", CFG_BAR_COLOR_HIGH));
            slug.AppendLine(string.Format(@"{0}:", CFG_BAR_COLOR_DEFAULT));
            slug.AppendLine(string.Format(@"{0}:", CFG_BAR_COLOR_DIVIDER));

            slug.AppendLine(string.Format(@"{0}:", CFG_COLOR_BACKGROUND));

            slug.AppendLine(string.Format(@"{0}:", CFG_STATE_SHOW));
            slug.AppendLine(string.Format(@"{0}:", CFG_STATE_COLOR_OFF));
            slug.AppendLine(string.Format(@"{0}:", CFG_STATE_COLOR_IDLE));
            slug.AppendLine(string.Format(@"{0}:", CFG_STATE_COLOR_ON));

            slug.AppendLine(string.Format(@"{0}:", CFG_LANGUAGE));

            slug.AppendLine(string.Format(@"{0}:", CFG_OPT_SCANLIMIT));
            Me.CustomData = slug.ToString();
        }
        
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}