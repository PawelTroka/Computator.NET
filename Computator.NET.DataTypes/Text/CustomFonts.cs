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
            return new Font("Cambria", fontSize, GraphicsUnit.Point);
        }

        public static Font GetScriptingFont(float fontSize)
        {
            return new Font("Consolas", fontSize, GraphicsUnit.Point);
        }
    }
}