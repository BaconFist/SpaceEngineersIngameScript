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

namespace ShipyardGateControl
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        ShipyardGateControl
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

        string panelGate1 = "[Status Gate 1]";
        string panelGate2 = "[Status Gate 2]";

        const string open1 = "Gate 1 Open";
        const string close1 = "Gate 1 Close";
        const string open2 = "Gate 2 Open";
        const string close2 = "Gate 2 Close";

        string label1 = "Gate 1";
        string label2 = "Gate 2";

        public void Main(string argument)
        {
            Echo(string.Format("Arguments:\n  {0}: switch panel state for {1} to open\n  {2}: switch panel state for {1} to close\n  {3}: switch panel state for {4} to open\n  {5}: switch panel state for {4} to close", open1, label1, close1, open2, label2, close2));
            IMyTextPanel GatePanel = null;
            switch (argument)
            {
                case open1:
                    GatePanel = getPanelGate1();
                    GatePanel?.WritePrivateText(getImageOpen(label1)?.ToString());
                    Echo(string.Format("Try Open {0}", label1));
                    break;
                case close1:
                    GatePanel = getPanelGate1();
                    GatePanel?.WritePrivateText(getImageClose(label1)?.ToString());
                    Echo(string.Format("Try Close {0}", label1));
                    break;
                case open2:
                    GatePanel = getPanelGate2();
                    GatePanel?.WritePrivateText(getImageOpen(label2)?.ToString());
                    Echo(string.Format("Try Open {0}", label2));
                    break;
                case close2:
                    GatePanel = getPanelGate2();
                    GatePanel?.WritePrivateText(getImageClose(label2)?.ToString());
                    Echo(string.Format("Try Close {0}", label2));
                    break;
                default:
                    Echo(string.Format("ERROR: Unsupported argument: \"{0}\".", argument));
                    break;
            }
            if(GatePanel == null)
            {
                Echo(string.Format("ERROR: Can't find Panel make sure it containse \"{0}\" in it's Name", (argument.Contains("1")?panelGate1:(argument.Contains("2")? panelGate2 :panelGate1 +"\" or \""+ panelGate2))));
            }
        }

        private IMyTextPanel getPanelGate1()
        {
            List<IMyTextPanel> Buffer = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(Buffer, (P => P.CustomName.Contains(panelGate1)));
            return (Buffer.Count > 0)?Buffer[0]:null;
        }

        private IMyTextPanel getPanelGate2()
        {
            List<IMyTextPanel> Buffer = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(Buffer, (P => P.CustomName.Contains(panelGate2)));
            return (Buffer.Count > 0) ? Buffer[0] : null;
        }

        private StringBuilder getImageOpen(string gate)
        {
            return (new StringBuilder())
                .AppendLine("--width=53 --height=28")
                .AppendLine("color Green")

                .AppendLine("moveTo 2,16")
                .AppendLine("lineTo 7,8")
                .AppendLine("lineTo 12,16")

                .AppendLine("moveTo 22,16")
                .AppendLine("lineTo 27,8")
                .AppendLine("lineTo 32,16")

                .AppendLine("moveTo 42,16")
                .AppendLine("lineTo 47,8")
                .AppendLine("lineTo 52,16")

                .AppendLine("Color Yellow")
                .AppendLine("moveTo 10,1")
                .AppendLine("text - " + gate)
                .AppendLine("moveTo 8,20")
                .AppendLine("text - o p e n")
            ;
        }

        private StringBuilder getImageClose(string gate)
        {
            return (new StringBuilder())
                .AppendLine("--width=53 --height=28")
                .AppendLine("color Red")

                .AppendLine("moveTo 2,8")
                .AppendLine("lineTo 7,16")
                .AppendLine("lineTo 12,8")

                .AppendLine("moveTo 22,8")
                .AppendLine("lineTo 27,16")
                .AppendLine("lineTo 32,8")

                .AppendLine("moveTo 42,8")
                .AppendLine("lineTo 47,16")
                .AppendLine("lineTo 52,8")

                .AppendLine("Color Yellow")
                .AppendLine("moveTo 10,1")
                .AppendLine("text - " + gate)
                .AppendLine("moveTo 12,20")
                .AppendLine("text - close")
            ;
        }
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}