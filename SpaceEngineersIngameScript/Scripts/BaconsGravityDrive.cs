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

namespace BaconsGravityDrive
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        BaconsGravityDrive
        ==============
        Copyright 2016 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE

        Description
        ===========
        Advanced Gravity Drive
            * WASD-Controls
            * Inertia-Dmapeners

        This script let you use a gravitiy drive like any other thrusters

        Setup
        =====
            1. setup PB an timer to run this script frequently (works best with timer set to "TriggerNow" istself.
            2. one thruster for each direction
            3. one GravityGenerator with "[BGD]" in name for each direction
            4. VirtualMass

        */


        const string TAG = "[BGD]"; // change this to anything you want
        const bool useAllGrids = false; // true => script will not be limited to the PB's CubeGrid

        // DO NOT MODIFY BELOW THIS LINE \\

        public void Main(string argument)
        {
            ApplyGForce(getThrustMultiplier());
        }

        public void ApplyGForce(Dictionary<string, float> multiplierMap)
        {
            List<IMyTerminalBlock> GravityGenerators = new List<IMyTerminalBlock>();
            foreach(string oKey in multiplierMap.Keys)
            {
                GravityGenerators.Clear();
                GridTerminalSystem.GetBlocksOfType<IMyGravityGenerator>(GravityGenerators, ( x => (useAllGrids || x.CubeGrid.Equals(Me.CubeGrid)) && x.Orientation.Up.ToString().Equals(oKey) && x.CustomName.Contains(TAG)));
                for(int i = 0; i < GravityGenerators.Count; i++)
                {
                    (GravityGenerators[i] as IMyGravityGenerator).SetValueFloat("Gravity", multiplierMap[oKey] * 10);                                    
                }
            }
        }
        
        public Dictionary<string, float> getThrustMultiplier()
        {
            Dictionary<string, float> maxMap = new Dictionary<string, float>();
            Dictionary<string, float> curMap = new Dictionary<string, float>();
            List<IMyTerminalBlock> Thrusters = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(Thrusters, (x => useAllGrids || x.CubeGrid.Equals(Me.CubeGrid)));
            for (int i = 0; i < Thrusters.Count; i++)
            {
                string oKey = Thrusters[i].Orientation.Forward.ToString();
                if (!maxMap.ContainsKey(oKey))
                {
                    maxMap.Add(oKey, 0.0f);
                }
                if (!curMap.ContainsKey(oKey))
                {
                    curMap.Add(oKey, 0.0f);
                }

                maxMap[oKey] = maxMap[oKey] + (Thrusters[i] as IMyThrust).MaxThrust;
                curMap[oKey] = curMap[oKey] + (Thrusters[i] as IMyThrust).CurrentThrust;
            }
            Dictionary<string, float> Result = new Dictionary<string, float>();
            foreach(string key in maxMap.Keys)
            {
                if (!Result.ContainsKey(key))
                {
                    Result.Add(key, curMap[key] / maxMap[key]);
                }
            }

            return Result;
        }


        public float getThrustMultiplier(string orientation)
        {
            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(Blocks);
            float maxThrust = 0;
            float currentThrust = 0;
            for(int i = 0; i < Blocks.Count; i++)
            {
                IMyThrust Thrust = Blocks[i] as IMyThrust;
                maxThrust += Thrust.MaxThrust;
                currentThrust += Thrust.CurrentThrust;
            }

            return currentThrust / maxThrust;
        }
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}