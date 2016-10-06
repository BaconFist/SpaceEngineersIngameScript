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

        public class BaconMessages
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
            class BaconArgs { static public BaconArgs parse(string a) { return (new Parser()).parseArgs(a); } static public string Escape(string a) { return a.Replace(@"\", @"\\").Replace(@" ", @"\ ").Replace(@"""", @"\"""); } static public string UnEscape(string a) { return a.Replace(@"\""", @"""").Replace(@"\ ", @" ").Replace(@"\\", @"\"); } public class Parser { static Dictionary<string, BaconArgs> h = new Dictionary<string, BaconArgs>(); public BaconArgs parseArgs(string a) { if (!h.ContainsKey(a)) { var b = new BaconArgs(); var c = false; var d = false; e = new StringBuilder(); for (int f = 0; f < a.Length; f++) { var g = a[f]; if (c) { e.Append(g); c = false; } else if (g.Equals('\\')) c = true; else if (d && !g.Equals('"')) e.Append(g); else if (g.Equals('"')) d = !d; else if (g.Equals(' ')) if (e.ToString().Equals("--")) { b.add(a.Substring(f).TrimStart()); e.Clear(); break; } else { b.add(e.ToString()); e.Clear(); } else e.Append(g); } if (e.Length > 0) b.add(e.ToString()); h.Add(a, b); } return h[a]; } } protected Dictionary<char, int> h = new Dictionary<char, int>(); protected List<string> i = new List<string>(); protected Dictionary<string, List<string>> j = new Dictionary<string, List<string>>(); public List<string> getArguments() { return i; } public int getFlag(char a) { return h.ContainsKey(a) ? h[a] : 0; } public List<string> getOption(string a) { return j.ContainsKey(a) ? j[a] : new List<string>(); } public void add(string a) { if (!a.StartsWith("-")) i.Add(a); else if (a.StartsWith("--")) { KeyValuePair<string, string> b = l(a); c = b.Key.Substring(2); if (!j.ContainsKey(c)) j.Add(c, new List<string>()); j[c].Add(b.Value); } else { var b = a.Substring(1); for (int d = 0; d < b.Length; d++) if (this.h.ContainsKey(b[d])) this.h[b[d]]++; else this.h.Add(b[d], 1); } } KeyValuePair<string, string> l(string a) { string[] b = a.Split(new char[] { '=' }, 2); return new KeyValuePair<string, string>(b[0], (b.Length > 1) ? b[1] : null); } string p(string a) { return (a != null) ? "\"" + a.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" : @"null"; } }
        }

        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
        class min
        {
            public class BaconMessages { IMyTextPanel j; public List<Message> Messages { get; } public BaconMessages(IMyTextPanel a) { this.j = a; string[] b = (this.j.GetPublicText() + this.j.GetPrivateText()).Split(new Char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries); for (int d = 0; d < b.Length; d++) { l f = l.parse(b[0]); if ((f.getOption(Message.OPT_SENDER).Count > 0) && (f.getOption(Message.OPT_TIMECREATED).Count > 0) && (f.getOption(Message.OPT_TIMETOLIVE).Count > 0) && (f.getOption(Message.OPT_TYPE).Count > 0) && (f.getArguments().Count > 0)) { int g = 0; double k = 0; var m = f.getArguments()[0]; var n = f.getOption(Message.OPT_TYPE)[0]; var o = f.getOption(Message.OPT_SENDER)[0]; if (int.TryParse(f.getOption(Message.OPT_TIMETOLIVE)[0], out g) && double.TryParse(f.getOption(Message.OPT_TIMECREATED)[0], out k)) { var q = new Message(m, o, n, g, k); Add(q); } } } } public bool Remove(Message a) { return Messages.Remove(a); } public Message First(Func<Message, bool> a) { for (int b = 0; b < Messages.Count; b++) if (a(Messages[b])) { return Messages[b]; } return null; } public List<Message> Find(Func<Message, bool> a) { var b = new List<Message>(); for (int d = 0; d < Messages.Count; d++) if (a(Messages[d])) { b.Add(Messages[d]); } return b; } public bool Add(Message a) { if (!Messages.Contains(a) && !a.isExpired()) { Messages.Add(a); return true; } return false; } bool p() { var a = string.Join("\n", Messages); var b = ""; if (a.Length > 8400) return false; if (a.Length > 4200) { b = a.Substring(4200); a = a.Remove(4200); } j.WritePublicText(a); j.WritePrivateText(b); return true; } public class Message { public const string TYPE_BROADCAST = "0"; public const string OPT_TIMETOLIVE = "TTL"; public const string OPT_TIMECREATED = "TC"; public const string OPT_TYPE = "TYPE"; public const string OPT_SENDER = "SENDER"; public int TimeToLive { get; } public DateTime TimeCreated { get; } public string Content { get; } public string Type { get; } public string Sender { get; } public Message(string a, string b, string d, int f, double g) { this.TimeToLive = f; this.Type = d; this.Sender = b; this.Content = a; this.TimeCreated = h(g); } public bool isExpired() { return (TimeCreated.AddSeconds(TimeToLive) > DateTime.Now); } public override string ToString() { var a = new List<string>(); a.Add(@"--" + OPT_TIMETOLIVE + "=" + l.Escape(TimeToLive.ToString())); a.Add(@"--" + OPT_TIMECREATED + "=" + l.Escape(i(TimeCreated).ToString())); a.Add(@"--" + OPT_TYPE + "=" + l.Escape(Type)); a.Add(@"--" + OPT_SENDER + "=" + l.Escape(Sender)); a.Add(@"--"); a.Add(Content); return string.Join(" ", a); } DateTime h(double a) { return (new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).AddSeconds(a).ToLocalTime(); } double i(DateTime a) { return (a.Subtract(new DateTime(1970, 1, 1))).TotalSeconds; } } class l { static public l parse(string a) { return (new Parser()).parseArgs(a); } static public string Escape(string a) { return a.Replace(@"\", @"\\").Replace(@" ", @"\ ").Replace(@"""", @"\"""); } static public string UnEscape(string a) { return a.Replace(@"\""", @"""").Replace(@"\ ", @" ").Replace(@"\\", @"\"); } public class Parser { static Dictionary<string, l> h = new Dictionary<string, l>(); public l parseArgs(string a) { if (!h.ContainsKey(a)) { l b = new l(); var d = false; var f = false; e = new StringBuilder(); for (int g = 0; g < a.Length; g++) { var k = a[g]; if (d) { e.Append(k); d = false; } else if (k.Equals('\\')) d = true; else if (f && !k.Equals('"')) e.Append(k); else if (k.Equals('"')) f = !f; else if (k.Equals(' ')) if (e.ToString().Equals("--")) { b.add(a.Substring(g).TrimStart()); e.Clear(); break; } else { b.add(e.ToString()); e.Clear(); } else e.Append(k); } if (e.Length > 0) b.add(e.ToString()); h.Add(a, b); } return h[a]; } } protected Dictionary<char, int> h = new Dictionary<char, int>(); protected List<string> i = new List<string>(); protected Dictionary<string, List<string>> j = new Dictionary<string, List<string>>(); public List<string> getArguments() { return i; } public int getFlag(char a) { return h.ContainsKey(a) ? h[a] : 0; } public List<string> getOption(string a) { return j.ContainsKey(a) ? j[a] : new List<string>(); } public void add(string a) { if (!a.StartsWith("-")) i.Add(a); else if (a.StartsWith("--")) { KeyValuePair<string, string> b = p(a); c = b.Key.Substring(2); if (!j.ContainsKey(c)) j.Add(c, new List<string>()); j[c].Add(b.Value); } else { var b = a.Substring(1); for (int d = 0; d < b.Length; d++) if (this.h.ContainsKey(b[d])) this.h[b[d]]++; else this.h.Add(b[d], 1); } } KeyValuePair<string, string> p(string a) { string[] b = a.Split(new char[] { '=' }, 2); return new KeyValuePair<string, string>(b[0], (b.Length > 1) ? b[1] : null); } string r(string a) { return (a != null) ? "\"" + a.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" : @"null"; } } }
        }
    }
}