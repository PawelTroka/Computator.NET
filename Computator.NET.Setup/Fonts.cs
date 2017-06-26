using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
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

        public static void InjectFonts(System.Xml.Linq.XDocument document)
        {
            var fontsDirectoryDocument = new XmlDocument();
            fontsDirectoryDocument.LoadXml(@"
            <Directory Id=""FontsFolder"">
                <Component Id=""Component.FontsFolder"" Guid="""">
                    <File Id=""cambria.ttc.1"" Source=""..\Computator.NET.Core\Static\fonts\cambria.ttc"" TrueType=""yes"" />
                    <File Id=""consola.ttf.1"" Source=""..\Computator.NET.Core\Static\fonts\consola.ttf"" TrueType=""yes"" />
                </Component>
            </Directory>");

            document.Root.Select("Product/Directory").AddElement(XmlElementToXelement(fontsDirectoryDocument.DocumentElement));


            var fontsFolderComponent = new XmlDocument();
            fontsFolderComponent.LoadXml(@"<ComponentRef Id=""Component.FontsFolder"" />");
            document.Root.Select("Product/Feature").AddElement(XmlElementToXelement(fontsFolderComponent.DocumentElement));
        }

        private static XElement XmlElementToXelement(XmlElement e)
        {
            return XElement.Parse(e.OuterXml);
        }
    }
}
