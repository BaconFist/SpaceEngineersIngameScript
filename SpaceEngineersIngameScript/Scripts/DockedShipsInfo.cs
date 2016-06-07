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

namespace DockedShipsInfo
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        DockedShipsInfo
        ==============
        Copyright 2016 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE

        Description
        ===========

        */

        public Program()
        {

            // The constructor, called only once every session and
            // always before any other method is called. Use it to
            // initialize your script. 
            //     
            // The constructor is optional and can be removed if not
            // needed.

        }

        public void Save()
        {

            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.

        }

        public void Main(string argument)
        {
            List<IMyTerminalBlock> Connectors = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(Connectors, (x => (x as IMyShipConnector).IsConnected && !x.CubeGrid.Equals(Me.CubeGrid)));
            List<IMyCubeGrid> CubeGrids = new List<IMyCubeGrid>();
            StringBuilder Output = new StringBuilder();
            for(int i = 0; i < Connectors.Count; i++)
            {
                IMyCubeGrid CubeGrid = Connectors[i].CubeGrid;
                if (!CubeGrids.Contains(CubeGrid))
                {
                    CubeGrids.Add(CubeGrid);
                    Output.AppendLine("[" + (CubeGrid.GridSizeEnum.Equals(MyCubeSize.Large)?"Large ":"Small ") + (CubeGrid.IsStatic?"Station":"Ship") + "] " + gridname(CubeGrid));
                }
            }
            Echo(Output.ToString());
        }

        public string gridname(IMyCubeGrid CubeGrid)
        {
            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(Blocks, (x => x.CubeGrid.Equals(CubeGrid) && (x is IMyRadioAntenna || x is IMyBeacon) && !x.CustomName.Trim().Equals("")));

            string name;
            if (Blocks.Count > 0)
            {
                string[] names = new string[Blocks.Count];
                for (int i = 0; i < Blocks.Count; i++)
                {
                    names[i] = Blocks[i].CustomName;
                }
                name = string.Join("|", names);
            } else
            {
                name = "Unnamed";
            }

            return name;
        }

        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}