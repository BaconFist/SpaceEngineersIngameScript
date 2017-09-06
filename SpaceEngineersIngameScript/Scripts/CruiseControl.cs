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

        const string OPT_DEADZONE = "DeadZone";
        const string OPT_THRUSTMULTI = "ThrustMultiplicator";

        const string PROPETY_OVERRIDE = "Override";

        BaconArgs Args;

        float thrustMultiplicator = 3;
        float deadZone = 0.25f;
        float targetSpeed = 0;

        float currentShipSpeed = 0;
        IMyShipController shipController;
      

        public void Main(string argument)
        {
            run(argument);
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
        
        public void run(string argument)
        {
            Args = BaconArgs.parse(argument);

            // update target Speed
            float newSpeedTargetBuffer = 0;
            if (Args.hasArguments() && float.TryParse(Args.getArguments()[0], out newSpeedTargetBuffer))
            {
                targetSpeed = newSpeedTargetBuffer;
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

            // update speed overrides
            if (TryGetUsedController(out shipController) && TryGetForwardSpeed(out currentShipSpeed, shipController))
            {
                List<IMyThrust> ThrustersAcceleration = findAccelerationThrusters(shipController);
                List<IMyThrust> ThrustersDecelartion = findDecelerationThruster(shipController);

                float speedDiff = Math.Abs(targetSpeed - currentShipSpeed);

                shipController.ShowOnHUD = true;
                shipController.CustomName = String.Format(
                    @"Speed: {0}, Target: {1}, Diff: {2}",
                   currentShipSpeed,
                   targetSpeed,
                   speedDiff
                    );

                if (targetSpeed == 0)
                {
                    foreach (IMyThrust Thrust in ThrustersDecelartion)
                    {
                        Thrust.SetValueFloat(PROPETY_OVERRIDE, 0f);
                    }
                    foreach (IMyThrust Thrust in ThrustersAcceleration)
                    {
                        Thrust.SetValueFloat(PROPETY_OVERRIDE, 0f);
                    }
                }
                else if (deadZone < speedDiff)
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
        
        public List<IMyThrust> findAccelerationThrusters(IMyShipController Controller)
        {
            List<IMyThrust> Buffer = new List<IMyThrust>();
            Vector3I forwardVector = getShipControllerForwardVector(Controller);
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(Buffer, (t => 
                t.CubeGrid.Equals(Me.CubeGrid)
                && !(t.ThrustOverride == 0 && t.CurrentThrust > 0)
                && getThrustDirection(t).Equals(forwardVector)            
            ));
            return Buffer;
        }

        public List<IMyThrust> findDecelerationThruster(IMyShipController Controller)
        {
            List<IMyThrust> Buffer = new List<IMyThrust>();
            Vector3I backwardVector = getShipControllerBackwardVector(Controller);
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(Buffer, (t =>
                t.CubeGrid.Equals(Me.CubeGrid)
                && !(t.ThrustOverride == 0 && t.CurrentThrust > 0)
                && getThrustDirection(t).Equals(backwardVector)
            ));
            return Buffer;
        }

        public bool TryGetForwardSpeed(out float speed, IMyShipController controller)
        {
            speed = 0;
            try
            {
                Vector3 forwardSpeed = Vector3.TransformNormal(controller.GetShipVelocities().LinearVelocity, Matrix.Transpose(controller.WorldMatrix));
                speed = forwardSpeed.Z * -1;
                
                return true;
            } catch (Exception e)
            {
                Echo(String.Format("{0}",e));
                return false;
            }
        }

        public bool TryGetUsedController(out IMyShipController controller)
        {
            List<IMyShipController> buffer = new List<IMyShipController>();
            GridTerminalSystem.GetBlocksOfType<IMyShipController>(buffer, (c => c.CubeGrid.Equals(Me.CubeGrid) && c.IsUnderControl));
            if(buffer.Count == 0)
            {
                controller = null;
                return false;
            } else
            {
                controller = buffer[0];
                return true;
            }
        }

        public Vector3I getShipControllerBackwardVector(IMyShipController ShipController)
        {
            Matrix localMatrix;
            ShipController.Orientation.GetMatrix(out localMatrix);
            Vector3 buffer;
            switch (ShipController.BlockDefinition.SubtypeName)
            {
                case "LargeBlockCockpit":
                case "LargeBlockCockpitSeat":
                case "SmallBlockCockpit":
                case "DBSmallBlockFighterCockpit":
                case "CockpitOpen":
                case "LargeBlockRemoteControl":
                case "SmallBlockRemoteControl":
                default:
                    buffer = localMatrix.Backward;
                    break;
            }

            return new Vector3I(buffer);
        }

        public Vector3I getShipControllerForwardVector(IMyShipController ShipController)
        {
            Matrix localMatrix;
            ShipController.Orientation.GetMatrix(out localMatrix);
            Vector3 buffer;
            switch (ShipController.BlockDefinition.SubtypeName)
            {
                case "LargeBlockCockpit":
                case "LargeBlockCockpitSeat":
                case "SmallBlockCockpit":
                case "DBSmallBlockFighterCockpit":
                case "CockpitOpen":
                case "LargeBlockRemoteControl":
                case "SmallBlockRemoteControl":
                default:
                    buffer = localMatrix.Forward;
                    break;
            }

            return new Vector3I(buffer);
        }

        public Vector3I getThrustDirection(IMyThrust Thruster)
        {
            Matrix localMatrix;
            Thruster.Orientation.GetMatrix(out localMatrix);
            Vector3 buffer;
            switch (Thruster.BlockDefinition.SubtypeName)
            {
                case "SmallBlockSmallThrust":
                case "SmallBlockLargeThrust":
                case "LargeBlockSmallThrust":
                case "LargeBlockLargeThrust":
                case "LargeBlockLargeHydrogenThrust":
                case "LargeBlockSmallHydrogenThrust":
                case "SmallBlockLargeHydrogenThrust":
                case "SmallBlockSmallHydrogenThrust":
                case "LargeBlockLargeAtmosphericThrust":
                case "LargeBlockSmallAtmosphericThrust":
                case "SmallBlockLargeAtmosphericThrust":
                case "SmallBlockSmallAtmosphericThrust":
                default:
                    buffer = localMatrix.Backward;
                    break;
            }

            return new Vector3I(buffer);
        }

        #region BaconArgs
        public class BaconArgs { public string Raw; static public BaconArgs parse(string a) { return (new Parser()).parseArgs(a); } static public string Escape(object a) { return string.Format("{0}", a).Replace(@"\", @"\\").Replace(@" ", @"\ ").Replace(@"""", @"\"""); } static public string UnEscape(string a) { return a.Replace(@"\""", @"""").Replace(@"\ ", @" ").Replace(@"\\", @"\"); } public class Parser { static Dictionary<string, BaconArgs> h = new Dictionary<string, BaconArgs>(); public BaconArgs parseArgs(string a) { if (!h.ContainsKey(a)) { var b = new BaconArgs(); b.Raw = a; var c = false; var d = false; var e = new StringBuilder(); for (int f = 0; f < a.Length; f++) { var g = a[f]; if (c) { e.Append(g); c = false; } else if (g.Equals('\\')) c = true; else if (d && !g.Equals('"')) e.Append(g); else if (g.Equals('"')) d = !d; else if (g.Equals(' ')) if (e.ToString().Equals("--")) { b.add(a.Substring(f).TrimStart()); e.Clear(); break; } else { b.add(e.ToString()); e.Clear(); } else e.Append(g); } if (e.Length > 0) b.add(e.ToString()); h.Add(a, b); } return h[a]; } } protected Dictionary<char, int> h = new Dictionary<char, int>(); protected List<string> i = new List<string>(); protected Dictionary<string, List<string>> j = new Dictionary<string, List<string>>(); public List<string> getArguments() { return i; } public int getFlag(char a) { return h.ContainsKey(a) ? h[a] : 0; } public bool hasArguments() { return i.Count > 0; } public bool hasOption(string a) { return j.ContainsKey(a); } public List<string> getOption(string a) { return j.ContainsKey(a) ? j[a] : new List<string>(); } public void add(string a) { if (a.Trim().Length > 0) if (!a.StartsWith("-")) { i.Add(a); } else if (a.StartsWith("--")) { KeyValuePair<string, string> b = k(a); string c = b.Key.Substring(2); if (!j.ContainsKey(c)) { j.Add(c, new List<string>()); } j[c].Add(b.Value); } else { string b = a.Substring(1); for (int d = 0; d < b.Length; d++) { if (this.h.ContainsKey(b[d])) { this.h[b[d]]++; } else { this.h.Add(b[d], 1); } } } } KeyValuePair<string, string> k(string a) { string[] b = a.Split(new char[] { '=' }, 2); return new KeyValuePair<string, string>(b[0], (b.Length > 1) ? b[1] : null); } public string ToArguments() { var a = new List<string>(); foreach (string argument in this.getArguments()) a.Add(Escape(argument)); foreach (KeyValuePair<string, List<string>> option in this.j) { var b = "--" + Escape(option.Key); foreach (string optVal in option.Value) a.Add(b + ((optVal != null) ? "=" + Escape(optVal) : "")); } var c = (h.Count > 0) ? "-" : ""; foreach (KeyValuePair<char, int> flag in h) c += new String(flag.Key, flag.Value); a.Add(c); return String.Join(" ", a.ToArray()); } override public string ToString() { var a = new List<string>(); foreach (string key in j.Keys) a.Add(l(key) + ":[" + string.Join(",", j[key].ConvertAll<string>(b => l(b)).ToArray()) + "]"); var c = new List<string>(); foreach (char key in h.Keys) c.Add(key + ":" + h[key].ToString()); var d = new StringBuilder(); d.Append("{\"a\":["); d.Append(string.Join(",", i.ConvertAll<string>(b => l(b)).ToArray())); d.Append("],\"o\":[{"); d.Append(string.Join("},{", a)); d.Append("}],\"f\":[{"); d.Append(string.Join("},{", c)); d.Append("}]}"); return d.ToString(); } string l(string a) { return (a != null) ? "\"" + a.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" : @"null"; } }
        #endregion BaconArgs


        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}