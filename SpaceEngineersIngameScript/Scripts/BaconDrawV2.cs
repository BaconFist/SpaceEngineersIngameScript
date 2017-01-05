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

namespace BaconDrawV2
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        BaconDrawV2
        ==============
        Copyright 2016 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE

        Description
        ===========

        */

        public Program()
        {
            BaconDraw = new BMyBaconDraw();
        }

        public void Main(string argument)
        {
            BMyBaconDraw.BMyEnvironment Env = new BMyBaconDraw.BMyEnvironment(this, argument);
            try
            {
                Env.Log?.PushStack("Main");
                BaconDraw.Env = Env;
                Env.Log?.Debug("updated environment");
            }
            catch (Exception e)
            {
                Env.Log?.Fatal(e.Message);
            }
            finally
            {
                Env.Log?.Flush();
            }
        }

        BMyBaconDraw BaconDraw;
              
        class BMyBaconDraw
        {
            public BMyEnvironment Env;

            public class BMyEnvironment
            {
                public readonly Program Assembly;
                public readonly BMyLog4PB Log;
                public readonly BaconArgs ArgBag;

                public BMyEnvironment(Program Assembly, string arguments)
                {
                    this.Assembly = Assembly;
                    ArgBag = BaconArgs.parse(arguments);
                    Log = BMyLoggerFactory.getLogger(ArgBag, Assembly);
                }
            }
            class BMyLoggerFactory
            {
                const char FLAG_VERBOSITY = 'v';
                static public BMyLog4PB getLogger(BaconArgs ArgBag, Program Assembly)
                {
                    byte filter = getVerbosityFilter(ArgBag);
                    if (filter == 0)
                    {
                        return null;
                    }
                    string tag = ArgBag.hasOption(BMyArgParams.LOG_TAG) ? ArgBag.getOption(BMyArgParams.LOG_TAG)[0] : BMyArgParams.LOG_TAG_DEFAULT;
                    BMyLog4PB Logger = new BMyLog4PB(
                        Assembly,
                        filter,
                        new BMyLog4PB.BMyEchoAppender(Assembly),
                        new BMyLog4PB.BMyKryptDebugSrvAppender(Assembly),
                        new BMyLog4PB.BMyTextPanelAppender(
                            tag,
                            Assembly
                        )
                    );
                    if (ArgBag.hasOption(BMyArgParams.LOG_FORMAT))
                    {
                        Logger.Format = ArgBag.getOption(BMyArgParams.LOG_FORMAT)[0];
                    }
                    Logger.AutoFlush = false;
                    Logger.If(BMyLog4PB.E_DEBUG)?.Debug("Log initialized. Tag: {0}, Format: {1}", tag, Logger.Format);
                    return Logger;
                }
                static private byte getVerbosityFilter(BaconArgs ArgBag)
                {
                    byte slug = 0;
                    int verbosity = ArgBag.getFlag(BMyArgParams.LOG_VERBOSITY);
                    if (verbosity >= 1)
                    {
                        slug |= BMyLog4PB.E_FATAL;
                    }
                    if (verbosity >= 2)
                    {
                        slug |= BMyLog4PB.E_ERROR;
                    }
                    if (verbosity >= 3)
                    {
                        slug |= BMyLog4PB.E_WARN;
                    }
                    if (verbosity >= 4)
                    {
                        slug |= BMyLog4PB.E_INFO;
                    }
                    if (verbosity >= 5 && ArgBag.hasOption(BMyArgParams.LOG_ENABLEDEBUG))
                    {
                        slug |= BMyLog4PB.E_DEBUG;
                    }
                    if (verbosity >= 6 && ArgBag.hasOption(BMyArgParams.LOG_ENABLEDEBUG))
                    {
                        slug |= BMyLog4PB.E_TRACE;
                    }

                    return slug;
                }
            }
            class BMyArgParams
            {
                public const char LOG_VERBOSITY = 'v';
                public const string LOG_ENABLEDEBUG = "debug";
                public const string LOG_FORMAT = "logFormat";
                public const string LOG_TAG = "logTag";
                public const string LOG_TAG_DEFAULT = "[BaconDrawLog]";
            }



            class BMyBitmap
            {
                private char[,] _raster;
                public readonly int Width;
                public readonly int Height;

                public BMyBitmap(int width, int height)
                {
                    _raster = new char[width, height];
                    Width = _raster.GetUpperBound(0);
                    Height = _raster.GetUpperBound(1);
                }
                public bool isInBounds(int x, int y)
                {
                    return (0 <= x && x < Width && 0 <= y && y < Height);
                }
                public bool TryGetPixel(int x, int y, out char pixel)
                {
                    if (isInBounds(x, y))
                    {
                        pixel = _raster[x, y];
                        return true;
                    }
                    else
                    {
                        pixel = '\0';
                        return false;
                    }
                }
                public bool TrySetPixel(int x, int y, char pixel)
                {
                    if (isInBounds(x, y))
                    {
                        _raster[x, y] = pixel;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }


        #region libraries
        public class BMyLog4PB { public const byte E_ALL = 63; public const byte E_TRACE = 32; public const byte E_DEBUG = 16; public const byte E_INFO = 8; public const byte E_WARN = 4; public const byte E_ERROR = 2; public const byte E_FATAL = 1; Dictionary<string, string> i = new Dictionary<string, string>() { { "{Date}", "{0}" }, { "{Time}", "{1}" }, { "{Milliseconds}", "{2}" }, { "{Severity}", "{3}" }, { "{CurrentInstructionCount}", "{4}" }, { "{MaxInstructionCount}", "{5}" }, { "{Message}", "{6}" }, { "{Stack}", "{7}" } }; Stack<string> j = new Stack<string>(); public byte Filter; public readonly Dictionary<BMyAppenderBase, string> Appenders = new Dictionary<BMyAppenderBase, string>(); string k = @"[{0}-{1}/{2}][{3}][{4}/{5}][{7}] {6}"; string l = @"[{Date}-{Time}/{Milliseconds}][{Severity}][{CurrentInstructionCount}/{MaxInstructionCount}][{Stack}] {Message}"; public string Format { get { return l; } set { k = n(value); l = value; } } readonly Program m; public bool AutoFlush = true; public BMyLog4PB(Program a) : this(a, E_FATAL | E_ERROR | E_WARN | E_INFO, new BMyEchoAppender(a)) { } public BMyLog4PB(Program a, byte b, params BMyAppenderBase[] c) { Filter = b; this.m = a; foreach (var Appender in c) AddAppender(Appender); } string n(string a) { var b = a; foreach (var item in i) b = b.Replace(item.Key, item.Value); return b; } public BMyLog4PB Flush() { foreach (var AppenderItem in Appenders) AppenderItem.Key.Flush(); return this; } public BMyLog4PB PushStack(string a) { j.Push(a); return this; } public string PopStack() { return (j.Count > 0) ? j.Pop() : null; } string o() { return (j.Count > 0) ? j.Peek() : null; } public string StackToString() { if (If(E_TRACE) != null) { string[] a = j.ToArray(); Array.Reverse(a); return string.Join(@"/", a); } else return o(); } public BMyLog4PB AddAppender(BMyAppenderBase a, string b = null) { if (!Appenders.ContainsKey(a)) Appenders.Add(a, n(b)); return this; } public BMyLog4PB If(byte a) { return ((a & Filter) != 0) ? this : null; } public BMyLog4PB Fatal(string a, params object[] b) { If(E_FATAL).p("FATAL", a, b); return this; } public BMyLog4PB Error(string a, params object[] b) { If(E_ERROR).p("ERROR", a, b); return this; } public BMyLog4PB Warn(string a, params object[] b) { If(E_WARN).p("WARN", a, b); return this; } public BMyLog4PB Info(string a, params object[] b) { If(E_INFO).p("INFO", a, b); return this; } public BMyLog4PB Debug(string a, params object[] b) { If(E_DEBUG).p("DEBUG", a, b); return this; } public BMyLog4PB Trace(string a, params object[] b) { If(E_TRACE).p("TRACE", a, b); return this; } void p(string a, string b, params object[] c) { DateTime d = DateTime.Now; q e = new q(d.ToShortDateString(), d.ToLongTimeString(), d.Millisecond.ToString(), a, m.Runtime.CurrentInstructionCount, m.Runtime.MaxInstructionCount, string.Format(b, c), StackToString()); foreach (var item in Appenders) { var f = (item.Value != null) ? item.Value : k; item.Key.Enqueue(e.ToString(f)); if (AutoFlush) item.Key.Flush(); } } class q { public string Date; public string Time; public string Milliseconds; public string Severity; public int CurrentInstructionCount; public int MaxInstructionCount; public string Message; public string Stack; public q(string a, string b, string c, string d, int e, int f, string g, string h) { this.Date = a; this.Time = b; this.Milliseconds = c; this.Severity = d; this.CurrentInstructionCount = e; this.MaxInstructionCount = f; this.Message = g; this.Stack = h; } public override string ToString() { return ToString(@"{0},{1},{2},{3},{4},{5},{6},{7},{8}"); } public string ToString(string a) { return string.Format(a, Date, Time, Milliseconds, Severity, CurrentInstructionCount, MaxInstructionCount, Message, Stack); } } public class BMyTextPanelAppender : BMyAppenderBase { List<string> i = new List<string>(); List<IMyTextPanel> j = new List<IMyTextPanel>(); public bool Autoscroll = true; public bool Prepend = false; public BMyTextPanelAppender(string a, Program b) { b.GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(j, (c => c.CustomName.Contains(a))); } public override void Enqueue(string a) { i.Add(a); } public override void Flush() { foreach (var Panel in j) { k(Panel); Panel.ShowTextureOnScreen(); Panel.ShowPublicTextOnScreen(); } i.Clear(); } void k(IMyTextPanel a) { if (Autoscroll) { var b = new List<string>(a.GetPublicText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)); b.AddRange(i); int c = Math.Min(l(a), b.Count); if (Prepend) b.Reverse(); a.WritePublicText(string.Join("\n", b.GetRange(b.Count - c, c).ToArray()), false); } else if (Prepend) { var b = new List<string>(a.GetPublicText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)); b.AddRange(i); b.Reverse(); a.WritePublicText(string.Join("\n", b.ToArray()), false); } else { a.WritePublicText(string.Join("\n", i.ToArray()), true); } } int l(IMyTextPanel a) { float b = a.GetValueFloat("FontSize"); if (b == 0.0f) b = 0.01f; return Convert.ToInt32(Math.Ceiling(17.0f / b)); } } public class BMyKryptDebugSrvAppender : BMyAppenderBase { IMyProgrammableBlock i; Queue<string> j = new Queue<string>(); public BMyKryptDebugSrvAppender(Program a) { i = a.GridTerminalSystem.GetBlockWithName("DebugSrv") as IMyProgrammableBlock; } public override void Flush() { if (i != null) { var a = true; while (a && j.Count > 0) if (i.TryRun("L" + j.Peek())) { j.Dequeue(); } else { a = false; } } } public override void Enqueue(string a) { j.Enqueue(a); } } public class BMyEchoAppender : BMyAppenderBase { Program i; public BMyEchoAppender(Program a) { this.i = a; } public override void Flush() { } public override void Enqueue(string a) { i.Echo(a); } } public abstract class BMyAppenderBase { public abstract void Enqueue(string a); public abstract void Flush(); } }
        public class BaconArgs { public string Raw; static public BaconArgs parse(string a) { return (new Parser()).parseArgs(a); } static public string Escape(object a) { return string.Format("{0}", a).Replace(@"\", @"\\").Replace(@" ", @"\ ").Replace(@"""", @"\"""); } static public string UnEscape(string a) { return a.Replace(@"\""", @"""").Replace(@"\ ", @" ").Replace(@"\\", @"\"); } public class Parser { static Dictionary<string, BaconArgs> h = new Dictionary<string, BaconArgs>(); public BaconArgs parseArgs(string a) { if (!h.ContainsKey(a)) { var b = new BaconArgs(); b.Raw = a; var c = false; var d = false; var e = new StringBuilder(); for (int f = 0; f < a.Length; f++) { var g = a[f]; if (c) { e.Append(g); c = false; } else if (g.Equals('\\')) c = true; else if (d && !g.Equals('"')) e.Append(g); else if (g.Equals('"')) d = !d; else if (g.Equals(' ')) if (e.ToString().Equals("--")) { b.add(a.Substring(f).TrimStart()); e.Clear(); break; } else { b.add(e.ToString()); e.Clear(); } else e.Append(g); } if (e.Length > 0) b.add(e.ToString()); h.Add(a, b); } return h[a]; } } protected Dictionary<char, int> h = new Dictionary<char, int>(); protected List<string> i = new List<string>(); protected Dictionary<string, List<string>> j = new Dictionary<string, List<string>>(); public List<string> getArguments() { return i; } public int getFlag(char a) { return h.ContainsKey(a) ? h[a] : 0; } public bool hasArguments() { return i.Count > 0; } public bool hasOption(string a) { return j.ContainsKey(a); } public List<string> getOption(string a) { return j.ContainsKey(a) ? j[a] : new List<string>(); } public void add(string a) { if (a.Trim().Length > 0) if (!a.StartsWith("-")) { i.Add(a); } else if (a.StartsWith("--")) { KeyValuePair<string, string> b = k(a); string c = b.Key.Substring(2); if (!j.ContainsKey(c)) { j.Add(c, new List<string>()); } j[c].Add(b.Value); } else { string b = a.Substring(1); for (int d = 0; d < b.Length; d++) { if (this.h.ContainsKey(b[d])) { this.h[b[d]]++; } else { this.h.Add(b[d], 1); } } } } KeyValuePair<string, string> k(string a) { string[] b = a.Split(new char[] { '=' }, 2); return new KeyValuePair<string, string>(b[0], (b.Length > 1) ? b[1] : null); } public string ToArguments() { var a = new List<string>(); foreach (string argument in this.getArguments()) a.Add(Escape(argument)); foreach (KeyValuePair<string, List<string>> option in this.j) { var b = "--" + Escape(option.Key); foreach (string optVal in option.Value) a.Add(b + ((optVal != null) ? "=" + Escape(optVal) : "")); } var c = (h.Count > 0) ? "-" : ""; foreach (KeyValuePair<char, int> flag in h) c += new String(flag.Key, flag.Value); a.Add(c); return String.Join(" ", a.ToArray()); } override public string ToString() { var a = new List<string>(); foreach (string key in j.Keys) a.Add(l(key) + ":[" + string.Join(",", j[key].ConvertAll<string>(b => l(b)).ToArray()) + "]"); var c = new List<string>(); foreach (char key in h.Keys) c.Add(key + ":" + h[key].ToString()); var d = new StringBuilder(); d.Append("{\"a\":["); d.Append(string.Join(",", i.ConvertAll<string>(b => l(b)).ToArray())); d.Append("],\"o\":[{"); d.Append(string.Join("},{", a)); d.Append("}],\"f\":[{"); d.Append(string.Join("},{", c)); d.Append("}]}"); return d.ToString(); } string l(string a) { return (a != null) ? "\"" + a.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" : @"null"; } }
        #endregion libraries
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}