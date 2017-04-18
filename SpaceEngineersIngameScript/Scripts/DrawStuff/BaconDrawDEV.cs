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

namespace BaconDrawDEV
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        public void Main(string argument)
        {
            Echo("## START ##");
            IMyTextPanel LCD = GridTerminalSystem.GetBlockWithName(argument) as IMyTextPanel;
            if(LCD != null)
            {
                string[] code = LCD.CustomData.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                BMyBaconDraw BD = new BMyBaconDraw();
                BMyBaconDraw.BMyCanvas canvas = BD.run(code, this);
                LCD.WritePublicText(canvas.ToString());
                LCD.ShowPublicTextOnScreen();
            } else
            {
                Echo("-- LCD not found --");
            }
            Echo("## END ##");
        }

        class BMyBaconDraw
        {

            public BMyCanvas run(string[] source, Program Assembly)
            {
                #region Environment
                BMyEnvironment Env = new BMyEnvironment(Assembly);
                Env.Log?.PushStack("run(string[] source, Program Assembly)");
                #endregion Environment

                #region build plugins
                // prepare pluginhandlers
                Env.Log?.Debug("START - Preparing Plugins");
                Env.DrawPlugins.AddPlugin(new BMyDrawPlugin_Background()); // "background R,G,B" where R G B is 0-6
                Env.DrawPlugins.AddPlugin(new BMyDrawPlugin_Circle()); // "circle RADIUS" where RADIUS is integer
                Env.DrawPlugins.AddPlugin(new BMyDrawPlugin_LineTo()); // "lineto x,y" where x y is integer
                Env.DrawPlugins.AddPlugin(new BMyDrawPlugin_MoveTo()); // "moveto x,y" where x y is integer
                Env.DrawPlugins.AddPlugin(new BMyDrawPlugin_Rect()); // "rect x,y" where x y is integer
                Env.DrawPlugins.AddPlugin(new BMyDrawPlugin_Polygon()); // "poly x,y x,y x,y" where x y is integer
                Env.DrawPlugins.AddPlugin(new BMyDrawPlugin_Color()); // "color R,G,B" where R G B is 0-6
                Env.DrawPlugins.AddPlugin(new BMyDrawPlugin_Dot()); // "dot x,y" where x y is integer
                foreach (KeyValuePair<string, List<BMyDrawPlugin>> _pluginlist in Env.DrawPlugins)
                {
                    Env.Log?.Debug(@"{0} Plugins for {1}", _pluginlist.Value.Count, _pluginlist.Key);
                }
                Env.Log?.Debug("END - Preparing Plugins");
                #endregion build plugins

                #region build code queue
                Env.Log?.Debug("START - Building Code queue");
                Queue<BMyDrawingCommand> codeQueue = new Queue<BMyDrawingCommand>();
                Env.Log?.Debug("code has {0} lines", source.Length);
                foreach (string currentLine in source)
                {
                    Env.Log?.Debug("enqueue command: {0}", currentLine);
                    codeQueue.Enqueue(new BMyDrawingCommand(currentLine));
                }
                Env.Log?.Debug("END - Building Code queue");
                #endregion build code queue

                #region make canvas
                BMyCanvas canvas = new BMyCanvas(50, 50, new BMyColor(0, 0, 0));
                Env.Log?.Debug("created canvas. {0}x{1}", canvas.Width, canvas.Height);
                #endregion make canvas

                #region progress codeQueue
                Env.Log?.Debug("START - progressing code");
                while (codeQueue.Count > 0)
                {
                    // break if over limits
                    BMyDrawingCommand command = codeQueue.Dequeue();
                    if (Env.TryRunDraw(command, canvas))
                    {
                        Env.Log?.Debug("OK: command \"{0}\"", command);
                    }
                    else
                    {
                        Env.Log?.Debug("FAIL: command \"{0}\"", command);
                    }
                }
                Env.Log?.Debug("END - progressing code");

                Env.Log?.PopStack();
                return canvas;
                #endregion progress codeQueue
            }

            #region pluginsystem
            class BMyPluginBag<T> : Dictionary<string, List<T>> where T : BMYPlugin
            {
                public readonly BMyEnvironment Env;

                public BMyPluginBag(BMyEnvironment Env)
                {
                    this.Env = Env;
                }


                public void AddPlugin(T Plugin)
                {
                    Env.Log?.PushStack("BMyPluginBag<T>.AddPlugin(T Plugin)");
                    if (!ContainsKey(Plugin.Name))
                    {
                        Env.Log?.Debug("new List for Plugin<T> {0}", Plugin);
                        Add(Plugin.Name, new List<T>());
                    }
                    this[Plugin.Name].Add(Plugin);
                    Env.Log?.PopStack();
                }

                public List<T> FindAllByName(string name)
                {
                    Env.Log?.PushStack("BMyPluginBag<T>.FindAllByName(string name)");
                    List<T> buffer = new List<T>();
                    if (ContainsKey(name))
                    {
                        buffer = this[name];
                        
                    }
                    Env.Log?.Debug("found {0} plugins for \"{1}\"", buffer.Count, name);
                    Env.Log?.PopStack();
                    return buffer;
                }
            }

            abstract class BMYPlugin
            {
                abstract public string Name { get; }
                abstract public string Vendor { get; }
            }

            abstract class BMyDrawPlugin : BMYPlugin
            {
                abstract public bool TryRun(BMyDrawingCommand command, BMyCanvas canvas, BMyEnvironment Env);
            }
            #endregion pluginsystem

            #region draw plugins
            class BMyDrawPlugin_Background : BMyDrawPlugin
            {
                public override string Name
                {
                    get
                    {
                        return "background";
                    }
                }

                public override string Vendor
                {
                    get
                    {
                        return "DasBaconfist";
                    }
                }


                public override bool TryRun(BMyDrawingCommand command, BMyCanvas canvas, BMyEnvironment Env)
                {
                    byte r;
                    byte g;
                    byte b;
                    string[] argv = command.Args.Split(new char[] { ',' });

                    if (argv.Length == 3
                        && byte.TryParse(argv[0], out r)
                        && byte.TryParse(argv[1], out g)
                        && byte.TryParse(argv[2], out b)
                    )
                    {
                        canvas = new BMyCanvas(canvas.Width, canvas.Height, new BMyColor(r, g, b));
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            class BMyDrawPlugin_MoveTo : BMyDrawPlugin
            {
                public override string Name
                {
                    get
                    {
                        return "moveto";
                    }
                }

                public override string Vendor
                {
                    get
                    {
                        return "DasBaconfist";
                    }
                }

                public override bool TryRun(BMyDrawingCommand command, BMyCanvas canvas, BMyEnvironment Env)
                {
                    string[] argv = command.Args.Split(new char[] { ',' });
                    int x;
                    int y;
                    if (
                        argv.Length == 2
                        && int.TryParse(argv[0], out x)
                        && int.TryParse(argv[1], out y)
                        )
                    {
                        canvas.Position = new BMyPoint(x, y);
                        return (canvas.Position.X == x && canvas.Position.Y == y);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            class BMyDrawPlugin_LineTo : BMyDrawPlugin
            {
                public override string Name
                {
                    get
                    {
                        return "lineto";
                    }
                }

                public override string Vendor
                {
                    get
                    {
                        return "DasBaconfist";
                    }
                }

                public override bool TryRun(BMyDrawingCommand command, BMyCanvas canvas, BMyEnvironment Env)
                {
                    string[] argv = command.Args.Split(new char[] { ',' });
                    int x;
                    int y;
                    if (
                        argv.Length == 2
                        && int.TryParse(argv[0], out x)
                        && int.TryParse(argv[1], out y)
                        )
                    {
                        BMyPoint target = new BMyPoint(x, y);
                        lineTo(target, canvas);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                private void lineTo(BMyPoint target, BMyCanvas canvas)
                {
                    int x, y, t, deltaX, deltaY, incrementX, incrementY, pdx, pdy, ddx, ddy, es, el, err;
                    BMyPoint origin = canvas.Position;
                    deltaX = target.X - origin.X;
                    deltaY = target.Y - origin.Y;

                    incrementX = Math.Sign(deltaX);
                    incrementY = Math.Sign(deltaY);
                    if (deltaX < 0) deltaX = -deltaX;
                    if (deltaY < 0) deltaY = -deltaY;

                    if (deltaX > deltaY)
                    {
                        pdx = incrementX; pdy = 0;
                        ddx = incrementX; ddy = incrementY;
                        es = deltaY; el = deltaX;
                    }
                    else
                    {
                        pdx = 0; pdy = incrementY;
                        ddx = incrementX; ddy = incrementY;
                        es = deltaX; el = deltaY;
                    }
                    x = origin.X;
                    y = origin.Y;
                    err = el / 2;
                    canvas.TrySetPixel(x, y);

                    for (t = 0; t < el; ++t)
                    {
                        err -= es;
                        if (err < 0)
                        {
                            err += el;
                            x += ddx;
                            y += ddy;
                        }
                        else
                        {
                            x += pdx;
                            y += pdy;
                        }
                        canvas.TrySetPixel(x, y);
                    }
                }
            }
            class BMyDrawPlugin_Circle : BMyDrawPlugin
            {
                public override string Name
                {
                    get
                    {
                        return "circle";
                    }
                }

                public override string Vendor
                {
                    get
                    {
                        return "DasBaconfist";
                    }
                }

                public override bool TryRun(BMyDrawingCommand command, BMyCanvas canvas, BMyEnvironment Env)
                {
                    int r;
                    if (int.TryParse(command.Args, out r))
                    {
                        circle(r, canvas);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                public void circle(int r, BMyCanvas canvas)
                {
                    int d;
                    int x = r;
                    d = r * -1;
                    for (int y = 0; y <= x; y++)
                    {
                        setSymetric(x, y, canvas);
                        d = d + 2 * y + 1;
                        if (d > 0)
                        {
                            d = d - 2 * x + 2;
                            x = x - 1;
                        }
                    }
                }

                private void setSymetric(int x, int y, BMyCanvas canvas)
                {
                    BMyPoint o = canvas.Position;

                    canvas.TrySetPixel(o.X + x, o.Y + y, false);
                    canvas.TrySetPixel(o.X + y, o.Y + x, false);
                    canvas.TrySetPixel(o.X + y, o.Y + -x, false);
                    canvas.TrySetPixel(o.X + x, o.Y + -y, false);
                    canvas.TrySetPixel(o.X + -x, o.Y + -y, false);
                    canvas.TrySetPixel(o.X + -y, o.Y + -x, false);
                    canvas.TrySetPixel(o.X + -y, o.Y + x, false);
                    canvas.TrySetPixel(o.X + -x, o.Y + y, false);
                }
            }
            class BMyDrawPlugin_Rect : BMyDrawPlugin
            {
                public override string Name
                {
                    get
                    {
                        return "rect";
                    }
                }

                public override string Vendor
                {
                    get
                    {
                        return "DasBaconfist";
                    }
                }

                public override bool TryRun(BMyDrawingCommand command, BMyCanvas canvas, BMyEnvironment Env)
                {
                    string[] argv = command.Args.Split(new char[] { ',' });
                    int x;
                    int y;
                    if (
                        argv.Length == 2
                        && int.TryParse(argv[0], out x)
                        && int.TryParse(argv[1], out y)
                        )
                    {
                        bool success = false;
                        BMyPoint origin = canvas.Position;
                        BMyDrawingCommand linetoCommand = new BMyDrawingCommand(string.Format(@"lineto {0},{1}", x, origin.Y));
                        if (Env.TryRunDraw(linetoCommand, canvas))
                        {
                            linetoCommand = new BMyDrawingCommand(string.Format(@"lineto {0},{1}", x, y));
                            if (Env.TryRunDraw(linetoCommand, canvas))
                            {
                                linetoCommand = new BMyDrawingCommand(string.Format(@"lineto {0},{1}", origin.X, y));
                                if (Env.TryRunDraw(linetoCommand, canvas))
                                {
                                    linetoCommand = new BMyDrawingCommand(string.Format(@"lineto {0},{1}", origin.X, origin.Y));
                                    if (Env.TryRunDraw(linetoCommand, canvas))
                                    {
                                        success = true;
                                    }
                                }
                            }
                        }

                        canvas.Position = new BMyPoint(x, y);
                        return success;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            class BMyDrawPlugin_Polygon : BMyDrawPlugin
            {
                public override string Name
                {
                    get
                    {
                        return "poly";
                    }
                }

                public override string Vendor
                {
                    get
                    {
                        return "DasBaconfist";
                    }
                }

                public override bool TryRun(BMyDrawingCommand command, BMyCanvas canvas, BMyEnvironment Env)
                {
                    bool success = false;
                    string[] argv = command.Args.Split(new char[] { ' ' });
                    foreach (string arg in argv)
                    {
                        string[] _pos = arg.Split(new char[] { ',' }, 2);
                        int x;
                        int y;
                        if (_pos.Length == 2
                            && int.TryParse(_pos[0], out x)
                            && int.TryParse(_pos[1], out y)
                            )
                        {
                            success = success && Env.TryRunDraw(new BMyDrawingCommand(string.Format(@"lineto {0},{1}", x, y)), canvas);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    return success;
                }
            }
            class BMyDrawPlugin_Color : BMyDrawPlugin
            {
                public override string Name
                {
                    get
                    {
                        return "color";
                    }
                }

                public override string Vendor
                {
                    get
                    {
                        return "DasBaconfist";
                    }
                }

                public override bool TryRun(BMyDrawingCommand command, BMyCanvas canvas, BMyEnvironment Env)
                {
                    byte r;
                    byte g;
                    byte b;
                    string[] argv = command.Args.Split(new char[] { ',' });

                    if (argv.Length == 3
                        && byte.TryParse(argv[0], out r)
                        && byte.TryParse(argv[1], out g)
                        && byte.TryParse(argv[2], out b)
                    )
                    {
                        canvas.color = new BMyColor(r, g, b);
                        return true;
                    }
                    return false;
                }
            }
            class BMyDrawPlugin_Dot : BMyDrawPlugin
            {
                public override string Name
                {
                    get
                    {
                        return "dot";
                    }
                }

                public override string Vendor
                {
                    get
                    {
                        return "DasBaconfist";
                    }
                }

                public override bool TryRun(BMyDrawingCommand command, BMyCanvas canvas, BMyEnvironment Env)
                {
                    string[] argv = command.Args.Split(new char[] { ',' });
                    int x;
                    int y;
                    if (
                        argv.Length == 2
                        && int.TryParse(argv[0], out x)
                        && int.TryParse(argv[1], out y)
                        )
                    {
                        return canvas.TrySetPixel(x, y);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            #endregion draw plugins

            class BMyEnvironment
            {
                public readonly BMyPluginBag<BMyDrawPlugin> DrawPlugins;
                public readonly Program Assembly;
                public readonly BMyLog4PB Log;

                public BMyEnvironment(Program Assembly)
                {
                    this.Assembly = Assembly;
                    Log = new BMyLog4PB(Assembly, BMyLog4PB.E_ALL, new BMyLog4PB.BMyCustomDataAppender(Assembly));
                    DrawPlugins = new BMyPluginBag<BMyDrawPlugin>(this);
                    Log.Debug("Environment initialized.");
                }

                public bool TryRunDraw(BMyDrawingCommand command, BMyCanvas canvas)
                {
                    Log?.PushStack("BMyEnvironment.TryRunDraw(BMyDrawingCommand command, BMyCanvas canvas)");
                    List<BMyDrawPlugin> DrawPluginsForCommand = DrawPlugins.FindAllByName(command.Key);
                    Log?.Debug("found {0} commands for {1}", DrawPluginsForCommand.Count, command.Key);
                    bool successfull = false;
                    if (DrawPluginsForCommand.Count > 0)
                    {
                        for (int i = 0; i < DrawPluginsForCommand.Count && !successfull; i++)
                        {
                            successfull = DrawPluginsForCommand[i].TryRun(command, canvas, this);
                            Log?.Debug("{0}: Plugin {1}/{2} with \"{3}\"", successfull?"OK":"FAIL", DrawPluginsForCommand[i].Vendor, DrawPluginsForCommand[i].Name, command);
                        }
                    }
                    Log?.Debug("{0}: Drawing of \"{1}\"", successfull ? "OK" : "FAIL", command);
                    Log?.PopStack();
                    return successfull;
                }
            }
            class BMyDrawingCommand
            {
                public string Key;
                public string Args;

                public BMyDrawingCommand(string cmd)
                {
                    string[] slug = cmd.Split(new Char[] { ' ' }, 2);
                    Key = slug[0];
                    Args = (slug.Length == 2) ? slug[1] : "";
                }

                public override string ToString()
                {
                    return string.Format(@"{0} {1}", Key, Args);
                }
            }
            public class BMyColor
            {
                public byte R;
                public byte G;
                public byte B;

                public BMyColor(byte red, byte green, byte blue)
                {
                    R = red;
                    G = green;
                    B = blue;
                }

                public char ToChar()
                {
                    return (char)(0xe100 + (R << 6) + (G << 3) + B);
                }
            }
            public class BMyPoint
            {
                public int X;
                public int Y;
                public BMyPoint(int X, int Y)
                {
                    this.X = X;
                    this.Y = Y;
                }
            }
            public class BMyCanvas
            {
                private char[][] _raster;
                public readonly int Width;
                public readonly int Height;
                public BMyColor color = new BMyColor(255, 255, 255);
                private BMyPoint _pos = new BMyPoint(0, 0);

                public BMyPoint Position
                {
                    get { return _pos; }
                    set
                    {
                        if (isInBounds(value.X, value.Y))
                        {
                            _pos = value;
                        }
                    }
                }

                public BMyCanvas(int width, int height, BMyColor background)
                {
                    Width = width;
                    Height = height;
                    _raster = new char[Height][];
                    char bgChar = background.ToChar();
                    for (int i = 0; i < _raster.Length; i++)
                    {
                        _raster[i] = (new String(bgChar, Width)).ToCharArray();
                    }
                }
                public bool isInBounds(int x, int y)
                {
                    return (0 <= x && x < Width && 0 <= y && y < Height);
                }
                public bool TrySetPixel(int x, int y, bool moveto = true)
                {
                    if (isInBounds(x, y))
                    {
                        _raster[x][y] = color.ToChar();
                        if (moveto)
                        {
                            Position = new BMyPoint(x, y);
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                override public string ToString()
                {
                    StringBuilder buffer = new StringBuilder();
                    foreach (char[] line in _raster)
                    {
                        buffer.AppendLine(new String(line));
                    }
                    return buffer.ToString();
                }
            }
        }

        #region BMyLog4PB
        public class BMyLog4PB
        {
            public const byte E_ALL = 63;
            public const byte E_TRACE = 32; //Lowest	Finest-grained informational events.
            public const byte E_DEBUG = 16; //Fine-grained informational events that are most useful to debug an application.
            public const byte E_INFO = 8; //Informational messages that highlight the progress of the application at coarse-grained level.
            public const byte E_WARN = 4; //Potentially harmful situations which still allow the application to continue running.
            public const byte E_ERROR = 2; //Error events that might still allow the application to continue running.
            public const byte E_FATAL = 1; //Highest	Very severe error events that will presumably lead the application to abort.

            private Dictionary<string, string> formatMarkerMap = new Dictionary<string, string>() {
                {"{Date}","{0}"},
                {"{Time}","{1}"},
                {"{Milliseconds}","{2}"},
                {"{Severity}","{3}"},
                {"{CurrentInstructionCount}","{4}"},
                {"{MaxInstructionCount}","{5}"},
                {"{Message}","{6}"},
                {"{Stack}","{7}" }
            };
            private Stack<string> Stack = new Stack<string>();
            public byte Filter;
            public readonly Dictionary<BMyAppenderBase, string> Appenders = new Dictionary<BMyAppenderBase, string>();
            private string _defaultFormat = @"[{0}-{1}/{2}][{3}][{4}/{5}][{7}] {6}";
            private string _formatRaw = @"[{Date}-{Time}/{Milliseconds}][{Severity}][{CurrentInstructionCount}/{MaxInstructionCount}][{Stack}] {Message}";
            public string Format
            {
                get { return _formatRaw; }
                set
                {
                    _defaultFormat = compileFormat(value);
                    _formatRaw = value;
                }
            }
            private readonly Program Assembly;
            public bool AutoFlush = true;

            public BMyLog4PB(Program Assembly) : this(Assembly, E_FATAL | E_ERROR | E_WARN | E_INFO, new BMyEchoAppender(Assembly))
            {

            }
            public BMyLog4PB(Program Assembly, byte filter, params BMyAppenderBase[] Appenders)
            {
                Filter = filter;
                this.Assembly = Assembly;
                foreach (var Appender in Appenders)
                {
                    AddAppender(Appender);
                }
            }
            private string compileFormat(string value)
            {
                string format = value;
                foreach (var item in formatMarkerMap)
                {
                    format = format?.Replace(item.Key, item.Value);
                }
                return format;
            }

            public BMyLog4PB Flush()
            {
                foreach (var AppenderItem in Appenders)
                {
                    AppenderItem.Key.Flush();
                }
                return this;
            }
            public BMyLog4PB PushStack(string name)
            {
                Stack.Push(name);
                return this;
            }
            public string PopStack()
            {
                return (Stack.Count > 0) ? Stack.Pop() : null;
            }
            private string PeekStack()
            {
                return (Stack.Count > 0) ? Stack.Peek() : null;
            }
            public string StackToString()
            {
                if (If(E_TRACE) != null)
                {
                    string[] buffer = Stack.ToArray();
                    Array.Reverse(buffer);
                    return string.Join(@"/", buffer);
                }
                else
                {
                    return PeekStack();
                }
            }
            public BMyLog4PB AddAppender(BMyAppenderBase Appender, string format = null)
            {
                if (!Appenders.ContainsKey(Appender))
                {
                    Appenders.Add(Appender, compileFormat(format));
                }

                return this;
            }
            public BMyLog4PB If(byte filter)
            {
                return ((filter & Filter) != 0) ? this : null;
            }
            public BMyLog4PB Fatal(string format, params object[] values)
            {
                If(E_FATAL)?.Append("FATAL", format, values);
                return this;
            }
            public BMyLog4PB Error(string format, params object[] values)
            {
                If(E_ERROR)?.Append("ERROR", format, values);
                return this;
            }
            public BMyLog4PB Warn(string format, params object[] values)
            {
                If(E_WARN)?.Append("WARN", format, values);
                return this;
            }
            public BMyLog4PB Info(string format, params object[] values)
            {
                If(E_INFO)?.Append("INFO", format, values);
                return this;
            }
            public BMyLog4PB Debug(string format, params object[] values)
            {
                If(E_DEBUG)?.Append("DEBUG", format, values);
                return this;
            }
            public BMyLog4PB Trace(string format, params object[] values)
            {
                If(E_TRACE)?.Append("TRACE", format, values);
                return this;
            }
            private void Append(string level, string format, params object[] values)
            {
                DateTime DT = DateTime.Now;
                var message = new BMyMessage(
                    DT.ToShortDateString(),
                    DT.ToLongTimeString(),
                    DT.Millisecond.ToString(),
                    level,
                    Assembly.Runtime.CurrentInstructionCount,
                    Assembly.Runtime.MaxInstructionCount,
                    string.Format(format, values),
                    StackToString()
                );
                foreach (var item in Appenders)
                {
                    var formatBuffer = (item.Value != null) ? item.Value : _defaultFormat;
                    item.Key.Enqueue(message.ToString(formatBuffer));
                    if (AutoFlush)
                    {
                        item.Key.Flush();
                    }
                }
            }
            class BMyMessage
            {
                public string Date;
                public string Time;
                public string Milliseconds;
                public string Severity;
                public int CurrentInstructionCount;
                public int MaxInstructionCount;
                public string Message;
                public string Stack;
                public BMyMessage(string Date, string Time, string Milliseconds, string Severity, int CurrentInstructionCount, int MaxInstructionCount, string Message, string Stack)
                {
                    this.Date = Date;
                    this.Time = Time;
                    this.Milliseconds = Milliseconds;
                    this.Severity = Severity;
                    this.CurrentInstructionCount = CurrentInstructionCount;
                    this.MaxInstructionCount = MaxInstructionCount;
                    this.Message = Message;
                    this.Stack = Stack;
                }
                public override string ToString()
                {
                    return ToString(@"{0},{1},{2},{3},{4},{5},{6},{7},{8}");
                }
                public string ToString(string format)
                {
                    return string.Format(
                        format,
                        Date,
                        Time,
                        Milliseconds,
                        Severity,
                        CurrentInstructionCount,
                        MaxInstructionCount,
                        Message,
                        Stack
                        );
                }
            }
            public class BMyTextPanelAppender : BMyAppenderBase
            {
                List<string> Queue = new List<string>();
                List<IMyTextPanel> Panels = new List<IMyTextPanel>();
                public bool Autoscroll = true;
                public bool Prepend = false;
                public BMyTextPanelAppender(string tag, Program Assembly)
                {
                    Assembly.GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(Panels, (p => p.CustomName.Contains(tag)));
                }
                public override void Enqueue(string message)
                {
                    Queue.Add(message);
                }
                public override void Flush()
                {
                    foreach (var Panel in Panels)
                    {
                        AddEntriesToPanel(Panel);
                        Panel.ShowTextureOnScreen();
                        Panel.ShowPublicTextOnScreen();
                    }
                    Queue.Clear();
                }
                private void AddEntriesToPanel(IMyTextPanel Panel)
                {
                    if (Autoscroll)
                    {
                        List<string> buffer = new List<string>(Panel.GetPublicText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
                        buffer.AddRange(Queue);
                        int maxLines = Math.Min(getMaxLinesFromPanel(Panel), buffer.Count);
                        if (Prepend)
                        {
                            buffer.Reverse();
                        }
                        Panel.WritePublicText(string.Join("\n", buffer.GetRange(buffer.Count - maxLines, maxLines).ToArray()), false);
                    }
                    else
                    {
                        if (Prepend)
                        {
                            var buffer = new List<string>(Panel.GetPublicText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
                            buffer.AddRange(Queue);
                            buffer.Reverse();
                            Panel.WritePublicText(string.Join("\n", buffer.ToArray()), false);
                        }
                        else
                        {
                            Panel.WritePublicText(string.Join("\n", Queue.ToArray()), true);
                        }
                    }
                }
                private int getMaxLinesFromPanel(IMyTextPanel Panel)
                {
                    float fontSize = Panel.GetValueFloat("FontSize");
                    if (fontSize == 0.0f)
                    {
                        fontSize = 0.01f;
                    }
                    return Convert.ToInt32(Math.Ceiling(17.0f / fontSize));
                }
            }
            public class BMyKryptDebugSrvAppender : BMyAppenderBase
            {
                private IMyProgrammableBlock _debugSrv;
                private Queue<string> queue = new Queue<string>();
                public BMyKryptDebugSrvAppender(Program Assembly)
                {
                    _debugSrv = Assembly.GridTerminalSystem.GetBlockWithName("DebugSrv") as IMyProgrammableBlock;
                }
                public override void Flush()
                {
                    if (_debugSrv != null)
                    {
                        bool proceed = true;
                        while (proceed && queue.Count > 0)
                        {
                            if (_debugSrv.TryRun("L" + queue.Peek()))
                            {
                                queue.Dequeue();
                            }
                            else
                            {
                                proceed = false;
                            }
                        }
                    }
                }
                public override void Enqueue(string message)
                {
                    queue.Enqueue(message);
                }
            }
            public class BMyEchoAppender : BMyAppenderBase
            {
                private Program Assembly;

                public BMyEchoAppender(Program Assembly)
                {
                    this.Assembly = Assembly;
                }

                public override void Flush() { }

                public override void Enqueue(string message)
                {
                    Assembly.Echo(message);
                }
            }
            public class BMyCustomDataAppender : BMyAppenderBase
            {
                Program Assembly;
                public BMyCustomDataAppender(Program Assembly)
                {
                    this.Assembly = Assembly;
                    this.Assembly.Me.CustomData = "";
                }
                public override void Enqueue(string message)
                {
                    Assembly.Me.CustomData = Assembly.Me.CustomData + '\n' + message;
                }
                public override void Flush()
                {

                }
            }

            public abstract class BMyAppenderBase
            {
                public abstract void Enqueue(string message);
                public abstract void Flush();
            }
        }

        #endregion BMyLog4PB

        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}