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
        Show distance to GPS Waypoints in LCD-Panels

          Setup:
            1. Load script to Programable Block and run it repeatedly with a Timer Block
            2. Add "[GPSDistance]" to the Remote Control with the Waypoints to Display
            3. Add "[GPSDistance]" to the LCD-Panel to display the distances on
          
          Tweaks:
            * change [GPSDistance] to you needs by passing something else as an argument
            * PublicTitle will be displayed as Heading in the LCD. Clear it to hide it.
            * Change Sorting of Waypoints by reordering it in the Remote Control
        */

        string TAG = "[GPSDistance]";

        public void Main(string argument)
        {
            TAG = (argument.Trim().Length > 0) ? argument : TAG;
            IMyShipController OriginCockpit = findPilotedCockpit();
            Vector3D origin = (OriginCockpit != null) ? OriginCockpit.GetPosition() : Me.GetPosition();
            List<IMyRemoteControl> RemoteControls = new List<IMyRemoteControl>();
            List<IMyTextPanel> TextPanels = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(RemoteControls, (x => x.CubeGrid.Equals(Me.CubeGrid) && x.CustomName.Contains(TAG)));
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(TextPanels, (x => x.CubeGrid.Equals(Me.CubeGrid) && x.CustomName.Contains(TAG)));
            if (TextPanels.Count > 0 && RemoteControls.Count > 0)
            {
                StringBuilder Distances = new StringBuilder();
                foreach (IMyRemoteControl RemoteControl in RemoteControls)
                {
                    List<MyWaypointInfo> Waypoints = new List<MyWaypointInfo>();
                    RemoteControl.GetWaypointInfo(Waypoints);
                    foreach (MyWaypointInfo Waypoint in Waypoints)
                    {
                        Distances.AppendLine(formatDistance(Vector3D.Distance(origin, Waypoint.Coords)) + ": " + Waypoint.Name);                        
                    }
                }
                foreach(IMyTextPanel TextPanel in TextPanels)
                {
                    TextPanel.WritePublicText(((TextPanel.GetPublicTitle().Trim().Length > 0)? TextPanel.GetPublicTitle().Trim() +"\n":"") + Distances.ToString());
                    TextPanel.ShowPublicTextOnScreen();
                }
            }
        }

        private IMyShipController findPilotedCockpit()
        {
            List<IMyShipController> Cockpits = new List<IMyShipController>();
            GridTerminalSystem.GetBlocksOfType<IMyShipController>(Cockpits, (x=> x.IsUnderControl && x.CubeGrid.Equals(Me.CubeGrid)));

            return (Cockpits.Count > 0) ? Cockpits[0] : null;
        }

        const double DIST_LS = 299792458;
        const double DIST_MM = 1000000;
        const double DIST_KM = 1000;
        private string formatDistance(double distance)
        {
            string label = " m ";
            double buffer = distance;
            if (buffer >= DIST_LS)
            {
                buffer /= DIST_LS;
                label = " ls";
            } else if (buffer > DIST_MM)
            {
                buffer /= DIST_MM;
                label = " Mm";
            } else if(buffer > DIST_KM)
            {
                buffer /= DIST_KM;
                label = " km";
            }
            return Math.Round(buffer, 2).ToString("### ### ### ###.00") + label;
        }
        
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}