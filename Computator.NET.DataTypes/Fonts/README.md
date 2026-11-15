# Mono mathematical fonts

These **unmodified** Noto font files are distributed under the SIL Open Font
License 1.1 in `OFL.txt`. `manifest.json` pins their upstream Git commit, exact
source URLs, font versions, copyright notices and SHA-256 hashes. No Windows
font is included.

The Mono desktop registers these fonts only for its process. It uses
Noto Serif for the default mathematical text and Noto Sans Mono for the default
script editor. Noto Sans Ol Chiki supplies U+1C78, the existing TSL superscript
decimal separator. Both primary fonts include U+02F8, the existing superscript
division symbol. The source spelling and evaluation of old documents do not
change. Explicit other font selections remain the user's choice.

The files are copied to `Static/fonts` only in the Mono build, together with
this provenance and license. The existing Linux archive and macOS bundle copy
that application content and include it in their hash inventories. Registration
does not install fonts or modify Fontconfig settings outside the application.
Windows retains its Cambria/Consolas defaults.
When restoring a saved Noto default on Windows, an installed matching family is
kept; otherwise its corresponding Cambria/Consolas default is used with the
saved size, style and units. Other saved family names are not rewritten.

`Startup.Start` calls `CustomFonts.Initialize` before the first
System.Drawing/WinForms use. Linux adds the files to Fontconfig's application
font set. This supports both the stock libgdiplus 6.1 source-build dependency
and the packaged drawing stack: stock 6.1 has broken private-font metrics, and
its Pango map caches available fonts on first use. Registration must therefore
happen at startup, rather than after drawing begins. Font families themselves
are created lazily, including when the first requested font comes from saved
settings. The shared Mono acceptance executable follows the same startup order.
macOS keeps a `PrivateFontCollection` alive in its existing, patched private
Pango/libgdiplus drawing stack.

Run the offline asset/hash/TSL coverage check with:

```sh
python3 -m unittest discover -s build/mono -p test_font_assets.py
```

Native acceptance also renders the actual superscript vocabulary through
System.Drawing and the desktop editors, restores saved font settings and checks
numeric-control layout. A file's cmap coverage alone does not establish that a
renderer displays the glyphs. See the
[Linux before/after evidence](../../docs/evidence/mono-fonts-2026-11-15/README.md)
for both tested drawing stacks and the remaining platform boundaries.
