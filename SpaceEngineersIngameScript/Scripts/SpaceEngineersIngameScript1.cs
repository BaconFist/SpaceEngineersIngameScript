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

namespace SpaceEngineersIngameScript1
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        SpaceEngineersIngameScript1
        ==============
        Copyright 2017 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE

        Description
        ===========

        */

        public void Main(string argument)
        {

            List<IMyThrust> TL = new List<IMyThrust>();
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(TL);
            if(TL.Count > 0)
            {
                IMyThrust T = TL[0];

                Vector3I directrionFromThruster = T.GridThrustDirection;

                List<IMyShipController> CL = new List<IMyShipController>();
                GridTerminalSystem.GetBlocksOfType<IMyShipController>(CL);
                string s = "";
                foreach(IMyShipController C in CL)
                {
                    Matrix m;
                    C.Orientation.GetMatrix(out m);
                    s += $"- [{C.CustomName}] T: {directrionFromThruster} => F: {m.Forward}\n";
                    s += $"- [{C.CustomName}] T: {directrionFromThruster} => B: {m.Backward}\n";
                    s += $"- [{C.CustomName}] T: {directrionFromThruster} => L: {m.Left}\n";
                    s += $"- [{C.CustomName}] T: {directrionFromThruster} => R: {m.Right}\n";
                    s += $"- [{C.CustomName}] T: {directrionFromThruster} => U: {m.Up}\n";
                    s += $"- [{C.CustomName}] T: {directrionFromThruster} => D: {m.Down}\n";
                    s += $"-------------------------------------------\n";
                }
                Me.CustomData = s;
                Echo($"{CL.Count} Controllers");
            } else
            {
                Echo("no Thruster");
            }

        }
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}