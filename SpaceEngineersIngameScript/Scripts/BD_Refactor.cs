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

        public void Main(string argument)
        {
            BMyEnvironment Environment = bootstrap(BaconArgs.parse(argument));
            Environment.Debug.autoscroll = false;
            Environment.Debug.clearPanels();
            Environment.Debug.newScope("Main");
            BMyInterpreter Interpreter = new BMyInterpreter(Environment);
            BMyTransaction<BMyEnvironment> EnvironmentTransaction = new BMyTransaction<BMyEnvironment>(Environment);
            string[] tags = (Environment.GlobalArgs.getArguments().Count > 0) ? Environment.GlobalArgs.getArguments().ToArray() : new string[] {"[BaconDraw]"};
            Environment.Debug.Debug("Tag(s): {0}", string.Join(",", tags));
            foreach (string tag in tags)
            {
                List<IMyTextPanel> Panels = new List<IMyTextPanel>();
                GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(Panels, (P=>P.CustomName.Contains(tag) && (P.CubeGrid.Equals(Me.CubeGrid) || P.CustomName.Contains("[BaconDrawIgnoreGrid]"))));
                Environment.Debug.Debug("Progressing tag \"{0}\" found {1} Panel(s)", tag, Panels.Count);
                foreach (IMyTextPanel Panel in Panels)
                {
                    Environment.Debug.Debug("Progressing Panel \"{0}\"", Panel.CustomName);
                    Environment.DrawPlugins.TryRequirePlugins(BaconArgs.parse(Panel.GetPrivateTitle()));
                    Environment.Debug.Debug("Loaded plugins({0}/{1}): {2}", Environment.DrawPlugins.Count, Environment.DrawPlugins.AvailabelPlugins.Count,string.Join(",", Environment.DrawPlugins.getLoadedPlugins(true)));
                    BMyCanvas canvas = new BMyCanvas(100,100,Environment);
                    Interpreter.ParseScript(Panel.GetPrivateText(), ref canvas);
                    Panel.WritePublicText(canvas.ToString());
                    Environment = EnvironmentTransaction.Revert();
                }
            }
            Environment.Debug.Debug(" - END - ");
            Environment.Debug.leaveScope();
        }

        #region bootstrap
        BMyEnvironment bootstrap(BaconArgs Args)
        {
            BMyEnvironment Env = new BMyEnvironment(this, Args, getDebugger(Args));
            Env.Debug.newScope("bootstrap");
            #region load plugins
            Env.DrawPlugins.TryRequirePlugins("BaconDraw");
            Env.DrawPlugins.TryRequirePlugins(Env.GlobalArgs);
            #endregion load plugins
            Env.Debug.leaveScope();
            return Env;
        }

        BaconDebug getDebugger(BaconArgs Args)
        {
            int verbosity = 0;
            switch (Args.getFlag('v'))
            {
                case BaconDebug.INFO:
                case BaconDebug.WARN:
                case BaconDebug.ERROR:
                    verbosity = Args.getFlag('v');
                    break;
                case BaconDebug.DEBUG:
                    if(Args.getOption("debug").Count > 0)
                    {
                        verbosity = BaconDebug.DEBUG;
                    }
                    break;
            }
            string tag = (Args.getOption("debug-screen").Count > 0) ? Args.getOption("debug-screen")[0] : "[BaconDraw_DEBUG]";
            return new BaconDebug(tag, GridTerminalSystem, this, verbosity);
        }
        #endregion bootstrap

        #region ENVIRONMENT
        class BMyEnvironment
        {
            public readonly BMyPluginChainloader DrawPlugins;
            public readonly Program Global;
            public readonly BaconArgs GlobalArgs;
            public readonly BaconDebug Debug;
            public readonly Dictionary<string, BMyFont> Fonts;
            public readonly BMyColor Color;

            public BMyEnvironment(Program Global, BaconArgs GlobalArgs, BaconDebug Debug)
            {
                Debug.newScope("BMyEnvironment.BMyEnvironment");
                this.Global = Global;
                this.GlobalArgs = GlobalArgs;
                this.Debug = Debug;
                this.DrawPlugins = new BMyPluginChainloader(this);
                this.Fonts = new Dictionary<string, BMyFont>();
                this.Color = new BMyColor(this);
                Debug.Debug("Envionment Initialized");
                Debug.leaveScope();
            }

            public bool TryAddFont(BMyFont font)
            {
                Debug.newScope("BMyEnvironment.TryAddFont");
                if (Fonts.ContainsKey(font.Name))
                {
                    Debug.Debug("there is already a font named \"{0}\"", font.Name);
                }
                Fonts.Add(font.Name, font);
                Debug.Debug("add font \"{0}\"", font.Name);
                Debug.leaveScope();
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
                Environment.Debug.newScope("BMyFont.getGlyph");
                if (ContainsKey(glyph))
                {
                    Environment.Debug.Debug("found char '{0}' in \"{1}\"", glyph, Name);
                    Environment.Debug.leaveScope();
                    return this[glyph];
                }
                if (Environment.Fonts.ContainsKey(Extends))
                {
                    Environment.Debug.Debug("look up in parent font \"{1}\"", Extends);
                    Environment.Debug.leaveScope();
                    return Environment.Fonts[Extends].getGlyph(glyph);
                }
                Environment.Debug.Debug("char '{0}' not found in \"{1}\" or one of it's parents", glyph, Name);
                Environment.Debug.leaveScope();
                return new string[] {};
            }
        }
        #endregion FONT

        #region INTERPRETER
        class BMyInterpreter
        {
            public readonly BMyEnvironment Environment;
            
            public BMyInterpreter(BMyEnvironment Environment)
            {
                this.Environment = Environment;
            }

            public void ParseScript(string Script, ref BMyCanvas canvas)
            {
                Environment.Debug.newScope("BMyInterpreter.ParseScript");
                string[] Code = Script.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string commandLine in Code)
                {
                    BaconArgs Args = BaconArgs.parse(commandLine);
                    if(Args.getArguments().Count > 0)
                    {
                        if(Environment.DrawPlugins.TryInterpret(Args, ref canvas))
                        {
                            Environment.Debug.Debug("success parsing: {0}", commandLine);
                        } else
                        {
                            Environment.Debug.Debug("failed parsing: {0}", commandLine);
                        }
                    } else
                    {
                        Environment.Debug.Debug("skip line, no arguments [\"{0}\"]", commandLine);
                    }
                }
                Environment.Debug.leaveScope();
            }
        }
        #endregion INTERPRETER
        
        #region CANVAS
        class BMyCanvas
        {
            private char[][] pixels; //[Y][X]
            public char color = '1';
            public char background;
            private Point Position;
            public readonly BMyEnvironment Environment;

            public BMyCanvas(int width, int height, BMyEnvironment Environment, string content) : this(width, height, Environment)
            {
                Environment.Debug.newScope("BMyCanvas.Canvas");
                string[] data = content.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                for(int i=0;i<data.Length && i < pixels.Length; i++)
                {
                    pixels[i] = (data[i] + (new string('0', width))).Substring(0,width).ToCharArray();
                }
                Environment.Debug.Debug("Filled canvas with exisiting content (lines:{0}; overall length:{1})", data.Length, content.Length);
                Environment.Debug.leaveScope();
            }

            public BMyCanvas(int width, int height, BMyEnvironment Environment)
            {
                Environment.Debug.newScope("BMyCanvas.Canvas");
                this.Environment = Environment;
                setPosition(0, 0);
                Clear(width,height);
                Environment.Debug.Debug("Created new Canvas with dimensions(BxH) {0}x{1}", width, height);
                Environment.Debug.leaveScope();
            }

            public bool TryParseCoords(string value, out Point coords)
            {
                Environment.Debug.newScope("BMyCanvas.TryParseCoords");
                string[] raw = value.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
                int x = 0;
                int y = 0;
                if(raw.Length == 2 && int.TryParse(raw[0], out x) && int.TryParse(raw[1], out y))
                {
                    coords = new Point(x,y);
                    Environment.Debug.Debug("Coordinates parsed from \"{0}\" => resulting in {1},{2}", value, coords.X, coords.Y);
                    Environment.Debug.leaveScope();
                    return true;
                }
                coords = new Point(0,0);

                Environment.Debug.Debug("Can't parse coordinates from \"{0}\" => resulting in {1},{2}", value, coords.X, coords.Y);
                Environment.Debug.leaveScope();
                return false;
            }

            void Clear(int width, int height)
            {
                Environment.Debug.newScope("BMyCanvas.Clear");
                width = Math.Max(width, 1);
                height = Math.Max(height, 1);
                pixels = new char[height][];
                for (int y = 0; y < pixels.Length; y++)
                {
                    pixels[y] = (new String('0', width)).ToCharArray();
                }
                Environment.Debug.leaveScope();
            }
            
            public void setPixel(int x, int y)
            {
                Environment.Debug.newScope("BMyCanvas.setPixel");
                if (0 <= x && x < pixels[0].Length && 0 <= y && y < pixels.Length)
                {
                    pixels[y][x] = color;
                    Environment.Debug.Debug("set pixel {1},{2} to {0}", color, x, y);
                } else
                {
                    Environment.Debug.Debug("Point({1},{2}) out of range(x[0,{3}[,y[0,{4}[) => can't assign color({0}", color, x, y, pixels[0].Length, pixels.Length);
                }
                Environment.Debug.leaveScope();
            }

            public Point getPosition()
            {
                return Position;
            }

            public void setPosition(int x, int y)
            {
                Environment.Debug.newScope("BMyCanvas.setPosition");
                Environment.Debug.Debug("update position to {0},{1}", x, y);
                Position = new Point(x,y);
                Environment.Debug.leaveScope();
            }

            public string ToStringRaw()
            {
                Environment.Debug.newScope("BMyCanvas.ToStringRaw");
                List<string> buffer = new List<string>();
                foreach(char[] line in pixels)
                {
                    buffer.Add(line.ToString());
                }
                Environment.Debug.Debug("generate image");
                Environment.Debug.leaveScope();
                return string.Join("\n", buffer.ToArray());
            }

            public override string ToString()
            {
                Environment.Debug.newScope("BMyCanvas.ToString");
                if(Environment.GlobalArgs.getOption("rawOutput").Count > 0)
                {
                    Environment.Debug.leaveScope();
                    return ToStringRaw();
                } else
                {
                    Environment.Debug.Debug("generate image");
                    Environment.Debug.leaveScope();
                    return Environment.Color.ConvertFromRawImage(this);
                }                
            }
        }
        #endregion CANVAS

        #region Transactions
        class BMyTransaction<T>
        {
            private T buffer;

            public BMyTransaction(T value)
            {
                buffer = value;
            }

            public T Revert()
            {
                return buffer;
            }
        }
        #endregion Transactions

        #region COLOR
        class BMyColor
        {
            public readonly BMyEnvironment Environment;
            protected Dictionary<char, char> map = new Dictionary<char, char>() {{'g','\uE001'},{'b','\uE002'},{'r','\uE003'},{'y','\uE004'},{'w','\uE006'},{'l','\uE00E'},{'d','\uE00F'}};

            public const char PLACEHOLDER_BG = '0';

            public BMyColor(BMyEnvironment Environment)
            {
                this.Environment = Environment;
            }

            public string ConvertFromRawImage(BMyCanvas canvas)
            {
                Environment.Debug.newScope("BMyColor.ConvertFromRawImage");
                StringBuilder image = new StringBuilder((new System.Text.RegularExpressions.Regex(@"[^gbrywld\n]", System.Text.RegularExpressions.RegexOptions.IgnoreCase)).Replace(canvas.ToStringRaw().ToLowerInvariant(), PLACEHOLDER_BG.ToString()));
                image = image.Replace(PLACEHOLDER_BG, canvas.background);
                foreach (KeyValuePair<char, char> color in map)
                {
                    image = image.Replace(color.Key, color.Value);
                }
                Environment.Debug.leaveScope();
                return image.ToString();
            }

            public char getColorCode(string args)
            {
                Environment.Debug.newScope("BMyColor.getColorCode");
                string raw = args.Trim();
                char value = '1';
                if ((new System.Text.RegularExpressions.Regex(@"^(\\u[0-9a-f]{4})|(U\+[0-9a-f]{4})$").IsMatch(raw)))
                {
                    int ord;
                    if (int.TryParse(raw.Substring(2), out ord))
                    {
                        value = (char)ord;
                        Environment.Debug.Debug("Found unicode Character: {0} => '{1}'", raw, value);
                    }
                }
                else if (raw.Length > 0)
                {
                    value = raw[0];
                    Environment.Debug.Debug("Matching Color: {0} => '{1}'", raw, value);
                }
                else
                {
                    Environment.Debug.Debug("No matching Color => using default: {0} => '{1}'", raw, value);
                }
                Environment.Debug.leaveScope();
                return value;
            }
        }
        #endregion COLOR

        #region Pluginsystem
        class BMyPluginChainloader : Dictionary<string, List<BMyDrawPlugin>>
        {
            public readonly BMyEnvironment Environment;
            public readonly Dictionary<string, List<BMyDrawPlugin>> AvailabelPlugins = new Dictionary<string, List<BMyDrawPlugin>>();


            public BMyPluginChainloader(BMyEnvironment Environment)
            {
                this.Environment = Environment;
                bootstrap();
            }

            private void bootstrap()
            {
                Environment.Debug.newScope("BMyPluginChainloader.bootstrap");
                #region DrawPlugin "BaconDraw"
                Environment.Debug.Debug("Initializing DrawPlugin \"BaconDraw\"");
                AvailabelPlugins.Add("BaconDraw", new List<BMyDrawPlugin>());
                AvailabelPlugins["BaconDraw"].Add(new BMyDrawPlugin_BaconDraw_Circle(Environment));
                AvailabelPlugins["BaconDraw"].Add(new BMyDrawPlugin_BaconDraw_Font(Environment));
                AvailabelPlugins["BaconDraw"].Add(new BMyDrawPlugin_BaconDraw_LineTo(Environment));
                AvailabelPlugins["BaconDraw"].Add(new BMyDrawPlugin_BaconDraw_Polygon(Environment));
                AvailabelPlugins["BaconDraw"].Add(new BMyDrawPlugin_BaconDraw_Rectangle(Environment));
                AvailabelPlugins["BaconDraw"].Add(new BMyDrawPlugin_BaconDraw_Text(Environment));
                AvailabelPlugins["BaconDraw"].Add(new BMyDrawPlugin_BaconDraw_Background(Environment));
                AvailabelPlugins["BaconDraw"].Add(new BMyDrawPlugin_BaconDraw_Color(Environment));
                #endregion DrawPlugin "BaconDraw"

                Environment.Debug.leaveScope();
            }

            public string[] getLoadedPlugins(bool showCommands = false)
            {
                List<string> names = new List<string>();
                foreach (KeyValuePair<string, List<BMyDrawPlugin>> buffer in this)
                {
                    if (showCommands)
                    {
                        List<string> commands = new List<string>();
                        foreach(BMyDrawPlugin Plugin in buffer.Value)
                        {
                            commands.Add(Plugin.Command);
                        }
                        names.Add(string.Format("{0}[{1}]", buffer.Key, string.Join("|", commands.ToArray())));
                    } else
                    {
                        names.Add(buffer.Key);
                    }
                }
                return names.ToArray();
            }

            public bool TryRequirePlugins(BaconArgs Args)
            {
                return TryRequirePlugins(getRequiredPluginNames(Args));
            }

            public bool TryRequirePlugins(params string[] Names)
            {
                Environment.Debug.newScope("BMyPluginChainloader.TryRequirePlugins");
                bool required = true;
                foreach (string Name in Names)
                {
                    if (!AvailabelPlugins.ContainsKey(Name))
                    {
                        Environment.Debug.Debug("Plugin \"{0}\" not found", Name);
                        Environment.Debug.leaveScope();
                        return false;
                    }
                    bool success = false;
                    foreach (BMyDrawPlugin Plugin in AvailabelPlugins[Name])
                    {
                        if (!TryAddPlugin(Plugin))
                        {
                            success = success || false;
                            Environment.Debug.Debug("Error loading plugin \"{0}({1})\"", Plugin.Name, Plugin.Command);
                        }
                    }
                    if (!success)
                    {
                        Environment.Debug.Debug("Unable to load anything of \"{0}\" plugin", Name);
                    }
                    required = required && success;
                }
                Environment.Debug.leaveScope();
                return required;
            }

            string[] getRequiredPluginNames(BaconArgs Args)
            {
                Environment.Debug.newScope("BMyPluginChainloader.getRequiredPluginNames");
                List<string> plugins = new List<string>();
                foreach (string names in Environment.GlobalArgs.getOption("plugin"))
                {
                    plugins.AddRange(names.Split(','));
                }
                Environment.Debug.leaveScope();
                return plugins.ToArray();
            }

            public bool TryAddPlugin(BMyDrawPlugin Plugin)
            {
                Environment.Debug.newScope("BMyPluginChainloader.AddPlugin");
                string command = Plugin.Command.ToLowerInvariant();
                if (!ContainsKey(command))
                {
                    Add(command, new List<BMyDrawPlugin>());
                }
                if (!this[command].Contains(Plugin))
                {
                    this[command].Insert(0, Plugin);
                    Environment.Debug.Debug("Load Plugin \"{1}\" for \"{0}\"", command, Plugin.Name);
                    Environment.Debug.leaveScope();
                    return true;
                }
                Environment.Debug.Debug("Unable load Plugin \"{1}\" for \"{0}\" => {2}", command, Plugin.Name, (this[command].Contains(Plugin)?"already loaded":"unknown reason"));
                Environment.Debug.leaveScope();
                return false;
            }

            public bool TryInterpret(BaconArgs Args, ref BMyCanvas canvas)
            {
                Environment.Debug.newScope("BMyPluginChainloader.TryInterpret");
                if(Args.getArguments().Count == 0)
                {
                    Environment.Debug.Debug("Skip line (no arguments)");
                    Environment.Debug.leaveScope();
                    return false;
                }

                if (!hasPlugin(Args.getArguments()[0]))
                {
                    Environment.Debug.leaveScope();
                    return false;
                }
                BMyTransaction<BMyCanvas> Buffer = new BMyTransaction<BMyCanvas>(canvas);

                foreach(BMyDrawPlugin Plugin in this[Args.getArguments()[0]])
                {
                    Environment.Debug.newScope(string.Format("BMyPluginChainloader[{0}/{1}].isValid", Plugin.Name, Plugin.Command));
                    bool isValid = Plugin.isValid(Args);
                    Environment.Debug.leaveScope();
                    if (isValid)
                    {
                        Environment.Debug.newScope(string.Format("BMyPluginChainloader[{0}/{1}].TryInterpret", Plugin.Name, Plugin.Command));
                        if (Plugin.TryInterpret(Args, ref canvas))
                        {
                            break;
                        }
                    }
                    canvas = Buffer.Revert();
                }
                Environment.Debug.leaveScope();
                return !Buffer.Revert().Equals(canvas);
            }

            public bool hasPlugin(string command)
            {
                Environment.Debug.newScope("BMyPluginChainloader.hasPlugin");
                bool contains = ContainsKey(command.ToLowerInvariant());
                Environment.Debug.Debug("{0}Plugin for \"{1}\"", contains?"":"no ", command);
                Environment.Debug.leaveScope();
                return contains;
            }
        }

        abstract class BMyDrawPlugin
        {
            abstract public string Name { get; }
            abstract public string Command { get; }
            abstract public bool isValid(BaconArgs Args);
            abstract public bool TryInterpret(BaconArgs Args, ref BMyCanvas canvas);
            public readonly BMyEnvironment Environment;

            public BMyDrawPlugin(BMyEnvironment Environment)
            {
                this.Environment = Environment;
            }
        }
        #endregion Pluginsystem

        #region included Plugins
        class BMyDrawPlugin_BaconDraw_LineTo : BMyDrawPlugin
        {
            public override string Name { get { return "BaconDraw"; } }
            public override string Command { get { return "lineto"; } }

            public BMyDrawPlugin_BaconDraw_LineTo(BMyEnvironment Environment) : base(Environment){}

            public override bool TryInterpret(BaconArgs Args, ref BMyCanvas canvas)
            {
                Point dest;
                if (Args.getArguments().Count < 2 || !canvas.TryParseCoords(Args.getArguments()[1], out dest))
                {
                    Environment.Debug.Debug("no valid arguments => can't draw {0}", Command);
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
                Environment.Debug.Debug("draw {2} to {0},{1}", _x, _y, Command);
                return true;
            }

            public override bool isValid(BaconArgs Args)
            {
                return Args.getArguments().Count == 2 && (new System.Text.RegularExpressions.Regex(@"\d+,\d+")).IsMatch(Args.getArguments()[1]);
            }
        }
        class BMyDrawPlugin_BaconDraw_Polygon : BMyDrawPlugin
        {
            System.Text.RegularExpressions.Regex MatchRgx = new System.Text.RegularExpressions.Regex(@"\d+,\d+");

            public override string Name { get { return "BaconDraw"; } }
            public override string Command { get { return "polygon"; } }

            public BMyDrawPlugin_BaconDraw_Polygon(BMyEnvironment Environment) : base(Environment){ }

            public override bool TryInterpret(BaconArgs Args, ref BMyCanvas canvas)
            {
                if (!Environment.DrawPlugins.hasPlugin("lineto"))
                {
                    Environment.Debug.Debug("no plugin for \"lineto\" => can't draw {0}", Command);
                    return false;
                }
                for(int i = 1; i < Args.getArguments().Count; i++)
                {
                    BaconArgs LineToArgs = new BaconArgs();
                    LineToArgs.add("lineto");
                    LineToArgs.add(Args.getArguments()[i]);
                    if(!Environment.DrawPlugins.TryInterpret(LineToArgs, ref canvas))
                    {
                        return false;
                    }
                }
                Environment.Debug.Debug("drawed {0}", Command);
                return true;
            }

            public override bool isValid(BaconArgs Args)
            {
                if(Args.getArguments().Count < 2)
                {
                    Environment.Debug.Debug("not enough arguments (must be at least 2)");
                    return false;
                }
                bool valid = true;

                for(int i = 1; i < Args.getArguments().Count; i++)
                {
                    if (!MatchRgx.IsMatch(Args.getArguments()[i]))
                    {
                        Environment.Debug.Debug("argument[{1}]({0}) invalid. Must be `number,number`", Args.getArguments()[i], i);
                        valid = false;
                    }
                }
                return valid;
            }
        }
        class BMyDrawPlugin_BaconDraw_Rectangle : BMyDrawPlugin
        {
            public override string Name { get { return "BaconDraw"; } }
            public override string Command { get { return "rect"; } }

            public BMyDrawPlugin_BaconDraw_Rectangle(BMyEnvironment Environment) : base(Environment){ }

            public override bool TryInterpret(BaconArgs Args, ref BMyCanvas canvas)
            {
                if (!Environment.DrawPlugins.hasPlugin("polygon"))
                {
                    Environment.Debug.Debug("no plugin for \"polygon\" => can't draw {0}", Command);
                    return false;
                }
                Point dest;
                if(!canvas.TryParseCoords(Args.getArguments()[1], out dest)){
                    Environment.Debug.Debug("invalid coordinates {0} => can't draw {1}", Args.getArguments()[1], Command);
                    return false;
                }
                BaconArgs PolygonArgs = new BaconArgs();
                PolygonArgs.add("polygon");
                PolygonArgs.add(string.Format("{0},{1}", dest.X, canvas.getPosition().Y));
                PolygonArgs.add(string.Format("{0},{1}", dest.X, dest.Y));
                PolygonArgs.add(string.Format("{0},{1}", canvas.getPosition().X, dest.Y));
                PolygonArgs.add(string.Format("{0},{1}", canvas.getPosition().X, canvas.getPosition().Y));
                if(!Environment.DrawPlugins.TryInterpret(PolygonArgs, ref canvas))
                {
                    Environment.Debug.Debug("failed drawing {0}", Command);
                    return false;
                }
                canvas.setPosition(dest.X,dest.Y);
                Environment.Debug.Debug("drawed {0} to {1},{2}", Command, dest.X, dest.Y);
                return true;
            }

            public override bool isValid(BaconArgs Args)
            {
                return Args.getArguments().Count == 2 && (new System.Text.RegularExpressions.Regex(@"\d+,\d+")).IsMatch(Args.getArguments()[1]);
            }
        }
        class BMyDrawPlugin_BaconDraw_Circle : BMyDrawPlugin
        {
            public override string Name { get { return "BaconDraw"; } }
            public override string Command { get { return "cirlce"; } }

            public BMyDrawPlugin_BaconDraw_Circle(BMyEnvironment Environment) : base(Environment){ }

            public override bool TryInterpret(BaconArgs Args, ref BMyCanvas canvas)
            {
                int Radius;
                if(!int.TryParse(Args.getArguments()[1], out Radius))
                {
                    Environment.Debug.Debug("Invalid argument \"{0}\" must be a number", Args.getArguments()[1]);
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
                Environment.Debug.Debug("drawed {0}", Command);
                return true;
            }

            public override bool isValid(BaconArgs Args)
            {
                return Args.getArguments().Count == 2 && (new System.Text.RegularExpressions.Regex(@"\d+")).IsMatch(Args.getArguments()[1]);
            }
        }
        class BMyDrawPlugin_BaconDraw_Font : BMyDrawPlugin
        {
            public override string Command { get { return "font"; }}
            public override string Name { get { return "BaconDraw"; } }
            private System.Text.RegularExpressions.Regex GlyphRgx = new System.Text.RegularExpressions.Regex(@"'.'\S+");
            private System.Text.RegularExpressions.Regex SizeRgx = new System.Text.RegularExpressions.Regex(@"\d+x\d+");
            private System.Text.RegularExpressions.Regex NameRgx = new System.Text.RegularExpressions.Regex(@"[^\s:]+(:[^\s:]+)?");
            private string splitChunkPatternFormat = @"(\S{{{0}}})";

            const int indexChars = 3;
            const int indexSize = 2;
            const int indexName = 1;

            const int indexGlyph = 1;
            const int indexGlyphData = 3;

            public BMyDrawPlugin_BaconDraw_Font(BMyEnvironment Environment) : base(Environment){}

            public override bool isValid(BaconArgs Args)
            {
                if(Args.getArguments().Count < 4)
                {
                    Environment.Debug.Debug("not enough arguments");
                    return false;
                }
                if (!NameRgx.IsMatch(Args.getArguments()[indexName]))
                {
                    Environment.Debug.Debug("invalid argument for fontname `{0}`", Args.getArguments()[indexName]);
                    return false;
                }

                if(!SizeRgx.IsMatch(Args.getArguments()[indexSize]))
                {
                    Environment.Debug.Debug("invalid argument for fontsize `{0}`", Args.getArguments()[indexSize]);
                    return false;
                }
                int w;
                int h;
                string[] size = Args.getArguments()[indexSize].Split(',');
                if(!(size.Length == 2) || !int.TryParse(size[0], out w) || !int.TryParse(size[1], out h))
                {
                    Environment.Debug.Debug("invalid argument for fontsize `{0}`", Args.getArguments()[indexSize]);
                }
                
                for(int i = indexChars; i < Args.getArguments().Count; i++)
                {
                    if (!GlyphRgx.IsMatch(Args.getArguments()[i]))
                    {
                        Environment.Debug.Debug("Invalid character definition `{0}` at argument #{1}", Args.getArguments()[i], i+1);
                        return false;
                    }
                }
                return true;
            }

            public override bool TryInterpret(BaconArgs Args, ref BMyCanvas canvas)
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
                for(int i = indexChars; i < Args.getArguments().Count; i++)
                {
                    string argument = Args.getArguments()[i];
                    char glyph = argument[indexGlyph];
                    string[] data = getData(argument.Substring(indexGlyphData), w, h);
                    font.Add(glyph, data);
                }
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
            public override string Command { get { return "text"; } }
            public override string Name { get { return "BaconDraw"; } }

            const int indexName = 1;
            
            public BMyDrawPlugin_BaconDraw_Text(BMyEnvironment Environment) : base(Environment){}

            public override bool isValid(BaconArgs Args)
            {
                Environment.Debug.Debug("NotImplementedException");
                return false;
            }

            public override bool TryInterpret(BaconArgs Args, ref BMyCanvas canvas)
            {
                Environment.Debug.Debug("NotImplementedException");
                return false;
            }
        }
        class BMyDrawPlugin_BaconDraw_Background : BMyDrawPlugin
        {
            public override string Command { get { return "background"; } }
            public override string Name { get { return "BaconDraw"; } }

            public BMyDrawPlugin_BaconDraw_Background(BMyEnvironment Environment) : base(Environment) { }

            public override bool isValid(BaconArgs Args)
            {
                return (Args.getArguments().Count == 2);
            }

            public override bool TryInterpret(BaconArgs Args, ref BMyCanvas canvas)
            {
                canvas.background = Environment.Color.getColorCode(Args.getArguments()[1]);
                return true;
            }
        }
        class BMyDrawPlugin_BaconDraw_Color : BMyDrawPlugin
        {
            public override string Command { get { return "color"; } }
            public override string Name { get { return "BaconDraw"; } }

            public BMyDrawPlugin_BaconDraw_Color(BMyEnvironment Environment) : base(Environment) { }

            public override bool isValid(BaconArgs Args)
            {
                return (Args.getArguments().Count == 2);
            }

            public override bool TryInterpret(BaconArgs Args, ref BMyCanvas canvas)
            {
                canvas.color = Environment.Color.getColorCode(Args.getArguments()[1]);
                return true;
            }
        }
        #endregion included Plugins

        #region 3rd party Plugins

        #endregion 3rd party Plugins

        #region included Libs
        public class BaconArgs { static public BaconArgs parse(string a) { return (new Parser()).parseArgs(a); } static public string Escape(string a) { return a.Replace(@"\", @"\\").Replace(@" ", @"\ ").Replace(@"""", @"\"""); } static public string UnEscape(string a) { return a.Replace(@"\""", @"""").Replace(@"\ ", @" ").Replace(@"\\", @"\"); } public class Parser { static Dictionary<string, BaconArgs> h = new Dictionary<string, BaconArgs>(); public BaconArgs parseArgs(string a) { if (!h.ContainsKey(a)) { var b = new BaconArgs(); var c = false; var d = false; var e = new StringBuilder(); for (int f = 0; f < a.Length; f++) { var g = a[f]; if (c) { e.Append(g); c = false; } else if (g.Equals('\\')) c = true; else if (d && !g.Equals('"')) e.Append(g); else if (g.Equals('"')) d = !d; else if (g.Equals(' ')) if (e.ToString().Equals("--")) { b.add(a.Substring(f).TrimStart()); e.Clear(); break; } else { b.add(e.ToString()); e.Clear(); } else e.Append(g); } if (e.Length > 0) b.add(e.ToString()); h.Add(a, b); } return h[a]; } } protected Dictionary<char, int> h = new Dictionary<char, int>(); protected List<string> i = new List<string>(); protected Dictionary<string, List<string>> j = new Dictionary<string, List<string>>(); public List<string> getArguments() { return i; } public int getFlag(char a) { return h.ContainsKey(a) ? h[a] : 0; } public List<string> getOption(string a) { return j.ContainsKey(a) ? j[a] : new List<string>(); } public void add(string a) { if (!a.StartsWith("-")) i.Add(a); else if (a.StartsWith("--")) { KeyValuePair<string, string> b = k(a); var c = b.Key.Substring(2); if (!j.ContainsKey(c)) j.Add(c, new List<string>()); j[c].Add(b.Value); } else { var b = a.Substring(1); for (int d = 0; d < b.Length; d++) if (this.h.ContainsKey(b[d])) { this.h[b[d]]++; } else { this.h.Add(b[d], 1); } } } KeyValuePair<string, string> k(string a) { string[] b = a.Split(new char[] { '=' }, 2); return new KeyValuePair<string, string>(b[0], (b.Length > 1) ? b[1] : null); } string l(string a) { return (a != null) ? "\"" + a.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" : @"null"; } }
        public class BaconDebug { public const int INFO = 3; public const int WARN = 2; public const int ERROR = 1; public const int DEBUG = 4; List<IMyTextPanel> h = new List<IMyTextPanel>(); MyGridProgram i; List<string> j = new List<string>(); int k = 0; bool l = true; public int remainingInstructions { get { return i.Runtime.MaxInstructionCount - i.Runtime.CurrentInstructionCount; } } public bool autoscroll { get { return l; } set { l = value; } } public void clearPanels() { for (int a = 0; a < h.Count; a++) h[a].WritePublicText(""); } public BaconDebug(string a, IMyGridTerminalSystem b, MyGridProgram c, int d) { this.k = d; var e = new List<IMyTerminalBlock>(); b.GetBlocksOfType<IMyTextPanel>(e, ((IMyTerminalBlock f) => f.CustomName.Contains(a) && f.CubeGrid.Equals(c.Me.CubeGrid))); h = e.ConvertAll<IMyTextPanel>(f => f as IMyTextPanel); this.i = c; newScope("BaconDebug"); } public int getVerbosity() { return k; } public MyGridProgram getGridProgram() { return this.i; } public void newScope(string a) { j.Add(a); } public void leaveScope() { if (j.Count > 1) j.RemoveAt(j.Count - 1); } public string getSender() { return j[j.Count - 1]; } public void Info(string a, params object[] b) { Info(string.Format(a, b)); } public void Warn(string a, params object[] b) { Warn(string.Format(a, b)); } public void Error(string a, params object[] b) { Error(string.Format(a, b)); } public void Debug(string a, params object[] b) { Debug(string.Format(a, b)); } public void Info(string a) { add(a, INFO); } public void Warn(string a) { add(a, WARN); } public void Error(string a) { add(a, ERROR); } public void Debug(string a) { add(a, DEBUG); } public void add(string a, int b, params object[] c) { add(string.Format(a, c), b); } public void add(string a, int b) { if (b <= this.k) { var c = n(a); if (b == ERROR) i.Echo(c); for (int d = 0; d < h.Count; d++) if (autoscroll) { List<string> e = new List<string>(); e.AddRange(h[d].GetPublicText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)); StringBuilder f = new StringBuilder(); e.Add(c); if (!h[d].GetPrivateTitle().ToLower().Equals("nolinelimit")) { int g = m(h[d]); if (e.Count > g) { e.RemoveRange(0, e.Count - g); } } h[d].WritePublicText(string.Join("\n", e)); } else { h[d].WritePublicText(c + '\n', true); } } } int m(IMyTextPanel a) { float b = a.GetValueFloat("FontSize"); if (b == 0.0f) b = 0.01f; return Convert.ToInt32(Math.Ceiling(17.0f / b)); } string n(string a) { var b = new StringBuilder(); b.Append("[" + DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToShortTimeString() + "." + DateTime.Now.Millisecond.ToString().TrimStart('0') + "]"); b.Append("[" + getSender() + "]"); b.Append("[IC " + i.Runtime.CurrentInstructionCount + "/" + i.Runtime.MaxInstructionCount + "]"); b.Append(" " + a); return b.ToString(); } }
        #endregion included Libs


        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}