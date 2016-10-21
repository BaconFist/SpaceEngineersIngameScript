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

namespace Snippet_BaconDebug
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        snippet_BaconDebug
        ==============
        Copyright 2016 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE

        
        */

        public class BaconDebug
        {
            public const int OFF = 0;
            public const int FATAL = 1;
            public const int ERROR = 2;
            public const int WARN = 3;
            public const int INFO = 4;
            public const int DEBUG = 5;
            public const int TRACE = 6;

            private Dictionary<int, string> VerbosityLabels = new Dictionary<int, string>() {
                {OFF,"OFF" },
                {FATAL,"FATAL"},
                {ERROR,"ERROR"},
                {WARN,"WARN"},
                {INFO,"INFO"},
                {DEBUG,"DEBUG"},
                {TRACE,"TRACE"},
            };

            private const int CHARACTER_LIMIT = 100000; //limitied by the Game itself.
            private List<IMyTextPanel> Panels = new List<IMyTextPanel>();
            private MyGridProgram GridProgram;
            private List<KeyValuePair<string, long>> callsStack = new List<KeyValuePair<string, long>>();
            private int verbosity = OFF;
            public string Format = @"[{0}-{1}.{2}][{3}][{4}][IC {5}/{6}] {7}";
            private bool _autoscroll = true;

            public int remainingInstructions {
                get { return GridProgram.Runtime.MaxInstructionCount - GridProgram.Runtime.CurrentInstructionCount; }
            }

            public bool autoscroll
            {
                get { return _autoscroll; }
                set { _autoscroll = value; }
            }

            public void clearPanels()
            {
                for(int i = 0; i < Panels.Count; i++)
                {
                    Panels[i].WritePublicText("");
                }
            }

            public BaconDebug(string tag, IMyGridTerminalSystem GTS, MyGridProgram PB, int verbosity, string debuggerName = "BaconDebug")
            {
                this.verbosity = verbosity;
                List<IMyTerminalBlock> slug = new List<IMyTerminalBlock>();
                GTS.GetBlocksOfType<IMyTextPanel>(slug, ((IMyTerminalBlock x) => x.CustomName.Contains(tag) && x.CubeGrid.Equals(PB.Me.CubeGrid)));
                Panels = slug.ConvertAll<IMyTextPanel>(x => x as IMyTextPanel);
                this.GridProgram = PB;
                newScope(debuggerName);
            }

            public int getVerbosity()
            {
                return verbosity;
            }

            public MyGridProgram getGridProgram()
            {
                return this.GridProgram;
            }
            

            public void newScope(string sender)
            {
                callsStack.Add(new KeyValuePair<string, long>(sender, DateTime.Now.Ticks));
                if (this.verbosity.Equals(TRACE))
                {
                    this.Trace("STEP INTO SCOPE");
                }
            }

            public void leaveScope()
            {
                if(callsStack.Count > 0 && this.verbosity.Equals(TRACE)){
                    this.Trace("LEAVE SCOPE ({0} Ticks)", getTimeDiffInTicks(callsStack[callsStack.Count - 1].Value));
                }
                if (callsStack.Count > 1)
                {
                    callsStack.RemoveAt(callsStack.Count - 1);
                }
            }
            public string getSender()
            {
                if(callsStack.Count > 0)
                {
                    if (this.verbosity.Equals(TRACE))
                    {
                        List<string> stack = new List<string>();
                        foreach(KeyValuePair<string,long> entry in callsStack)
                        {
                            stack.Add(entry.Key);
                        }
                        return string.Join(">", stack.ToArray());
                    } else
                    {
                        return callsStack[callsStack.Count - 1].Key;
                    }
                }
                return "NO SCOPE DEFINED";
            }

            private double getTimeDiffInTicks(long start)
            {
                long stop = DateTime.Now.Ticks;
                return (Math.Max(start, stop) - Math.Min(start, stop));
            }

            public void Fatal(string msg, params object[] values)
            {
                Fatal(string.Format(msg, values));
            }

            public void Error(string msg, params object[] values)
            {
                Error(string.Format(msg, values));
            }

            public void Warn(string msg, params object[] values)
            {
                Warn(string.Format(msg, values));
            }

            public void Info(string msg, params object[] values)
            {
                Info(string.Format(msg, values));
            }

            public void Debug(string msg, params object[] values)
            {
                Debug(string.Format(msg, values));
            }

            public void Trace(string msg, params object[] values)
            {
                Trace(string.Format(msg, values));
            }

            public void Fatal(string msg)
            {
                add(msg, FATAL);
            }
                     
            public void Error(string msg)
            {
                add(msg, ERROR);
            }

            public void Warn(string msg)
            {
                add(msg, WARN);
            }

            public void Info(string msg)
            {
                add(msg, INFO);
            }

            public void Debug(string msg)
            {
                add(msg, DEBUG);
            }

            public void Trace(string msg)
            {
                add(msg, TRACE);
            }

            public void add(string msg, int verbosity)
            {

                DisplayOnProgramableBlock(msg, verbosity);
                if (verbosity <= this.verbosity)
                {
                    string newLogEntry = FormatEntry(msg, verbosity);
                    for (int i = 0; i < Panels.Count; i++)
                    {
                        List<string> PublicText = new List<string>();
                        PublicText.AddRange(Panels[i].GetPublicText().Trim().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
                        PublicText.Add(newLogEntry);
                        TrimLines(ref PublicText, getMaxLines(Panels[i]));
                        string NewPublicTextContentChars = string.Join("\n", PublicText.ToArray());
                        TrimTextPanelMax(ref NewPublicTextContentChars);
                        Panels[i].WritePublicText(NewPublicTextContentChars);
                    }
                }
            }

            private void DisplayOnProgramableBlock(string msg, int verbosity)
            {
                if (verbosity <= ERROR)
                {
                    GridProgram.Echo(msg);
                }
            }

            private void TrimTextPanelMax(ref string Text)
            {
                if(CHARACTER_LIMIT < Text.Length)
                {
                    Text = Text.Substring(Text.Length - CHARACTER_LIMIT);
                    int indexOfNextLinebreak = Text.IndexOf('\n');
                    Text = Text.Substring(Text.Length - indexOfNextLinebreak).TrimStart(new char[] {'\n','\r' });  
                }
            }

            private void TrimLines(ref List<string> PublicText, int limit)
            {
                if (autoscroll && 0 < limit && limit < PublicText.Count)
                {
                    PublicText.RemoveRange(0,PublicText.Count-limit);
                }
            }



            private int getMaxLines(IMyTextPanel Panel)
            {
                float fontSize =Panel.GetValueFloat("FontSize");
                if(fontSize == 0.0f)
                {
                    fontSize = 0.01f;
                }

                return Convert.ToInt32(Math.Ceiling(17.0f / fontSize));
            }

            private string FormatEntry(string msg, int verbosity)
            {
                DateTime Time = DateTime.Now;
                object[] values = new object[] {
                    Time.ToShortDateString(), //0
                    Time.ToShortTimeString(), //1
                    Time.Millisecond.ToString().TrimStart('0'), // 2
                    getVerbosityLabel(verbosity), // 3
                    getSender(), // 4
                    GridProgram.Runtime.CurrentInstructionCount, // 5
                    GridProgram.Runtime.MaxInstructionCount, // 6
                    msg // 7
                };

                return string.Format(Format, values);
            }

            private string getVerbosityLabel(int verbosity)
            {
                if (VerbosityLabels.ContainsKey(verbosity))
                {
                    return VerbosityLabels[verbosity];
                }
                return string.Format("`{0}`", verbosity);
            }
        }
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game

        public class min
        {
            public class BaconDebug { public const int OFF = 0; public const int FATAL = 1; public const int ERROR = 2; public const int WARN = 3; public const int INFO = 4; public const int DEBUG = 5; public const int TRACE = 6; Dictionary<int, string> h = new Dictionary<int, string>() { { OFF, "OFF" }, { FATAL, "FATAL" }, { ERROR, "ERROR" }, { WARN, "WARN" }, { INFO, "INFO" }, { DEBUG, "DEBUG" }, { TRACE, "TRACE" }, }; List<IMyTextPanel> j = new List<IMyTextPanel>(); MyGridProgram k; List<KeyValuePair<string, long>> l = new List<KeyValuePair<string, long>>(); int m = OFF; public string Format = @"[{0}-{1}.{2}][{3}][{4}][IC {5}/{6}] {7}"; bool n = true; public int remainingInstructions { get { return k.Runtime.MaxInstructionCount - k.Runtime.CurrentInstructionCount; } } public bool autoscroll { get { return n; } set { n = value; } } public void clearPanels() { for (int a = 0; a < j.Count; a++) j[a].WritePublicText(""); } public BaconDebug(string a, IMyGridTerminalSystem b, MyGridProgram c, int d, string e = "BaconDebug") { this.m = d; var f = new List<IMyTerminalBlock>(); b.GetBlocksOfType<IMyTextPanel>(f, ((IMyTerminalBlock g) => g.CustomName.Contains(a) && g.CubeGrid.Equals(c.Me.CubeGrid))); j = f.ConvertAll<IMyTextPanel>(g => g as IMyTextPanel); this.k = c; newScope(e); } public int getVerbosity() { return m; } public MyGridProgram getGridProgram() { return this.k; } public void newScope(string a) { l.Add(new KeyValuePair<string, long>(a, DateTime.Now.Ticks)); if (this.m.Equals(TRACE)) this.Trace("STEP INTO SCOPE"); } public void leaveScope() { if (l.Count > 0 && this.m.Equals(TRACE)) this.Trace("LEAVE SCOPE ({0} Ticks)", o(l[l.Count - 1].Value)); if (l.Count > 1) l.RemoveAt(l.Count - 1); } public string getSender() { if (l.Count > 0) if (this.m.Equals(TRACE)) { List<string> a = new List<string>(); foreach (KeyValuePair<string, long> entry in l) { a.Add(entry.Key); } return string.Join(">", a.ToArray()); } else { return l[l.Count - 1].Key; } return "NO SCOPE DEFINED"; } double o(long a) { long b = DateTime.Now.Ticks; return (Math.Max(a, b) - Math.Min(a, b)); } public void Fatal(string a, params object[] b) { Fatal(string.Format(a, b)); } public void Error(string a, params object[] b) { Error(string.Format(a, b)); } public void Warn(string a, params object[] b) { Warn(string.Format(a, b)); } public void Info(string a, params object[] b) { Info(string.Format(a, b)); } public void Debug(string a, params object[] b) { Debug(string.Format(a, b)); } public void Trace(string a, params object[] b) { Trace(string.Format(a, b)); } public void Fatal(string a) { add(a, FATAL); } public void Error(string a) { add(a, ERROR); } public void Warn(string a) { add(a, WARN); } public void Info(string a) { add(a, INFO); } public void Debug(string a) { add(a, DEBUG); } public void Trace(string a) { add(a, TRACE); } public void add(string a, int b) { p(a, b); if (b <= this.m) { var c = t(a, b); for (int d = 0; d < j.Count; d++) { var e = new List<string>(); e.AddRange(j[d].GetPublicText().Trim().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)); e.Add(c); r(ref e, s(j[d])); var f = string.Join("\n", e.ToArray()); q(ref f); j[d].WritePublicText(f); } } } void p(string a, int b) { if (b <= ERROR) k.Echo(a); } void q(ref string a) { if (100000 < a.Length) { a = a.Substring(a.Length - 100000); int b = a.IndexOf('\n'); a = a.Substring(a.Length - b).TrimStart(new char[] { '\n', '\r' }); } } void r(ref List<string> a, int b) { if (autoscroll && 0 < b && b < a.Count) a.RemoveRange(0, a.Count - b); } int s(IMyTextPanel a) { float b = a.GetValueFloat("FontSize"); if (b == 0.0f) b = 0.01f; return Convert.ToInt32(Math.Ceiling(17.0f / b)); } string t(string a, int b) { DateTime c = DateTime.Now; object[] d = new object[] { c.ToShortDateString(), c.ToShortTimeString(), c.Millisecond.ToString().TrimStart('0'), u(b), getSender(), k.Runtime.CurrentInstructionCount, k.Runtime.MaxInstructionCount, a }; return string.Format(Format, d); } string u(int a) { if (h.ContainsKey(a)) return h[a]; return string.Format("`{0}`", a); } }
        }
    }
}