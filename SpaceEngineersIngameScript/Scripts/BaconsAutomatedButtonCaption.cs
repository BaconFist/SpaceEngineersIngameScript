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

namespace BaconsAutomatedButtonCaption
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        BaconsAutomatedButtonCaption
        ==============
        Copyright 2017 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE

        Description
        ===========
            Shows captions of buttons on an lcd.

            What you need to do:
            1. load Scrit in a Programmable Block
            2. set up a Timer to start itself and the Programmable Block

            What the script does:
            1. looks for LCDs with [BPC] in name
            2. looks one block around this lcd if there is any button panel
            3. displays commds of butonpanel on LCD

            
            In case you use a lot of LCDs it could take a few runs to update all LCDs.



        */

        Dictionary<IMyTextPanel, IMyButtonPanel> Cache = new Dictionary<IMyTextPanel, IMyButtonPanel>();
        Queue<IMyTextPanel> PanelQueue = new Queue<IMyTextPanel>();

        const int LOAD_LIMIT = 15000;

        public void Main(string argument)
        {
            string tag = (argument.Trim().Length > 0) ? argument.Trim() : "[BPC]";
            List<IMyTextPanel> Panels = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(Panels, (p => p.CustomName.Contains(tag)));
            foreach (IMyTextPanel Panel in Panels)
            {
                if (!PanelQueue.Contains(Panel))
                {
                    PanelQueue.Enqueue(Panel);
                }
            }
            progressQueue();
        }
        

        public void progressQueue()
        {
            while (Runtime.CurrentInstructionCount < LOAD_LIMIT && PanelQueue.Count > 0)
            {
                progressPanel(PanelQueue.Dequeue());
            }
        }

        public void progressPanel(IMyTextPanel Panel)
        {
            IMyButtonPanel ButtonPanel = getButtonPanel(Panel);
            if(ButtonPanel != null)
            {
                StringBuilder slug = new StringBuilder();
                for(int i = 0; i < 4; i++)
                {
                    if(ButtonPanel.IsButtonAssigned(i) && ButtonPanel.HasCustomButtonName(i))
                    {
                        slug.AppendLine(string.Format(@"[{0}]: {1}", i+1, ButtonPanel.GetButtonName(i)));
                    } else
                    {
                        slug.AppendLine(string.Format(@"[{0}]: ...", i + 1));
                    }
                }
                Panel.WritePublicText(slug.ToString());
                Panel.WritePublicTitle(ButtonPanel.CustomName);
                Panel.SetValueFloat("FontSize", 0.65f);
                Panel.ShowPublicTextOnScreen();
            }
        }
        
        public IMyButtonPanel getButtonPanel(IMyTextPanel Panel)
        {
            if (Cache.ContainsKey(Panel))
            {
                IMyTerminalBlock buffer = GridTerminalSystem.GetBlockWithId(Cache[Panel].EntityId);
                if (buffer != null && buffer is IMyButtonPanel)
                {
                    return buffer as IMyButtonPanel;      
                } else
                {
                    Cache.Remove(Panel);
                }
            }
            IMyButtonPanel ButtonPanel = findButtonPanelFromLCD(Panel);
            if(ButtonPanel != null && ButtonPanel is IMyButtonPanel)
            {
                Cache.Add(Panel, ButtonPanel);
                return ButtonPanel;
            }

            return null;
        }

        public IMyButtonPanel findButtonPanelFromLCD(IMyTextPanel Panel)
        {

            IMyButtonPanel ButtonPanel = null;

            if (TryFindButtonPanel(new Vector3I(Panel.Position.X - 1, Panel.Position.Y, Panel.Position.Z), Panel.CubeGrid, out ButtonPanel))
            {
                return ButtonPanel;
            }

            if (TryFindButtonPanel(new Vector3I(Panel.Position.X + 1, Panel.Position.Y, Panel.Position.Z), Panel.CubeGrid, out ButtonPanel))
            {
                return ButtonPanel;
            }

            if (TryFindButtonPanel(new Vector3I(Panel.Position.X, Panel.Position.Y - 1, Panel.Position.Z), Panel.CubeGrid, out ButtonPanel))
            {
                return ButtonPanel;
            }

            if (TryFindButtonPanel(new Vector3I(Panel.Position.X, Panel.Position.Y + 1, Panel.Position.Z), Panel.CubeGrid, out ButtonPanel))
            {
                return ButtonPanel;
            }

            if (TryFindButtonPanel(new Vector3I(Panel.Position.X, Panel.Position.Y, Panel.Position.Z - 1), Panel.CubeGrid, out ButtonPanel))
            {
                return ButtonPanel;
            }

            if (TryFindButtonPanel(new Vector3I(Panel.Position.X, Panel.Position.Y, Panel.Position.Z + 1), Panel.CubeGrid, out ButtonPanel))
            {
                return ButtonPanel;
            }

            if (TryFindButtonPanel(new Vector3I(Panel.Position.X - 1, Panel.Position.Y - 1, Panel.Position.Z), Panel.CubeGrid, out ButtonPanel))
            {
                return ButtonPanel;
            }

            if (TryFindButtonPanel(new Vector3I(Panel.Position.X - 1, Panel.Position.Y + 1, Panel.Position.Z), Panel.CubeGrid, out ButtonPanel))
            {
                return ButtonPanel;
            }

            if (TryFindButtonPanel(new Vector3I(Panel.Position.X + 1, Panel.Position.Y - 1, Panel.Position.Z), Panel.CubeGrid, out ButtonPanel))
            {
                return ButtonPanel;
            }

            if (TryFindButtonPanel(new Vector3I(Panel.Position.X + 1, Panel.Position.Y + 1, Panel.Position.Z), Panel.CubeGrid, out ButtonPanel))
            {
                return ButtonPanel;
            }

            if (TryFindButtonPanel(new Vector3I(Panel.Position.X - 1, Panel.Position.Y - 1, Panel.Position.Z - 1), Panel.CubeGrid, out ButtonPanel))
            {
                return ButtonPanel;
            }

            if (TryFindButtonPanel(new Vector3I(Panel.Position.X - 1, Panel.Position.Y + 1, Panel.Position.Z - 1), Panel.CubeGrid, out ButtonPanel))
            {
                return ButtonPanel;
            }

            if (TryFindButtonPanel(new Vector3I(Panel.Position.X + 1, Panel.Position.Y - 1, Panel.Position.Z - 1), Panel.CubeGrid, out ButtonPanel))
            {
                return ButtonPanel;
            }

            if (TryFindButtonPanel(new Vector3I(Panel.Position.X + 1, Panel.Position.Y + 1, Panel.Position.Z - 1), Panel.CubeGrid, out ButtonPanel))
            {
                return ButtonPanel;
            }

            if (TryFindButtonPanel(new Vector3I(Panel.Position.X - 1, Panel.Position.Y - 1, Panel.Position.Z + 1), Panel.CubeGrid, out ButtonPanel))
            {
                return ButtonPanel;
            }

            if (TryFindButtonPanel(new Vector3I(Panel.Position.X - 1, Panel.Position.Y + 1, Panel.Position.Z + 1), Panel.CubeGrid, out ButtonPanel))
            {
                return ButtonPanel;
            }

            if (TryFindButtonPanel(new Vector3I(Panel.Position.X + 1, Panel.Position.Y - 1, Panel.Position.Z + 1), Panel.CubeGrid, out ButtonPanel))
            {
                return ButtonPanel;
            }

            if (TryFindButtonPanel(new Vector3I(Panel.Position.X + 1, Panel.Position.Y + 1, Panel.Position.Z + 1), Panel.CubeGrid, out ButtonPanel))
            {
                return ButtonPanel;
            }

            return ButtonPanel;
        }

        public bool TryFindButtonPanel(Vector3I Position, IMyCubeGrid Grid, out IMyButtonPanel Match)
        {
            IMySlimBlock SlimBlock = Grid.GetCubeBlock(Position);
            if (SlimBlock != null && SlimBlock.FatBlock != null && SlimBlock.FatBlock is IMyTerminalBlock && SlimBlock.FatBlock is IMyButtonPanel)
            {
                Match = SlimBlock.FatBlock as IMyButtonPanel;
                return true;
            }
            else
            {
                Match = null;
                return false;
            }
        }
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}