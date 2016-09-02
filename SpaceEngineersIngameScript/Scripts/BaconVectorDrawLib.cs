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

namespace BaconVectorDrawLib
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
            BaconDotmatrix.Canvas c = new BaconDotmatrix.Canvas(10,10);
            BaconDotmatrix.Draw b = new BaconDotmatrix.Draw();
            b.color('1', c);
            b.moveTo(new Point(10,10), c);
            b.circle(4,c);
            Console.WriteLine(c);
        }

        class BaconDotmatrix {
            public class Parser
            {
                private void parseLine(string line, Canvas canvas, Draw Draw)
                {
                    string[] a = line.Split(new char[] {' '}, 2);
                    string cmd = ((a.Length > 0) ? a[0] : "null").ToLower();
                    string args = (a.Length > 1) ? a[1] : "";
                    switch (cmd)
                    {
                        case "circle":
                            break;
                        case "color":                            
                            break;
                        case "moveto":
                            break;
                        case "lineto":
                            break;
                        default:
                            break;
                    }
                }
            }

            public class Draw
            {

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

                public void color(char color, Canvas canvas)
                {
                    canvas.setColor(color);
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

                public Canvas(int w, int h) : this(w, h, '0')
                {
                }

                public Canvas(int w, int h, char bgColor)
                {
                    width = w;
                    height = h;
                    data = new Char[h][];
                    for (int i = 0; i < h; i++)
                    {
                        data[i] = (new String(bgColor, w)).ToCharArray();
                    }
                }

                public void setColor(char c)
                {
                    color = c;
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
        }
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}