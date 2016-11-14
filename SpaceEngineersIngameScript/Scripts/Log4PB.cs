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
                {"{Severity}","{3}"},
                {"{CurrentInstructionCount}","{4}"},
                {"{MaxInstructionCount}","{5}"},
                {"{Message}","{6}"},
                {"{Stack}","{7}" }
            };
            private Stack<string> Stack = new Stack<string>();
            public byte Filter;
            public readonly Dictionary<BMyAppenderBase, string> Appenders = new Dictionary<BMyAppenderBase, string>();
            private string _defaultFormat = @"[{0}-{1}/{2}][{3}][{4}/{5}][{7}] {6}";
            private string _formatRaw = @"[{Date}-{Time}/{Milliseconds}][{Severity}][{CurrentInstructionCount}/{MaxInstructionCount}][{Stack}] {Message}";
            public string Format {
                get { return _formatRaw; }
                set {
                    _defaultFormat = compileFormat(value);
                    _formatRaw = value;
                }
            }
            private readonly Program Assembly;
            public bool AutoFlush = true;

            public BMyLog4PB(Program Assembly) : this(Assembly, E_FATAL | E_ERROR | E_WARN | E_INFO, new BMyEchoAppender(Assembly))
            {

            }
            public BMyLog4PB(Program Assembly, byte filter, params BMyAppenderBase[] Appenders)
            {
                Filter = filter;
                this.Assembly = Assembly;
                foreach (BMyAppenderBase Appender in Appenders)
                {
                    AddAppender(Appender);
                }
            }
            private string compileFormat(string value)
            {
                string format = value;
                foreach (KeyValuePair<string, string> item in formatMarkerMap)
                {
                    format = format?.Replace(item.Key, item.Value);
                }
                return format;
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
                Stack.Push(name);
                return this;
            }
            public string PopStack()
            {
                if (Stack.Count > 0)
                {
                    return Stack.Pop();
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
            public string StackToString()
            {
                if (If(E_TRACE) != null)
                {
                    string[] buffer = Stack.ToArray();
                    Array.Reverse(buffer);
                    return string.Join(@"/", buffer);
                }
                else
                {
                    return PeekStack();
                }
            }
            public BMyLog4PB AddAppender(BMyAppenderBase Appender, string format = null)
            {
                if (!Appenders.ContainsKey(Appender))
                {
                    Appenders.Add(Appender, compileFormat(format));
                }

                return this;
            }
            public BMyLog4PB If(byte filter)
            {
                return ((filter & Filter) != 0) ? this : null;
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
                    DT.Millisecond.ToString(),
                    level,
                    Assembly.Runtime.CurrentInstructionCount,
                    Assembly.Runtime.MaxInstructionCount,
                    string.Format(format, values),
                    StackToString()
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
                public string Severity;
                public int CurrentInstructionCount;
                public int MaxInstructionCount;
                public string Message;
                public string Stack;
                public BMyMessage(string Date, string Time, string Milliseconds, string Severity, int CurrentInstructionCount, int MaxInstructionCount, string Message, string Stack)
                {
                    this.Date = Date;
                    this.Time = Time;
                    this.Milliseconds = Milliseconds;
                    this.Severity = Severity;
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
                        Severity,
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
                    Queue.Clear();
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
                private Queue<string> queue = new Queue<string>();
                public BMyKryptDebugSrvAppender(Program Assembly)
                {
                    _debugSrv = Assembly.GridTerminalSystem.GetBlockWithName("DebugSrv") as IMyProgrammableBlock;
                }
                public override void Flush(){
                    if (_debugSrv != null)
                    {
                        bool proceed = true;
                        while (proceed && queue.Count > 0)
                        {
                            if(_debugSrv.TryRun("L" + queue.Peek())){
                                queue.Dequeue();
                            } else
                            {
                                proceed = false;
                            }
                        }
                    }
                }
                public override void Enqueue(string message)
                {
                    queue.Enqueue(message);
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
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
        class min
        {
            public class BMyLog4PB { public const byte E_ALL = 63; public const byte E_TRACE = 32; public const byte E_DEBUG = 16; public const byte E_INFO = 8; public const byte E_WARN = 4; public const byte E_ERROR = 2; public const byte E_FATAL = 1; Dictionary<string, string> i = new Dictionary<string, string>() { { "{Date}", "{0}" }, { "{Time}", "{1}" }, { "{Milliseconds}", "{2}" }, { "{Severity}", "{3}" }, { "{CurrentInstructionCount}", "{4}" }, { "{MaxInstructionCount}", "{5}" }, { "{Message}", "{6}" }, { "{Stack}", "{7}" } }; Stack<string> j = new Stack<string>(); public byte Filter; public readonly Dictionary<BMyAppenderBase, string> Appenders = new Dictionary<BMyAppenderBase, string>(); string k = @"[{0}-{1}/{2}][{3}][{4}/{5}][{7}] {6}"; string l = @"[{Date}-{Time}/{Milliseconds}][{Severity}][{CurrentInstructionCount}/{MaxInstructionCount}][{Stack}] {Message}"; public string Format { get { return l; } set { k = n(value); l = value; } } readonly Program m; public bool AutoFlush = true; public BMyLog4PB(Program a) : this(a, E_FATAL | E_ERROR | E_WARN | E_INFO, new BMyEchoAppender(a)) { } public BMyLog4PB(Program a, byte b, params BMyAppenderBase[] c) { Filter = b; this.m = a; foreach (BMyAppenderBase Appender in c) AddAppender(Appender); } string n(string a) { var b = a; foreach (KeyValuePair<string, string> item in i) b = b.Replace(item.Key, item.Value); return b; } public BMyLog4PB Flush() { foreach (var AppenderItem in Appenders) AppenderItem.Key.Flush(); return this; } public BMyLog4PB PushStack(string a) { j.Push(a); return this; } public string PopStack() { if (j.Count > 0) return j.Pop(); else return null; } string o() { if (j.Count > 0) return j.Peek(); else return null; } public string StackToString() { if (If(E_TRACE) != null) { string[] a = j.ToArray(); Array.Reverse(a); return string.Join(@"/", a); } else return o(); } public BMyLog4PB AddAppender(BMyAppenderBase a, string b = null) { if (!Appenders.ContainsKey(a)) Appenders.Add(a, n(b)); return this; } public BMyLog4PB If(byte a) { return ((a & Filter) != 0) ? this : null; } public BMyLog4PB Fatal(string a, params object[] b) { If(E_FATAL).p("FATAL", a, b); return this; } public BMyLog4PB Error(string a, params object[] b) { If(E_ERROR).p("ERROR", a, b); return this; } public BMyLog4PB Warn(string a, params object[] b) { If(E_WARN).p("WARN", a, b); return this; } public BMyLog4PB Info(string a, params object[] b) { If(E_INFO).p("INFO", a, b); return this; } public BMyLog4PB Debug(string a, params object[] b) { If(E_DEBUG).p("DEBUG", a, b); return this; } public BMyLog4PB Trace(string a, params object[] b) { If(E_TRACE).p("TRACE", a, b); return this; } void p(string a, string b, params object[] c) { DateTime d = DateTime.Now; q e = new q(d.ToShortDateString(), d.ToLongTimeString(), d.Millisecond.ToString(), a, m.Runtime.CurrentInstructionCount, m.Runtime.MaxInstructionCount, string.Format(b, c), StackToString()); foreach (KeyValuePair<BMyAppenderBase, string> Item in Appenders) { var f = (Item.Value != null) ? Item.Value : k; Item.Key.Enqueue(e.ToString(f)); if (AutoFlush) Item.Key.Flush(); } } class q { public string Date; public string Time; public string Milliseconds; public string Severity; public int CurrentInstructionCount; public int MaxInstructionCount; public string Message; public string Stack; public q(string a, string b, string c, string d, int e, int f, string g, string h) { this.Date = a; this.Time = b; this.Milliseconds = c; this.Severity = d; this.CurrentInstructionCount = e; this.MaxInstructionCount = f; this.Message = g; this.Stack = h; } public override string ToString() { return ToString(@"{0},{1},{2},{3},{4},{5},{6},{7},{8}"); } public string ToString(string a) { return string.Format(a, Date, Time, Milliseconds, Severity, CurrentInstructionCount, MaxInstructionCount, Message, Stack); } } public class BMyTextPanelAppender : BMyAppenderBase { List<string> i = new List<string>(); List<IMyTextPanel> j = new List<IMyTextPanel>(); public bool Autoscroll = true; public bool Prepend = false; public BMyTextPanelAppender(string a, Program b) { b.GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(j, (c => c.CustomName.Contains(a))); } public override void Enqueue(string a) { i.Add(a); } public override void Flush() { foreach (IMyTextPanel Panel in j) { k(Panel); Panel.ShowTextureOnScreen(); Panel.ShowPrivateTextOnScreen(); } i.Clear(); } void k(IMyTextPanel a) { if (Autoscroll) { var b = new List<string>(a.GetPrivateText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)); b.AddRange(i); int c = Math.Min(l(a), b.Count); if (Prepend) b.Reverse(); a.WritePrivateText(string.Join("\n", b.GetRange(b.Count - c, c).ToArray()), false); } else if (Prepend) { List<string> b = new List<string>(a.GetPrivateText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)); b.AddRange(i); b.Reverse(); a.WritePrivateText(string.Join("\n", b.ToArray()), false); } else { a.WritePrivateText(string.Join("\n", i.ToArray()), true); } } int l(IMyTextPanel a) { float b = a.GetValueFloat("FontSize"); if (b == 0.0f) b = 0.01f; return Convert.ToInt32(Math.Ceiling(17.0f / b)); } } public class BMyKryptDebugSrvAppender : BMyAppenderBase { IMyProgrammableBlock i; Queue<string> j = new Queue<string>(); public BMyKryptDebugSrvAppender(Program a) { i = a.GridTerminalSystem.GetBlockWithName("DebugSrv") as IMyProgrammableBlock; } public override void Flush() { if (i != null) { var a = true; while (a && j.Count > 0) if (i.TryRun("L" + j.Peek())) { j.Dequeue(); } else { a = false; } } } public override void Enqueue(string a) { j.Enqueue(a); } } public class BMyEchoAppender : BMyAppenderBase { Program i; public BMyEchoAppender(Program a) { this.i = a; } public override void Flush() { } public override void Enqueue(string a) { i.Echo(a); } } public abstract class BMyAppenderBase { public abstract void Enqueue(string a); public abstract void Flush(); } }
        }

        
    }
}