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

namespace RemoteTerminal
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        public void Main(string argument)
        {
            IMyRadioAntenna Radio = GridTerminalSystem.GetBlockWithName("Antenna") as IMyRadioAntenna;
            if(Radio is IMyRadioAntenna)
            {
                Echo(string.Format(@"Radio Found ""{0}"", IsFunctional:{1}, IsWorking:{2}, IsBroadcasting:{3}", Radio.CustomName, Radio.IsFunctional, Radio.IsWorking, Radio.IsBroadcasting));
                if(Radio.TransmitMessage(argument, MyTransmitTarget.Everyone))
                {
                    Echo(string.Format(@"sent ""{0}"" to everyone", argument));
                } else
                {
                    Echo("Can't send message");
                }
                
            } else
            {
                Echo("Radio not found");
            }
        }
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}