namespace Computator.NET.Config
{
    internal static class MathCustomFonts
    {
        private static readonly Logging.SimpleLogger logger = new Logging.SimpleLogger
        {
            ClassName = typeof (GlobalConfig).FullName
        };

        private static System.Drawing.Text.PrivateFontCollection pfc;

        public static System.Drawing.Font mathFont
        {
            get
            {
                if (pfc == null) getCustomFont();
                return new System.Drawing.Font(pfc.Families[0], 18.2F, System.Drawing.GraphicsUnit.Point);
            }
        }

        public static System.Drawing.FontFamily mathFontFamily
        {
            get
            {
                if (pfc == null) getCustomFont();
                return pfc.Families[0];
            }
        }

        public static System.Drawing.Font GetMathFont(float fontSize)
        {
            if (pfc == null) getCustomFont();
            return new System.Drawing.Font(pfc.Families[0], fontSize, System.Drawing.GraphicsUnit.Point);
        }

        public static void changeFontToMathFont(params System.Windows.Forms.Control[] controls)
        {
            foreach (var c in controls)
            {
                var oldFont = c.Font;
                c.Font = new System.Drawing.Font(mathFontFamily, c.Font.Size, c.Font.Unit);
            }
        }

        private static void getCustomFont()
        {
            pfc = new System.Drawing.Text.PrivateFontCollection();
            var pathToFont = GlobalConfig.FullPath("UI", "fonts", "CAMBRIA.TTC");
            try
            {
                pfc.AddFontFile(pathToFont);
            }
            catch (System.Exception ex)
            {
                var nex = new System.Exception("Probably missing " + pathToFont + " file\nDetails:" + ex.Message, ex);
                logger.MethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                logger.Log("Probably missing " + pathToFont + " file\nDetails:" + ex.Message, ErrorType.General, nex);
                throw nex;
            }
        }
    }
}