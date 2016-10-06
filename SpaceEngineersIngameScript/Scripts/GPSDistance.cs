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

namespace GPSDistance
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        GPSDisplay
        ==============
        Copyright 2016 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE

        Description
        ===========
        
        */

        string TAG = "[GPSDistance]";

        public void Main(string argument)
        {
            TAG = (argument.Trim().Length > 0) ? argument : TAG;
            List<IMyRemoteControl> RemoteControls = new List<IMyRemoteControl>();
            List<IMyTextPanel> Panels = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(RemoteControls, (x => x.CubeGrid.Equals(Me.CubeGrid) && x.CustomName.Contains(TAG)));
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(Panels, (x => x.CubeGrid.Equals(Me.CubeGrid) && x.CustomName.Contains(TAG)));
            if (Panels.Count > 0 && RemoteControls.Count > 0)
            {
                StringBuilder Distances = new StringBuilder();
                foreach (IMyRemoteControl RC in RemoteControls)
                {
                    List<MyWaypointInfo> WPs = new List<MyWaypointInfo>();
                    RC.GetWaypointInfo(WPs);
                    foreach (MyWaypointInfo WP in WPs)
                    {
                        Distances.AppendLine(WP.Name + " => " + Math.Round(Vector3D.Distance(Me.GetPosition(), WP.Coords), 2).ToString("### ### ### ###.00") + "m");
                    }
                }
                foreach(IMyTextPanel P in Panels)
                {
                    P.WritePublicText(Distances.ToString());
                }
            }
        }

        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}