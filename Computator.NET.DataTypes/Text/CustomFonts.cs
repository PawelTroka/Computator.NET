using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using Computator.NET.DataTypes.Configuration;
using Computator.NET.DataTypes.Utility;
using NLog;

namespace Computator.NET.DataTypes.Text
{
    public static class CustomFonts
    {
        [DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private static PrivateFontCollection mathFontCollection;

        private static PrivateFontCollection scriptingFontCollection;

        public static Font GetFontFromFont(Font value)
        {
            if (value.FontFamily.Name.ToLowerInvariant().Contains("cambria"))
            {
                return GetMathFont(value.Size);
            }
            if (value.FontFamily.Name.ToLowerInvariant().Contains("consola"))
            {
                return GetScriptingFont(value.Size);
            }
            return value;
        }

        public static Font GetMathFont(float fontSize)
        {
            if (mathFontCollection == null) GetCustomFonts();
            return new Font(mathFontCollection.Families[0], fontSize, GraphicsUnit.Point);
        }

        public static Font GetScriptingFont(float fontSize)
        {
            if (scriptingFontCollection == null) GetCustomFonts();
            return new Font(scriptingFontCollection.Families[0], fontSize, GraphicsUnit.Point);
        }


        private static void GetCustomFonts()
        {
            mathFontCollection = new PrivateFontCollection();
            scriptingFontCollection = new PrivateFontCollection();

            var pathToMathFont = PathUtility.GetFullPath("Static", "fonts", "cambria.ttc");
            var pathToScriptFont = PathUtility.GetFullPath("Static", "fonts", "consola.ttf");
            try
            {
                if (RuntimeInformation.IsUnix)
                {
                    mathFontCollection.AddFontFile(pathToMathFont);
                    scriptingFontCollection.AddFontFile(pathToScriptFont);
                }
                else
                {
                    RegisterFont(mathFontCollection,pathToMathFont);
                    RegisterFont(scriptingFontCollection, pathToScriptFont);
                }
            }
            catch (Exception ex)
            {
                var nex =
                    new Exception(
                        "Probably missing " + pathToMathFont + " or " + pathToScriptFont + " file\nDetails:" + ex.Message, ex);

                Logger.Error(ex, "Probably missing " + pathToMathFont + " file\nDetails:" + ex.Message);
                throw nex;
            }
        }

        private static void RegisterFont(PrivateFontCollection pfc, string path)
        {
            var fontData = File.ReadAllBytes(path);

            //create an unsafe memory block for the data
            System.IntPtr data = Marshal.AllocCoTaskMem((int)fontData.Length);


            //copy the bytes to the unsafe memory block
            Marshal.Copy(fontData, 0, data, (int)fontData.Length);

            // We HAVE to do this to register the font to the system (Weird .NET bug !)
            uint pcFonts = 0;
            AddFontMemResourceEx(data, (uint)fontData.Length, IntPtr.Zero, ref pcFonts);

            //pass the font to the font collection
            pfc.AddMemoryFont(data, (int)fontData.Length);

            //free the unsafe memory
            Marshal.FreeCoTaskMem(data);
        }
    }
}