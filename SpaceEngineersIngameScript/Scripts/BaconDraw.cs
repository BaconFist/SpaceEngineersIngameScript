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
        BaconDraw
        ==============
        Copyright 2016 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE

        Description
        ===========

        */
        string defaultTag = "[BaconDotmatrix]";
        const bool DEBUG = false;

        public void Main(string argument)
        {
            BaconDebug debug = new BaconDebug("[BD]", GridTerminalSystem, this, DEBUG);

            BaconDotmatrix BDM = new BaconDotmatrix();
            BaconArgs.Parser BP = new BaconArgs.Parser();
            BaconArgs.Bag Args = BP.parseArgs(argument);
            List<string> Tags = Args.getArguments();
            if(Tags.Count > 0)
            {
                for(int i = 0; i < Tags.Count; i++)
                {
                    BDM.updatePanels(Tags[i], GridTerminalSystem, Me.CubeGrid, debug);
                }
            } else
            {
                BDM.updatePanels(defaultTag, GridTerminalSystem, Me.CubeGrid, debug);
            }
        }

        class BaconDotmatrix {
            public void updatePanels(string tag, IMyGridTerminalSystem GTS, IMyCubeGrid CG, BaconDebug debug)
            {
                debug.putSender("BaconDotmatrix.updatePanels");
                List<IMyTextPanel> Panels = new List<IMyTextPanel>();
                GTS.GetBlocksOfType<IMyTextPanel>(Panels, (x => x.CustomName.Contains(tag) && x.CubeGrid.Equals(CG)));
                debug.add("found " + Panels.Count.ToString() + " BaconDraw Panels");
                VectorScriptParser VSP = new VectorScriptParser(debug);
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
                                    VSP.parseLine(script[s], canvas, draw, debug);
                                }
                                Panels[i].WritePublicText(canvas.ToString());
                            }                            
                        }
                    }
                }
                debug.pullSender();
            }

            public class VectorScriptParser
            {
                string defaultFontDefinition = @"! 2,0 2,1 2,2 2,4 "" 1,0 3,0 1,1 3,1  # 1,0 3,0 0,1 1,1 2,1 3,1 4,1 1,2 3,2 0,3 1,3 2,3 3,3 4,3 1,4 3,4 $ 1,0 2,0 3,0 4,0 0,1 2,1 1,2 2,2 3,2 2,3 4,3 0,4 1,4 2,4 3,4 % 0,0 1,0 4,0 0,1 1,1 3,1 2,2 1,3 3,3 4,3 0,4 3,4 4,4 ' 2,0 2,1 ( 3,0 2,1 2,3 2,3 3,4 ) 1,0 2,1 2,2 2,3 1,4 * 1,0 3,0 2,1 1,2 3,2 , 2,3 1,4 - 1,2 2,2 3,2 . 2,4 / 4,0 3,1 2,2 1,3 0,4 0 1,0 2,0 3,0 0,1 3,1 4,1 0,2 2,2 4,2 0,3 1,3 4,3 1,4 2,4 3,4 1 2,0 3,0 3,1 3,2 3,3 2,4 3,4 4,4 2 0,0 1,0 2,0 3,0 4,1 1,2 2,2 3,2 0,3 0,4 1,4 2,4 3,4 4,4 3 0,0 1,0 2,0 3,0 4,1 2,2 3,2 4,3 0,4 1,4 2,4 3,4 4 2,0 3,0 1,1 3,1 0,2 3,2 0,3 1,3 2,3 3,3 4,3 3,4 5 0,0 1,0 2,0 3,3 4,0 0,1 0,2 1,2 2,2 3,3 4,3 0,4 1,4 2,4 3,4 6 1,0 2,0 3,0 4,0 0,1 0,2 1,2 2,2 3,2 0,3 4,1 1,4 2,4 3,4 7 0,0 1,0 2,0 3,0 4,0 4,1 3,2 2,3 1,4 8 1,0 2,0 3,0 0,1 4,1 1,2 2,2 3,2 0,3 4,3 1,4 2,4 3,4 9 1,0 2,0 3,0 0,1 4,1 1,2 2,2 3,2 4,2 4,3 0,4 1,4 2,4 3,4 : 2,1 2,3 ; 2,1 2,3 1,4 < 4,0 3,1 2,2 3,3 4,4 = 1,1 2,1 3,1 1,3 2,3 3,3 > 0,0 1,1 2,2 1,3 0,4 ? 1,0 2,0 3,0 4,1 2,2 3,2 2,4 @ 1,0 2,0 3,0 4,0 0,1 4,1 0,2 2,2 3,2 4,2 0,3 2,3 3,3 1,4 2,4 3,4 4,4 A 2,0 1,1 3,1 0,2 4,2 0,3 1,3 2,3 3,3 4,3 0,4 4,4 B 0,0 1,0 2,0 3,0 0,1 4,1 0,2 1,2 2,2 3,2 0,3 4,3 0,4 1,4 2,4 3,4 C 1,0 2,0 3,0 4,0 0,1 0,2 0,3 1,4 2,4 3,4 4,4 D 0,0 1,0 2,0 3,0 0,1 4,1 0,2 4,1 0,3 4,3 0,4 1,4 2,4 3,4 E 0,0 1,0 2,0 3,0 4,0 0,1 0,2 1,2 2,2 0,3 0,4 1,4 2,4 3,4 4,4 F 0,0 1,0 2,0 3,0 4,0 0,1 0,2 1,2 2,2 0,3 0,4 G 1,0 2,0 3,0 4,0 0,1 0,2 3,2 4,2 0,3 4,3 1,4 2,4 3,4 4,4 H 0,0 4,0 0,1 4,1 0,2 1,2 2,2 3,2 4,2 0,3 4,3 0,4 4,4 I 0,0 1,0 2,0 3,0 4,0 2,1 2,2 2,3 0,4 1,4 2,4 3,4 4,4 J 0,0 1,0 2,0 3,0 4,0 3,1 3,2 0,3 3,3 1,4 2,4 K 0,0 4,0 0,1 3,1 0,2 1,2 2,2 0,3 3,3 0,4 4,4 L 0,0 0,1 0,2 0,3 0,4 1,4 2,4 3,4 4,4 M 0,0 4,0 0,1 1,1 3,1 4,1 0,2 2,2 4,2 0,3 4,3 0,4 4,4 N 0,0 4,0 0,1 1,1 4,1 0,2 2,2 4,2 0,3 3,3 4,3 0,4 4,4 O 1,0 2,0 3,0 0,1 4,1 0,2 4,2 0,3 4,3 1,4 2,4 3,4 P 0,0 1,0 2,0 3,0 0,1 4,1 0,2 1,2 2,2 3,2 0,3 0,4 Q 1,0 2,0 3,0 0,1 4,1 0,2 2,2 4,2 0,3 3,3 1,4 2,4 4,4 R 0,0 1,0 2,0 3,0 0,1 4,1 0,2 1,2 2,2 3,2 0,3 4,3 0,4 4,4 S 1,0 2,0 3,0 4,0 0,1 1,2 2,2 3,2 4,3 0,4 1,4 2,4 3,4 T 0,0 1,0 2,0 3,0 4,0 2,1 2,2 2,3 2,4 U 0,0 4,0 0,1 4,1 0,2 4,2 0,3 4,3 1,4 2,4 3,4 V 0,0 4,0 0,1 4,1 0,2 4,2 1,3 3,3 2,4 W 0,0 4,0 0,1 4,1 0,2 2,2 4,2 0,3 1,3 3,3 4,3 0,4 4,4 X 0,0 4,0 1,1 3,1 2,2 1,3 3,3 0,4 4,4  Y 0,0 4,0 1,1 3,1 2,2 2,3 2,4 Z 0,0 1,0 2,0 3,0 4,0 3,1 2,2 1,3 0,4 1,4 2,4 3,4 4,4 [ 2,0 3,0 2,1 2,2 2,3 2,4 3,4 \ 0,0 1,1 2,2 3,3 4,4 ] 1,0 2,0 2,1 2,2 2,3 1,4 2,4 ^ 2,0 1,1 3,1 _ 0,4 1,4 2,4 3,4 4,4 ` 2,0 3,1 { 2,0 3,0 2,1 1,2 2,3 2,4 3,4 | 2,0 2,1 2,2 2,3 2,4 } 1,0 2,0 2,1 3,2 2,3 1,4 2,4 ~ 1,1 0,2 2,2 4,2 3,3";
                private Font defaultFont = null;
                private Dictionary<string, Font> Fonts = new Dictionary<string, Font>();
                        

                public VectorScriptParser(BaconDebug debug)
                {
                    defaultFont = this.parseFontFromByDefinition(defaultFontDefinition, debug);
                }

                public void parseLine(string line, Canvas canvas, Draw draw, BaconDebug debug)
                {
                    debug.putSender("VectorScriptParser.parseLine");
                    string[] a = line.Split(new char[] {' '}, 2);
                    string cmd = ((a.Length > 0) ? a[0] : "null").ToLower();
                    string args = (a.Length > 1) ? a[1] : "";
                    debug.add("cmd: " + cmd + " | args: " + args);
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
                            parseText(args, canvas, draw, debug);
                            break;
                        case "font":
                            parseFont(args, debug);
                            break;
                        default:
                            break;
                    }
                    debug.pullSender();
                }

                private Font getFontByName(string name)
                {
                    return Fonts.ContainsKey(name) ? Fonts[name] : defaultFont;
                }

                private void parseFont(string line, BaconDebug debug)
                {
                    string[] argv = line.Split(new char[] {' '}, 2);
                    if (argv.Length == 2 && !Fonts.ContainsKey(argv[0]))
                    {
                        Font tmp = parseFontFromByDefinition(argv[1], debug);
                        if(tmp != null)
                        {
                            Fonts.Add(argv[0], tmp);
                        }                        
                    }
                }

                public Font parseFontFromByDefinition(string defintion, BaconDebug debug)
                {
                    debug.putSender("VectorScriptParser.parseFontFromByDefinition");
                    string[] argv = defintion.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (argv.Length > 0)
                    {
                        Font font = new Font();
                        System.Text.RegularExpressions.Regex pointRgx = new System.Text.RegularExpressions.Regex(@"\d+,\d+");
                        List<Point> PointSlug = new List<Point>();
                        char glyph = '\0';
                        bool skip = true;
                        for (int i = 1; i < argv.Length; i++)
                        {
                            string tmpArg = argv[i];

                            if (tmpArg.Length == 1) //glyph
                            {
                                if (!glyph.Equals('\0') && PointSlug.Count > 0)
                                {
                                    debug.add("add glyph to font '" + glyph + "'");
                                    font.addGlyph(glyph, PointSlug, debug);
                                }
                                debug.add("new glyph => " + tmpArg);
                                glyph = tmpArg.ToLower()[0];
                                if (font.has(glyph))
                                {
                                    debug.add("gflyph exists -> skip");
                                    skip = true;
                                }
                                else
                                {
                                    skip = false;
                                    debug.add("start parsing glyph '" + glyph + "'");
                                }
                                PointSlug = new List<Point>();
                            }
                            else if (!skip && pointRgx.IsMatch(tmpArg)) //point
                            {
                                debug.add("found point for '" + glyph + "' => " + tmpArg);
                                string[] points = tmpArg.Trim().Split(',');
                                if (points.Length == 2)
                                {
                                    int x = 0;
                                    int y = 0;
                                    debug.add("try parse x: " + points[0].ToString() + ", y: " + points[1].ToString());
                                    if (int.TryParse(points[0], out x) && int.TryParse(points[1], out y))
                                    {
                                        debug.add("add Point " + x.ToString() + "," + y.ToString());
                                        PointSlug.Add(new Point(x, y));
                                    }
                                }
                            }
                            else if (skip)
                            {
                                debug.add("skipped: \"" + tmpArg + "\"");
                            } else { 
                                debug.add("cant parse: \"" + tmpArg + "\"");
                            }
                        }
                        font.addGlyph(' ', new List<Point>(), debug); // 'space'
                        debug.pullSender();
                        return font;
                    } else
                    {
                        debug.add("No entries in definition -> can't parse font");
                        debug.pullSender();
                        return null;
                    }
                }

                private void parseText(string args, Canvas canvas, Draw draw, BaconDebug debug)
                {
                    string[] argv = args.Split(new char[] {' '}, 2);
                    if (argv.Length > 1)
                    {
                        Font tmpF = getFontByName(argv[0]);
                        if (tmpF != null)
                        {
                            draw.text(argv[1], tmpF, canvas, debug);
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
                public void text(string text, Font font, Canvas canvas, BaconDebug debug)
                {
                    debug.putSender("Draw.text");
                    int offsetX = canvas.getPos().X;
                    int offsetY = canvas.getPos().Y;
                    debug.add("length: " + text.Length.ToString() + ", position: " + offsetX.ToString() + "," + offsetY.ToString() );
                    for (int i = 0; i < text.Length; i++) {
                        char curChar = text.ToLower()[i];
                        List<Point> slug = font.getPoints(curChar, debug);
                        if(slug != null)
                        {
                            string dbgTmp = "";
                            for(int p = 0; p < slug.Count; p++) {
                                Point P = new Point(offsetX + slug[p].X, offsetY + slug[p].Y);
                                canvas.add(P, false);
                                dbgTmp += "["+P.X.ToString()+","+P.Y.ToString()+"]";
                            }
                            debug.add("drawing '" + curChar + "' (" + slug.Count + ") " + dbgTmp);
                            dbgTmp = "";
                        }
                        offsetX = offsetX + font.getWidth() +1;
                    }
                    moveTo(new Point(offsetX, offsetY), canvas);
                    debug.pullSender();
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

                public void addGlyph(char glyph, List<Point> Points, BaconDebug debug)
                {
                    debug.putSender("Font.addGlyph");
                    if (!glyphMap.ContainsKey(glyph))
                    {
                        for(int i = 0; i < Points.Count; i++)
                        {
                            width = Math.Max(Points[i].X+1, width);
                            height = Math.Max(Points[i].Y+1, height);
                            debug.add("set font diemsions to " + width.ToString() + "x" + height.ToString() + " (Point => " + Points[i].ToString() + ")");
                        }
                        glyphMap.Add(glyph, Points);
                    }
                    debug.pullSender();
                }        

                public bool has(char glyph)
                {
                    return glyphMap.ContainsKey(glyph);
                }
                
                public int getHeight()
                {
                    return height;
                }        

                public int getWidth()
                {
                    return width;
                }

                public List<Point> getPoints(char glyph, BaconDebug debug)
                {
                    debug.putSender("Font.getPoints");
                    debug.add("Points for '" + glyph + "' " + (glyphMap.ContainsKey(glyph)?"MATCH (" + glyphMap[glyph].Count.ToString() + ")":"NO MATCH - using placeholder (" + getUnknownChar().Count.ToString() + ")"));                    
                    debug.pullSender();
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

        public class BaconArgs { static public Bag parse(string a) { return (new Parser()).parseArgs(a); } public class Parser { static Dictionary<string, Bag> h = new Dictionary<string, Bag>(); public Bag parseArgs(string a) { if (!h.ContainsKey(a)) { Bag b = new Bag(); var c = false; var d = false; var e = new StringBuilder(); for (int f = 0; f < a.Length; f++) { var g = a[f]; if (c) { e.Append(g); c = false; } else if (g.Equals('\\')) c = true; else if (d && !g.Equals('"')) e.Append(g); else if (g.Equals('"')) d = !d; else if (g.Equals(' ')) { b.add(e.ToString()); e.Clear(); } else e.Append(g); } if (e.Length > 0) b.add(e.ToString()); h.Add(a, b); } return h[a]; } } public class Bag { protected Dictionary<char, int> h = new Dictionary<char, int>(); protected List<string> i = new List<string>(); protected Dictionary<string, List<string>> j = new Dictionary<string, List<string>>(); public List<string> getArguments() { return i; } public int getFlag(char a) { return h.ContainsKey(a) ? h[a] : 0; } public List<string> getOption(string a) { return j.ContainsKey(a) ? j[a] : new List<string>(); } public void add(string a) { if (!a.StartsWith("-")) i.Add(a); else if (a.StartsWith("--")) { KeyValuePair<string, string> b = k(a); var c = b.Key.Substring(2); if (!j.ContainsKey(c)) j.Add(c, new List<string>()); j[c].Add(b.Value); } else { var b = a.Substring(1); for (int d = 0; d < b.Length; d++) if (this.h.ContainsKey(b[d])) { this.h[b[d]]++; } else { this.h.Add(b[d], 1); } } } KeyValuePair<string, string> k(string a) { string[] b = a.Split(new char[] { '=' }, 2); return new KeyValuePair<string, string>(b[0], (b.Length > 1) ? b[1] : null); } override public string ToString() { var a = new List<string>(); foreach (string key in j.Keys) a.Add(l(key) + ":[" + string.Join(",", j[key].ConvertAll<string>(b => l(b)).ToArray()) + "]"); var c = new List<string>(); foreach (char key in h.Keys) c.Add(key + ":" + h[key].ToString()); var d = new StringBuilder(); d.Append("{\"a\":["); d.Append(string.Join(",", i.ConvertAll<string>(b => l(b)).ToArray())); d.Append("],\"o\":[{"); d.Append(string.Join("},{", a)); d.Append("}],\"f\":[{"); d.Append(string.Join("},{", c)); d.Append("}]}"); return d.ToString(); } string l(string a) { return (a != null) ? "\"" + a.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" : @"null"; } } }
        public class BaconDebug { List<IMyTextPanel> g = new List<IMyTextPanel>(); MyGridProgram h; List<string> i = new List<string>(); bool j = false; public BaconDebug(string a, IMyGridTerminalSystem b, MyGridProgram c, bool d) { var e = new List<IMyTerminalBlock>(); b.GetBlocksOfType<IMyTextPanel>(e, ((IMyTerminalBlock f) => f.CustomName.Contains(a) && f.CubeGrid.Equals(c.Me.CubeGrid))); g = e.ConvertAll<IMyTextPanel>(f => f as IMyTextPanel); this.h = c; putSender("BaconDebug"); k(d); } void k(bool a) { this.j = a; } void l(string a, IMyGridTerminalSystem b) { } public void putSender(string a) { i.Add(a); } public void pullSender() { if (i.Count > 1) i.RemoveAt(i.Count - 1); } public string getSender() { return i[i.Count - 1]; } public void add(string a) { if (j) for (int b = 0; b < g.Count; b++) { List<string> c = new List<string>(); c.AddRange(g[b].GetPublicText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)); StringBuilder d = new StringBuilder(); c.Add(n(a)); if (!g[b].GetPrivateTitle().ToLower().Equals("nolinelimit")) { int e = m(g[b]); if (c.Count > e) { c.RemoveRange(0, c.Count - e); } } g[b].WritePublicText(string.Join("\n", c)); } } int m(IMyTextPanel a) { float b = a.GetValueFloat("FontSize"); if (b == 0.0f) b = 0.01f; return Convert.ToInt32(Math.Ceiling(17.0f / b)); } string n(string a) { var b = new StringBuilder(); b.Append("[" + DateTime.Now.ToShortTimeString() + "]"); b.Append("[" + getSender() + "]"); b.Append("[IC " + h.Runtime.CurrentInstructionCount + "/" + h.Runtime.MaxInstructionCount + "]"); b.Append("[MCC " + h.Runtime.CurrentMethodCallCount + "/" + h.Runtime.MaxMethodCallCount + "]"); b.Append(" " + a); return b.ToString(); } }


        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}