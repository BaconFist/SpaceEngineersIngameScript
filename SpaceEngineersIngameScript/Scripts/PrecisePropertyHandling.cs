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

namespace PrecisePropertyHandling
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        PrecisePropertyHandling
        ==============
        Copyright 2017 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE

        Description
        ===========

        */

        private System.Text.RegularExpressions.Regex commandPattern = new System.Text.RegularExpressions.Regex(@"^\{(?<tag>[\s\S]+)\}->(?<property>[^(]+)\((?<value>[^)]+)\)$");
        private Dictionary<string, List<IMyTerminalBlock>> findBlocksByTagCache = new Dictionary<string, List<IMyTerminalBlock>>();
                        
        public void Main(string argument)
        {
            foreach (string arg in argument.Split(';'))
            {
                Echo(string.Format(@"Progress Argument: {0}", arg));
                Command command = getCommand(arg);
                if(command != null)
                {
                    List<IMyTerminalBlock> affectedBlocks = findBlocksByTag(command.Tag);
                    foreach(IMyTerminalBlock block in affectedBlocks)
                    {
                        ITerminalProperty property = block.GetProperty(command.Property);
                        
                        if(property != null)
                        {
                            Echo(string.Format(@"Property ""{0}"" found for ""{1}""", command.Property, block.CustomName));
                            Echo("propets type = " + property.TypeName);
                            switch (property.TypeName.ToLowerInvariant())
                            {
                                case "bool":
                                    if (command.isBool)
                                    {
                                        block.SetValueBool(property.Id, command.ValueBool);
                                    } else if (command.Value.Equals("toggle"))
                                    {
                                        block.SetValueBool(property.Id, !block.GetValueBool(property.Id));
                                        Echo(string.Format(@"set to: {0}", block.GetValueBool(property.Id)));
                                    }                                    
                                    break;
                                case "float":
                                case "single":
                                    if (command.Value.StartsWith("inc "))
                                    {
                                        float _buffer = 0;
                                        if(float.TryParse(command.Value.Substring(3), out _buffer))
                                        {
                                            block.SetValueFloat(property.Id, block.GetValueFloat(property.Id) + _buffer);
                                            Echo(string.Format(@"set to: {0}", block.GetValueFloat(property.Id)));
                                        }

                                    } else if (command.Value.StartsWith("dec "))
                                    {
                                        float _buffer = 0;
                                        if (float.TryParse(command.Value.Substring(3), out _buffer))
                                        {
                                            block.SetValueFloat(property.Id, block.GetValueFloat(property.Id) - _buffer);
                                            Echo(string.Format(@"set to: {0}", block.GetValueFloat(property.Id)));
                                        }
                                    } else if (command.isFloat)
                                    {
                                        block.SetValueFloat(property.Id, command.ValueFloat);
                                        Echo(string.Format(@"set to: {0}", block.GetValueFloat(property.Id)));
                                    }
                                    break;
                            }
                        } else
                        {
                            Echo(string.Format(@"Property ""{0}"" not found for ""{0}""", command.Property, block.CustomName));
                        }
                    }
                }
            }
        }        

        private List<IMyTerminalBlock> findBlocksByTag(string tag)
        {
            if (!findBlocksByTagCache.ContainsKey(tag))
            {
                List<IMyTerminalBlock> matches = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(matches, (b => b.CustomName.Contains(tag) ));

                List<IMyBlockGroup> groupMatches = new List<IMyBlockGroup>();
                GridTerminalSystem.GetBlockGroups(groupMatches, (g => g.Name.Contains(tag)));
                foreach(IMyBlockGroup group in groupMatches)
                {
                    List<IMyTerminalBlock> buffer = new List<IMyTerminalBlock>();
                    group.GetBlocksOfType<IMyTerminalBlock>(buffer, (b => !matches.Contains(b) ));
                    matches.AddRange(buffer);
                    buffer.Clear();
                }

                findBlocksByTagCache.Add(tag, matches);
            }

            return findBlocksByTagCache[tag];
        }

        private Command getCommand(string argument)
        {
            if (commandPattern.IsMatch(argument))
            {
                var match = commandPattern.Match(argument);
                Echo("match");
                Command buffer = new Command(match.Groups["tag"].Value, match.Groups["property"].Value, match.Groups["value"].Value);
                Echo(string.Format(@"Command: <block>{0} -> <propert>{1} (<value> {2} | <bool> {3} | <float> {4})", buffer.Tag, buffer.Property, buffer.Value, buffer.isBool?buffer.ValueBool.ToString():"N/A", buffer.isFloat?buffer.ValueFloat.ToString():"N/A"));
                return buffer;
            } else
            {
                Echo("no match");
            }

            return null;
        }

        private class Command
        {
            public string Tag;
            public string Property;
            public string Value;
            public float ValueFloat;
            public bool ValueBool;

            public bool isFloat = false;
            public bool isBool = false;

            public Command(string Tag, string Property, string Value)
            {
                this.Tag = Tag;
                this.Property = Property;
                this.Value = Value;

                this.isFloat = float.TryParse(Value, out this.ValueFloat);
                this.isBool = bool.TryParse(Value, out this.ValueBool);
            }



        }


        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}