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

namespace Block_Prenamer
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        Block_Prenamer
        ==============
        Copyright 2017 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE

        Description
        ===========

            Block Renamer HELP
            ======================
            This script is meant to rename new Blocks.
            run script with argument "Light Runway"
            and all new placed blocks will be named
            "Light Runway".

            WARNING: DO NOT RUN THIS WITH A TIMER!
            
            Type desired name in the argument and run script. 
            Any Block since last run or last time compiled 
            will get this name.
        */

        List<IMyTerminalBlock> LastKnownBlocks = new List<IMyTerminalBlock>();
        string NewBlockName = "";


        public Program()
        {
            Help();
            GridTerminalSystem.GetBlocks(LastKnownBlocks);
        }

        public void Save()
        {

        }

        public void Main(string argument)
        {
            Echo(@"You can find help in CustomData");
            if(argument.Trim().Length > 0)
            {
                NewBlockName = argument;
            }
            Echo(string.Format(@"new Blocknames: ""{0}""", NewBlockName));

            if (NewBlockName.Trim().Length > 0)
            {
                List<IMyTerminalBlock> NewBlocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(NewBlocks, (b => Me.CubeGrid.Equals(b.CubeGrid) && !LastKnownBlocks.Contains(b)));
                foreach (IMyTerminalBlock Block in NewBlocks)
                {
                    UpdateName(Block);
                }
            } else
            {
                Echo("script canceled: new block must not be empty.");
            }

            GridTerminalSystem.GetBlocks(LastKnownBlocks);
        }

        private void UpdateName(IMyTerminalBlock Block)
        {
            Echo(string.Format(@"Renamed :'{0}' => '{1}'", Block.CustomName, NewBlockName));
            Block.CustomName = NewBlockName;
        }

        private void Help()
        {
            StringBuilder help = new StringBuilder();
            help.AppendLine(@"Block Renamer HELP");
            help.AppendLine(@"======================");
            help.AppendLine(@"This script is meant to rename new Blocks.");
            help.AppendLine(@"run script with argument ""Light Runway""");
            help.AppendLine(@"and all new placed blocks will be named");
            help.AppendLine(@"""Light Runway"".");
            help.AppendLine(@"");
            help.AppendLine(@"WARNING: DO NOT RUN THIS WITH A TIMER!");
            help.AppendLine(@"");
            help.AppendLine(@"Type desired name in the argument and run script.");
            help.AppendLine(@"Any Block since last run or last time compiled"); 
            help.AppendLine(@"will get this name.");
            Me.CustomData = help.ToString();            
        }

        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}