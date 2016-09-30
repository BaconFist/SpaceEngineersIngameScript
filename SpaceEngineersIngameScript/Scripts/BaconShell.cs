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

namespace BaconShell
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        BaconShell
        ==============
        Copyright 2016 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE

        Description
        ===========

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
            List<IMyTextPanel> TTYs = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(TTYs, (IMyTextPanel x) => x.CustomName.Contains("TTY") && x.CubeGrid.Equals(Me.CubeGrid));
            BaconShell.Environment env = new BaconShell.Environment(this, GridTerminalSystem, new BaconDebug("debug", GridTerminalSystem, this, 99));
            BaconShell bs = new BaconShell(env);

            for (int i_TTY = 0; i_TTY < TTYs.Count; i_TTY++)
            {
                IMyTextPanel TTY = TTYs[i_TTY];
                string stdin = TTY.GetPrivateText();
                if (!stdin.Equals(""))
                {
                    StringBuilder stdout = new StringBuilder();
                    string[] command = stdin.Split(new Char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    for(int i = 0; i < command.Length; i++)
                    {
                        BaconArgs Args = BaconArgs.parse(command[i]);
                        stdout.Append(bs.runCommand(Args, env).ToString());
                    }
                    TTY.WritePrivateText("");
                    TTY.WritePublicText(stdout.ToString());
                }     
            }
        }

        public class BaconShell
        {
            private Dictionary<string, Command> availableCommands = new Dictionary<string, Command>();
            
            public BaconShell(Environment env)
            {
                initCommands(env);
            }

            public StringBuilder runCommand(BaconArgs Args, Environment Env)
            {
                Env.Debug.newScope("runCommand");
                StringBuilder response = new StringBuilder();
                if(Args.getArguments().Count > 0)
                {
                    string cmd = Args.getArguments()[0];
                    if (availableCommands.ContainsKey(cmd))
                    {
                        Args.getArguments().RemoveAt(0);
                        StringBuilder _tmp = new StringBuilder();
                        if (!availableCommands[cmd].exec(Args, Env, out _tmp))
                        {
                            response.AppendLine("an error occured during executing of '" + cmd + "'");
                        }
                        response.Append(_tmp.ToString());
                    } else
                    {
                        Env.Debug.add("unknown command: " + cmd, BaconDebug.DEBUG);
                        response.AppendLine("unknown command: " + cmd);
                    }
                } else
                {
                    Env.Debug.add("no command given", BaconDebug.DEBUG);
                }

                Env.Debug.leaveScope();
                return response;
            }

            private void initCommands(Environment env)
            {
                env.Debug.newScope("initCommands");

                //begin info command
                availableCommands.Add("info", new Command("info", "show short info of a command", env, (BaconArgs Args, Environment Env) => {
                    StringBuilder result = new StringBuilder();
                    if(Args.getArguments().Count > 0)
                    {
                        for(int i = 0; i < Args.getArguments().Count; i++)
                        {
                            string cmd = Args.getArguments()[i];
                            if (availableCommands.ContainsKey(cmd))
                            {
                                result.AppendLine(availableCommands[cmd].name + ": " + availableCommands[cmd].info);
                            }
                            else
                            {
                                result.AppendLine("unknown command: " + cmd);
                            }
                        }
                    } else
                    {
                        List<Command> commands = new List<Command>(availableCommands.Values);
                        for(int i = 0; i < commands.Count; i++)
                        {
                            result.AppendLine(commands[i].name + ": " + commands[i].info);
                        }
                    }
                    return result;
                }));
                availableCommands["info"].help.AppendLine("info [command [...]]");
                availableCommands["info"].help.AppendLine(availableCommands["info"].info);
                availableCommands["info"].help.AppendLine("no argument: info for all available commands");
                availableCommands["info"].help.AppendLine("give one ore more arguments to display info for these");
                //end info command
                //begin help command
                availableCommands.Add("help", new Command("help", "show help of a command", env, (BaconArgs Args, Environment Env) => {
                    StringBuilder result = new StringBuilder();
                    for(int i = 0; i < Args.getArguments().Count; i++)
                    {
                        if (availableCommands.ContainsKey(Args.getArguments()[i]))
                        {
                            result.Append(availableCommands[Args.getArguments()[i]].help.ToString());
                        }
                    }
                    return result;
                }));
                availableCommands["help"].help.AppendLine("help command [...]");
                availableCommands["help"].help.AppendLine(availableCommands["help"].info);
                availableCommands["help"].help.AppendLine("shows help for the given command(s)");
                //end help commnd
                //begin details command
                availableCommands.Add("details", 
                    new Command("details", "show detailed info of a block", env, (BaconArgs Args, Environment Env) => {
                        StringBuilder result = new StringBuilder();
                        for(int i_arg = 0; i_arg < Args.getArguments().Count; i_arg++)
                        {
                            string tag = Args.getArguments()[i_arg];
                            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
                            Env.GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(Blocks, (IMyTerminalBlock x) => x.CustomName.Contains(tag) && (!(Args.getFlag('t') == 0) || x.CubeGrid.Equals(Env.GridProgram.Me.CubeGrid)));
                            for(int i_blocks = 0; i_blocks < Blocks.Count; i_blocks++)
                            {
                                if (Args.getFlag('b') > 0)
                                {
                                    result.AppendLine(Blocks[i_blocks].CustomName);
                                }
                                if (Args.getOption("line").Count > 0)
                                {
                                    DetailedInfo info = new DetailedInfo(Blocks[i_blocks]);
                                    for (int i = 0; i < Args.getOption("line").Count; i++)
                                    {
                                        int line = 0;
                                        if(int.TryParse(Args.getOption("line")[i], out line))
                                        {
                                            DetailedInfo.DetailedInfoValue data = info.getValue(line);
                                            result.AppendLine((data!=null)?data.k + ": " + data.v:"null");
                                        }
                                    }
                                } else
                                {
                                    result.AppendLine(Blocks[i_blocks].DetailedInfo);
                                }
                            }
                        }
                        return result;
                    })
                );
                availableCommands["details"].help.AppendLine("details tag [tag [...]] [--line=N [...]] [-t] [-b]");
                availableCommands["details"].help.AppendLine(availableCommands["details"].info);
                availableCommands["details"].help.AppendLine("tag: filter block by a tag in it's name");
                availableCommands["details"].help.AppendLine("--line=N: show only detailed info from line N");
                availableCommands["details"].help.AppendLine("-t: find only blocks of same grid");
                availableCommands["details"].help.AppendLine("-b: add the Blockname on top of the info");
                //end details command

                env.Debug.leaveScope();
            }

            public class Environment
            {
                public MyGridProgram GridProgram;
                public IMyGridTerminalSystem GridTerminalSystem;
                public BaconDebug Debug;

                public Environment(MyGridProgram GridProgram, IMyGridTerminalSystem GridTerminalSystem, BaconDebug Debug)
                {
                    this.GridProgram = GridProgram;
                    this.GridTerminalSystem = GridTerminalSystem;
                    this.Debug = Debug;
                }               
            }

            public class Command
            {
                private string _name = "";
                private string _info = "";
                private StringBuilder _help = new StringBuilder();
                private Func<BaconArgs, Environment, StringBuilder> func;

                public string name { get { return _name; } }
                public string info { get { return _info; } }
                public StringBuilder help { get { return _help; } }

                public Command(string name, string info, Environment Environment, Func<BaconArgs, Environment, StringBuilder> function)
                {
                    Environment.Debug.newScope("BaconShell.Command.Command");
                    Environment.Debug.add("prepare command '" + name + "' (" + info + ")", BaconDebug.DEBUG);
                    this._name = name;
                    this._info = info;
                    this.func = function;
                    Environment.Debug.leaveScope();
                }

                public bool exec(BaconArgs Args, Environment Environment, out StringBuilder Response)
                {
                    Environment.Debug.newScope("BaconShell.Command.exec");
                    Environment.Debug.add("exec command '"+name+"'", BaconDebug.DEBUG);
                    Response = new StringBuilder();
                    bool succeed = false;
                    try
                    {
                        Environment.Debug.newScope("dynamic command '" + name + "'");
                        Response = this.func(Args, Environment);
                        Environment.Debug.leaveScope();
                        succeed = true;
                    }
                    catch (Exception e)
                    {
                        succeed = false;
                        Response.Clear();
                        Response.AppendLine(e.ToString());
                    }

                    Environment.Debug.add("response length: " + Response.Length.ToString(), BaconDebug.DEBUG);
                    Environment.Debug.leaveScope();
                    return succeed;
                }
            }
        }

        class DetailedInfo { private List<DetailedInfoValue> s = new List<DetailedInfoValue>(); public DetailedInfo(IMyTerminalBlock B) { string[] I = B.DetailedInfo.Split(new string[] { "\r\n", "\n\r", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries); for (int i = 0; i < I.Length; i++) { List<string> d = new List<string>(); d.AddRange(I[i].Split(':')); if (d.Count > 1) { s.Add(new DetailedInfoValue(d[0], String.Join(":", d.GetRange(1, d.Count - 1)))); } } } public DetailedInfoValue getValue(int i) { return (i < s.Count && i > -1) ? s[i] : null; } public class DetailedInfoValue { public string k; public string v; public DetailedInfoValue(string k, string v) { this.k = k; this.v = v; } } }
        public class BaconArgs { static public BaconArgs parse(string a) { return (new Parser()).parseArgs(a); } public class Parser { static Dictionary<string, BaconArgs> h = new Dictionary<string, BaconArgs>(); public BaconArgs parseArgs(string a) { if (!h.ContainsKey(a)) { var b = new BaconArgs(); var c = false; var d = false; var e = new StringBuilder(); for (int f = 0; f < a.Length; f++) { var g = a[f]; if (c) { e.Append(g); c = false; } else if (g.Equals('\\')) c = true; else if (d && !g.Equals('"')) e.Append(g); else if (g.Equals('"')) d = !d; else if (g.Equals(' ')) { b.add(e.ToString()); e.Clear(); } else e.Append(g); } if (e.Length > 0) b.add(e.ToString()); h.Add(a, b); } return h[a]; } } protected Dictionary<char, int> h = new Dictionary<char, int>(); protected List<string> i = new List<string>(); protected Dictionary<string, List<string>> j = new Dictionary<string, List<string>>(); public List<string> getArguments() { return i; } public int getFlag(char a) { return h.ContainsKey(a) ? h[a] : 0; } public List<string> getOption(string a) { return j.ContainsKey(a) ? j[a] : new List<string>(); } public void add(string a) { if (!a.StartsWith("-")) i.Add(a); else if (a.StartsWith("--")) { KeyValuePair<string, string> b = k(a); var c = b.Key.Substring(2); if (!j.ContainsKey(c)) j.Add(c, new List<string>()); j[c].Add(b.Value); } else { var b = a.Substring(1); for (int d = 0; d < b.Length; d++) if (this.h.ContainsKey(b[d])) { this.h[b[d]]++; } else { this.h.Add(b[d], 1); } } } KeyValuePair<string, string> k(string a) { string[] b = a.Split(new char[] { '=' }, 2); return new KeyValuePair<string, string>(b[0], (b.Length > 1) ? b[1] : null); } override public string ToString() { var a = new List<string>(); foreach (string key in j.Keys) a.Add(l(key) + ":[" + string.Join(",", j[key].ConvertAll<string>(b => l(b)).ToArray()) + "]"); var c = new List<string>(); foreach (char key in h.Keys) c.Add(key + ":" + h[key].ToString()); var d = new StringBuilder(); d.Append("{\"a\":["); d.Append(string.Join(",", i.ConvertAll<string>(b => l(b)).ToArray())); d.Append("],\"o\":[{"); d.Append(string.Join("},{", a)); d.Append("}],\"f\":[{"); d.Append(string.Join("},{", c)); d.Append("}]}"); return d.ToString(); } string l(string a) { return (a != null) ? "\"" + a.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" : @"null"; } }
        public class BaconDebug { public const int INFO = 3; public const int WARN = 2; public const int ERROR = 1; public const int DEBUG = 4; List<IMyTextPanel> h = new List<IMyTextPanel>(); MyGridProgram i; List<string> j = new List<string>(); int k = 0; bool l = true; public int remainingInstructions { get { return i.Runtime.MaxInstructionCount - i.Runtime.CurrentInstructionCount; } } public bool autoscroll { get { return l; } set { l = value; } } public void clearPanels() { for (int a = 0; a < h.Count; a++) h[a].WritePublicText(""); } public BaconDebug(string a, IMyGridTerminalSystem b, MyGridProgram c, int d) { this.k = d; var e = new List<IMyTerminalBlock>(); b.GetBlocksOfType<IMyTextPanel>(e, ((IMyTerminalBlock f) => f.CustomName.Contains(a) && f.CubeGrid.Equals(c.Me.CubeGrid))); h = e.ConvertAll<IMyTextPanel>(f => f as IMyTextPanel); this.i = c; newScope("BaconDebug"); } public int getVerbosity() { return k; } public MyGridProgram getGridProgram() { return this.i; } public void newScope(string a) { j.Add(a); } public void leaveScope() { if (j.Count > 1) j.RemoveAt(j.Count - 1); } public string getSender() { return j[j.Count - 1]; } public void add(string a, int b) { if (b <= this.k) { var c = n(a); if (b == ERROR) i.Echo(c); for (int d = 0; d < h.Count; d++) if (autoscroll) { List<string> e = new List<string>(); e.AddRange(h[d].GetPublicText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)); StringBuilder f = new StringBuilder(); e.Add(c); if (!h[d].GetPrivateTitle().ToLower().Equals("nolinelimit")) { int g = m(h[d]); if (e.Count > g) { e.RemoveRange(0, e.Count - g); } } h[d].WritePublicText(string.Join("\n", e)); } else { h[d].WritePublicText(c + '\n', true); } } } int m(IMyTextPanel a) { float b = a.GetValueFloat("FontSize"); if (b == 0.0f) b = 0.01f; return Convert.ToInt32(Math.Ceiling(17.0f / b)); } string n(string a) { var b = new StringBuilder(); b.Append("[" + DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToShortTimeString() + "." + DateTime.Now.Millisecond.ToString().TrimStart('0') + "]"); b.Append("[" + getSender() + "]"); b.Append("[IC " + i.Runtime.CurrentInstructionCount + "/" + i.Runtime.MaxInstructionCount + "]"); b.Append(" " + a); return b.ToString(); } }
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}