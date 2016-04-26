using System;
using System.Drawing;
using System.Drawing.Text;
using System.Reflection;
using Computator.NET.Logging;

namespace Computator.NET.DataTypes
{
    public static class CustomFonts
    {
        private static readonly SimpleLogger logger = new SimpleLogger
        {
            ClassName = typeof(GlobalConfig).FullName
        };

        private static PrivateFontCollection mathFontCollection;

        private static PrivateFontCollection scriptingFontCollection;


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

            var pathToFont = GlobalConfig.FullPath("UI", "fonts", "CAMBRIA.TTC");
            var pathToFont2 = GlobalConfig.FullPath("UI", "fonts", "consola.ttf");
            try
            {
                mathFontCollection.AddFontFile(pathToFont);
                scriptingFontCollection.AddFontFile(pathToFont2);
            }
            catch (Exception ex)
            {
                var nex =
                    new Exception(
                        "Probably missing " + pathToFont + " or " + pathToFont2 + " file\nDetails:" + ex.Message, ex);
                logger.MethodName = MethodBase.GetCurrentMethod().Name;
                logger.Log("Probably missing " + pathToFont + " file\nDetails:" + ex.Message, ErrorType.General, nex);
                throw nex;
            }
        }
    }
}