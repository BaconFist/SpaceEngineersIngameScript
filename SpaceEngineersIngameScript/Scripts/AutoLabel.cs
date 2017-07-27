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

namespace AutoLabel
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        AutoLabel
        ==============
        Copyright 2017 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE

        Description
        ===========

        */

        float modTextWidth = 26.5f;
        float maxFontSize = 2.7f;
        string tag = "[AutoLabel]";

        public void Main(string argument)
        {
            try
            {
                updatePanels();
            } catch (Exception E)
            {
                Echo(E.Message);
            }
        }
        
        public void updatePanels()
        {
            List<IMyTextPanel> panels = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(panels, p => p.CustomName.Contains(tag));
            foreach (IMyTextPanel panel in panels)
            {
                string label = "Block not found.";
                float fontSize = 1.65625f;
                IMyTerminalBlock target;
                if (findTerminalBlockBehind(panel, out target))
                {
                    label = target.CustomName.Trim();
                    fontSize = modTextWidth / label.Length;
                    if (fontSize > maxFontSize)
                    {
                        fontSize = maxFontSize;
                    }
                }
                panel.ShowPublicTextOnScreen();
                panel.WritePublicText(label);
                panel.SetValueFloat("FontSize", fontSize);
                panel.SetValue<long>("Font", 1147350002); // mono

            }
        }

            public bool findTerminalBlockBehind(IMyTerminalBlock Panel, out IMyTerminalBlock Match)
            {
                IMySlimBlock SlimBlock = Panel.CubeGrid.GetCubeBlock(Panel.Position + getMountPointVector(Panel));
                if (SlimBlock != null && SlimBlock.FatBlock != null && SlimBlock.FatBlock is IMyTerminalBlock)
                {
                    Match = SlimBlock.FatBlock as IMyTerminalBlock;
                    return true;
                }
                else
                {
                    Match = null;
                    return false;
                }
            }

            private Vector3I getMountPointVector(IMyTerminalBlock Block)
            {
                Matrix localMatrix;
                Block.Orientation.GetMatrix(out localMatrix);
                Vector3 buffer;
                switch (Block.BlockDefinition.SubtypeName)
                {
                    case "LargeBlockCorner_LCD_Flat_1":
                    case "LargeBlockCorner_LCD_Flat_2":
                    case "LargeBlockCorner_LCD_1":
                    case "LargeBlockCorner_LCD_2":
                    case "SmallBlockCorner_LCD_Flat_1":
                    case "SmallBlockCorner_LCD_Flat_2":
                    case "SmallBlockCorner_LCD_1":
                    case "SmallBlockCorner_LCD_2":
                        buffer = localMatrix.Down;
                        break;
                    default:
                        buffer = localMatrix.Forward;
                        break;
                }

                return new Vector3I(buffer);
            }
        
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}