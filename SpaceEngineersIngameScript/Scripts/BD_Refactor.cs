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

namespace BD_Refactor
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        /**
        BD_Refactor
        ==============
        Copyright 2016 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE

        Description
        ===========

        */
        BMyEnvironment Environment;

        public void Main(string argument)
        {
            Echo("- START -");
            try
            {
                Environment = bootstrap(BaconArgs.parse(argument));
                run(Environment);
            }
            catch (Exception e)
            {
                string fatalError = string.Format("Message: {0}\nSender:{1}\nStackTrace{2}", e.Message, e.Source, e.StackTrace);
                foreach(var data in e.Data)
                {
                    fatalError += "\n"+data.ToString();
                }
                Environment?.Log?.Fatal(fatalError);
                Echo(fatalError);
            }
            Echo("- END -");
        }

   
        void run(BMyEnvironment Environment)
        {
            Environment.Log?.newScope("run");
            BMyPanelTaskBag Bag = Environment.PanelTaskBagFactory.FromArguments(Environment.GlobalArgs, "[BaconDraw]", "[BaconDrawIgnoreGrid]");
            foreach(KeyValuePair<long,BMyPanelTask> Entry in Bag)
            {
                Environment.Log?.Trace("current Task: {0}:\"{1}\"", Entry.Key, Entry.Value.Panel.CustomName);
                BMyPanelTask Task = Entry.Value;
                if (!Task.hasChanged())
                {
                    Environment.Log?.Trace("panel not changed => skip");
                    continue;
                }
                Environment.Log?.Trace("panel has changed");
                BMyCanvas canvas = Environment.CanvasFactory.FromTask(Task);
                for (; !Task.isFinished() && canContinue(); Task.nextLineToProgress++)
                {
                    
                    BaconArgs InstructionArgs = BaconArgs.parse(
                        (0 <= Task.nextLineToProgress && Task.nextLineToProgress < Task.Count)
                        ?Task[Task.nextLineToProgress]
                        :""
                    );
                    if (InstructionArgs.getArguments().Count == 0)
                    {
                        continue;
                    }
                    string command = InstructionArgs.getArguments()[0];
                    InstructionArgs.getArguments().RemoveAt(0);
                    Environment.DrawPlugins.TryRun(command, InstructionArgs, canvas);   
                }
                if (Task.isFinished())
                {
                    Environment.Log?.Trace("Task finished: End of Code");
                } else
                {
                    Environment.Log?.Trace("Task delayed: instruction limit");
                }
                Task.Save(canvas);
                Environment.Log?.Trace("END Task: {0}:\"{1}\"", Entry.Key, Entry.Value.Panel.CustomName);
            }
            Environment.Log?.leaveScope();
        }

        public bool canContinue()
        {
            return Runtime.CurrentInstructionCount < 10000;
        }

        #region bootstrap
        BMyEnvironment bootstrap(BaconArgs Args)
        {
            BaconDebug Debugger = getDebugger(Args);
            Debugger.newScope("bootstrap");
            BMyEnvironment Env = new BMyEnvironment(this, Args, Debugger);
            addDefaultFont(Env);
            #region enable plugins
            foreach(string pluginame in Args.getOption("plugin"))
            {
                Env.DrawPlugins.TryEnable(pluginame);
            }
            // default plugins
            Env.DrawPlugins.TryEnable("BaconDraw");
            #endregion enbale plugins
            Env.Log?.leaveScope();
            return Env;
        }

        void addDefaultFont(BMyEnvironment E)
        {
            BMyFont font = new BMyFont("-", "", 5, 5, E);
            font.Add('\u0021', new string[] { "  1  ", "  1  ", "  1  ", "     ", "  1  " }); //'!'
            font.Add('\u0022', new string[] { " 1 1 ", " 1 1 ", "     ", "     ", "     " }); //'"'
            font.Add('\u0023', new string[] { " 1 1 ", "11111", " 1 1 ", "11111", " 1 1 " }); //'#'
            font.Add('\u0024', new string[] { " 1111", "1 1  ", " 111 ", "  1 1", "1111 " }); //'$'
            font.Add('\u0025', new string[] { "11  1", "11 1 ", "  1  ", " 1 11", "1  11" }); //'%'
            font.Add('\u0027', new string[] { "  1  ", "  1  ", "     ", "     ", "     " }); //'''
            font.Add('\u0028', new string[] { "   1 ", "  1  ", "     ", "  1  ", "   1 " }); //'('
            font.Add('\u0029', new string[] { " 1   ", "  1  ", "  1  ", "  1  ", " 1   " }); //')'
            font.Add('\u002A', new string[] { " 1 1 ", "  1  ", " 1 1 ", "     ", "     " }); //'*'
            font.Add('\u002C', new string[] { "     ", "     ", "     ", "  1  ", " 1   " }); //','
            font.Add('\u002D', new string[] { "     ", "     ", " 111 ", "     ", "     " }); //'-'
            font.Add('\u002E', new string[] { "     ", "     ", "     ", "     ", "  1  " }); //'.'
            font.Add('\u002F', new string[] { "    1", "   1 ", "  1  ", " 1   ", "1    " }); //'/'
            font.Add('\u0030', new string[] { " 111 ", "1  11", "1 1 1", "11  1", " 111 " }); //'0'
            font.Add('\u0031', new string[] { "  11 ", "   1 ", "   1 ", "   1 ", "  111" }); //'1'
            font.Add('\u0032', new string[] { "1111 ", "    1", " 111 ", "1    ", "11111" }); //'2'
            font.Add('\u0033', new string[] { "1111 ", "    1", "  11 ", "    1", "1111 " }); //'3'
            font.Add('\u0034', new string[] { "  11 ", " 1 1 ", "1  1 ", "11111", "   1 " }); //'4'
            font.Add('\u0035', new string[] { "111 1", "1    ", "111  ", "   11", "1111 " }); //'5'
            font.Add('\u0036', new string[] { " 1111", "1   1", "1111 ", "1    ", " 111 " }); //'6'
            font.Add('\u0037', new string[] { "11111", "    1", "   1 ", "  1  ", " 1   " }); //'7'
            font.Add('\u0038', new string[] { " 111 ", "1   1", " 111 ", "1   1", " 111 " }); //'8'
            font.Add('\u0039', new string[] { " 111 ", "1   1", " 1111", "    1", "1111 " }); //'9'
            font.Add('\u003A', new string[] { "     ", "  1  ", "     ", "  1  ", "     " }); //':'
            font.Add('\u003B', new string[] { "     ", "  1  ", "     ", "  1  ", " 1   " }); //';'
            font.Add('\u003C', new string[] { "    1", "   1 ", "  1  ", "   1 ", "    1" }); //'<'
            font.Add('\u003D', new string[] { "     ", " 111 ", "     ", " 111 ", "     " }); //'='
            font.Add('\u003E', new string[] { "1    ", " 1   ", "  1  ", " 1   ", "1    " }); //'>'
            font.Add('\u003F', new string[] { " 111 ", "    1", "  11 ", "     ", "  1  " }); //'?'
            font.Add('\u0040', new string[] { " 1111", "1   1", "1 111", "1 11 ", " 1111" }); //'@'
            font.Add('\u0041', new string[] { "  1  ", " 1 1 ", "1   1", "11111", "1   1" }); //'A'
            font.Add('\u0061', new string[] { "  1  ", " 1 1 ", "1   1", "11111", "1   1" }); //'a'
            font.Add('\u0042', new string[] { "1111 ", "1   1", "1111 ", "1   1", "1111 " }); //'B'
            font.Add('\u0062', new string[] { "1111 ", "1   1", "1111 ", "1   1", "1111 " }); //'b'
            font.Add('\u0043', new string[] { " 1111", "1    ", "1    ", "1    ", " 1111" }); //'C'
            font.Add('\u0063', new string[] { " 1111", "1    ", "1    ", "1    ", " 1111" }); //'c'
            font.Add('\u0044', new string[] { "1111 ", "1   1", "1    ", "1   1", "1111 " }); //'D'
            font.Add('\u0064', new string[] { "1111 ", "1   1", "1    ", "1   1", "1111 " }); //'d'
            font.Add('\u0045', new string[] { "11111", "1    ", "111  ", "1    ", "11111" }); //'E'
            font.Add('\u0065', new string[] { "11111", "1    ", "111  ", "1    ", "11111" }); //'e'
            font.Add('\u0046', new string[] { "11111", "1    ", "111  ", "1    ", "1    " }); //'F'
            font.Add('\u0066', new string[] { "11111", "1    ", "111  ", "1    ", "1    " }); //'f'
            font.Add('\u0047', new string[] { " 1111", "1    ", "1  11", "1   1", " 1111" }); //'G'
            font.Add('\u0067', new string[] { " 1111", "1    ", "1  11", "1   1", " 1111" }); //'g'
            font.Add('\u0048', new string[] { "1   1", "1   1", "11111", "1   1", "1   1" }); //'H'
            font.Add('\u0068', new string[] { "1   1", "1   1", "11111", "1   1", "1   1" }); //'h'
            font.Add('\u0049', new string[] { "11111", "  1  ", "  1  ", "  1  ", "11111" }); //'I'
            font.Add('\u0069', new string[] { "11111", "  1  ", "  1  ", "  1  ", "11111" }); //'i'
            font.Add('\u004A', new string[] { "11111", "   1 ", "   1 ", "1  1 ", " 11  " }); //'J'
            font.Add('\u006A', new string[] { "11111", "   1 ", "   1 ", "1  1 ", " 11  " }); //'j'
            font.Add('\u004B', new string[] { "1   1", "1  1 ", "111  ", "1  1 ", "1   1" }); //'K'
            font.Add('\u006B', new string[] { "1   1", "1  1 ", "111  ", "1  1 ", "1   1" }); //'k'
            font.Add('\u004C', new string[] { "1    ", "1    ", "1    ", "1    ", "11111" }); //'L'
            font.Add('\u006C', new string[] { "1    ", "1    ", "1    ", "1    ", "11111" }); //'l'
            font.Add('\u004D', new string[] { "1   1", "11 11", "1 1 1", "1   1", "1   1" }); //'M'
            font.Add('\u006D', new string[] { "1   1", "11 11", "1 1 1", "1   1", "1   1" }); //'m'
            font.Add('\u004E', new string[] { "1   1", "11  1", "1 1 1", "1  11", "1   1" }); //'N'
            font.Add('\u006E', new string[] { "1   1", "11  1", "1 1 1", "1  11", "1   1" }); //'n'
            font.Add('\u004F', new string[] { " 111 ", "1   1", "1   1", "1   1", " 111 " }); //'O'
            font.Add('\u006F', new string[] { " 111 ", "1   1", "1   1", "1   1", " 111 " }); //'o'
            font.Add('\u0050', new string[] { "1111 ", "1   1", "1111 ", "1    ", "1    " }); //'P'
            font.Add('\u0070', new string[] { "1111 ", "1   1", "1111 ", "1    ", "1    " }); //'p'
            font.Add('\u0051', new string[] { " 111 ", "1   1", "1 1 1", "1  1 ", " 11 1" }); //'Q'
            font.Add('\u0071', new string[] { " 111 ", "1   1", "1 1 1", "1  1 ", " 11 1" }); //'q'
            font.Add('\u0052', new string[] { "1111 ", "1   1", "1111 ", "1   1", "1   1" }); //'R'
            font.Add('\u0072', new string[] { "1111 ", "1   1", "1111 ", "1   1", "1   1" }); //'r'
            font.Add('\u0053', new string[] { " 1111", "1    ", " 111 ", "    1", "1111 " }); //'S'
            font.Add('\u0073', new string[] { " 1111", "1    ", " 111 ", "    1", "1111 " }); //'s'
            font.Add('\u0054', new string[] { "11111", "  1  ", "  1  ", "  1  ", "  1  " }); //'T'
            font.Add('\u0074', new string[] { "11111", "  1  ", "  1  ", "  1  ", "  1  " }); //'t'
            font.Add('\u0055', new string[] { "1   1", "1   1", "1   1", "1   1", " 111 " }); //'U'
            font.Add('\u0075', new string[] { "1   1", "1   1", "1   1", "1   1", " 111 " }); //'u'
            font.Add('\u0056', new string[] { "1   1", "1   1", "1   1", " 1 1 ", "  1  " }); //'V'
            font.Add('\u0076', new string[] { "1   1", "1   1", "1   1", " 1 1 ", "  1  " }); //'v'
            font.Add('\u0057', new string[] { "1   1", "1   1", "1 1 1", "11 11", "1   1" }); //'W'
            font.Add('\u0077', new string[] { "1   1", "1   1", "1 1 1", "11 11", "1   1" }); //'w'
            font.Add('\u0058', new string[] { "1   1", " 1 1 ", "  1  ", " 1 1 ", "1   1" }); //'X'
            font.Add('\u0078', new string[] { "1   1", " 1 1 ", "  1  ", " 1 1 ", "1   1" }); //'x'
            font.Add('\u0059', new string[] { "1   1", " 1 1 ", "  1  ", "  1  ", "  1  " }); //'Y'
            font.Add('\u0079', new string[] { "1   1", " 1 1 ", "  1  ", "  1  ", "  1  " }); //'y'
            font.Add('\u005A', new string[] { "11111", "   1 ", "  1  ", " 1   ", "11111" }); //'Z'
            font.Add('\u007A', new string[] { "11111", "   1 ", "  1  ", " 1   ", "11111" }); //'z'
            font.Add('\u005B', new string[] { "  11 ", "  1  ", "  1  ", "  1  ", "  11 " }); //'['
            font.Add('\u005C', new string[] { "1    ", " 1   ", "  1  ", "   1 ", "    1" }); //'\'
            font.Add('\u005D', new string[] { " 11  ", "  1  ", "  1  ", "  1  ", " 11  " }); //']'
            font.Add('\u005E', new string[] { "  1  ", " 1 1 ", "     ", "     ", "     " }); //'^'
            font.Add('\u005F', new string[] { "     ", "     ", "     ", "     ", "11111" }); //'_'
            font.Add('\u0060', new string[] { "  1  ", "   1 ", "     ", "     ", "     " }); //'`'
            font.Add('\u007B', new string[] { "  11 ", "  1  ", " 1   ", "  1  ", "  11 " }); //'{'
            font.Add('\u007C', new string[] { "  1  ", "  1  ", "  1  ", "  1  ", "  1  " }); //'|'
            font.Add('\u007D', new string[] { " 11  ", "  1  ", "   1 ", "  1  ", " 11  " }); //'}'
            font.Add('\u007E', new string[] { "     ", " 1   ", "1 1 1", "   1 ", "     " }); //'~'
            font.Add('\u0020', new string[] { "     ", "     ", "     ", "     ", "     " }); //' '
            
            E.Fonts.Add(font.Name, font);
            E.Fonts.Add("default", new BMyFont("default", "-", 5, 5, E));
        }

        BaconDebug getDebugger(BaconArgs Args)
        {
            int verbosity = Math.Min(Args.getFlag('v'), BaconDebug.INFO);
            if(verbosity == BaconDebug.OFF)
            {
                Echo("Log/Debugger diabled");
                return null;
            }
            if (Args.getFlag('v') >= BaconDebug.TRACE)
            {
                if(Args.getOption("debug").Count > 0)
                {
                    verbosity = BaconDebug.TRACE;
                }
            } else if (Args.getFlag('v') >= BaconDebug.DEBUG)
            {
                if (Args.getOption("debug").Count > 0)
                {
                    verbosity = BaconDebug.DEBUG;
                }
            }
            Echo(string.Format("Log Verbosity: {0}; -v Flag = {1}; --debug: {2}", verbosity, Args.getFlag('v'), (Args.getOption("debug").Count>0)));
            string tag = (Args.getOption("logScreen").Count > 0) ? Args.getOption("logScreen")[0] : "[BaconDraw_DEBUG]";
            BaconDebug Debugger = new BaconDebug(tag, GridTerminalSystem, this, verbosity, "BaconDraw");
            if (Args.getOption("logFormat").Count > 0) {
                Debugger.Format = Args.getOption("logFormat")[0];
            }
            Debugger.autoscroll = false;
            Debugger.clearPanels();
            return Debugger;

        }
        #endregion bootstrap

        #region PANELTASK
        class BMyPanelTaskBag : Dictionary<long, BMyPanelTask>
        {
            public readonly BMyEnvironment Environment;
            
            public BMyPanelTaskBag(BMyEnvironment Environment)
            {
                this.Environment = Environment;
            }

            public void Add(IMyTextPanel Panel)
            {
                if (!ContainsKey(Panel.EntityId))
                {
                    Add(Panel.EntityId, new BMyPanelTask(Environment, Panel));
                }
            }
        }
        class BMyPanelTask : List<string>
        {
            public readonly BMyEnvironment Environment;
            public readonly int currentCodeHash;
            public readonly int lastKnownCodeHash;
            public int nextLineToProgress = 0;
            public readonly IMyTextPanel Panel;
                        
            public BMyPanelTask(BMyEnvironment Environment, IMyTextPanel Panel)
            {
                this.Environment = Environment;
                this.Panel = Panel;
                this.currentCodeHash = string.Format(@"FontSize:{1},Code:{0}", Panel.GetPrivateText(),Panel.GetValueFloat("FontSize")).GetHashCode();
                this.AddRange(Panel.GetPrivateText().Split(new char[] { '\n','\r'}, StringSplitOptions.RemoveEmptyEntries));
                BaconArgs PrivateTextArgs = BaconArgs.parse(Panel.GetPrivateText());
                if(PrivateTextArgs.getOption("nextLineToProgress").Count > 0 && PrivateTextArgs.getOption("lastKnownCodeHash").Count > 0)
                {
                    int hashBuffer = 0;
                    if(int.TryParse(PrivateTextArgs.getOption("lastKnownCodeHash")[0], out hashBuffer))
                    {
                        this.lastKnownCodeHash = hashBuffer;
                    }
                    int lineBuffer = 0;
                    if(!hasChanged() && int.TryParse(PrivateTextArgs.getOption("nextLineToProgress")[0], out lineBuffer) && lineBuffer < Count)
                    {
                        this.nextLineToProgress = lineBuffer;
                    }
                }
            }

            public bool hasChanged()
            {
                return true;
               // return !this.lastKnownCodeHash.Equals(this.currentCodeHash);
            }

            public bool isFinished()
            {
                return this.nextLineToProgress > Count;
            }

            public void Save(BMyCanvas canvas)
            {
                Environment.Log?.newScope("BMyPanelTask.Save");
                BaconArgs PrivateTextArgs = new BaconArgs();
                PrivateTextArgs.add(string.Format(@"--{0}={1}", "lastKnownCodeHash", this.currentCodeHash));
                Environment.Log?.Trace("Snapshot: {0} => {1}", "lastKnownCodeHash", this.currentCodeHash);
                PrivateTextArgs.add(string.Format(@"--{0}={1}", "nextLineToProgress", this.nextLineToProgress));
                Environment.Log?.Trace("Snapshot: {0} => {1}","nextLineToProgress", this.nextLineToProgress);
                Panel.WritePrivateTitle(PrivateTextArgs.ToArguments());
                Panel.WritePublicText(canvas.ToString());
                Panel.ShowPublicTextOnScreen();
                Environment.Log?.leaveScope();
            }
        }
        #endregion PANELTASK

        #region FACTORY
        class BMyPanelTaskBagFactory
        {
            public readonly BMyEnvironment Environment;
            public BMyPanelTaskBagFactory(BMyEnvironment Environment)
            {
                this.Environment = Environment;
            }

            public BMyPanelTaskBag FromArguments(BaconArgs Args, string defaultTag, string IgnoreCubeGridByTag)
            {
                Environment.Log?.newScope("BMyPanelTaskBagFactory.FromArguments");
                BMyPanelTaskBag Bag = new BMyPanelTaskBag(Environment);
                List<string> Tags = (Args.getArguments().Count > 0) ? Args.getArguments() : new List<string>() { defaultTag };
                foreach (string tag in Tags)
                {
                    List<IMyTextPanel> buffer = new List<IMyTextPanel>();
                    Environment.Global.GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(buffer, (p =>
                     (p.CustomName.Contains(IgnoreCubeGridByTag) || p.CubeGrid.Equals(Environment.Global.Me.CubeGrid))
                     &&
                     p.CustomName.Contains(tag)
                    ));
                    foreach (IMyTextPanel Panel in buffer)
                    {
                        Bag.Add(Panel);
                    }
                }
                Environment.Log?.leaveScope();
                return Bag;
            }

        }
        class BMyCanvasFactory
        {
            public readonly BMyEnvironment Environment;
            private Point defaultFontSizeRelation = new Point(16, 17);
            private Dictionary<string, Point> FontSizeRelation = new Dictionary<string, Point>()
            {
                {"SmallLCDPanel",new Point(16,17)},
                {"SmallLCDPanelWide",new Point(32,17)},
                {"LargeLCDPanel",new Point(16,17)},
                {"LargeLCDPanelWide",new Point(32,17)},
                {"TextPanel",new Point(16,17)},
                {"SmallTextPanel",new Point(16,17)},
                {"LargeTextPanel",new Point(16,17)}
            };

            public BMyCanvasFactory(BMyEnvironment Environment)
            {
                this.Environment = Environment;
            }

            public BMyCanvas FromTask(BMyPanelTask Task)
            {
                Point dimension = getDimensionFromPanel(Task.Panel);
                BMyCanvas canvas;
                if (Task.hasChanged())
                {
                    canvas = new BMyCanvas(dimension.X, dimension.Y, Environment);
                }
                else
                {
                    canvas = new BMyCanvas(dimension.X, dimension.Y, Environment, Task.Panel.GetPublicText());
                }
                return canvas;
            }

            private Point getDimensionFromPanel(IMyTextPanel Panel)
            {
                Point relation = defaultFontSizeRelation;
                if (FontSizeRelation.ContainsKey(Panel.BlockDefinition.SubtypeName))
                {
                    relation = FontSizeRelation[Panel.BlockDefinition.SubtypeName];
                }
                return new Point(
                    Convert.ToInt32(Math.Round(relation.X / Panel.GetValueFloat("FontSize"))),
                    Convert.ToInt32(Math.Round(relation.Y / Panel.GetValueFloat("FontSize")))
                );
            }
        }
        #endregion FACTORY

        #region ENVIRONMENT
        class BMyEnvironment
        {
            public readonly BMyDrawPluginHandler DrawPlugins;
            public readonly Program Global;
            public readonly BaconArgs GlobalArgs;
            public readonly BaconDebug Log;
            public readonly Dictionary<string, BMyFont> Fonts;
            public readonly BMyColor Color;
            public readonly BMyPanelTaskBagFactory PanelTaskBagFactory;
            public readonly BMyCanvasFactory CanvasFactory;

            public BMyEnvironment(Program Global, BaconArgs GlobalArgs, BaconDebug Log)
            {
                Log?.newScope("BMyEnvironment.BMyEnvironment");
                this.Global = Global;
                this.GlobalArgs = GlobalArgs;
                this.Log = Log;
                this.DrawPlugins = new BMyDrawPluginHandler(this);
                this.Fonts = new Dictionary<string, BMyFont>();
                this.Color = new BMyColor(this);
                this.CanvasFactory = new BMyCanvasFactory(this);
                this.PanelTaskBagFactory = new BMyPanelTaskBagFactory(this);
                Log?.Trace("Envionment Initialized");
                Log?.leaveScope();
            }

            public bool TryFindFontByName(string name, out BMyFont font)
            {
                Log?.newScope("BMyEnvironment.TryFindFontByName");
                if (Fonts.ContainsKey(name))
                {
                    font = Fonts[name];
                    Log?.Trace("Matching font \"{0}\" {1}x{2}", font.Name, font.Width, font.Height);
                    Log?.leaveScope();
                    return true;
                }
                font = null;
                Log?.Trace("font \"{0}\" not found", name);
                Log?.leaveScope();
                return false;
            }

            public bool TryAddFont(BMyFont font)
            {
                Log?.newScope("BMyEnvironment.TryAddFont");
                if (Fonts.ContainsKey(font.Name))
                {
                    Log?.Trace("there is already a font named \"{0}\"", font.Name);
                }
                Fonts.Add(font.Name, font);
                Log?.Trace("add font \"{0}\"", font.Name);
                Log?.leaveScope();
                return true;
            }
            
        }
        #endregion ENVIRONMENT

        #region FONT
        class BMyFont : Dictionary<char, string[]>
        {
            public readonly BMyEnvironment Environment;
            public readonly string Name;
            public readonly string Extends;
            public readonly int Width;
            public readonly int Height;

            public BMyFont(string name, string extends, int width, int height, BMyEnvironment Environment)
            {
                Name = name;
                Extends = extends;
                Width = width;
                Height = height;
                this.Environment = Environment;
            }

            public string[] getGlyph(char glyph)
            {
                Environment.Log?.newScope("BMyFont.getGlyph");
                if (ContainsKey(glyph))
                {
                    Environment.Log?.Trace("found char '{0}' in \"{1}\"", glyph, Name);
                    Environment.Log?.leaveScope();
                    return this[glyph];
                }
                if (Environment.Fonts.ContainsKey(Extends))
                {
                    Environment.Log?.Trace("look up in parent font \"{0}\"", Extends);
                    Environment.Log?.leaveScope();
                    return Environment.Fonts[Extends].getGlyph(glyph);
                }
                Environment.Log?.Trace("char '{0}' not found in \"{1}\" or one of it's parents", glyph, Name);
                Environment.Log?.leaveScope();
                return new string[] {};
            }
        }
        #endregion FONT

        #region CANVAS
        class BMyCanvas
        {
            private char[][] pixels; //[Y][X]
            public char color = 'l';
            public char background = 'd';
            private Point Position;
            public readonly BMyEnvironment Environment;
            public int Width { get { return (pixels.Length > 0) ? pixels[0].Length : 0; } }
            public int Height { get { return pixels.Length; } }

            public BMyCanvas(int width, int height, BMyEnvironment Environment, string content) : this(width, height, Environment)
            {
                Environment.Log?.newScope("BMyCanvas.Canvas");
                if(Environment.GlobalArgs.getOption("rawOutput").Count == 0)
                {
                    content = Environment.Color.Decode(this, content);
                }
                string[] data = content.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                for(int i=0;i<data.Length && i < pixels.Length; i++)
                {
                    pixels[i] = (data[i] + (new string('0', width))).Substring(0,width).ToCharArray();
                }
                Environment.Log?.Trace("Filled canvas with exisiting content (lines:{0}; overall length:{1})", data.Length, content.Length);
                Environment.Log?.leaveScope();
            }

            public BMyCanvas(int width, int height, BMyEnvironment Environment)
            {
                Environment.Log?.newScope("BMyCanvas.Canvas");
                this.Environment = Environment;
                setPosition(0, 0);
                Clear(width,height);
                Environment.Log?.Trace("Created new Canvas with dimensions(BxH) {0}x{1}", width, height);
                Environment.Log?.leaveScope();
            }

            public bool TryParseCoords(string value, out Point coords)
            {
                Environment.Log?.newScope("BMyCanvas.TryParseCoords");
                string[] raw = value.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
                int x = 0;
                int y = 0;
                if(raw.Length == 2 && int.TryParse(raw[0], out x) && int.TryParse(raw[1], out y))
                {
                    coords = new Point(x,y);
                    Environment.Log?.Trace("Coordinates parsed from \"{0}\" => resulting in {1},{2}", value, coords.X, coords.Y);
                    Environment.Log?.leaveScope();
                    return true;
                }
                coords = new Point(0,0);

                Environment.Log?.Trace("Can't parse coordinates from \"{0}\" => resulting in {1},{2}", value, coords.X, coords.Y);
                Environment.Log?.leaveScope();
                return false;
            }

            void Clear(int width, int height)
            {
                Environment.Log?.newScope("BMyCanvas.Clear");
                width = Math.Max(width, 1);
                height = Math.Max(height, 1);
                pixels = new char[height][];
                for (int y = 0; y < pixels.Length; y++)
                {
                    pixels[y] = (new String('0', width)).ToCharArray();
                }
                Environment.Log?.leaveScope();
            }
            
            public void overrideAt(int x, int y, string data)
            {
                Environment.Log?.newScope("BMyCanvas.overrideAt");
                if (inArea(x, y))
                {
                    string bufferY = new string(pixels[y]);
                    string bufferLeft = bufferY.Substring(0, x);
                    int startIndexBufferRight = x + data.Length;
                    string bufferRight = (startIndexBufferRight < bufferY.Length) ? bufferY.Substring(startIndexBufferRight) : "";
                    string newPixelRow = (bufferLeft + data + bufferRight);
                    if(newPixelRow.Length > pixels[y].Length)
                    {
                        newPixelRow = newPixelRow.Substring(0, pixels[y].Length);
                    }
                    pixels[y] = newPixelRow.ToCharArray();
                    for (int i= newPixelRow.IndexOf(' ');i > 0 && i<pixels[y].Length;i=newPixelRow.IndexOf(' '))
                    {
                        if(i< bufferY.Length)
                        {
                            pixels[y][i] = bufferY[i];
                        } else
                        {
                            pixels[y][i] = '0';
                        }
                    }                    
                } else
                {
                    Environment.Log?.Trace("Point({0},{1}) out of range(X:`[0,{2}[` Y:`[0,{3}[`) => can't insert data", x, y, pixels[0].Length, pixels.Length);
                }
                Environment.Log?.leaveScope();
            }

            private bool inArea(int x, int y)
            {
                return (0 <= x && x < pixels[0].Length && 0 <= y && y < pixels.Length);
            }

            public void setPixel(int x, int y)
            {
                Environment.Log?.newScope("BMyCanvas.setPixel");
                if (inArea(x,y))
                {
                    pixels[y][x] = color;
                    Environment.Log?.Trace("set pixel {1},{2} to {0}", color, x, y);
                } else
                {
                    Environment.Log?.Trace("Point({1},{2}) out of range(X:`[0,{3}[` Y:`[0,{4}[`) => can't assign color '{0}'", color, x, y, pixels[0].Length, pixels.Length);
                }
                Environment.Log?.leaveScope();
            }

            public Point getPosition()
            {
                return Position;
            }

            public void setPosition(int x, int y)
            {
                Environment.Log?.newScope("BMyCanvas.setPosition");
                Environment.Log?.Trace("update position to {0},{1}", x, y);
                Position = new Point(x,y);
                Environment.Log?.leaveScope();
            }

            public string ToStringRaw()
            {
                Environment.Log?.newScope("BMyCanvas.ToStringRaw");
                List<string> slug = new List<string>();
                foreach (char[] line in pixels)
                {
                    string buffer = new string(line);
                    slug.Add(buffer);
                }
                Environment.Log?.Trace("created raw image with {0} line(s)", slug.Count);
                Environment.Log?.leaveScope();
                return string.Join("\n", slug.ToArray());
            }

            public override string ToString()
            {
                Environment.Log?.newScope("BMyCanvas.ToString");
                if(Environment.GlobalArgs.getOption("rawOutput").Count > 0)
                {
                    string buffer = ToStringRaw();
                    Environment.Log?.leaveScope();
                    return buffer;
                } else
                {
                    string buffer = Environment.Color.Encode(this);
                    Environment.Log?.Trace("created encoded image");
                    Environment.Log?.leaveScope();
                    return buffer;
                }                
            }
        }

        #endregion CANVAS

        #region COLOR
        class BMyColor
        {
            public readonly BMyEnvironment Environment;
            protected Dictionary<char, char> map = new Dictionary<char, char>() {{'g','\uE001'},{'b','\uE002'},{'r','\uE003'},{'y','\uE004'},{'w','\uE006'},{'l','\uE00E'},{'d','\uE00F'}};
            private System.Text.RegularExpressions.Regex RgxEncode;
            private System.Text.RegularExpressions.Regex RgxDecode;
            public const char PLACEHOLDER_BG = '0';

            public BMyColor(BMyEnvironment Environment)
            {
                this.Environment = Environment;
                string colorKeys = "";
                string colorValues = "";
                foreach(KeyValuePair<char,char> C in map)
                {
                    colorKeys += C.Key;
                    colorValues += C.Value;
                }
                colorKeys = System.Text.RegularExpressions.Regex.Escape(colorKeys);
                colorValues = System.Text.RegularExpressions.Regex.Escape(colorValues);
                RgxEncode = new System.Text.RegularExpressions.Regex(string.Format(@"[^{0}\n]", colorKeys), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                RgxDecode = new System.Text.RegularExpressions.Regex(string.Format(@"[^{0}\n]", colorValues), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }

            public string Encode(BMyCanvas canvas)
            {
                Environment.Log?.newScope("BMyColor.Encode");
                StringBuilder image = new StringBuilder(RgxEncode.Replace(canvas.ToStringRaw().ToLowerInvariant(), PLACEHOLDER_BG.ToString()));
                image = image.Replace(PLACEHOLDER_BG, canvas.background);
                foreach (KeyValuePair<char, char> color in map)
                {
                    image = image.Replace(color.Key, color.Value);
                }
                Environment.Log?.leaveScope();
                return image.ToString();
            }

            public string Decode(BMyCanvas canvas, string data)
            {
                Environment.Log?.newScope("BMyColor.Decode");
                StringBuilder image = new StringBuilder(RgxDecode.Replace(canvas.ToStringRaw().ToLowerInvariant(), PLACEHOLDER_BG.ToString()));
                foreach (KeyValuePair<char, char> color in map)
                {
                    image = image.Replace(color.Value, color.Key);
                }
                Environment.Log?.leaveScope();
                return image.ToString();
            }

            public char getColorCode(string args)
            {
                Environment.Log?.newScope("BMyColor.getColorCode");
                string raw = args.Trim();
                char value = '1';
                if ((new System.Text.RegularExpressions.Regex(@"^(\\u[0-9a-f]{4})|(U\+[0-9a-f]{4})$").IsMatch(raw)))
                {
                    int ord;
                    if (int.TryParse(raw.Substring(2), out ord))
                    {
                        value = (char)ord;
                        Environment.Log?.Trace("Found unicode Character: {0} => '{1}'", raw, value);
                    }
                }
                else if (raw.Length > 0)
                {
                    value = raw[0];
                    Environment.Log?.Trace("Matching Color: {0} => '{1}'", raw, value);
                }
                else
                {
                    Environment.Log?.Trace("No matching Color => using default: {0} => '{1}'", raw, value);
                }
                Environment.Log?.leaveScope();
                return value;
            }
        }
        #endregion COLOR

        #region Pluginsystem
        #region BMyPluginHandler base
        class BMyPluginHandler<T> : Dictionary<string, List<T>> where T : BMyPlugin
        {
            private Dictionary<string, List<T>> AvailablePlugins = new Dictionary<string, List<T>>();
            private Dictionary<string, List<T>> EnabledPlugins = new Dictionary<string, List<T>>();
            public readonly BMyEnvironment Environment;
            public BMyPluginHandler(BMyEnvironment Environment)
            {
                this.Environment = Environment;
            }

            protected void AddAvailable(params T[] Plugins)
            {
                Environment.Log?.newScope("BMyPluginHandler.AddAvailable");
                Environment.Log?.Trace("Try to register {0} Plugin(s)", Plugins.Length);

                foreach (T Plugin in Plugins)
                {
                    Environment.Log?.Trace("Try to register Plugin {0}/{1}", Plugin.Type, Plugin.SubType);
                    if (!AvailablePlugins.ContainsKey(Plugin.Type))
                    {
                        Environment.Log?.Trace("create new Collection for Type \"{0}\"", Plugin.Type);
                        AvailablePlugins.Add(Plugin.Type, new List<T>());
                    }
                    if (!AvailablePlugins[Plugin.Type].Contains(Plugin))
                    {
                        Environment.Log?.Trace("Add plugin \"{0}/{1}\"", Plugin.Type, Plugin.SubType);
                        AvailablePlugins[Plugin.Type].Add(Plugin);
                    } else
                    {
                        Environment.Log?.Trace("Cant add plugin \"{0}/{1}\". Plugin already exists.", Plugin.Type, Plugin.SubType);
                    }
                }
                Environment.Log?.leaveScope();
            }
            public bool FindBySubtype(string SubType, out List<T> Matches)
            {
                Environment.Log?.newScope("BMyPluginHandler.findBySubtype");

                string subTypeLow = SubType.ToLowerInvariant();
                Matches = new List<T>();
                if (!ContainsKey(subTypeLow))
                {
                    Environment.Log?.Trace("found no plugins for {1}", Matches.Count, SubType);
                    Environment.Log?.leaveScope();
                    return false;
                }
                Matches = this[subTypeLow];
                Environment.Log?.Trace("found {0} plugins for {1}", Matches.Count, SubType);
                Environment.Log?.leaveScope();
                return true;
            }
            public bool isEnabled(string SubType)
            {
                return ContainsKey(SubType.ToLowerInvariant());
            }
            public bool TryEnable(string Type)
            {
                Environment.Log?.newScope("BMyPluginHandler.TryEnable");

                if (!AvailablePlugins.ContainsKey(Type))
                {
                    Environment.Log?.Trace("no plugins available for \"{0}\"", Type);
                    Environment.Log?.leaveScope();
                    return false;
                }

                foreach(T Plugin in AvailablePlugins[Type])
                {
                    if (!ContainsKey(Plugin.SubType))
                    {
                        this.Add(Plugin.SubType, new List<T>());
                    }
                    if (!this[Plugin.SubType].Contains(Plugin))
                    {
                        this[Plugin.SubType].Add(Plugin);
                        Plugin.init();
                    }
                }

                Environment.Log?.leaveScope();
                return true;
            }
            public bool TryRun(string SubType, BaconArgs Args, params object[] parameters)
            {
                Environment.Log?.newScope("BMyPluginHandler.TryRun");
                Environment.Log?.Trace(@"subtype ""{0}"" with {1}", SubType, Args.ToString());
                List<T> Plugins;
                if (!FindBySubtype(SubType, out Plugins))
                {
                    Environment.Log?.leaveScope();
                    return false;
                }
                bool success = false;
                for(int i = 0;!success && i < Plugins.Count; i++)
                {
                    Environment.Log?.newScope(string.Format("BMyPlugin({0}/{1}).TryRun", Plugins[i].Type, Plugins[i].SubType));
                    success = Plugins[i].TryRun(Args, parameters);
                    Environment.Log?.leaveScope();
                    if (success)
                    {
                        Environment.Log?.Trace("Success on plugin \"{0}/{1}\" with {2}", Plugins[i].Type, Plugins[i].SubType, Args.ToString());
                    }
                    else
                    {
                        Environment.Log?.Trace("Failed on plugin \"{0}/{1}\" with {2}", Plugins[i].Type, Plugins[i].SubType, Args.ToString());
                    }
                }
                if (!success)
                {
                    Environment.Log?.Trace("No success with {0}", Args.ToString());
                }

                Environment.Log?.leaveScope();
                return success;
            }
        }
        #endregion BMyPluginHandler base
        #region Plugin base
        abstract class BMyPlugin
        {
            abstract public string Type { get; }
            abstract protected string subType { get; }

            public string SubType { get { return subType.ToLowerInvariant(); } }
            public readonly BMyEnvironment Environment;

            abstract protected bool isValid(BaconArgs Args);
            abstract protected bool TryExecute(BaconArgs Args, params object[] parameters);
            abstract protected bool isValidParameter(params object[] parameters);

            public bool TryRun(BaconArgs Args, params object[] parameters)
            {
                if (isValidParameter(parameters) && isValid(Args))
                {
                    return TryExecute(Args, parameters);
                }
                return false;
            }
            public BMyPlugin(BMyEnvironment Environment)
            {
                this.Environment = Environment;
            }
            public void init() {}
        }
        #endregion Plugin base

        #region Pluginhandlers
        class BMyDrawPluginHandler : BMyPluginHandler<BMyDrawPlugin>
        {
            public BMyDrawPluginHandler(BMyEnvironment Environment) : base(Environment) {
                AddAvailable(
                    new BMyDrawPlugin_BaconDraw_Background(Environment),
                    new BMyDrawPlugin_BaconDraw_Circle(Environment),
                    new BMyDrawPlugin_BaconDraw_Color(Environment),
                    new BMyDrawPlugin_BaconDraw_Font(Environment),
                    new BMyDrawPlugin_BaconDraw_LineTo(Environment),
                    new BMyDrawPlugin_BaconDraw_MoveTo(Environment),
                    new BMyDrawPlugin_BaconDraw_Polygon(Environment),
                    new BMyDrawPlugin_BaconDraw_Rectangle(Environment),
                    new BMyDrawPlugin_BaconDraw_Text(Environment),
                    new BMyDrawPlugin_BaconDraw_Bitmap(Environment)
                );
            }
        }
        #endregion Pluginhandlers

        #region PluginTypes
        abstract class BMyDrawPlugin : BMyPlugin
        {
            public BMyDrawPlugin(BMyEnvironment Environment) : base(Environment) { }
            protected override bool isValidParameter(params object[] parameters)
            {
                Environment.Log?.newScope("BMyDrawPlugin.isValidParameter");
                bool valid = true;
                if (parameters.Length == 1 && !(parameters[0] is BMyCanvas))
                {
                    Environment.Log?.Trace("parameter must be of type {0}", "BMyCanvas");
                    valid = false;
                }
                Environment.Log?.leaveScope();
                return valid;
            }
        }
        #endregion PluginTypes
        #endregion Pluginsystem

        #region included Plugins
        #region DrawPlugin "BaconDraw"
        class BMyDrawPlugin_BaconDraw_LineTo : BMyDrawPlugin
        {
            public override string Type { get { return "BaconDraw"; } }
            protected override string subType { get { return "lineto"; } }
            public BMyDrawPlugin_BaconDraw_LineTo(BMyEnvironment Environment) : base(Environment){}
            protected override bool isValid(BaconArgs Args)
            {
                if (!(Args.getArguments().Count == 1))
                {
                    Environment.Log?.Trace("wrong number of arguments");
                    return false;
                }

                if (!(new System.Text.RegularExpressions.Regex(@"\d+,\d+")).IsMatch(Args.getArguments()[0]))
                {
                    Environment.Log?.Trace("argument in wrong format (must be a number)");
                    return false;
                }
                return true;
            }
            protected override bool TryExecute(BaconArgs Args, params object[] parameters)
            {
                BMyCanvas canvas = parameters[0] as BMyCanvas;
                Point dest;
                if (Args.getArguments().Count < 1 || !canvas.TryParseCoords(Args.getArguments()[0], out dest))
                {
                    Environment.Log?.Trace("no valid arguments => can't draw {0}", SubType);
                    return false;
                }
                Point Position = canvas.getPosition();
                int _x, _y, _t, _deltaX, _deltaY, _incX, _incY, _pdx, _pdy, _ddx, _ddy, _es, _el, _err;
                _deltaX = dest.X - Position.X;
                _deltaY = dest.Y - Position.Y;

                _incX = Math.Sign(_deltaX);
                _incY = Math.Sign(_deltaY);
                if (_deltaX < 0) _deltaX = -_deltaX;
                if (_deltaY < 0) _deltaY = -_deltaY;

                if (_deltaX > _deltaY)
                {
                    _pdx = _incX; _pdy = 0;
                    _ddx = _incX; _ddy = _incY;
                    _es = _deltaY; _el = _deltaX;
                }
                else
                {
                    _pdx = 0; _pdy = _incY;
                    _ddx = _incX; _ddy = _incY;
                    _es = _deltaX; _el = _deltaY;
                }
                _x = Position.X;
                _y = Position.Y;
                _err = _el / 2;
                canvas.setPixel(_x, _y);

                for (_t = 0; _t < _el; ++_t)
                {
                    _err -= _es;
                    if (_err < 0)
                    {
                        _err += _el;
                        _x += _ddx;
                        _y += _ddy;
                    }
                    else
                    {
                        _x += _pdx;
                        _y += _pdy;
                    }
                    canvas.setPixel(_x, _y);
                }
                canvas.setPosition(_x, _y);
                Environment.Log?.Trace("draw {2} to {0},{1}", _x, _y, SubType);
                return true;
            }
        }
        class BMyDrawPlugin_BaconDraw_Polygon : BMyDrawPlugin
        {
            System.Text.RegularExpressions.Regex MatchRgx = new System.Text.RegularExpressions.Regex(@"\d+,\d+");
            public override string Type { get { return "BaconDraw"; } }
            protected override string subType { get { return "polygon"; } }
            public BMyDrawPlugin_BaconDraw_Polygon(BMyEnvironment Environment) : base(Environment){ }
            protected override bool TryExecute(BaconArgs Args, params object[] parameters)
            {
                BMyCanvas canvas = parameters[0] as BMyCanvas;
                if (!Environment.DrawPlugins.isEnabled("lineto"))
                {
                    Environment.Log?.Trace("no plugin for \"lineto\" => can't draw {0}", SubType);
                    return false;
                }
                for(int i = 0; i < Args.getArguments().Count; i++)
                {
                    BaconArgs LineToArgs = new BaconArgs();
                    LineToArgs.add(Args.getArguments()[i]);
                    if(!Environment.DrawPlugins.TryRun("lineto", LineToArgs, canvas))
                    {
                        Environment.Log?.Trace("failed drawing line to {0} for {1}", Args.getArguments()[i], SubType);
                        return false;
                    }
                }
                Environment.Log?.Trace("drawed {0}", SubType);
                return true;
            }
            protected override bool isValid(BaconArgs Args)
            {
                if(Args.getArguments().Count < 1)
                {
                    Environment.Log?.Trace("not enough arguments (must be at least 1)");
                    return false;
                }
                bool valid = true;

                for(int i = 0; i < Args.getArguments().Count; i++)
                {
                    if (!MatchRgx.IsMatch(Args.getArguments()[i]))
                    {
                        Environment.Log?.Trace("argument[{1}]({0}) invalid. Must be `number,number`", Args.getArguments()[i], i);
                        valid = false;
                    }
                }
                return valid;
            }
        }
        class BMyDrawPlugin_BaconDraw_Rectangle : BMyDrawPlugin
        {
            public override string Type { get { return "BaconDraw"; } }
            protected override string subType { get { return "rect"; } }
            public BMyDrawPlugin_BaconDraw_Rectangle(BMyEnvironment Environment) : base(Environment){ }
            protected override bool TryExecute(BaconArgs Args, params object[] parmeters)
            {
                BMyCanvas canvas = parmeters[0] as BMyCanvas;
                if (!Environment.DrawPlugins.isEnabled("polygon"))
                {
                    Environment.Log?.Trace("no plugin for \"polygon\" => can't draw {0}", SubType);
                    return false;
                }
                Point dest;
                if(!canvas.TryParseCoords(Args.getArguments()[0], out dest)){
                    Environment.Log?.Trace("invalid coordinates {0} => can't draw {1}", Args.getArguments()[0], SubType);
                    return false;
                }
                BaconArgs PolygonArgs = new BaconArgs();
                PolygonArgs.add(string.Format("{0},{1}", dest.X, canvas.getPosition().Y));
                PolygonArgs.add(string.Format("{0},{1}", dest.X, dest.Y));
                PolygonArgs.add(string.Format("{0},{1}", canvas.getPosition().X, dest.Y));
                PolygonArgs.add(string.Format("{0},{1}", canvas.getPosition().X, canvas.getPosition().Y));
                if(!Environment.DrawPlugins.TryRun("polygon", PolygonArgs, canvas))
                {
                    Environment.Log?.Trace("failed drawing {0}", SubType);
                    return false;
                }
                canvas.setPosition(dest.X,dest.Y);
                Environment.Log?.Trace("drawed a {0} to {1},{2}", SubType, dest.X, dest.Y);
                return true;
            }
            protected override bool isValid(BaconArgs Args)
            {
                if (!(Args.getArguments().Count == 1))
                {
                    Environment.Log?.Trace("wrong number of arguments");
                    return false;
                }

                if (!(new System.Text.RegularExpressions.Regex(@"\d+,\d+")).IsMatch(Args.getArguments()[0]))
                {
                    Environment.Log?.Trace("argument in wrong format (must be a number)");
                    return false;
                }

                return true;
            }
        }
        class BMyDrawPlugin_BaconDraw_Circle : BMyDrawPlugin
        {
            public override string Type { get { return "BaconDraw"; } }
            protected override string subType { get { return "circle"; } }
            public BMyDrawPlugin_BaconDraw_Circle(BMyEnvironment Environment) : base(Environment){ }
            protected override bool TryExecute(BaconArgs Args, params object[] parmeters)
            {
                BMyCanvas canvas = parmeters[0] as BMyCanvas;
                int Radius;
                if(!int.TryParse(Args.getArguments()[0], out Radius))
                {
                    Environment.Log?.Trace("Invalid argument \"{0}\" must be a number", Args.getArguments()[0]);
                    return false;
                }
                int d;
                int x = Radius;
                d = Radius * -1;
                for (int y = 0; y <= x; y++)
                {
                    canvas.setPixel(canvas.getPosition().X + x, canvas.getPosition().Y + y);
                    canvas.setPixel(canvas.getPosition().X + y, canvas.getPosition().Y + x);
                    canvas.setPixel(canvas.getPosition().X + y, canvas.getPosition().Y + -x);
                    canvas.setPixel(canvas.getPosition().X + x, canvas.getPosition().Y + -y);
                    canvas.setPixel(canvas.getPosition().X + -x, canvas.getPosition().Y + -y);
                    canvas.setPixel(canvas.getPosition().X + -y, canvas.getPosition().Y + -x);
                    canvas.setPixel(canvas.getPosition().X + -y, canvas.getPosition().Y + x);
                    canvas.setPixel(canvas.getPosition().X + -x, canvas.getPosition().Y + y);
                    d = d + 2 * y + 1;
                    if (d > 0)
                    {
                        d = d - 2 * x + 2;
                        x = x - 1;
                    }
                }
                Environment.Log?.Trace("drawed a {0} with a radius of {1}", SubType, Radius);
                return true;
            }
            protected override bool isValid(BaconArgs Args)
            {
                if(!(Args.getArguments().Count == 1))
                {
                    Environment.Log?.Trace("wrong number of arguments");
                    return false;
                }

                if(!(new System.Text.RegularExpressions.Regex(@"\d+")).IsMatch(Args.getArguments()[0]))
                {
                    Environment.Log?.Trace("argument in wrong format (must be a number)");
                    return false;
                }

                return true;
            }
        }
        class BMyDrawPlugin_BaconDraw_Font : BMyDrawPlugin
        {
            protected override string subType { get { return "font"; }}
            public override string Type { get { return "BaconDraw"; } }
            private System.Text.RegularExpressions.Regex GlyphRgx = new System.Text.RegularExpressions.Regex(@"'.'[\S ]+");
            private System.Text.RegularExpressions.Regex SizeRgx = new System.Text.RegularExpressions.Regex(@"\d+x\d+");
            private System.Text.RegularExpressions.Regex NameRgx = new System.Text.RegularExpressions.Regex(@"[^\s:]+(:[^\s:]+)?");
            private string splitChunkPatternFormat = @"(\S{{{0}}})";
            const int indexChars = 2;
            const int indexSize = 1;
            const int indexName = 0;
            const int indexGlyph = 1;
            const int indexGlyphData = 3;
            public BMyDrawPlugin_BaconDraw_Font(BMyEnvironment Environment) : base(Environment){}
            protected override bool isValid(BaconArgs Args)
            {
                if(Args.getArguments().Count < 3)
                {
                    Environment.Log?.Trace("not enough arguments");
                    return false;
                }
                if (!NameRgx.IsMatch(Args.getArguments()[indexName]))
                {
                    Environment.Log?.Trace("invalid argument for fontname `{0}`", Args.getArguments()[indexName]);
                    return false;
                }

                if(!SizeRgx.IsMatch(Args.getArguments()[indexSize]))
                {
                    Environment.Log?.Trace("invalid argument for fontsize `{0}`", Args.getArguments()[indexSize]);
                    return false;
                }
                int w;
                int h;
                string[] size = Args.getArguments()[indexSize].Split(',');
                if(!(size.Length == 2) || !int.TryParse(size[0], out w) || !int.TryParse(size[1], out h))
                {
                    Environment.Log?.Trace("invalid argument for fontsize `{0}`", Args.getArguments()[indexSize]);
                }
                
                for(int i = indexChars; i < Args.getArguments().Count; i++)
                {
                    if (!GlyphRgx.IsMatch(Args.getArguments()[i]))
                    {
                        Environment.Log?.Trace("Invalid character definition `{0}` at argument #{1}", Args.getArguments()[i], i+1);
                        return false;
                    }
                }
                return true;
            }
            protected override bool TryExecute(BaconArgs Args, params object[] parameters)
            {
                int w = int.Parse(Args.getArguments()[indexSize].Split(',')[0]);
                int h = int.Parse(Args.getArguments()[indexSize].Split(',')[1]);
                string[] nameArg = Args.getArguments()[indexName].Split(':');
                string name = nameArg[0];
                string extends = "";
                if (nameArg.Length == 2)
                {
                    extends = nameArg[1];
                }
                BMyFont font = new BMyFont(name, extends, w, h, Environment);
                for (int i = indexChars; i < Args.getArguments().Count; i++)
                {
                    string argument = Args.getArguments()[i];
                    char glyph = argument[indexGlyph];
                    string[] data = getData(argument.Substring(indexGlyphData), w, h);
                    font.Add(glyph, data);
                }
                Environment.Log?.Trace("new font \"{0}{1}\" with BxH {2}x{3} and {4} characters", font.Name, (extends.Length >0)?":"+font.Extends:"",font.Width,font.Height,font.Count);
                return Environment.TryAddFont(font);
            }
            private string[] getData(string data, int width, int height)
            {
                List<string> buffer = new List<string>();
                System.Text.RegularExpressions.MatchCollection Matches = (new System.Text.RegularExpressions.Regex(string.Format(splitChunkPatternFormat, width))).Matches(data);
                foreach(System.Text.RegularExpressions.Match Match in Matches)
                {
                    if (Match.Value.Trim().Length == width)
                    {
                        buffer.Add(Match.Value);
                    }
                }
                for(int i = buffer.Count; i < height; i++)
                {
                    buffer.Add(new string('0', width));
                }

                return buffer.GetRange(0,height).ToArray();
            }
        }
        class BMyDrawPlugin_BaconDraw_Text : BMyDrawPlugin
        {
            protected override string subType { get { return "text"; } }
            public override string Type { get { return "BaconDraw"; } }
            const int indexName = 1;
            public BMyDrawPlugin_BaconDraw_Text(BMyEnvironment Environment) : base(Environment){}
            protected override bool isValid(BaconArgs Args)
            {
                if (Args.getArguments().Count < 2)
                {
                    Environment.Log?.Trace("not enough arguments");
                    return false;
                }
                return true;
            }
            protected override bool TryExecute(BaconArgs Args, params object[] parameters)
            {
                BMyCanvas canvas = parameters[0] as BMyCanvas;
                BMyFont font;
                if (!Environment.TryFindFontByName(Args.getArguments()[0], out font))
                {
                    Environment.Log?.Trace("can't render text with {0}", Args.getArguments()[0]);
                    return false;
                }
                Point posInit = canvas.getPosition();
                bool wordwrap = (Args.getOption("word-wrap").Count > 0);
                for (int i = 1; i < Args.getArguments().Count; i++)
                {
                    string text = Args.getArguments()[i];
                    Environment.Log?.Trace("Subpart: {0}", text);
                    foreach (char glyph in text.ToCharArray())
                    {
                        if (wordwrap && (canvas.getPosition().X + font.Width) >= canvas.Width)
                        {
                            canvas.setPosition(posInit.X, canvas.getPosition().Y + font.Height);
                        }
                        string[] data = font.getGlyph(glyph);
                        Environment.Log?.Trace(@"glyph '{0}' => ({2})[""{1}""]", glyph, string.Join("\",\"", data), data.Length);
                        for (int y = 0; y < font.Height; y++)
                        {
                            if (y < data.Length && 0 < data[y].Trim().Length)
                            {
                                canvas.overrideAt(canvas.getPosition().X, canvas.getPosition().Y + y, data[y]);
                            }
                        }
                        canvas.setPosition(canvas.getPosition().X + font.Width, canvas.getPosition().Y);
                    }
                }
                return true;
            }
        }
        class BMyDrawPlugin_BaconDraw_Background : BMyDrawPlugin
        {
            protected override string subType { get { return "background"; } }
            public override string Type { get { return "BaconDraw"; } }
            public BMyDrawPlugin_BaconDraw_Background(BMyEnvironment Environment) : base(Environment) { }
            protected override bool isValid(BaconArgs Args)
            {
                if(!(Args.getArguments().Count == 1)){
                    Environment.Log?.Trace("wrong number of arguments");
                    return false;
                }
                return true;
            }
            protected override bool TryExecute(BaconArgs Args, params object[] parameters)
            {
                BMyCanvas canvas = parameters[0] as BMyCanvas;
                canvas.background = Environment.Color.getColorCode(Args.getArguments()[0]);
                Environment.Log?.Trace("interpreted \"{0}\" as color '{1}'", Args.getArguments()[0], canvas.background);
                return true;
            }
        }
        class BMyDrawPlugin_BaconDraw_Color : BMyDrawPlugin
        {
            protected override string subType { get { return "color"; } }
            public override string Type { get { return "BaconDraw"; } }
            public BMyDrawPlugin_BaconDraw_Color(BMyEnvironment Environment) : base(Environment) { }
            protected override bool isValid(BaconArgs Args)
            {
                if (!(Args.getArguments().Count == 1))
                {
                    Environment.Log?.Trace("wrong number of arguments");
                    return false;
                }
                return true;
            }
            protected override bool TryExecute(BaconArgs Args, params object[] parameters)
            {
                BMyCanvas canvas = parameters[0] as BMyCanvas;
                canvas.color = Environment.Color.getColorCode(Args.getArguments()[0]);
                Environment.Log?.Trace("interpreted \"{0}\" as color '{1}'", Args.getArguments()[0], canvas.color);
                return true;
            }
        }
        class BMyDrawPlugin_BaconDraw_MoveTo : BMyDrawPlugin
        {
            protected override string subType { get{ return "moveto"; } }
            public override string Type { get{ return "BaconDraw"; } }
            public BMyDrawPlugin_BaconDraw_MoveTo(BMyEnvironment Environment) : base(Environment) { }
            protected override bool isValid(BaconArgs Args)
            {
                if (!(Args.getArguments().Count == 1))
                {
                    Environment.Log?.Trace("wrong number of arguments");
                    return false;
                }

                if (!(new System.Text.RegularExpressions.Regex(@"\d+,\d+")).IsMatch(Args.getArguments()[0]))
                {
                    Environment.Log?.Trace("argument in wrong format (must be a number)");
                    return false;
                }
                return true;
            }
            protected override bool TryExecute(BaconArgs Args, params object[] parameters)
            {
                BMyCanvas canvas = parameters[0] as BMyCanvas;
                Point coord;
                if(!canvas.TryParseCoords(Args.getArguments()[0], out coord))
                {
                    Environment.Log?.Trace("unable to paarse coordinates: {0}", Args.getArguments()[0]);
                    return false;
                }
                canvas.setPosition(coord.X, coord.Y);
                return true;
            }
        }
        class BMyDrawPlugin_BaconDraw_Bitmap : BMyDrawPlugin
        {
            public override string Type { get { return "BaconDraw"; } }
            protected override string subType { get { return "bitmap"; } }

            public BMyDrawPlugin_BaconDraw_Bitmap(BMyEnvironment Environment) : base(Environment) { }

            protected override bool isValid(BaconArgs Args)
            {
                if (!(Args.getArguments().Count > 0))
                {
                    Environment.Log?.Trace("no argument defiend");
                    return false;
                }
                return true;               
            }

            protected override bool TryExecute(BaconArgs Args, params object[] parameters)
            {
                BMyCanvas canvas = parameters[0] as BMyCanvas;
                for(int i = 0; i < Args.getArguments().Count; i++)
                {
                    canvas.overrideAt(canvas.getPosition().X, canvas.getPosition().Y+i, Args.getArguments()[i]);
                }
                return true;
            }
        }
        #endregion DrawPlugin "BaconDraw"
        #endregion included Plugins

        #region 3rd party Plugins

        #endregion 3rd party Plugins

        #region included Libs
        public class BaconArgs { public string InputData; static public BaconArgs parse(string a) { return (new Parser()).parseArgs(a); } static public string Escape(object a) { return string.Format("{0}", a).Replace(@"\", @"\\").Replace(@" ", @"\ ").Replace(@"""", @"\"""); } static public string UnEscape(string a) { return a.Replace(@"\""", @"""").Replace(@"\ ", @" ").Replace(@"\\", @"\"); } public class Parser { static Dictionary<string, BaconArgs> h = new Dictionary<string, BaconArgs>(); public BaconArgs parseArgs(string a) { if (!h.ContainsKey(a)) { var b = new BaconArgs(); b.InputData = a; var c = false; var d = false; var e = new StringBuilder(); for (int f = 0; f < a.Length; f++) { var g = a[f]; if (c) { e.Append(g); c = false; } else if (g.Equals('\\')) c = true; else if (d && !g.Equals('"')) e.Append(g); else if (g.Equals('"')) d = !d; else if (g.Equals(' ')) if (e.ToString().Equals("--")) { b.add(a.Substring(f).TrimStart()); e.Clear(); break; } else { b.add(e.ToString()); e.Clear(); } else e.Append(g); } if (e.Length > 0) b.add(e.ToString()); h.Add(a, b); } return h[a]; } } protected Dictionary<char, int> h = new Dictionary<char, int>(); protected List<string> i = new List<string>(); protected Dictionary<string, List<string>> j = new Dictionary<string, List<string>>(); public List<string> getArguments() { return i; } public int getFlag(char a) { return h.ContainsKey(a) ? h[a] : 0; } public List<string> getOption(string a) { return j.ContainsKey(a) ? j[a] : new List<string>(); } public void add(string a) { if (a.Trim().Length > 0) if (!a.StartsWith("-")) { i.Add(a); } else if (a.StartsWith("--")) { KeyValuePair<string, string> b = k(a); string c = b.Key.Substring(2); if (!j.ContainsKey(c)) { j.Add(c, new List<string>()); } j[c].Add(b.Value); } else { string b = a.Substring(1); for (int d = 0; d < b.Length; d++) { if (this.h.ContainsKey(b[d])) { this.h[b[d]]++; } else { this.h.Add(b[d], 1); } } } } KeyValuePair<string, string> k(string a) { string[] b = a.Split(new char[] { '=' }, 2); return new KeyValuePair<string, string>(b[0], (b.Length > 1) ? b[1] : null); } public string ToArguments() { var a = new List<string>(); foreach (string argument in this.getArguments()) a.Add(Escape(argument)); foreach (KeyValuePair<string, List<string>> option in this.j) { var b = "--" + Escape(option.Key); foreach (string optVal in option.Value) a.Add(b + ((optVal != null) ? "=" + Escape(optVal) : "")); } var c = (h.Count > 0) ? "-" : ""; foreach (KeyValuePair<char, int> flag in h) c += new String(flag.Key, flag.Value); a.Add(c); return String.Join(" ", a.ToArray()); } override public string ToString() { var a = new List<string>(); foreach (string key in j.Keys) a.Add(l(key) + ":[" + string.Join(",", j[key].ConvertAll<string>(b => l(b)).ToArray()) + "]"); var c = new List<string>(); foreach (char key in h.Keys) c.Add(key + ":" + h[key].ToString()); var d = new StringBuilder(); d.Append("{\"a\":["); d.Append(string.Join(",", i.ConvertAll<string>(b => l(b)).ToArray())); d.Append("],\"o\":[{"); d.Append(string.Join("},{", a)); d.Append("}],\"f\":[{"); d.Append(string.Join("},{", c)); d.Append("}]}"); return d.ToString(); } string l(string a) { return (a != null) ? "\"" + a.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" : @"null"; } }
        public class BaconDebug { public const int OFF = 0; public const int FATAL = 1; public const int ERROR = 2; public const int WARN = 3; public const int INFO = 4; public const int DEBUG = 5; public const int TRACE = 6; Dictionary<int, string> h = new Dictionary<int, string>() { { OFF, "OFF" }, { FATAL, "FATAL" }, { ERROR, "ERROR" }, { WARN, "WARN" }, { INFO, "INFO" }, { DEBUG, "DEBUG" }, { TRACE, "TRACE" }, }; List<IMyTextPanel> j = new List<IMyTextPanel>(); MyGridProgram k; List<KeyValuePair<string, long>> l = new List<KeyValuePair<string, long>>(); int m = OFF; public string Format = @"[{0}-{1}.{2}][{3}][{4}][IC {5}/{6}][MCC {8}/{9}] {7}"; bool n = true; public int remainingInstructions { get { return k.Runtime.MaxInstructionCount - k.Runtime.CurrentInstructionCount; } } public bool autoscroll { get { return n; } set { n = value; } } public void clearPanels() { for (int a = 0; a < j.Count; a++) j[a].WritePublicText(""); } public BaconDebug(string a, IMyGridTerminalSystem b, MyGridProgram c, int d, string e = "BaconDebug") { this.m = d; var f = new List<IMyTerminalBlock>(); b.GetBlocksOfType<IMyTextPanel>(f, ((IMyTerminalBlock g) => g.CustomName.Contains(a) && g.CubeGrid.Equals(c.Me.CubeGrid))); j = f.ConvertAll<IMyTextPanel>(g => g as IMyTextPanel); this.k = c; newScope(e); } public int getVerbosity() { return m; } public MyGridProgram getGridProgram() { return this.k; } public void newScope(string a) { l.Add(new KeyValuePair<string, long>(a, DateTime.Now.Ticks)); if (this.m.Equals(TRACE)) this.Trace("STEP INTO SCOPE"); } public void leaveScope() { if (l.Count > 0 && this.m.Equals(TRACE)) this.Trace("LEAVE SCOPE ({0} Ticks)", o((l.Count > 0 ? l[l.Count - 1].Value : 0))); if (l.Count > 1) l.RemoveAt(l.Count - 1); } public string getSender() { if (l.Count > 0) if (this.m.Equals(TRACE)) { List<string> a = new List<string>(); foreach (KeyValuePair<string, long> entry in l) { a.Add(entry.Key); } return string.Join(">", a.ToArray()); } else { return l[l.Count - 1].Key; } return "NO SCOPE DEFINED"; } double o(long a) { long b = DateTime.Now.Ticks; return (Math.Max(a, b) - Math.Min(a, b)); } public void Fatal(string a, params object[] b) { Fatal(string.Format(a, b)); } public void Error(string a, params object[] b) { Error(string.Format(a, b)); } public void Warn(string a, params object[] b) { Warn(string.Format(a, b)); } public void Info(string a, params object[] b) { Info(string.Format(a, b)); } public void Debug(string a, params object[] b) { Debug(string.Format(a, b)); } public void Trace(string a, params object[] b) { Trace(string.Format(a, b)); } public void Fatal(string a) { add(a, FATAL); } public void Error(string a) { add(a, ERROR); } public void Warn(string a) { add(a, WARN); } public void Info(string a) { add(a, INFO); } public void Debug(string a) { add(a, DEBUG); } public void Trace(string a) { add(a, TRACE); } public void add(string a, int b) { p(a, b); if (b <= this.m) { var c = t(a, b); for (int d = 0; d < j.Count; d++) { var e = new List<string>(); e.AddRange(j[d].GetPublicText().Trim().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)); e.Add(c); r(ref e, s(j[d])); var f = string.Join("\n", e.ToArray()); q(ref f); j[d].WritePublicText(f); } } } void p(string a, int b) { if (b <= ERROR) k.Echo(a); } void q(ref string a) { if (100000 < a.Length) { a = a.Substring(a.Length - 100000); int b = a.IndexOf('\n'); a = a.Substring(a.Length - b).TrimStart(new char[] { '\n', '\r' }); } } void r(ref List<string> a, int b) { if (autoscroll && 0 < b && b < a.Count) a.RemoveRange(0, a.Count - b); } int s(IMyTextPanel a) { float b = a.GetValueFloat("FontSize"); if (b == 0.0f) b = 0.01f; return Convert.ToInt32(Math.Ceiling(17.0f / b)); } string t(string a, int b) { DateTime c = DateTime.Now; object[] d = new object[] { c.ToShortDateString(), c.ToShortTimeString(), c.Millisecond.ToString().TrimStart('0'), u(b), getSender(), k.Runtime.CurrentInstructionCount, k.Runtime.MaxInstructionCount, a, k.Runtime.CurrentMethodCallCount, k.Runtime.MaxMethodCallCount, }; return string.Format(Format, d); } string u(int a) { if (h.ContainsKey(a)) return h[a]; return string.Format("`{0}`", a); } }
        #endregion included Libs

        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}