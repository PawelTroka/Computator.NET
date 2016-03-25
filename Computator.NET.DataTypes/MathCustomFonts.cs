using System;
using System.Drawing;
using System.Drawing.Text;
using System.Reflection;
using System.Windows.Forms;
using Computator.NET.Logging;

namespace Computator.NET.Config
{
    public static class MathCustomFonts
    {
        private static readonly SimpleLogger logger = new SimpleLogger
        {
            ClassName = typeof (GlobalConfig).FullName
        };

        private static PrivateFontCollection pfc;

        public static Font mathFont
        {
            get
            {
                if (pfc == null) getCustomFont();
                return new Font(pfc.Families[0], 18.2F, GraphicsUnit.Point);
            }
        }

        public static FontFamily mathFontFamily
        {
            get
            {
                if (pfc == null) getCustomFont();
                return pfc.Families[0];
            }
        }

        public static Font GetMathFont(float fontSize)
        {
            if (pfc == null) getCustomFont();
            return new Font(pfc.Families[0], fontSize, GraphicsUnit.Point);
        }

        public static void changeFontToMathFont(params Control[] controls)
        {
            foreach (var c in controls)
            {
                var oldFont = c.Font;
                c.Font = new Font(mathFontFamily, c.Font.Size, c.Font.Unit);
            }
        }

        private static void getCustomFont()
        {
            pfc = new PrivateFontCollection();
            var pathToFont = GlobalConfig.FullPath("UI", "fonts", "CAMBRIA.TTC");
            try
            {
                pfc.AddFontFile(pathToFont);
            }
            catch (Exception ex)
            {
                var nex = new Exception("Probably missing " + pathToFont + " file\nDetails:" + ex.Message, ex);
                logger.MethodName = MethodBase.GetCurrentMethod().Name;
                logger.Log("Probably missing " + pathToFont + " file\nDetails:" + ex.Message, ErrorType.General, nex);
                throw nex;
            }
        }
    }
}