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

            // TODO: transform ShipsForward direction to Cockpit forward direction

        const string OPT_DEADZONE = "DeadZone";
        const string OPT_THRUSTMULTI = "ThrustMultiplicator";

        const string PROPETY_OVERRIDE = "Override";

        BaconArgs Args;

        float thrustMultiplicator = 3;
        int deadZone = 1;
        int targetSpeed = 0;

        int currentShipSpeed = 0;
      

        public void Main(string argument)
        {
            Args = BaconArgs.parse(argument);

            // update target Speed
            int newSpeedTargetBuffer = 0;
            if(Args.hasArguments() && int.TryParse(Args.getArguments()[0], out newSpeedTargetBuffer))
            {
                targetSpeed = newSpeedTargetBuffer;
            }

            //update deadZone
            int newDeadZoneBuffer = 0;
            if (Args.hasOption(OPT_DEADZONE) && int.TryParse(Args.getOption(OPT_DEADZONE)[0], out newDeadZoneBuffer))
            {
                deadZone = newDeadZoneBuffer;
            }

            //update thrust multiplicator
            float newThrustMultiplicatorBuffer = 0f;
            if(Args.hasOption(OPT_THRUSTMULTI) && float.TryParse(Args.getOption(OPT_THRUSTMULTI)[0], out newThrustMultiplicatorBuffer))
            {
                thrustMultiplicator = newThrustMultiplicatorBuffer;
            }

            // update speed overrides
            if (TryGetForwardSpeed(out currentShipSpeed))
            {
                List<IMyThrust> ThrustersAcceleration = findAccelerationThrusters();
                List<IMyThrust> ThrustersDecelartion = findDecelerationThruster();

                int speedDiff = Math.Abs(targetSpeed - currentShipSpeed);

                if(targetSpeed == 0)
                {
                    foreach (IMyThrust Thrust in ThrustersDecelartion)
                    {
                        Thrust.SetValueFloat(PROPETY_OVERRIDE, 0f);
                    }
                    foreach (IMyThrust Thrust in ThrustersAcceleration)
                    {
                        Thrust.SetValueFloat(PROPETY_OVERRIDE, 0f);
                    }
                } else if (deadZone < speedDiff)
                {
                    if (targetSpeed < currentShipSpeed)
                    {
                        //decelerate
                        foreach (IMyThrust Thrust in ThrustersAcceleration)
                        {
                            Thrust.SetValueFloat(PROPETY_OVERRIDE, 0f);
                        }
                        foreach (IMyThrust Thrust in ThrustersDecelartion)
                        {
                            Thrust.SetValueFloat(PROPETY_OVERRIDE, Math.Min(100, (currentShipSpeed - targetSpeed) * thrustMultiplicator));
                        }
                    }
                    else if (currentShipSpeed < targetSpeed)
                    {
                        //accelerate
                        foreach (IMyThrust Thrust in ThrustersDecelartion)
                        {
                            Thrust.SetValueFloat(PROPETY_OVERRIDE, 0f);
                        }
                        foreach (IMyThrust Thrust in ThrustersAcceleration)
                        {
                            Thrust.SetValueFloat(PROPETY_OVERRIDE, Math.Min(100, (targetSpeed - currentShipSpeed) * thrustMultiplicator));
                        }

                    }
                }
            }

            printInfo();
        }

        public void printInfo()
        {
            StringBuilder info = new StringBuilder();
            info.AppendLine(string.Format(@"[Bacon's Cruise Control]"));
            info.AppendLine(string.Format(@"Forward speed: {0} m/s", currentShipSpeed));
            info.AppendLine(string.Format(@"Target speed: {0} m/s", targetSpeed));
            info.AppendLine(string.Format(@"DeadZone: {0} m/s", deadZone));
            info.AppendLine(string.Format(@"Thrust multiplicator: {0}", thrustMultiplicator));
            info.AppendLine(string.Format(@"HELP:"));
            info.AppendLine(string.Format(@"* set target speed by passing it as an argument."));
            info.AppendLine(string.Format(@"* set DeadZone with --{0}=NUMBER", OPT_DEADZONE));
            info.AppendLine(string.Format(@"* set Thrust multiplicator with --{0}=NUMBER", OPT_THRUSTMULTI));
            info.AppendLine(string.Format(@"All settings will be saved until the script is recompiled."));

            Me.CustomData = info.ToString();
            Echo(info.ToString());
        }   
        
        public List<IMyThrust> findAccelerationThrusters()
        {
            List<IMyThrust> Buffer = new List<IMyThrust>();
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(Buffer, (b => b.CubeGrid.Equals(Me.CubeGrid) && !(b.ThrustOverride == 0 && b.CurrentThrust > 0) && b.Orientation.Forward.Equals(getAccelerationDirection(b))));
            return Buffer;
        }

        public List<IMyThrust> findDecelerationThruster()
        {
            List<IMyThrust> Buffer = new List<IMyThrust>();
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(Buffer, (b => b.CubeGrid.Equals(Me.CubeGrid) && !(b.ThrustOverride == 0 && b.CurrentThrust > 0)  && b.Orientation.Forward.Equals(getDecelerationDirection(b))));
            return Buffer;
        }

        public Base6Directions.Direction getAccelerationDirection(IMyThrust Thruster)
        {
            Base6Directions.Direction Buffer;
            switch (Thruster.BlockDefinition.SubtypeName)
            {
                default:
                    Buffer = Base6Directions.Direction.Backward;
                    break;
            }

            return Buffer;
        }

        public Base6Directions.Direction getDecelerationDirection(IMyThrust Thruster)
        {
            Base6Directions.Direction Buffer;
            switch (Thruster.BlockDefinition.SubtypeName)
            {
                default:
                    Buffer = Base6Directions.Direction.Forward;
                    break;
            }

            return Buffer;
        }

        public bool TryGetForwardSpeed(out int speed)
        {
            speed = 0;
            List<IMyShipController> Controler = new List<IMyShipController>();
            GridTerminalSystem.GetBlocksOfType<IMyShipController>(Controler, (b => b.IsWorking && b.IsFunctional && b.IsUnderControl));
            if(Controler.Count > 0)
            {
                IMyShipController CurrentControlSeat;
                CurrentControlSeat = Controler[0];
                speed = (CurrentControlSeat.CubeGrid.WorldToGridInteger(CurrentControlSeat.WorldMatrix.Translation + CurrentControlSeat.GetShipVelocities().LinearVelocity * CurrentControlSeat.CubeGrid.GridSize).Z) * -1;

                CurrentControlSeat.CustomName = string.Format("{0}", speed);

                return true;
            }
            return false;
        }

        #region BaconArgs
        public class BaconArgs { public string Raw; static public BaconArgs parse(string a) { return (new Parser()).parseArgs(a); } static public string Escape(object a) { return string.Format("{0}", a).Replace(@"\", @"\\").Replace(@" ", @"\ ").Replace(@"""", @"\"""); } static public string UnEscape(string a) { return a.Replace(@"\""", @"""").Replace(@"\ ", @" ").Replace(@"\\", @"\"); } public class Parser { static Dictionary<string, BaconArgs> h = new Dictionary<string, BaconArgs>(); public BaconArgs parseArgs(string a) { if (!h.ContainsKey(a)) { var b = new BaconArgs(); b.Raw = a; var c = false; var d = false; var e = new StringBuilder(); for (int f = 0; f < a.Length; f++) { var g = a[f]; if (c) { e.Append(g); c = false; } else if (g.Equals('\\')) c = true; else if (d && !g.Equals('"')) e.Append(g); else if (g.Equals('"')) d = !d; else if (g.Equals(' ')) if (e.ToString().Equals("--")) { b.add(a.Substring(f).TrimStart()); e.Clear(); break; } else { b.add(e.ToString()); e.Clear(); } else e.Append(g); } if (e.Length > 0) b.add(e.ToString()); h.Add(a, b); } return h[a]; } } protected Dictionary<char, int> h = new Dictionary<char, int>(); protected List<string> i = new List<string>(); protected Dictionary<string, List<string>> j = new Dictionary<string, List<string>>(); public List<string> getArguments() { return i; } public int getFlag(char a) { return h.ContainsKey(a) ? h[a] : 0; } public bool hasArguments() { return i.Count > 0; } public bool hasOption(string a) { return j.ContainsKey(a); } public List<string> getOption(string a) { return j.ContainsKey(a) ? j[a] : new List<string>(); } public void add(string a) { if (a.Trim().Length > 0) if (!a.StartsWith("-")) { i.Add(a); } else if (a.StartsWith("--")) { KeyValuePair<string, string> b = k(a); string c = b.Key.Substring(2); if (!j.ContainsKey(c)) { j.Add(c, new List<string>()); } j[c].Add(b.Value); } else { string b = a.Substring(1); for (int d = 0; d < b.Length; d++) { if (this.h.ContainsKey(b[d])) { this.h[b[d]]++; } else { this.h.Add(b[d], 1); } } } } KeyValuePair<string, string> k(string a) { string[] b = a.Split(new char[] { '=' }, 2); return new KeyValuePair<string, string>(b[0], (b.Length > 1) ? b[1] : null); } public string ToArguments() { var a = new List<string>(); foreach (string argument in this.getArguments()) a.Add(Escape(argument)); foreach (KeyValuePair<string, List<string>> option in this.j) { var b = "--" + Escape(option.Key); foreach (string optVal in option.Value) a.Add(b + ((optVal != null) ? "=" + Escape(optVal) : "")); } var c = (h.Count > 0) ? "-" : ""; foreach (KeyValuePair<char, int> flag in h) c += new String(flag.Key, flag.Value); a.Add(c); return String.Join(" ", a.ToArray()); } override public string ToString() { var a = new List<string>(); foreach (string key in j.Keys) a.Add(l(key) + ":[" + string.Join(",", j[key].ConvertAll<string>(b => l(b)).ToArray()) + "]"); var c = new List<string>(); foreach (char key in h.Keys) c.Add(key + ":" + h[key].ToString()); var d = new StringBuilder(); d.Append("{\"a\":["); d.Append(string.Join(",", i.ConvertAll<string>(b => l(b)).ToArray())); d.Append("],\"o\":[{"); d.Append(string.Join("},{", a)); d.Append("}],\"f\":[{"); d.Append(string.Join("},{", c)); d.Append("}]}"); return d.ToString(); } string l(string a) { return (a != null) ? "\"" + a.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" : @"null"; } }
        #endregion BaconArgs


        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}