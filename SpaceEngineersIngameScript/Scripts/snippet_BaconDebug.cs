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

        Description
        ===========
            argument:  lcdtag originname message~----
        */

        public class BaconDebug
        {
            private List<IMyTextPanel> Panels = new List<IMyTextPanel>();
            private MyGridProgram PB;
            private List<string> callsStack = new List<string>();
            private bool enabled = false;

            public BaconDebug(string tag, IMyGridTerminalSystem GTS, MyGridProgram PB, bool enabled)
            {
                List<IMyTerminalBlock> slug = new List<IMyTerminalBlock>();
                GTS.GetBlocksOfType<IMyTextPanel>(slug, ((IMyTerminalBlock x) => x.CustomName.Contains(tag) && x.CubeGrid.Equals(PB.Me.CubeGrid)));
                Panels = slug.ConvertAll<IMyTextPanel>(x => x as IMyTextPanel);
                this.PB = PB;
                putSender("BaconDebug");
                setEnabled(enabled);                
            }

            private void setEnabled(bool e)
            {
                this.enabled = e;
            }

            private void updatePanels(string tag, IMyGridTerminalSystem GTS)
            {
                
            }

            public void putSender(string sender)
            {
                callsStack.Add(sender);
            }

            public void pullSender()
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

            public void add(string msg)
            {
                if (enabled)
                {
                    for (int i = 0; i < Panels.Count; i++)
                    {
                        List<string> lines = new List<string>();
                        lines.AddRange(Panels[i].GetPublicText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
                        StringBuilder sb = new StringBuilder();
                        lines.Add(buildLine(msg));
                        if (!Panels[i].GetPrivateTitle().ToLower().Equals("nolinelimit"))
                        {
                            int maxLines = getMaxLines(Panels[i]);
                            if (lines.Count > maxLines)
                            {
                                lines.RemoveRange(0, lines.Count - maxLines);
                            }
                        }
                        Panels[i].WritePublicText(string.Join("\n", lines));
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
                slug.Append("[" + DateTime.Now.ToShortTimeString() + "]");
                slug.Append("[" + getSender() + "]");
                slug.Append("[IC " + PB.Runtime.CurrentInstructionCount + "/" + PB.Runtime.MaxInstructionCount + "]");
                slug.Append("[MCC " + PB.Runtime.CurrentMethodCallCount + "/" + PB.Runtime.MaxMethodCallCount + "]");
                slug.Append(" " + msg);

                return slug.ToString();
            }
        }
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game

        public class min
        {
            public class BaconDebug { List<IMyTextPanel> g = new List<IMyTextPanel>(); MyGridProgram h; List<string> i = new List<string>(); bool j = false; public BaconDebug(string a, IMyGridTerminalSystem b, MyGridProgram c, bool d) { var e = new List<IMyTerminalBlock>(); b.GetBlocksOfType<IMyTextPanel>(e, ((IMyTerminalBlock f) => f.CustomName.Contains(a) && f.CubeGrid.Equals(c.Me.CubeGrid))); g = e.ConvertAll<IMyTextPanel>(f => f as IMyTextPanel); this.h = c; putSender("BaconDebug"); k(d); } void k(bool a) { this.j = a; } void l(string a, IMyGridTerminalSystem b) { } public void putSender(string a) { i.Add(a); } public void pullSender() { if (i.Count > 1) i.RemoveAt(i.Count - 1); } public string getSender() { return i[i.Count - 1]; } public void add(string a) { if (j) for (int b = 0; b < g.Count; b++) { List<string> c = new List<string>(); c.AddRange(g[b].GetPublicText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)); StringBuilder d = new StringBuilder(); c.Add(n(a)); if (!g[b].GetPrivateTitle().ToLower().Equals("nolinelimit")) { int e = m(g[b]); if (c.Count > e) { c.RemoveRange(0, c.Count - e); } } g[b].WritePublicText(string.Join("\n", c)); } } int m(IMyTextPanel a) { float b = a.GetValueFloat("FontSize"); if (b == 0.0f) b = 0.01f; return Convert.ToInt32(Math.Ceiling(17.0f / b)); } string n(string a) { var b = new StringBuilder(); b.Append("[" + DateTime.Now.ToShortTimeString() + "]"); b.Append("[" + getSender() + "]"); b.Append("[IC " + h.Runtime.CurrentInstructionCount + "/" + h.Runtime.MaxInstructionCount + "]"); b.Append("[MCC " + h.Runtime.CurrentMethodCallCount + "/" + h.Runtime.MaxMethodCallCount + "]"); b.Append(" " + a); return b.ToString(); } }
        }
    }
}