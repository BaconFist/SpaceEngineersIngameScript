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

namespace Log4PB_TEST
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game


        /* 
    This this script does nothing by it's own. 
     This is a placeholder to keep trak of BMyLog4PB.
   

     Please go to http://forums.keenswh.com/threads/log4pb-logging-debugging-lib.7389240/ 
     for more information.                 
*/
        public void Main(string argument)
        {
            //initialize the logger (logging all kinds of messages to PB's CustomData)
            BMyLog4PB Log = new BMyLog4PB(this, BMyLog4PB.E_ALL, new BMyLog4PB.BMyCustomDataAppender(this));

            Log.Info("This message will appera in this PBs CustomData.");

            // output all the log messages
            Log.Flush();
        }

        public class BMyLog4PB { public const byte E_ALL = 63; public const byte E_TRACE = 32; public const byte E_DEBUG = 16; public const byte E_INFO = 8; public const byte E_WARN = 4; public const byte E_ERROR = 2; public const byte E_FATAL = 1; Dictionary<string, string> j = new Dictionary<string, string>() { { "{Date}", "{0}" }, { "{Time}", "{1}" }, { "{Milliseconds}", "{2}" }, { "{Severity}", "{3}" }, { "{CurrentInstructionCount}", "{4}" }, { "{MaxInstructionCount}", "{5}" }, { "{Message}", "{6}" }, { "{Stack}", "{7}" }, { "{Origin}", "{8}" } }; Stack<string> k = new Stack<string>(); public byte Filter; public readonly Dictionary<BMyAppenderBase, string> Appenders = new Dictionary<BMyAppenderBase, string>(); string l = @"[{0}-{1}/{2}][{3}][{4}/{5}][{8}][{7}] {6}"; string m = @"[{Date}-{Time}/{Milliseconds}][{Severity}][{CurrentInstructionCount}/{MaxInstructionCount}][{Origin}][{Stack}] {Message}"; public string Format { get { return m; } set { l = o(value); m = value; } } readonly Program n; public bool AutoFlush = true; public BMyLog4PB(Program a) : this(a, E_FATAL | E_ERROR | E_WARN | E_INFO, new BMyEchoAppender(a)) { } public BMyLog4PB(Program a, byte b, params BMyAppenderBase[] c) { Filter = b; this.n = a; foreach (var Appender in c) AddAppender(Appender); } string o(string a) { var b = a; foreach (var item in j) b = b?.Replace(item.Key, item.Value); return b; } public BMyLog4PB Flush() { foreach (var AppenderItem in Appenders) AppenderItem.Key.Flush(); return this; } public BMyLog4PB PushStack(string a) { k.Push(a); return this; } public string PopStack() { return (k.Count > 0) ? k.Pop() : null; } string p() { return (k.Count > 0) ? k.Peek() : null; } public string StackToString() { if (If(E_TRACE) != null) { string[] a = k.ToArray(); Array.Reverse(a); return string.Join(@"/", a); } else return p(); } public BMyLog4PB AddAppender(BMyAppenderBase a, string b = null) { if (!Appenders.ContainsKey(a)) Appenders.Add(a, o(b)); return this; } public BMyLog4PB If(byte a) { return ((a & Filter) != 0) ? this : null; } public BMyLog4PB Fatal(string a, params object[] b) { If(E_FATAL)?.q("FATAL", a, b); return this; } public BMyLog4PB Error(string a, params object[] b) { If(E_ERROR)?.q("ERROR", a, b); return this; } public BMyLog4PB Warn(string a, params object[] b) { If(E_WARN)?.q("WARN", a, b); return this; } public BMyLog4PB Info(string a, params object[] b) { If(E_INFO)?.q("INFO", a, b); return this; } public BMyLog4PB Debug(string a, params object[] b) { If(E_DEBUG).q("DEBUG", a, b); return this; } public BMyLog4PB Trace(string a, params object[] b) { If(E_TRACE)?.q("TRACE", a, b); return this; } void q(string a, string b, params object[] c) { DateTime d = DateTime.Now; r e = new r(d.ToShortDateString(), d.ToLongTimeString(), d.Millisecond.ToString(), a, n.Runtime.CurrentInstructionCount, n.Runtime.MaxInstructionCount, string.Format(b, c), StackToString(), n.Me.CustomName); foreach (var item in Appenders) { var f = (item.Value != null) ? item.Value : l; item.Key.Enqueue(e.ToString(f)); if (AutoFlush) item.Key.Flush(); } } class r { public string Date; public string Time; public string Milliseconds; public string Severity; public int CurrentInstructionCount; public int MaxInstructionCount; public string Message; public string Stack; public string Origin; public r(string a, string b, string c, string d, int e, int f, string g, string h, string i) { this.Date = a; this.Time = b; this.Milliseconds = c; this.Severity = d; this.CurrentInstructionCount = e; this.MaxInstructionCount = f; this.Message = g; this.Stack = h; this.Origin = i; } public override string ToString() { return ToString(@"{0},{1},{2},{3},{4},{5},{6},{7},{8}"); } public string ToString(string a) { return string.Format(a, Date, Time, Milliseconds, Severity, CurrentInstructionCount, MaxInstructionCount, Message, Stack, Origin); } } public class BMyTextPanelAppender : BMyAppenderBase { List<string> j = new List<string>(); List<IMyTextPanel> k = new List<IMyTextPanel>(); public bool Autoscroll = true; public bool Prepend = false; public BMyTextPanelAppender(string a, Program b) { b.GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(k, (c => c.CustomName.Contains(a))); } public override void Enqueue(string a) { j.Add(a); } public override void Flush() { foreach (var Panel in k) { l(Panel); Panel.ShowTextureOnScreen(); Panel.ShowPublicTextOnScreen(); } j.Clear(); } void l(IMyTextPanel a) { if (Autoscroll) { var b = new List<string>(a.GetPublicText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)); b.AddRange(j); int c = Math.Min(m(a), b.Count); if (Prepend) b.Reverse(); a.WritePublicText(string.Join("\n", b.GetRange(b.Count - c, c).ToArray()), false); } else if (Prepend) { var b = new List<string>(a.GetPublicText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)); b.AddRange(j); b.Reverse(); a.WritePublicText(string.Join("\n", b.ToArray()), false); } else { a.WritePublicText(string.Join("\n", j.ToArray()), true); } } int m(IMyTextPanel a) { float b = a.GetValueFloat("FontSize"); if (b == 0.0f) b = 0.01f; return Convert.ToInt32(Math.Ceiling(17.0f / b)); } } public class BMyKryptDebugSrvAppender : BMyAppenderBase { IMyProgrammableBlock j; Queue<string> k = new Queue<string>(); public BMyKryptDebugSrvAppender(Program a) { j = a.GridTerminalSystem.GetBlockWithName("DebugSrv") as IMyProgrammableBlock; } public override void Flush() { if (j != null) { var a = true; while (a && k.Count > 0) if (j.TryRun("L" + k.Peek())) { k.Dequeue(); } else { a = false; } } } public override void Enqueue(string a) { k.Enqueue(a); } } public class BMyEchoAppender : BMyAppenderBase { Program j; public BMyEchoAppender(Program a) { this.j = a; } public override void Flush() { } public override void Enqueue(string a) { j.Echo(a); } } public class BMyCustomDataAppender : BMyAppenderBase { Program j; public BMyCustomDataAppender(Program a) { this.j = a; this.j.Me.CustomData = ""; } public override void Enqueue(string a) { j.Me.CustomData = j.Me.CustomData + '\n' + a; } public override void Flush() { } } public abstract class BMyAppenderBase { public abstract void Enqueue(string a); public abstract void Flush(); } }

        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}