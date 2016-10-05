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

namespace Snippet_BaconMessages
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        BaconScriptMessages
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

        class BaconMessages
        {
            const int FIELD_LIMIT = 4200;
            const int DEVICE_LIMIT = 8400;
            private IMyTextPanel StorageDevice;
            public List<Message> Messages { get; }
            
            public BaconMessages(IMyTextPanel StorageDevice)
            {
                this.StorageDevice = StorageDevice;
                string[] data = (this.StorageDevice.GetPublicText() + this.StorageDevice.GetPrivateText()).Split(new Char[] {'\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < data.Length; i++)
                {
                    BaconArgs R = BaconArgs.parse(data[0]);
                    if((R.getOption(Message.OPT_SENDER).Count > 0) && (R.getOption(Message.OPT_TIMECREATED).Count > 0) && (R.getOption(Message.OPT_TIMETOLIVE).Count > 0) && (R.getOption(Message.OPT_TYPE).Count > 0) && (R.getArguments().Count > 0)) 
                    {
                        int TimeToLive = 0;
                        double TimeCreated = 0;
                        string Content = R.getArguments()[0];
                        string Type = R.getOption(Message.OPT_TYPE)[0];
                        string Sender = R.getOption(Message.OPT_SENDER)[0];
                        if(int.TryParse(R.getOption(Message.OPT_TIMETOLIVE)[0], out TimeToLive) && double.TryParse(R.getOption(Message.OPT_TIMECREATED)[0], out TimeCreated))
                        {
                            Message msg = new Message(Content, Sender, Type, TimeToLive, TimeCreated);
                            Add(msg);
                        }
                    }
                }
            }

            public bool Remove(Message msg)
            {
                return Messages.Remove(msg);
            }

            public Message First(Func<Message, bool> collect)
            {
                for (int i = 0; i < Messages.Count; i++)
                {
                    if (collect(Messages[i]))
                    {
                        return Messages[i];
                    }
                }
                return null;
            }

            public List<Message> Find(Func<Message, bool> collect)
            {
                List<Message> Matches = new List<Message>();
                for(int i = 0; i < Messages.Count; i++)
                {
                    if (collect(Messages[i]))
                    {
                        Matches.Add(Messages[i]);
                    }
                }

                return Matches;
            }

            public bool Add(Message msg)
            {
                if (!Messages.Contains(msg) && !msg.isExpired())
                {
                    Messages.Add(msg);
                    return true;
                }
                return false;
            }

            private bool Save()
            {
                string dataPublicText = string.Join("\n", Messages);
                string dataPrivateText = "";
                if (dataPublicText.Length > DEVICE_LIMIT)
                {
                    return false;
                }
                if(dataPublicText.Length > FIELD_LIMIT)
                {
                    dataPrivateText = dataPublicText.Substring(FIELD_LIMIT);
                    dataPublicText = dataPublicText.Remove(FIELD_LIMIT);
                }
                StorageDevice.WritePublicText(dataPublicText);
                StorageDevice.WritePrivateText(dataPrivateText);
                return true;
            }

            public class Message
            {
                public const string TYPE_BROADCAST = "0";
                public const string OPT_TIMETOLIVE = "TTL";
                public const string OPT_TIMECREATED = "TC";
                public const string OPT_TYPE = "TYPE";
                public const string OPT_SENDER = "SENDER";


                public int TimeToLive { get; }
                public DateTime TimeCreated { get; }
                public string Content { get; }
                public string Type { get;  }
                public string Sender { get; }

                public Message(string Content, string Sender, string Type, int TimeToLive, double TimeCreated)
                {
                    this.TimeToLive = TimeToLive;
                    this.Type = Type;
                    this.Sender = Sender;
                    this.Content = Content;
                    this.TimeCreated = TimeStampToDateTime(TimeCreated);
                }

                public bool isExpired()
                {
                    return (TimeCreated.AddSeconds(TimeToLive) > DateTime.Now);
                }

                public override string ToString()
                {
                    List<string> slug = new List<string>();
                    slug.Add(@"--"+OPT_TIMETOLIVE+"="+BaconArgs.Escape(TimeToLive.ToString()));
                    slug.Add(@"--"+OPT_TIMECREATED+"=" + BaconArgs.Escape(DateTimeToTimestamp(TimeCreated).ToString()));
                    slug.Add(@"--"+OPT_TYPE+"=" + BaconArgs.Escape(Type));
                    slug.Add(@"--"+OPT_SENDER+"=" + BaconArgs.Escape(Sender));
                    slug.Add(@"--");
                    slug.Add(Content);
                    return string.Join(" ", slug);
                }
                private DateTime TimeStampToDateTime(double timestamp)
                {
                    return (new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).AddSeconds(timestamp).ToLocalTime();
                }

                private double DateTimeToTimestamp(DateTime DT)
                {
                    return (DT.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                }
            }
        }

        public class BaconArgs { static public BaconArgs parse(string a) { return (new Parser()).parseArgs(a); } static public string Escape(string a) { return a.Replace(@"\", @"\\").Replace(@" ", @"\ ").Replace(@"""", @"\"""); } static public string UnEscape(string a) { return a.Replace(@"\""", @"""").Replace(@"\ ", @" ").Replace(@"\\", @"\"); } public class Parser { static Dictionary<string, BaconArgs> h = new Dictionary<string, BaconArgs>(); public BaconArgs parseArgs(string a) { if (!h.ContainsKey(a)) { var b = new BaconArgs(); var c = false; var d = false; var e = new StringBuilder(); for (int f = 0; f < a.Length; f++) { var g = a[f]; if (c) { e.Append(g); c = false; } else if (g.Equals('\\')) c = true; else if (d && !g.Equals('"')) e.Append(g); else if (g.Equals('"')) d = !d; else if (g.Equals(' ')) if (e.ToString().Equals("--")) { b.add(a.Substring(f).TrimStart()); e.Clear(); break; } else { b.add(e.ToString()); e.Clear(); } else e.Append(g); } if (e.Length > 0) b.add(e.ToString()); h.Add(a, b); } return h[a]; } } protected Dictionary<char, int> h = new Dictionary<char, int>(); protected List<string> i = new List<string>(); protected Dictionary<string, List<string>> j = new Dictionary<string, List<string>>(); public List<string> getArguments() { return i; } public int getFlag(char a) { return h.ContainsKey(a) ? h[a] : 0; } public List<string> getOption(string a) { return j.ContainsKey(a) ? j[a] : new List<string>(); } public void add(string a) { if (!a.StartsWith("-")) i.Add(a); else if (a.StartsWith("--")) { KeyValuePair<string, string> b = k(a); var c = b.Key.Substring(2); if (!j.ContainsKey(c)) j.Add(c, new List<string>()); j[c].Add(b.Value); } else { var b = a.Substring(1); for (int d = 0; d < b.Length; d++) if (this.h.ContainsKey(b[d])) { this.h[b[d]]++; } else { this.h.Add(b[d], 1); } } } KeyValuePair<string, string> k(string a) { string[] b = a.Split(new char[] { '=' }, 2); return new KeyValuePair<string, string>(b[0], (b.Length > 1) ? b[1] : null); } string l(string a) { return (a != null) ? "\"" + a.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" : @"null"; } }
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}