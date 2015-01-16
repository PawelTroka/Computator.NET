using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using Computator.NET.Functions;
using Computator.NET.Localization;

namespace Computator.NET.Config
{
    internal static class GlobalConfig
    {
        public const string version = "v1.8.2α";

        public const string authorWithEmail = author + " (ptroka@fizyka.dk)";
        public const string author = "Paweł Troka";

        //

        public const char dotSymbol = '·'; //'⋅'

        public const string Superscripts = " ⁽⁾⁺⁻˙˸⁼ॱ⁰¹²³⁴⁵⁶⁷⁸⁹ᴬᴮᴰᴱᴳᴴᴵᴶᴷᴸᴹᴺᴼᴾᴿᵀᵁᵂᵃᵇᶜᵈᵉᶠᵍʰⁱʲᵏˡᵐⁿᵒᵖʳˢᵗᵘᵛʷˣʸᶻᵅᵝᵞᵟᵋᶿᶥᶲᵠᵡ";
            //ⱽ

        //ॱ 
        //˸
        public const string AsciiForSuperscripts =
            " ()+-*/=.0123456789ABDEGHIJKLMNOPRTUWabcdefghijklmnoprstuvwxyzαβγδεθιφψχ"; //ⱽ

        private const char blankCharacter = 'ⱽ';

        public static readonly string betatesters = Strings.betaTesters +
                                                    ":\n - Kordian Czyżewski (kordiancz25@wp.pl)\n - Vojtech Mańkowski (vojtaman@gmail.com)\n - Marcin Piwowarski (marcpiwowarski@gmail.com)";

        public static readonly string translators = Strings.translators +
                                                    ":\n - Paweł Troka (ptroka@fizyka.dk) - English&Polish versions\n - Vojtech Mańkowski (vojtaman@gmail.com) - Czech version\n - Athena Hristanas (athena@fizyka.dk) - Deutsch version";

        public static readonly string libraries = Strings.librariesUsed +
                                                  ":\n - Meta.Numerics v2.2.0 | © David Wright | Microsoft Public License (Ms-PL)\n - GNU Scientific Library v1.16 | GNU General Public License (GNU GPL)\n - Math.NET Numerics v3.2.3 | © Math.NET Team | The MIT License (MIT)\n - Autocomplete Menu rev.28 | © Pavel Torgashov | LGPLv3\n - ScintillaNET v2.6 | © Garrett Serack | https://scintillanet.codeplex.com/license\n - Accord.Math v2.13.1 | © César Roberto de Souza | GNU LGPL v2.1";

        public static readonly string others = Strings.otherContributors +
                                               ":\n - Jianzhong Zhang (Chart3D classes are based on code from High performance WPF 3D Chart rev.6 application on Code Project Open License (CPOL) 1.02)\n - Claudio Rocchini (standard algorithm for complex domain coloring)";

        public static readonly string features = Strings.featuresInclude;

        public static readonly string basePath = Path.GetDirectoryName(Application.ExecutablePath) +
                                                 Path.DirectorySeparatorChar;


        public static readonly string gslLibPath = (Environment.Is64BitOperatingSystem)
            ? fullPath("Special", "x64", "libgsl-0.dll")
            : fullPath("Special", "x86", "libgsl-0.dll");

        public static readonly string gslBlasPath = (Environment.Is64BitOperatingSystem)
            ? fullPath("Special", "x64", "libgslcblas-0.dll")
            : fullPath("Special", "x86", "libgslcblas-0.dll");

        public static readonly Dictionary<string, FunctionInfo> functionsDetails = loadFunctionsDetailsFromXmlFile();
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

        public static string SuperscriptsToAscii(string s)
        {
            var sb = new StringBuilder(s);

            for (int i = 0; i < s.Length; i++)
                if (Superscripts.Contains(sb[i]))
                    sb[i] = SuperscriptToAscii(sb[i]);
            return sb.ToString();
        }

        public static string AsciiToSuperscript(string s)
        {
            var sb = new StringBuilder(s);

            for (int i = 0; i < s.Length; i++)
                if (AsciiForSuperscripts.Contains(sb[i]))
                    sb[i] = AsciiToSuperscript(sb[i]);
            return sb.ToString();
        }

        public static char AsciiToSuperscript(char c)
        {
            if (AsciiForSuperscripts.Contains(c))
                return Superscripts[AsciiForSuperscripts.IndexOf(c)];
            return blankCharacter;
        }

        public static char SuperscriptToAscii(char c)
        {
            if (Superscripts.Contains(c))
                return AsciiForSuperscripts[Superscripts.IndexOf(c)];
            return blankCharacter;
        }

        public static string fullPath(params string[] foldersAndFile)
        {
            return Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar +
                   Path.Combine(foldersAndFile);
        }

        public static Font GetMathFont(float fontSize)
        {
            if (pfc == null) getCustomFont();
            return new Font(pfc.Families[0], fontSize, GraphicsUnit.Point);
        }

        public static void changeFontToMathFont(params Control[] controls)
        {
            foreach (Control c in controls)
            {
                Font oldFont = c.Font;
                c.Font = new Font(mathFontFamily, c.Font.Size, c.Font.Unit);
            }
        }

        private static void getCustomFont()
        {
            pfc = new PrivateFontCollection();
            string pathToFont = fullPath("UI", "fonts", "CAMBRIA.TTC");
            try
            {
                pfc.AddFontFile(pathToFont);
            }
            catch (Exception ex)
            {
                throw new Exception("Probably missing " + pathToFont + " file\nDetails:" + ex.Message);
            }
        }

        private static void saveFunctionDetailsToXmlFile()
        {
            //var functionsDetails = new Dictionary<string, FunctionInfo>();
            //foreach (var item in sourceItems)
            //functionsDetails.Add(item.functionInfo.Signature, item.functionInfo);

            var serializer = new XmlSerializer(typeof (FunctionInfo[]),
                new XmlRootAttribute("FunctionsDetails"));
            var stream = new StreamWriter("functions_saved.xml");

            serializer.Serialize(stream,
                functionsDetails.Select(
                    kv =>
                        new FunctionInfo
                        {
                            Signature = kv.Key,
                            Url = kv.Value.Url,
                            Description = kv.Value.Description,
                            Title = kv.Value.Title,
                            Category = kv.Value.Category,
                            Type = kv.Value.Type
                        }).ToArray());
        }

        private static Dictionary<string, FunctionInfo> loadFunctionsDetailsFromXmlFile()
        {
            // Constants.PhysicalConstants.parseConstantsFromNIST();
            //Constants.MathematicalConstants.parseConstantsFromNIST();

            var serializer = new XmlSerializer(typeof (FunctionInfo[]),
                new XmlRootAttribute("FunctionsDetails"));
            var stream = new StreamReader(fullPath("Data", "functions.xml"));

            Dictionary<string, FunctionInfo> functionsInfos = ((FunctionInfo[]) serializer.Deserialize(stream))
                .ToDictionary(kv => kv.Signature,
                    kv =>
                        new FunctionInfo
                        {
                            Signature = kv.Signature,
                            Url = kv.Url,
                            Description = kv.Description,
                            Title = kv.Title,
                            Category = kv.Category,
                            Type = kv.Type
                        });

            return functionsInfos;
        }
    }


    public class MyMessageFilter : IMessageFilter
    {
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_MOUSEHWHEEL = 0x020E;

        public bool PreFilterMessage(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_MOUSEWHEEL: // 0x020A
                case WM_MOUSEHWHEEL: // 0x020E
                    IntPtr hControlUnderMouse = WindowFromPoint(new Point((int) m.LParam));
                    if (hControlUnderMouse == m.HWnd)
                        return false; // already headed for the right control
                    // redirect the message to the control under the mouse
                    SendMessage(hControlUnderMouse, m.Msg, m.WParam, m.LParam);
                    return true;
                default:
                    return false;
            }
        }

        [DllImport("user32.dll", SetLastError = false)]
        private static extern IntPtr SendMessage(
            IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "WindowFromPoint", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr WindowFromPoint(Point pt);
    }
}