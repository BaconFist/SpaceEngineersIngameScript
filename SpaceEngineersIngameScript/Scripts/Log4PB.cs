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

            public BMyLog4PB IfFatal{get{
                    return ((E_FATAL & Filter) != 0) ? this : null;
                } }
            public BMyLog4PB IfError{get{
                    return ((E_ERROR & Filter) != 0) ? this : null;
                } }
            public BMyLog4PB IfWarn{get{
                    return ((E_WARN & Filter) != 0) ? this : null;
                } }
            public BMyLog4PB IfInfo{get{
                    return ((E_INFO & Filter) != 0) ? this : null;
                } }
            public BMyLog4PB IfDebug{get{
                    return ((E_DEBUG & Filter) != 0) ? this : null;
                } }
            public BMyLog4PB IfTrace{get{
                    return ((E_TRACE & Filter) != 0) ? this : null;
                } }           


            private Dictionary<string, string> PredefindedMessages = new Dictionary<string, string>();

            private Dictionary<string, string> formatMarkerMap = new Dictionary<string, string>() {
                {"{Date}","{0}"},
                {"{Time}","{1}"},
                {"{Milliseconds}","{2}"},
                {"{Severity}","{3}"},
                {"{CurrentInstructionCount}","{4}"},
                {"{MaxInstructionCount}","{5}"},
                {"{Message}","{6}"},
                {"{Stack}","{7}" },
                {"{Origin}", "{8}" }
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
            public BMyLog4PB SetMessage(string key, string message)
            {
                if (!PredefindedMessages.ContainsKey(key))
                {
                    PredefindedMessages.Add(key,message);
                } else
                {
                    PredefindedMessages[key] = message;
                }

                return this;
            }
            private string TryGetPredefinedMessage(string key)
            {
                return PredefindedMessages.ContainsKey(key) ? PredefindedMessages[key] : key;
            }
            private string compileFormat(string value)
            {
                string format = value;
                foreach (var item in formatMarkerMap)
                {
                    
                    format = (format != null )?format.Replace(item.Key, item.Value):null;
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
                if (IfTrace != null)
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
                
                if(IfFatal != null)
                    Append("FATAL", format, values);
                return this;
            }
            public BMyLog4PB Error(string format, params object[] values)
            {
                if(IfError != null)
                    Append("ERROR", format, values);
                return this;
            }
            public BMyLog4PB Warn(string format, params object[] values)
            {
                if(IfWarn != null)
                    Append("WARN", format, values);
                return this;
            }
            public BMyLog4PB Info(string format, params object[] values)
            {
                if(IfInfo != null)
                    Append("INFO", format, values);
                return this;
            }
            public BMyLog4PB Debug(string format, params object[] values)
            {
                if(IfDebug != null)
                    Append("DEBUG", format, values);
                return this;
            }
            public BMyLog4PB Trace(string format, params object[] values)
            {
                if(IfTrace != null)
                    Append("TRACE", format, values);
                return this;
            }
            private void Append(string level, string format, params object[] values)
            {
                DateTime DT = DateTime.Now;

                format = TryGetPredefinedMessage(format);
                string formattedMessage;
                try
                {
                    formattedMessage = string.Format(format, values);
                } catch (FormatException e)
                {
                    formattedMessage = format + " [BMYLOG4PB FATAL]: " + e.Message;
                }

                var message = new BMyMessage(
                    DT.ToShortDateString(),
                    DT.ToLongTimeString(),
                    DT.Millisecond.ToString(),
                    level,
                    Assembly.Runtime.CurrentInstructionCount,
                    Assembly.Runtime.MaxInstructionCount,
                    formattedMessage,
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

        // public class BMyLog4PB{public const byte E_ALL=63;public const byte E_TRACE=32;public const byte E_DEBUG=16;public const byte E_INFO=8;public const byte E_WARN=4;public const byte E_ERROR=2;public const byte E_FATAL=1;public BMyLog4PB IfFatal{get{return((E_FATAL&Filter)!=0)?this:null;}}public BMyLog4PB IfError{get{return((E_ERROR&Filter)!=0)?this:null;}}public BMyLog4PB IfWarn{get{return((E_WARN&Filter)!=0)?this:null;}}public BMyLog4PB IfInfo{get{return((E_INFO&Filter)!=0)?this:null;}}public BMyLog4PB IfDebug{get{return((E_DEBUG&Filter)!=0)?this:null;}}public BMyLog4PB IfTrace{get{return((E_TRACE&Filter)!=0)?this:null;}}Dictionary<string,string>k=new Dictionary<string,string>();Dictionary<string,string>l=new Dictionary<string,string>(){{"{Date}","{0}"},{"{Time}","{1}"},{"{Milliseconds}","{2}"},{"{Severity}","{3}"},{"{CurrentInstructionCount}","{4}"},{"{MaxInstructionCount}","{5}"},{"{Message}","{6}"},{"{Stack}","{7}"},{"{Origin}","{8}"}};Stack<string>m=new Stack<string>();public byte Filter;public readonly Dictionary<BMyAppenderBase,string>Appenders=new Dictionary<BMyAppenderBase,string>();string n=@"[{0}-{1}/{2}][{3}][{4}/{5}][{8}][{7}] {6}";string o=@"[{Date}-{Time}/{Milliseconds}][{Severity}][{CurrentInstructionCount}/{MaxInstructionCount}][{Origin}][{Stack}] {Message}";public string Format{get{return o;}set{n=r(value);o=value;}}readonly Program p;public bool AutoFlush=true;public BMyLog4PB(Program a):this(a,E_FATAL|E_ERROR|E_WARN|E_INFO,new BMyEchoAppender(a)){}public BMyLog4PB(Program a,byte b,params BMyAppenderBase[]c){Filter=b;this.p=a;foreach(var Appender in c)AddAppender(Appender);}public BMyLog4PB SetMessage(string a,string b){if(!k.ContainsKey(a))k.Add(a,b);else k[a]=b;return this;}string q(string a){return k.ContainsKey(a)?k[a]:a;}string r(string a){var b=a;foreach(var item in l)b=(b!=null)?b.Replace(item.Key,item.Value):null;return b;}public BMyLog4PB Flush(){foreach(var AppenderItem in Appenders)AppenderItem.Key.Flush();return this;}public BMyLog4PB PushStack(string a){m.Push(a);return this;}public string PopStack(){return(m.Count>0)?m.Pop():null;}string s(){return(m.Count>0)?m.Peek():null;}public string StackToString(){if(IfTrace!=null){string[]a=m.ToArray();Array.Reverse(a);return string.Join(@"/",a);}else return s();}public BMyLog4PB AddAppender(BMyAppenderBase a,string b=null){if(!Appenders.ContainsKey(a))Appenders.Add(a,r(b));return this;}public BMyLog4PB If(byte a){return((a&Filter)!=0)?this:null;}public BMyLog4PB Fatal(string a,params object[]b){if(IfFatal!=null)t("FATAL",a,b);return this;}public BMyLog4PB Error(string a,params object[]b){if(IfError!=null)t("ERROR",a,b);return this;}public BMyLog4PB Warn(string a,params object[]b){if(IfWarn!=null)t("WARN",a,b);return this;}public BMyLog4PB Info(string a,params object[]b){if(IfInfo!=null)t("INFO",a,b);return this;}public BMyLog4PB Debug(string a,params object[]b){if(IfDebug!=null)t("DEBUG",a,b);return this;}public BMyLog4PB Trace(string a,params object[]b){if(IfTrace!=null)t("TRACE",a,b);return this;}void t(string a,string b,params object[]c){DateTime d=DateTime.Now;b=q(b);string f;try{f=string.Format(b,c);}catch(FormatException e){f=b+" [BMYLOG4PB FATAL]: "+e.Message;}u g=new u(d.ToShortDateString(),d.ToLongTimeString(),d.Millisecond.ToString(),a,p.Runtime.CurrentInstructionCount,p.Runtime.MaxInstructionCount,f,StackToString(),p.Me.CustomName);foreach(var item in Appenders){var h=(item.Value!=null)?item.Value:n;item.Key.Enqueue(g.ToString(h));if(AutoFlush)item.Key.Flush();}}class u{public string Date;public string Time;public string Milliseconds;public string Severity;public int CurrentInstructionCount;public int MaxInstructionCount;public string Message;public string Stack;public string Origin;public u(string a,string b,string c,string d,int f,int g,string h,string i,string j){this.Date=a;this.Time=b;this.Milliseconds=c;this.Severity=d;this.CurrentInstructionCount=f;this.MaxInstructionCount=g;this.Message=h;this.Stack=i;this.Origin=j;}public override string ToString(){return ToString(@"{0},{1},{2},{3},{4},{5},{6},{7},{8}");}public string ToString(string a){return string.Format(a,Date,Time,Milliseconds,Severity,CurrentInstructionCount,MaxInstructionCount,Message,Stack,Origin);}}public class BMyTextPanelAppender:BMyAppenderBase{List<string>k=new List<string>();List<IMyTextPanel>l=new List<IMyTextPanel>();public bool Autoscroll=true;public bool Prepend=false;public BMyTextPanelAppender(string a,Program b){b.GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(l,(c=>c.CustomName.Contains(a)));}public override void Enqueue(string a){k.Add(a);}public override void Flush(){foreach(var Panel in l){m(Panel);Panel.ShowTextureOnScreen();Panel.ShowPublicTextOnScreen();}k.Clear();}void m(IMyTextPanel a){if(Autoscroll){var b=new List<string>(a.GetPublicText().Split(new char[]{'\n','\r'},StringSplitOptions.RemoveEmptyEntries));b.AddRange(k);int c=Math.Min(n(a),b.Count);if(Prepend)b.Reverse();a.WritePublicText(string.Join("\n",b.GetRange(b.Count-c,c).ToArray()),false);}else if(Prepend){var b=new List<string>(a.GetPublicText().Split(new char[]{'\n','\r'},StringSplitOptions.RemoveEmptyEntries));b.AddRange(k);b.Reverse();a.WritePublicText(string.Join("\n",b.ToArray()),false);}else{a.WritePublicText(string.Join("\n",k.ToArray()),true);}}int n(IMyTextPanel a){float b=a.GetValueFloat("FontSize");if(b==0.0f)b=0.01f;return Convert.ToInt32(Math.Ceiling(17.0f/b));}}public class BMyKryptDebugSrvAppender:BMyAppenderBase{IMyProgrammableBlock k;Queue<string>l=new Queue<string>();public BMyKryptDebugSrvAppender(Program a){k=a.GridTerminalSystem.GetBlockWithName("DebugSrv")as IMyProgrammableBlock;}public override void Flush(){if(k!=null){var a=true;while(a&&l.Count>0)if(k.TryRun("L"+l.Peek())){l.Dequeue();}else{a=false;}}}public override void Enqueue(string a){l.Enqueue(a);}}public class BMyEchoAppender:BMyAppenderBase{Program k;public BMyEchoAppender(Program a){this.k=a;}public override void Flush(){}public override void Enqueue(string a){k.Echo(a);}}public class BMyCustomDataAppender:BMyAppenderBase{Program k;public BMyCustomDataAppender(Program a){this.k=a;this.k.Me.CustomData="";}public override void Enqueue(string a){k.Me.CustomData=k.Me.CustomData+'\n'+a;}public override void Flush(){}}public abstract class BMyAppenderBase{public abstract void Enqueue(string a);public abstract void Flush();}}
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}
 