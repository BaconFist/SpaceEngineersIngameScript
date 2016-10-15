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

            
            PB-ARGUMENT:
                Everything not starting with a "-" is interpreted as a TAG for a TextPanel to draw on. 

                -v      
                        show Errors on debug screen
                -vv     
                        like -v + shows Warnings
                -vvv    
                        like -vv + shows Info
                -vvvv   
                        like -vvv + shows all Debug stuff (requires --debug)
                (hint: "-vv" and "-v -v" means the same as Flags will be countet)

                --debug
                        enable debug output for "-vvvv" flags. This additional argument is required because the debugger causes massive Simspeed drop (had down to 0.12)
                --debug-screen=*value*
                        debuger will write on all TextPanels with *value* in their name
        */
        string defaultTag = "[BaconDraw]";
        string defaultDebugTag = "[BaconDraw_DEBUG]";

        public Program()
        {
            BaconDebug debug = new BaconDebug("-", GridTerminalSystem, this, 0);
        }

        public void Main(string argument)
        {
            BaconArgs Args = BaconArgs.parse(argument);
            BaconDebug debug = createDebugger(Args);
            BaconDraw BDM = new BaconDraw();
            
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

        void run(BaconArgs Args, BaconDebug debug)
        {
            if(Args.getOption("showCommands").Count > 0)
            {
                string[] tags = Args.getOption("showCommands").ToArray();
                List<IMyTextPanel> Panels = new List<IMyTextPanel>();
                GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(Panels, (p => tags.Any(t => p.CustomName.Contains(t))));
            }
        }

        private BaconDebug createDebugger(BaconArgs Args)
        {
            int debugVerbosity = Args.getFlag('v');
            if(debugVerbosity >= BaconDebug.DEBUG && (Args.getOption("debug").Count <= 0))
            {
                debugVerbosity = BaconDebug.INFO;
            }
            string tag = (Args.getOption("debug-screen").Count > 0) ? Args.getOption("debug-screen")[0] : defaultDebugTag;

            BaconDebug slug = new BaconDebug(tag, GridTerminalSystem, this, debugVerbosity);
            slug.newScope("createDebugger");
            slug.add("Debugger initialized with a verbosity: " + debugVerbosity.ToString() + " on Panels with \"" + tag + "\"", BaconDebug.DEBUG);
            slug.leaveScope();
            return slug;
        }
        
        class BaconDraw {
            static private DrawingTask DrawingTasks = new DrawingTask(null);
            static private Dictionary<long, int> PanelLastState = new Dictionary<long, int>();

            public void updatePanels(string tag, IMyGridTerminalSystem GTS, IMyCubeGrid CG, BaconDebug debug)
            {
                debug.newScope("BaconDotmatrix.updatePanels");
                List<IMyTextPanel> Panels = new List<IMyTextPanel>();
                GTS.GetBlocksOfType<IMyTextPanel>(Panels, (x => x.CustomName.Contains(tag) && x.CubeGrid.Equals(CG)));
                if (Panels.Count > 0)
                {
                    debug.add("found " + Panels.Count.ToString() + " BaconDraw Panels", BaconDebug.DEBUG);
                } else
                {
                    debug.add("can't find any BaconDraw Panels with tag \"" + tag + "\" on this Grid.", BaconDebug.WARN);
                }
                for (int i = 0; i < Panels.Count; i++)
                {
                    BaconArgs PanelArgs = BaconArgs.parse(Panels[i].GetPrivateTitle());
                    updatePanel(Panels[i], debug, PanelArgs);
                }
                debug.leaveScope();
            }

            private void updatePanel(IMyTextPanel Panel, BaconDebug debug, BaconArgs PanelArgs)
            {
                debug.newScope("updatePanel");
                debug.add("updating \"" + Panel.ToString() + "\"", BaconDebug.DEBUG);

                if (hasPanelChanged(Panel))
                {
                    resetPanel(Panel, debug, PanelArgs);
                }

                if (isPanelInProgress(Panel))
                {
                    continueTask(Panel, BaconDraw.DrawingTasks[Panel.EntityId], debug);
                }

                debug.leaveScope();
            }

            private void continueTask(IMyTextPanel Panel, DrawingTask Task, BaconDebug debug)
            {
                debug.newScope("continueTask");
                debug.add("continue Task " + Panel.EntityId.ToString() + " (" + Panel.CustomName + ")", BaconDebug.INFO);

                string[] script = Panel.GetPrivateText().Trim(new Char[] { '\n', '\r', ' ' }).Split(new Char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                VectorScriptParser Parser = new VectorScriptParser(debug);
                Draw Draw = new Draw();
                bool limitReached = (debug.remainingInstructions < ((debug.getVerbosity() >= BaconDebug.DEBUG) ? 25000 : 3000));

                for (int i=Task.currentLine; i<script.Length && !limitReached; i++, Task.currentLine++)
                {
                    string cmd = script[i];
                    debug.add("Parsing Command: " + cmd, BaconDebug.INFO);
                    Parser.parseLine(cmd, Task.canvas, Draw, debug);
                    limitReached = (debug.remainingInstructions < ((debug.getVerbosity() >= BaconDebug.DEBUG) ? 25000 : 3000));
                }
                if(Task.currentLine >= script.Length)
                {
                    debug.add("Task stopped -> END OF SCRIPT", BaconDebug.INFO);
                }
                if (limitReached)
                {
                    debug.add("Task stopped -> Instructionlimit reached: " + debug.remainingInstructions.ToString(), BaconDebug.INFO);
                }

                Panel.WritePublicText(Task.canvas.ToString(), false);
                Panel.ShowPublicTextOnScreen();

                if (Task.currentLine >= script.Length)
                {
                    BaconDraw.DrawingTasks.terminate(Panel);
                    debug.add("Task terminated", BaconDebug.INFO);
                }                
                debug.leaveScope();
            }

            private bool isPanelInProgress(IMyTextPanel Panel)
            {
                return BaconDraw.DrawingTasks.has(Panel);
            }

            private bool hasPanelChanged(IMyTextPanel Panel)
            {
                return (!(BaconDraw.PanelLastState.ContainsKey(Panel.EntityId) && BaconDraw.PanelLastState[Panel.EntityId].Equals((Panel.GetValueFloat("FontSize") + Panel.GetPrivateText()).GetHashCode())));
            }

            private void resetPanel(IMyTextPanel Panel, BaconDebug debug, BaconArgs PanelArgs)
            {
                debug.newScope("resetPanel");

                BaconDraw.DrawingTasks.terminate(Panel);
                BaconDraw.DrawingTasks.Add(Panel.EntityId, new DrawingTask(getCanvasFromPanel(Panel, debug, PanelArgs)));
                debug.add("New Task for " + "[" + Panel.EntityId.ToString() + "]" + Panel.CustomName, BaconDebug.DEBUG);

                if (BaconDraw.PanelLastState.ContainsKey(Panel.EntityId))
                {
                    BaconDraw.PanelLastState.Remove(Panel.EntityId);
                }
                BaconDraw.PanelLastState.Add(Panel.EntityId, (Panel.GetValueFloat("FontSize") + Panel.GetPrivateText()).GetHashCode());
                debug.add("Reset state of " + "[" + Panel.EntityId.ToString() + "]" + Panel.CustomName, BaconDebug.DEBUG);

                debug.leaveScope();
            }

            private Canvas getCanvasFromPanel(IMyTextPanel Panel, BaconDebug debug, BaconArgs PanelArgs)
            {
                Point Dimesions = getCanvasDimensions(Panel, debug, PanelArgs);
                Canvas slug = new Canvas(Dimesions.X, Dimesions.Y);

                return slug;
            }
            
            private Point getCanvasDimensions(IMyTextPanel Panel, BaconDebug debug, BaconArgs PanelArgs)
            {
                bool dimensionsSetByScript = false;
                string method = null;
                int newHeight = 0;
                int newWidth = 0;

                string[] script = Panel.GetPrivateText().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (script.Length > 0)
                {
                    BaconArgs ScriptArgs = BaconArgs.parse(script[0]);
                    if(ScriptArgs.getOption("width").Count > 0 && ScriptArgs.getOption("height").Count > 0 && int.TryParse(ScriptArgs.getOption("width")[0], out newWidth) && int.TryParse(ScriptArgs.getOption("height")[0], out newHeight))
                    {
                        dimensionsSetByScript = true;
                        method = "Script";
                    }
                }

                if (!dimensionsSetByScript)
                {
                    debug.newScope("getCanvasDimensions");
                    float f_width = 16f;
                    float f_height = 17f;

                    float tempFW;
                    float tempFH;

                    if ((PanelArgs.getOption("--f-width").Count > 0) && float.TryParse(PanelArgs.getOption("--f-width")[0], out tempFW))
                    {
                        debug.add("using Panels settings to adjust image width.  W*" + tempFW.ToString(), BaconDebug.DEBUG);
                        f_width = f_width * tempFW;
                    }

                    if ((PanelArgs.getOption("--f-height").Count > 0) && float.TryParse(PanelArgs.getOption("--f-height")[0], out tempFH)){
                        debug.add("using Panels settings to adjust image height.  H*" + tempFH.ToString(), BaconDebug.DEBUG);
                        f_height = f_height * tempFH;
                    }

                    float fontSize = Panel.GetValueFloat("FontSize");
                    newWidth = (int)Math.Floor(f_width / fontSize);
                    newHeight = (int)Math.Floor(f_height / fontSize);
                    method = "FontSize (" + fontSize.ToString() + ")";
                }
                if (method == null)
                {
                    method = "error fallback";
                    newHeight = 50;
                    newWidth = 50;
                }
                debug.add("Dimensions for \"" + Panel.CustomName + "\" set by " + method + " to " + newWidth.ToString() + "*" + newHeight.ToString(), BaconDebug.DEBUG);

                debug.leaveScope();
                return new Point(newWidth, newHeight);
            }

            public class DrawingTask : Dictionary<long, DrawingTask>
            {
                public Canvas canvas;
                public int currentLine;
                
                public DrawingTask(Canvas canvas)
                {
                    this.canvas = canvas;
                    currentLine = 0;
                }

                public bool has(IMyTextPanel Panel)
                {
                    return this.ContainsKey(Panel.EntityId);
                }   
                
                public void terminate(IMyTextPanel Panel) {
                    if (has(Panel))
                    {
                        Remove(Panel.EntityId);
                    }
                }             
            }

            public class VectorScriptParser
            {
                private string defaultFontDefinition = @"! 2,0 2,1 2,2 2,4 "" 1,0 3,0 1,1 3,1  # 1,0 3,0 0,1 1,1 2,1 3,1 4,1 1,2 3,2 0,3 1,3 2,3 3,3 4,3 1,4 3,4 $ 1,0 2,0 3,0 4,0 0,1 2,1 1,2 2,2 3,2 2,3 4,3 0,4 1,4 2,4 3,4 % 0,0 1,0 4,0 0,1 1,1 3,1 2,2 1,3 3,3 4,3 0,4 3,4 4,4 ' 2,0 2,1 ( 3,0 2,1 2,3 2,3 3,4 ) 1,0 2,1 2,2 2,3 1,4 * 1,0 3,0 2,1 1,2 3,2 , 2,3 1,4 - 1,2 2,2 3,2 . 2,4 / 4,0 3,1 2,2 1,3 0,4 0 1,0 2,0 3,0 0,1 3,1 4,1 0,2 2,2 4,2 0,3 1,3 4,3 1,4 2,4 3,4 1 2,0 3,0 3,1 3,2 3,3 2,4 3,4 4,4 2 0,0 1,0 2,0 3,0 4,1 1,2 2,2 3,2 0,3 0,4 1,4 2,4 3,4 4,4 3 0,0 1,0 2,0 3,0 4,1 2,2 3,2 4,3 0,4 1,4 2,4 3,4 4 2,0 3,0 1,1 3,1 0,2 3,2 0,3 1,3 2,3 3,3 4,3 3,4 5 0,0 1,0 2,0 3,3 4,0 0,1 0,2 1,2 2,2 3,3 4,3 0,4 1,4 2,4 3,4 6 1,0 2,0 3,0 4,0 0,1 0,2 1,2 2,2 3,2 0,3 4,1 1,4 2,4 3,4 7 0,0 1,0 2,0 3,0 4,0 4,1 3,2 2,3 1,4 8 1,0 2,0 3,0 0,1 4,1 1,2 2,2 3,2 0,3 4,3 1,4 2,4 3,4 9 1,0 2,0 3,0 0,1 4,1 1,2 2,2 3,2 4,2 4,3 0,4 1,4 2,4 3,4 : 2,1 2,3 ; 2,1 2,3 1,4 < 4,0 3,1 2,2 3,3 4,4 = 1,1 2,1 3,1 1,3 2,3 3,3 > 0,0 1,1 2,2 1,3 0,4 ? 1,0 2,0 3,0 4,1 2,2 3,2 2,4 @ 1,0 2,0 3,0 4,0 0,1 4,1 0,2 2,2 3,2 4,2 0,3 2,3 3,3 1,4 2,4 3,4 4,4 A 2,0 1,1 3,1 0,2 4,2 0,3 1,3 2,3 3,3 4,3 0,4 4,4 B 0,0 1,0 2,0 3,0 0,1 4,1 0,2 1,2 2,2 3,2 0,3 4,3 0,4 1,4 2,4 3,4 C 1,0 2,0 3,0 4,0 0,1 0,2 0,3 1,4 2,4 3,4 4,4 D 0,0 1,0 2,0 3,0 0,1 4,1 0,2 4,1 0,3 4,3 0,4 1,4 2,4 3,4 E 0,0 1,0 2,0 3,0 4,0 0,1 0,2 1,2 2,2 0,3 0,4 1,4 2,4 3,4 4,4 F 0,0 1,0 2,0 3,0 4,0 0,1 0,2 1,2 2,2 0,3 0,4 G 1,0 2,0 3,0 4,0 0,1 0,2 3,2 4,2 0,3 4,3 1,4 2,4 3,4 4,4 H 0,0 4,0 0,1 4,1 0,2 1,2 2,2 3,2 4,2 0,3 4,3 0,4 4,4 I 0,0 1,0 2,0 3,0 4,0 2,1 2,2 2,3 0,4 1,4 2,4 3,4 4,4 J 0,0 1,0 2,0 3,0 4,0 3,1 3,2 0,3 3,3 1,4 2,4 K 0,0 4,0 0,1 3,1 0,2 1,2 2,2 0,3 3,3 0,4 4,4 L 0,0 0,1 0,2 0,3 0,4 1,4 2,4 3,4 4,4 M 0,0 4,0 0,1 1,1 3,1 4,1 0,2 2,2 4,2 0,3 4,3 0,4 4,4 N 0,0 4,0 0,1 1,1 4,1 0,2 2,2 4,2 0,3 3,3 4,3 0,4 4,4 O 1,0 2,0 3,0 0,1 4,1 0,2 4,2 0,3 4,3 1,4 2,4 3,4 P 0,0 1,0 2,0 3,0 0,1 4,1 0,2 1,2 2,2 3,2 0,3 0,4 Q 1,0 2,0 3,0 0,1 4,1 0,2 2,2 4,2 0,3 3,3 1,4 2,4 4,4 R 0,0 1,0 2,0 3,0 0,1 4,1 0,2 1,2 2,2 3,2 0,3 4,3 0,4 4,4 S 1,0 2,0 3,0 4,0 0,1 1,2 2,2 3,2 4,3 0,4 1,4 2,4 3,4 T 0,0 1,0 2,0 3,0 4,0 2,1 2,2 2,3 2,4 U 0,0 4,0 0,1 4,1 0,2 4,2 0,3 4,3 1,4 2,4 3,4 V 0,0 4,0 0,1 4,1 0,2 4,2 1,3 3,3 2,4 W 0,0 4,0 0,1 4,1 0,2 2,2 4,2 0,3 1,3 3,3 4,3 0,4 4,4 X 0,0 4,0 1,1 3,1 2,2 1,3 3,3 0,4 4,4  Y 0,0 4,0 1,1 3,1 2,2 2,3 2,4 Z 0,0 1,0 2,0 3,0 4,0 3,1 2,2 1,3 0,4 1,4 2,4 3,4 4,4 [ 2,0 3,0 2,1 2,2 2,3 2,4 3,4 \ 0,0 1,1 2,2 3,3 4,4 ] 1,0 2,0 2,1 2,2 2,3 1,4 2,4 ^ 2,0 1,1 3,1 _ 0,4 1,4 2,4 3,4 4,4 ` 2,0 3,1 { 2,0 3,0 2,1 1,2 2,3 2,4 3,4 | 2,0 2,1 2,2 2,3 2,4 } 1,0 2,0 2,1 3,2 2,3 1,4 2,4 ~ 1,1 0,2 2,2 4,2 3,3";
                private Font defaultFont = null;
                static private Dictionary<string, Font> FontParsingCache = new Dictionary<string, Font>();
                static private Dictionary<string, Font> Fonts = new Dictionary<string, Font>();

                public VectorScriptParser(BaconDebug debug)
                {
                    defaultFont = this.parseFontFromByDefinition(defaultFontDefinition, debug);
                }

                public void parseLine(string line, Canvas canvas, Draw draw, BaconDebug debug)
                {
                    debug.newScope("VectorScriptParser.parseLine");
                    if (line.StartsWith("//")) { 
                        debug.add("skip comment: " + line, BaconDebug.DEBUG);
                    }
                    else
                    {
                        string[] a = line.Split(new char[] { ' ' }, 2);
                        string cmd = ((a.Length > 0) ? a[0] : "null").ToLower();
                        string args = (a.Length > 1) ? a[1] : "";
                        debug.add("cmd: " + cmd + " | args: " + args, BaconDebug.DEBUG);
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
                                debug.add("skipping unknown command: " + cmd + " ARGS: " + args, BaconDebug.WARN);
                                break;
                        }
                    }
                    debug.leaveScope();
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
                    debug.newScope("VectorScriptParser.parseFontFromByDefinition");
                    if (!VectorScriptParser.FontParsingCache.ContainsKey(defintion))
                    {
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
                                        debug.add("add glyph to font '" + glyph + "'", BaconDebug.DEBUG);
                                        font.addGlyph(glyph, PointSlug, debug);
                                    }
                                    debug.add("new glyph => " + tmpArg, BaconDebug.DEBUG);
                                    glyph = tmpArg.ToLower()[0];
                                    if (font.has(glyph))
                                    {
                                        debug.add("gflyph exists -> skip", BaconDebug.DEBUG);
                                        skip = true;
                                    }
                                    else
                                    {
                                        skip = false;
                                        debug.add("start parsing glyph '" + glyph + "'", BaconDebug.DEBUG);
                                    }
                                    PointSlug = new List<Point>();
                                }
                                else if (!skip && pointRgx.IsMatch(tmpArg)) //point
                                {
                                    debug.add("found point for '" + glyph + "' => " + tmpArg, BaconDebug.DEBUG);
                                    string[] points = tmpArg.Trim().Split(',');
                                    if (points.Length == 2)
                                    {
                                        int x = 0;
                                        int y = 0;
                                        debug.add("try parse x: " + points[0].ToString() + ", y: " + points[1].ToString(), BaconDebug.DEBUG);
                                        if (int.TryParse(points[0], out x) && int.TryParse(points[1], out y))
                                        {
                                            debug.add("add Point " + x.ToString() + "," + y.ToString(), BaconDebug.DEBUG);
                                            PointSlug.Add(new Point(x, y));
                                        }
                                    }
                                }
                                else if (skip)
                                {
                                    debug.add("skipped: \"" + tmpArg + "\"", BaconDebug.DEBUG);
                                }
                                else
                                {
                                    debug.add("cant parse: \"" + tmpArg + "\"", BaconDebug.ERROR);
                                }
                            }
                            font.addGlyph(' ', new List<Point>(), debug); // 'space'
                            VectorScriptParser.FontParsingCache.Add(defintion, font);
                        }
                        else
                        {
                            debug.add("No entries in definition -> can't parse font", BaconDebug.ERROR);
                        }
                    }
                    debug.leaveScope();
                    return VectorScriptParser.FontParsingCache.ContainsKey(defintion) ? VectorScriptParser.FontParsingCache[defintion] : null;
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
                    debug.newScope("Draw.text");
                    int offsetX = canvas.getPos().X;
                    int offsetY = canvas.getPos().Y;
                    debug.add("length: " + text.Length.ToString() + ", position: " + offsetX.ToString() + "," + offsetY.ToString(), BaconDebug.DEBUG);
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
                            debug.add("drawing '" + curChar + "' (" + slug.Count + ") " + dbgTmp, BaconDebug.DEBUG);
                            dbgTmp = "";
                        }
                        offsetX = offsetX + font.getWidth() +1;
                    }
                    moveTo(new Point(offsetX, offsetY), canvas);
                    debug.leaveScope();
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
                    Point o = canvas.getPos();

                    canvas.add(new Point(o.X + x, o.Y + y), false); 
                    canvas.add(new Point(o.X + y, o.Y + x), false); 
                    canvas.add(new Point(o.X + y, o.Y + -x), false); 
                    canvas.add(new Point(o.X + x, o.Y + -y), false); 
                    canvas.add(new Point(o.X + -x, o.Y + -y), false);
                    canvas.add(new Point(o.X + -y, o.Y + -x), false); 
                    canvas.add(new Point(o.X + -y, o.Y + x), false); 
                    canvas.add(new Point(o.X + -x, o.Y + y), false);
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
                private char color = '\uE00E';

                private Char[][] data; // h*x > y,x

                public Canvas(int w, int h) : this(w, h, '\uE00F')
                {
                }

                public Canvas(int w, int h, char bgColor)
                {
                    char bg = bgColor;

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

            public class Font
            {
                private Dictionary<char, List<Point>> glyphMap = new Dictionary<char, List<Point>>();
                private List<Point> unknownChar = null;
                private int width = 0;
                private int height = 0;

                public void addGlyph(char glyph, List<Point> Points, BaconDebug debug)
                {
                    debug.newScope("Font.addGlyph");
                    if (!glyphMap.ContainsKey(glyph))
                    {
                        for(int i = 0; i < Points.Count; i++)
                        {
                            width = Math.Max(Points[i].X+1, width);
                            height = Math.Max(Points[i].Y+1, height);
                            debug.add("set font diemsions to " + width.ToString() + "x" + height.ToString() + " (Point => " + Points[i].ToString() + ")", BaconDebug.DEBUG);
                        }
                        glyphMap.Add(glyph, Points);
                    }
                    debug.leaveScope();
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
                    debug.newScope("Font.getPoints");
                    debug.add("Points for '" + glyph + "' " + (glyphMap.ContainsKey(glyph)?"MATCH (" + glyphMap[glyph].Count.ToString() + ")":"NO MATCH - using placeholder (" + getUnknownChar().Count.ToString() + ")"), BaconDebug.DEBUG);                    
                    debug.leaveScope();
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
                    {'d','\uE00F'},
                };

                public const char DEFAULT = '\uE006';

                static public char get(string col)
                {
                    char key = (col.Length > 0) ? col.ToLower()[0] : DEFAULT;
                    return (map.ContainsKey(key)) ? map[key] : DEFAULT;
                }
            }
        }


        public class BaconArgs { static public BaconArgs parse(string a) { return (new Parser()).parseArgs(a); } public class Parser { static Dictionary<string, BaconArgs> h = new Dictionary<string, BaconArgs>(); public BaconArgs parseArgs(string a) { if (!h.ContainsKey(a)) { var b = new BaconArgs(); var c = false; var d = false; var e = new StringBuilder(); for (int f = 0; f < a.Length; f++) { var g = a[f]; if (c) { e.Append(g); c = false; } else if (g.Equals('\\')) c = true; else if (d && !g.Equals('"')) e.Append(g); else if (g.Equals('"')) d = !d; else if (g.Equals(' ')) { b.add(e.ToString()); e.Clear(); } else e.Append(g); } if (e.Length > 0) b.add(e.ToString()); h.Add(a, b); } return h[a]; } } protected Dictionary<char, int> h = new Dictionary<char, int>(); protected List<string> i = new List<string>(); protected Dictionary<string, List<string>> j = new Dictionary<string, List<string>>(); public List<string> getArguments() { return i; } public int getFlag(char a) { return h.ContainsKey(a) ? h[a] : 0; } public List<string> getOption(string a) { return j.ContainsKey(a) ? j[a] : new List<string>(); } public void add(string a) { if (!a.StartsWith("-")) i.Add(a); else if (a.StartsWith("--")) { KeyValuePair<string, string> b = k(a); var c = b.Key.Substring(2); if (!j.ContainsKey(c)) j.Add(c, new List<string>()); j[c].Add(b.Value); } else { var b = a.Substring(1); for (int d = 0; d < b.Length; d++) if (this.h.ContainsKey(b[d])) { this.h[b[d]]++; } else { this.h.Add(b[d], 1); } } } KeyValuePair<string, string> k(string a) { string[] b = a.Split(new char[] { '=' }, 2); return new KeyValuePair<string, string>(b[0], (b.Length > 1) ? b[1] : null); } override public string ToString() { var a = new List<string>(); foreach (string key in j.Keys) a.Add(l(key) + ":[" + string.Join(",", j[key].ConvertAll<string>(b => l(b)).ToArray()) + "]"); var c = new List<string>(); foreach (char key in h.Keys) c.Add(key + ":" + h[key].ToString()); var d = new StringBuilder(); d.Append("{\"a\":["); d.Append(string.Join(",", i.ConvertAll<string>(b => l(b)).ToArray())); d.Append("],\"o\":[{"); d.Append(string.Join("},{", a)); d.Append("}],\"f\":[{"); d.Append(string.Join("},{", c)); d.Append("}]}"); return d.ToString(); } string l(string a) { return (a != null) ? "\"" + a.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" : @"null"; } }
        public class BaconDebug { public const int INFO = 3; public const int WARN = 2; public const int ERROR = 1; public const int DEBUG = 4; List<IMyTextPanel> h = new List<IMyTextPanel>(); MyGridProgram i; List<string> j = new List<string>(); int k = 0; bool l = true; public int remainingInstructions { get { return i.Runtime.MaxInstructionCount - i.Runtime.CurrentInstructionCount; } } public bool autoscroll { get { return l; } set { l = value; } } public void clearPanels() { for (int a = 0; a < h.Count; a++) h[a].WritePublicText(""); } public BaconDebug(string a, IMyGridTerminalSystem b, MyGridProgram c, int d) { this.k = d; var e = new List<IMyTerminalBlock>(); b.GetBlocksOfType<IMyTextPanel>(e, ((IMyTerminalBlock f) => f.CustomName.Contains(a) && f.CubeGrid.Equals(c.Me.CubeGrid))); h = e.ConvertAll<IMyTextPanel>(f => f as IMyTextPanel); this.i = c; newScope("BaconDebug"); } public int getVerbosity() { return k; } public MyGridProgram getGridProgram() { return this.i; } public void newScope(string a) { j.Add(a); } public void leaveScope() { if (j.Count > 1) j.RemoveAt(j.Count - 1); } public string getSender() { return j[j.Count - 1]; } public void add(string a, int b) { if (b <= this.k) { var c = n(a); if (b == ERROR) i.Echo(c); for (int d = 0; d < h.Count; d++) if (autoscroll) { List<string> e = new List<string>(); e.AddRange(h[d].GetPublicText().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)); StringBuilder f = new StringBuilder(); e.Add(c); if (!h[d].GetPrivateTitle().ToLower().Equals("nolinelimit")) { int g = m(h[d]); if (e.Count > g) { e.RemoveRange(0, e.Count - g); } } h[d].WritePublicText(string.Join("\n", e)); } else { h[d].WritePublicText(c + '\n', true); } } } int m(IMyTextPanel a) { float b = a.GetValueFloat("FontSize"); if (b == 0.0f) b = 0.01f; return Convert.ToInt32(Math.Ceiling(17.0f / b)); } string n(string a) { var b = new StringBuilder(); b.Append("[" + DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToShortTimeString() + "." + DateTime.Now.Millisecond.ToString().TrimStart('0') + "]"); b.Append("[" + getSender() + "]"); b.Append("[IC " + i.Runtime.CurrentInstructionCount + "/" + i.Runtime.MaxInstructionCount + "]"); b.Append(" " + a); return b.ToString(); } }


        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}

/*
--width=159 --height=177 --bgcolor=d --color=l 
text - derp ^^ debugging and stuff 
color r 
text - und so 
color l 
moveTo 10,10 
text - abcdefghijklmnopqrstuvw 
moveTo 10,20 
text - xyz,.;:1234567890ß´^°!"§ 
moveTo 10,30 
text - $%&/()=?`+*~#'-_äö<>-_  
moveto 8,8 
color g 
rect 150,38 
moveto 20,50 
color y 
circle 80 
color r 
circle 50 
color b 
moveto 60,100 
lineto 60,80 
lineto 120,80 
lineto 120,100 
lineto 60,100 
lineto 80,50 
lineto 120,80 
lineto 60,100 
moveto 0,100 
color r 
lineto 0,80 
lineto 20,80 
lineto 20,100 
lineto 0,80 
lineto 10,70 
lineto 20,80 
lineto 0,100 
lineto 20,100 
moveto 22,110 
lineto 22,102 
lineto 30,102 
moveto 22,102 
lineto 42,122 
moveto 45,125 
text - My awesome house
    */
