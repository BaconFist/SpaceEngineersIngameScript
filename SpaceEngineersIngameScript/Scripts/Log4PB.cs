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
                {"{Stack}","{7}" },
                {"Origin}", "{8}" }
            };
            private Stack<string> Stack = new Stack<string>();
            public byte Filter;
            public readonly Dictionary<BMyAppenderBase, string> Appenders = new Dictionary<BMyAppenderBase, string>();
            private string _defaultFormat = @"[{0}-{1}/{2}][{3}][{4}/{5}][{8}][{7}] {6}";
            private string _formatRaw = @"[{Date}-{Time}/{Milliseconds}][{Severity}][{CurrentInstructionCount}/{MaxInstructionCount}][{Origin}][{Stack}] {Message}";
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
                foreach (var Appender in Appenders)
                {
                    AddAppender(Appender);
                }
            }
            private string compileFormat(string value)
            {
                string format = value;
                foreach (var item in formatMarkerMap)
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
                return (Stack.Count > 0) ? Stack.Pop():null;
            }
            private string PeekStack()
            {
                return (Stack.Count > 0) ? Stack.Peek() : null;
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
                var message = new BMyMessage(
                    DT.ToShortDateString(),
                    DT.ToLongTimeString(),
                    DT.Millisecond.ToString(),
                    level,
                    Assembly.Runtime.CurrentInstructionCount,
                    Assembly.Runtime.MaxInstructionCount,
                    string.Format(format, values),
                    StackToString(),
                    Assembly.Me.CustomName
                );
                foreach(var item in Appenders)
                {
                    var formatBuffer = (item.Value != null) ? item.Value : _defaultFormat;
                    item.Key.Enqueue(message.ToString(formatBuffer));
                    if (AutoFlush)
                    {
                        item.Key.Flush();
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
                public string Origin;

                public BMyMessage(string Date, string Time, string Milliseconds, string Severity, int CurrentInstructionCount, int MaxInstructionCount, string Message, string Stack, string Origin)
                {
                    this.Date = Date;
                    this.Time = Time;
                    this.Milliseconds = Milliseconds;
                    this.Severity = Severity;
                    this.CurrentInstructionCount = CurrentInstructionCount;
                    this.MaxInstructionCount = MaxInstructionCount;
                    this.Message = Message;
                    this.Stack = Stack;
                    this.Origin = Origin;
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
                        Stack,
                        Origin
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
                    foreach(var Panel in Panels)
                    {
                        AddEntriesToPanel(Panel);
                        Panel.ShowTextureOnScreen();
                        Panel.ShowPublicTextOnScreen();
                    }
                    Queue.Clear();
                }
                private void AddEntriesToPanel(IMyTextPanel Panel)
                {
                    if (Autoscroll)
                    {
                        List<string> buffer = new List<string>(Panel.GetPublicText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
                        buffer.AddRange(Queue);
                        int maxLines = Math.Min(getMaxLinesFromPanel(Panel), buffer.Count);
                        if (Prepend)
                        {
                            buffer.Reverse();
                        }
                        Panel.WritePublicText(string.Join("\n", buffer.GetRange(buffer.Count - maxLines, maxLines).ToArray()), false);
                    } else
                    {
                        if (Prepend)
                        {
                            var buffer = new List<string>(Panel.GetPublicText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
                            buffer.AddRange(Queue);
                            buffer.Reverse();
                            Panel.WritePublicText(string.Join("\n", buffer.ToArray()), false);
                        }
                        else {
                            Panel.WritePublicText(string.Join("\n", Queue.ToArray()), true);
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
            public class BMyCustomDataAppender : BMyAppenderBase
            {
                Program Assembly;
                public BMyCustomDataAppender(Program Assembly)
                {
                    this.Assembly = Assembly;
                    this.Assembly.Me.CustomData = "";
                }
                public override void Enqueue(string message)
                {
                    Assembly.Me.CustomData = Assembly.Me.CustomData + '\n' + message;
                }
                public override void Flush()
                {

                }                
            }

            public abstract class BMyAppenderBase
            {
                public abstract void Enqueue(string message);
                public abstract void Flush();
            }
        }
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}
 