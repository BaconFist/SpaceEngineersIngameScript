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

namespace BaconCLI
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        BaconCLI
        ==============
        Copyright 2016 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE

        Description
        ===========
            Workflow: Tpye text/command to LCD private text -> parse command and dispaly on Public Text.
        */

        public Program()
        {

            // The constructor, called only once every session and
            // always before any other method is called. Use it to
            // initialize your script. 
            //     
            // The constructor is optional and can be removed if not
            // needed.

        }

        public void Save()
        {

            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.

        }

        public void Main(string argument)
        {
            // The main entry point of the script, invoked every time
            // one of the programmable block's Run actions are invoked.
            // 
            // The method itself is required, but the argument above
            // can be removed if not needed.
        }

        public class BaconCLI
        {
            private BaconDebug debug;
            private IMyGridTerminalSystem GridTerminalSystem;
            private Dictionary<string, Command> Commands = new Dictionary<string, Command>();

            public BaconCLI(IMyTextPanel Panel, IMyGridTerminalSystem GridTerminalSystem, BaconDebug debug)
            {
                debug.newScope("BaconCLI");
                this.debug = debug;
                this.GridTerminalSystem = GridTerminalSystem;
                debug.add("Create new BaconCLI for \"" + Panel.CustomName + "\"", BaconDebug.DEBUG);
                debug.leaveScope();
            }

            private bool TryRunCommand(string command, BaconArgs Args, out string result)
            {
                debug.newScope("runCommand");
                result = null;
                bool success = false;
                if (Commands.ContainsKey(command))
                {
                    success = Commands[command].run(Args, out result);
                } else
                {
                    debug.add("Unknown command: " + command, BaconDebug.ERROR);
                }

                debug.leaveScope();
                return success;
            }

            public class History : Dictionary<long, List<string>>
            {
                const int MAX_ENTRIES = 20;
                public void Add(IMyTextPanel Panel, string line)
                {
                    long id = Panel.EntityId;
                    if (!this.ContainsKey(id))
                    {
                        this.Add(id, new List<string>());
                    }
                    this[id].Add(line);
                    if(this[id].Count > MAX_ENTRIES)
                    {
                        this[id].Reverse();
                        this[id] = this[id].GetRange(0,20);
                        this[id].Reverse();
                    }                
                }

                public List<string> get(IMyTextPanel Panel)
                {
                    long id = Panel.EntityId;
                    if (!this.ContainsKey(id))
                    {
                        this.Add(id, new List<string>());
                    }

                    return this[id];
                }
            }

            public class Command
            {
                private BaconDebug debug;
                private IMyGridTerminalSystem GridTerminalSystem;
                private string _name = null;
                private StringBuilder _help = new StringBuilder();

                public string name { get { return _name; } }
                public StringBuilder help { get { return _help; } set { _help = value; } }

                public Command(string name, IMyGridTerminalSystem GridTerminalSystem, BaconDebug debug)
                {
                    this.debug = debug;
                    this.GridTerminalSystem = GridTerminalSystem;
                    this._name = name;
                    _help.Append("Help for '" + name + "'");
                    _help.Append("  Useage: " + name + " [argument]");
                }

                public bool isCommand(string v)
                {
                    return name.Equals(v.Trim());
                }
                
                public bool run(BaconArgs Args, out string result)
                {
                    debug.newScope("run");
                    result = null;
                    bool success = false;
                    List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
                    StringBuilder slug = new StringBuilder();
                    for (int i_Arguments = 0; i_Arguments < Args.getArguments().Count; i_Arguments++)
                    {
                        Blocks.Clear();
                        GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(Blocks, (x => x.CustomName.Contains(Args.getArguments()[i_Arguments])));
                        debug.add("found " + Blocks.Count.ToString() + " named " + Args.getArguments()[i_Arguments], BaconDebug.INFO);
                        for (int b = 0; b < Blocks.Count; b++)
                        {
                            string blockNameId = Blocks[b].CustomName + " [" + Blocks[b].EntityId.ToString() + "]";
                            slug.AppendLine(blockNameId);
                            if (Args.getOption("line").Count > 0)
                            {
                                DetailedInfo Info = new DetailedInfo(Blocks[b]);
                                for (int o = 0; o < Args.getOption("line").Count; o++)
                                {
                                    int line = 0;
                                    if (int.TryParse(Args.getOption("line")[o], out line))
                                    {
                                        DetailedInfo.DetailedInfoValue Value = Info.getValue(line);
                                        if (Value != null)
                                        {
                                            slug.AppendLine(Value.ToString());
                                            success = true;
                                        }
                                        else
                                        {
                                            debug.add("no value in line " + line.ToString() + " for " + blockNameId, BaconDebug.WARN);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                success = true;
                                slug.AppendLine(Blocks[b].DetailedInfo);
                            }
                        }
                    }
                    result = slug.ToString();
                    debug.leaveScope();
                    return success;
                }
                                
            }
        }

        public class BaconDebug { public const int INFO = 3; public const int WARN = 2; public const int ERROR = 1; public const int DEBUG = 4; List<IMyTextPanel> h = new List<IMyTextPanel>(); MyGridProgram i; List<string> j = new List<string>(); int k = 0; bool l = true; public int remainingInstructions { get { return i.Runtime.MaxInstructionCount - i.Runtime.CurrentInstructionCount; } } public bool autoscroll { get { return l; } set { l = value; } } public void clearPanels() { for (int a = 0; a < h.Count; a++) h[a].WritePublicText(""); } public BaconDebug(string a, IMyGridTerminalSystem b, MyGridProgram c, int d) { this.k = d; var e = new List<IMyTerminalBlock>(); b.GetBlocksOfType<IMyTextPanel>(e, ((IMyTerminalBlock f) => f.CustomName.Contains(a) && f.CubeGrid.Equals(c.Me.CubeGrid))); h = e.ConvertAll<IMyTextPanel>(f => f as IMyTextPanel); this.i = c; newScope("BaconDebug"); } public int getVerbosity() { return k; } public MyGridProgram getGridProgram() { return this.i; } public void newScope(string a) { j.Add(a); } public void leaveScope() { if (j.Count > 1) j.RemoveAt(j.Count - 1); } public string getSender() { return j[j.Count - 1]; } public void add(string a, int b) { if (b <= this.k) { var c = n(a); if (b == ERROR) i.Echo(c); for (int d = 0; d < h.Count; d++) if (autoscroll) { List<string> e = new List<string>(); e.AddRange(h[d].GetPublicText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)); StringBuilder f = new StringBuilder(); e.Add(c); if (!h[d].GetPrivateTitle().ToLower().Equals("nolinelimit")) { int g = m(h[d]); if (e.Count > g) { e.RemoveRange(0, e.Count - g); } } h[d].WritePublicText(string.Join("\n", e)); } else { h[d].WritePublicText(c + '\n', true); } } } int m(IMyTextPanel a) { float b = a.GetValueFloat("FontSize"); if (b == 0.0f) b = 0.01f; return Convert.ToInt32(Math.Ceiling(17.0f / b)); } string n(string a) { var b = new StringBuilder(); b.Append("[" + DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToShortTimeString() + "." + DateTime.Now.Millisecond.ToString().TrimStart('0') + "]"); b.Append("[" + getSender() + "]"); b.Append("[IC " + i.Runtime.CurrentInstructionCount + "/" + i.Runtime.MaxInstructionCount + "]"); b.Append(" " + a); return b.ToString(); } }
        public class BaconArgs { static public BaconArgs parse(string a) { return (new Parser()).parseArgs(a); } public class Parser { static Dictionary<string, BaconArgs> h = new Dictionary<string, BaconArgs>(); public BaconArgs parseArgs(string a) { if (!h.ContainsKey(a)) { var b = new BaconArgs(); var c = false; var d = false; var e = new StringBuilder(); for (int f = 0; f < a.Length; f++) { var g = a[f]; if (c) { e.Append(g); c = false; } else if (g.Equals('\\')) c = true; else if (d && !g.Equals('"')) e.Append(g); else if (g.Equals('"')) d = !d; else if (g.Equals(' ')) { b.add(e.ToString()); e.Clear(); } else e.Append(g); } if (e.Length > 0) b.add(e.ToString()); h.Add(a, b); } return h[a]; } } protected Dictionary<char, int> h = new Dictionary<char, int>(); protected List<string> i = new List<string>(); protected Dictionary<string, List<string>> j = new Dictionary<string, List<string>>(); public List<string> getArguments() { return i; } public int getFlag(char a) { return h.ContainsKey(a) ? h[a] : 0; } public List<string> getOption(string a) { return j.ContainsKey(a) ? j[a] : new List<string>(); } public void add(string a) { if (!a.StartsWith("-")) i.Add(a); else if (a.StartsWith("--")) { KeyValuePair<string, string> b = k(a); var c = b.Key.Substring(2); if (!j.ContainsKey(c)) j.Add(c, new List<string>()); j[c].Add(b.Value); } else { var b = a.Substring(1); for (int d = 0; d < b.Length; d++) if (this.h.ContainsKey(b[d])) { this.h[b[d]]++; } else { this.h.Add(b[d], 1); } } } KeyValuePair<string, string> k(string a) { string[] b = a.Split(new char[] { '=' }, 2); return new KeyValuePair<string, string>(b[0], (b.Length > 1) ? b[1] : null); } override public string ToString() { var a = new List<string>(); foreach (string key in j.Keys) a.Add(l(key) + ":[" + string.Join(",", j[key].ConvertAll<string>(b => l(b)).ToArray()) + "]"); var c = new List<string>(); foreach (char key in h.Keys) c.Add(key + ":" + h[key].ToString()); var d = new StringBuilder(); d.Append("{\"a\":["); d.Append(string.Join(",", i.ConvertAll<string>(b => l(b)).ToArray())); d.Append("],\"o\":[{"); d.Append(string.Join("},{", a)); d.Append("}],\"f\":[{"); d.Append(string.Join("},{", c)); d.Append("}]}"); return d.ToString(); } string l(string a) { return (a != null) ? "\"" + a.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" : @"null"; } }
        public class DetailedInfo { private List<DetailedInfoValue> s = new List<DetailedInfoValue>(); public DetailedInfo(IMyTerminalBlock B) { string[] I = B.DetailedInfo.Split(new string[] { "\r\n", "\n\r", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries); for (int i = 0; i < I.Length; i++) { List<string> d = new List<string>(); d.AddRange(I[i].Split(':')); if (d.Count > 1) { s.Add(new DetailedInfoValue(d[0], String.Join(":", d.GetRange(1, d.Count - 1)))); } } } public DetailedInfoValue getValue(int i) { return (i < s.Count && i > -1) ? s[i] : null; } public class DetailedInfoValue { public string k; public string v; public DetailedInfoValue(string k, string v) { this.k = k; this.v = v; } } }

        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}