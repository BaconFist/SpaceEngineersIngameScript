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

namespace Snippet_BaconArgs
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        Snippet_BaconArgs
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

        public class BaconArgs
        {
            public class Parser
            {
                static Dictionary<string, Bag> cache = new Dictionary<string, Bag>();
                public Bag parseArgs(string args)
                {
                    if (!cache.ContainsKey(args))
                    {
                        Bag Result = new Bag();
                        bool isEscape = false;
                        bool isEncapsulatedString = false;
                        StringBuilder slug = new StringBuilder();
                        for (int i = 0; i < args.Length; i++)
                        {
                            char glyp = args[i];
                            if (isEscape)
                            {
                                slug.Append(glyp);
                                isEscape = false;
                            }
                            else if (glyp.Equals('\\'))
                            {
                                isEscape = true;
                            }
                            else if (isEncapsulatedString && !glyp.Equals('"'))
                            {
                                slug.Append(glyp);
                            }
                            else if (glyp.Equals('"'))
                            {
                                isEncapsulatedString = !isEncapsulatedString;
                            }
                            else if (glyp.Equals(' '))
                            {
                                Result.add(slug.ToString());
                                slug.Clear();
                            }
                            else
                            {
                                slug.Append(glyp);
                            }
                        }
                        if (slug.Length > 0)
                        {
                            Result.add(slug.ToString());
                        }
                        cache.Add(args, Result);
                    }

                    return cache[args];
                }
            }

            public class Bag
            {
                protected Dictionary<char, int> Flags = new Dictionary<char, int>();
                protected List<string> Arguments = new List<string>();
                protected Dictionary<string, List<string>> Options = new Dictionary<string, List<string>>();

                public List<string> getArguments()
                {
                    return Arguments;
                }

                public int getFlag(char flag)
                {
                    return Flags.ContainsKey(flag) ? Flags[flag] : 0;
                }

                public List<string> getOption(string name)
                {
                    return Options.ContainsKey(name) ? Options[name] : new List<string>();
                }

                public void add(string arg)
                {
                    if (!arg.StartsWith("-"))
                    {
                        Arguments.Add(arg);
                    }
                    else if (arg.StartsWith("--"))
                    {
                        KeyValuePair<string, string> slug = getKeyValuePair(arg);
                        string key = slug.Key.Substring(2);
                        if (!Options.ContainsKey(key))
                        {
                            Options.Add(key, new List<string>());
                        }
                        Options[key].Add(slug.Value);
                    }
                    else
                    {
                        string slug = arg.Substring(1);
                        for (int i = 0; i < slug.Length; i++)
                        {
                            if (this.Flags.ContainsKey(slug[i]))
                            {
                                this.Flags[slug[i]]++;
                            }
                            else
                            {
                                this.Flags.Add(slug[i], 1);
                            }
                        }
                    }
                }

                private KeyValuePair<string, string> getKeyValuePair(string arg)
                {
                    string[] pair = arg.Split(new char[] { '=' }, 2);
                    return new KeyValuePair<string, string>(pair[0], (pair.Length > 1) ? pair[1] : null);
                }

                override public string ToString()
                {
                    List<string> opts = new List<string>();
                    foreach (string key in Options.Keys)
                    {
                        opts.Add(escape(key) + ":[" + string.Join(",", Options[key].ConvertAll<string>(x => escape(x)).ToArray()) + "]");
                    }
                    List<string> flags = new List<string>();
                    foreach (char key in Flags.Keys)
                    {
                        flags.Add(key + ":" + Flags[key].ToString());
                    }
                    StringBuilder slug = new StringBuilder();
                    slug.Append("{\"a\":[");
                    slug.Append(string.Join(",", Arguments.ConvertAll<string>(x => escape(x)).ToArray()));
                    slug.Append("],\"o\":[{");
                    slug.Append(string.Join("},{", opts));
                    slug.Append("}],\"f\":[{");
                    slug.Append(string.Join("},{", flags));
                    slug.Append("}]}");
                    return slug.ToString();
                }

                private string escape(string val)
                {
                    return (val != null) ? "\"" + val.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" : @"null";
                }
            }
        }

        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}