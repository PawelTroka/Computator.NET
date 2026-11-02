"""Create an isolated Mono configuration and launcher; never modify installed files."""
import hashlib
import json
import os
from pathlib import Path
import platform
import shlex
import subprocess
import xml.etree.ElementTree as ET

root = Path(os.environ["COMPUTATOR_MAC_NATIVE_ROOT"]).resolve()
mono = Path(os.environ["COMPUTATOR_MAC_MONO_PREFIX"]).resolve()
brew = Path(os.environ["COMPUTATOR_MAC_BREW_PREFIX"])
config = ET.parse(mono / "etc/mono/config")


def library(formula, name):
    prefix = Path(subprocess.check_output(["brew", "--prefix", formula], text=True).strip())
    result = (prefix / "lib" / name).resolve(strict=True)
    subprocess.run(["lipo", str(result), "-verify_arch", platform.machine()], check=True)
    return result


gdiplus = (root / "libgdiplus/lib/libgdiplus.dylib").resolve(strict=True)
subprocess.run(["lipo", str(gdiplus), "-verify_arch", platform.machine()], check=True)
x11 = library("libx11", "libX11.6.dylib")
xtst = library("libxtst", "libXtst.6.dylib")
gsl = library("gsl", "libgsl.dylib")
gslcblas = library("gsl", "libgslcblas.dylib")
cairo = library("cairo", "libcairo.2.dylib")
mappings = {name: gdiplus for name in ("gdiplus", "gdiplus.dll", "gdi32", "gdi32.dll", "libgdiplus")}
mappings.update({name: x11 for name in ("libX11", "libX11.so.6")})
mappings.update({name: xtst for name in ("libXtst", "libXtst.so.6")})
mappings.update({"libXinerama": library("libxinerama", "libXinerama.1.dylib"),
                 "libXext": library("libxext", "libXext.6.dylib"),
                 "libXcursor": library("libxcursor", "libXcursor.1.dylib"),
                 "libXfixes": library("libxfixes", "libXfixes.3.dylib"),
                 "libcairo-2.dll": cairo, "gsl": gsl, "gslcblas": gslcblas})
mappings["computator_mac_display"] = (root / "display/libcomputator-mac-display.dylib").resolve(strict=True)
shim = root / "xim/libcomputator-mono-xim.dylib"
if platform.machine() == "arm64":
    mappings["computator_xim"] = shim.resolve(strict=True)
# Remove only the selected names from the copied configuration. This avoids
# depending on parser duplicate ordering and retains all unrelated system maps.
for entry in list(config.getroot()):
    if entry.tag == "dllmap" and entry.get("dll") in mappings:
        config.getroot().remove(entry)
for name, target in mappings.items():
    entry = ET.SubElement(config.getroot(), "dllmap", {"dll": name, "target": str(target), "os": "osx"})
    if name == "libX11" and platform.machine() == "arm64":
        for function in ("XGetIMValues", "XGetICValues", "XSetICValues", "XCreateIC", "XVaCreateNestedList"):
            ET.SubElement(entry, "dllentry", {"name": function, "dll": str(shim), "target": "ComputatorMono" + function})
path = root / "mono.config"
# Framework Mono's native config reader rejects ElementTree's declaration on
# affected builds. Use the same headerless format as the runtime's own config.
config.write(path, encoding="utf-8", xml_declaration=False)
q = shlex.quote
launcher = root / "bin/mono"
launcher.write_text("#!/bin/bash\nset -euo pipefail\n"
                    "export MONO_MWF_MAC_FORCE_X11=1\n"
                    "export COMPUTATOR_MAC_DRAWABLE_PROBE=1\n"
                    + ("export COMPUTATOR_MAC_XIM_SHIM=1\n"
                       "if [[ ${COMPUTATOR_MAC_XIM_MODE:-enabled} == disabled ]]; then export MONO_WINFORMS_XIM_STYLE=disabled; "
                       "else unset MONO_WINFORMS_XIM_STYLE; fi\n" if platform.machine() == "arm64" else "") +
                    f"exec {q(str(mono / 'bin/mono'))} --config {q(str(path))} \"$@\"\n")
launcher.chmod(0o755)
# mcs is already a script that invokes mono. The process-scoped wrapper on PATH
# makes its compiler process use the same runtime without editing that script.
mcs = root / "bin/mcs"
mcs.write_text("#!/bin/bash\nset -euo pipefail\n"
               f"export PATH={q(str(root / 'bin'))}:\"$PATH\"\n"
               f"exec {q(str(mono / 'bin/mcs'))} \"$@\"\n")
mcs.chmod(0o755)
manifest = {"schemaVersion": 1, "architecture": platform.machine(), "monoPrefix": str(mono),
            "machineWideConfigurationChanged": False, "monoConfig": str(path),
            "monoConfigSha256": hashlib.sha256(path.read_bytes()).hexdigest(),
            "libraries": [{"name": name, "path": str(target),
                           "sha256": hashlib.sha256(target.read_bytes()).hexdigest()}
                          for name, target in mappings.items()]}
(root / "native-manifest.json").write_text(json.dumps(manifest, indent=2) + "\n")
print(json.dumps(manifest, indent=2))
