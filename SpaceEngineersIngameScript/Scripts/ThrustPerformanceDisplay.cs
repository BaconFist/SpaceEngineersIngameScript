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

namespace ThrustPerformanceDisplay
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        ThrustPerformanceDisplay
        ==============
        Copyright 2017 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE

        Description
        ===========

        */

        string tag = "[TPD]";
        IMyShipController ctrl;

        float UpCur = 0.0f;
        float DownCur = 0.0f;
        float LeftCur = 0.0f;
        float RightCur = 0.0f;
        float ForwardCur = 0.0f;
        float BackwardCur = 0.0f;

        float UpMax = 0.0f;
        float DownMax = 0.0f;
        float LeftMax = 0.0f;
        float RightMax = 0.0f;
        float ForwardMax = 0.0f;
        float BackwardMax = 0.0f;

        float UpP = 0.0f;
        float DownP = 0.0f;
        float LeftP = 0.0f;
        float RightP = 0.0f;
        float ForwardP = 0.0f;
        float BackwardP = 0.0f;

        string defaultTemplate = "{DateTime}\n"
                + "Forward: {ForwardP}% {ForwardCur}/{ForwardMax} N\n"
                + "Backward: {BackwardP}% {BackwardCur}/{BackwardMax} N\n"
                + "Up: {UpP}% {UpCur}/{UpMax} N\n"
                + "Down: {DownP}% {DownCur}/{DownMax} N\n"
                + "Left: {LeftP}% {LeftCur}/{LeftMax} N\n"
                + "Right: {RightP}% {RightCur}/{RightMax} N\n"
            ;



        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }   
                
        public void Main(string argument, UpdateType updateSource)
        {
            UpdateTag(argument);
            Reset();
            Run();
            ToLCD();
            PrintInfo();
        }

        private void ToLCD()
        {
            List<IMyTextPanel> LCDs = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(LCDs, (l => l.CustomName.Contains(tag) && l.CubeGrid.Equals(Me.CubeGrid)));
            foreach(IMyTextPanel L in LCDs)
            {
                if(L.CustomData.Trim().Length == 0)
                {
                    L.CustomData = defaultTemplate;
                }
                string template = L.CustomData;
                string content = template
                    .Replace("{DateTime}", string.Format("{0}", DateTime.Now))
                    .Replace("{UpCur}", string.Format("{0:0.00}", Math.Round(UpCur, 2)))
                    .Replace("{DownCur}", string.Format("{0:0.00}", Math.Round(DownCur, 2)))
                    .Replace("{LeftCur}", string.Format("{0:0.00}", Math.Round(LeftCur, 2)))
                    .Replace("{RightCur}", string.Format("{0:0.00}", Math.Round(RightCur, 2)))
                    .Replace("{ForwardCur}", string.Format("{0:0.00}", Math.Round(ForwardCur, 2)))
                    .Replace("{BackwardCur}", string.Format("{0:0.00}", Math.Round(BackwardCur, 2)))
                    .Replace("{UpMax}", string.Format("{0:0.00}", Math.Round(UpMax, 2)))
                    .Replace("{DownMax}", string.Format("{0:0.00}", Math.Round(DownMax, 2)))
                    .Replace("{LeftMax}", string.Format("{0:0.00}", Math.Round(LeftMax, 2)))
                    .Replace("{RightMax}", string.Format("{0:0.00}", Math.Round(RightMax, 2)))
                    .Replace("{ForwardMax}", string.Format("{0:0.00}", Math.Round(ForwardMax, 2)))
                    .Replace("{BackwardMax}", string.Format("{0:0.00}", Math.Round(BackwardMax, 2)))
                    .Replace("{UpP}", string.Format("{0:0.00}", Math.Round(UpP, 2)))
                    .Replace("{DownP}", string.Format("{0:0.00}", Math.Round(DownP, 2)))
                    .Replace("{LeftP}", string.Format("{0:0.00}", Math.Round(LeftP, 2)))
                    .Replace("{RightP}", string.Format("{0:0.00}", Math.Round(RightP, 2)))
                    .Replace("{ForwardP}", string.Format("{0:0.00}", Math.Round(ForwardP, 2)))
                    .Replace("{BackwardP}", string.Format("{0:0.00}", Math.Round(BackwardP, 2)));
                L.WritePublicText(content);
            }
        }

        private void Reset()
        {
            UpCur = 0.0f;
            DownCur = 0.0f;
            LeftCur = 0.0f;
            RightCur = 0.0f;
            ForwardCur = 0.0f;
            BackwardCur = 0.0f;

            UpMax = 0.0f;
            DownMax = 0.0f;
            LeftMax = 0.0f;
            RightMax = 0.0f;
            ForwardMax = 0.0f;
            BackwardMax = 0.0f;

            UpP = 0.0f;
            DownP = 0.0f;
            LeftP = 0.0f;
            RightP = 0.0f;
            ForwardP = 0.0f;
            BackwardP = 0.0f;
        }

        private void Run()
        {
            List<IMyShipController> ControllerList = new List<IMyShipController>();
            GridTerminalSystem.GetBlocksOfType<IMyShipController>(ControllerList, (c => c.CustomName.Contains(tag) && c.CubeGrid.Equals(Me.CubeGrid)));
            ctrl = ControllerList[0]??null;
            if(ctrl != null)
            {
                Matrix cm;
                ctrl.Orientation.GetMatrix(out cm);
                List<IMyThrust> Thrusters = new List<IMyThrust>();
                GridTerminalSystem.GetBlocksOfType<IMyThrust>(Thrusters, (t => t.CubeGrid.Equals(Me.CubeGrid)));

                    

                foreach(IMyThrust t in Thrusters)
                {
                    Matrix tm;
                    t.Orientation.GetMatrix(out tm);
                    if (tm.Backward.Equals(cm.Forward))
                    {
                        ForwardCur += t.CurrentThrust;
                        ForwardMax += t.MaxEffectiveThrust;
                    }
                    else if (tm.Backward.Equals(cm.Backward))
                    {
                        BackwardCur += t.CurrentThrust;
                        BackwardMax+= t.MaxEffectiveThrust;
                    }
                    else if (tm.Backward.Equals(cm.Left))
                    {
                        LeftCur+= t.CurrentThrust;
                        LeftMax += t.MaxEffectiveThrust;
                    }
                    else if (tm.Backward.Equals(cm.Right))
                    {
                        RightCur += t.CurrentThrust;
                        RightMax += t.MaxEffectiveThrust;
                    }
                    else if (tm.Backward.Equals(cm.Up))
                    {
                        UpCur += t.CurrentThrust;
                        UpMax += t.MaxEffectiveThrust;
                    }
                    else if (tm.Backward.Equals(cm.Down))
                    {
                        DownCur += t.CurrentThrust;
                        DownMax += t.MaxEffectiveThrust;
                    }                    
                }                

                ForwardP = (ForwardMax == 0) ? 0 : (ForwardCur / ForwardMax) * 100;
                BackwardP = (BackwardMax == 0) ? 0 : (BackwardCur / BackwardMax) * 100;
                LeftP = (LeftMax == 0) ? 0 : (LeftCur / LeftMax) * 100;
                RightP = (RightMax == 0) ? 0 : (RightCur / RightMax) * 100;
                UpP = (UpMax == 0) ? 0 : (UpCur / UpMax) * 100;
                DownP = (DownMax == 0) ? 0 : (DownCur / DownMax) * 100;

            }
            else
            {
                Echo("ERROR: Cockpit not found!");
            }
        }

        private void UpdateTag(string argument)
        {
            argument = argument.Trim();
            if (argument.ToLowerInvariant().Equals("default"))
            {
                tag = "[TPD]";
            }
            else
            {
                tag = Storage.Length > 0 ? Storage : tag;
                tag = argument.Length > 0 ? argument : tag;
            }
            Storage = tag;            
        }

        private void PrintInfo()
        {
            Echo("TAG: \"" + tag + "\" (reset with \"DEFAULT\"");
            Echo("use `argument` to set a new TAG");
            Echo(tag + " is used to find cockpit and LCD Panel.");
        }        
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}