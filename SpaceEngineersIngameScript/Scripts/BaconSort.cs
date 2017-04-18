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

namespace BaconSort
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        BaconSort
        ==============
        Copyright 2016 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE
        Source: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/SpaceEngineersIngameScript/Scripts/BaconSort.cs
        
        Inventory Sorting
        - Sort Items by tags in the Blockname
                Tags: #ammo, #component, #bottle, #ingot, #ore, #handtool
        - Ignore all Reactors by default (can be changed by arguments)
        - Ignore all Weapons by default (can be changed by arguments)
        - Ignore all containers with "#!BaconSort" in their Name (can be changed by arguments)
        - Ignore docked Ships & Stations (can be changed by arguments)        
        - Sorts only once every 5 minutes (can be changed by arguments)
 
        How it works: 
        * Add any tag to a Container with an Inventory to pull items of this type in it. 
        * Script is lazy, it will sort a item only if it is not in a Container with a matching tag. (Example: It will not pull an SteelPlate from an container called "Large Cargo Container #component") 
        * Source & Target Block must match some requirements: 1. Same Grid as PB, 2. Must be connected throug Conveyor-System
            
        
            

        Arguments for Storting:
       ====================================
            --reactors
                Also sort Inventories of reactors
            --weapons
                Also sort Inventories of Weapons
            --docked
                Also sort inventories from docked Ships and Stations
            --ignore="TAG"
                ignore all Blocks with TAG in its name where TAG defaults to "#!BaconSort"
               

        Arguments for Logging:
       ====================================
            --log-noecho    
                Disable log messages in Detailed Info

            --log-lcd="TAG"
                Print Log Messages on TextPanels/LCDs where TAG is in its Name 
                  
            --log-filter="F" (Default: fatal,error,warn,info)
                Only log messages if type "F" wehre F can be any of TRACE,DEBUG,INFO,WARN,ERROR,FATAL,ALL.
                Can be defined more than once (like --log-filter="info" --log-filter="warn")
                Can also be defined like --log-filter="debug,WARN,Fatal".
                "ALL" is equal to a combination of all other filters.

        Arguments for other stuff:
       ==================================
            --sleep="N"
                scipt waits N seconfs before executing sorting loop where N defaults to 300 (5 minutes)
                    //this is a workaround for flashing timer bug on Dedicated Servers.
                     
        **/


        public void Main(string argument)
        {
            BaconArgs Args = BaconArgs.parse(argument);
            BootstrapLog(Args);
            try {
                Log?.Debug("Stat Script at {0}", DateTime.Now);
                if (isAllowedToExecute(Args))
                {
                    Run(Args);
                }
            } catch(Exception e) {
                Log?.Fatal("Exception of Type {0} occured. Script Execution Terminated. => {1}", e.GetType().Name, e.Message);
            } finally
            {
                Log?.Flush();
            }
        }

        private void Run(BaconArgs Args)
        {
            Log?.PushStack("private void Run()");
            if (Args.hasOption("reactors"))
            {
                INCLUDE_REACTORS = true;
                Log?.Info("Include reactors in sorting.");
            }
            if (Args.hasOption("weapons"))
            {
                INCLUDE_WEAPONS = true;
                Log?.Info("Include weapons in sorting.");
            }
            if (Args.hasOption("docked"))
            {
                INCLUDE_DOCKED = true;
                Log?.Info("Include docked in sorting.");
            }
            if(Args.hasOption("ignore") && Args.getOption("ignore")[0] != null)
            {
                TAG_IGNORE = Args.getOption("ignore")[0];
                Log?.Info("Use Tag \"{0}\" instead of \"#!BaconSort\" to exclude containers.", TAG_IGNORE);
            }
            List<IMyTerminalBlock> SourceBlocks = findAllBlocks();
            Dictionary<string, List<IMyTerminalBlock>> DestinationBlockMap = getDestinationMap(SourceBlocks);
            foreach (IMyTerminalBlock SourceBlock in SourceBlocks)
            {
                DoSortContainer(SourceBlock, DestinationBlockMap);
            }
            Log?.PopStack();
        }

        #region execution time control
        private bool isAllowedToExecute(BaconArgs Args)
        {
            if(LastRun == null)
            {
                return true;
            }
            int buff = 0;
            if(Args.hasOption("sleep") && int.TryParse(Args.getOption("sleep")[0], out buff))
            {
                sleepTimeS = buff;
            }
            Log?.Debug("execution limit once every {0} seconds", sleepTimeS);

            TimeSpan TimeSinceLastRun = DateTime.Now.Subtract(LastRun);
            if(sleepTimeS <= TimeSinceLastRun.TotalSeconds)
            {
                LastRun = DateTime.Now;
                return true;
            }

            Log?.Info("Sorting in {0} seconds.", sleepTimeS - TimeSinceLastRun.TotalSeconds);
            return false;
        }

        private DateTime LastRun;
        private int sleepTimeS = 300;
        #endregion execution time control

        #region Log
        BMyLog4PB Log;
        byte log_Filter = BMyLog4PB.E_FATAL | BMyLog4PB.E_ERROR | BMyLog4PB.E_WARN | BMyLog4PB.E_INFO;


        private void BootstrapLog(BaconArgs Args)
        {
            Log = new BMyLog4PB(this, 0);
            if (!Args.hasOption("log-noecho"))
            {
                Log?.AddAppender(new BMyLog4PB.BMyEchoAppender(this));
            }            
            if (Args.hasOption("log-lcd"))
            {
                string logLcdTag = Args.getOption("log-lcd")[0];
                if(logLcdTag != null)
                {
                    Log?.AddAppender(new BMyLog4PB.BMyTextPanelAppender(logLcdTag,this));
                }
            }            
            if (Args.hasOption("log-filter") && Args.getOption("log-filter").Count > 0)
            {
                log_Filter = 0;
                foreach(string filterArgValue in Args.getOption("log-filter"))
                {
                    string[] filterList = filterArgValue.ToLowerInvariant().Split(new Char[]{ ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string filter in filterList)
                    {
                        switch (filter)
                        {
                            case "trace":
                                log_Filter |= BMyLog4PB.E_TRACE;
                                break;
                            case "debug":
                                log_Filter |= BMyLog4PB.E_DEBUG;
                                break;
                            case "info":
                                log_Filter |= BMyLog4PB.E_INFO;
                                break;
                            case "warn":
                                log_Filter |= BMyLog4PB.E_WARN;
                                break;
                            case "error":
                                log_Filter |= BMyLog4PB.E_ERROR;
                                break;
                            case "fatal":
                                log_Filter |= BMyLog4PB.E_FATAL;
                                break;
                            case "all":
                                log_Filter |= BMyLog4PB.E_ALL;
                                break;
                        }
                    }
                }
            }
            if (Log != null) {
                Log.Filter = log_Filter;
            }
        }
        #endregion Log

        #region sorting

        private Dictionary<string, string> TypeTagMap = new Dictionary<string, string>() {
            {"MyObjectBuilder_AmmoMagazine", "#ammo"},
            {"MyObjectBuilder_Component", "#component"},
            {"MyObjectBuilder_GasContainerObject", "#bottle"},
            {"MyObjectBuilder_OxygenContainerObject", "#bottle"},
            {"MyObjectBuilder_Ingot", "#ingot"},
            {"MyObjectBuilder_Ore", "#ore"},
            {"MyObjectBuilder_PhysicalGunObject", "#handtool"}
        };
        
        private string TAG_IGNORE = "#!BaconSort";
        private bool INCLUDE_REACTORS = false;
        private bool INCLUDE_WEAPONS = false;
        private bool INCLUDE_DOCKED = false;

        private void DoSortContainer(IMyTerminalBlock SourceBlock, Dictionary<string, List<IMyTerminalBlock>> DestinationMap)
        {
            Log?.PushStack("private void DoSortContainer(IMyTerminalBlock SourceBlock, Dictionary<string, List<IMyTerminalBlock>> DestinationMap)");
            for(int i_SourceBlockInventory = 0; i_SourceBlockInventory < SourceBlock.InventoryCount; i_SourceBlockInventory++)
            {
                IMyInventory InventorySource = SourceBlock.GetInventory(i_SourceBlockInventory);
                for(int i_Item=InventorySource.GetItems().Count-1;i_Item>=0;i_Item--)
                {
                    bool isItemPending = true;
                    IMyInventoryItem Item = InventorySource.GetItems()[i_Item];
                    string typeId = Item.Content.TypeId.ToString();
                    if (TypeTagMap.ContainsKey(typeId))
                    {
                        string tag = TypeTagMap[typeId];
                        if (SourceBlock.CustomName.Contains(tag))
                        {
                            if (DestinationMap.ContainsKey(tag) && DestinationMap[tag].Count > 0)
                            {
                                Log?.Info("Found {0} Containers for \"{1}\"", DestinationMap[tag].Count, tag);
                                for(int i_DestinationBlock = 0; isItemPending && i_DestinationBlock < DestinationMap[tag].Count; i_DestinationBlock++)
                                {
                                    IMyTerminalBlock DestinationBlock = DestinationMap[tag][i_DestinationBlock];
                                    for(int i_DestinationBlockInventory = 0; isItemPending && i_DestinationBlockInventory < DestinationBlock.InventoryCount; i_DestinationBlockInventory++)
                                    {
                                        IMyInventory InventoryDestination = DestinationBlock.GetInventory(i_DestinationBlockInventory);
                                        if (InventorySource.IsConnectedTo(InventoryDestination))
                                        {
                                            VRage.MyFixedPoint lastAmount = Item.Amount;
                                            InventorySource.TransferItemTo(InventoryDestination, i_Item, null, true);
                                            VRage.MyFixedPoint movedAmount = lastAmount - Item.Amount;
                                            isItemPending = Item.Amount > 0;
                                            Log?.Info("Moved {0} of {1} from \"{2}\" to \"{3}\". Remaining: {4}", movedAmount, Item.Content.SubtypeName, SourceBlock.CustomName, DestinationBlock.CustomName, Item.Amount);
                                        } else
                                        {
                                            Log?.Error("Block \"{0}\" has no connection to Block \"{1}\". Please check your conveyors.", SourceBlock.CustomName, DestinationBlock.CustomName);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Log?.Info("No destination for \"{0}\" defined. Skip Item", tag);
                            }
                        } else
                        {
                            Log?.Info("Item already sorted correctly. Skip Item");
                        }
                        
                    } else
                    {
                        Log?.Error("Unknown Type \"{0}\". Skip Item", typeId);
                    }
                    if (isItemPending)
                    {
                        Log?.Warn("Cant move all of {0} from \"{1}\"", Item.Content.SubtypeName, SourceBlock.CustomName);
                    }
                }
            }
            Log?.PopStack();
        }

        private Dictionary<string, List<IMyTerminalBlock>> getDestinationMap(List<IMyTerminalBlock> OriginBlocks)
        {
            Log?.PushStack("private Dictionary<string, List<IMyTerminalBlock>> getDestinationMap(List<IMyTerminalBlock> OriginBlocks)");
            Dictionary<string, List<IMyTerminalBlock>> Buffer = new Dictionary<string, List<IMyTerminalBlock>>();
            foreach (IMyTerminalBlock Block in OriginBlocks)
            {
                foreach (KeyValuePair<string, string> slug in TypeTagMap)
                {
                    string tag = slug.Value;
                    if (Block.CustomName.Contains(tag))
                    {
                        if (!Buffer.ContainsKey(tag))
                        {
                            Buffer.Add(slug.Value, new List<IMyTerminalBlock>());
                        }
                        Buffer[tag].Add(Block);
                        Log?.Info("Container {0} added as Destination for {1}", Block.CustomName, tag);
                    }
                }
            }
            Log?.PopStack();
            return Buffer;
        }

        private List<IMyTerminalBlock> findAllBlocks()
        {
            Log?.PushStack("private List<IMyTerminalBlock> findAllBlocks()");
            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(Blocks, 
                (b => 
                    b.HasInventory
                && 
                    !b.CustomName.Contains(TAG_IGNORE)
                &&
                    (
                        INCLUDE_DOCKED 
                        ||
                        b.CubeGrid.Equals(Me.CubeGrid)
                    )
                &&
                    (
                        INCLUDE_REACTORS
                        ||
                        !(b is IMyReactor)
                    )
                &&
                    (
                        INCLUDE_WEAPONS
                        ||
                        (
                            !(b is IMySmallGatlingGun)
                            &&
                            !(b is IMySmallMissileLauncherReload)
                            &&
                            !(b is IMySmallMissileLauncher)
                            &&
                            !(b is IMyLargeMissileTurret)
                            &&
                            !(b is IMyLargeInteriorTurret)
                            &&
                            !(b is IMyLargeGatlingTurret)                        
                       )
                    )
                )
            );
            Log?.Debug("Found {0} Blocks to be sorted.", Blocks.Count);
            Log?.PopStack();
            return Blocks;
        }
        #endregion sorting




        #region includes
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
            public abstract class BMyAppenderBase
            {
                public abstract void Enqueue(string message);
                public abstract void Flush();
            }
        }
        public class BaconArgs { public string Raw; static public BaconArgs parse(string a) { return (new Parser()).parseArgs(a); } static public string Escape(object a) { return string.Format("{0}", a).Replace(@"\", @"\\").Replace(@" ", @"\ ").Replace(@"""", @"\"""); } static public string UnEscape(string a) { return a.Replace(@"\""", @"""").Replace(@"\ ", @" ").Replace(@"\\", @"\"); } public class Parser { static Dictionary<string, BaconArgs> h = new Dictionary<string, BaconArgs>(); public BaconArgs parseArgs(string a) { if (!h.ContainsKey(a)) { var b = new BaconArgs(); b.Raw = a; var c = false; var d = false; var e = new StringBuilder(); for (int f = 0; f < a.Length; f++) { var g = a[f]; if (c) { e.Append(g); c = false; } else if (g.Equals('\\')) c = true; else if (d && !g.Equals('"')) e.Append(g); else if (g.Equals('"')) d = !d; else if (g.Equals(' ')) if (e.ToString().Equals("--")) { b.add(a.Substring(f).TrimStart()); e.Clear(); break; } else { b.add(e.ToString()); e.Clear(); } else e.Append(g); } if (e.Length > 0) b.add(e.ToString()); h.Add(a, b); } return h[a]; } } protected Dictionary<char, int> h = new Dictionary<char, int>(); protected List<string> i = new List<string>(); protected Dictionary<string, List<string>> j = new Dictionary<string, List<string>>(); public List<string> getArguments() { return i; } public int getFlag(char a) { return h.ContainsKey(a) ? h[a] : 0; } public bool hasArguments() { return i.Count > 0; } public bool hasOption(string a) { return j.ContainsKey(a); } public List<string> getOption(string a) { return j.ContainsKey(a) ? j[a] : new List<string>(); } public void add(string a) { if (a.Trim().Length > 0) if (!a.StartsWith("-")) { i.Add(a); } else if (a.StartsWith("--")) { KeyValuePair<string, string> b = k(a); string c = b.Key.Substring(2); if (!j.ContainsKey(c)) { j.Add(c, new List<string>()); } j[c].Add(b.Value); } else { string b = a.Substring(1); for (int d = 0; d < b.Length; d++) { if (this.h.ContainsKey(b[d])) { this.h[b[d]]++; } else { this.h.Add(b[d], 1); } } } } KeyValuePair<string, string> k(string a) { string[] b = a.Split(new char[] { '=' }, 2); return new KeyValuePair<string, string>(b[0], (b.Length > 1) ? b[1] : null); } public string ToArguments() { var a = new List<string>(); foreach (string argument in this.getArguments()) a.Add(Escape(argument)); foreach (KeyValuePair<string, List<string>> option in this.j) { var b = "--" + Escape(option.Key); foreach (string optVal in option.Value) a.Add(b + ((optVal != null) ? "=" + Escape(optVal) : "")); } var c = (h.Count > 0) ? "-" : ""; foreach (KeyValuePair<char, int> flag in h) c += new String(flag.Key, flag.Value); a.Add(c); return String.Join(" ", a.ToArray()); } override public string ToString() { var a = new List<string>(); foreach (string key in j.Keys) a.Add(l(key) + ":[" + string.Join(",", j[key].ConvertAll<string>(b => l(b)).ToArray()) + "]"); var c = new List<string>(); foreach (char key in h.Keys) c.Add(key + ":" + h[key].ToString()); var d = new StringBuilder(); d.Append("{\"a\":["); d.Append(string.Join(",", i.ConvertAll<string>(b => l(b)).ToArray())); d.Append("],\"o\":[{"); d.Append(string.Join("},{", a)); d.Append("}],\"f\":[{"); d.Append(string.Join("},{", c)); d.Append("}]}"); return d.ToString(); } string l(string a) { return (a != null) ? "\"" + a.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" : @"null"; } }
        #endregion includes
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}