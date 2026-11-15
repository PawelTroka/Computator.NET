using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Computator.NET.DataTypes.Text
{
    public static class CustomFonts
    {
#if __MonoCS__
        // Keep the registered families (and the macOS private collection) alive
        // for every Font created from them. Nothing is installed system-wide.
        private static readonly Lazy<MonoFonts> Fonts = new Lazy<MonoFonts>(() => new MonoFonts());

        private sealed class MonoFonts
        {
            private readonly PrivateFontCollection collection;
            private readonly Lazy<FontFamily[]> families;
            internal FontFamily Math => Family("Noto Serif");
            internal FontFamily Scripting => Family("Noto Sans Mono");

            internal MonoFonts()
            {
                var directory = Path.Combine(Path.GetDirectoryName(typeof(CustomFonts).Assembly.Location), "Static", "fonts");
                var files = new[] { "NotoSansMono-Regular.ttf", "NotoSerif-Regular.ttf", "NotoSansOlChiki-Regular.ttf" };
                if (Configuration.RuntimeInformation.IsLinux)
                {
                    // Stock Linux libgdiplus 6.1 corrupts private-font metrics.
                    // Fontconfig's application set works with that source-build
                    // dependency and the packaged Pango stack, including fonts
                    // reconstructed by name inside WinForms/TextEditor. A null
                    // config adds only to this process, without a config edit.
                    foreach (var file in files)
                        if (FcConfigAppFontAddFile(IntPtr.Zero, Encoding.UTF8.GetBytes(Path.Combine(directory, file) + "\0")) == 0)
                            throw new InvalidOperationException("The bundled Mono font could not be registered: " + file);
                    families = new Lazy<FontFamily[]>(() => new[] { new FontFamily("Noto Serif"), new FontFamily("Noto Sans Mono") });
                }
                else
                {
                    // macOS ships a private, patched Pango/libgdiplus closure;
                    // keep registration within its existing private font map.
                    collection = new PrivateFontCollection();
                    foreach (var file in files) collection.AddFontFile(Path.Combine(directory, file));
                    families = new Lazy<FontFamily[]>(() => collection.Families);
                }
            }

            private FontFamily Family(string name)
            {
                foreach (var family in families.Value)
                    if (family.Name == name) return family;
                throw new InvalidOperationException("The bundled Mono mathematical font could not be loaded: " + name);
            }

            [DllImport("libfontconfig.so.1", CallingConvention = CallingConvention.Cdecl)]
            private static extern int FcConfigAppFontAddFile(IntPtr config, [In] byte[] file);
        }

        private static bool IsFamily(string actual, string expected) =>
            string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);

        private static Font WithFamily(Font value, FontFamily family) =>
            new Font(family, value.Size, value.Style, value.Unit, value.GdiCharSet, value.GdiVerticalFont);
#endif

        /// <summary>
        /// Register Mono application fonts before any System.Drawing/WinForms
        /// use, so libgdiplus builds its first Pango map with these fonts present.
        /// FontFamily creation is deferred until a font is actually requested.
        /// </summary>
        public static void Initialize()
        {
#if __MonoCS__
            _ = Fonts.Value;
#endif
        }

        public static Font GetFontFromFont(Font value)
        {
#if __MonoCS__
            // Saved fonts must resolve through the process's registered families.
            // Migrate missing historical Windows defaults, but keep installed or
            // explicitly selected alternative families and every size/style setting.
            var requested = value.OriginalFontName ?? value.Name;
            if (IsFamily(requested, "Noto Serif") ||
                (IsFamily(requested, "Cambria") && !IsFamily(value.Name, "Cambria")))
                return WithFamily(value, Fonts.Value.Math);
            if (IsFamily(requested, "Noto Sans Mono") ||
                (IsFamily(requested, "Consolas") && !IsFamily(value.Name, "Consolas")))
                return WithFamily(value, Fonts.Value.Scripting);
            return value;
#else
            if (value.FontFamily.Name.ToLowerInvariant().Contains("cambria"))
            {
                return GetMathFont(value.Size);
            }
            if (value.FontFamily.Name.ToLowerInvariant().Contains("consola"))
            {
                return GetScriptingFont(value.Size);
            }
            return value;
#endif
        }

        public static Font RestoreSavedFont(string name, float size, FontStyle style, GraphicsUnit unit,
            byte gdiCharSet, bool gdiVerticalFont)
        {
#if __MonoCS__
            // Register before constructing a saved Noto font: libgdiplus caches
            // failed family-name lookups even after Fontconfig receives a font.
            Initialize();
#endif
            var requested = new Font(name, size, style, unit, gdiCharSet, gdiVerticalFont);
            try
            {
                var resolved = ResolveSavedFont(requested);
                if (!ReferenceEquals(resolved, requested)) requested.Dispose();
                return resolved;
            }
            catch { requested.Dispose(); throw; }
        }

        private static Font ResolveSavedFont(Font value)
        {
#if __MonoCS__
            return GetFontFromFont(value);
#else
            // Profiles can move back from Mono to Windows. Preserve installed
            // Noto families and every other explicit selection; only an absent
            // bundled default returns to its corresponding Windows default.
            var requested = value.OriginalFontName;
            if (!string.Equals(requested, value.Name, StringComparison.OrdinalIgnoreCase))
            {
                var fallback = string.Equals(requested, "Noto Serif", StringComparison.OrdinalIgnoreCase) ? "Cambria" :
                    string.Equals(requested, "Noto Sans Mono", StringComparison.OrdinalIgnoreCase) ? "Consolas" : null;
                if (fallback != null)
                    return new Font(fallback, value.Size, value.Style, value.Unit, value.GdiCharSet, value.GdiVerticalFont);
            }
            return value;
#endif
        }

        public static Font GetMathFont(float fontSize)
        {
#if __MonoCS__
            return new Font(Fonts.Value.Math, fontSize, GraphicsUnit.Point);
#else
            return new Font("Cambria", fontSize, GraphicsUnit.Point);
#endif
        }

        public static Font GetScriptingFont(float fontSize)
        {
#if __MonoCS__
            return new Font(Fonts.Value.Scripting, fontSize, GraphicsUnit.Point);
#else
            return new Font("Consolas", fontSize, GraphicsUnit.Point);
#endif
        }
    }
}
