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

namespace DoorInterlock
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        DoorInterlock
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

        System.Text.RegularExpressions.Regex TAG = new System.Text.RegularExpressions.Regex(@"\[xor:([^\]]+)\]", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        public void Main(string argument)
        {
            foreach(KeyValuePair<string, List<IMyDoor>> Group in getInterlockGroups())
            {
                IMyDoor MaxOpenDoor = MaxOpenRatio(Group.Value);
                foreach(IMyDoor Door in Group.Value)
                {
                    if (!Door.Equals(MaxOpenDoor))
                    {
                        Door.ApplyAction("Open_Off");
                    }
                }
            }                
        }

        

        GroupDict getInterlockGroups()
        {
            GroupDict Groups = new GroupDict();
            List<IMyDoor> Doors = new List<IMyDoor>();
            GridTerminalSystem.GetBlocksOfType<IMyDoor>(Doors, (d => d.CubeGrid.Equals(Me.CubeGrid) && TAG.IsMatch(d.CustomName)));
            foreach(IMyDoor Door in Doors)
            {
                string groupName = TAG.Match(Door.CustomName)?.Groups[1]?.Value;
                if(groupName != null)
                {
                    Groups.Add(groupName, Door);
                }
            }
            return Groups;
        }

        class GroupDict : Dictionary<string, List<IMyDoor>>
        {
            public void Add(string key, IMyDoor Door)
            {
                if (!ContainsKey(key))
                {
                    Add(key, new List<IMyDoor>());
                }
                this[key].Add(Door);
            }
        }

        public IMyDoor MaxOpenRatio(List<IMyDoor> Doors)
        {
            IMyDoor Match = null;
            for(int i = 0; i < Doors.Count; i++)
            {
                if(Match == null || Doors[i].OpenRatio > Match.OpenRatio)
                {
                    Match = Doors[i];
                }
            }
            return Match;
        }
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}