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


        /*
            example implementation for BaconArgs:
            run the PB with 
                --source="TextPanel 1" --destination="TextPanel 2"
            to copy PublicText from TextPanel 1 to TextPanel 2
        */
        public void Main(string argument)
        {
            // parse PB arguments with Baconargs
            BaconArgs Args = BaconArgs.parse(argument);

            // validate if there is one --source and one --destination option given
            // as you can see the options are accessed without the leading --
            if (Args.getOption("source").Count == 1 && Args.getOption("destination").Count == 1)
            {
                // get the TextPanels using --source and --destination options
                // as options saved as a List<string> they are accessed like getOption("the option name")[zero-based-index]
                IMyTextPanel SourcePanel = GridTerminalSystem.GetBlockWithName(Args.getOption("source")[0]) as IMyTextPanel;
                IMyTextPanel DestinationPanel = GridTerminalSystem.GetBlockWithName(Args.getOption("destination")[0]) as IMyTextPanel;

                //copy private text from source to destination if both block could be found
                if (SourcePanel != null && DestinationPanel != null)
                {
                    DestinationPanel.WritePublicText(SourcePanel.GetPublicText());
                }
            }
        }

        #region BaconArgs
        public class BaconArgs
        {
            /*
                --option=value => option with values => always start with `--`
                --option => option without value (will be saved with a `null` value)
                -a => Flag => starts with `-`
                arguments => anything not starting with a `-`
                -- => terminate argument parsing, all after this will be considered as one argument
                    
                Example:
                BaconArgs Args = BaconArgs.parse(@"argument\ 1 --opt1=val1 --opt1=val2 --opt1 --opt2 --opt2=""value 2"" -flags -f -s ""argument 2"" -- all the stuff after -- will be parsed as one argument");
                //acces flags:
                    Args.getFlag('f'); // (int)2
                    Args.getFlag('l'); // (int)1
                    Args.getFlag('a'); // (int)1
                    Args.getFlag('g'); // (int)1
                    Args.getFlag('s'); // (int)2
                    Args.getFlag('m'); // (int)0
                //access arguments:
                    Args.getArguments(); // (List<string>)["argument 1", "argument 2", "all the stuff after -- will be parsed as one argument"]
                //acces options:
                    Args.getOption("opt1"); // (List<string>)["val1", null]
                    Args.getOption("opt2"); // (List<string>)[null, "value 2"]
                //dump all the stuff
                    Args.ToString(); // outputs a valid json object
                //check if option is set
                    (Args.getOption("opt1").Count > 0); // true if there is any --opt1
                    (Args.getFlag('g') > 0); // true if tehere is at least one 'g' flag
                    (Args.getArguments().Count > 0); // true if ther is any argument
                //some explonation:
                    getFlag(char flag) shows how often a flag is found
                    getOption(string option) gives a list of all the values assigned to this option (will be an empty list if the option is not set
                    getArguments() gives a list with all the arguments which are not options and not flags
            */

            static public BaconArgs parse(string args)
            {
                return (new Parser()).parseArgs(args);
            }

            static public string Escape(string value)
            {
                return value.Replace(@"\", @"\\").Replace(@" ", @"\ ").Replace(@"""",@"\""");
            }

            static public string UnEscape(string value)
            {
                return value.Replace(@"\""", @"""").Replace(@"\ ", @" ").Replace(@"\\", @"\");
            }

            public class Parser
            {
                static Dictionary<string, BaconArgs> cache = new Dictionary<string, BaconArgs>();
                public BaconArgs parseArgs(string args)
                {
                    if (!cache.ContainsKey(args))
                    {
                        BaconArgs Result = new BaconArgs();
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
                                if (slug.ToString().Equals("--"))
                                {
                                    Result.add(args.Substring(i).TrimStart());
                                    slug.Clear();
                                    break;
                                }
                                else {
                                    Result.add(slug.ToString());
                                    slug.Clear();
                                }
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
        #endregion BaconArgs
        class minified
        {
            #region BaconArgs
            public class BaconArgs { static public BaconArgs parse(string a) { return (new Parser()).parseArgs(a); } static public string Escape(string a) { return a.Replace(@"\", @"\\").Replace(@" ", @"\ ").Replace(@"""", @"\"""); } static public string UnEscape(string a) { return a.Replace(@"\""", @"""").Replace(@"\ ", @" ").Replace(@"\\", @"\"); } public class Parser { static Dictionary<string, BaconArgs> h = new Dictionary<string, BaconArgs>(); public BaconArgs parseArgs(string a) { if (!h.ContainsKey(a)) { var b = new BaconArgs(); var c = false; var d = false; var e = new StringBuilder(); for (int f = 0; f < a.Length; f++) { var g = a[f]; if (c) { e.Append(g); c = false; } else if (g.Equals('\\')) c = true; else if (d && !g.Equals('"')) e.Append(g); else if (g.Equals('"')) d = !d; else if (g.Equals(' ')) if (e.ToString().Equals("--")) { b.add(a.Substring(f).TrimStart()); e.Clear(); break; } else { b.add(e.ToString()); e.Clear(); } else e.Append(g); } if (e.Length > 0) b.add(e.ToString()); h.Add(a, b); } return h[a]; } } protected Dictionary<char, int> h = new Dictionary<char, int>(); protected List<string> i = new List<string>(); protected Dictionary<string, List<string>> j = new Dictionary<string, List<string>>(); public List<string> getArguments() { return i; } public int getFlag(char a) { return h.ContainsKey(a) ? h[a] : 0; } public List<string> getOption(string a) { return j.ContainsKey(a) ? j[a] : new List<string>(); } public void add(string a) { if (!a.StartsWith("-")) i.Add(a); else if (a.StartsWith("--")) { KeyValuePair<string, string> b = k(a); var c = b.Key.Substring(2); if (!j.ContainsKey(c)) j.Add(c, new List<string>()); j[c].Add(b.Value); } else { var b = a.Substring(1); for (int d = 0; d < b.Length; d++) if (this.h.ContainsKey(b[d])) { this.h[b[d]]++; } else { this.h.Add(b[d], 1); } } } KeyValuePair<string, string> k(string a) { string[] b = a.Split(new char[] { '=' }, 2); return new KeyValuePair<string, string>(b[0], (b.Length > 1) ? b[1] : null); } string l(string a) { return (a != null) ? "\"" + a.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" : @"null"; } }
            #endregion BaconArgs
        }
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}