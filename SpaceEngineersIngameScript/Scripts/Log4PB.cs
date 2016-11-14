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

namespace Log4PB
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        public void Main(string argument)
        {
            var LCDApender = new BMyLog4PB.BMyTextPanelAppender("[Log]", this);
            // create an Appender to display Messages on all LCD's containing [Log]

            var DebugSrvAppender = new BMyLog4PB.BMyKryptDebugSrvAppender(this);
            // create an Appender to display Messages on Krypt's Debug Server

            var Logger = new BMyLog4PB(this, 0, null);
            // create a new Logger without any output

            Logger.AutoFlush = false;
            // queue all Messages

            Logger.AddAppender(LCDApender);
            // add TextPanel Appender with default format

            Logger.AddAppender(DebugSrvAppender, @"{Verbosity} - {Message}");
            // add DebugServer Appender with custom format (example: "INFO - This is an informational Message")

            Logger.Mask = BMyLog4PB.E_ALL;
            // show all Messages

            try
            {
                Logger.PushStack("Main");
                // step into scope

                Logger.Info("Start building a simple List of active Lights.");
                var ActiveLights = new List<string>();
                var Lights = new List<IMyInteriorLight>();
                GridTerminalSystem.GetBlocksOfType<IMyInteriorLight>(Lights);
                if(Lights.Count > 0)
                {
                    Logger.Info("Fond {0} Lights.", Lights.Count);
                    // Logging using internal Format

                    Logger.PushStack("Light - Loop");
                    // step into scrope

                    for (int i = 0; i < Lights.Count; i++)
                    {
                        var Light = Lights[i];
                        if (Light.Enabled)
                        {
                            ActiveLights.Add(Light.CustomName);
                            Logger.Info("Adding Light \"{2}\" {0}/{1} to List", i, Lights.Count, Light.CustomName);
                        }
                    }
                    Logger.PopStack();
                    // leave scope an go back to "Main"

                    Logger.If(BMyLog4PB.E_DEBUG)?.Debug(@"Found {0} active Lights => [""{1}""]", ActiveLights.Count, string.Join("\",\"", ActiveLights.ToArray()));
                    // because of "If(BMyLog4PB.E_DEBUG)" the concatenation if active Lights will only be processed if DEBUG is enabled.
                    // This might save you some Instructions.
                }
                else
                {
                    Logger.Warn("Found no Lights.");
                }
                Logger.PopStack();
                // leave the "Main" scope
            }
            finally
            {
                // until here all Messages are queued but not displayed
                // this is to improve performace as it reduces the changes on textpanels
                Logger.Flush();
                // Flush() tells all Appenders to send their queue to their outputs and clears the queues.
                // It's best used with a try-finally block to ensure it is flushed when an Exception occures
            }
        }

        public class BMyLog4PB
        {
            public const byte E_ALL = 63;
            public const byte E_TRACE = 32; //Lowest	Finest-grained informational events.
            public const byte E_DEBUG = 16; //Fine-grained informational events that are most useful to debug an application.
            public const byte E_INFO = 8; //Informational messages that highlight the progress of the application at coarse-grained level.
            public const byte E_WARN = 4; //Potentially harmful situations which still allow the application to continue running.
            public const byte E_ERROR = 2; //Error events that might still allow the application to continue running.
            public const byte E_FATAL = 1; //Highest	Very severe error events that will presumably lead the application to abort.

            private Dictionary<string, string> formatMarkerMap = new Dictionary<string, string>() {
                {"{Date}","{0}"},
                {"{Time}","{1}"},
                {"{Milliseconds}","{2}"},
                {"{Verbosity}","{3}"},
                {"{CurrentInstructionCount}","{4}"},
                {"{MaxInstructionCount}","{5}"},
                {"{Message}","{6}"},
                {"{Stack}","{7}" }
            };
            private Queue<string> Stack = new Queue<string>();
            public byte Mask;
            public readonly Dictionary<BMyAppenderBase, string> Appenders = new Dictionary<BMyAppenderBase, string>();
            private string _defaultFormat = @"[{0}-{1}.{2}][{3}][{4}/{5}][{7}] {6}";
            public string Format {
                get { return _defaultFormat; }
                set {
                    string format = value;
                    foreach (KeyValuePair<string, string> item in formatMarkerMap)
                    {
                        format = format.Replace(item.Key, item.Value);
                    }
                    _defaultFormat = format;
                }
            }
            private readonly Program Assembly;
            public bool AutoFlush = true;

            public BMyLog4PB(Program Assembly) : this(Assembly, E_FATAL | E_ERROR | E_WARN | E_INFO, new BMyEchoAppender(Assembly))
            {

            }
            public BMyLog4PB(Program Assembly, byte mask, params BMyAppenderBase[] Appenders)
            {
                Mask = mask;
                this.Assembly = Assembly;
                foreach(BMyAppenderBase Appender in Appenders)
                {
                    AddAppender(Appender);
                }
            }

            public BMyLog4PB Flush()
            {
                foreach(var AppenderItem in Appenders)
                {
                    AppenderItem.Key.Flush();
                }
                return this;
            }
            public BMyLog4PB PushStack(string name)
            {
                Stack.Enqueue(name);
                return this;
            }
            public string PopStack()
            {
                if (Stack.Count > 0)
                {
                    return Stack.Dequeue();
                } else
                {
                    return null;
                }
            }
            private string PeekStack()
            {
                if (Stack.Count > 0)
                {
                    return Stack.Peek();
                }
                else
                {
                    return null;
                }
            }
            public BMyLog4PB AddAppender(BMyAppenderBase Appender, string format = null)
            {
                if (!Appenders.ContainsKey(Appender))
                {
                    Appenders.Add(Appender, format);
                }

                return this;
            }
            public BMyLog4PB If(byte mask)
            {
                return ((mask & Mask) != 0) ? this : null;
            }
            public BMyLog4PB Fatal(string format, params object[] values)
            {
                If(E_FATAL)?.Append("FATAL", format, values);
                return this;
            }
            public BMyLog4PB Error(string format, params object[] values)
            {
                If(E_ERROR)?.Append("ERROR", format, values);
                return this;
            }
            public BMyLog4PB Warn(string format, params object[] values)
            {
                If(E_WARN)?.Append("WARN", format, values);
                return this;
            }
            public BMyLog4PB Info(string format, params object[] values)
            {
                If(E_INFO)?.Append("INFO", format, values);
                return this;
            }
            public BMyLog4PB Debug(string format, params object[] values)
            {
                If(E_DEBUG)?.Append("DEBUG", format, values);
                return this;
            }
            public BMyLog4PB Trace(string format, params object[] values)
            {
                If(E_TRACE)?.Append("TRACE", format, values);
                return this;
            }
            private void Append(string level, string format, params object[] values)
            {
                DateTime DT = DateTime.Now;
                BMyMessage message = new BMyMessage(
                    DT.ToShortDateString(),
                    DT.ToLongTimeString(),
                    DT.Millisecond.ToString().TrimStart('0'),
                    level,
                    Assembly.Runtime.CurrentInstructionCount,
                    Assembly.Runtime.MaxInstructionCount,
                    string.Format(format, values),
                    (If(E_TRACE)!=null)?string.Join("/", Stack.ToArray()):PeekStack()                                    
                );
                foreach(KeyValuePair<BMyAppenderBase, string> Item in Appenders)
                {
                    string formatBuffer = (Item.Value != null) ? Item.Value : _defaultFormat;
                    Item.Key.Enqueue(message.ToString(formatBuffer));
                    if (AutoFlush)
                    {
                        Item.Key.Flush();
                    }
                }
            }
            class BMyMessage
            {
                public string Date;
                public string Time;
                public string Milliseconds;
                public string Level;
                public int CurrentInstructionCount;
                public int MaxInstructionCount;
                public string Message;
                public string Stack;
                public BMyMessage(string Date, string Time, string Milliseconds, string Level, int CurrentInstructionCount, int MaxInstructionCount, string Message, string Stack)
                {
                    this.Date = Date;
                    this.Time = Time;
                    this.Milliseconds = Milliseconds;
                    this.Level = Level;
                    this.CurrentInstructionCount = CurrentInstructionCount;
                    this.MaxInstructionCount = MaxInstructionCount;
                    this.Message = Message;
                    this.Stack = Stack;
                }
                public override string ToString()
                {
                    return ToString(@"{0},{1},{2},{3},{4},{5},{6},{7},{8}"); 
                }
                public string ToString(string format)
                {
                    return string.Format(
                        format,
                        Date,
                        Time,
                        Milliseconds,
                        Level,
                        CurrentInstructionCount,
                        MaxInstructionCount,
                        Message,
                        Stack
                        );
                }
            }
            public class BMyTextPanelAppender : BMyAppenderBase
            {
                List<string> Queue = new List<string>();
                List<IMyTextPanel> Panels = new List<IMyTextPanel>();
                public bool Autoscroll = true;
                public bool Prepend = false;
                public BMyTextPanelAppender(string tag, Program Assembly)
                {
                    Assembly.GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(Panels, (p => p.CustomName.Contains(tag)));
                }
                public override void Enqueue(string message)
                {
                    Queue.Add(message);
                }
                public override void Flush()
                {
                    foreach(IMyTextPanel Panel in Panels)
                    {
                        AddEntriesToPanel(Panel);
                        Panel.ShowTextureOnScreen();
                        Panel.ShowPrivateTextOnScreen();
                    }
                }
                private void AddEntriesToPanel(IMyTextPanel Panel)
                {
                    if (Autoscroll)
                    {
                        List<string> buffer = new List<string>(Panel.GetPrivateText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
                        buffer.AddRange(Queue);
                        int maxLines = Math.Min(getMaxLinesFromPanel(Panel), buffer.Count);
                        if (Prepend)
                        {
                            buffer.Reverse();
                        }
                        Panel.WritePrivateText(string.Join("\n", buffer.GetRange(buffer.Count - maxLines, maxLines).ToArray()), false);
                    } else
                    {
                        if (Prepend)
                        {
                            List<string> buffer = new List<string>(Panel.GetPrivateText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
                            buffer.AddRange(Queue);
                            buffer.Reverse();
                            Panel.WritePrivateText(string.Join("\n", buffer.ToArray()), false);
                        }
                        else {
                            Panel.WritePrivateText(string.Join("\n", Queue.ToArray()), true);
                        }
                    }
                    Queue.Clear();
                }
                private int getMaxLinesFromPanel(IMyTextPanel Panel)
                {
                    float fontSize = Panel.GetValueFloat("FontSize");
                    if (fontSize == 0.0f)
                    {
                        fontSize = 0.01f;
                    }
                    return Convert.ToInt32(Math.Ceiling(17.0f / fontSize));
                }
            }
            public class BMyKryptDebugSrvAppender : BMyAppenderBase
            {
                private IMyProgrammableBlock _debugSrv;
                public BMyKryptDebugSrvAppender(Program Assembly)
                {
                    _debugSrv = Assembly.GridTerminalSystem.GetBlockWithName("DebugSrv") as IMyProgrammableBlock;
                }
                public override void Flush(){}
                public override void Enqueue(string message)
                {
                    if (_debugSrv != null) { _debugSrv.TryRun("L" + message); }
                }
            }
            public class BMyEchoAppender : BMyAppenderBase
            {
                private Program Assembly;

                public BMyEchoAppender(Program Assembly)
                {
                    this.Assembly = Assembly;
                }

                public override void Flush(){}

                public override void Enqueue(string message)
                {
                    Assembly.Echo(message);
                }
            }
            public abstract class BMyAppenderBase
            {
                public abstract void Enqueue(string message);
                public abstract void Flush();
            }
        }
        class min
        {
            public class BMyLog4PB { public const byte E_ALL = 63; public const byte E_TRACE = 32; public const byte E_DEBUG = 16; public const byte E_INFO = 8; public const byte E_WARN = 4; public const byte E_ERROR = 2; public const byte E_FATAL = 1; Dictionary<string, string> i = new Dictionary<string, string>() { { "{Date}", "{0}" }, { "{Time}", "{1}" }, { "{Milliseconds}", "{2}" }, { "{Verbosity}", "{3}" }, { "{CurrentInstructionCount}", "{4}" }, { "{MaxInstructionCount}", "{5}" }, { "{Message}", "{6}" }, { "{Stack}", "{7}" } }; Queue<string> j = new Queue<string>(); public byte Mask; public readonly Dictionary<BMyAppenderBase, string> Appenders = new Dictionary<BMyAppenderBase, string>(); string k = @"[{0}-{1}.{2}][{3}][{4}/{5}][{7}] {6}"; public string Format { get { return k; } set { var a = value; foreach (KeyValuePair<string, string> item in i) a = a.Replace(item.Key, item.Value); k = a; } } readonly Program l; public bool AutoFlush = true; public BMyLog4PB(Program a) : this(a, E_FATAL | E_ERROR | E_WARN, new BMyEchoAppender(a)) { } public BMyLog4PB(Program a, byte b, params BMyAppenderBase[] c) { Mask = b; this.l = a; foreach (BMyAppenderBase Appender in c) AddAppender(Appender); } public BMyLog4PB Flush() { foreach (var AppenderItem in Appenders) AppenderItem.Key.Flush(); return this; } public BMyLog4PB PushStack(string a) { j.Enqueue(a); return this; } public string PopStack() { if (j.Count > 0) return j.Dequeue(); else return null; } string m() { if (j.Count > 0) return j.Peek(); else return null; } public BMyLog4PB AddAppender(BMyAppenderBase a, string b = null) { if (!Appenders.ContainsKey(a)) Appenders.Add(a, b); return this; } public BMyLog4PB If(byte a) { return ((a & Mask) != 0) ? this : null; } public BMyLog4PB Fatal(string a, params object[] b) { If(E_FATAL).n("FATAL", a, b); return this; } public BMyLog4PB Error(string a, params object[] b) { If(E_ERROR).n("ERROR", a, b); return this; } public BMyLog4PB Warn(string a, params object[] b) { If(E_WARN).n("WARN", a, b); return this; } public BMyLog4PB Info(string a, params object[] b) { If(E_INFO).n("INFO", a, b); return this; } public BMyLog4PB Debug(string a, params object[] b) { If(E_DEBUG).n("DEBUG", a, b); return this; } public BMyLog4PB Trace(string a, params object[] b) { If(E_TRACE).n("TRACE", a, b); return this; } void n(string a, string b, params object[] c) { DateTime d = DateTime.Now; o e = new o(d.ToShortDateString(), d.ToLongTimeString(), d.Millisecond.ToString().TrimStart('0'), a, l.Runtime.CurrentInstructionCount, l.Runtime.MaxInstructionCount, string.Format(b, c), (If(E_TRACE) != null) ? string.Join("/", j.ToArray()) : m()); foreach (KeyValuePair<BMyAppenderBase, string> Item in Appenders) { var f = (Item.Value != null) ? Item.Value : k; Item.Key.Enqueue(e.ToString(f)); if (AutoFlush) Item.Key.Flush(); } } class o { public string Date; public string Time; public string Milliseconds; public string Level; public int CurrentInstructionCount; public int MaxInstructionCount; public string Message; public string Stack; public o(string a, string b, string c, string d, int e, int f, string g, string h) { this.Date = a; this.Time = b; this.Milliseconds = c; this.Level = d; this.CurrentInstructionCount = e; this.MaxInstructionCount = f; this.Message = g; this.Stack = h; } public override string ToString() { return ToString(@"{0},{1},{2},{3},{4},{5},{6},{7},{8}"); } public string ToString(string a) { return string.Format(a, Date, Time, Milliseconds, Level, CurrentInstructionCount, MaxInstructionCount, Message, Stack); } } public class BMyTextPanelAppender : BMyAppenderBase { List<string> i = new List<string>(); List<IMyTextPanel> j = new List<IMyTextPanel>(); public bool Autoscroll = true; public bool Prepend = false; public BMyTextPanelAppender(string a, Program b) { b.GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(j, (c => c.CustomName.Contains(a))); } public override void Enqueue(string a) { i.Add(a); } public override void Flush() { foreach (IMyTextPanel Panel in j) { k(Panel); Panel.ShowTextureOnScreen(); Panel.ShowPrivateTextOnScreen(); } } void k(IMyTextPanel a) { if (Autoscroll) { var b = new List<string>(a.GetPrivateText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)); b.AddRange(i); int c = Math.Min(l(a), b.Count); if (Prepend) b.Reverse(); a.WritePrivateText(string.Join("\n", b.GetRange(b.Count - c, c).ToArray()), false); } else if (Prepend) { List<string> b = new List<string>(a.GetPrivateText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)); b.AddRange(i); b.Reverse(); a.WritePrivateText(string.Join("\n", b.ToArray()), false); } else { a.WritePrivateText(string.Join("\n", i.ToArray()), true); } i.Clear(); } int l(IMyTextPanel a) { float b = a.GetValueFloat("FontSize"); if (b == 0.0f) b = 0.01f; return Convert.ToInt32(Math.Ceiling(17.0f / b)); } } public class BMyKryptDebugSrvAppender : BMyAppenderBase { IMyProgrammableBlock i; public BMyKryptDebugSrvAppender(Program a) { i = a.GridTerminalSystem.GetBlockWithName("DebugSrv") as IMyProgrammableBlock; } public override void Flush() { } public override void Enqueue(string a) { if (i != null) i.TryRun("L" + a); } } public class BMyEchoAppender : BMyAppenderBase { Program i; public BMyEchoAppender(Program a) { this.i = a; } public override void Flush() { } public override void Enqueue(string a) { i.Echo(a); } } public abstract class BMyAppenderBase { public abstract void Enqueue(string a); public abstract void Flush(); } }
        }

        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}