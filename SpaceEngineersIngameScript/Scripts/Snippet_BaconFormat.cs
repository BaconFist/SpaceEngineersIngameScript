using System;
using System.Collections.Generic;
using VRageMath; // VRage.Math.dll
using VRage.Game; // VRage.Game.dll
using System.Text;
using Sandbox.ModAPI.Interfaces; // SandboBaconFormat.x.Common.dll
using Sandbox.ModAPI.Ingame; // SandboBaconFormat.x.Common.dll
using Sandbox.Game.EntityComponents; // SandboBaconFormat.x.Game.dll
using VRage.Game.Components; // VRage.Game.dll
using VRage.Collections; // VRage.Library.dll
using VRage.Game.ObjectBuilders.Definitions; // VRage.Game.dll
using VRage.Game.ModAPI.Ingame; // VRage.Game.dll
using SpaceEngineers.Game.ModAPI.Ingame; // SpacenEngineers.Game.dll

namespace Snippet_BaconFormat
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        Snippet_BaconFormat
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
        }

        public class BaconFormat
        {
            private BaconFontWidthCalc fontWidthCalc = new BaconFontWidthCalc();
            private const char BULLET = '\u2022';

            private string bold(string text)
            {
                return string.Join("\u0020", text.ToCharArray());
            }

            private string right(string text, int availableWidth)
            {
                string slug;
                double diff = availableWidth - fontWidthCalc.getTextWidth(text);
                if(diff < 0)
                {
                    slug = prependSpace(text, diff);
                } else if (diff > 0)
                {
                    slug = removePreSpace(text, diff*-1);
                } else
                {
                    slug = text;
                }
                return slug;
            }

            private string center(string text, int availableWidth)
            {
                string slug;
                int textSize = fontWidthCalc.getTextWidth(text);
                double diff = (availableWidth - textSize) / 2;
                if (diff > 0)
                {
                    slug = prependSpace(text, diff);
                }
                else if(diff < 0)
                {
                    slug = removePreSpace(text, diff*-1);
                } else
                {
                    slug = text;
                }

                return slug;
            }

            private string removePreSpace(string text, double width)
            {
                int cw = 0;
                int i = 0;
                for (i = 0; i < text.Length && width < cw; i++)
                {
                    cw = cw + fontWidthCalc.getChar(text[i]);
                }
                return text.Substring(i);
            }

            private string prependSpace(string text, double width)
            {
                int offset = Convert.ToInt32(Math.Round(width / BaconFontWidthCalc.SPACE_WIDTH));
                return new String('\u0020', offset) + text;
            }           
        }

        public class BaconFontWidthCalc { public const int SPACE_WIDTH = 8; static Dictionary<char, int> i = null; static Dictionary<string, int> j = null; public Dictionary<char, int> CharMap { get { k(); return i; } } public Dictionary<string, int> Cache { get { k(); return j; } } public int getTextWidth(string a) { var b = a.TrimEnd(); if (!Cache.ContainsKey(b)) { string[] c = b.Split(new Char[] { '\u0020' }, StringSplitOptions.RemoveEmptyEntries); int d = 0; for (int e = 0; e < c.Length; e++) { var f = c[e]; if (!Cache.ContainsKey(f)) { int g = 0; for (int h = 0; h < f.Length; h++) g = g + getChar(f[h]); Cache.Add(f, g); } d = d + Cache[f]; } d = d + ((a.Split('\u0020').Length - 1) * SPACE_WIDTH); Cache.Add(a, d); } return Cache[b]; } public int getChar(char a) { return CharMap.ContainsKey(a) ? CharMap[a] : SPACE_WIDTH; } static void k() { if (j == null) j = new Dictionary<string, int>(); if (i == null) { i.Add('\u0021', 8); i.Add('\u0022', 10); i.Add('\u0023', 19); i.Add('\u0024', 20); i.Add('\u0025', 24); i.Add('\u0026', 20); i.Add('\u0027', 6); i.Add('\u0028', 9); i.Add('\u0029', 9); i.Add('\u002a', 11); i.Add('\u002b', 18); i.Add('\u002c', 9); i.Add('\u002d', 10); i.Add('\u002e', 9); i.Add('\u002f', 14); i.Add('\u0030', 19); i.Add('\u0031', 9); i.Add('\u0032', 19); i.Add('\u0033', 17); i.Add('\u0034', 19); i.Add('\u0035', 19); i.Add('\u0036', 19); i.Add('\u0037', 16); i.Add('\u0038', 19); i.Add('\u0039', 19); i.Add('\u003a', 9); i.Add('\u003b', 9); i.Add('\u003c', 18); i.Add('\u003d', 18); i.Add('\u003e', 18); i.Add('\u003f', 16); i.Add('\u0040', 25); i.Add('\u0041', 21); i.Add('\u0042', 21); i.Add('\u0043', 19); i.Add('\u0044', 21); i.Add('\u0045', 18); i.Add('\u0046', 17); i.Add('\u0047', 20); i.Add('\u0048', 20); i.Add('\u0049', 8); i.Add('\u004a', 16); i.Add('\u004b', 17); i.Add('\u004c', 15); i.Add('\u004d', 26); i.Add('\u004e', 21); i.Add('\u004f', 21); i.Add('\u0050', 20); i.Add('\u0051', 21); i.Add('\u0052', 21); i.Add('\u0053', 21); i.Add('\u0054', 17); i.Add('\u0055', 20); i.Add('\u0056', 20); i.Add('\u0057', 31); i.Add('\u0058', 19); i.Add('\u0059', 20); i.Add('\u005a', 19); i.Add('\u005b', 9); i.Add('\u005c', 12); i.Add('\u005d', 9); i.Add('\u005e', 18); i.Add('\u005f', 15); i.Add('\u0060', 8); i.Add('\u0061', 17); i.Add('\u0062', 17); i.Add('\u0063', 16); i.Add('\u0064', 17); i.Add('\u0065', 17); i.Add('\u0066', 9); i.Add('\u0067', 17); i.Add('\u0068', 17); i.Add('\u0069', 8); i.Add('\u006a', 8); i.Add('\u006b', 17); i.Add('\u006c', 8); i.Add('\u006d', 27); i.Add('\u006e', 17); i.Add('\u006f', 17); i.Add('\u0070', 17); i.Add('\u0071', 17); i.Add('\u0072', 10); i.Add('\u0073', 17); i.Add('\u0074', 9); i.Add('\u0075', 17); i.Add('\u0076', 15); i.Add('\u0077', 27); i.Add('\u0078', 15); i.Add('\u0079', 17); i.Add('\u007a', 16); i.Add('\u007b', 9); i.Add('\u007c', 6); i.Add('\u007d', 9); i.Add('\u007e', 18); i.Add('\u00a2', 16); i.Add('\u00a3', 17); i.Add('\u00a4', 19); i.Add('\u00a5', 19); i.Add('\u00a7', 20); i.Add('\u00b5', 17); i.Add('\u00df', 19); i.Add('\u2022', 15); i.Add('\u20ac', 19); } } }
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}