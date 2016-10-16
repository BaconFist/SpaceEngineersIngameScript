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

namespace ItemConsumption
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        ItemConsumption
        ==============
        Copyright 2016 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE

        Description
        ===========

        */

        static Dictionary<string, long> lastRunItemAmmounts;
        static long lastRun = 0;
        long thisRun;
        List<IMyTerminalBlock> InventoryBlocks = new List<IMyTerminalBlock>();
        Dictionary<string, long> ItemAmmounts = new Dictionary<string, long>();

        public Program()
        {
            lastRunItemAmmounts = new Dictionary<string, long>();
        }

        string TextPanelTag = "[BIC]";

        public void Main(string argument)
        {
            boot();
            run(BaconArgs.parse(argument));
            shutdown();
        }

        void run(BaconArgs Args)
        {
            List<IMyTextPanel> TextPanels = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(TextPanels, (p => p.CubeGrid.Equals(Me.CubeGrid) && p.CustomName.Contains(TextPanelTag)));
            string[] PrivateText;
            StringBuilder PublicText = new StringBuilder();
            foreach (IMyTextPanel TextPanel in TextPanels)
            {
                PrivateText = TextPanel.GetPrivateText().Split(new char[] { '\n','\r' });
                if(TextPanel.GetPrivateText().Trim().Length < 1)
                {
                    PublicText.AppendLine("Write Commands to PrivateText");
                }

                foreach(string line in PrivateText)
                {
                    PublicText.AppendLine(parseCommand(line));
                }
                TextPanel.WritePublicText(PublicText.ToString());
                TextPanel.ShowPublicTextOnScreen();
            }
        }

        void boot()
        {
            thisRun = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(InventoryBlocks, (b => b.CubeGrid.Equals(Me.CubeGrid) && b.HasInventory()));
            countItems();
        }

        void shutdown()
        {
            lastRunItemAmmounts.Clear();
            foreach(KeyValuePair<string, long> v in ItemAmmounts)
            {
                lastRunItemAmmounts.Add(v.Key, v.Value);
            }
            ItemAmmounts.Clear();
            InventoryBlocks.Clear();
            lastRun = thisRun;
        }

        string parseCommand(string command)
        {
            BaconArgs Args = BaconArgs.parse(command);
            if (Args.getArguments().Count < 1)
            {
                return "";
            }

            string value = "";
            switch (Args.getArguments()[0].Trim().ToLower())
            {
                case "datetime":
                    value = parseCmd_DateTime(Args);
                    break;
                case "count":
                    value = parseCmd_Count(Args);
                    break;
                case "consumption":
                    value = parseCmd_Consumption(Args);
                    break;
                default:
                    value = "Unknown Command:" + Args.getArguments()[0].Trim();
                    break;
            }

            return value;
        }

        string parseCmd_Consumption(BaconArgs Args)
        {
            StringBuilder value = new StringBuilder();
            for(int i = 1; i < Args.getArguments().Count; i++)
            {
                string typeId = getNormalizedTypeId(Args.getArguments()[i]);
                if (!lastRunItemAmmounts.ContainsKey(typeId))
                {
                    value.AppendLine(typeId + ":pending");
                } else if (!ItemAmmounts.ContainsKey(typeId))
                {
                    value.AppendLine(typeId + ":empty");
                }
                else
                {
                    long lastAmmount = lastRunItemAmmounts[typeId];
                    long currentAmmount = ItemAmmounts[typeId];
                    long diff = currentAmmount - lastAmmount;
                    double msSinceLastRun = (thisRun - lastRun);
                    double itemPerSecond = diff / msSinceLastRun;
                    value.AppendLine(String.Format("lastAmmount: {0}", lastAmmount));
                    value.AppendLine(String.Format("currentAmmount: {0}", currentAmmount));
                    value.AppendLine(String.Format("thisRun: {0}", thisRun));
                    value.AppendLine(String.Format("lastRun: {0}", lastRun));
                    value.AppendLine(String.Format("msSinceLastRun: {0}", msSinceLastRun));
                    value.AppendLine(String.Format("itemPerSecond: {0}", itemPerSecond));
                    
                    value.AppendLine(typeId + ":" + itemPerSecond.ToString() + "/s");
                }
            }

            return value.ToString();
        }

        string parseCmd_Count(BaconArgs Args)
        {
            List<string> value = new List<string>();
            if(Args.getArguments().Count == 1)
            {
                foreach (KeyValuePair<string, long> Item in ItemAmmounts)
                {
                    value.Add(Item.Key + ":" + Item.Value.ToString());
                }
            } else
            {
                for(int i = 1; i < Args.getArguments().Count; i++)
                {
                    string typeId = getNormalizedTypeId(Args.getArguments()[i]);
                    if (ItemAmmounts.ContainsKey(typeId))
                    {
                        value.Add(typeId + ":" + getFormattedWeight(getNormalizedItemAmmount(ItemAmmounts[typeId])));
                    } else
                    {
                        value.Add(typeId + ":0");
                    }
                }
            }

            return string.Join("\n", value);
        }

        string parseCmd_DateTime(BaconArgs Args)
        {
            if(!(Args.getArguments().Count > 1))
            {
                return "DateTime needs at least one argument";
            }
            return DateTime.Now.ToString(Args.getArguments()[1]);
        }

        void countItems()
        {
            string typeId;
            foreach(IMyTerminalBlock Block in InventoryBlocks)
            {
                for(int i = 0; i < Block.GetInventoryCount(); i++)
                {
                    IMyInventory Inventory = Block.GetInventory(i);
                    foreach (IMyInventoryItem Item in Inventory.GetItems())
                    {
                        typeId = getNormalizedTypeId(Item);
                        if (!ItemAmmounts.ContainsKey(typeId))
                        {
                            ItemAmmounts.Add(typeId, 0);
                        }
                        ItemAmmounts[typeId] = ItemAmmounts[typeId] + Item.Amount.RawValue;
                    }
                }                    
            }
        }

        string getNormalizedTypeId(string type)
        {
            return type.ToLower();
        }

        string getNormalizedTypeId(IMyInventoryItem Item)
        {
            return getNormalizedTypeId(Item.Content.TypeId.ToString().ToLower().Replace("myobjectbuilder_", "") + "/" + Item.Content.SubtypeName);
        }

        double getNormalizedItemAmmount(long Ammount)
        {
            return Ammount / 1000000;
        }

        string getFormattedWeight(double number)
        {
            double num = Math.Round(number, 2);
            string ext = "";
            if (number > 1000)
            {
                num = Math.Round((double)number / 1000, 2);
                ext = "k";
            }
            
            return String.Format("{0:n} "+ext, num);
        }

        public class BaconArgs { static public BaconArgs parse(string a) { return (new Parser()).parseArgs(a); } static public string Escape(string a) { return a.Replace(@"\", @"\\").Replace(@" ", @"\ ").Replace(@"""", @"\"""); } static public string UnEscape(string a) { return a.Replace(@"\""", @"""").Replace(@"\ ", @" ").Replace(@"\\", @"\"); } public class Parser { static Dictionary<string, BaconArgs> h = new Dictionary<string, BaconArgs>(); public BaconArgs parseArgs(string a) { if (!h.ContainsKey(a)) { var b = new BaconArgs(); var c = false; var d = false; var e = new StringBuilder(); for (int f = 0; f < a.Length; f++) { var g = a[f]; if (c) { e.Append(g); c = false; } else if (g.Equals('\\')) c = true; else if (d && !g.Equals('"')) e.Append(g); else if (g.Equals('"')) d = !d; else if (g.Equals(' ')) if (e.ToString().Equals("--")) { b.add(a.Substring(f).TrimStart()); e.Clear(); break; } else { b.add(e.ToString()); e.Clear(); } else e.Append(g); } if (e.Length > 0) b.add(e.ToString()); h.Add(a, b); } return h[a]; } } protected Dictionary<char, int> h = new Dictionary<char, int>(); protected List<string> i = new List<string>(); protected Dictionary<string, List<string>> j = new Dictionary<string, List<string>>(); public List<string> getArguments() { return i; } public int getFlag(char a) { return h.ContainsKey(a) ? h[a] : 0; } public List<string> getOption(string a) { return j.ContainsKey(a) ? j[a] : new List<string>(); } public void add(string a) { if (!a.StartsWith("-")) i.Add(a); else if (a.StartsWith("--")) { KeyValuePair<string, string> b = k(a); var c = b.Key.Substring(2); if (!j.ContainsKey(c)) j.Add(c, new List<string>()); j[c].Add(b.Value); } else { var b = a.Substring(1); for (int d = 0; d < b.Length; d++) if (this.h.ContainsKey(b[d])) { this.h[b[d]]++; } else { this.h.Add(b[d], 1); } } } KeyValuePair<string, string> k(string a) { string[] b = a.Split(new char[] { '=' }, 2); return new KeyValuePair<string, string>(b[0], (b.Length > 1) ? b[1] : null); } string l(string a) { return (a != null) ? "\"" + a.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" : @"null"; } }
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}