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

namespace BaconFontWidthCalc
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game
        public class BaconFontWidthCalc
        {
            public const int SPACE_WIDTH = 8;
            static private Dictionary<char, int> _chars = null;
            static private Dictionary<string, int> _cache = null;
            public Dictionary<char, int> CharMap { get { bootstrap(); return _chars; } }
            public Dictionary<string, int> Cache { get { bootstrap(); return _cache; } }

            public int getTextWidth(string text)
            {
                string slug = text.TrimEnd();
                if (!Cache.ContainsKey(slug))
                {
                    string[] words = slug.Split(new Char[] { '\u0020' }, StringSplitOptions.RemoveEmptyEntries);
                    int textWidth = 0;
                    for (int i_words = 0; i_words < words.Length; i_words++)
                    {
                        string word = words[i_words];
                        if (!Cache.ContainsKey(word))
                        {
                            int wordWidth = 0;
                            for (int i_char = 0; i_char < word.Length; i_char++)
                            {
                                wordWidth = wordWidth + getChar(word[i_char]);
                            }
                            Cache.Add(word, wordWidth);
                        }
                        textWidth = textWidth + Cache[word];
                    }
                    textWidth = textWidth + ((text.Split('\u0020').Length - 1) * SPACE_WIDTH);
                    Cache.Add(text, textWidth);
                }
                return Cache[slug];
            }

            public int getChar(char c)
            {
                return CharMap.ContainsKey(c) ? CharMap[c] : SPACE_WIDTH;
            }

            static private void bootstrap()
            {
                if (_cache == null)
                {
                    _cache = new Dictionary<string, int>();
                }
                if (_chars == null)
                {
                    _chars.Add('\u0021', 8); //!
                    _chars.Add('\u0022', 10); //"
                    _chars.Add('\u0023', 19); //#
                    _chars.Add('\u0024', 20); //$
                    _chars.Add('\u0025', 24); //%
                    _chars.Add('\u0026', 20); //&
                    _chars.Add('\u0027', 6); //'
                    _chars.Add('\u0028', 9); //(
                    _chars.Add('\u0029', 9); //)
                    _chars.Add('\u002a', 11); //*
                    _chars.Add('\u002b', 18); //+
                    _chars.Add('\u002c', 9); //,
                    _chars.Add('\u002d', 10); //-
                    _chars.Add('\u002e', 9); //.
                    _chars.Add('\u002f', 14); ///
                    _chars.Add('\u0030', 19); //0
                    _chars.Add('\u0031', 9); //1
                    _chars.Add('\u0032', 19); //2
                    _chars.Add('\u0033', 17); //3
                    _chars.Add('\u0034', 19); //4
                    _chars.Add('\u0035', 19); //5
                    _chars.Add('\u0036', 19); //6
                    _chars.Add('\u0037', 16); //7
                    _chars.Add('\u0038', 19); //8
                    _chars.Add('\u0039', 19); //9
                    _chars.Add('\u003a', 9); //:
                    _chars.Add('\u003b', 9); //;
                    _chars.Add('\u003c', 18); //<
                    _chars.Add('\u003d', 18); //=
                    _chars.Add('\u003e', 18); //>
                    _chars.Add('\u003f', 16); //?
                    _chars.Add('\u0040', 25); //@
                    _chars.Add('\u0041', 21); //A
                    _chars.Add('\u0042', 21); //B
                    _chars.Add('\u0043', 19); //C
                    _chars.Add('\u0044', 21); //D
                    _chars.Add('\u0045', 18); //E
                    _chars.Add('\u0046', 17); //F
                    _chars.Add('\u0047', 20); //G
                    _chars.Add('\u0048', 20); //H
                    _chars.Add('\u0049', 8); //I
                    _chars.Add('\u004a', 16); //J
                    _chars.Add('\u004b', 17); //K
                    _chars.Add('\u004c', 15); //L
                    _chars.Add('\u004d', 26); //M
                    _chars.Add('\u004e', 21); //N
                    _chars.Add('\u004f', 21); //O
                    _chars.Add('\u0050', 20); //P
                    _chars.Add('\u0051', 21); //Q
                    _chars.Add('\u0052', 21); //R
                    _chars.Add('\u0053', 21); //S
                    _chars.Add('\u0054', 17); //T
                    _chars.Add('\u0055', 20); //U
                    _chars.Add('\u0056', 20); //V
                    _chars.Add('\u0057', 31); //W
                    _chars.Add('\u0058', 19); //X
                    _chars.Add('\u0059', 20); //Y
                    _chars.Add('\u005a', 19); //Z
                    _chars.Add('\u005b', 9); //[
                    _chars.Add('\u005c', 12); //\
                    _chars.Add('\u005d', 9); //]
                    _chars.Add('\u005e', 18); //^
                    _chars.Add('\u005f', 15); //_
                    _chars.Add('\u0060', 8); //`
                    _chars.Add('\u0061', 17); //a
                    _chars.Add('\u0062', 17); //b
                    _chars.Add('\u0063', 16); //c
                    _chars.Add('\u0064', 17); //d
                    _chars.Add('\u0065', 17); //e
                    _chars.Add('\u0066', 9); //f
                    _chars.Add('\u0067', 17); //g
                    _chars.Add('\u0068', 17); //h
                    _chars.Add('\u0069', 8); //i
                    _chars.Add('\u006a', 8); //j
                    _chars.Add('\u006b', 17); //k
                    _chars.Add('\u006c', 8); //l
                    _chars.Add('\u006d', 27); //m
                    _chars.Add('\u006e', 17); //n
                    _chars.Add('\u006f', 17); //o
                    _chars.Add('\u0070', 17); //p
                    _chars.Add('\u0071', 17); //q
                    _chars.Add('\u0072', 10); //r
                    _chars.Add('\u0073', 17); //s
                    _chars.Add('\u0074', 9); //t
                    _chars.Add('\u0075', 17); //u
                    _chars.Add('\u0076', 15); //v
                    _chars.Add('\u0077', 27); //w
                    _chars.Add('\u0078', 15); //x
                    _chars.Add('\u0079', 17); //y
                    _chars.Add('\u007a', 16); //z
                    _chars.Add('\u007b', 9); //{
                    _chars.Add('\u007c', 6); //|
                    _chars.Add('\u007d', 9); //}
                    _chars.Add('\u007e', 18); //~
                    _chars.Add('\u00a2', 16); //¢
                    _chars.Add('\u00a3', 17); //£
                    _chars.Add('\u00a4', 19); //¤
                    _chars.Add('\u00a5', 19); //¥
                    _chars.Add('\u00a7', 20); //§
                    _chars.Add('\u00b5', 17); //µ
                    _chars.Add('\u00df', 19); //ß
                    _chars.Add('\u2022', 15); //•
                    _chars.Add('\u20ac', 19); //€
                }
            }
        }
        
        public class min
        {
            public class BaconFontWidthCalc { public const int SPACE_WIDTH = 8; static Dictionary<char, int> i = null; static Dictionary<string, int> j = null; public Dictionary<char, int> CharMap { get { k(); return i; } } public Dictionary<string, int> Cache { get { k(); return j; } } public int getTextWidth(string a) { var b = a.TrimEnd(); if (!Cache.ContainsKey(b)) { string[] c = b.Split(new Char[] { '\u0020' }, StringSplitOptions.RemoveEmptyEntries); int d = 0; for (int e = 0; e < c.Length; e++) { var f = c[e]; if (!Cache.ContainsKey(f)) { int g = 0; for (int h = 0; h < f.Length; h++) g = g + getChar(f[h]); Cache.Add(f, g); } d = d + Cache[f]; } d = d + ((a.Split('\u0020').Length - 1) * SPACE_WIDTH); Cache.Add(a, d); } return Cache[b]; } public int getChar(char a) { return CharMap.ContainsKey(a) ? CharMap[a] : SPACE_WIDTH; } static void k() { if (j == null) j = new Dictionary<string, int>(); if (i == null) { i.Add('\u0021', 8); i.Add('\u0022', 10); i.Add('\u0023', 19); i.Add('\u0024', 20); i.Add('\u0025', 24); i.Add('\u0026', 20); i.Add('\u0027', 6); i.Add('\u0028', 9); i.Add('\u0029', 9); i.Add('\u002a', 11); i.Add('\u002b', 18); i.Add('\u002c', 9); i.Add('\u002d', 10); i.Add('\u002e', 9); i.Add('\u002f', 14); i.Add('\u0030', 19); i.Add('\u0031', 9); i.Add('\u0032', 19); i.Add('\u0033', 17); i.Add('\u0034', 19); i.Add('\u0035', 19); i.Add('\u0036', 19); i.Add('\u0037', 16); i.Add('\u0038', 19); i.Add('\u0039', 19); i.Add('\u003a', 9); i.Add('\u003b', 9); i.Add('\u003c', 18); i.Add('\u003d', 18); i.Add('\u003e', 18); i.Add('\u003f', 16); i.Add('\u0040', 25); i.Add('\u0041', 21); i.Add('\u0042', 21); i.Add('\u0043', 19); i.Add('\u0044', 21); i.Add('\u0045', 18); i.Add('\u0046', 17); i.Add('\u0047', 20); i.Add('\u0048', 20); i.Add('\u0049', 8); i.Add('\u004a', 16); i.Add('\u004b', 17); i.Add('\u004c', 15); i.Add('\u004d', 26); i.Add('\u004e', 21); i.Add('\u004f', 21); i.Add('\u0050', 20); i.Add('\u0051', 21); i.Add('\u0052', 21); i.Add('\u0053', 21); i.Add('\u0054', 17); i.Add('\u0055', 20); i.Add('\u0056', 20); i.Add('\u0057', 31); i.Add('\u0058', 19); i.Add('\u0059', 20); i.Add('\u005a', 19); i.Add('\u005b', 9); i.Add('\u005c', 12); i.Add('\u005d', 9); i.Add('\u005e', 18); i.Add('\u005f', 15); i.Add('\u0060', 8); i.Add('\u0061', 17); i.Add('\u0062', 17); i.Add('\u0063', 16); i.Add('\u0064', 17); i.Add('\u0065', 17); i.Add('\u0066', 9); i.Add('\u0067', 17); i.Add('\u0068', 17); i.Add('\u0069', 8); i.Add('\u006a', 8); i.Add('\u006b', 17); i.Add('\u006c', 8); i.Add('\u006d', 27); i.Add('\u006e', 17); i.Add('\u006f', 17); i.Add('\u0070', 17); i.Add('\u0071', 17); i.Add('\u0072', 10); i.Add('\u0073', 17); i.Add('\u0074', 9); i.Add('\u0075', 17); i.Add('\u0076', 15); i.Add('\u0077', 27); i.Add('\u0078', 15); i.Add('\u0079', 17); i.Add('\u007a', 16); i.Add('\u007b', 9); i.Add('\u007c', 6); i.Add('\u007d', 9); i.Add('\u007e', 18); i.Add('\u00a2', 16); i.Add('\u00a3', 17); i.Add('\u00a4', 19); i.Add('\u00a5', 19); i.Add('\u00a7', 20); i.Add('\u00b5', 17); i.Add('\u00df', 19); i.Add('\u2022', 15); i.Add('\u20ac', 19); } } }
        }
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}