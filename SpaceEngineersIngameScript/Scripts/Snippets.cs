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

namespace Snippets
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        class DetailedInfo
        {
            private List<DetailedInfoValue> storage = new List<DetailedInfoValue>();

            public DetailedInfo(IMyTerminalBlock Block)
            {
                string[] Info = Block.DetailedInfo.Split(new string[] { "\r\n", "\n\r", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < Info.Length; i++)
                {
                    List<string> data = new List<string>();
                    data.AddRange(Info[i].Split(':'));
                    if (data.Count > 1)
                    {
                        storage.Add(new DetailedInfoValue(data[0], String.Join(":", data.GetRange(1, data.Count - 1))));
                    }
                }
            }

            public DetailedInfoValue getValue(int index)
            {
                return (index < storage.Count && index > -1)?storage[index]:null;
            }

            public class DetailedInfoValue
            {
                public string key;
                public string value;
                public DetailedInfoValue(string k, string v)
                {
                    key = k;
                    value = v;
                }
            }
        }
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}