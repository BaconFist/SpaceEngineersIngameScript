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

namespace CruiseControlRefactored
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        CruiseControlRefactored
        ==============
        Copyright 2017 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE

        Description
        ===========

        */

        /// <summary>
        /// Available options for arguments.
        /// To be used as scritp arguments like --KEY or --KEY="VALUE".
        /// Structure: {"Key","example;value;default;Description"}
        /// </summary>
        Dictionary<string, string> AvailableOptions = new Dictionary<string, string>() {
            {"SetController","--{0}=\"VALUE\";any string or AUTO;AUTO;Defines the Cockpit/RC to be used. AUTO: use the first piloted controller. any other VALUE: use Cockpit/RC with VALUE in it's name (MUST be exactly ONE)."},
            {"DeadZone","--{0}=\"NUMBER\";decimal number;0.0;[DEPRECATED] see: ErrorMargin"},
            {"ErrorMargin","--{0}=\"NUMBER\";decimal number;0.0;this is how close the velocity must be to the set velocity."},
            {"inc","--{0}=\"NUMBER\";decimal number;N/A;add this to target speed."},
            {"dec","--{0}=\"NUMBER\";decimal number;N/A;remove this from target speed."},
            {"LogLCD","--{0}=\"TAG\";any string;[BCC-LOG];write Logfile to any LCD with TAG in name."},
            {"LogLevel","--{0}=\"LEVEL\";one of: trace,debug,info,warn,error,fatal or \"OFF\" to diable;error;the verbosity of the log. (trace includes debug includes info includes warn inculdes error includes fatal)"},
        };

        BaconCruiseControl CruiseControl;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10 | UpdateFrequency.Update100;
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

        public void Main(string argument, UpdateType updateSource)
        {
            Environment Env = new Environment(this, argument, updateSource);
            if (CruiseControl == null)
            {
                CruiseControl = new BaconCruiseControl(Env);
            } else
            {
                CruiseControl.Env = Env;
            }

            
        }
        
        class BaconCruiseControl
        {
            private IMyShipController RefecenceController;
            public Environment Env { get; set; }

            public BaconCruiseControl(Environment Env)
            {
                Env.Log?.PushStack("BaconCruiseControl");
                this.Env = Env;
                Env.Log?.PopStack();
            }

            private void UpdateControllerFromArgument()
            {
                Env.Log?.PushStack("UpdateControllerFromArgument");
                RefecenceController = null;
                Env.Log?.IfDebug?.Debug("D1");
                string optionKey = "SetController";
                string findBy = Env.GlobalArgs.hasOption(optionKey) && (Env.GlobalArgs.getOption(optionKey).Count > 0) ? Env.GlobalArgs.getOption(optionKey)[0] : "AUTO";
                switch (findBy)                {
                    case "AUTO":
                        UpdateControlerByAutodetect();
                        break;
                    default:
                        UpdateControllerByTag(findBy);
                        break;
                }
                Env.Log?.PopStack();
            }

            private void UpdateControlerByAutodetect()
            {
                Env.Log?.PushStack("UpdateControlerByAutodetect");

                Env.Log?.PopStack();
            }

            private void UpdateControllerByTag(string tag)
            {
                Env.Log?.PushStack("UpdateControllerByTag");

                List<IMyShipController> ControllerBag = new List<IMyShipController>();
                Env.App.GridTerminalSystem.GetBlocksOfType<IMyShipController>(ControllerBag, (c => c.CubeGrid.Equals(Env.App.Me.CubeGrid) && c.CustomName.Contains(tag)));
                if(ControllerBag.Count == 0)
                {
                    Env.Log?.IfError?.Error("F1",tag,Env.App.Me.CubeGrid.CustomName);
                    Env.Log?.PopStack();
                    return;
                }
                if(ControllerBag.Count > 1)
                {
                    Env.Log?.IfWarn.Warn("W1",tag,ControllerBag.Count,Env.App.Me.CubeGrid.CustomName, ControllerBag[ControllerBag.Count-1].CustomName);
                }
                RefecenceController = ControllerBag[0];
                Env.Log?.PopStack();
            }
        }


        

 


        /// <summary>
        ///     Generate help text for script arguments
        /// </summary>
        /// <param name="Env">Environment</param>
        /// <returns>Environment</returns>
        private StringBuilder GetHelpText(Environment Env)
        {
            Env.Log?.PushStack("GetHelpText");
            StringBuilder slug = new StringBuilder();
            foreach(KeyValuePair<string,string> option in AvailableOptions)
            {
                string[] details = option.Value.Split(';');
                if (details.Length == 4)
                {
                    slug.AppendFormat(
                          "OPTION:      {0}\n"
                        + "-USEAGE:      {1}\n"
                        + "-VALUE:       {2}\n"
                        + "-DEFAULT:     {3}\n"
                        + "-DESCRIPTION: {4}\n",
                        option.Key,
                        details[0],
                        details[1],
                        details[2],
                        details[3]
                    );
                }
                else { 
                    Env.Log?.IfError?.Error("E1", option.Key);
                    slug.AppendFormat(
                          "OPTION:      {0}\n"
                        + "-DETAILS: {1}\n",
                        option.Key, 
                        option.Value
                    );
                }                                    
            }
            Env.Log?.PopStack();
            return slug;
        }

        class Environment
        {
            public Program App { get; }
            public string argumentRaw { get; }
            public BaconArgs GlobalArgs { get; }
            public UpdateType updateSource { get; }
            public BMyLog4PB Log { get; }
            private Dictionary<string, string> LogMessages = new Dictionary<string, string>() {
                {"E1","Can't render Option {0} -> wrong number of arguments"},
                {"F1","no Cockpit or Remote with \"{0}\" in it's Name found on Grid \"{1}\""},
                {"W1","there are {1} Controllers with \"{0}\" in it's name on Grid \"{2}\". Using first one \"{3}\""},
                {"D1","disabled referemce Controller"},
                {"I1","Log Started, Environment initialized."}
            };

            public Environment(Program App, string argument, UpdateType updateSource)
            {
                this.App = App;
                this.argumentRaw = argument;
                this.GlobalArgs = BaconArgs.parse(argument);
                this.updateSource = updateSource;
                this.Log = NewLog();                
            }

            private BMyLog4PB NewLog()
            {
                if(App == null || GlobalArgs == null)
                {
                    return null;
                }

                BMyLog4PB TempLogger;
                string verbosity = GlobalArgs.getOption("LogLevel")[0]?.Trim() ?? "error";
                if (verbosity.Equals("OFF"))
                {
                    TempLogger = null;
                }
                else
                {
                    TempLogger = new BMyLog4PB(this.App);
                    switch (verbosity)
                    {
                        case "trace":
                            TempLogger.Filter = BMyLog4PB.E_TRACE | BMyLog4PB.E_DEBUG | BMyLog4PB.E_INFO | BMyLog4PB.E_WARN | BMyLog4PB.E_ERROR | BMyLog4PB.E_FATAL;
                            break;
                        case "debug":
                            TempLogger.Filter = BMyLog4PB.E_DEBUG | BMyLog4PB.E_INFO | BMyLog4PB.E_WARN | BMyLog4PB.E_ERROR | BMyLog4PB.E_FATAL;
                            break;
                        case "info":
                            TempLogger.Filter = BMyLog4PB.E_INFO | BMyLog4PB.E_WARN | BMyLog4PB.E_ERROR | BMyLog4PB.E_FATAL;
                            break;
                        case "warn":
                            TempLogger.Filter = BMyLog4PB.E_WARN | BMyLog4PB.E_ERROR | BMyLog4PB.E_FATAL;
                            break;
                        case "error":
                            TempLogger.Filter = BMyLog4PB.E_ERROR | BMyLog4PB.E_FATAL;
                            break;
                        case "fatal":
                            TempLogger.Filter = BMyLog4PB.E_FATAL;
                            break;
                        default:
                            TempLogger.Filter = BMyLog4PB.E_ERROR | BMyLog4PB.E_FATAL;
                            break;
                    }
                    foreach (KeyValuePair<string, string> logMsg in LogMessages)
                    {
                        TempLogger.SetMessage(logMsg.Key, logMsg.Value);
                    }

                    TempLogger.AutoFlush = true;
                    TempLogger.AddAppender(new BMyLog4PB.BMyTextPanelAppender(GlobalArgs.getOption("LogLCD")[0]?.Trim() ?? "[BCC-LOG]", App));
                    TempLogger.Info("L1");
                }
                return TempLogger;
            }
            
        }

        #region includes
        public class BaconArgs { public string Raw; static public BaconArgs parse(string a) { return (new Parser()).parseArgs(a); } static public string Escape(object a) { return string.Format("{0}", a).Replace(@"\", @"\\").Replace(@" ", @"\ ").Replace(@"""", @"\"""); } static public string UnEscape(string a) { return a.Replace(@"\""", @"""").Replace(@"\ ", @" ").Replace(@"\\", @"\"); } public class Parser { static Dictionary<string, BaconArgs> h = new Dictionary<string, BaconArgs>(); public BaconArgs parseArgs(string a) { if (!h.ContainsKey(a)) { var b = new BaconArgs(); b.Raw = a; var c = false; var d = false; var e = new StringBuilder(); for (int f = 0; f < a.Length; f++) { var g = a[f]; if (c) { e.Append(g); c = false; } else if (g.Equals('\\')) c = true; else if (d && !g.Equals('"')) e.Append(g); else if (g.Equals('"')) d = !d; else if (g.Equals(' ')) if (e.ToString().Equals("--")) { b.add(a.Substring(f).TrimStart()); e.Clear(); break; } else { b.add(e.ToString()); e.Clear(); } else e.Append(g); } if (e.Length > 0) b.add(e.ToString()); h.Add(a, b); } return h[a]; } } protected Dictionary<char, int> h = new Dictionary<char, int>(); protected List<string> i = new List<string>(); protected Dictionary<string, List<string>> j = new Dictionary<string, List<string>>(); public List<string> getArguments() { return i; } public int getFlag(char a) { return h.ContainsKey(a) ? h[a] : 0; } public bool hasArguments() { return i.Count > 0; } public bool hasOption(string a) { return j.ContainsKey(a); } public List<string> getOption(string a) { return j.ContainsKey(a) ? j[a] : new List<string>(); } public void add(string a) { if (a.Trim().Length > 0) if (!a.StartsWith("-")) { i.Add(a); } else if (a.StartsWith("--")) { KeyValuePair<string, string> b = k(a); string c = b.Key.Substring(2); if (!j.ContainsKey(c)) { j.Add(c, new List<string>()); } j[c].Add(b.Value); } else { string b = a.Substring(1); for (int d = 0; d < b.Length; d++) { if (this.h.ContainsKey(b[d])) { this.h[b[d]]++; } else { this.h.Add(b[d], 1); } } } } KeyValuePair<string, string> k(string a) { string[] b = a.Split(new char[] { '=' }, 2); return new KeyValuePair<string, string>(b[0], (b.Length > 1) ? b[1] : null); } public string ToArguments() { var a = new List<string>(); foreach (string argument in this.getArguments()) a.Add(Escape(argument)); foreach (KeyValuePair<string, List<string>> option in this.j) { var b = "--" + Escape(option.Key); foreach (string optVal in option.Value) a.Add(b + ((optVal != null) ? "=" + Escape(optVal) : "")); } var c = (h.Count > 0) ? "-" : ""; foreach (KeyValuePair<char, int> flag in h) c += new String(flag.Key, flag.Value); a.Add(c); return String.Join(" ", a.ToArray()); } override public string ToString() { var a = new List<string>(); foreach (string key in j.Keys) a.Add(l(key) + ":[" + string.Join(",", j[key].ConvertAll<string>(b => l(b)).ToArray()) + "]"); var c = new List<string>(); foreach (char key in h.Keys) c.Add(key + ":" + h[key].ToString()); var d = new StringBuilder(); d.Append("{\"a\":["); d.Append(string.Join(",", i.ConvertAll<string>(b => l(b)).ToArray())); d.Append("],\"o\":[{"); d.Append(string.Join("},{", a)); d.Append("}],\"f\":[{"); d.Append(string.Join("},{", c)); d.Append("}]}"); return d.ToString(); } string l(string a) { return (a != null) ? "\"" + a.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" : @"null"; } }
        public class BMyLog4PB { public const byte E_ALL = 63; public const byte E_TRACE = 32; public const byte E_DEBUG = 16; public const byte E_INFO = 8; public const byte E_WARN = 4; public const byte E_ERROR = 2; public const byte E_FATAL = 1; public BMyLog4PB IfFatal { get { return ((E_FATAL & Filter) != 0) ? this : null; } } public BMyLog4PB IfError { get { return ((E_ERROR & Filter) != 0) ? this : null; } } public BMyLog4PB IfWarn { get { return ((E_WARN & Filter) != 0) ? this : null; } } public BMyLog4PB IfInfo { get { return ((E_INFO & Filter) != 0) ? this : null; } } public BMyLog4PB IfDebug { get { return ((E_DEBUG & Filter) != 0) ? this : null; } } public BMyLog4PB IfTrace { get { return ((E_TRACE & Filter) != 0) ? this : null; } } Dictionary<string, string> k = new Dictionary<string, string>(); Dictionary<string, string> l = new Dictionary<string, string>() { { "{Date}", "{0}" }, { "{Time}", "{1}" }, { "{Milliseconds}", "{2}" }, { "{Severity}", "{3}" }, { "{CurrentInstructionCount}", "{4}" }, { "{MaxInstructionCount}", "{5}" }, { "{Message}", "{6}" }, { "{Stack}", "{7}" }, { "{Origin}", "{8}" } }; Stack<string> m = new Stack<string>(); public byte Filter; public readonly Dictionary<BMyAppenderBase, string> Appenders = new Dictionary<BMyAppenderBase, string>(); string n = @"[{0}-{1}/{2}][{3}][{4}/{5}][{8}][{7}] {6}"; string o = @"[{Date}-{Time}/{Milliseconds}][{Severity}][{CurrentInstructionCount}/{MaxInstructionCount}][{Origin}][{Stack}] {Message}"; public string Format { get { return o; } set { n = r(value); o = value; } } readonly Program p; public bool AutoFlush = true; public BMyLog4PB(Program a) : this(a, E_FATAL | E_ERROR | E_WARN | E_INFO, new BMyEchoAppender(a)) { } public BMyLog4PB(Program a, byte b, params BMyAppenderBase[] c) { Filter = b; this.p = a; foreach (var Appender in c) AddAppender(Appender); } public BMyLog4PB SetMessage(string a, string b) { if (!k.ContainsKey(a)) k.Add(a, b); else k[a] = b; return this; } string q(string a) { return k.ContainsKey(a) ? k[a] : a; } string r(string a) { var b = a; foreach (var item in l) b = (b != null) ? b.Replace(item.Key, item.Value) : null; return b; } public BMyLog4PB Flush() { foreach (var AppenderItem in Appenders) AppenderItem.Key.Flush(); return this; } public BMyLog4PB PushStack(string a) { m.Push(a); return this; } public string PopStack() { return (m.Count > 0) ? m.Pop() : null; } string s() { return (m.Count > 0) ? m.Peek() : null; } public string StackToString() { if (IfTrace != null) { string[] a = m.ToArray(); Array.Reverse(a); return string.Join(@"/", a); } else return s(); } public BMyLog4PB AddAppender(BMyAppenderBase a, string b = null) { if (!Appenders.ContainsKey(a)) Appenders.Add(a, r(b)); return this; } public BMyLog4PB If(byte a) { return ((a & Filter) != 0) ? this : null; } public BMyLog4PB Fatal(string a, params object[] b) { if (IfFatal != null) t("FATAL", a, b); return this; } public BMyLog4PB Error(string a, params object[] b) { if (IfError != null) t("ERROR", a, b); return this; } public BMyLog4PB Warn(string a, params object[] b) { if (IfWarn != null) t("WARN", a, b); return this; } public BMyLog4PB Info(string a, params object[] b) { if (IfInfo != null) t("INFO", a, b); return this; } public BMyLog4PB Debug(string a, params object[] b) { if (IfDebug != null) t("DEBUG", a, b); return this; } public BMyLog4PB Trace(string a, params object[] b) { if (IfTrace != null) t("TRACE", a, b); return this; } void t(string a, string b, params object[] c) { DateTime d = DateTime.Now; b = q(b); string f; try { f = string.Format(b, c); } catch (FormatException e) { f = b + " [BMYLOG4PB FATAL]: " + e.Message; } u g = new u(d.ToShortDateString(), d.ToLongTimeString(), d.Millisecond.ToString(), a, p.Runtime.CurrentInstructionCount, p.Runtime.MaxInstructionCount, f, StackToString(), p.Me.CustomName); foreach (var item in Appenders) { var h = (item.Value != null) ? item.Value : n; item.Key.Enqueue(g.ToString(h)); if (AutoFlush) item.Key.Flush(); } } class u { public string Date; public string Time; public string Milliseconds; public string Severity; public int CurrentInstructionCount; public int MaxInstructionCount; public string Message; public string Stack; public string Origin; public u(string a, string b, string c, string d, int f, int g, string h, string i, string j) { this.Date = a; this.Time = b; this.Milliseconds = c; this.Severity = d; this.CurrentInstructionCount = f; this.MaxInstructionCount = g; this.Message = h; this.Stack = i; this.Origin = j; } public override string ToString() { return ToString(@"{0},{1},{2},{3},{4},{5},{6},{7},{8}"); } public string ToString(string a) { return string.Format(a, Date, Time, Milliseconds, Severity, CurrentInstructionCount, MaxInstructionCount, Message, Stack, Origin); } } public class BMyTextPanelAppender : BMyAppenderBase { List<string> k = new List<string>(); List<IMyTextPanel> l = new List<IMyTextPanel>(); public bool Autoscroll = true; public bool Prepend = false; public BMyTextPanelAppender(string a, Program b) { b.GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(l, (c => c.CustomName.Contains(a))); } public override void Enqueue(string a) { k.Add(a); } public override void Flush() { foreach (var Panel in l) { m(Panel); Panel.ShowTextureOnScreen(); Panel.ShowPublicTextOnScreen(); } k.Clear(); } void m(IMyTextPanel a) { if (Autoscroll) { var b = new List<string>(a.GetPublicText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)); b.AddRange(k); int c = Math.Min(n(a), b.Count); if (Prepend) b.Reverse(); a.WritePublicText(string.Join("\n", b.GetRange(b.Count - c, c).ToArray()), false); } else if (Prepend) { var b = new List<string>(a.GetPublicText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)); b.AddRange(k); b.Reverse(); a.WritePublicText(string.Join("\n", b.ToArray()), false); } else { a.WritePublicText(string.Join("\n", k.ToArray()), true); } } int n(IMyTextPanel a) { float b = a.GetValueFloat("FontSize"); if (b == 0.0f) b = 0.01f; return Convert.ToInt32(Math.Ceiling(17.0f / b)); } } public class BMyKryptDebugSrvAppender : BMyAppenderBase { IMyProgrammableBlock k; Queue<string> l = new Queue<string>(); public BMyKryptDebugSrvAppender(Program a) { k = a.GridTerminalSystem.GetBlockWithName("DebugSrv") as IMyProgrammableBlock; } public override void Flush() { if (k != null) { var a = true; while (a && l.Count > 0) if (k.TryRun("L" + l.Peek())) { l.Dequeue(); } else { a = false; } } } public override void Enqueue(string a) { l.Enqueue(a); } } public class BMyEchoAppender : BMyAppenderBase { Program k; public BMyEchoAppender(Program a) { this.k = a; } public override void Flush() { } public override void Enqueue(string a) { k.Echo(a); } } public class BMyCustomDataAppender : BMyAppenderBase { Program k; public BMyCustomDataAppender(Program a) { this.k = a; this.k.Me.CustomData = ""; } public override void Enqueue(string a) { k.Me.CustomData = k.Me.CustomData + '\n' + a; } public override void Flush() { } } public abstract class BMyAppenderBase { public abstract void Enqueue(string a); public abstract void Flush(); } }
        #endregion includes

        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}