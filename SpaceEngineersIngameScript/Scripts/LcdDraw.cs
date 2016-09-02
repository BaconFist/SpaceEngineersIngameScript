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

namespace LcdDraw
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        LcdDraw
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
            Console.WriteLine((new BaconArgs.Parser()).parseArgs(@"-abc --opt1=o1v1 --opt2=o2\ v1 --opt3_null arg1 arg\ 2 ""arg 3""").ToString());
        }
        

        public class BaconLcdDraw
        {
            public BaconLcdDraw(string tag, IMyGridTerminalSystem GTS)
            {
                List<IMyTextPanel> Panels = new List<IMyTextPanel>();
                GTS.GetBlocksOfType<IMyTextPanel>(Panels, (x => x.CustomName.Contains(tag) && x.CubeGrid.Equals(Me.CubeGrid)));
                for(int i=0;i<Panels.Count; i++)
                {

                }
            }

            class Image
            {
                private Parser Parser;
                private Dotmatrix Dotmatrix;
                private Canvas Canvas;

                public Image(IMyTextPanel Panel)
                {
                    Parser = new Parser();
                    Canvas = new Canvas(Panel);
                    
                }
            }

            class Parser
            {
                public Dotmatrix getMatrix(Canvas Canvas)
                {
                    
                }                
            }
            
            class Dotmatrix
            {
                private double pixelTolerance = 0.1;

                private int width;
                private int height;
                private char[][] matrixYX;
                private char currentColor;
                private Point cursor;

                public Dotmatrix(int width, int height, string background)
                {
                    this.width = width;
                    this.height = height;
                    color(background);
                    fillAll();
                    moveTo(new Point(0, 0));
                }

                public StringBuilder getImage()
                {
                    StringBuilder content = new StringBuilder();
                    for (int i = 0; i < matrixYX.Length; i++)
                    {
                        content.AppendLine(new String(matrixYX[i]));
                    }

                    return content;
                }

                public Dotmatrix fillAll()
                {
                    matrixYX = new char[getHeight()][];
                    for (int i = 0; i < getHeight(); i++)
                    {
                        matrixYX[i] = (new String(currentColor, getWidth())).ToCharArray();
                    }

                    return this;
                }

                public int getWidth()
                {
                    return width;
                }

                private int getXMax()
                {
                    return getWidth() - 1;
                }

                private int getYMax()
                {
                    return getHeight() - 1;
                }

                public int getHeight()
                {
                    return height;
                }

                private void setCursor(Point point)
                {
                    cursor = point;
                }

                private void setPixel(Point point)
                {
                    if (isPointInViewport(point))
                    {
                        matrixYX[point.Y][point.X] = currentColor;
                    }
                }

                public Dotmatrix dot(Point point)
                {
                    setPixel(point);
                    return this;
                }

                public Dotmatrix moveTo(int x, int y)
                {
                    return moveTo(new Point(x, y));
                }

                public Dotmatrix moveTo(Point point)
                {
                    setCursor(point);
                    return this;
                }

                public Dotmatrix lineTo(int x, int y)
                {
                    return lineTo(new Point(x, y));
                }

                public Dotmatrix lineTo(Point point)
                {
                    Point origin = cursor;
                    Point target = point;

                    int xLow = System.Math.Min(origin.X, target.X);
                    int xHight = System.Math.Max(origin.X, target.X);
                    int yLow = System.Math.Min(origin.Y, target.Y);
                    int yHight = System.Math.Max(origin.Y, target.Y);

                    xLow = (xLow < 0) ? 0 : xLow;
                    yLow = (yLow < 0) ? 0 : yLow;
                    yHight = (yHight < matrixYX.Length) ? yHight : (matrixYX.Length - 1);

                    for (int iY = yLow; iY <= yHight; iY++)
                    {
                        xHight = (xHight < matrixYX[iY].Length) ? xHight : (matrixYX[iY].Length - 1);
                        for (int iX = xLow; iX <= xHight; iX++)
                        {
                            Point dot = new Point(iX, iY);
                            if (isPointOnVector(dot, origin, target))
                            {
                                this.dot(dot);
                            }
                        }
                    }

                    moveTo(target);
                    return this;
                }

                public Dotmatrix color(string color)
                {
                    currentColor = Color.get(color);
                    return this;
                }

                private bool isPointInViewport(Point point)
                {
                    if (0 <= point.Y && point.Y < matrixYX.Length)
                    {
                        if (0 <= point.X && point.X < matrixYX[point.Y].Length)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                private bool isPointOnVector(Point P, Point A, Point B)
                {
                    if (P.Equals(A) || P.Equals(B))
                    {
                        return true;
                    }

                    int diffABX = B.X - A.X;
                    int diffABY = B.Y - A.Y;
                    int diffPAX = P.X - A.X;
                    int diffPAY = P.Y - A.Y;

                    if (diffABX == 0)
                    {
                        return A.X <= P.X && P.X <= B.X;
                    }
                    if (diffABY == 0)
                    {
                        return A.Y <= P.Y && P.Y <= B.Y;
                    }

                    double eqX = (double)diffPAX / (double)diffABX;
                    double eqY = (double)diffPAY / (double)diffABY;
                    double diffEq = System.Math.Max(eqX, eqY) - System.Math.Min(eqX, eqY);

                    return (diffEq <= pixelTolerance);
                }
            }

            public class Canvas
            {
                protected IMyTextPanel Panel = null;
                protected Point Pos = new Point(0,0);
                protected char color = '\0';
                protected int height = 0;
                protected int width = 0;

                public Canvas(IMyTextPanel Panel)
                {
                    this.Panel = Panel;
                    string[] stat = Panel.GetPrivateTitle().Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if(stat.Length == 5)
                    {
                        int w;
                        int h;
                        int x;
                        int y;
                        if(
                            int.TryParse(stat[0], out w)
                            && int.TryParse(stat[1], out h)
                            && int.TryParse(stat[2], out x)
                            && int.TryParse(stat[3], out y)
                            )
                        {
                            Pos = new Point(x, y);
                            height = h;
                            width = h;
                            color = Color.get(stat[4]);
                        }
                    }
                }

                public Point getPos()
                {
                    return Pos;
                }

                public char getColor()
                {
                    return color;
                }

                public IMyTextPanel getPanel()
                {
                    return Panel;
                }

                public void saveStat(Point Pos, char color)
                {
                    Panel.WritePrivateTitle(width.ToString() + ":" + height.ToString() + ":" + Pos.X.ToString() + ":" + Pos.Y.ToString() + ":" + color);
                }
            }

            public class Color
            {
                static protected Dictionary<char, char> map = new Dictionary<char, char>() {
                    {'g','\uE001'},
                    {'b','\uE002'},
                    {'r','\uE003'},
                    {'y','\uE004'},
                    {'w','\uE006'},
                    {'l','\uE00E'},
                    {'m','\uE00D'},
                    {'d','\uE00F'},
                };

                static public char get(string col)
                {
                    char key = (col.Length > 0) ? col.ToLower()[0] : ' ';
                    return (map.ContainsKey(key)) ? map[key] : key;
                }
            }
        }

        public class BaconArgs { 
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
                            } else {
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