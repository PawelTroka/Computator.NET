using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WixSharp;

namespace Computator.NET.Setup
{
    class Fonts
    {
        public static WixEntity[] GetFontFiles()
        {
            var consolaFont = new FontFile(@"..\Computator.NET.Core\Static\fonts\consola.ttf")
            {
                NeverOverwrite = false,
                TrueType = true,
            };
            var cambriaFont = new FontFile(@"..\Computator.NET.Core\Static\fonts\cambria.ttc")
            {
                NeverOverwrite = false,
                TrueType = true,
            };

            return new WixEntity[] {consolaFont, cambriaFont};
        }
    }
}
