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

namespace BaconDraw
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        BaconVectorDrawLib
        ==============
        Copyright 2016 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE

        Description
        ===========

        */
        string defaultTag = "[BaconDotmatrix]";

        public void Main(string argument)
        {
            BaconDotmatrix BDM = new BaconDotmatrix();
            BaconArgs.Parser BP = new BaconArgs.Parser();
            BaconArgs.Bag Args = BP.parseArgs(argument);
            List<string> Tags = Args.getArguments();
            if(Tags.Count > 0)
            {
                for(int i = 0; i < Tags.Count; i++)
                {
                    BDM.updatePanels(Tags[i], GridTerminalSystem, Me.CubeGrid);
                }
            } else
            {
                BDM.updatePanels(defaultTag, GridTerminalSystem, Me.CubeGrid);
            }
        }

        class BaconDotmatrix {

            public void updatePanels(string tag, IMyGridTerminalSystem GTS, IMyCubeGrid CG)
            {
                List<IMyTextPanel> Panels = new List<IMyTextPanel>();
                GTS.GetBlocksOfType<IMyTextPanel>(Panels, (x => x.CustomName.Contains(tag) && x.CubeGrid.Equals(CG)));
                VectorScriptParser VSP = new VectorScriptParser();
                Draw draw = new Draw();
                BaconArgs.Parser BAP = new BaconArgs.Parser();
                for (int i = 0; i < Panels.Count; i++)
                {
                    string[] script = Panels[i].GetPrivateText().Split(new char[] { '\n','\r'}, StringSplitOptions.RemoveEmptyEntries);
                    if (script.Length > 1)
                    {
                        BaconArgs.Bag Args = BAP.parseArgs(script[0]);
                        if((Args.getOption("width").Count > 0) && (Args.getOption("height").Count > 0))
                        {
                            int w;
                            int h;
                            if(int.TryParse(Args.getOption("width")[0], out w) && int.TryParse(Args.getOption("height")[0], out h))
                            {
                                Canvas canvas = null;
                                if (Args.getOption("bgcolor").Count > 0 && Args.getOption("bgcolor")[0].Length > 0 )
                                {
                                    canvas = new Canvas(w, h, Color.get(Args.getOption("bgcolor")[0]));
                                }
                                else
                                {
                                    canvas = new Canvas(w, h);
                                }

                                if (Args.getOption("color").Count > 0 && Args.getOption("color")[0].Length > 0)
                                {
                                    canvas.setColor(Color.get(Args.getOption("color")[0]));
                                }
                                for(int s = 1; s < script.Length; s++)
                                {
                                    VSP.parseLine(script[s], canvas, draw);
                                }
                                Panels[i].WritePublicText(canvas.ToString());
                            }                            
                        }
                    }
                }
            }

            public class VectorScriptParser
            {
                string defaultFontDefinition = @"! 2,0 2,1 2,2 2,4 "" 1,0 3,0 1,1 3,1  # 1,0 3,0 0,1 1,1 2,1 3,1 4,1 1,2 3,2 0,3 1,3 2,3 3,3 4,3 1,4 3,4 $ 1,0 2,0 3,0 4,0 0,1 2,1 1,2 2,2 3,2 2,3 4,3 0,4 1,4 2,4 3,4 % 0,0 1,0 4,0 0,1 1,1 3,1 2,2 1,3 3,3 4,3 0,4 3,4 4,4 ' 2,0 2,1 ( 3,0 2,1 2,3 2,3 3,4 ) 1,0 2,1 2,2 2,3 1,4 * 1,0 3,0 2,1 1,2 3,2 , 2,3 1,4 - 1,2 2,2 3,2 . 2,4 / 4,0 3,1 2,2 1,3 0,4 0 1,0 2,0 3,0 0,1 3,1 4,1 0,2 2,2 4,2 0,3 1,3 4,3 1,4 2,4 3,4 1 2,0 3,0 3,1 3,2 3,3 2,4 3,4 4,4 2 0,0 1,0 2,0 3,0 4,1 1,2 2,2 3,2 0,3 0,4 1,4 2,4 3,4 4,4 3 0,0 1,0 2,0 3,0 4,1 2,2 3,2 4,3 0,4 1,4 2,4 3,4 4 2,0 3,0 1,1 3,1 0,2 3,2 0,3 1,3 2,3 3,3 4,3 3,4 5 0,0 1,0 2,0 3,3 4,0 0,1 0,2 1,2 2,2 3,3 4,3 0,4 1,4 2,4 3,4 6 1,0 2,0 3,0 4,0 0,1 0,2 1,2 2,2 3,2 0,3 4,1 1,4 2,4 3,4 7 0,0 1,0 2,0 3,0 4,0 4,1 3,2 2,3 1,4 8 1,0 2,0 3,0 0,1 4,1 1,2 2,2 3,2 0,3 4,3 1,4 2,4 3,4 9 1,0 2,0 3,0 0,1 4,1 1,2 2,2 3,2 4,2 4,3 0,4 1,4 2,4 3,4 : 2,1 2,3 ; 2,1 2,3 1,4 < 4,0 3,1 2,2 3,3 4,4 = 1,1 2,1 3,1 1,3 2,3 3,3 > 0,0 1,1 2,2 1,3 0,4 ? 1,0 2,0 3,0 4,1 2,2 3,2 2,4 @ 1,0 2,0 3,0 4,0 0,1 4,1 0,2 2,2 3,2 4,2 0,3 2,3 3,3 1,4 2,4 3,4 4,4 A 2,0 1,1 3,1 0,2 4,2 0,3 1,3 2,3 3,3 4,3 0,4 4,4 B 0,0 1,0 2,0 3,0 0,1 4,1 0,2 1,2 2,2 3,2 0,3 4,3 0,4 1,4 2,4 3,4 C 1,0 2,0 3,0 4,0 0,1 0,2 0,3 1,4 2,4 3,4 4,4 D 0,0 1,0 2,0 3,0 0,1 4,1 0,2 4,1 0,3 4,3 0,4 1,4 2,4 3,4 E 0,0 1,0 2,0 3,0 4,0 0,1 0,2 1,2 2,2 0,3 0,4 1,4 2,4 3,4 4,4 F 0,0 1,0 2,0 3,0 4,0 0,1 0,2 1,2 2,2 0,3 0,4 G 1,0 2,0 3,0 4,0 0,1 0,2 3,2 4,2 0,3 4,3 1,4 2,4 3,4 4,4 H 0,0 4,0 0,1 4,1 0,2 1,2 2,2 3,2 4,2 0,3 4,3 0,4 4,4 I 0,0 1,0 2,0 3,0 4,0 2,1 2,2 2,3 0,4 1,4 2,4 3,4 4,4 J 0,0 1,0 2,0 3,0 4,0 3,1 3,2 0,3 3,3 1,4 2,4 K 0,0 4,0 0,1 3,1 0,2 1,2 2,2 0,3 3,3 0,4 4,4 L 0,0 0,1 0,2 0,3 0,4 1,4 2,4 3,4 4,4 M 0,0 4,0 0,1 1,1 3,1 4,1 0,2 2,2 4,2 0,3 4,3 0,4 4,4 N 0,0 4,0 0,1 1,1 4,1 0,2 2,2 4,2 0,3 3,3 4,3 0,4 4,4 O 1,0 2,0 3,0 0,1 4,1 0,2 4,2 0,3 4,3 1,4 2,4 3,4 P 0,0 1,0 2,0 3,0 0,1 4,1 0,2 1,2 2,2 3,2 0,3 0,4 Q 1,0 2,0 3,0 0,1 4,1 0,2 2,2 4,2 0,3 3,3 1,4 2,4 4,4 R 0,0 1,0 2,0 3,0 0,1 4,1 0,2 1,2 2,2 3,2 0,3 4,3 0,4 4,4 S 1,0 2,0 3,0 4,0 0,1 1,2 2,2 3,2 4,3 0,4 1,4 2,4 3,4 T 0,0 1,0 2,0 3,0 4,0 2,1 2,2 2,3 2,4 U 0,0 4,0 0,1 4,1 0,2 4,2 0,3 4,3 1,4 2,4 3,4 V 0,0 4,0 0,1 4,1 0,2 4,2 1,3 3,3 2,4 W 0,0 4,0 0,1 4,1 0,2 2,2 4,2 0,3 1,3 3,3 4,3 0,4 4,4 X 0,0 4,0 1,1 3,1 2,2 1,3 3,3 0,4 4,4  Y 0,0 4,0 1,1 3,1 2,2 2,3 2,4 Z 0,0 1,0 2,0 3,0 4,0 3,1 2,2 1,3 0,4 1,4 2,4 3,4 4,4 [ 2,0 3,0 2,1 2,2 2,3 2,4 3,4 \ 0,0 1,1 2,2 3,3 4,4 ] 1,0 2,0 2,1 2,2 2,3 1,4 2,4 ^ 2,0 1,1 3,1 _ 0,4 1,4 2,4 3,4 4,4 ` 2,0 3,1 { 2,0 3,0 2,1 1,2 2,3 2,4 3,4 | 2,0 2,1 2,2 2,3 2,4 } 1,0 2,0 2,1 3,2 2,3 1,4 2,4 ~ 1,1 0,2 2,2 4,2 3,3";
                private Font defaultFont = null;
                private Dictionary<string, Font> Fonts = new Dictionary<string, Font>();
                        

                public VectorScriptParser()
                {
                    defaultFont = this.parseFontFromByDefinition(defaultFontDefinition);
                }

                public void parseLine(string line, Canvas canvas, Draw draw)
                {
                    string[] a = line.Split(new char[] {' '}, 2);
                    string cmd = ((a.Length > 0) ? a[0] : "null").ToLower();
                    string args = (a.Length > 1) ? a[1] : "";
                    switch (cmd)
                    {
                        case "rect":
                            parseRectangle(args, canvas, draw);
                            break;
                        case "circle":
                            parseCircle(args, canvas, draw);
                            break;
                        case "color":
                            parseColor(args, canvas, draw);
                            break;
                        case "moveto":
                            parseMoveTo(args, canvas, draw);
                            break;
                        case "lineto":
                            parseLineTo(args, canvas, draw);
                            break;
                        case "text":
                            parseText(args, canvas, draw);
                            break;
                        case "font":
                            parseFont(args);
                            break;
                        default:
                            break;
                    }
                }

                private Font getFontByName(string name)
                {
                    return Fonts.ContainsKey(name) ? Fonts[name] : defaultFont;
                }

                private void parseFont(string line)
                {
                    string[] argv = line.Split(new char[] {' '}, 2);
                    if (argv.Length == 2 && !Fonts.ContainsKey(argv[0]))
                    {
                        Font tmp = parseFontFromByDefinition(argv[1]);
                        if(tmp != null)
                        {
                            Fonts.Add(argv[0], tmp);
                        }                        
                    }
                }

                public Font parseFontFromByDefinition(string defintion)
                {
                    string[] argv = defintion.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (argv.Length > 0)
                    {
                        Font font = new Font();
                        System.Text.RegularExpressions.Regex pointRgx = new System.Text.RegularExpressions.Regex(@"\d+,\d+");
                        List<Point> PointSlug = new List<Point>();
                        char glyph = '\0';
                        for (int i = 1; i < argv.Length; i++)
                        {
                            string tmpArg = argv[i];
                            if (tmpArg.Length == 1) //glyph
                            {
                                if (!glyph.Equals('\0') && PointSlug.Count > 0)
                                {
                                    font.addGlyph(glyph, PointSlug);
                                    glyph = tmpArg[0];
                                    PointSlug = new List<Point>();
                                }
                            }
                            else if (pointRgx.IsMatch(tmpArg)) //point
                            {
                                string[] points = tmpArg.Split(',');
                                if (points.Length == 2)
                                {
                                    int x = 0;
                                    int y = 0;
                                    if (int.TryParse(points[0], out x) && int.TryParse(points[1], out y))
                                    {
                                        PointSlug.Add(new Point(x, y));
                                    }
                                }
                            }
                        }
                        font.addGlyph(' ', new List<Point>()); // 'space'
                        return font;
                    } else
                    {
                        return null;
                    }
                }

                private void parseText(string args, Canvas canvas, Draw draw)
                {
                    string[] argv = args.Split(new char[] {' '}, 2);
                    if (argv.Length > 1)
                    {
                        Font tmpF = getFontByName(argv[0]);
                        if (tmpF != null)
                        {
                            draw.text(argv[1], tmpF, canvas);
                        }
                    }
                }

                private void parseRectangle(string args, Canvas canvas, Draw draw)
                {
                    Point P;
                    if (tryParsePointFormArg(args, out P))
                    {
                        Point O = canvas.getPos();
                        draw.lineTo(new Point(P.X,O.Y), canvas);
                        draw.lineTo(new Point(P.X, P.Y), canvas);
                        draw.lineTo(new Point(O.X, P.Y), canvas);
                        draw.lineTo(new Point(O.X, O.Y), canvas);
                        draw.moveTo(P, canvas);
                    }
                }

                private void parseCircle(string args, Canvas canvas, Draw draw)
                {
                    int radius = 0;
                    if (int.TryParse(args.Trim(), out radius))
                    {
                        draw.circle(radius, canvas);
                    }
                }

                private void parseColor(string args, Canvas canvas, Draw draw)
                {
                    draw.color(args, canvas);
                }

                private void parseMoveTo(string args, Canvas canvas, Draw draw)
                {
                    Point P;
                    if(tryParsePointFormArg(args, out P))
                    {
                        draw.moveTo(P, canvas);
                    }
                }

                private void parseLineTo(string args, Canvas canvas, Draw draw)
                {
                    Point P;
                    if (tryParsePointFormArg(args, out P))
                    {
                        draw.lineTo(P, canvas);
                    }
                }

                private bool tryParsePointFormArg(string args, out Point P)
                {
                    string[] argv = args.Split(new char[] { ',','/' }, StringSplitOptions.RemoveEmptyEntries);
                    P = new Point(0, 0);
                    if(argv.Length > 1)
                    {
                        int x;
                        int y;
                        if(int.TryParse(argv[0], out x) && int.TryParse(argv[1], out y))
                        {
                            P = new Point(x,y);
                            return true;
                        }
                    }
                    return false;
                }
            }

            public class Draw
            {
                public void text(string text, Font font, Canvas canvas)
                {
                    int offsetX = canvas.getPos().X;
                    int offsetY = canvas.getPos().Y;
                    for (int i = 0; i < text.Length; i++) {
                        char curChar = text[i];
                        List<Point> slug = font.getPoints(curChar);
                        if(slug != null)
                        {
                            for(int p = 0; p < slug.Count; p++) {
                                canvas.add(new Point(offsetX+slug[p].X, offsetY + slug[p].Y));
                            }
                        }
                        offsetX = offsetX + font.getWidth();
                    }
                    moveTo(new Point(offsetX, offsetY+font.getHeight()), canvas);
                }

                public void circle(int r, Canvas canvas)
                {
                    int d;
                    int x = r;
                    d = r * -1;
                    for (int y=0;y<=x;y++)
                    {
                        setSymetric(x,y,canvas);
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
                    Point origin = canvas.getPos();
                    int ox = origin.X;
                    int oy = origin.Y;
                    canvas.add(new Point(x+ox,y+oy), false);
                    canvas.add(new Point(y+oy,x+ox), false);
                    canvas.add(new Point(y+oy,x*-1+ox), false);
                    canvas.add(new Point(x+ox,y*-1+oy), false);
                    canvas.add(new Point(x*-1+ox,y*-1+oy), false);
                    canvas.add(new Point(y*-1+oy,x*-1+ox), false);
                    canvas.add(new Point(y*-1+oy,x+ox), false);
                    canvas.add(new Point(x*-1+ox,y+oy), false);
                }

                public void color(string color, Canvas canvas)
                {
                    canvas.setColor(Color.get(color));
                }

                public void moveTo(Point P, Canvas canvas)
                {
                    canvas.setPos(P);
                }

                public void lineTo(Point target, Canvas canvas)
                {
                    int x, y, t, deltaX, deltaY, incrementX, incrementY, pdx, pdy, ddx, ddy, es, el, err;
                    Point origin = canvas.getPos();
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
                    canvas.add(new Point(x, y));

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
                        canvas.add(new Point(x, y));
                    }
                }
            }

            public class Canvas
            {
                private Point pos = new Point(0, 0);
                private int width;
                private int height;
                private char color = '1';

                private Char[][] data; // h*x > y,x

                public Canvas(int w, int h) : this(w, h, '\uE00E')
                {
                }

                public Canvas(int w, int h, char bgColor)
                {
                    char bg = (bgColor.Equals(Color.NULL)) ? '\uE00E' : bgColor;

                    width = w;
                    height = h;
                    data = new Char[h][];
                    for (int i = 0; i < h; i++)
                    {
                        data[i] = (new String(bg, w)).ToCharArray();
                    }
                }

                public void setColor(char c)
                {
                    char cl = (c.Equals(Color.NULL)) ? '\uE00F' : c;
                    color = cl;
                }

                public Point getPos()
                {
                    return pos;
                }

                public int getWidth()
                {
                    return width;
                }

                public int getHeight()
                {
                    return height;
                }

                public void add(Point P)
                {
                    add(P, true);
                }

                public void add(Point p, bool move)
                {
                    if (p.X >= 0 && p.Y >= 0 && p.X < width && p.Y < height)
                    {
                        data[p.Y][p.X] = color;
                        if (move)
                        {
                            setPos(p);
                        }
                    }
                }

                public void setPos(Point P)
                {
                    this.pos = P;
                }

                public override string ToString()
                {
                    StringBuilder slug = new StringBuilder();
                    for (int h = 0; h < data.Length; h++)
                    {
                        slug.AppendLine(new String(data[h]));
                    }
                    return slug.ToString();
                }
            }

            public class Font
            {
                private Dictionary<char, List<Point>> glyphMap = new Dictionary<char, List<Point>>();
                private List<Point> unknownChar = null;
                private int width = 0;
                private int height = 0;

                public void addGlyph(char glyph, List<Point> Points)
                {
                    if (!glyphMap.ContainsKey(glyph))
                    {
                        for(int i = 0; i < Points.Count; i++)
                        {
                            width = Math.Max(Points[i].X, width) +1;
                            height = Math.Max(Points[i].Y, height) +1;
                            glyphMap.Add(glyph, Points);
                        }
                    }
                }        
                
                public int getHeight()
                {
                    return height;
                }        

                public int getWidth()
                {
                    return width;
                }

                public List<Point> getPoints(char glyph)
                {
                    return glyphMap.ContainsKey(glyph) ? glyphMap[glyph] : getUnknownChar();
                }

                private List<Point> getUnknownChar()
                {
                    if(unknownChar == null)
                    {
                        unknownChar = new List<Point>();
                        for(int x = 1; x < width - 1; x++)
                        {
                            unknownChar.Add(new Point(x,1));
                            unknownChar.Add(new Point(x, height-2));
                        }
                        for(int y = 2; y < height - 2; y++)
                        {
                            unknownChar.Add(new Point(1, y));
                            unknownChar.Add(new Point(width-2, y));
                        }
                    }
                    return unknownChar;
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

                public const char NULL = '\0';

                static public char get(string col)
                {
                    char key = (col.Length > 0) ? col.ToLower()[0] : NULL;
                    return (map.ContainsKey(key)) ? map[key] : NULL;
                }
            }
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