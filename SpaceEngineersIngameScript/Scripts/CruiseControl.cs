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

namespace CruiseControl
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game 

        /** 
        CruiseControl 
        ============== 
        Copyright 2017 Thomas Klose <thomas@bratler.net> 
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE 
 
        Description 
        =========== 
 
        */

        const string LCD_TAG = "[BCC]";

        const string OPT_DEADZONE = "DeadZone";
        const string OPT_THRUSTMULTI = "ThrustMultiplicator";
        const string OPT_INC = "inc";
        const string OPT_DEC = "dec";

        const string PROPETY_OVERRIDE = "Override";

        BaconArgs Args;
        IMyShipController lastUsedController = null;

        float thrustMultiplicator = 3;
        float deadZone = 0.25f;
        float targetSpeed = 0;

        float currentShipSpeed = 0;
        IMyShipController shipController;

        List<IMyThrust> AcceleratoinThruster = new List<IMyThrust>();
        List<IMyThrust> DeceleratoinThruster = new List<IMyThrust>();

        public void Main(string argument)
        {
            run(argument);
        }

        private void resetThrusters()
        {
            List<IMyThrust> Thrustes = new List<IMyThrust>();
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(Thrustes);
            BatchApplyThrustOverride(Thrustes, 0f);
            AcceleratoinThruster = new List<IMyThrust>();
            DeceleratoinThruster = new List<IMyThrust>();
        }

        private StringBuilder getHelptext()
        {
            StringBuilder help = new StringBuilder();
            help.AppendLine(string.Format(@"HELP:"));
            help.AppendLine(string.Format(@"* set target speed by passing it as an argument."));
            help.AppendLine(string.Format(@"* increase target speed with --{0}=NUMBER.", OPT_INC));
            help.AppendLine(string.Format(@"* decerase target speed with --{0}=NUMBER.", OPT_DEC));
            help.AppendLine(string.Format(@"* set DeadZone with --{0}=NUMBER", OPT_DEADZONE));
            help.AppendLine(string.Format(@"* set Thrust multiplicator with --{0}=NUMBER", OPT_THRUSTMULTI));
            help.AppendLine(string.Format(@"All settings will be saved until the script is recompiled."));
            return help;
        }

        private void run(string argument)
        {
            Args = BaconArgs.parse(argument);
            UpdateSettingFromArguments();


            // update speed overrides 
            UpdateForwardVelocityOverride();

            StringBuilder Info = getInfo();
            StringBuilder Help = new StringBuilder(Info.ToString());
            Help.Append(getHelptext().ToString());
            Me.CustomData = Help.ToString();
            Echo(Info.ToString());
            List<IMyTextPanel> TextPanels = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(TextPanels, (p => p.CubeGrid.Equals(Me.CubeGrid) && p.CustomName.Contains(LCD_TAG)));
            foreach (IMyTextPanel Panel in TextPanels)
            {
                Panel.WritePublicText(Info.ToString());
                Panel.ShowPublicTextOnScreen();
            }

            lastUsedController = shipController;
        }

        private StringBuilder getInfo()
        {
            StringBuilder lcdText = new StringBuilder();
            lcdText.AppendLine(string.Format(@"Forward speed: {0} m/s", currentShipSpeed));
            lcdText.AppendLine(string.Format(@"Target speed: {0} m/s", targetSpeed));
            lcdText.AppendLine(string.Format(@"DeadZone: {0} m/s", deadZone));
            lcdText.AppendLine(string.Format(@"Thrust multiplicator: {0}", thrustMultiplicator));
            return lcdText;
        }

        private void UpdateSettingFromArguments()
        {
            // update target Speed 
            float newSpeedTargetBuffer = 0;

            if (Args.hasArguments() && float.TryParse(Args.getArguments()[0], out newSpeedTargetBuffer))
            {
                targetSpeed = newSpeedTargetBuffer;
            }

            if (Args.hasOption(OPT_INC) && float.TryParse(Args.getOption(OPT_INC)[0], out newSpeedTargetBuffer))
            {
                targetSpeed += newSpeedTargetBuffer;
            }

            if (Args.hasOption(OPT_DEC) && float.TryParse(Args.getOption(OPT_DEC)[0], out newSpeedTargetBuffer))
            {
                targetSpeed -= newSpeedTargetBuffer;
            }

            //update deadZone 
            float newDeadZoneBuffer = 0;
            if (Args.hasOption(OPT_DEADZONE) && float.TryParse(Args.getOption(OPT_DEADZONE)[0], out newDeadZoneBuffer))
            {
                deadZone = newDeadZoneBuffer;
            }

            //update thrust multiplicator 
            float newThrustMultiplicatorBuffer = 0f;
            if (Args.hasOption(OPT_THRUSTMULTI) && float.TryParse(Args.getOption(OPT_THRUSTMULTI)[0], out newThrustMultiplicatorBuffer))
            {
                thrustMultiplicator = newThrustMultiplicatorBuffer;
            }
        }

        private void UpdateForwardVelocityOverride()
        {

            if (TryGetUsedController(out shipController))
            {
                if (!shipController.Equals(lastUsedController))
                {
                    resetThrusters();
                }

                List<IMyThrust> ThrustersAcceleration = findThrusters(GetLocalMatrix(shipController).Forward);
                List<IMyThrust> ThrustersDecelartion = findThrusters(GetLocalMatrix(shipController).Backward);

                currentShipSpeed = Vector3.TransformNormal(shipController.GetShipVelocities().LinearVelocity, Matrix.Transpose(shipController.WorldMatrix)).Z * -1;
                float speedDiff = Math.Abs(targetSpeed - currentShipSpeed);

                if (targetSpeed == 0)
                {
                    BatchApplyThrustOverride(ThrustersAcceleration, 0f);
                    BatchApplyThrustOverride(ThrustersDecelartion, 0f);
                }
                else if (deadZone < speedDiff)
                {
                    float newOverrideValue = Math.Max(5f, Math.Min(100f, speedDiff * thrustMultiplicator));
                    if (targetSpeed < currentShipSpeed)
                    {
                        //decelerate 
                        BatchApplyThrustOverride(ThrustersAcceleration, 0f);
                        BatchApplyThrustOverride(ThrustersDecelartion, newOverrideValue);
                    }
                    else if (currentShipSpeed < targetSpeed)
                    {
                        //accelerate 
                        BatchApplyThrustOverride(ThrustersDecelartion, 0f);
                        BatchApplyThrustOverride(ThrustersAcceleration, newOverrideValue);

                    }
                }
            }
        }

        private void BatchApplyThrustOverride(List<IMyThrust> Thrusters, float value)
        {
            foreach (IMyThrust Thrust in Thrusters)
            {
                Thrust.SetValueFloat(PROPETY_OVERRIDE, value);
            }
        }

        private List<IMyThrust> findThrusters(Vector3 direction)
        {
            List<IMyThrust> Matches = new List<IMyThrust>();
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(
                Matches,
                (t =>
                    t.CubeGrid.Equals(Me.CubeGrid)
                    && GetLocalMatrix(t).Backward.Equals(direction)
                )
            );

            return Matches;
        }

        private bool TryGetUsedController(out IMyShipController controller)
        {
            List<IMyShipController> buffer = new List<IMyShipController>();
            GridTerminalSystem.GetBlocksOfType<IMyShipController>(buffer, (c => c.CubeGrid.Equals(Me.CubeGrid) && c.IsUnderControl));
            if (buffer.Count == 0)
            {
                controller = null;
                return false;
            }
            else
            {
                controller = buffer[0];
                return true;
            }
        }

        private Matrix GetLocalMatrix(IMyTerminalBlock Block)
        {
            Matrix localMatrix;
            Block.Orientation.GetMatrix(out localMatrix);

            return localMatrix;
        }

        #region BaconArgs 
        public class BaconArgs { public string Raw; static public BaconArgs parse(string a) { return (new Parser()).parseArgs(a); } static public string Escape(object a) { return string.Format("{0}", a).Replace(@"\", @"\\").Replace(@" ", @"\ ").Replace(@"""", @"\"""); } static public string UnEscape(string a) { return a.Replace(@"\""", @"""").Replace(@"\ ", @" ").Replace(@"\\", @"\"); } public class Parser { static Dictionary<string, BaconArgs> h = new Dictionary<string, BaconArgs>(); public BaconArgs parseArgs(string a) { if (!h.ContainsKey(a)) { var b = new BaconArgs(); b.Raw = a; var c = false; var d = false; var e = new StringBuilder(); for (int f = 0; f < a.Length; f++) { var g = a[f]; if (c) { e.Append(g); c = false; } else if (g.Equals('\\')) c = true; else if (d && !g.Equals('"')) e.Append(g); else if (g.Equals('"')) d = !d; else if (g.Equals(' ')) if (e.ToString().Equals("--")) { b.add(a.Substring(f).TrimStart()); e.Clear(); break; } else { b.add(e.ToString()); e.Clear(); } else e.Append(g); } if (e.Length > 0) b.add(e.ToString()); h.Add(a, b); } return h[a]; } } protected Dictionary<char, int> h = new Dictionary<char, int>(); protected List<string> i = new List<string>(); protected Dictionary<string, List<string>> j = new Dictionary<string, List<string>>(); public List<string> getArguments() { return i; } public int getFlag(char a) { return h.ContainsKey(a) ? h[a] : 0; } public bool hasArguments() { return i.Count > 0; } public bool hasOption(string a) { return j.ContainsKey(a); } public List<string> getOption(string a) { return j.ContainsKey(a) ? j[a] : new List<string>(); } public void add(string a) { if (a.Trim().Length > 0) if (!a.StartsWith("-")) { i.Add(a); } else if (a.StartsWith("--")) { KeyValuePair<string, string> b = k(a); string c = b.Key.Substring(2); if (!j.ContainsKey(c)) { j.Add(c, new List<string>()); } j[c].Add(b.Value); } else { string b = a.Substring(1); for (int d = 0; d < b.Length; d++) { if (this.h.ContainsKey(b[d])) { this.h[b[d]]++; } else { this.h.Add(b[d], 1); } } } } KeyValuePair<string, string> k(string a) { string[] b = a.Split(new char[] { '=' }, 2); return new KeyValuePair<string, string>(b[0], (b.Length > 1) ? b[1] : null); } public string ToArguments() { var a = new List<string>(); foreach (string argument in this.getArguments()) a.Add(Escape(argument)); foreach (KeyValuePair<string, List<string>> option in this.j) { var b = "--" + Escape(option.Key); foreach (string optVal in option.Value) a.Add(b + ((optVal != null) ? "=" + Escape(optVal) : "")); } var c = (h.Count > 0) ? "-" : ""; foreach (KeyValuePair<char, int> flag in h) c += new String(flag.Key, flag.Value); a.Add(c); return String.Join(" ", a.ToArray()); } override public string ToString() { var a = new List<string>(); foreach (string key in j.Keys) a.Add(l(key) + ":[" + string.Join(",", j[key].ConvertAll<string>(b => l(b)).ToArray()) + "]"); var c = new List<string>(); foreach (char key in h.Keys) c.Add(key + ":" + h[key].ToString()); var d = new StringBuilder(); d.Append("{\"a\":["); d.Append(string.Join(",", i.ConvertAll<string>(b => l(b)).ToArray())); d.Append("],\"o\":[{"); d.Append(string.Join("},{", a)); d.Append("}],\"f\":[{"); d.Append(string.Join("},{", c)); d.Append("}]}"); return d.ToString(); } string l(string a) { return (a != null) ? "\"" + a.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" : @"null"; } }
        #endregion BaconArgs 

        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}