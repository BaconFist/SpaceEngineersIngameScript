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

namespace LAN
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        LAN
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
            IMyTerminalBlock B = GridTerminalSystem.GetBlockWithName("Laserantenne 5 Bb.-Heck");
            if (B != null && B is IMyTerminalBlock)
            {

                List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyRadioAntenna>(Blocks);
                if (Blocks.Count > 0)
                {
                    Blocks[0].SetCustomName((B as IMyLaserAntenna).TargetCoords.ToString());
                    B.ApplyAction("OnOff");
                }
            }

        }

        public class LAN
        {
            public class Pakage {
                public string dst;
                public string src;
                public int length;
                public int checksum;
                public byte[] data;                
            }             


        }
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}