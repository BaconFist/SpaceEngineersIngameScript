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

namespace Snippet_BaconSerialize
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        public void Main()
        {
            BaconSerialize bs = new BaconSerialize();
            bs.Add("hallo welt!");
            bs.Add(01234);
            bs.Add("s11:hallo welt!i4:1234d6:435.98cR0");
            bs.Add(435.980000);
            bs.Add(new KeyValuePair<int, string>(42, "derp"));
            bs.Add('R');
            bs.Add(false);
            Console.WriteLine(bs.ToString());
            List<object> data = bs.deSerialize(bs.ToString());
            for (int i = 0; i < data.Count; i++)
            {
                Console.WriteLine(data[i].GetType().ToString() + " => " + data[i].ToString());
            }
        }

        class BaconSerialize
        {
            List<string> data = new List<string>();
            
            public interface ISerializeable
            {
                string getSerialized();
            }

            public List<object> deSerialize(string serialiezed)
            {
                List<object> deSerialized = new List<object>();
                string slug = serialiezed;
                int count = 0;
                int loopMax = slug.Length;
                for (int i = 0; i < loopMax && slug.Length > 0; i++)
                {
                    char glyph = slug[0];
                    slug = slug.Remove(0, 1);
                    switch (glyph)
                    {
                        case 's':
                            if (int.TryParse(slug.Substring(0, slug.IndexOf(':')), out count))
                            {
                                slug = slug.Remove(0, slug.IndexOf(':') + 1);
                                deSerialized.Add(slug.Substring(0, count));
                                slug = slug.Remove(0, count);
                            }
                            else
                            {
                                throw new ArgumentException("unable to parse number " + slug.Substring(0, slug.IndexOf(':')));
                            }
                            break;
                        case 'i':
                            if (int.TryParse(slug.Substring(0, slug.IndexOf(':')), out count))
                            {
                                slug = slug.Remove(0, slug.IndexOf(':') + 1);
                                int intTmp = 0;
                                if(int.TryParse(slug.Substring(0, count), out intTmp))
                                {
                                    deSerialized.Add(intTmp);
                                    slug = slug.Remove(0, count);
                                }
                                else
                                {
                                    throw new ArgumentException("unable to parse int '" + slug.Substring(0, count) + "' // " + slug);
                                }
                            }
                            else
                            {
                                throw new ArgumentException("unable to parse number '" + slug.Substring(0, slug.IndexOf(':')) + "' // " + slug);
                            }
                            break;
                        case 'd':
                            if (int.TryParse(slug.Substring(0, slug.IndexOf(':')), out count))
                            {
                                slug = slug.Remove(0, slug.IndexOf(':') + 1);
                                double doubleTmp = 0;
                                if (double.TryParse(slug.Substring(0, count), out doubleTmp))
                                {
                                    deSerialized.Add(doubleTmp);
                                    slug = slug.Remove(0, count);
                                }
                                else
                                {
                                    throw new ArgumentException("unable to parse double '" + slug.Substring(0, count) + "' // " + slug);
                                }
                            }
                            else
                            {
                                throw new ArgumentException("unable to parse number " + slug.Substring(0, slug.IndexOf(':')));
                            }
                            break;
                        case 'c':
                            deSerialized.Add(slug[0]);
                            slug = slug.Remove(0, 1);
                            break;
                        case '0':
                            deSerialized.Add(false);
                            break;
                        case '1':
                            deSerialized.Add(true);
                            break;
                        case 'o':
                        case 'S':
                            if (int.TryParse(slug.Substring(0, slug.IndexOf(':')), out count))
                            {
                                slug = slug.Remove(0, slug.IndexOf(':') + 1);
                                deSerialized.Add(slug.Substring(0, count));
                                slug = slug.Remove(0, count);
                            }
                            else
                            {
                                throw new ArgumentException("unable to parse number " + slug.Substring(0, slug.IndexOf(':')));
                            }
                            break;
                        default:
                            break;
                    }
                }
                return deSerialized;
            }

            public void Add(ISerializeable value)
            {
                string serializedObject = "[" + value.GetType().ToString() + "]" + value.getSerialized();
                data.Add("S" + serializedObject.Length.ToString() + ":" + serializedObject);
            }

            public void Add(object value)
            {
                string serializedObject = "[" + value.GetType().ToString() + "]" + value.ToString();
                data.Add("o" + serializedObject.Length.ToString() + ":" + serializedObject);
            }

            public void Add(string value)
            {
                data.Add("s" + value.Length.ToString() + ":" + value);
            }

            public void Add(int value)
            {
                string slug = value.ToString();
                data.Add("i" + slug.Length + ":" + slug);
            }

            public void Add(double value)
            {
                string slug = value.ToString();
                data.Add("d" + slug.Length + ":" + slug);
            }

            public void Add(bool value)
            {
                data.Add(((value) ? "1" : "0"));
            }

            public void Add(char value)
            {
                data.Add("c" + value);
            }

            override public string ToString()
            {
                return string.Join("", data);
            }
        } 
        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}