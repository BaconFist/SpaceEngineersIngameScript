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

namespace snippet_BaconDebug
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
            public const int INFO = 3;
            public const int WARN = 2;
            public const int ERROR = 1;
            public const int DEBUG = 4;

            private List<IMyTextPanel> Panels = new List<IMyTextPanel>();
            private MyGridProgram GridProgram;
            private List<string> callsStack = new List<string>();
            private int verbosity = 0;
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

            public BaconDebug(string tag, IMyGridTerminalSystem GTS, MyGridProgram PB, int verbosity)
            {
                this.verbosity = verbosity;
                List<IMyTerminalBlock> slug = new List<IMyTerminalBlock>();
                GTS.GetBlocksOfType<IMyTextPanel>(slug, ((IMyTerminalBlock x) => x.CustomName.Contains(tag) && x.CubeGrid.Equals(PB.Me.CubeGrid)));
                Panels = slug.ConvertAll<IMyTextPanel>(x => x as IMyTextPanel);
                this.GridProgram = PB;
                newScope("BaconDebug");
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
                callsStack.Add(sender);
            }

            public void leaveScope()
            {
                if (callsStack.Count > 1)
                {
                    callsStack.RemoveAt(callsStack.Count - 1);
                }
            }
            public string getSender()
            {
                return callsStack[callsStack.Count-1];
            }

            public void add(string msg, int verbosity)
            {
                if (verbosity <= this.verbosity)
                {
                    string debugMsg = buildLine(msg);
                    if (verbosity == ERROR)
                    {
                        GridProgram.Echo(debugMsg);
                    }
                    for (int i = 0; i < Panels.Count; i++)
                    {
                        if (autoscroll)
                        {
                            List<string> lines = new List<string>();
                            lines.AddRange(Panels[i].GetPublicText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
                            StringBuilder sb = new StringBuilder();
                            lines.Add(debugMsg);
                            if (!Panels[i].GetPrivateTitle().ToLower().Equals("nolinelimit"))
                            {
                                int maxLines = getMaxLines(Panels[i]);
                                if (lines.Count > maxLines)
                                {
                                    lines.RemoveRange(0, lines.Count - maxLines);
                                }
                            }
                            Panels[i].WritePublicText(string.Join("\n", lines));
                        } else
                        {
                            Panels[i].WritePublicText(debugMsg + '\n', true);
                        }
                    }
                }
            }

            private int getMaxLines(IMyTextPanel Panel)
            {
                float fs =Panel.GetValueFloat("FontSize");
                if(fs == 0.0f)
                {
                    fs = 0.01f;
                }

                return Convert.ToInt32(Math.Ceiling(17.0f / fs));
            }

            private string buildLine(string msg)
            {
                StringBuilder slug = new StringBuilder();


                slug.Append("[" + DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToShortTimeString() + "." + DateTime.Now.Millisecond.ToString().TrimStart('0') + "]");
                slug.Append("[" + getSender() + "]");
                slug.Append("[IC " + GridProgram.Runtime.CurrentInstructionCount + "/" + GridProgram.Runtime.MaxInstructionCount + "]");
                slug.Append(" " + msg);

                return slug.ToString();
            }
        }
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game

        public class min
        {
            public class BaconDebug { public const int INFO = 3; public const int WARN = 2; public const int ERROR = 1; public const int DEBUG = 4; List<IMyTextPanel> h = new List<IMyTextPanel>(); MyGridProgram i; List<string> j = new List<string>(); int k = 0; bool l = true; public int remainingInstructions { get { return i.Runtime.MaxInstructionCount - i.Runtime.CurrentInstructionCount; } } public bool autoscroll { get { return l; } set { l = value; } } public void clearPanels() { for (int a = 0; a < h.Count; a++) h[a].WritePublicText(""); } public BaconDebug(string a, IMyGridTerminalSystem b, MyGridProgram c, int d) { this.k = d; var e = new List<IMyTerminalBlock>(); b.GetBlocksOfType<IMyTextPanel>(e, ((IMyTerminalBlock f) => f.CustomName.Contains(a) && f.CubeGrid.Equals(c.Me.CubeGrid))); h = e.ConvertAll<IMyTextPanel>(f => f as IMyTextPanel); this.i = c; newScope("BaconDebug"); } public int getVerbosity() { return k; } public MyGridProgram getGridProgram() { return this.i; } public void newScope(string a) { j.Add(a); } public void leaveScope() { if (j.Count > 1) j.RemoveAt(j.Count - 1); } public string getSender() { return j[j.Count - 1]; } public void add(string a, int b) { if (b <= this.k) { var c = n(a); if (b == ERROR) i.Echo(c); for (int d = 0; d < h.Count; d++) if (autoscroll) { List<string> e = new List<string>(); e.AddRange(h[d].GetPublicText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)); StringBuilder f = new StringBuilder(); e.Add(c); if (!h[d].GetPrivateTitle().ToLower().Equals("nolinelimit")) { int g = m(h[d]); if (e.Count > g) { e.RemoveRange(0, e.Count - g); } } h[d].WritePublicText(string.Join("\n", e)); } else { h[d].WritePublicText(c + '\n', true); } } } int m(IMyTextPanel a) { float b = a.GetValueFloat("FontSize"); if (b == 0.0f) b = 0.01f; return Convert.ToInt32(Math.Ceiling(17.0f / b)); } string n(string a) { var b = new StringBuilder(); b.Append("[" + DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToShortTimeString() + "." + DateTime.Now.Millisecond.ToString().TrimStart('0') + "]"); b.Append("[" + getSender() + "]"); b.Append("[IC " + i.Runtime.CurrentInstructionCount + "/" + i.Runtime.MaxInstructionCount + "]"); b.Append(" " + a); return b.ToString(); } }
        }
    }
}