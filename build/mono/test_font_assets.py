"""Offline provenance and glyph coverage for the actual bundled Mono fonts.

Only the standard library is required. Runtime rendering remains a separate
native acceptance case; a cmap entry cannot prove Pango's font selection.
"""
import hashlib
import json
from pathlib import Path
import re
import struct
import unittest


ROOT = Path(__file__).resolve().parents[2]
FONTS = ROOT / "Computator.NET.DataTypes/Fonts"


def codepoints(path):
    data = path.read_bytes()

    def u16(offset):
        return struct.unpack_from(">H", data, offset)[0]

    def u32(offset):
        return struct.unpack_from(">I", data, offset)[0]

    table = None
    for index in range(u16(4)):
        entry = 12 + 16 * index
        if data[entry:entry + 4] == b"cmap":
            table = u32(entry + 8)
            break
    if table is None:
        raise ValueError("Font has no cmap: " + str(path))
    points = set()
    for index in range(u16(table + 2)):
        entry = table + 4 + 8 * index
        platform, encoding = u16(entry), u16(entry + 2)
        if platform != 0 and not (platform == 3 and encoding in (1, 10)):
            continue
        mapping = table + u32(entry + 4)
        if u16(mapping) == 4:
            count = u16(mapping + 6) // 2
            ends = mapping + 14
            starts = ends + 2 * count + 2
            deltas = starts + 2 * count
            offsets = deltas + 2 * count
            for segment in range(count):
                delta = u16(deltas + 2 * segment)
                address = offsets + 2 * segment
                shift = u16(address)
                for point in range(u16(starts + 2 * segment), u16(ends + 2 * segment) + 1):
                    glyph = (point + delta) & 0xffff
                    if shift:
                        glyph = u16(address + shift + 2 * (point - u16(starts + 2 * segment)))
                        if glyph:
                            glyph = (glyph + delta) & 0xffff
                    if glyph and point != 0xffff:
                        points.add(point)
        elif u16(mapping) == 12:
            for group in range(u32(mapping + 12)):
                first, last, glyph = struct.unpack_from(">III", data, mapping + 16 + group * 12)
                points.update(range(first + (glyph == 0), last + 1))
    return points


def symbol_constants():
    source = (ROOT / "Computator.NET.DataTypes/Text/SpecialSymbols.cs").read_text(encoding="utf-8-sig")
    source = re.sub(r"//[^\n]*", "", source)
    values = {}
    # This vocabulary consists of literal strings/chars and earlier constants
    # joined with '+'. Reject any new syntax instead of silently missing glyphs.
    for name, expression in re.findall(r"const\s+(?:string|char)\s+(\w+)\s*=\s*(.*?);", source, re.S):
        token = re.compile(r'''\s*(?:@?"([^"]*)"|'([^']*)'|(\w+)|(\+))''')
        value, position, expect_value = "", 0, True
        while position < len(expression.rstrip()):
            match = token.match(expression, position)
            if not match or (match.group(4) is None) != expect_value:
                raise ValueError("Unsupported symbol constant: " + name)
            if expect_value:
                value += match.group(1) if match.group(1) is not None else (
                    match.group(2) if match.group(2) is not None else values[match.group(3)])
            position, expect_value = match.end(), not expect_value
        if expect_value:
            raise ValueError("Incomplete symbol constant: " + name)
        values[name] = value
    return values


class MonoFontAssetsTests(unittest.TestCase):
    def setUp(self):
        self.manifest = json.loads((FONTS / "manifest.json").read_text())

    def test_assets_are_unmodified_pinned_upstream_files_with_their_license(self):
        self.assertEqual(self.manifest["schemaVersion"], 1)
        self.assertRegex(self.manifest["commit"], r"^[0-9a-f]{40}$")
        entries = self.manifest["fonts"] + [self.manifest["license"]]
        self.assertEqual({entry["file"] for entry in self.manifest["fonts"]},
                         {path.name for path in FONTS.glob("*.ttf")})
        for entry in entries:
            with self.subTest(file=entry["file"]):
                self.assertEqual(Path(entry["file"]).name, entry["file"])
                self.assertIn("/" + self.manifest["commit"] + "/", entry["source"])
                self.assertEqual(hashlib.sha256((FONTS / entry["file"]).read_bytes()).hexdigest(), entry["sha256"])
        self.assertIn("SIL OPEN FONT LICENSE", (FONTS / self.manifest["license"]["file"]).read_text())

    def test_every_current_tsl_symbol_has_a_real_bundled_glyph(self):
        coverage = set().union(*(codepoints(FONTS / entry["file"]) for entry in self.manifest["fonts"]))
        constants = symbol_constants()
        self.assertEqual(constants["DecimalSeparatorSuperscript"], "ᱸ")
        self.assertIn("˸", constants["OperatorsSuperscript"])
        for name, symbols in constants.items():
            with self.subTest(constant=name):
                self.assertEqual(["U+%04X" % ord(char) for char in symbols if ord(char) not in coverage], [])


if __name__ == "__main__":
    unittest.main()
