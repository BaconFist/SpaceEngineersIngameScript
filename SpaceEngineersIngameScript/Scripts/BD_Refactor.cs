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
            Environment.Log.autoscroll = false;
            Environment.Log.clearPanels();
            Environment.Log.newScope("Main");
            BMyInterpreter Interpreter = new BMyInterpreter(Environment);
            BMyTransaction<BMyEnvironment> EnvironmentTransaction = new BMyTransaction<BMyEnvironment>(Environment);
            string[] tags = (Environment.GlobalArgs.getArguments().Count > 0) ? Environment.GlobalArgs.getArguments().ToArray() : new string[] {"[BaconDraw]"};
            Environment.Log.Trace("Tag(s): {0}", string.Join(",", tags));
            foreach (string tag in tags)
            {
                List<IMyTextPanel> Panels = new List<IMyTextPanel>();
                GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(Panels, (P=>P.CustomName.Contains(tag) && (P.CubeGrid.Equals(Me.CubeGrid) || P.CustomName.Contains("[BaconDrawIgnoreGrid]"))));
                Environment.Log.Trace("Progressing tag \"{0}\" found {1} Panel(s)", tag, Panels.Count);
                foreach (IMyTextPanel Panel in Panels)
                {
                    Environment.Log.Trace("Progressing Panel \"{0}\"", Panel.CustomName);
                    Environment.DrawPlugins.TryRequirePlugins(BaconArgs.parse(Panel.GetPrivateTitle()));
                    string[] plugins = Environment.DrawPlugins.getLoadedPlugins();
                    Environment.Log.Trace("Loaded plugins({0}/{1}): [{2}]", plugins.Length, Environment.DrawPlugins.AvailabelPlugins.Count, string.Join("],[", plugins));
                    BMyCanvas canvas = new BMyCanvas(100,100,Environment);
                    Interpreter.ParseScript(Panel.GetPrivateText(), ref canvas);
                    Panel.WritePublicText(canvas.ToString());
                    Environment = EnvironmentTransaction.Revert();
                }
            }
            Environment.Log.Trace(" - END - ");
            Environment.Log.leaveScope();
        }

        #region bootstrap
        BMyEnvironment bootstrap(BaconArgs Args)
        {
            BMyEnvironment Env = new BMyEnvironment(this, Args, getDebugger(Args));
            Env.Log.newScope("bootstrap");
            #region load plugins
            Env.DrawPlugins.TryRequirePlugins("BaconDraw");
            Env.DrawPlugins.TryRequirePlugins(Env.GlobalArgs);
            #endregion load plugins
            Env.Log.leaveScope();
            return Env;
        }

        BaconDebug getDebugger(BaconArgs Args)
        {
            int verbosity = 0;
            switch (Args.getFlag('v'))
            {
                case BaconDebug.OFF:
                case BaconDebug.INFO:
                case BaconDebug.WARN:
                case BaconDebug.ERROR:
                case BaconDebug.FATAL:
                    verbosity = Args.getFlag('v');
                    break;
                case BaconDebug.DEBUG:
                    if(Args.getOption("debug").Count > 0)
                    {
                        verbosity = BaconDebug.DEBUG;
                    }
                    break;
                case BaconDebug.TRACE:
                    if (Args.getOption("debug-trace").Count > 0)
                    {
                        verbosity = BaconDebug.TRACE;
                    }
                    break;
            }
            string tag = (Args.getOption("debug-screen").Count > 0) ? Args.getOption("debug-screen")[0] : "[BaconDraw_DEBUG]";
            return new BaconDebug(tag, GridTerminalSystem, this, verbosity, "BaconDraw");
        }
        #endregion bootstrap

        #region ENVIRONMENT
        class BMyEnvironment
        {
            public readonly BMyDrawPluginHandler DrawPlugins;
            public readonly Program Global;
            public readonly BaconArgs GlobalArgs;
            public readonly BaconDebug Log;
            public readonly Dictionary<string, BMyFont> Fonts;
            public readonly BMyColor Color;

            public BMyEnvironment(Program Global, BaconArgs GlobalArgs, BaconDebug Log)
            {
                Log.newScope("BMyEnvironment.BMyEnvironment");
                this.Global = Global;
                this.GlobalArgs = GlobalArgs;
                this.Log = Log;
                this.DrawPlugins = new BMyDrawPluginHandler(this);
                this.Fonts = new Dictionary<string, BMyFont>();
                this.Color = new BMyColor(this);
                Log.Trace("Envionment Initialized");
                Log.leaveScope();
            }

            public bool TryAddFont(BMyFont font)
            {
                Log.newScope("BMyEnvironment.TryAddFont");
                if (Fonts.ContainsKey(font.Name))
                {
                    Log.Trace("there is already a font named \"{0}\"", font.Name);
                }
                Fonts.Add(font.Name, font);
                Log.Trace("add font \"{0}\"", font.Name);
                Log.leaveScope();
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
                Environment.Log.newScope("BMyFont.getGlyph");
                if (ContainsKey(glyph))
                {
                    Environment.Log.Trace("found char '{0}' in \"{1}\"", glyph, Name);
                    Environment.Log.leaveScope();
                    return this[glyph];
                }
                if (Environment.Fonts.ContainsKey(Extends))
                {
                    Environment.Log.Trace("look up in parent font \"{1}\"", Extends);
                    Environment.Log.leaveScope();
                    return Environment.Fonts[Extends].getGlyph(glyph);
                }
                Environment.Log.Trace("char '{0}' not found in \"{1}\" or one of it's parents", glyph, Name);
                Environment.Log.leaveScope();
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
                Environment.Log.newScope("BMyInterpreter.ParseScript");
                string[] Code = Script.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string commandLine in Code)
                {
                    BaconArgs Args = BaconArgs.parse(commandLine);
                    Environment.Log.Trace("Arguments: `{0}`", Args.ToString());
                    if(Args.getArguments().Count > 0)
                    {
                        if(Environment.DrawPlugins.TryInterpret(Args, ref canvas))
                        {
                            Environment.Log.Trace("success parsing: {0}", commandLine);
                        } else
                        {
                            Environment.Log.Trace("failed parsing: {0}", commandLine);
                        }
                    } else
                    {
                        Environment.Log.Trace("skip line, no arguments [\"{0}\"]", commandLine);
                    }
                }
                Environment.Log.leaveScope();
            }
        }
        #endregion INTERPRETER
        
        #region CANVAS
        class BMyCanvas
        {
            private char[][] pixels; //[Y][X]
            public char color = 'l';
            public char background = 'd';
            private Point Position;
            public readonly BMyEnvironment Environment;

            public BMyCanvas(int width, int height, BMyEnvironment Environment, string content) : this(width, height, Environment)
            {
                Environment.Log.newScope("BMyCanvas.Canvas");
                string[] data = content.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                for(int i=0;i<data.Length && i < pixels.Length; i++)
                {
                    pixels[i] = (data[i] + (new string('0', width))).Substring(0,width).ToCharArray();
                }
                Environment.Log.Trace("Filled canvas with exisiting content (lines:{0}; overall length:{1})", data.Length, content.Length);
                Environment.Log.leaveScope();
            }

            public BMyCanvas(int width, int height, BMyEnvironment Environment)
            {
                Environment.Log.newScope("BMyCanvas.Canvas");
                this.Environment = Environment;
                setPosition(0, 0);
                Clear(width,height);
                Environment.Log.Trace("Created new Canvas with dimensions(BxH) {0}x{1}", width, height);
                Environment.Log.leaveScope();
            }

            public bool TryParseCoords(string value, out Point coords)
            {
                Environment.Log.newScope("BMyCanvas.TryParseCoords");
                string[] raw = value.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
                int x = 0;
                int y = 0;
                if(raw.Length == 2 && int.TryParse(raw[0], out x) && int.TryParse(raw[1], out y))
                {
                    coords = new Point(x,y);
                    Environment.Log.Trace("Coordinates parsed from \"{0}\" => resulting in {1},{2}", value, coords.X, coords.Y);
                    Environment.Log.leaveScope();
                    return true;
                }
                coords = new Point(0,0);

                Environment.Log.Trace("Can't parse coordinates from \"{0}\" => resulting in {1},{2}", value, coords.X, coords.Y);
                Environment.Log.leaveScope();
                return false;
            }

            void Clear(int width, int height)
            {
                Environment.Log.newScope("BMyCanvas.Clear");
                width = Math.Max(width, 1);
                height = Math.Max(height, 1);
                pixels = new char[height][];
                for (int y = 0; y < pixels.Length; y++)
                {
                    pixels[y] = (new String('0', width)).ToCharArray();
                }
                Environment.Log.leaveScope();
            }
            
            public void setPixel(int x, int y)
            {
                Environment.Log.newScope("BMyCanvas.setPixel");
                if (0 <= x && x < pixels[0].Length && 0 <= y && y < pixels.Length)
                {
                    pixels[y][x] = color;
                    Environment.Log.Trace("set pixel {1},{2} to {0}", color, x, y);
                } else
                {
                    Environment.Log.Trace("Point({1},{2}) out of range(X:`[0,{3}[` Y:`[0,{4}[`) => can't assign color '{0}'", color, x, y, pixels[0].Length, pixels.Length);
                }
                Environment.Log.leaveScope();
            }

            public Point getPosition()
            {
                return Position;
            }

            public void setPosition(int x, int y)
            {
                Environment.Log.newScope("BMyCanvas.setPosition");
                Environment.Log.Trace("update position to {0},{1}", x, y);
                Position = new Point(x,y);
                Environment.Log.leaveScope();
            }

            public string ToStringRaw()
            {
                Environment.Log.newScope("BMyCanvas.ToStringRaw");
                List<string> slug = new List<string>();
                Environment.Log.Trace("generate raw image");
                foreach (char[] line in pixels)
                {
                    string buffer = new string(line);
                    slug.Add(buffer);
                }                
                Environment.Log.leaveScope();
                return string.Join("\n", slug.ToArray());
            }

            public override string ToString()
            {
                Environment.Log.newScope("BMyCanvas.ToString");
                if(Environment.GlobalArgs.getOption("rawOutput").Count > 0)
                {
                    Environment.Log.leaveScope();
                    return ToStringRaw();
                } else
                {
                    Environment.Log.Trace("generate image");
                    Environment.Log.leaveScope();
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
                Environment.Log.newScope("BMyColor.ConvertFromRawImage");
                StringBuilder image = new StringBuilder((new System.Text.RegularExpressions.Regex(@"[^gbrywld\n]", System.Text.RegularExpressions.RegexOptions.IgnoreCase)).Replace(canvas.ToStringRaw().ToLowerInvariant(), PLACEHOLDER_BG.ToString()));
                image = image.Replace(PLACEHOLDER_BG, canvas.background);
                foreach (KeyValuePair<char, char> color in map)
                {
                    image = image.Replace(color.Key, color.Value);
                }
                Environment.Log.leaveScope();
                return image.ToString();
            }

            public char getColorCode(string args)
            {
                Environment.Log.newScope("BMyColor.getColorCode");
                string raw = args.Trim();
                char value = '1';
                if ((new System.Text.RegularExpressions.Regex(@"^(\\u[0-9a-f]{4})|(U\+[0-9a-f]{4})$").IsMatch(raw)))
                {
                    int ord;
                    if (int.TryParse(raw.Substring(2), out ord))
                    {
                        value = (char)ord;
                        Environment.Log.Trace("Found unicode Character: {0} => '{1}'", raw, value);
                    }
                }
                else if (raw.Length > 0)
                {
                    value = raw[0];
                    Environment.Log.Trace("Matching Color: {0} => '{1}'", raw, value);
                }
                else
                {
                    Environment.Log.Trace("No matching Color => using default: {0} => '{1}'", raw, value);
                }
                Environment.Log.leaveScope();
                return value;
            }
        }
        #endregion COLOR

        #region Pluginsystem
        class BMyDrawPluginHandler : Dictionary<string, List<BMyDrawPlugin>>
        {
            public readonly BMyEnvironment Environment;
            public readonly Dictionary<string, List<BMyDrawPlugin>> AvailabelPlugins = new Dictionary<string, List<BMyDrawPlugin>>();


            public BMyDrawPluginHandler(BMyEnvironment Environment)
            {
                this.Environment = Environment;
                bootstrap();
            }

            private void bootstrap()
            {
                Environment.Log.newScope("BMyDrawPluginHandler.bootstrap");
                #region DrawPlugin "BaconDraw"
                Environment.Log.Trace("Initializing DrawPlugin \"BaconDraw\"");
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

                Environment.Log.leaveScope();
            }

            public string[] getLoadedPlugins()
            {
                List<string> names = new List<string>();
                foreach (KeyValuePair<string, List<BMyDrawPlugin>> buffer in this)
                {
                    foreach(BMyDrawPlugin Plugin in buffer.Value)
                    {
                        if (!names.Contains(Plugin.Name))
                        {
                            names.Add(Plugin.Name);
                        }
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
                Environment.Log.newScope("BMyDrawPluginHandler.TryRequirePlugins");
                bool required = true;
                foreach (string Name in Names)
                {
                    if (!AvailabelPlugins.ContainsKey(Name))
                    {
                        Environment.Log.Trace("Plugin \"{0}\" not found", Name);
                        Environment.Log.leaveScope();
                        return false;
                    }
                    bool success = false;
                    foreach (BMyDrawPlugin Plugin in AvailabelPlugins[Name])
                    {
                        if (!TryAddPlugin(Plugin))
                        {
                            success = success || false;
                            Environment.Log.Trace("Error loading plugin \"{0}({1})\"", Plugin.Name, Plugin.Command);
                        }
                    }
                    if (!success)
                    {
                        Environment.Log.Trace("Unable to load anything of \"{0}\" plugin", Name);
                    }
                    required = required && success;
                }
                Environment.Log.leaveScope();
                return required;
            }

            string[] getRequiredPluginNames(BaconArgs Args)
            {
                Environment.Log.newScope("BMyDrawPluginHandler.getRequiredPluginNames");
                List<string> plugins = new List<string>();
                foreach (string names in Environment.GlobalArgs.getOption("plugin"))
                {
                    plugins.AddRange(names.Split(','));
                }
                Environment.Log.leaveScope();
                return plugins.ToArray();
            }

            public bool TryAddPlugin(BMyDrawPlugin Plugin)
            {
                Environment.Log.newScope("BMyDrawPluginHandler.AddPlugin");
                string command = Plugin.Command.ToLowerInvariant();
                if (!ContainsKey(command))
                {
                    Add(command, new List<BMyDrawPlugin>());
                }
                if (!this[command].Contains(Plugin))
                {
                    this[command].Insert(0, Plugin);
                    Environment.Log.Trace("Load Plugin \"{1}\" for \"{0}\"", command, Plugin.Name);
                    Environment.Log.leaveScope();
                    return true;
                }
                Environment.Log.Trace("Unable load Plugin \"{1}\" for \"{0}\" => {2}", command, Plugin.Name, (this[command].Contains(Plugin)?"already loaded":"unknown reason"));
                Environment.Log.leaveScope();
                return false;
            }

            public bool TryInterpret(BaconArgs Args, ref BMyCanvas canvas)
            {
                Environment.Log.newScope("BMyDrawPluginHandler.TryInterpret");
                if(Args.getArguments().Count == 0)
                {
                    Environment.Log.Trace("Skip line (no arguments)");
                    Environment.Log.leaveScope();
                    return false;
                }

                if (!hasPlugin(Args.getArguments()[0]))
                {
                    Environment.Log.leaveScope();
                    return false;
                }
                BMyTransaction<BMyCanvas> Buffer = new BMyTransaction<BMyCanvas>(canvas);

                bool success = false;
                Environment.Log.Trace("found {0} plugins for {1}", this[Args.getArguments()[0]].Count, Args.getArguments()[0]);
                foreach(BMyDrawPlugin Plugin in this[Args.getArguments()[0]])
                {
                    canvas = Buffer.Revert();
                    Environment.Log.Trace("try plugin {0}/{1}", Plugin.Name, Plugin.Command);
                    Environment.Log.newScope(string.Format("BMyDrawPlugin(\"{0}/{1}\").isValid", Plugin.Name, Plugin.Command));
                    bool isValid = Plugin.isValid(Args);
                    Environment.Log.leaveScope();
                    if (isValid)
                    {
                        Environment.Log.newScope(string.Format("BMyDrawPlugin(\"{0}/{1}\").TryInterpret", Plugin.Name, Plugin.Command));
                        success = Plugin.TryInterpret(Args, ref canvas);
                        Environment.Log.leaveScope();
                        if (success)
                        {
                            Environment.Log.Trace("successfull with plugin {0}/{1}", Plugin.Name, Plugin.Command);
                            break;
                        }
                        Environment.Log.Trace("no success with plugin {0}/{1} => try to continue", Plugin.Name, Plugin.Command);
                    }
                }
                Environment.Log.leaveScope();
                return success;
            }

            public bool hasPlugin(string command)
            {
                Environment.Log.newScope("BMyDrawPluginHandler.hasPlugin");
                bool contains = ContainsKey(command.ToLowerInvariant());
                Environment.Log.Trace("{0}Plugin for \"{1}\"", contains?"":"no ", command);
                Environment.Log.leaveScope();
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
                    Environment.Log.Trace("no valid arguments => can't draw {0}", Command);
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
                Environment.Log.Trace("draw {2} to {0},{1}", _x, _y, Command);
                return true;
            }

            public override bool isValid(BaconArgs Args)
            {
                if (!(Args.getArguments().Count == 2))
                {
                    Environment.Log.Trace("wrong number of arguments");
                    return false;
                }

                if (!(new System.Text.RegularExpressions.Regex(@"\d+,\d+")).IsMatch(Args.getArguments()[1]))
                {
                    Environment.Log.Trace("argument in wrong format (must be a number)");
                    return false;
                }
                return true;
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
                    Environment.Log.Trace("no plugin for \"lineto\" => can't draw {0}", Command);
                    return false;
                }
                for(int i = 1; i < Args.getArguments().Count; i++)
                {
                    BaconArgs LineToArgs = new BaconArgs();
                    LineToArgs.add("lineto");
                    LineToArgs.add(Args.getArguments()[i]);
                    if(!Environment.DrawPlugins.TryInterpret(LineToArgs, ref canvas))
                    {
                        Environment.Log.Trace("failed drawing line to {0} for {1}", Args.getArguments()[i], Command);
                        return false;
                    }
                }
                Environment.Log.Trace("drawed {0}", Command);
                return true;
            }

            public override bool isValid(BaconArgs Args)
            {
                if(Args.getArguments().Count < 2)
                {
                    Environment.Log.Trace("not enough arguments (must be at least 2)");
                    return false;
                }
                bool valid = true;

                for(int i = 1; i < Args.getArguments().Count; i++)
                {
                    if (!MatchRgx.IsMatch(Args.getArguments()[i]))
                    {
                        Environment.Log.Trace("argument[{1}]({0}) invalid. Must be `number,number`", Args.getArguments()[i], i);
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
                    Environment.Log.Trace("no plugin for \"polygon\" => can't draw {0}", Command);
                    return false;
                }
                Point dest;
                if(!canvas.TryParseCoords(Args.getArguments()[1], out dest)){
                    Environment.Log.Trace("invalid coordinates {0} => can't draw {1}", Args.getArguments()[1], Command);
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
                    Environment.Log.Trace("failed drawing {0}", Command);
                    return false;
                }
                canvas.setPosition(dest.X,dest.Y);
                Environment.Log.Trace("drawed a {0} to {1},{2}", Command, dest.X, dest.Y);
                return true;
            }

            public override bool isValid(BaconArgs Args)
            {
                if (!(Args.getArguments().Count == 2))
                {
                    Environment.Log.Trace("wrong number of arguments");
                    return false;
                }

                if (!(new System.Text.RegularExpressions.Regex(@"\d+,\d+")).IsMatch(Args.getArguments()[1]))
                {
                    Environment.Log.Trace("argument in wrong format (must be a number)");
                    return false;
                }

                return true;
            }
        }
        class BMyDrawPlugin_BaconDraw_Circle : BMyDrawPlugin
        {
            public override string Name { get { return "BaconDraw"; } }
            public override string Command { get { return "circle"; } }

            public BMyDrawPlugin_BaconDraw_Circle(BMyEnvironment Environment) : base(Environment){ }

            public override bool TryInterpret(BaconArgs Args, ref BMyCanvas canvas)
            {
                int Radius;
                if(!int.TryParse(Args.getArguments()[1], out Radius))
                {
                    Environment.Log.Trace("Invalid argument \"{0}\" must be a number", Args.getArguments()[1]);
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
                Environment.Log.Trace("drawed a {0} with a radius of {1}", Command, Radius);
                return true;
            }

            public override bool isValid(BaconArgs Args)
            {
                if(!(Args.getArguments().Count == 2))
                {
                    Environment.Log.Trace("wrong number of arguments");
                    return false;
                }

                if(!(new System.Text.RegularExpressions.Regex(@"\d+")).IsMatch(Args.getArguments()[1]))
                {
                    Environment.Log.Trace("argument in wrong format (must be a number)");
                    return false;
                }

                return true;
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
                    Environment.Log.Trace("not enough arguments");
                    return false;
                }
                if (!NameRgx.IsMatch(Args.getArguments()[indexName]))
                {
                    Environment.Log.Trace("invalid argument for fontname `{0}`", Args.getArguments()[indexName]);
                    return false;
                }

                if(!SizeRgx.IsMatch(Args.getArguments()[indexSize]))
                {
                    Environment.Log.Trace("invalid argument for fontsize `{0}`", Args.getArguments()[indexSize]);
                    return false;
                }
                int w;
                int h;
                string[] size = Args.getArguments()[indexSize].Split(',');
                if(!(size.Length == 2) || !int.TryParse(size[0], out w) || !int.TryParse(size[1], out h))
                {
                    Environment.Log.Trace("invalid argument for fontsize `{0}`", Args.getArguments()[indexSize]);
                }
                
                for(int i = indexChars; i < Args.getArguments().Count; i++)
                {
                    if (!GlyphRgx.IsMatch(Args.getArguments()[i]))
                    {
                        Environment.Log.Trace("Invalid character definition `{0}` at argument #{1}", Args.getArguments()[i], i+1);
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
                for (int i = indexChars; i < Args.getArguments().Count; i++)
                {
                    string argument = Args.getArguments()[i];
                    char glyph = argument[indexGlyph];
                    string[] data = getData(argument.Substring(indexGlyphData), w, h);
                    font.Add(glyph, data);
                }
                Environment.Log.Trace("new font \"{0}{1}\" with BxH {2}x{3} and {4} characters", font.Name, (extends.Length >0)?":"+font.Extends:"",font.Width,font.Height,font.Count);
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
                Environment.Log.Trace("NotImplementedException");
                return false;
            }

            public override bool TryInterpret(BaconArgs Args, ref BMyCanvas canvas)
            {
                Environment.Log.Trace("NotImplementedException");
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
                if(!(Args.getArguments().Count == 2)){
                    Environment.Log.Trace("wrong number of arguments");
                    return false;
                }
                return true;
            }

            public override bool TryInterpret(BaconArgs Args, ref BMyCanvas canvas)
            {
                canvas.background = Environment.Color.getColorCode(Args.getArguments()[1]);
                Environment.Log.Trace("interpreted \"{0}\" as color '{1}'", Args.getArguments()[1], canvas.background);
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
                if (!(Args.getArguments().Count == 2))
                {
                    Environment.Log.Trace("wrong number of arguments");
                    return false;
                }
                return true;
            }

            public override bool TryInterpret(BaconArgs Args, ref BMyCanvas canvas)
            {
                canvas.color = Environment.Color.getColorCode(Args.getArguments()[1]);
                Environment.Log.Trace("interpreted \"{0}\" as color '{1}'", Args.getArguments()[1], canvas.color);
                return true;
            }
        }
        #endregion included Plugins

        #region 3rd party Plugins

        #endregion 3rd party Plugins

        #region included Libs
        public class BaconArgs { public string InputData; static public BaconArgs parse(string a) { return (new Parser()).parseArgs(a); } static public string Escape(string a) { return a.Replace(@"\", @"\\").Replace(@" ", @"\ ").Replace(@"""", @"\"""); } static public string UnEscape(string a) { return a.Replace(@"\""", @"""").Replace(@"\ ", @" ").Replace(@"\\", @"\"); } public class Parser { static Dictionary<string, BaconArgs> h = new Dictionary<string, BaconArgs>(); public BaconArgs parseArgs(string a) { if (!h.ContainsKey(a)) { var b = new BaconArgs(); b.InputData = a; var c = false; var d = false; var e = new StringBuilder(); for (int f = 0; f < a.Length; f++) { var g = a[f]; if (c) { e.Append(g); c = false; } else if (g.Equals('\\')) c = true; else if (d && !g.Equals('"')) e.Append(g); else if (g.Equals('"')) d = !d; else if (g.Equals(' ')) if (e.ToString().Equals("--")) { b.add(a.Substring(f).TrimStart()); e.Clear(); break; } else { b.add(e.ToString()); e.Clear(); } else e.Append(g); } if (e.Length > 0) b.add(e.ToString()); h.Add(a, b); } return h[a]; } } protected Dictionary<char, int> h = new Dictionary<char, int>(); protected List<string> i = new List<string>(); protected Dictionary<string, List<string>> j = new Dictionary<string, List<string>>(); public List<string> getArguments() { return i; } public int getFlag(char a) { return h.ContainsKey(a) ? h[a] : 0; } public List<string> getOption(string a) { return j.ContainsKey(a) ? j[a] : new List<string>(); } public void add(string a) { if (a.Trim().Length > 0) if (!a.StartsWith("-")) { i.Add(a); } else if (a.StartsWith("--")) { KeyValuePair<string, string> b = k(a); string c = b.Key.Substring(2); if (!j.ContainsKey(c)) { j.Add(c, new List<string>()); } j[c].Add(b.Value); } else { string b = a.Substring(1); for (int d = 0; d < b.Length; d++) { if (this.h.ContainsKey(b[d])) { this.h[b[d]]++; } else { this.h.Add(b[d], 1); } } } } KeyValuePair<string, string> k(string a) { string[] b = a.Split(new char[] { '=' }, 2); return new KeyValuePair<string, string>(b[0], (b.Length > 1) ? b[1] : null); } public string ToArguments() { var a = new List<string>(); foreach (string argument in this.getArguments()) a.Add(Escape(argument)); foreach (KeyValuePair<string, List<string>> option in this.j) { var b = "--" + Escape(option.Key); foreach (string optVal in option.Value) a.Add(b + ((optVal != null) ? "=" + Escape(optVal) : "")); } var c = (h.Count > 0) ? "-" : ""; foreach (KeyValuePair<char, int> flag in h) c += new String(flag.Key, flag.Value); a.Add(c); return String.Join(" ", a.ToArray()); } override public string ToString() { var a = new List<string>(); foreach (string key in j.Keys) a.Add(l(key) + ":[" + string.Join(",", j[key].ConvertAll<string>(b => l(b)).ToArray()) + "]"); var c = new List<string>(); foreach (char key in h.Keys) c.Add(key + ":" + h[key].ToString()); var d = new StringBuilder(); d.Append("{\"a\":["); d.Append(string.Join(",", i.ConvertAll<string>(b => l(b)).ToArray())); d.Append("],\"o\":[{"); d.Append(string.Join("},{", a)); d.Append("}],\"f\":[{"); d.Append(string.Join("},{", c)); d.Append("}]}"); return d.ToString(); } string l(string a) { return (a != null) ? "\"" + a.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" : @"null"; } }
        public class BaconDebug { public const int OFF = 0; public const int FATAL = 1; public const int ERROR = 2; public const int WARN = 3; public const int INFO = 4; public const int DEBUG = 5; public const int TRACE = 6; Dictionary<int, string> h = new Dictionary<int, string>() { { OFF, "OFF" }, { FATAL, "FATAL" }, { ERROR, "ERROR" }, { WARN, "WARN" }, { INFO, "INFO" }, { DEBUG, "DEBUG" }, { TRACE, "TRACE" }, }; List<IMyTextPanel> j = new List<IMyTextPanel>(); MyGridProgram k; List<KeyValuePair<string, long>> l = new List<KeyValuePair<string, long>>(); int m = OFF; bool n = true; public int remainingInstructions { get { return k.Runtime.MaxInstructionCount - k.Runtime.CurrentInstructionCount; } } public bool autoscroll { get { return n; } set { n = value; } } public void clearPanels() { for (int a = 0; a < j.Count; a++) j[a].WritePublicText(""); } public BaconDebug(string a, IMyGridTerminalSystem b, MyGridProgram c, int d, string e = "BaconDebug") { this.m = d; var f = new List<IMyTerminalBlock>(); b.GetBlocksOfType<IMyTextPanel>(f, ((IMyTerminalBlock g) => g.CustomName.Contains(a) && g.CubeGrid.Equals(c.Me.CubeGrid))); j = f.ConvertAll<IMyTextPanel>(g => g as IMyTextPanel); this.k = c; newScope(e); } public int getVerbosity() { return m; } public MyGridProgram getGridProgram() { return this.k; } public void newScope(string a) { l.Add(new KeyValuePair<string, long>(a, DateTime.Now.Ticks)); if (this.m.Equals(TRACE)) this.Trace("STEP INTO SCOPE"); } public void leaveScope() { if (l.Count > 0 && this.m.Equals(TRACE)) this.Trace("LEAVE SCOPE ({0} Ticks)", o(l[l.Count - 1].Value)); if (l.Count > 1) l.RemoveAt(l.Count - 1); } public string getSender() { if (l.Count > 0) if (this.m.Equals(TRACE)) { List<string> a = new List<string>(); foreach (KeyValuePair<string, long> entry in l) { a.Add(entry.Key); } return string.Join(">", a.ToArray()); } else { return l[l.Count - 1].Key; } return "NO SCOPE DEFINED"; } double o(long a) { long b = DateTime.Now.Ticks; return (Math.Max(a, b) - Math.Min(a, b)); } public void Fatal(string a, params object[] b) { Fatal(string.Format(a, b)); } public void Error(string a, params object[] b) { Error(string.Format(a, b)); } public void Warn(string a, params object[] b) { Warn(string.Format(a, b)); } public void Info(string a, params object[] b) { Info(string.Format(a, b)); } public void Debug(string a, params object[] b) { Debug(string.Format(a, b)); } public void Trace(string a, params object[] b) { Trace(string.Format(a, b)); } public void Fatal(string a) { add(a, FATAL); } public void Error(string a) { add(a, ERROR); } public void Warn(string a) { add(a, WARN); } public void Info(string a) { add(a, INFO); } public void Debug(string a) { add(a, DEBUG); } public void Trace(string a) { add(a, TRACE); } public void add(string a, int b) { p(a, b); if (b <= this.m) { var c = t(a, b); for (int d = 0; d < j.Count; d++) { var e = new List<string>(); e.AddRange(j[d].GetPublicText().Trim().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)); e.Add(c); r(ref e, s(j[d])); var f = string.Join("\n", e.ToArray()); q(ref f); j[d].WritePublicText(f); } } } void p(string a, int b) { if (b <= ERROR) k.Echo(a); } void q(ref string a) { if (100000 < a.Length) { a = a.Substring(a.Length - 100000); int b = a.IndexOf('\n'); a = a.Substring(a.Length - b).TrimStart(new char[] { '\n', '\r' }); } } void r(ref List<string> a, int b) { if (autoscroll && 0 < b && b < a.Count) a.RemoveRange(0, a.Count - b); } int s(IMyTextPanel a) { float b = a.GetValueFloat("FontSize"); if (b == 0.0f) b = 0.01f; return Convert.ToInt32(Math.Ceiling(17.0f / b)); } string t(string a, int b) { DateTime c = DateTime.Now; var d = @"[{0}-{1}.{2}][{3}][{4}][IC {5}/{6}] {7}"; object[] e = new object[] { c.ToShortDateString(), c.ToShortTimeString(), c.Millisecond.ToString().TrimStart('0'), u(b), getSender(), k.Runtime.CurrentInstructionCount, k.Runtime.MaxInstructionCount, a }; return string.Format(d, e); } string u(int a) { if (h.ContainsKey(a)) return h[a]; return string.Format("`{0}`", a); } }
        #endregion included Libs


        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}