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

namespace Snippet_BaconMonoText
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        BaconMonoText
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
            BaconMonoText BMT = new BaconMonoText();
            BMT.AppendLine("\u2554\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2557");
            BMT.AppendLine("\u2551Hello World\u2551");
            BMT.AppendLine("\u255A\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u255A");
            Console.Write(BMT.ToString().Replace('\uE00F',' ').Replace('\uE00E','#'));
        }
        
        public class BaconMonoText
        {
            private char color = '\uE00E';
            private char background = '\uE00F';
            private Dictionary<char, string[]> FontMap = new Dictionary<char, string[]>();
            private string[] unknownGlyph = new string[] { "00000", "01110", "01010", "01110", "00000" };
            private StringBuilder content = new StringBuilder();
            private int _letterSpacing = 0;
            private int _lineSpacing = 0;

            public int letterSpacing { get { return _letterSpacing; } set { _letterSpacing = (value >= 0) ? value : 0; } }
            public int lineSpacing { get { return _lineSpacing; } set { _lineSpacing = (value>=0)?value:0; } }

            public void Clear()
            {
                content.Clear();
            }

            public void AppendLine(string line)
            {
                string[] slug = new string[5];
                for(int i = 0; i < line.Length; i++)
                {
                    string space = (i > 0)? new String('0', letterSpacing):"";
                    string[] glyph = getGlyph(line[i]);
                    slug[0] += space + glyph[0];
                    slug[1] += space + glyph[1];
                    slug[2] += space + glyph[2];
                    slug[3] += space + glyph[3];
                    slug[4] += space + glyph[4];
                }
                content.AppendLine(slug[0]);
                content.AppendLine(slug[1]);
                content.AppendLine(slug[2]);
                content.AppendLine(slug[3]);
                content.AppendLine(slug[4]);
                for(int i = 0; i < lineSpacing; i++)
                {
                    content.AppendLine();
                }                
            }

            public void AppendLine()
            {
                for(int i=0;i<lineSpacing + 5; i++)
                {
                    content.AppendLine();
                }
            }

            private string[] getGlyph(char glyph)
            {
                char upper = glyph.ToString().ToUpper()[0];
                return FontMap.ContainsKey(glyph) ? FontMap[glyph] : (FontMap.ContainsKey(upper) ? FontMap[upper] : this.unknownGlyph);
            }

            public StringBuilder getContent()
            {
                return content.Replace('0', background).Replace('1', color);
            }

            override public string ToString()
            {
                return getContent().ToString();
            }

            public BaconMonoText()
            {
                FontMap = new Dictionary<char, string[]>();
                FontMap.Add('!', new string[] { "00100", "00100", "00100", "00000", "00100" });
                FontMap.Add('"', new string[] { "01010", "01010", "00000", "00000", "00000" });
                FontMap.Add('#', new string[] { "01010", "11111", "01010", "11111", "01010" });
                FontMap.Add('$', new string[] { "01111", "10100", "01110", "00101", "11110" });
                FontMap.Add('%', new string[] { "11001", "11010", "00100", "01011", "10011" });
                FontMap.Add('\'', new string[] { "00100", "00100", "00000", "00000", "00000" });
                FontMap.Add('(', new string[] { "00010", "00100", "00100", "00100", "00010" });
                FontMap.Add(')', new string[] { "01000", "00100", "00100", "00100", "01000" });
                FontMap.Add('*', new string[] { "01010", "00100", "01010", "00000", "00000" });
                FontMap.Add(',', new string[] { "00000", "00000", "00000", "00100", "01000" });
                FontMap.Add('-', new string[] { "00000", "00000", "01110", "00000", "00000" });
                FontMap.Add('.', new string[] { "00000", "00000", "00000", "00000", "00100" });
                FontMap.Add('/', new string[] { "00001", "00010", "00100", "01000", "10000" });
                FontMap.Add('0', new string[] { "01110", "10011", "10101", "11001", "01110" });
                FontMap.Add('1', new string[] { "00110", "00010", "00010", "00010", "00111" });
                FontMap.Add('2', new string[] { "11110", "00001", "01110", "10000", "11111" });
                FontMap.Add('3', new string[] { "11110", "00001", "00110", "00001", "11110" });
                FontMap.Add('4', new string[] { "00110", "01010", "10010", "11111", "00010" });
                FontMap.Add('5', new string[] { "11111", "10000", "11110", "00001", "11110" });
                FontMap.Add('6', new string[] { "01111", "10000", "11110", "10001", "01110" });
                FontMap.Add('7', new string[] { "11111", "00001", "00010", "00100", "01000" });
                FontMap.Add('8', new string[] { "01110", "10001", "01110", "10001", "01110" });
                FontMap.Add('9', new string[] { "01110", "10001", "01111", "00001", "11110" });
                FontMap.Add(':', new string[] { "00000", "00100", "00000", "00100", "00000" });
                FontMap.Add(';', new string[] { "00000", "00100", "00000", "00100", "01000" });
                FontMap.Add('<', new string[] { "00001", "00010", "00100", "00010", "00001" });
                FontMap.Add('=', new string[] { "00000", "01110", "00000", "01110", "00000" });
                FontMap.Add('>', new string[] { "10000", "01000", "00100", "01000", "10000" });
                FontMap.Add('?', new string[] { "01110", "00001", "00110", "00000", "00100" });
                FontMap.Add('@', new string[] { "01111", "10001", "10111", "10110", "01111" });
                FontMap.Add('A', new string[] { "00100", "01010", "10001", "11111", "10001" });
                FontMap.Add('B', new string[] { "11110", "10001", "11110", "10001", "11110" });
                FontMap.Add('C', new string[] { "01111", "10000", "10000", "10000", "01111" });
                FontMap.Add('D', new string[] { "11110", "10001", "10001", "10001", "11110" });
                FontMap.Add('E', new string[] { "11111", "10000", "11100", "10000", "11111" });
                FontMap.Add('F', new string[] { "11111", "10000", "11100", "10000", "10000" });
                FontMap.Add('G', new string[] { "01111", "10000", "10011", "10001", "01111" });
                FontMap.Add('H', new string[] { "10001", "10001", "11111", "10001", "10001" });
                FontMap.Add('I', new string[] { "11111", "00100", "00100", "00100", "11111" });
                FontMap.Add('J', new string[] { "11111", "00010", "00010", "10010", "01100" });
                FontMap.Add('K', new string[] { "10001", "10010", "11100", "10010", "10001" });
                FontMap.Add('L', new string[] { "10000", "10000", "10000", "10000", "11111" });
                FontMap.Add('M', new string[] { "10001", "11011", "10101", "10001", "10001" });
                FontMap.Add('N', new string[] { "10001", "11001", "10101", "10011", "10001" });
                FontMap.Add('O', new string[] { "01110", "10001", "10001", "10001", "01110" });
                FontMap.Add('P', new string[] { "11110", "10001", "11110", "10000", "10000" });
                FontMap.Add('Q', new string[] { "01110", "10001", "10101", "10010", "01101" });
                FontMap.Add('R', new string[] { "11110", "10001", "11110", "10001", "10001" });
                FontMap.Add('S', new string[] { "01111", "10000", "01110", "00001", "11110" });
                FontMap.Add('T', new string[] { "11111", "00100", "00100", "00100", "00100" });
                FontMap.Add('U', new string[] { "10001", "10001", "10001", "10001", "01110" });
                FontMap.Add('V', new string[] { "10001", "10001", "10001", "01010", "00100" });
                FontMap.Add('W', new string[] { "10001", "10001", "10101", "11011", "10001" });
                FontMap.Add('X', new string[] { "10001", "01010", "00100", "01010", "10001" });
                FontMap.Add('Y', new string[] { "10001", "01010", "00100", "00100", "00100" });
                FontMap.Add('Z', new string[] { "11111", "00010", "00100", "01000", "11111" });
                FontMap.Add('[', new string[] { "00110", "00100", "00100", "00100", "00110" });
                FontMap.Add('\\', new string[] { "10000", "01000", "00100", "00010", "00001" });
                FontMap.Add(']', new string[] { "01100", "00100", "00100", "00100", "01100" });
                FontMap.Add('^', new string[] { "00100", "01010", "00000", "00000", "00000" });
                FontMap.Add('_', new string[] { "00000", "00000", "00000", "00000", "11111" });
                FontMap.Add('`', new string[] { "00100", "00010", "00000", "00000", "00000" });
                FontMap.Add('{', new string[] { "00110", "00100", "01000", "00100", "00110" });
                FontMap.Add('|', new string[] { "00100", "00100", "00100", "00100", "00100" });
                FontMap.Add('}', new string[] { "01100", "00100", "00010", "00100", "01100" });
                FontMap.Add('~', new string[] { "00000", "01000", "10101", "00010", "00000" });
                FontMap.Add(' ', new string[] { "00000", "00000", "00000", "00000", "00000" });
                FontMap.Add('\u2550', new string[] { "00000", "11111", "00000", "11111", "00000" });
                FontMap.Add('\u2551', new string[] { "01010", "01010", "01010", "01010", "01010" });
                FontMap.Add('\u2552', new string[] { "00000", "01111", "01000", "01111", "01000" });
                FontMap.Add('\u2553', new string[] { "00000", "00000", "01111", "01010", "01010" });
                FontMap.Add('\u2554', new string[] { "00000", "01111", "01000", "01011", "01010" });
                FontMap.Add('\u2555', new string[] { "00000", "11100", "00100", "11100", "00100" });
                FontMap.Add('\u2556', new string[] { "00000", "00000", "11110", "01010", "01010" });
                FontMap.Add('\u2557', new string[] { "00000", "11110", "00010", "11010", "01010" });
                FontMap.Add('\u2558', new string[] { "01000", "01111", "01000", "01111", "00000" });
                FontMap.Add('\u2559', new string[] { "01010", "01010", "01111", "00000", "00000" });
                FontMap.Add('\u255A', new string[] { "01010", "01011", "01000", "01111", "00000" });
                FontMap.Add('\u255B', new string[] { "00100", "11100", "00100", "11100", "00000" });
                FontMap.Add('\u255C', new string[] { "01010", "01010", "11110", "00000", "00000" });
                FontMap.Add('\u255D', new string[] { "01010", "11010", "00010", "11110", "00000" });
                FontMap.Add('\u255E', new string[] { "01000", "01111", "01000", "01111", "01000" });
                FontMap.Add('\u255F', new string[] { "01010", "01010", "01011", "01010", "01010" });
                FontMap.Add('\u2560', new string[] { "01010", "01111", "01010", "01111", "01010" });
                FontMap.Add('\u2561', new string[] { "00100", "11100", "00100", "11100", "00100" });
                FontMap.Add('\u2562', new string[] { "01010", "01010", "11010", "01010", "01010" });
                FontMap.Add('\u2563', new string[] { "01010", "11010", "00010", "11010", "01010" });
                FontMap.Add('\u2564', new string[] { "00000", "11111", "00000", "11111", "00100" });
                FontMap.Add('\u2565', new string[] { "00000", "00000", "11111", "01010", "01010" });
                FontMap.Add('\u2566', new string[] { "00000", "11111", "00000", "11011", "01010" });
                FontMap.Add('\u2567', new string[] { "00100", "11111", "00000", "11111", "00000" });
                FontMap.Add('\u2568', new string[] { "01010", "01010", "11111", "00000", "00000" });
                FontMap.Add('\u2569', new string[] { "01010", "11011", "00000", "11111", "00000" });
                FontMap.Add('\u256A', new string[] { "00100", "11111", "00100", "11111", "00100" });
                FontMap.Add('\u256B', new string[] { "01010", "01010", "11111", "01010", "01010" });
                FontMap.Add('\u256C', new string[] { "01010", "11011", "00000", "11011", "01010" });
                FontMap.Add('\u256D', new string[] { "00000", "00000", "00001", "00010", "00100" });
                FontMap.Add('\u256E', new string[] { "00000", "00000", "10000", "01000", "00100" });
                FontMap.Add('\u256F', new string[] { "00100", "01000", "10000", "00000", "00000" });
                FontMap.Add('\u2570', new string[] { "00100", "00010", "00001", "00000", "00000" });
                FontMap.Add('\u2571', new string[] { "00001", "00010", "00100", "01000", "10000" });
                FontMap.Add('\u2572', new string[] { "10000", "01000", "00100", "00010", "00001" });
                FontMap.Add('\u2573', new string[] { "10001", "01010", "00100", "01010", "10001" });
                FontMap.Add('\u2574', new string[] { "00000", "00000", "11100", "00000", "00000" });
                FontMap.Add('\u2575', new string[] { "00100", "00100", "00100", "00000", "00000" });
                FontMap.Add('\u2576', new string[] { "00000", "00000", "00111", "00000", "00000" });
                FontMap.Add('\u2577', new string[] { "00000", "00000", "00100", "00100", "00100" });
            }
        }

        class min
        {
            public class BaconMonoText { char f = '\uE00E'; char g = '\uE00F'; Dictionary<char, string[]> h = new Dictionary<char, string[]>(); string[] i = new string[] { "00000", "01110", "01010", "01110", "00000" }; StringBuilder j = new StringBuilder(); int k = 0; int l = 0; public int letterSpacing { get { return k; } set { k = (value >= 0) ? value : 0; } } public int lineSpacing { get { return l; } set { l = (value >= 0) ? value : 0; } } public void Clear() { j.Clear(); } public void AppendLine(string a) { string[] b = new string[5]; for (int c = 0; c < a.Length; c++) { var d = (c > 0) ? new String('0', letterSpacing) : ""; string[] e = m(a[c]); b[0] += d + e[0]; b[1] += d + e[1]; b[2] += d + e[2]; b[3] += d + e[3]; b[4] += d + e[4]; } j.AppendLine(b[0]); j.AppendLine(b[1]); j.AppendLine(b[2]); j.AppendLine(b[3]); j.AppendLine(b[4]); for (int c = 0; c < lineSpacing; c++) j.AppendLine(); } public void AppendLine() { for (int a = 0; a < lineSpacing + 5; a++) j.AppendLine(); } string[] m(char a) { var b = a.ToString().ToUpper()[0]; return h.ContainsKey(a) ? h[a] : (h.ContainsKey(b) ? h[b] : this.i); } public StringBuilder getContent() { return j.Replace('0', g).Replace('1', f); } override public string ToString() { return getContent().ToString(); } public BaconMonoText() { h = new Dictionary<char, string[]>(); h.Add('!', new string[] { "00100", "00100", "00100", "00000", "00100" }); h.Add('"', new string[] { "01010", "01010", "00000", "00000", "00000" }); h.Add('#', new string[] { "01010", "11111", "01010", "11111", "01010" }); h.Add('$', new string[] { "01111", "10100", "01110", "00101", "11110" }); h.Add('%', new string[] { "11001", "11010", "00100", "01011", "10011" }); h.Add('\'', new string[] { "00100", "00100", "00000", "00000", "00000" }); h.Add('(', new string[] { "00010", "00100", "00100", "00100", "00010" }); h.Add(')', new string[] { "01000", "00100", "00100", "00100", "01000" }); h.Add('*', new string[] { "01010", "00100", "01010", "00000", "00000" }); h.Add(',', new string[] { "00000", "00000", "00000", "00100", "01000" }); h.Add('-', new string[] { "00000", "00000", "01110", "00000", "00000" }); h.Add('.', new string[] { "00000", "00000", "00000", "00000", "00100" }); h.Add('/', new string[] { "00001", "00010", "00100", "01000", "10000" }); h.Add('0', new string[] { "01110", "10011", "10101", "11001", "01110" }); h.Add('1', new string[] { "00110", "00010", "00010", "00010", "00111" }); h.Add('2', new string[] { "11110", "00001", "01110", "10000", "11111" }); h.Add('3', new string[] { "11110", "00001", "00110", "00001", "11110" }); h.Add('4', new string[] { "00110", "01010", "10010", "11111", "00010" }); h.Add('5', new string[] { "11111", "10000", "11110", "00001", "11110" }); h.Add('6', new string[] { "01111", "10000", "11110", "10001", "01110" }); h.Add('7', new string[] { "11111", "00001", "00010", "00100", "01000" }); h.Add('8', new string[] { "01110", "10001", "01110", "10001", "01110" }); h.Add('9', new string[] { "01110", "10001", "01111", "00001", "11110" }); h.Add(':', new string[] { "00000", "00100", "00000", "00100", "00000" }); h.Add(';', new string[] { "00000", "00100", "00000", "00100", "01000" }); h.Add('<', new string[] { "00001", "00010", "00100", "00010", "00001" }); h.Add('=', new string[] { "00000", "01110", "00000", "01110", "00000" }); h.Add('>', new string[] { "10000", "01000", "00100", "01000", "10000" }); h.Add('?', new string[] { "01110", "00001", "00110", "00000", "00100" }); h.Add('@', new string[] { "01111", "10001", "10111", "10110", "01111" }); h.Add('A', new string[] { "00100", "01010", "10001", "11111", "10001" }); h.Add('B', new string[] { "11110", "10001", "11110", "10001", "11110" }); h.Add('C', new string[] { "01111", "10000", "10000", "10000", "01111" }); h.Add('D', new string[] { "11110", "10001", "10001", "10001", "11110" }); h.Add('E', new string[] { "11111", "10000", "11100", "10000", "11111" }); h.Add('F', new string[] { "11111", "10000", "11100", "10000", "10000" }); h.Add('G', new string[] { "01111", "10000", "10011", "10001", "01111" }); h.Add('H', new string[] { "10001", "10001", "11111", "10001", "10001" }); h.Add('I', new string[] { "11111", "00100", "00100", "00100", "11111" }); h.Add('J', new string[] { "11111", "00010", "00010", "10010", "01100" }); h.Add('K', new string[] { "10001", "10010", "11100", "10010", "10001" }); h.Add('L', new string[] { "10000", "10000", "10000", "10000", "11111" }); h.Add('M', new string[] { "10001", "11011", "10101", "10001", "10001" }); h.Add('N', new string[] { "10001", "11001", "10101", "10011", "10001" }); h.Add('O', new string[] { "01110", "10001", "10001", "10001", "01110" }); h.Add('P', new string[] { "11110", "10001", "11110", "10000", "10000" }); h.Add('Q', new string[] { "01110", "10001", "10101", "10010", "01101" }); h.Add('R', new string[] { "11110", "10001", "11110", "10001", "10001" }); h.Add('S', new string[] { "01111", "10000", "01110", "00001", "11110" }); h.Add('T', new string[] { "11111", "00100", "00100", "00100", "00100" }); h.Add('U', new string[] { "10001", "10001", "10001", "10001", "01110" }); h.Add('V', new string[] { "10001", "10001", "10001", "01010", "00100" }); h.Add('W', new string[] { "10001", "10001", "10101", "11011", "10001" }); h.Add('X', new string[] { "10001", "01010", "00100", "01010", "10001" }); h.Add('Y', new string[] { "10001", "01010", "00100", "00100", "00100" }); h.Add('Z', new string[] { "11111", "00010", "00100", "01000", "11111" }); h.Add('[', new string[] { "00110", "00100", "00100", "00100", "00110" }); h.Add('\\', new string[] { "10000", "01000", "00100", "00010", "00001" }); h.Add(']', new string[] { "01100", "00100", "00100", "00100", "01100" }); h.Add('^', new string[] { "00100", "01010", "00000", "00000", "00000" }); h.Add('_', new string[] { "00000", "00000", "00000", "00000", "11111" }); h.Add('`', new string[] { "00100", "00010", "00000", "00000", "00000" }); h.Add('{', new string[] { "00110", "00100", "01000", "00100", "00110" }); h.Add('|', new string[] { "00100", "00100", "00100", "00100", "00100" }); h.Add('}', new string[] { "01100", "00100", "00010", "00100", "01100" }); h.Add('~', new string[] { "00000", "01000", "10101", "00010", "00000" }); h.Add(' ', new string[] { "00000", "00000", "00000", "00000", "00000" }); h.Add('\u2550', new string[] { "00000", "11111", "00000", "11111", "00000" }); h.Add('\u2551', new string[] { "01010", "01010", "01010", "01010", "01010" }); h.Add('\u2552', new string[] { "00000", "01111", "01000", "01111", "01000" }); h.Add('\u2553', new string[] { "00000", "00000", "01111", "01010", "01010" }); h.Add('\u2554', new string[] { "00000", "01111", "01000", "01011", "01010" }); h.Add('\u2555', new string[] { "00000", "11100", "00100", "11100", "00100" }); h.Add('\u2556', new string[] { "00000", "00000", "11110", "01010", "01010" }); h.Add('\u2557', new string[] { "00000", "11110", "00010", "11010", "01010" }); h.Add('\u2558', new string[] { "01000", "01111", "01000", "01111", "00000" }); h.Add('\u2559', new string[] { "01010", "01010", "01111", "00000", "00000" }); h.Add('\u255A', new string[] { "01010", "01011", "01000", "01111", "00000" }); h.Add('\u255B', new string[] { "00100", "11100", "00100", "11100", "00000" }); h.Add('\u255C', new string[] { "01010", "01010", "11110", "00000", "00000" }); h.Add('\u255D', new string[] { "01010", "11010", "00010", "11110", "00000" }); h.Add('\u255E', new string[] { "01000", "01111", "01000", "01111", "01000" }); h.Add('\u255F', new string[] { "01010", "01010", "01011", "01010", "01010" }); h.Add('\u2560', new string[] { "01010", "01111", "01010", "01111", "01010" }); h.Add('\u2561', new string[] { "00100", "11100", "00100", "11100", "00100" }); h.Add('\u2562', new string[] { "01010", "01010", "11010", "01010", "01010" }); h.Add('\u2563', new string[] { "01010", "11010", "00010", "11010", "01010" }); h.Add('\u2564', new string[] { "00000", "11111", "00000", "11111", "00100" }); h.Add('\u2565', new string[] { "00000", "00000", "11111", "01010", "01010" }); h.Add('\u2566', new string[] { "00000", "11111", "00000", "11011", "01010" }); h.Add('\u2567', new string[] { "00100", "11111", "00000", "11111", "00000" }); h.Add('\u2568', new string[] { "01010", "01010", "11111", "00000", "00000" }); h.Add('\u2569', new string[] { "01010", "11011", "00000", "11111", "00000" }); h.Add('\u256A', new string[] { "00100", "11111", "00100", "11111", "00100" }); h.Add('\u256B', new string[] { "01010", "01010", "11111", "01010", "01010" }); h.Add('\u256C', new string[] { "01010", "11011", "00000", "11011", "01010" }); h.Add('\u256D', new string[] { "00000", "00000", "00001", "00010", "00100" }); h.Add('\u256E', new string[] { "00000", "00000", "10000", "01000", "00100" }); h.Add('\u256F', new string[] { "00100", "01000", "10000", "00000", "00000" }); h.Add('\u2570', new string[] { "00100", "00010", "00001", "00000", "00000" }); h.Add('\u2571', new string[] { "00001", "00010", "00100", "01000", "10000" }); h.Add('\u2572', new string[] { "10000", "01000", "00100", "00010", "00001" }); h.Add('\u2573', new string[] { "10001", "01010", "00100", "01010", "10001" }); h.Add('\u2574', new string[] { "00000", "00000", "11100", "00000", "00000" }); h.Add('\u2575', new string[] { "00100", "00100", "00100", "00000", "00000" }); h.Add('\u2576', new string[] { "00000", "00000", "00111", "00000", "00000" }); h.Add('\u2577', new string[] { "00000", "00000", "00100", "00100", "00100" }); } }
        }

        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}