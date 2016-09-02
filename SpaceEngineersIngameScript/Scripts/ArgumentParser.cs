﻿using System;
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

namespace ArgumentParser
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        ArgumentParser
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
            // The main entry point of the script, invoked every time
            // one of the programmable block's Run actions are invoked.
            // 
            // The method itself is required, but the argument above
            // can be removed if not needed.
        }

        private class SimpleArgs
        {
            List<char> Flags = new List<char>();
            List<string> Arguments = new List<string>();
            Dictionary<string, string> Options = new Dictionary<string, string>();

            public SimpleArgs(string rawArgs)
            {
                System.Text.RegularExpressions.Regex RgxWhitespace = new System.Text.RegularExpressions.Regex(@"\s+");
                System.Text.RegularExpressions.Regex RgxString = new System.Text.RegularExpressions.Regex(@"^(?:^(?<!\\)"".+?(?<!\\)"")|(?:^[^\s-""]+)");
                System.Text.RegularExpressions.Regex RgxFlags = new System.Text.RegularExpressions.Regex(@"^-[^\s-]+");
                System.Text.RegularExpressions.Regex RgxOption = new System.Text.RegularExpressions.Regex(@"^--[^\s=]+(?=\s)");
                System.Text.RegularExpressions.Regex RgxOptVal = new System.Text.RegularExpressions.Regex(@"^--[^\s=]+=");

                string match = "";
                string lastOptVal = null;
                for(int i = 0; i < rawArgs.Length; i++)
                {
                    if (RgxWhitespace.IsMatch(rawArgs))
                    {
                        lastOptVal = null;
                        match = RgxWhitespace.Match(rawArgs).Value;
                    } else if (RgxString.IsMatch(rawArgs))
                    {
                        
                    }
                }
            }

            private void addArg(string raw)
            {

            }

            private void addFlags(string raw)
            {

            }

            private void addOption(string raw)
            {

            }

            private void addOptVal(string optRaw, string valRaw)
            {

            }

        }
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}