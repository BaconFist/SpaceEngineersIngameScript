using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // In order to add a new utility class, right-click on your project, 
        // select 'New' then 'Add Item...'. Now find the 'Space Engineers'
        // category under 'Visual C# Items' on the left hand side, and select
        // 'Utility Class' in the main area. Name it in the box below, and
        // press OK. This utility class will be merged in with your code when
        // deploying your final script.
        //
        // You can also simply create a new utility class manually, you don't
        // have to use the template if you don't want to. Just do so the first
        // time to see what a utility class looks like.

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


            BMyLog4PB Log = new BMyLog4PB(this, BMyLog4PB.E_ALL, new BMyLog4PB.BMyTextPanelAppender("DrawLog", this));
            Log.AutoFlush = true;
            
            IMyTextPanel panel = GridTerminalSystem.GetBlockWithName("DrawPanel") as IMyTextPanel;
            if(panel == null)
            {
                throw new Exception("Panel \"DrawPanel\" not found");
            }

            string[] code = panel?.CustomData.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            Log?.PushStack("BaconDraw");
            string image = (new BaconDraw()).run(code, Log).ToString();
            Log.PopStack();
            panel?.WritePublicText(image);
            panel?.ShowPublicTextOnScreen();
            
        }

        class BaconDraw
        {
            public Canvas run(string[] source, BMyLog4PB Log)
            {
                Log?.PushStack(@"BaconDraw.run");

                Log?.IfDebug?.Debug("creating environment");
                #region Environment
                Environment Env = new Environment(Log);
                #endregion Environment
                
                #region build plugins
                // prepare pluginhandlers
                Env.DrawPlugins.AddPlugin(new DrawPlugin_Background()); // "background R,G,B" where R G B is 0-7
                Env.DrawPlugins.AddPlugin(new DrawPlugin_Circle()); // "circle RADIUS" where RADIUS is integer
                Env.DrawPlugins.AddPlugin(new DrawPlugin_LineTo()); // "lineto x,y" where x y is integer
                Env.DrawPlugins.AddPlugin(new DrawPlugin_MoveTo()); // "moveto x,y" where x y is integer
                Env.DrawPlugins.AddPlugin(new DrawPlugin_Rect()); // "rect x,y" where x y is integer
                Env.DrawPlugins.AddPlugin(new DrawPlugin_Polygon()); // "poly x,y x,y x,y" where x y is integer
                Env.DrawPlugins.AddPlugin(new DrawPlugin_Color()); // "color R,G,B" where R G B is 0-7
                #endregion build plugins
                Log?.IfDebug?.Debug(@"Plugins loaded: {0}", string.Join(",", Env.DrawPlugins.Values.ToList().ConvertAll<string>(L => string.Join(",", L.ConvertAll<string>(P => string.Format(@"{0}.{1}", P.Vendor, P.Name)).ToArray())).ToArray()));
                
                #region build code queue
                Queue<Command> codeQueue = new Queue<Command>();
                foreach (string currentLine in source)
                {
                    codeQueue.Enqueue(new Command(currentLine));
                }
                #endregion build code queue
                Log?.IfDebug?.Debug(@"build code queue with {0} commands", codeQueue.Count);

                #region make canvas
                Canvas canvas = new Canvas(50, 50, new Color(0, 0, 0));
                #endregion make canvas
                Log?.IfDebug?.Debug(@"created canvas {0}x{1}", canvas.Width, canvas.Height);

                #region progress codeQueue
                while (codeQueue.Count > 0)
                {
                    // break if over limits
                    Command command = codeQueue.Dequeue();
                    if (Env.TryRunDraw(command, canvas))
                    {
                        // success
                        Log?.IfDebug?.Debug(@"sucessfully run command ""{0} {1}"" ", command.Key, command.Args);
                    }
                    else
                    {
                        // failed
                        Log?.IfDebug?.Debug(@"failed at command ""{0} {1}"" ", command.Key, command.Args);
                    }
                }

                Log?.PopStack();
                return canvas;
                #endregion progress codeQueue
            }

            #region pluginsystem
            class PluginBag<T> : Dictionary<string, List<T>> where T : Plugin
            {
                private Environment Env;

                public PluginBag(Environment Env)
                {
                    this.Env = Env;
                }

                public void AddPlugin(T Plugin)
                {
                    Env.Log?.PushStack(@"PluginBag.AddPlugin(T Plugin)");
                    if (!ContainsKey(Plugin.Name))
                    {
                        Add(Plugin.Name, new List<T>());
                    }
                    this[Plugin.Name].Add(Plugin);
                    Env.Log?.PopStack();
                }

                public List<T> FindAllByName(string name)
                {
                    Env.Log?.PushStack("PluginBag<T>.FindAllByName");
                    List<T> buffer = ContainsKey(name) ? this[name] : new List<T>();
                    Env.Log?.IfDebug?.Debug(@"Found {0} plugins for {1}", buffer.Count, name);
                    Env.Log?.PopStack();
                    return buffer;
                }
            }

            abstract class Plugin
            {
                abstract public string Name { get; }
                abstract public string Vendor { get; }
            }

            abstract class DrawPlugin : Plugin
            {
                abstract public bool TryRun(Command command, Canvas canvas, Environment Env);
            }
            #endregion pluginsystem

            #region draw plugins
            class DrawPlugin_Background : DrawPlugin
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


                public override bool TryRun(Command command, Canvas canvas, Environment Env)
                {
                    Env.Log?.PushStack("DrawPlugin_Background.TryRun");
                    bool success;
                    byte r=0;
                    byte g=0;
                    byte b=0;
                    string[] argv = command.Args.Split(new char[] { ',' });

                    if (argv.Length == 3
                        && byte.TryParse(argv[0], out r)
                        && byte.TryParse(argv[1], out g)
                        && byte.TryParse(argv[2], out b)
                    )
                    {
                        canvas = new Canvas(canvas.Width, canvas.Height, new Color(r, g, b));
                        success = true;
                        Env.Log?.IfDebug?.Debug(@"background set to {0},{1},{2}", r,g,b);
                    }
                    else
                    {
                        success = false;
                        Env.Log?.IfDebug?.Debug(@"cant set background to {0},{1},{2}", r,g,b);
                    }
                    Env.Log?.PopStack();
                    return success;
                }
            }
            class DrawPlugin_MoveTo : DrawPlugin
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

                public override bool TryRun(Command command, Canvas canvas, Environment Env)
                {
                    Env.Log?.PushStack("DrawPlugin_MoveTo.TryRun");
                    bool success;
                    string[] argv = command.Args.Split(new char[] { ',' });
                    int x=0;
                    int y=0;
                    if (
                        argv.Length == 2
                        && int.TryParse(argv[0], out x)
                        && int.TryParse(argv[1], out y)
                        )
                    {
                        canvas.Position = new Point(x, y);
                        success = (canvas.Position.X == x && canvas.Position.Y == y);
                        Env.Log?.IfDebug.Debug(@"moved pencil to {0},{1}", canvas.Position.X, canvas.Position.Y);
                    }
                    else
                    {
                        success = false;
                        Env.Log?.IfDebug.Debug(@"cant move to {0},{1}", x,y);
                    }
                    Env.Log?.PopStack();
                    return success;
                }
            }
            class DrawPlugin_LineTo : DrawPlugin
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

                public override bool TryRun(Command command, Canvas canvas, Environment Env)
                {
                    Env.Log?.PushStack("DrawPlugin_LineTo.TryRun");
                    bool success;
                    string[] argv = command.Args.Split(new char[] { ',' });
                    int x=0;
                    int y=0;
                    if (
                        argv.Length == 2
                        && int.TryParse(argv[0], out x)
                        && int.TryParse(argv[1], out y)
                        )
                    {
                        Point target = new Point(x, y);
                        lineTo(target, canvas);
                        success = true;
                        Env.Log?.IfDebug?.Debug(@"draw line to {0},{1}", x,y);
                    }
                    else
                    {
                        success = false;
                        Env.Log?.IfDebug?.Debug(@"cant draw line to {0},{1}", x, y);
                    }
                    Env.Log?.PopStack();
                    return success;
                }

                private void lineTo(Point target, Canvas canvas)
                {
                    int x, y, t, deltaX, deltaY, incrementX, incrementY, pdx, pdy, ddx, ddy, es, el, err;
                    Point origin = canvas.Position;
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
            class DrawPlugin_Circle : DrawPlugin
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

                public override bool TryRun(Command command, Canvas canvas, Environment Env)
                {
                    Env.Log?.PushStack("DrawPlugin_Circle.TryRun");
                    bool success;
                    int r;
                    if (int.TryParse(command.Args, out r))
                    {
                        circle(r, canvas);
                        success = true;
                        Env.Log?.IfDebug?.Debug(@"draw circle at position {0},{1} with a radius of {2}", canvas.Position.X, canvas.Position.Y, r);
                    }
                    else
                    {
                        success = false;
                        Env.Log?.IfDebug?.Debug(@"can't draw circle at position {0},{1} with a radius of {2}", canvas.Position.X, canvas.Position.Y, r);
                    }
                    Env.Log?.PopStack();
                    return success;
                }

                public void circle(int r, Canvas canvas)
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

                private void setSymetric(int x, int y, Canvas canvas)
                {
                    Point o = canvas.Position;

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
            class DrawPlugin_Rect : DrawPlugin
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

                public override bool TryRun(Command command, Canvas canvas, Environment Env)
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
                        Point origin = canvas.Position;
                        Command linetoCommand = new Command(string.Format(@"lineto {0},{1}", x, origin.Y));
                        if (Env.TryRunDraw(linetoCommand, canvas))
                        {
                            linetoCommand = new Command(string.Format(@"lineto {0},{1}", x, y));
                            if (Env.TryRunDraw(linetoCommand, canvas))
                            {
                                linetoCommand = new Command(string.Format(@"lineto {0},{1}", origin.X, y));
                                if (Env.TryRunDraw(linetoCommand, canvas))
                                {
                                    linetoCommand = new Command(string.Format(@"lineto {0},{1}", origin.X, origin.Y));
                                    if (Env.TryRunDraw(linetoCommand, canvas))
                                    {
                                        success = true;
                                    }
                                }
                            }
                        }

                        canvas.Position = new Point(x, y);
                        return success;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            class DrawPlugin_Polygon : DrawPlugin
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

                public override bool TryRun(Command command, Canvas canvas, Environment Env)
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
                            success = success && Env.TryRunDraw(new Command(string.Format(@"lineto {0},{1}", x, y)), canvas);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    return success;
                }
            }
            class DrawPlugin_Color : DrawPlugin
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

                public override bool TryRun(Command command, Canvas canvas, Environment Env)
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
                        canvas.color = new Color(r, g, b);
                        return true;
                    }
                    return false;
                }
            }
            #endregion draw plugins

            class Environment
            {
                public readonly PluginBag<DrawPlugin> DrawPlugins;
                public BMyLog4PB Log;

                public Environment(BMyLog4PB Log)
                {
                    this.Log = Log;
                    DrawPlugins = new PluginBag<DrawPlugin>(this);
                }

                public bool TryRunDraw(Command command, Canvas canvas)
                {
                    List<DrawPlugin> DrawPluginsForCommand = DrawPlugins.FindAllByName(command.Key);
                    bool successfull = false;
                    if (DrawPluginsForCommand.Count > 0)
                    {
                        for (int i = 0; i < DrawPluginsForCommand.Count && !successfull; i++)
                        {
                            successfull = DrawPluginsForCommand[i].TryRun(command, canvas, this);
                        }
                    }
                    return successfull;
                }
            }

            class Command
            {
                public string Key;
                public string Args;

                public Command(string cmd)
                {
                    string[] slug = cmd.Split(new Char[] { ' ' }, 2);
                    Key = slug[0];
                    Args = (slug.Length == 2) ? slug[1] : "";
                }
            }
            public class Color
            {
                public byte R;
                public byte G;
                public byte B;

                public Color(byte red, byte green, byte blue)
                {
                    R = Math.Min(red, (byte)7);
                    G = Math.Min(green, (byte)7);
                    B = Math.Min(blue, (byte)7);
                }

                public char ToChar()
                {
                    return (char)(0xe100 + (R << 6) + (G << 3) + B);
                }
            }
            public class Point
            {
                public int X;
                public int Y;
                public Point(int X, int Y)
                {
                    this.X = X;
                    this.Y = Y;
                }
            }
            public class Canvas
            {
                private char[][] _raster;
                public readonly int Width;
                public readonly int Height;
                public Color color = new Color(255, 255, 255);
                private Point _pos = new Point(0, 0);

                public Point Position
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

                public Canvas(int width, int height, Color background)
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
                public bool TrySetPixel(int x, int y, bool moveto = false)
                {
                    if (isInBounds(x, y))
                    {
                        _raster[x][y] = color.ToChar();
                        if (moveto)
                        {
                            Position = new Point(x, y);
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

        public class BMyLog4PB { public const byte E_ALL = 63; public const byte E_TRACE = 32; public const byte E_DEBUG = 16; public const byte E_INFO = 8; public const byte E_WARN = 4; public const byte E_ERROR = 2; public const byte E_FATAL = 1; public BMyLog4PB IfFatal { get { return If(E_FATAL); } } public BMyLog4PB IfError { get { return If(E_ERROR); } } public BMyLog4PB IfWarn { get { return If(E_WARN); } } public BMyLog4PB IfInfo { get { return If(E_INFO); } } public BMyLog4PB IfDebug { get { return If(E_DEBUG); } } public BMyLog4PB IfTrace { get { return If(E_TRACE); } } Dictionary<string, string> j = new Dictionary<string, string>() { { "{Date}", "{0}" }, { "{Time}", "{1}" }, { "{Milliseconds}", "{2}" }, { "{Severity}", "{3}" }, { "{CurrentInstructionCount}", "{4}" }, { "{MaxInstructionCount}", "{5}" }, { "{Message}", "{6}" }, { "{Stack}", "{7}" }, { "{Origin}", "{8}" } }; Stack<string> k = new Stack<string>(); public byte Filter; public readonly Dictionary<BMyAppenderBase, string> Appenders = new Dictionary<BMyAppenderBase, string>(); string l = @"[{0}-{1}/{2}][{3}][{4}/{5}][{8}][{7}] {6}"; string m = @"[{Date}-{Time}/{Milliseconds}][{Severity}][{CurrentInstructionCount}/{MaxInstructionCount}][{Origin}][{Stack}] {Message}"; public string Format { get { return m; } set { l = o(value); m = value; } } readonly Program n; public bool AutoFlush = true; public BMyLog4PB(Program a) : this(a, E_FATAL | E_ERROR | E_WARN | E_INFO, new BMyEchoAppender(a)) { } public BMyLog4PB(Program a, byte b, params BMyAppenderBase[] c) { Filter = b; this.n = a; foreach (var Appender in c) AddAppender(Appender); } string o(string a) { var b = a; foreach (var item in j) b = b?.Replace(item.Key, item.Value); return b; } public BMyLog4PB Flush() { foreach (var AppenderItem in Appenders) AppenderItem.Key.Flush(); return this; } public BMyLog4PB PushStack(string a) { k.Push(a); return this; } public string PopStack() { return (k.Count > 0) ? k.Pop() : null; } string p() { return (k.Count > 0) ? k.Peek() : null; } public string StackToString() { if (If(E_TRACE) != null) { string[] a = k.ToArray(); Array.Reverse(a); return string.Join(@"/", a); } else return p(); } public BMyLog4PB AddAppender(BMyAppenderBase a, string b = null) { if (!Appenders.ContainsKey(a)) Appenders.Add(a, o(b)); return this; } public BMyLog4PB If(byte a) { return ((a & Filter) != 0) ? this : null; } public BMyLog4PB Fatal(string a, params object[] b) { If(E_FATAL)?.q("FATAL", a, b); return this; } public BMyLog4PB Error(string a, params object[] b) { If(E_ERROR)?.q("ERROR", a, b); return this; } public BMyLog4PB Warn(string a, params object[] b) { If(E_WARN)?.q("WARN", a, b); return this; } public BMyLog4PB Info(string a, params object[] b) { If(E_INFO)?.q("INFO", a, b); return this; } public BMyLog4PB Debug(string a, params object[] b) { If(E_DEBUG)?.q("DEBUG", a, b); return this; } public BMyLog4PB Trace(string a, params object[] b) { If(E_TRACE)?.q("TRACE", a, b); return this; } void q(string a, string b, params object[] c) { DateTime d = DateTime.Now; r e = new r(d.ToShortDateString(), d.ToLongTimeString(), d.Millisecond.ToString(), a, n.Runtime.CurrentInstructionCount, n.Runtime.MaxInstructionCount, string.Format(b, c), StackToString(), n.Me.CustomName); foreach (var item in Appenders) { var f = (item.Value != null) ? item.Value : l; item.Key.Enqueue(e.ToString(f)); if (AutoFlush) item.Key.Flush(); } } class r { public string Date; public string Time; public string Milliseconds; public string Severity; public int CurrentInstructionCount; public int MaxInstructionCount; public string Message; public string Stack; public string Origin; public r(string a, string b, string c, string d, int e, int f, string g, string h, string i) { this.Date = a; this.Time = b; this.Milliseconds = c; this.Severity = d; this.CurrentInstructionCount = e; this.MaxInstructionCount = f; this.Message = g; this.Stack = h; this.Origin = i; } public override string ToString() { return ToString(@"{0},{1},{2},{3},{4},{5},{6},{7},{8}"); } public string ToString(string a) { return string.Format(a, Date, Time, Milliseconds, Severity, CurrentInstructionCount, MaxInstructionCount, Message, Stack, Origin); } } public class BMyTextPanelAppender : BMyAppenderBase { List<string> j = new List<string>(); List<IMyTextPanel> k = new List<IMyTextPanel>(); public bool Autoscroll = true; public bool Prepend = false; public BMyTextPanelAppender(string a, Program b) { b.GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(k, (c => c.CustomName.Contains(a))); } public override void Enqueue(string a) { j.Add(a); } public override void Flush() { foreach (var Panel in k) { l(Panel); Panel.ShowTextureOnScreen(); Panel.ShowPublicTextOnScreen(); } j.Clear(); } void l(IMyTextPanel a) { if (Autoscroll) { var b = new List<string>(a.GetPublicText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)); b.AddRange(j); int c = Math.Min(m(a), b.Count); if (Prepend) b.Reverse(); a.WritePublicText(string.Join("\n", b.GetRange(b.Count - c, c).ToArray()), false); } else if (Prepend) { var b = new List<string>(a.GetPublicText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)); b.AddRange(j); b.Reverse(); a.WritePublicText(string.Join("\n", b.ToArray()), false); } else { a.WritePublicText(string.Join("\n", j.ToArray()), true); } } int m(IMyTextPanel a) { float b = a.GetValueFloat("FontSize"); if (b == 0.0f) b = 0.01f; return Convert.ToInt32(Math.Ceiling(17.0f / b)); } } public class BMyKryptDebugSrvAppender : BMyAppenderBase { IMyProgrammableBlock j; Queue<string> k = new Queue<string>(); public BMyKryptDebugSrvAppender(Program a) { j = a.GridTerminalSystem.GetBlockWithName("DebugSrv") as IMyProgrammableBlock; } public override void Flush() { if (j != null) { var a = true; while (a && k.Count > 0) if (j.TryRun("L" + k.Peek())) { k.Dequeue(); } else { a = false; } } } public override void Enqueue(string a) { k.Enqueue(a); } } public class BMyEchoAppender : BMyAppenderBase { Program j; public BMyEchoAppender(Program a) { this.j = a; } public override void Flush() { } public override void Enqueue(string a) { j.Echo(a); } } public class BMyCustomDataAppender : BMyAppenderBase { Program j; public BMyCustomDataAppender(Program a) { this.j = a; this.j.Me.CustomData = ""; } public override void Enqueue(string a) { j.Me.CustomData = j.Me.CustomData + '\n' + a; } public override void Flush() { } } public abstract class BMyAppenderBase { public abstract void Enqueue(string a); public abstract void Flush(); } }
    }
}