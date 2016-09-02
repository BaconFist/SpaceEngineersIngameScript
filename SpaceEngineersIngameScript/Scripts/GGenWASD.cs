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

namespace GGenWASD
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        GGenWASD
        ==============
        Copyright 2016 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE

        Description
        ===========

        */



        public void Main(string argument)
        {
            List<IMyTerminalBlock> MassBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyVirtualMass>(MassBlocks);
            List<IMyTerminalBlock> GravityGeneratorBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyGravityGenerator>(GravityGeneratorBlocks);
            List<IMyTerminalBlock> ThrusterBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(ThrusterBlocks);

            Dictionary<string, double> ThrusterMax = new Dictionary<string, double>();
            Dictionary<string, double> ThrusterCurrent = new Dictionary<string, double>();

            for (int i_ThrusterBlocks = 0; i_ThrusterBlocks < ThrusterBlocks.Count; i_ThrusterBlocks++)
            {
                IMyThrust Thruster = ThrusterBlocks[i_ThrusterBlocks] as IMyThrust;
                string orientation = Thruster.Orientation.ToString();
                if (!ThrusterMax.ContainsKey(orientation))
                {
                    ThrusterMax.Add(orientation, 0.0);
                }
                if (!ThrusterCurrent.ContainsKey(orientation))
                {
                    ThrusterCurrent.Add(orientation, 0.0);
                }
                ThrusterMax[orientation] = ThrusterMax[orientation] + getThrusterMaxPower(Thruster);
                ThrusterCurrent[orientation] = ThrusterCurrent[orientation] + getThrusterCurrentPower(Thruster);
            }

            List<string> Orientations = new List<string>();
            Orientations.AddRange(ThrusterMax.Keys);
            Dictionary<string, double> GValues = new Dictionary<string, double>();
            for(int i_Orientations = 0; i_Orientations < Orientations.Count; i_Orientations++)
            {
                string key = Orientations[i_Orientations];
                if (ThrusterCurrent.ContainsKey(key) && ThrusterMax.ContainsKey(key))
                {
                    double gValue = getGravityValue(ThrusterCurrent[key], ThrusterMax[key]);
                    if (!GValues.ContainsKey(key))
                    {
                        GValues.Add(key, gValue);
                    }
                }
            }

            for(int i = 0; i < GravityGeneratorBlocks.Count; i++)
            {
                IMyGravityGenerator GravGen = GravityGeneratorBlocks[i] as IMyGravityGenerator;
                string orientationGravGen = GravGen.Orientation.ToString();
                if (GValues.ContainsKey(orientationGravGen))
                {
                    GravGen.SetValueFloat("Gravity", (float)GValues[orientationGravGen]);
                }
            }            
        }

        public double getGravityValue(double cur, double max)
        {
            double multi = (max == 0)?0.0:(Math.Pow((cur / max), 2));
            double maxValueGrav = 1.0;
            double newValueGrav = maxValueGrav * multi;

            return Math.Max(0.0,Math.Min(maxValueGrav, newValueGrav));
        }

        public double getThrusterMaxPower(IMyThrust Thruster)
        {
            throw new NotImplementedException();
        }

        public double getThrusterCurrentPower(IMyThrust Thruster)
        {
            throw new NotImplementedException();
        }
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}