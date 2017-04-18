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

namespace DedicatedTimerFix
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        DedicatedTimerFix
        ==============
        Copyright 2017 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE

        Description
        ===========
            // Workaround for Flashing Timers.
            This will trigger timers afeter a specific delay.

            Place your timers in CustomData of this PB like this:
            SECONDS:NAME

            this will trigger all Times called NAME and run all ProgrammableBlocks called NAME (without any argument)
        */

        int hashCustomData = 0;
        List<BMyAction> Actions = new List<BMyAction>();

        public void Main(string argument)
        {
            TryUpdateActions();
            DateTime NOW = DateTime.Now;
            foreach(BMyAction Action in Actions)
            {
                if(Action.TimeNextCall <= NOW)
                {
                    FEcho("RUN -> {0}:{1}",Action.Delay,Action.Blockname);
                    Action.updateTime();
                    foreach(IMyTerminalBlock Block in getBlocks(Action.Blockname))
                    {
                        if(Block is IMyTimerBlock)
                        {
                            (Block as IMyTimerBlock).Trigger();
                        } else if(Block is IMyProgrammableBlock)
                        {
                            (Block as IMyProgrammableBlock).TryRun("");
                        }
                    }
                } else
                {
                    FEcho("WAIT -> {2}/{0}:{1}", Action.Delay, Action.Blockname, Action.TimeNextCall.Subtract(NOW).Seconds);
                    
                }
            }
        }

        public List<IMyTerminalBlock> getBlocks(string name)
        {
            List<IMyTerminalBlock> buffer = new List<IMyTerminalBlock>();
            if (name.StartsWith("T:"))
            {
                GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(buffer, (b => (b is IMyTimerBlock  || b is IMyProgrammableBlock) && b.CustomName.Contains(name.Substring(2)) && Me.CubeGrid.Equals(b.CubeGrid)));
            } else
            {
                GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(buffer, (b => (b is IMyTimerBlock || b is IMyProgrammableBlock) && b.CustomName.Contains(name)));
            }

            return buffer;
        }

        public void TryUpdateActions()
        {
            int currentHash = Me.CustomData.GetHashCode();
            if (currentHash.Equals(hashCustomData))
            {
                return;
            }
            Actions.Clear();
            string[] code = Me.CustomData.Split(new Char[] {'\n','\r'},StringSplitOptions.RemoveEmptyEntries);

            foreach(string line in code)
            {
                string[] arguments = line.Split(new Char[]{':'}, 2);
                if(arguments.Length == 2)
                {
                    int seconds = 0;
                    if(int.TryParse(arguments[0], out seconds))
                    {
                        Actions.Add(new BMyAction(arguments[1], seconds));
                    }
                }
            }
            hashCustomData = currentHash;
        }

        class BMyAction
        {
            public DateTime TimeNextCall;
            public String Blockname;
            public int Delay;

            public BMyAction(String b, int d)
            {
                Blockname = b;
                Delay = d;
                updateTime();
            }

            public void updateTime()
            {
                TimeNextCall = DateTime.Now.AddSeconds(Delay);
            }
        }

        public void FEcho(string msg, params object[] values)
        {
            Echo(string.Format(msg, values));
        }
        
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}