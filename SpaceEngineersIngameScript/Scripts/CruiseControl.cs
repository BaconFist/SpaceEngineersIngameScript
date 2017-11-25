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

            //TODO: Detect if ship is docked
            //TODO: add optoin for debug => --debug="debug,trace,warn,all" --debug-lcd="tag" => if not --debug => Log = null
                    
                

        BaconArgs Args;

        Dictionary<IMyShipController, BMyCruiseControl> CruiseControls = new Dictionary<IMyShipController, BMyCruiseControl>();

        string TAG_LCD_VIEW= "[BCC]";
        string TAG_VIEW_CONTROLLER = "[BCC]";

        string TEMPLATE_DEFAULT_LCD = ""
            + "[Cruise Control {{NOW}}]\n"
            + "Speed: {{CURRENT_FORWARD_VELOCITY}}/{{TARGET_FORWARD_VELOCITY}} m/s\n"
            + "Diff: {{FORWARD_VELOCITY_DIFF}}\n"
            + "Mode: {{MODE_OF_OPERATION}}\n"
            + "\n"
            + "Non-Hydrogen Forward: {{OVERRIDE_ATMOSPHERIC_AND_ION}} %\n"
            + "Hydrogen, Forward: {{OVERRIDE_FORWARD_HYDROGEN}} %\n"
            + "\n"
            + "Non-Hydrogen Backward: {{OVERRIDE_BACKWARD_ATMOSPEHRIC_AND_ION}} %\n"
            + "Hydrogen Backward: {{OVERRIDE_BACKWARD_HYDROGEN}} %";


        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10 | UpdateFrequency.Update100;
        }

        public void Main(string argument, UpdateType updateSource)        {
            BMyLog4PB Log = new BMyLog4PB(this, BMyLog4PB.E_ALL ^ BMyLog4PB.E_TRACE, new BMyLog4PB.BMyCustomDataAppender(this));
            Log.AutoFlush = true;
            try
            {
                Log?.PushStack("Main");

                if (updateSource.HasFlag(UpdateType.Trigger))
                {
                    Log?.IfWarn?.Warn("Script is called by Timer/Sensor. (script updates itself, do not use Timers)");
                }


                Log?.PushStack("-Prepare");
                Args = BaconArgs.parse(argument);
                CruiseControlOptionBag Options = new CruiseControlOptionBag(Args);

                IMyShipController Controller = GetController(Options.ControllerFilter, Log);
                BMyCruiseControl CruiseControl = GetCruiseControl(Controller, Log);
                CruiseControl.TurnOffOnIdle = Options.TurnOffOnIdle;
                if(Options.HasSetSpeed)
                {
                    CruiseControl.TargetVelocity = Options.SetSpeed;
                }
                CruiseControl.TargetVelocity += Options.IncrementSpeed;

                CruiseControl.ErrorMargin = Options.ErrorMargin;
                Log?.PopStack();

                CruiseControl.Run();
                Log?.IfDebug?.Debug("END: Run");

                BMyCruiseControlView View = new BMyCruiseControlView(CruiseControl, Log);

                if (updateSource.HasFlag(UpdateType.Update100))
                {
                    Log?.IfDebug?.Debug("Update100 => refresh LCDs");
                    WriteToLCD(View, Log);

                    List<IMyProgrammableBlock> ViewControllers = new List<IMyProgrammableBlock>();
                    GridTerminalSystem.GetBlocksOfType<IMyProgrammableBlock>(ViewControllers, (v => v.CubeGrid.Equals(Me.CubeGrid) && v.IsWorking && v.CustomName.Contains(TAG_VIEW_CONTROLLER)));
                    if (ViewControllers.Count > 0)
                    {
                        string externalViewArgument = GetExternalViewArgument(View);
                        foreach (IMyProgrammableBlock ViewPB in ViewControllers)
                        {
                            if (ViewPB.TryRun(externalViewArgument))
                            {
                                Log?.IfDebug?.Debug("successfully called external view \"{0}\"", ViewPB.CustomName);
                            } else
                            {
                                Log?.IfError?.Error("failed calling external view \"{0}\"", ViewPB.CustomName);
                            }                            
                        }
                    } else
                    {
                        Log?.IfDebug?.Debug("no external View Controller found.");
                    }
                }
                WriteToEcho(View, Log);

                
                Log?.PopStack();
            } catch (Exception e)
            {
                Log?.IfFatal?.Fatal("EXCEPTION: {0}", e);
                Echo(string.Format("{0}", e));
            } finally
            {
                Log?.Flush();                
            }            
        }
   
       
        /// <summary>
        /// returns a shipcontroller (cokpit or remote) accordinf to given filter.
        /// </summary>
        /// <param name="filter">must be contained in controller's customname</param>
        /// <param name="Log">an instance of BMyLog4PB or null</param>
        /// <returns>the cockpit/remote under controll</returns>
        private IMyShipController GetController(string filter, BMyLog4PB Log)
        {
            Log?.PushStack("GetController");
            List<IMyShipController> Controller = new List<IMyShipController>();
            GridTerminalSystem.GetBlocksOfType<IMyShipController>(Controller, (c => 
                c.IsWorking
                && c.CanControlShip
                && c.CubeGrid.Equals(Me.CubeGrid)
                && c.IsUnderControl
                && c.ControlThrusters
                && (filter.Length == 0 || c.CustomName.Contains(filter))
            ));

            IMyShipController Match = Controller.Find(c => (c is IMyCockpit) && (c as IMyCockpit).IsMainCockpit);
            if(Match == null)
            {
                Match = Controller.Count > 0 ? Controller[0] : null;
            }

            Log?.IfDebug?.Debug("Match: {0}", Match.CustomName);
            Log?.PopStack();
            return Match;
        }

        /// <summary>
        /// returns the cruisecontrol mapped to "Controller"
        /// </summary>
        /// <param name="Controller">cockpit / remote to be used as reference</param>
        /// <param name="Log"></param>
        /// <returns></returns>
        private BMyCruiseControl GetCruiseControl(IMyShipController Controller, BMyLog4PB Log)
        {
            Log?.PushStack("GetCruiseControl");
            if (!CruiseControls.ContainsKey(Controller))
            {
                CruiseControls.Add(Controller, new BMyCruiseControl(Controller, this, Log));
                Log?.IfDebug?.Debug("CruiseControl for {0} created.", Controller.CustomName);
            }
            Log?.PopStack();
            return CruiseControls[Controller];
        }

        /// <summary>
        /// holds all the options/settings/arguments for a cruise controll
        /// </summary>
        private class CruiseControlOptionBag {
            const string ID_DEADZONE = "DeadZone";
            const string ID_ERROR_MARGIN = "ErrorMargin";
            const string ID_INCREMENT = "inc";
            const string ID_DECREMENT = "dec";
            const string ID_CONTROLLER_FILTER = "ControllerFilter";
            const string ID_TURN_OFF_ON_IDLE = "TurnOffOnIdle";

            public float IncrementSpeed { get; }
            public float SetSpeed { get; }
            public float ErrorMargin { get; }
            public string ControllerFilter { get; }

            public bool HasSetSpeed = false;

            public bool TurnOffOnIdle = false;

            public CruiseControlOptionBag(BaconArgs Args)
            {
                float buffer = 0f;
                if (Args.hasArguments() && float.TryParse(Args.getArguments()[0], out buffer)) {
                    SetSpeed = buffer;
                    HasSetSpeed = true;
                }
                buffer = 0f;
                if (Args.hasOption(ID_DEADZONE) && float.TryParse(Args.getOption(ID_DEADZONE)[0], out buffer))
                {
                     ErrorMargin = buffer;
                }
                buffer = 0f;
                if (Args.hasOption(ID_ERROR_MARGIN) && float.TryParse(Args.getOption(ID_ERROR_MARGIN)[0], out buffer))
                {
                     ErrorMargin = buffer;
                }
                buffer = 0f;
                if (Args.hasOption(ID_INCREMENT) && float.TryParse(Args.getOption(ID_INCREMENT)[0], out buffer))
                {
                     IncrementSpeed = buffer;
                }
                buffer = 0f;
                if (Args.hasOption(ID_DECREMENT) && float.TryParse(Args.getOption(ID_DECREMENT)[0], out buffer))
                {
                    IncrementSpeed -= buffer;
                }
                bool bufferTurnOfOnIdle = false;
                if(Args.hasOption(ID_TURN_OFF_ON_IDLE) && bool.TryParse(Args.getOption(ID_TURN_OFF_ON_IDLE)[0], out bufferTurnOfOnIdle))
                {
                        TurnOffOnIdle = bufferTurnOfOnIdle;
                }
                ControllerFilter = Args.hasOption(ID_CONTROLLER_FILTER) ? Args.getOption(ID_CONTROLLER_FILTER)[0] : "";

            }
        }
        

        public class BMyCruiseControl {
            private BMyLog4PB Log { get; }

            const float NEWTON_PER_KG = 9.8066500286389f;
            const byte THRUSTER_TYPE_ANY = 7;
            const byte THRUSTER_TYPE_UNKNOWN = 0;
            const byte THRUSTER_TYPE_ION = 1;
            const byte THRUSTER_TYPE_HYDROGEN = 2;
            const byte THRUSTER_TYPE_ATMOSPHERIC = 4;

            const byte OVERRIDE_FORCE_ON = 1;
            const byte OVERRIDE_FORCE_OFF = 2;

            public bool TurnOffOnIdle = false;

            /// <summary>
            /// defines wich type of thruster has wich propulsion system
            /// </summary>
            static Dictionary<byte, List<string>> ThrusterTypeMap = new Dictionary<byte, List<string>>(){
                {THRUSTER_TYPE_HYDROGEN, new List<string>{
                    "LargeBlockLargeHydrogenThrust",
                    "LargeBlockSmallHydrogenThrust",
                    "SmallBlockLargeHydrogenThrust",
                    "SmallBlockSmallHydrogenThrust"
                }},
                {THRUSTER_TYPE_ION, new List<string>{
                    "SmallBlockSmallThrust",
                    "SmallBlockLargeThrust",
                    "LargeBlockSmallThrust",
                    "LargeBlockLargeThrust"
                }},
                {THRUSTER_TYPE_ATMOSPHERIC, new List<string>{
                    "LargeBlockLargeAtmosphericThrust",
                    "LargeBlockSmallAtmosphericThrust",
                    "SmallBlockLargeAtmosphericThrust",
                    "SmallBlockSmallAtmosphericThrust"
                } },
            };

            /// <summary>
            /// hild data later required by a View
            /// </summary>
            public State state { get; }

            private IMyShipController Controller { get; }
            private Matrix ControllerMatrix { get; }
            private Program App  { get; }
            private float CurrentVelocity;

            private List<IMyThrust> ThrustersForwardAtmosphericAndIon = new List<IMyThrust>();
            private List<IMyThrust> ThrustersBackwardAtmosphericAndIon = new List<IMyThrust>();

            private List<IMyThrust> ThrustersForwardHydrogen= new List<IMyThrust>();
            private List<IMyThrust> ThrustersBackwardHydrogen = new List<IMyThrust>();
            
            public float TargetVelocity { get; set; }            
            /// <summary>
            /// allowed difference between current and target speed
            /// </summary>
            public float ErrorMargin { get; set; }

            private float ForceAvailableForwardIonAndAtmospheric = 0f;
            private float ForceAvailableBackwardIonAndAtmospheric = 0f;

            private float ForceAvailableForwardHydrogen = 0f;
            private float ForceAvailableBackwardHydrogen = 0f;


            public BMyCruiseControl(IMyShipController Controller, Program App, BMyLog4PB Log = null)
            {
                state = new State(this);
                this.Log = Log;
                this.App = App;
                this.Controller = Controller;
                ControllerMatrix = GetLocalMatrix(this.Controller);
                TargetVelocity = 0f;
                ErrorMargin = 0f;
                UpdateThrusterCollections();
            }

            public void Run()
            {
                Log?.PushStack("Run");
                UpdateForwardVelocity();
                state.VelocityDelta = TargetVelocity - CurrentVelocity;
                if(TargetVelocity == 0f)
                {
                    Log?.IfDebug?.Debug("TargetVelocity is 0 => turning cruise control off.");
                    Disable();
                } else if(CurrentVelocity < TargetVelocity - ErrorMargin)
                {
                    Log?.IfDebug?.Debug("Current velocity {0} is below the allowed Value {1}. => accelerate", CurrentVelocity, TargetVelocity - ErrorMargin);
                    Accelerate();
                } else if(TargetVelocity + ErrorMargin < CurrentVelocity)
                {
                    Log?.IfDebug?.Debug("Current velocity {0} is above allowed Value {1}. => decellerate", CurrentVelocity, TargetVelocity + ErrorMargin);
                    Decellerate();
                } else {
                    Log?.IfDebug?.Debug("Current velocity {0} is inbetween allowed range ]{1},{2}[. => turn off thrust", CurrentVelocity, TargetVelocity - ErrorMargin, TargetVelocity + ErrorMargin);
                    Idle();
                }

                Log?.PopStack();
            }

            public void Accelerate()
            {
                Log?.PushStack("Accelerate");
                state.Operation = "Accelerate";

                float velocityDelta = Math.Abs(TargetVelocity - ErrorMargin - CurrentVelocity);
                float forceRequiredIonAndAtmospheric = GetRequiredThrustForce() * velocityDelta;
                float forceRequiredHydrogen = forceRequiredIonAndAtmospheric - ForceAvailableForwardIonAndAtmospheric;
                float overrideIonAndAtmosphericThrusters = Range(0f, (forceRequiredIonAndAtmospheric / Math.Min(1, ForceAvailableForwardIonAndAtmospheric)) * 100f);
                float overrideHydrogenThrusters = 0f;

                BatchThrustOverride(ThrustersForwardAtmosphericAndIon, overrideIonAndAtmosphericThrusters);

                if (forceRequiredHydrogen > 0)
                {
                    overrideHydrogenThrusters = Range(0f, (forceRequiredHydrogen / ForceAvailableBackwardHydrogen) * 100f);
                }
                BatchThrustOverride(ThrustersForwardHydrogen, overrideHydrogenThrusters);

                Log?.IfDebug?.Debug("Override forward Ion/Atmospheric: {0}%", overrideIonAndAtmosphericThrusters);
                Log?.IfDebug?.Debug("Override forward Hydrogen: {0}%", overrideHydrogenThrusters);

                BatchThrustOverride(ThrustersBackwardAtmosphericAndIon, 0f);
                BatchThrustOverride(ThrustersBackwardHydrogen, 0f);
                Log?.IfDebug?.Debug("Set backward Thrusters to Idle ({0} %)", 0f);

                state.VelocityDelta = velocityDelta;

                state.OverrideBackwardAtmosphericAndIon = 0f;
                state.OverrideBackwardHydrogen = 0f;
                state.OverrideForwardAtmosphericAndIon = overrideIonAndAtmosphericThrusters;
                state.OverrideForwardHydrogen = overrideHydrogenThrusters;

                Log?.PopStack();
            }

            public float Range(float Min, float Max)
            {
                return Math.Max(Math.Min(Min,Max),Max);
            }

            public void Decellerate()
            {
                Log?.PushStack("Decellerate");
                state.Operation = "Decellerate";

                float velocityDelta = Math.Abs(TargetVelocity + ErrorMargin - CurrentVelocity);
                float forceRequiredIonAndAtmospheric = GetRequiredThrustForce() * velocityDelta;
                float forceRequiredHydrogen = forceRequiredIonAndAtmospheric - ForceAvailableBackwardIonAndAtmospheric;
                float overrideIonAndAtmosphericThrusters = Range(0f, (forceRequiredIonAndAtmospheric / Math.Min(1,ForceAvailableBackwardIonAndAtmospheric)) * 100f);
                float overrideHydrogenThrusters = 0f;

                BatchThrustOverride(ThrustersBackwardAtmosphericAndIon, overrideIonAndAtmosphericThrusters);

                if (forceRequiredHydrogen > 0)
                {
                    overrideHydrogenThrusters = Range(0f, (forceRequiredHydrogen / ForceAvailableBackwardHydrogen) * 100f);
                }
                BatchThrustOverride(ThrustersBackwardHydrogen, overrideHydrogenThrusters);

                Log?.IfDebug?.Debug("Override backward Ion/Atmospheric: {0}%", overrideIonAndAtmosphericThrusters);
                Log?.IfDebug?.Debug("Override backward Hydrogen: {0}%", overrideHydrogenThrusters);

                BatchThrustOverride(ThrustersForwardAtmosphericAndIon, 0f);
                BatchThrustOverride(ThrustersForwardHydrogen, 0f);
                Log?.IfDebug?.Debug("Set forward Thrusters to Idle ({0} %)", 0f);

                state.VelocityDelta = velocityDelta;

                state.OverrideBackwardAtmosphericAndIon = overrideIonAndAtmosphericThrusters;
                state.OverrideBackwardHydrogen = overrideHydrogenThrusters;
                state.OverrideForwardAtmosphericAndIon = 0f;
                state.OverrideForwardHydrogen = 0f;

                Log?.PopStack();
            }

            /// <summary>
            /// disable all Overrides to turn off cruise control
            /// </summary>
            public void Disable()
            {
                Log?.PushStack("Disable");
                state.Operation = "OFF";

                BatchThrustOverride(ThrustersForwardAtmosphericAndIon, 0f, OVERRIDE_FORCE_ON);
                BatchThrustOverride(ThrustersBackwardAtmosphericAndIon, 0f, OVERRIDE_FORCE_ON);
                BatchThrustOverride(ThrustersForwardHydrogen, 0f, OVERRIDE_FORCE_ON);
                BatchThrustOverride(ThrustersBackwardHydrogen, 0f, OVERRIDE_FORCE_ON);
;
                state.OverrideBackwardAtmosphericAndIon = 0f;
                state.OverrideBackwardHydrogen = 0f;
                state.OverrideForwardAtmosphericAndIon = 0f;
                state.OverrideForwardHydrogen = 0f;

                Log?.IfDebug?.Debug("All Thruster Overrides disabled");
                Log?.PopStack();
            }

            /// <summary>
            /// sets all thrusters to idel at 5% to prevent Inertiadampeners to fire.
            /// </summary>
            public void Idle()
            {
                Log?.PushStack("Idle");
                state.Operation = "IDLE";

                BatchThrustOverride(ThrustersForwardAtmosphericAndIon, 0f);
                BatchThrustOverride(ThrustersBackwardAtmosphericAndIon, 0f);
                BatchThrustOverride(ThrustersForwardHydrogen, 0f);
                BatchThrustOverride(ThrustersBackwardHydrogen, 0f);

                state.OverrideBackwardAtmosphericAndIon = 0f;
                state.OverrideBackwardHydrogen = 0f;
                state.OverrideForwardAtmosphericAndIon = 0f;
                state.OverrideForwardHydrogen = 0f;


                Log?.IfDebug?.Debug("All Thrusters set to Idle at {0} % Override", 5f);
                Log?.PopStack();
            }

            /// <summary>
            /// set an override value on multiple thrusters
            /// </summary>
            /// <param name="Thrusters"></param>
            /// <param name="value"></param>
            private void BatchThrustOverride(List<IMyThrust> Thrusters, float value, byte options = 0)
            {
                Log?.PushStack("BatchThrustOverride");
                foreach(IMyThrust Thruster in Thrusters)
                {
                    if (TurnOffOnIdle)
                    {
                        if ((options & OVERRIDE_FORCE_OFF) > 0 || (value == 0f && (options & OVERRIDE_FORCE_ON) == 0))
                        {
                            Thruster?.ApplyAction("OnOff_Off");
                        }
                        if ((options & OVERRIDE_FORCE_ON) > 0 || (value > 0f && (options & OVERRIDE_FORCE_OFF) == 0))
                        {
                            Thruster?.ApplyAction("OnOff_On");
                        }
                    } else if(value > 0f)
                    {
                        Thruster?.ApplyAction("OnOff_On");
                    }
                    Thruster?.SetValueFloat("Override", value);                    
                }
                Log?.IfDebug.Debug("ThrustOverride set to {0} on [{1}]", value, string.Join(",", Thrusters.ConvertAll<string>(t => t.CustomName)));
                Log?.PopStack();
            }

            private void UpdateForwardVelocity()
            {
                Log?.PushStack("UpdateForwardVelocity");
                this.CurrentVelocity = Vector3.TransformNormal(Controller.GetShipVelocities().LinearVelocity, Matrix.Transpose(Controller.WorldMatrix)).Z * -1;
                Log?.PopStack();
            }

            /// <summary>
            /// Reasigns Thrusters to this Control
            /// </summary>
            private void UpdateThrusterCollections()
            {
                Log?.PushStack("UpdateThrusterCollections");
                Disable();

                ThrustersForwardAtmosphericAndIon.Clear();
                ThrustersBackwardAtmosphericAndIon.Clear();
                ThrustersForwardHydrogen.Clear();
                ThrustersBackwardHydrogen.Clear();

                if(TryFindThrusters(ControllerMatrix.Forward, THRUSTER_TYPE_ATMOSPHERIC | THRUSTER_TYPE_ION, "", out ThrustersForwardAtmosphericAndIon))
                {
                    Log?.IfDebug?.Debug("Found {0}  Forwward Ion/Atmospheric Thrusters", ThrustersForwardAtmosphericAndIon.Count);
                } else
                {
                    Log?.IfWarn?.Warn("No Forwward Ion/Atmospheric Thruster found");
                }

                if (TryFindThrusters(ControllerMatrix.Backward, THRUSTER_TYPE_ATMOSPHERIC | THRUSTER_TYPE_ION, "", out ThrustersBackwardAtmosphericAndIon))
                {
                    Log?.IfDebug?.Debug("Found {0}  Backward Ion/Atmospheric Thrusters", ThrustersBackwardAtmosphericAndIon.Count);
                }
                else
                {
                    Log?.IfWarn?.Warn("No Backward Ion/Atmospheric Thruster found");
                }

                if (TryFindThrusters(ControllerMatrix.Forward, THRUSTER_TYPE_HYDROGEN, "", out ThrustersForwardHydrogen))
                {
                    Log?.IfDebug?.Debug("Found {0}  Forwward Hydrogen Thrusters", ThrustersForwardHydrogen.Count);
                }
                else
                {
                    Log?.IfWarn?.Warn("No Forwward Hydrogen Thruster found");
                }

                if (TryFindThrusters(ControllerMatrix.Backward, THRUSTER_TYPE_HYDROGEN, "", out ThrustersBackwardHydrogen))
                {
                    Log?.IfDebug?.Debug("Found {0}  Backward Hydrogen Thrusters", ThrustersBackwardHydrogen.Count);
                }
                else
                {
                    Log?.IfWarn?.Warn("No Backward Hydrogen Thruster found");
                }

                Log?.IfDebug?.Debug("List of Thrusters refreshed");

                ForceAvailableBackwardHydrogen = GetThrustforce(ThrustersBackwardHydrogen);
                ForceAvailableBackwardIonAndAtmospheric = GetThrustforce(ThrustersBackwardAtmosphericAndIon);
                ForceAvailableForwardHydrogen = GetThrustforce(ThrustersForwardHydrogen);
                ForceAvailableForwardIonAndAtmospheric = GetThrustforce(ThrustersForwardAtmosphericAndIon);

                Log?.IfDebug?.Debug("available thrust force refreshed. Atmospheric+Ion.Forward: {0}, Atmospheric+Ion.Backward: {1}, Hydrogen.Forward: {2}, Hydrogen.Backward: {3}", ForceAvailableForwardIonAndAtmospheric, ForceAvailableBackwardIonAndAtmospheric, ForceAvailableForwardHydrogen, ForceAvailableBackwardHydrogen);


                Log?.PopStack();
            }

           
            /// <summary>
            ///     Looks for thrusters 
            /// </summary>
            /// <param name="direction">the direction where the Thruster should move the Ship</param>
            /// <param name="thrustType">any combination of THRUSTER_TYPE_</param>
            /// <param name="filter">if > 0 must be contained in Thruster's Name</param>
            /// <param name="Matches"></param>
            /// <returns>true if any thrusters where found</returns>
            private bool TryFindThrusters(Vector3 direction, byte thrustType, string filter, out List<IMyThrust> Matches)
            {
                Log?.PushStack("TryFindThrusters");
                Matches = new List<IMyThrust>();
                App.GridTerminalSystem.GetBlocksOfType<IMyThrust>(Matches, (t =>
                    (filter.Length == 0 || t.CustomName.Contains(filter))
                    && (GetThrusterType(t) & thrustType) > 0
                    && GetLocalMatrix(t).Backward.Equals(direction)
                ));

                Log?.IfDebug?.Debug("Found {0} Thrusters with direction {1}, type {2}, filter \"{3}\"", Matches.Count, direction, thrustType, filter);

                Log?.PopStack();
                return (Matches.Count > 0);
            }

            /// <summary>
            /// Gets the MaxEffective force of all Thrusters passed
            /// </summary>
            /// <param name="Thrusters"></param>
            /// <returns>Newtons</returns>
            private float GetThrustforce(List<IMyThrust> Thrusters)
            {
                Log?.PushStack("GetThrustforce");
                float force = 0f;
                foreach(IMyThrust Thruster in Thrusters)
                {
                    force += Thruster?.MaxEffectiveThrust ?? 0f;                    
                }
                Log?.IfDebug?.Debug("Force: {0} N for [{1}]", force, string.Join(",",Thrusters.ConvertAll<string>(t => t.CustomName)));
                Log?.PopStack();
                return force;
            }
            

            private Matrix GetLocalMatrix(IMyTerminalBlock Block)
            {
                Log?.PushStack("GetLocalMatrix");
                Matrix localMatrix;
                Block.Orientation.GetMatrix(out localMatrix);

                Log?.PopStack();
                return localMatrix;
            }

            /// <summary>
            /// Force in Newtons required to move the Ship
            /// </summary>
            private float GetRequiredThrustForce()
            {
                Log?.PushStack("GetRequiredThrustForce");
                float force = this.Controller.CalculateShipMass().PhysicalMass * NEWTON_PER_KG;
                Log?.IfDebug?.Debug("Force: {0} N", force);
                Log?.PopStack();
                return force;
            }

            /// <summary>
            ///  returns if the THruster is Hydrogen, Ion, Atmospheric or even UNKNOWN.
            /// </summary>
            /// <param name="Thruster"></param>
            /// <param name="guess">try to guess the type based on SubTypeId</param>
            /// <returns>THRUSTER_TYPE_*</returns>
            private byte GetThrusterType(IMyThrust Thruster, bool guess = true)
            {
                Log?.PushStack("GetThrusterType");
                bool IsGuessed = false;
                byte match = THRUSTER_TYPE_UNKNOWN;
                foreach (KeyValuePair<byte, List<string>> Map in ThrusterTypeMap)
                {
                    if (Map.Value.Contains(Thruster.BlockDefinition.SubtypeId))
                    {
                        match = Map.Key;
                        break;
                    }
                }

                if (guess && match.Equals(THRUSTER_TYPE_UNKNOWN))
                {
                    IsGuessed = true;
                    if (Thruster.BlockDefinition.SubtypeId.ToLower().Contains("hydrogen"))
                    {
                        match = THRUSTER_TYPE_HYDROGEN;
                    }
                    else if (Thruster.BlockDefinition.SubtypeId.ToLower().Contains("atmospheric"))
                    {
                        match = THRUSTER_TYPE_HYDROGEN;
                    }
                    else
                    {
                        match = THRUSTER_TYPE_ION;
                    }
                }
                Log?.IfTrace?.Trace("Thruster {0} [{3}] is of Type {1}{2}", Thruster.CustomName, match, IsGuessed?" (guessed)":"", Thruster.BlockDefinition.SubtypeId);
                Log?.PopStack();
                return match;
            }

            public class State
            {
                private BMyCruiseControl CruiseControl { get; }

                public float OverrideForwardHydrogen = 0f;
                public float OverrideBackwardHydrogen = 0f;
                public float OverrideForwardAtmosphericAndIon = 0f;
                public float OverrideBackwardAtmosphericAndIon = 0f;

                public string Operation = "OFF";

                public float VelocityDelta = 0f;
                public float SetVelocity { get { return CruiseControl.TargetVelocity; } }
                public float CurrentForwardVelocity { get{ return CruiseControl.CurrentVelocity; } }

                public State(BMyCruiseControl CruiseControl)
                {
                    this.CruiseControl = CruiseControl;
                }
            }

        }

        #region View

        /// <summary>
        /// build all view informations as an argument that can be passed to an other Script to create output
        /// </summary>
        /// <param name="View"></param>
        /// <returns></returns>
        private string GetExternalViewArgument(BMyCruiseControlView View)
        {
            StringBuilder slug = new StringBuilder();
            foreach(KeyValuePair<string,object> marker in View.Context)
            {
                slug.AppendFormat("{0}:{1}\n", marker.Key, marker.Value);
            }
            return slug.ToString();
        }
        
        class BMyCruiseControlView
        {
            public Dictionary<string, object> Context = new Dictionary<string, object>();
            public Dictionary<string, string> Format = new Dictionary<string, string>();

            BMyLog4PB Log;

            public BMyCruiseControlView(BMyCruiseControl CruiseControl, BMyLog4PB Log)
            {
                Log?.PushStack("BMyView.BMyView");
                this.Log = Log;
                
                SetValue("{{NOW}}", DateTime.Now);
                SetValue("{{CURRENT_FORWARD_VELOCITY}}", Math.Round(CruiseControl.state.CurrentForwardVelocity, 2), "{0:0.00}");
                SetValue("{{TARGET_FORWARD_VELOCITY}}", Math.Round(CruiseControl.state.SetVelocity, 2), "{0:0.00}");
                SetValue("{{FORWARD_VELOCITY_DIFF}}", Math.Round(CruiseControl.state.SetVelocity - CruiseControl.state.CurrentForwardVelocity, 2), "{0:0.00}");
                SetValue("{{MODE_OF_OPERATION}}", CruiseControl.state.Operation);
                SetValue("{{OVERRIDE_ATMOSPHERIC_AND_ION}}", Math.Round(CruiseControl.state.OverrideForwardAtmosphericAndIon, 2), "{0:0.00}");
                SetValue("{{OVERRIDE_FORWARD_HYDROGEN}}", Math.Round(CruiseControl.state.OverrideForwardHydrogen, 2), "{0:0.00}");
                SetValue("{{OVERRIDE_BACKWARD_ATMOSPEHRIC_AND_ION}}", Math.Round(CruiseControl.state.OverrideBackwardAtmosphericAndIon, 2), "{0:0.00}");
                SetValue("{{OVERRIDE_BACKWARD_HYDROGEN}}", Math.Round(CruiseControl.state.OverrideBackwardHydrogen, 2), "{0:0.00}");

                Log?.PopStack();
            }

            /// <summary>
            /// add/update a marker with a new value and format
            /// </summary>
            /// <param name="key">will be replaced with value</param>
            /// <param name="value">will replace value</param>
            /// <param name="format">used by string.Format(format,value) on replacement</param>
            public void SetValue(string key, object value, string format = "{0}")
            {
                if (Context.ContainsKey(key))
                {
                    Context[key] = value;
                } else
                {
                    Context.Add(key, value);
                }

                if (Format.ContainsKey(key))
                {
                    Format[key] = format;
                } else
                {
                    Format.Add(key, format);
                }
            }

            /// <summary>
            /// the actual template to work with. 
            /// </summary>
            /// <param name="Template">prepared content containig markers</param>
            /// <returns></returns>
            public string Render(string Template)
            {
                string content = Template;
                foreach(KeyValuePair<string,object> marker in Context)
                {
                    content = content.Replace(marker.Key, string.Format(Format[marker.Key], marker.Value));
                }
                return content;
            }
        }

        private void WriteToEcho(BMyCruiseControlView View, BMyLog4PB Log)
        {
            Log?.PushStack("WriteToEcho");
            Echo(View.Render("[CruiseControl - {{NOW}}]\nSpeed: {{CURRENT_FORWARD_VELOCITY}}\nTarget: {{TARGET_FORWARD_VELOCITY}}\nDiff: {{FORWARD_VELOCITY_DIFF}}\nMode: {{MODE_OF_OPERATION}}"));
            Log?.PopStack();
        }

        private void WriteToLCD(BMyCruiseControlView View, BMyLog4PB Log)
        {
            Log?.PushStack("WriteToLCD");
            List<IMyTextPanel> LcdViews = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(LcdViews, b => b.IsWorking && b.CustomName.Contains(TAG_LCD_VIEW) && b.CubeGrid.Equals(Me.CubeGrid));

            Log?.IfDebug?.Debug("View LCDs: [{0}] {1}", LcdViews.Count, string.Join(",", LcdViews.ConvertAll<string>(x => x.CustomName)));

            if (LcdViews.Count == 0)
            {
                Log?.IfWarn?.Warn("No LCDs for output found.");
            }
            else
            {
                foreach (IMyTextPanel LCD in LcdViews)
                {
                    try
                    {
                        LCD.ShowPublicTextOnScreen();
                        LCD.WritePublicText(View.Render((LCD.CustomData.Trim().Length > 0) ? LCD.CustomData : TEMPLATE_DEFAULT_LCD));
                    }
                    catch (Exception e)
                    {
                        Log?.IfError?.Error("Failed rendering Template for \"{0}\" => {1}", LCD.CustomName, e);
                    }
                }
            }
            Log?.PopStack();
        }

        #endregion View

        #region includes 
        public class BaconArgs { public string Raw; static public BaconArgs parse(string a) { return (new Parser()).parseArgs(a); } static public string Escape(object a) { return string.Format("{0}", a).Replace(@"\", @"\\").Replace(@" ", @"\ ").Replace(@"""", @"\"""); } static public string UnEscape(string a) { return a.Replace(@"\""", @"""").Replace(@"\ ", @" ").Replace(@"\\", @"\"); } public class Parser { static Dictionary<string, BaconArgs> h = new Dictionary<string, BaconArgs>(); public BaconArgs parseArgs(string a) { if (!h.ContainsKey(a)) { var b = new BaconArgs(); b.Raw = a; var c = false; var d = false; var e = new StringBuilder(); for (int f = 0; f < a.Length; f++) { var g = a[f]; if (c) { e.Append(g); c = false; } else if (g.Equals('\\')) c = true; else if (d && !g.Equals('"')) e.Append(g); else if (g.Equals('"')) d = !d; else if (g.Equals(' ')) if (e.ToString().Equals("--")) { b.add(a.Substring(f).TrimStart()); e.Clear(); break; } else { b.add(e.ToString()); e.Clear(); } else e.Append(g); } if (e.Length > 0) b.add(e.ToString()); h.Add(a, b); } return h[a]; } } protected Dictionary<char, int> h = new Dictionary<char, int>(); protected List<string> i = new List<string>(); protected Dictionary<string, List<string>> j = new Dictionary<string, List<string>>(); public List<string> getArguments() { return i; } public int getFlag(char a) { return h.ContainsKey(a) ? h[a] : 0; } public bool hasArguments() { return i.Count > 0; } public bool hasOption(string a) { return j.ContainsKey(a); } public List<string> getOption(string a) { return j.ContainsKey(a) ? j[a] : new List<string>(); } public void add(string a) { if (a.Trim().Length > 0) if (!a.StartsWith("-")) { i.Add(a); } else if (a.StartsWith("--")) { KeyValuePair<string, string> b = k(a); string c = b.Key.Substring(2); if (!j.ContainsKey(c)) { j.Add(c, new List<string>()); } j[c].Add(b.Value); } else { string b = a.Substring(1); for (int d = 0; d < b.Length; d++) { if (this.h.ContainsKey(b[d])) { this.h[b[d]]++; } else { this.h.Add(b[d], 1); } } } } KeyValuePair<string, string> k(string a) { string[] b = a.Split(new char[] { '=' }, 2); return new KeyValuePair<string, string>(b[0], (b.Length > 1) ? b[1] : null); } public string ToArguments() { var a = new List<string>(); foreach (string argument in this.getArguments()) a.Add(Escape(argument)); foreach (KeyValuePair<string, List<string>> option in this.j) { var b = "--" + Escape(option.Key); foreach (string optVal in option.Value) a.Add(b + ((optVal != null) ? "=" + Escape(optVal) : "")); } var c = (h.Count > 0) ? "-" : ""; foreach (KeyValuePair<char, int> flag in h) c += new String(flag.Key, flag.Value); a.Add(c); return String.Join(" ", a.ToArray()); } override public string ToString() { var a = new List<string>(); foreach (string key in j.Keys) a.Add(l(key) + ":[" + string.Join(",", j[key].ConvertAll<string>(b => l(b)).ToArray()) + "]"); var c = new List<string>(); foreach (char key in h.Keys) c.Add(key + ":" + h[key].ToString()); var d = new StringBuilder(); d.Append("{\"a\":["); d.Append(string.Join(",", i.ConvertAll<string>(b => l(b)).ToArray())); d.Append("],\"o\":[{"); d.Append(string.Join("},{", a)); d.Append("}],\"f\":[{"); d.Append(string.Join("},{", c)); d.Append("}]}"); return d.ToString(); } string l(string a) { return (a != null) ? "\"" + a.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" : @"null"; } }
        public class BMyLog4PB { public const byte E_ALL = 63; public const byte E_TRACE = 32; public const byte E_DEBUG = 16; public const byte E_INFO = 8; public const byte E_WARN = 4; public const byte E_ERROR = 2; public const byte E_FATAL = 1; public BMyLog4PB IfFatal { get { return If(E_FATAL); } } public BMyLog4PB IfError { get { return If(E_ERROR); } } public BMyLog4PB IfWarn { get { return If(E_WARN); } } public BMyLog4PB IfInfo { get { return If(E_INFO); } } public BMyLog4PB IfDebug { get { return If(E_DEBUG); } } public BMyLog4PB IfTrace { get { return If(E_TRACE); } } Dictionary<string, string> j = new Dictionary<string, string>() { { "{Date}", "{0}" }, { "{Time}", "{1}" }, { "{Milliseconds}", "{2}" }, { "{Severity}", "{3}" }, { "{CurrentInstructionCount}", "{4}" }, { "{MaxInstructionCount}", "{5}" }, { "{Message}", "{6}" }, { "{Stack}", "{7}" }, { "{Origin}", "{8}" } }; Stack<string> k = new Stack<string>(); public byte Filter; public readonly Dictionary<BMyAppenderBase, string> Appenders = new Dictionary<BMyAppenderBase, string>(); string l = @"[{0}-{1}/{2}][{3}][{4}/{5}][{8}][{7}] {6}"; string m = @"[{Date}-{Time}/{Milliseconds}][{Severity}][{CurrentInstructionCount}/{MaxInstructionCount}][{Origin}][{Stack}] {Message}"; public string Format { get { return m; } set { l = o(value); m = value; } } readonly Program n; public bool AutoFlush = true; public BMyLog4PB(Program a) : this(a, E_FATAL | E_ERROR | E_WARN | E_INFO, new BMyEchoAppender(a)) { } public BMyLog4PB(Program a, byte b, params BMyAppenderBase[] c) { Filter = b; this.n = a; foreach (var Appender in c) AddAppender(Appender); } string o(string a) { var b = a; foreach (var item in j) b = b?.Replace(item.Key, item.Value); return b; } public BMyLog4PB Flush() { foreach (var AppenderItem in Appenders) AppenderItem.Key.Flush(); return this; } public BMyLog4PB PushStack(string a) { k.Push(a); return this; } public string PopStack() { return (k.Count > 0) ? k.Pop() : null; } string p() { return (k.Count > 0) ? k.Peek() : null; } public string StackToString() { if (If(E_TRACE) != null) { string[] a = k.ToArray(); Array.Reverse(a); return string.Join(@"/", a); } else return p(); } public BMyLog4PB AddAppender(BMyAppenderBase a, string b = null) { if (!Appenders.ContainsKey(a)) Appenders.Add(a, o(b)); return this; } public BMyLog4PB If(byte a) { return ((a & Filter) != 0) ? this : null; } public BMyLog4PB Fatal(string a, params object[] b) { If(E_FATAL)?.q("FATAL", a, b); return this; } public BMyLog4PB Error(string a, params object[] b) { If(E_ERROR)?.q("ERROR", a, b); return this; } public BMyLog4PB Warn(string a, params object[] b) { If(E_WARN)?.q("WARN", a, b); return this; } public BMyLog4PB Info(string a, params object[] b) { If(E_INFO)?.q("INFO", a, b); return this; } public BMyLog4PB Debug(string a, params object[] b) { If(E_DEBUG)?.q("DEBUG", a, b); return this; } public BMyLog4PB Trace(string a, params object[] b) { If(E_TRACE)?.q("TRACE", a, b); return this; } void q(string a, string b, params object[] c) { DateTime d = DateTime.Now; r e = new r(d.ToShortDateString(), d.ToLongTimeString(), d.Millisecond.ToString(), a, n.Runtime.CurrentInstructionCount, n.Runtime.MaxInstructionCount, string.Format(b, c), StackToString(), n.Me.CustomName); foreach (var item in Appenders) { var f = (item.Value != null) ? item.Value : l; item.Key.Enqueue(e.ToString(f)); if (AutoFlush) item.Key.Flush(); } } class r { public string Date; public string Time; public string Milliseconds; public string Severity; public int CurrentInstructionCount; public int MaxInstructionCount; public string Message; public string Stack; public string Origin; public r(string a, string b, string c, string d, int e, int f, string g, string h, string i) { this.Date = a; this.Time = b; this.Milliseconds = c; this.Severity = d; this.CurrentInstructionCount = e; this.MaxInstructionCount = f; this.Message = g; this.Stack = h; this.Origin = i; } public override string ToString() { return ToString(@"{0},{1},{2},{3},{4},{5},{6},{7},{8}"); } public string ToString(string a) { return string.Format(a, Date, Time, Milliseconds, Severity, CurrentInstructionCount, MaxInstructionCount, Message, Stack, Origin); } } public class BMyTextPanelAppender : BMyAppenderBase { List<string> j = new List<string>(); List<IMyTextPanel> k = new List<IMyTextPanel>(); public bool Autoscroll = true; public bool Prepend = false; public BMyTextPanelAppender(string a, Program b) { b.GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(k, (c => c.CustomName.Contains(a))); } public override void Enqueue(string a) { j.Add(a); } public override void Flush() { foreach (var Panel in k) { l(Panel); Panel.ShowTextureOnScreen(); Panel.ShowPublicTextOnScreen(); } j.Clear(); } void l(IMyTextPanel a) { if (Autoscroll) { var b = new List<string>(a.GetPublicText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)); b.AddRange(j); int c = Math.Min(m(a), b.Count); if (Prepend) b.Reverse(); a.WritePublicText(string.Join("\n", b.GetRange(b.Count - c, c).ToArray()), false); } else if (Prepend) { var b = new List<string>(a.GetPublicText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)); b.AddRange(j); b.Reverse(); a.WritePublicText(string.Join("\n", b.ToArray()), false); } else { a.WritePublicText(string.Join("\n", j.ToArray()), true); } } int m(IMyTextPanel a) { float b = a.GetValueFloat("FontSize"); if (b == 0.0f) b = 0.01f; return Convert.ToInt32(Math.Ceiling(17.0f / b)); } } public class BMyKryptDebugSrvAppender : BMyAppenderBase { IMyProgrammableBlock j; Queue<string> k = new Queue<string>(); public BMyKryptDebugSrvAppender(Program a) { j = a.GridTerminalSystem.GetBlockWithName("DebugSrv") as IMyProgrammableBlock; } public override void Flush() { if (j != null) { var a = true; while (a && k.Count > 0) if (j.TryRun("L" + k.Peek())) { k.Dequeue(); } else { a = false; } } } public override void Enqueue(string a) { k.Enqueue(a); } } public class BMyEchoAppender : BMyAppenderBase { Program j; public BMyEchoAppender(Program a) { this.j = a; } public override void Flush() { } public override void Enqueue(string a) { j.Echo(a); } } public class BMyCustomDataAppender : BMyAppenderBase { Program j; public BMyCustomDataAppender(Program a) { this.j = a; this.j.Me.CustomData = ""; } public override void Enqueue(string a) { j.Me.CustomData = j.Me.CustomData + '\n' + a; } public override void Flush() { } } public abstract class BMyAppenderBase { public abstract void Enqueue(string a); public abstract void Flush(); } }
        #endregion includes 

        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}