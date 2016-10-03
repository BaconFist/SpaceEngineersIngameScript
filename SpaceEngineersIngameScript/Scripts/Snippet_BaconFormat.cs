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
            static private Dictionary<char, Point> CharMap = null;
            
            public BaconFormat()
            {
                bootstrap();
            }

            static private void bootstrap()
            {
                if (BaconFormat.CharMap == null)
                {
                    BaconFormat.CharMap = new Dictionary<char, Point>();
                    BaconFormat.CharMap.Add('\u0020', new Point(15, 45));
                    BaconFormat.CharMap.Add('\u0021', new Point(24, 45));
                    BaconFormat.CharMap.Add('\u0022', new Point(25, 45));
                    BaconFormat.CharMap.Add('\u0023', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0024', new Point(36, 45));
                    BaconFormat.CharMap.Add('\u0025', new Point(39, 45));
                    BaconFormat.CharMap.Add('\u0026', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0027', new Point(22, 45));
                    BaconFormat.CharMap.Add('\u0028', new Point(24, 45));
                    BaconFormat.CharMap.Add('\u0029', new Point(24, 45));
                    BaconFormat.CharMap.Add('\u002a', new Point(26, 45));
                    BaconFormat.CharMap.Add('\u002b', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u002c', new Point(25, 45));
                    BaconFormat.CharMap.Add('\u002d', new Point(25, 45));
                    BaconFormat.CharMap.Add('\u002e', new Point(25, 45));
                    BaconFormat.CharMap.Add('\u002f', new Point(30, 45));
                    BaconFormat.CharMap.Add('\u0030', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0031', new Point(24, 45));
                    BaconFormat.CharMap.Add('\u0032', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u0033', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0034', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u0035', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0036', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0037', new Point(31, 45));
                    BaconFormat.CharMap.Add('\u0038', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0039', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u003a', new Point(25, 45));
                    BaconFormat.CharMap.Add('\u003b', new Point(25, 45));
                    BaconFormat.CharMap.Add('\u003c', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u003d', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u003e', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u003f', new Point(31, 45));
                    BaconFormat.CharMap.Add('\u0040', new Point(40, 45));
                    BaconFormat.CharMap.Add('\u0041', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u0042', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u0043', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0044', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u0045', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u0046', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u0047', new Point(36, 45));
                    BaconFormat.CharMap.Add('\u0048', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0049', new Point(24, 45));
                    BaconFormat.CharMap.Add('\u004a', new Point(31, 45));
                    BaconFormat.CharMap.Add('\u004b', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u004c', new Point(30, 45));
                    BaconFormat.CharMap.Add('\u004d', new Point(42, 45));
                    BaconFormat.CharMap.Add('\u004e', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u004f', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u0050', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0051', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u0052', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u0053', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u0054', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u0055', new Point(36, 45));
                    BaconFormat.CharMap.Add('\u0056', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0057', new Point(47, 45));
                    BaconFormat.CharMap.Add('\u0058', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0059', new Point(36, 45));
                    BaconFormat.CharMap.Add('\u005a', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u005b', new Point(25, 45));
                    BaconFormat.CharMap.Add('\u005c', new Point(28, 45));
                    BaconFormat.CharMap.Add('\u005d', new Point(25, 45));
                    BaconFormat.CharMap.Add('\u005e', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u005f', new Point(31, 45));
                    BaconFormat.CharMap.Add('\u0060', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u0061', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0062', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0063', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u0064', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0065', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0066', new Point(24, 45));
                    BaconFormat.CharMap.Add('\u0067', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0068', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0069', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u006a', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u006b', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u006c', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u006d', new Point(42, 45));
                    BaconFormat.CharMap.Add('\u006e', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u006f', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0070', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0071', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0072', new Point(25, 45));
                    BaconFormat.CharMap.Add('\u0073', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0074', new Point(25, 45));
                    BaconFormat.CharMap.Add('\u0075', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0076', new Point(30, 45));
                    BaconFormat.CharMap.Add('\u0077', new Point(42, 45));
                    BaconFormat.CharMap.Add('\u0078', new Point(31, 45));
                    BaconFormat.CharMap.Add('\u0079', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u007a', new Point(31, 45));
                    BaconFormat.CharMap.Add('\u007b', new Point(25, 45));
                    BaconFormat.CharMap.Add('\u007c', new Point(22, 45));
                    BaconFormat.CharMap.Add('\u007d', new Point(25, 45));
                    BaconFormat.CharMap.Add('\u007e', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u00a0', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u00a1', new Point(24, 45));
                    BaconFormat.CharMap.Add('\u00a2', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u00a3', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00a4', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u00a5', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u00a6', new Point(22, 45));
                    BaconFormat.CharMap.Add('\u00a7', new Point(36, 45));
                    BaconFormat.CharMap.Add('\u00a8', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u00a9', new Point(40, 45));
                    BaconFormat.CharMap.Add('\u00aa', new Point(26, 45));
                    BaconFormat.CharMap.Add('\u00ab', new Point(30, 45));
                    BaconFormat.CharMap.Add('\u00ac', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u00ad', new Point(14, 8));
                    BaconFormat.CharMap.Add('\u00ae', new Point(40, 45));
                    BaconFormat.CharMap.Add('\u00af', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u00b0', new Point(27, 45));
                    BaconFormat.CharMap.Add('\u00b1', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u00b2', new Point(27, 45));
                    BaconFormat.CharMap.Add('\u00b3', new Point(27, 45));
                    BaconFormat.CharMap.Add('\u00b4', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u00b5', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00b6', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u00b7', new Point(25, 45));
                    BaconFormat.CharMap.Add('\u00b8', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u00b9', new Point(27, 45));
                    BaconFormat.CharMap.Add('\u00ba', new Point(26, 45));
                    BaconFormat.CharMap.Add('\u00bb', new Point(30, 45));
                    BaconFormat.CharMap.Add('\u00bc', new Point(43, 45));
                    BaconFormat.CharMap.Add('\u00bd', new Point(45, 45));
                    BaconFormat.CharMap.Add('\u00be', new Point(43, 45));
                    BaconFormat.CharMap.Add('\u00bf', new Point(31, 45));
                    BaconFormat.CharMap.Add('\u00c0', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u00c1', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u00c2', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u00c3', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u00c4', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u00c5', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u00c6', new Point(47, 45));
                    BaconFormat.CharMap.Add('\u00c7', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u00c8', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u00c9', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u00ca', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u00cb', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u00cc', new Point(24, 45));
                    BaconFormat.CharMap.Add('\u00cd', new Point(24, 45));
                    BaconFormat.CharMap.Add('\u00ce', new Point(24, 45));
                    BaconFormat.CharMap.Add('\u00cf', new Point(24, 45));
                    BaconFormat.CharMap.Add('\u00d0', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u00d1', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u00d2', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u00d3', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u00d4', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u00d5', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u00d6', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u00d7', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u00d8', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u00d9', new Point(36, 45));
                    BaconFormat.CharMap.Add('\u00da', new Point(36, 45));
                    BaconFormat.CharMap.Add('\u00db', new Point(36, 45));
                    BaconFormat.CharMap.Add('\u00dc', new Point(36, 45));
                    BaconFormat.CharMap.Add('\u00dd', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00de', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u00df', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u00e0', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00e1', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00e2', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00e3', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00e4', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00e5', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00e6', new Point(44, 45));
                    BaconFormat.CharMap.Add('\u00e7', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u00e8', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00e9', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00ea', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00eb', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00ec', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u00ed', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u00ee', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u00ef', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u00f0', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00f1', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00f2', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00f3', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00f4', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00f5', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00f6', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00f7', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u00f8', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00f9', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00fa', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00fb', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00fc', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00fd', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00fe', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u00ff', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0100', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0101', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0102', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u0103', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0104', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u0105', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0106', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0107', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u0108', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0109', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u010a', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u010b', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u010c', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u010d', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u010e', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u010f', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0110', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u0111', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0112', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u0113', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0114', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u0115', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0116', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u0117', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0118', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u0119', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u011a', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u011b', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u011c', new Point(36, 45));
                    BaconFormat.CharMap.Add('\u011d', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u011e', new Point(36, 45));
                    BaconFormat.CharMap.Add('\u011f', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0120', new Point(36, 45));
                    BaconFormat.CharMap.Add('\u0121', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0122', new Point(36, 45));
                    BaconFormat.CharMap.Add('\u0123', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0124', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0125', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0126', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0127', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0128', new Point(24, 45));
                    BaconFormat.CharMap.Add('\u0129', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u012a', new Point(24, 45));
                    BaconFormat.CharMap.Add('\u012b', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u012e', new Point(24, 45));
                    BaconFormat.CharMap.Add('\u012f', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u0130', new Point(24, 45));
                    BaconFormat.CharMap.Add('\u0131', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u0132', new Point(40, 45));
                    BaconFormat.CharMap.Add('\u0133', new Point(29, 45));
                    BaconFormat.CharMap.Add('\u0134', new Point(31, 45));
                    BaconFormat.CharMap.Add('\u0135', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u0136', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0137', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u0139', new Point(30, 45));
                    BaconFormat.CharMap.Add('\u013a', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u013b', new Point(30, 45));
                    BaconFormat.CharMap.Add('\u013c', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u013d', new Point(30, 45));
                    BaconFormat.CharMap.Add('\u013e', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u013f', new Point(30, 45));
                    BaconFormat.CharMap.Add('\u0140', new Point(26, 45));
                    BaconFormat.CharMap.Add('\u0141', new Point(30, 45));
                    BaconFormat.CharMap.Add('\u0142', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u0143', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u0144', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0145', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u0146', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0147', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u0148', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0149', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u014c', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u014d', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u014e', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u014f', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0150', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u0151', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0152', new Point(47, 45));
                    BaconFormat.CharMap.Add('\u0153', new Point(44, 45));
                    BaconFormat.CharMap.Add('\u0154', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u0155', new Point(25, 45));
                    BaconFormat.CharMap.Add('\u0156', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u0157', new Point(25, 45));
                    BaconFormat.CharMap.Add('\u0158', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u0159', new Point(25, 45));
                    BaconFormat.CharMap.Add('\u015a', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u015b', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u015c', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u015d', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u015e', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u015f', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0160', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u0161', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0162', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u0163', new Point(25, 45));
                    BaconFormat.CharMap.Add('\u0164', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u0165', new Point(25, 45));
                    BaconFormat.CharMap.Add('\u0166', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u0167', new Point(25, 45));
                    BaconFormat.CharMap.Add('\u0168', new Point(36, 45));
                    BaconFormat.CharMap.Add('\u0169', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u016a', new Point(36, 45));
                    BaconFormat.CharMap.Add('\u016b', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u016c', new Point(36, 45));
                    BaconFormat.CharMap.Add('\u016d', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u016e', new Point(36, 45));
                    BaconFormat.CharMap.Add('\u016f', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0170', new Point(36, 45));
                    BaconFormat.CharMap.Add('\u0171', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0172', new Point(36, 45));
                    BaconFormat.CharMap.Add('\u0173', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0174', new Point(47, 45));
                    BaconFormat.CharMap.Add('\u0175', new Point(42, 45));
                    BaconFormat.CharMap.Add('\u0176', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0177', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0178', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0179', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u017a', new Point(31, 45));
                    BaconFormat.CharMap.Add('\u017b', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u017c', new Point(31, 45));
                    BaconFormat.CharMap.Add('\u017d', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u017e', new Point(31, 45));
                    BaconFormat.CharMap.Add('\u0192', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0218', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u0219', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u021a', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u021b', new Point(25, 45));
                    BaconFormat.CharMap.Add('\u02c6', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u02c7', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u02c9', new Point(22, 45));
                    BaconFormat.CharMap.Add('\u02d8', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u02d9', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u02da', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u02db', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u02dc', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u02dd', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u0401', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u0403', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u0404', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u0405', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u0406', new Point(24, 45));
                    BaconFormat.CharMap.Add('\u0407', new Point(24, 45));
                    BaconFormat.CharMap.Add('\u0408', new Point(31, 45));
                    BaconFormat.CharMap.Add('\u0409', new Point(43, 45));
                    BaconFormat.CharMap.Add('\u040a', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u040c', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u040e', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u040f', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0410', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0411', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0412', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0413', new Point(30, 45));
                    BaconFormat.CharMap.Add('\u0414', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0415', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u0416', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u0417', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0418', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0419', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u041a', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u041b', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u041c', new Point(42, 45));
                    BaconFormat.CharMap.Add('\u041d', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u041e', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u041f', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u0420', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u0421', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0422', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0423', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0424', new Point(36, 45));
                    BaconFormat.CharMap.Add('\u0425', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0426', new Point(36, 45));
                    BaconFormat.CharMap.Add('\u0427', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u0428', new Point(42, 45));
                    BaconFormat.CharMap.Add('\u0429', new Point(45, 45));
                    BaconFormat.CharMap.Add('\u042a', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u042b', new Point(40, 45));
                    BaconFormat.CharMap.Add('\u042c', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u042d', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u042e', new Point(42, 45));
                    BaconFormat.CharMap.Add('\u042f', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u0430', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u0431', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0432', new Point(31, 45));
                    BaconFormat.CharMap.Add('\u0433', new Point(30, 45));
                    BaconFormat.CharMap.Add('\u0434', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0435', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0436', new Point(36, 45));
                    BaconFormat.CharMap.Add('\u0437', new Point(30, 45));
                    BaconFormat.CharMap.Add('\u0438', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u0439', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u043a', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u043b', new Point(30, 45));
                    BaconFormat.CharMap.Add('\u043c', new Point(41, 45));
                    BaconFormat.CharMap.Add('\u043d', new Point(31, 45));
                    BaconFormat.CharMap.Add('\u043e', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u043f', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u0440', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u0441', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u0442', new Point(30, 45));
                    BaconFormat.CharMap.Add('\u0443', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0444', new Point(37, 45));
                    BaconFormat.CharMap.Add('\u0445', new Point(31, 45));
                    BaconFormat.CharMap.Add('\u0446', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0447', new Point(31, 45));
                    BaconFormat.CharMap.Add('\u0448', new Point(41, 45));
                    BaconFormat.CharMap.Add('\u0449', new Point(42, 45));
                    BaconFormat.CharMap.Add('\u044a', new Point(31, 45));
                    BaconFormat.CharMap.Add('\u044b', new Point(36, 45));
                    BaconFormat.CharMap.Add('\u044c', new Point(31, 45));
                    BaconFormat.CharMap.Add('\u044d', new Point(30, 45));
                    BaconFormat.CharMap.Add('\u044e', new Point(39, 45));
                    BaconFormat.CharMap.Add('\u044f', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u0451', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0452', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0453', new Point(31, 45));
                    BaconFormat.CharMap.Add('\u0454', new Point(30, 45));
                    BaconFormat.CharMap.Add('\u0455', new Point(32, 45));
                    BaconFormat.CharMap.Add('\u0456', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u0457', new Point(23, 45));
                    BaconFormat.CharMap.Add('\u0458', new Point(22, 45));
                    BaconFormat.CharMap.Add('\u0459', new Point(38, 45));
                    BaconFormat.CharMap.Add('\u045a', new Point(41, 45));
                    BaconFormat.CharMap.Add('\u045b', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u045c', new Point(31, 45));
                    BaconFormat.CharMap.Add('\u045e', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u045f', new Point(33, 45));
                    BaconFormat.CharMap.Add('\u0490', new Point(30, 45));
                    BaconFormat.CharMap.Add('\u0491', new Point(29, 45));
                    BaconFormat.CharMap.Add('\u2013', new Point(30, 45));
                    BaconFormat.CharMap.Add('\u2014', new Point(46, 45));
                    BaconFormat.CharMap.Add('\u2018', new Point(22, 45));
                    BaconFormat.CharMap.Add('\u2019', new Point(22, 45));
                    BaconFormat.CharMap.Add('\u201a', new Point(22, 45));
                    BaconFormat.CharMap.Add('\u201c', new Point(27, 45));
                    BaconFormat.CharMap.Add('\u201d', new Point(27, 45));
                    BaconFormat.CharMap.Add('\u201e', new Point(27, 45));
                    BaconFormat.CharMap.Add('\u2020', new Point(36, 45));
                    BaconFormat.CharMap.Add('\u2021', new Point(36, 45));
                    BaconFormat.CharMap.Add('\u2022', new Point(31, 45));
                    BaconFormat.CharMap.Add('\u2026', new Point(47, 45));
                    BaconFormat.CharMap.Add('\u2030', new Point(47, 45));
                    BaconFormat.CharMap.Add('\u2039', new Point(24, 45));
                    BaconFormat.CharMap.Add('\u203a', new Point(24, 45));
                    BaconFormat.CharMap.Add('\u20ac', new Point(35, 45));
                    BaconFormat.CharMap.Add('\u2122', new Point(46, 45));
                    BaconFormat.CharMap.Add('\u2212', new Point(34, 45));
                    BaconFormat.CharMap.Add('\u2219', new Point(24, 45));
                    BaconFormat.CharMap.Add('\u25a1', new Point(37, 45));
                    BaconFormat.CharMap.Add('\ue001', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue002', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue003', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue004', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue005', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue006', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue007', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue008', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue009', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue00a', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue00b', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue00c', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue00d', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue00e', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue00f', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue010', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue011', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue012', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue013', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue014', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue015', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue016', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue017', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue018', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue019', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue020', new Point(53, 52));
                    BaconFormat.CharMap.Add('\ue021', new Point(53, 52));
                }
            }
        }

        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}