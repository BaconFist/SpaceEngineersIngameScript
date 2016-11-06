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

namespace BaconsWingmanDrone
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        BaconsWingmanDrone
        ==============
        Copyright 2016 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE

        Description
        ===========

        */

        List<IMyRemoteControl> RCs;
        BaconArgs Args;
        System.Text.RegularExpressions.Regex TagRgx = new System.Text.RegularExpressions.Regex(@"\[BWD(:(-?\d+)){3}\]");

        public void Main(string argument)
        {

            if (RCs == null)
            {
                RCs = new List<IMyRemoteControl>();
            }
            Args = BaconArgs.parse(argument);
            if(Args.getOption("reset").Count > 0)
            {
                reset();
            }
            List<IMyShipController> ShipCon = new List<IMyShipController>();
            GridTerminalSystem.GetBlocksOfType<IMyShipController>(ShipCon, (s => s.CustomName.Contains("[BWDANCHOR]")));
            if (ShipCon.Count > 0)
            {
                foreach (IMyRemoteControl Remote in RCs)
                {
                    updateDrone(Remote, ShipCon[0]);
                }
            }
        }

        void reset()
        {
            RCs.Clear();
            GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(RCs, (r => TagRgx.IsMatch(r.CustomName)));
        }

        void updateDrone(IMyRemoteControl Remote, IMyShipController Anchor)
        {
            System.Text.RegularExpressions.MatchCollection Matches = TagRgx.Matches(Remote.CustomName);

            if (Matches.Count == 3)
            {
                if (Remote.CubeGrid.Equals(Me.CubeGrid))
                {
                    Echo(string.Format(@"Drone ""{0}"" on Master-Grid -> skip", Remote.CustomName));
                    Remote.SetAutoPilotEnabled(false);
                    return;
                }

                int x;
                int y;
                int z;
                if (int.TryParse(Matches[0].Value, out x) && int.TryParse(Matches[1].Value, out y) && int.TryParse(Matches[2].Value, out z))
                {
                    Vector3 newPos = getPosRel(Anchor, new Vector3I(x, y, z));
                    Remote.ClearWaypoints();
                    Remote.AddWaypoint(newPos, "Wingman Target");
                    Remote.SetAutoPilotEnabled(true);
                }                
            }
        }

        Vector3D getPosRel(IMyCubeBlock anchor, Vector3I offset)
        {
            //1) Get world position
            Vector3D basePosition = anchor.GetPosition();

            //2) Get world direction vectors
            Vector3D upVector = anchor.WorldMatrix.Up;
            Vector3D leftVector = anchor.WorldMatrix.Left;
            Vector3D backwardVector = anchor.WorldMatrix.Backward;

            //Relative distances
            int leftDistance = offset.X;
            int upDistance = offset.Y;
            int backDistance = offset.Z;

            //3) Calc target position
            return basePosition + upVector * upDistance + backwardVector * backDistance + leftVector * leftDistance;
        }

        #region BaconArgs
        public class BaconArgs { public string InputData; static public BaconArgs parse(string a) { return (new Parser()).parseArgs(a); } static public string Escape(object a) { return string.Format("{0}", a).Replace(@"\", @"\\").Replace(@" ", @"\ ").Replace(@"""", @"\"""); } static public string UnEscape(string a) { return a.Replace(@"\""", @"""").Replace(@"\ ", @" ").Replace(@"\\", @"\"); } public class Parser { static Dictionary<string, BaconArgs> h = new Dictionary<string, BaconArgs>(); public BaconArgs parseArgs(string a) { if (!h.ContainsKey(a)) { var b = new BaconArgs(); b.InputData = a; var c = false; var d = false; var e = new StringBuilder(); for (int f = 0; f < a.Length; f++) { var g = a[f]; if (c) { e.Append(g); c = false; } else if (g.Equals('\\')) c = true; else if (d && !g.Equals('"')) e.Append(g); else if (g.Equals('"')) d = !d; else if (g.Equals(' ')) if (e.ToString().Equals("--")) { b.add(a.Substring(f).TrimStart()); e.Clear(); break; } else { b.add(e.ToString()); e.Clear(); } else e.Append(g); } if (e.Length > 0) b.add(e.ToString()); h.Add(a, b); } return h[a]; } } protected Dictionary<char, int> h = new Dictionary<char, int>(); protected List<string> i = new List<string>(); protected Dictionary<string, List<string>> j = new Dictionary<string, List<string>>(); public List<string> getArguments() { return i; } public int getFlag(char a) { return h.ContainsKey(a) ? h[a] : 0; } public List<string> getOption(string a) { return j.ContainsKey(a) ? j[a] : new List<string>(); } public void add(string a) { if (a.Trim().Length > 0) if (!a.StartsWith("-")) { i.Add(a); } else if (a.StartsWith("--")) { KeyValuePair<string, string> b = k(a); string c = b.Key.Substring(2); if (!j.ContainsKey(c)) { j.Add(c, new List<string>()); } j[c].Add(b.Value); } else { string b = a.Substring(1); for (int d = 0; d < b.Length; d++) { if (this.h.ContainsKey(b[d])) { this.h[b[d]]++; } else { this.h.Add(b[d], 1); } } } } KeyValuePair<string, string> k(string a) { string[] b = a.Split(new char[] { '=' }, 2); return new KeyValuePair<string, string>(b[0], (b.Length > 1) ? b[1] : null); } public string ToArguments() { var a = new List<string>(); foreach (string argument in this.getArguments()) a.Add(Escape(argument)); foreach (KeyValuePair<string, List<string>> option in this.j) { var b = "--" + Escape(option.Key); foreach (string optVal in option.Value) a.Add(b + ((optVal != null) ? "=" + Escape(optVal) : "")); } var c = (h.Count > 0) ? "-" : ""; foreach (KeyValuePair<char, int> flag in h) c += new String(flag.Key, flag.Value); a.Add(c); return String.Join(" ", a.ToArray()); } override public string ToString() { var a = new List<string>(); foreach (string key in j.Keys) a.Add(l(key) + ":[" + string.Join(",", j[key].ConvertAll<string>(b => l(b)).ToArray()) + "]"); var c = new List<string>(); foreach (char key in h.Keys) c.Add(key + ":" + h[key].ToString()); var d = new StringBuilder(); d.Append("{\"a\":["); d.Append(string.Join(",", i.ConvertAll<string>(b => l(b)).ToArray())); d.Append("],\"o\":[{"); d.Append(string.Join("},{", a)); d.Append("}],\"f\":[{"); d.Append(string.Join("},{", c)); d.Append("}]}"); return d.ToString(); } string l(string a) { return (a != null) ? "\"" + a.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" : @"null"; } }
        #endregion BaconArgs

        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}