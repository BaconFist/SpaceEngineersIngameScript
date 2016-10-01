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

namespace MassBlockRenamer
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        MassBlockRenamer
        ==============
        Copyright 2016 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE
           
           Summary 
           ------------------------------ 
           Change names of many Blocks at once 
             
           Abstract 
           ------------------------------ 
                simple: "oldText;newText" 
                limited to Group: "Groupname;oldText;newText" 
                Globs in oldText: 
                        * -> matches any number of any characters including none 
                        ? -> matches any single character 
                        [abc] -> matches one character given in the bracket 
                        [a-z] -> matches one character from the range given in the bracket 
                Pipe oldText to newText with "|": (good for adding text to stuff) 
                        Inject Text: "Light*;Shiny |" will replace "Light 1", "Light 22" or any matching "Light*" with "Shiny Light 1", "Shiny Light 22" and so on. 
                        Append Text: "*;| MyShip" 
                        Prepend Text: "*;My Ship |" 
                        ! Caution !: something like "a;|derp" could end in a mess like "Smaderpll Readerpctor 11"  
                  
                Script is Limited to the CubeGrid of the PB to prevent unwanted changes on docked ships. 
            
           Example 
           ------------------------------ 
                Blocks: "Interior Light 1", "Interior Light 2", "Interior Light 3", "Small Reactor 11" 
                 
                Replace: "Small Reactor;Power Generator" => "Interior Light 1", "Interior Light 2", "Interior Light 3", "Power Generator 11" 
                Append: "Light*;| CargonRoom 1" => "Interior Light 1 CargonRoom 1", "Interior Light 2 CargonRoom 1", "Interior Light 3 CargonRoom 1", "Small Reactor 11" 
                Prepend: "*;MyShip |" => "MyShip Interior Light 1", "MyShip Interior Light 2", "MyShip Interior Light 3", "MyShip Small Reactor 11" 
                Remove: "Interior;" => "Light 1", "Light 2", "Light 3", "Small Reactor 11" 
         */


        const string MARKER_MATCH = "|";
        const string MARKER_NUMBER = "#";

        public void Main(string args)
        {
            Argument Arg = getArgument(args);
            if (Arg != null)
            {
                Glob Filter = new Glob(Arg.glob);
                Echo(Arg.glob + " => " + Filter.Rgx.ToString());
                List<IMyTerminalBlock> Group = getBlockGroup(Arg);
                List<IMyTerminalBlock> Blocks = findBlocksByGlob(Group, Filter);
                replaceNamesInBlocklist(Blocks, Filter, Arg);
            }
        }

        public void replaceNamesInBlocklist(List<IMyTerminalBlock> Blocks, Glob Filter, Argument Arg)
        {
            Dictionary<string, int> BlockCounter = new Dictionary<string, int>();
            for (int i = 0; i < Blocks.Count; i++)
            {
                string typeIdString = Blocks[i].BlockDefinition.TypeIdString;
                if (!BlockCounter.ContainsKey(typeIdString))
                {
                    BlockCounter.Add(typeIdString, 0);
                }
                BlockCounter[typeIdString] = BlockCounter[typeIdString] + 1;
                replaceBlockname(Blocks[i], Filter, Arg, BlockCounter[typeIdString]);
            }
        }

        public void replaceBlockname(IMyTerminalBlock Block, Glob Filter, Argument Arg, int blockNumber)
        {
            StringBuilder slug = new StringBuilder(Block.CustomName);
            string[] matches = Filter.getMatches(Block.CustomName);
            for (int i = 0; i < matches.Length; i++)
            {
                if (matches[i].Length > 0)
                {
                    Echo(Block.CustomName + " => \"" + matches[i] + "\"");
                    slug = slug.Replace(matches[i], Arg.replacement.Replace(MARKER_MATCH, matches[i]));
                    slug = slug.Replace(MARKER_NUMBER, blockNumber.ToString());
                }
            }
            Block.SetCustomName(slug.ToString());
        }

        public Argument getArgument(string args)
        {
            Argument Arg = new Argument();
            string[] argv = args.Split(';');
            if (argv.Length == 2)
            {
                Arg.glob = argv[0];
                Arg.replacement = argv[1];
            }
            else if (argv.Length == 3)
            {
                Arg.group = argv[0];
                Arg.glob = argv[1];
                Arg.replacement = argv[2];
            }
            else
            {
                return null;
            }

            return Arg;
        }

        public List<IMyTerminalBlock> getBlockGroup(Argument Arg)
        {
            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
            if (Arg.group != null)
            {
                IMyBlockGroup Group = GridTerminalSystem.GetBlockGroupWithName(Arg.group);
                if (Group != null)
                {
                    Group.GetBlocks(Blocks);
                }
            }
            else
            {
                GridTerminalSystem.GetBlocks(Blocks);
            }

            return Blocks;
        }

        public List<IMyTerminalBlock> findBlocksByGlob(List<IMyTerminalBlock> BlockGroup, Glob Filter)
        {
            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
            for (int i = 0; i < BlockGroup.Count; i++)
            {
                if (Filter.isMatch(BlockGroup[i].CustomName) && BlockGroup[i].CubeGrid.Equals(Me.CubeGrid))
                {
                    Blocks.Add(BlockGroup[i]);
                }
            };

            return Blocks;
        }

        public class Argument
        {
            public string group = null;
            public string glob;
            public string replacement;
        }

        public class Glob
        {
            public System.Text.RegularExpressions.Regex Rgx;
            string pattern;

            public Glob(string pattern)
            {
                this.pattern = pattern;
                this.Rgx = getRegexFromGlob(pattern);
            }

            private System.Text.RegularExpressions.Regex getRegexFromGlob(string glob)
            {
                pattern = System.Text.RegularExpressions.Regex.Escape(glob)
                    .Replace(@"\*", @".*")
                    .Replace(@"\?", @".")
                    .Replace(@"\\\[([^\]]+)\]", @"[$1]")
                    .Replace(@"\ ", @" ");

                return new System.Text.RegularExpressions.Regex(pattern);
            }

            public bool isMatch(string input)
            {
                return Rgx.IsMatch(input);
            }

            public string[] getMatches(string input)
            {
                System.Text.RegularExpressions.MatchCollection RgxMatches = Rgx.Matches(input);
                List<string> Matches = new List<string>();
                for (int i = 0; i < RgxMatches.Count; i++)
                {
                    Matches.Add(RgxMatches[i].Value);
                }

                return Matches.ToArray();
            }
        }

        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}