#!/usr/bin/env python3
"""Package the actual Mono desktop as an unsigned, runtime-dependent Mac app.

Run natively after Mono-Mac-Validate. Mono and XQuartz are prerequisites; all
non-system drawing/numerical dylibs are copied, relocated and inventoried.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import platform
import plistlib
import re
import shutil
import stat
import subprocess
import sys
import tempfile
import urllib.request
import zipfile

ROOT = Path(__file__).resolve().parents[2]


def run(*command):
    return subprocess.check_output(command, text=True).strip()


def sha(path):
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def verify_bundle(bundle):
    bundle = bundle.absolute()
    resources = bundle / "Contents/Resources"
    if bundle.suffix != ".app" or bundle.is_symlink():
        raise ValueError("Expected a regular .app bundle")
    subprocess.run([sys.executable, str(ROOT / "build/mono/package.py"), "--verify", "--output", str(resources)], check=True)
    metadata = json.loads((resources / "mono-package.json").read_text())
    entries = metadata.get("bundleBootstrap", [])
    allowed = {"Contents/Info.plist", "Contents/MacOS/Computator.NET"}
    if len(entries) != 2 or {entry["path"] for entry in entries} != allowed:
        raise ValueError("Bundle requires the exact two fixed bootstrap files")
    for entry in entries:
        path = bundle / entry["path"]
        component = bundle
        for part in entry["path"].split("/"):
            component /= part
            if component.is_symlink():
                raise ValueError("Bootstrap paths cannot traverse symbolic links")
        if not path.is_file() or path.stat().st_size != entry["size"] or sha(path) != entry["sha256"]:
            raise ValueError("Bundle bootstrap file differs from its inventory: " + entry["path"])
    expected = {"Contents", "Contents/Resources", "Contents/MacOS", *allowed}
    for path in bundle.rglob("*"):
        relative = path.relative_to(bundle).as_posix()
        if relative.startswith("Contents/Resources/"):
            continue  # Complete confined Resources inventory was checked above.
        if path.is_symlink() or relative not in expected:
            raise ValueError("Unrecorded bundle bootstrap item: " + relative)
    if not os.access(bundle / "Contents/MacOS/Computator.NET", os.X_OK):
        raise ValueError("Bundle launcher is not executable")
    return metadata


def archive_bundle(bundle, archive):
    if archive.exists() or archive.is_symlink() or archive.resolve().is_relative_to(bundle.resolve()):
        raise ValueError("Archive must be new and outside the application bundle")
    archive.parent.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(archive, "x", compression=zipfile.ZIP_DEFLATED, compresslevel=6) as output:
        for path in sorted(bundle.rglob("*")):
            if path.is_symlink():
                raise ValueError("No symbolic links are supported in this bundle archive")
            if not path.is_file():
                continue
            item = zipfile.ZipInfo((Path(bundle.name) / path.relative_to(bundle)).as_posix(), (2020, 1, 1, 0, 0, 0))
            item.create_system = 3
            item.external_attr = (stat.S_IFREG | stat.S_IMODE(path.stat().st_mode)) << 16
            item.compress_type = zipfile.ZIP_DEFLATED
            with path.open("rb") as source, output.open(item, "w") as target:
                shutil.copyfileobj(source, target)
    return sha(archive)


def extract_bundle(archive, output):
    if output.exists() or output.is_symlink():
        raise ValueError("Extraction requires a new output directory")
    with zipfile.ZipFile(archive) as source:
        entries = source.infolist()
        if not entries or len(entries) > 20000 or sum(item.file_size for item in entries) > 2 * 1024 ** 3:
            raise ValueError("Archive inventory is empty or exceeds the bounded package limit")
        names = set()
        root = None
        for item in entries:
            name = item.filename
            parts = name.split("/")
            mode = item.external_attr >> 16
            if (not name or name.startswith("/") or "\\" in name or any(part in ("", ".", "..") for part in parts)
                    or ":" in name or name.casefold() in names or item.file_size > 512 * 1024 ** 2
                    or not stat.S_ISREG(mode)):
                raise ValueError("Unsafe, duplicate or non-regular bundle archive member: " + name)
            names.add(name.casefold())
            if root is None:
                root = parts[0]
            if parts[0] != root or not root.endswith(".app") or len(parts) < 3:
                raise ValueError("Archive must contain exactly one complete .app root")
        output.mkdir(parents=True)
        for item in entries:
            path = output.joinpath(*item.filename.split("/"))
            path.parent.mkdir(parents=True, exist_ok=True)
            with source.open(item) as stream, path.open("xb") as target:
                shutil.copyfileobj(stream, target)
            path.chmod(stat.S_IMODE(item.external_attr >> 16))
    bundle = output / root
    verify_bundle(bundle)
    return bundle


def dependencies(path):
    result = []
    for line in run("otool", "-L", str(path)).splitlines()[1:]:
        match = re.match(r"\s+(.+?) \(compatibility version", line)
        if match:
            result.append(match.group(1))
    return result


def deployment_minimum(path):
    versions=[]
    for block in re.split(r"\nLoad command \d+\n",run("otool","-l",str(path))):
        if re.search(r"\bcmd LC_BUILD_VERSION\b",block):
            match=re.search(r"^\s*minos (\d+\.\d+(?:\.\d+)?)\s*$",block,re.M)
        elif re.search(r"\bcmd LC_VERSION_MIN_MACOSX\b",block):
            match=re.search(r"^\s*version (\d+\.\d+(?:\.\d+)?)\s*$",block,re.M)
        else:
            continue
        if not match:
            raise ValueError("Cannot read the actual Mach-O deployment minimum: "+str(path))
        version=tuple(map(int,match[1].split('.')))
        versions.append(version+(0,)*(3-len(version)))
    if not versions:
        raise ValueError("Packaged dylib has no reviewed macOS deployment command: "+str(path))
    return max(versions)


def resolve_dependency(name, owner):
    if name.startswith(("/usr/lib/", "/System/Library/")):
        return None
    if name.startswith("@loader_path/"):
        return (owner.parent / name.removeprefix("@loader_path/")).resolve(strict=True)
    if name.startswith("/"):
        return Path(name).resolve(strict=True)
    # Guessing a different ABI from a same-named library is never acceptable.
    raise ValueError(f"Unresolved Mach-O dependency {name} in {owner}; add an explicit reviewed resolution.")


def copy_closure(selected, target, architecture):
    queue = list(selected.values())
    libraries = {}
    names = {}
    while queue:
        source = queue.pop().resolve(strict=True)
        if source in libraries:
            continue
        name = source.name
        if name in names and names[name] != source:
            raise ValueError(f"Conflicting dylib basenames: {source} and {names[name]}")
        names[name] = source
        subprocess.run(["lipo", str(source), "-verify_arch", architecture], check=True)
        links = dependencies(source)
        libraries[source] = links
        for link in links:
            dependency = resolve_dependency(link, source)
            if dependency is not None and dependency != source:
                queue.append(dependency)
    target.mkdir(parents=True)
    for source in libraries:
        shutil.copy2(source, target / source.name)
        (target / source.name).chmod(0o755)
    provenance = []
    for source, links in libraries.items():
        copied = target / source.name
        subprocess.run(["codesign", "--remove-signature", str(copied)], check=False,
                       stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        subprocess.run(["install_name_tool", "-id", "@loader_path/" + copied.name, str(copied)], check=True)
        for link in links:
            dependency = resolve_dependency(link, source)
            if dependency is not None:
                subprocess.run(["install_name_tool", "-change", link,
                                "@loader_path/" + dependency.name, str(copied)], check=True)
        # Apple Silicon requires valid code signatures after modifying Mach-O.
        # This is local integrity signing with no owner identity or notarization.
        subprocess.run(["codesign", "--force", "--sign", "-", "--timestamp=none", str(copied)], check=True)
        subprocess.run(["codesign", "--verify", "--strict", str(copied)], check=True)
        for link in dependencies(copied):
            if not link.startswith(("/usr/lib/", "/System/Library/", "@loader_path/")):
                raise ValueError(f"Relocated library retains a host dependency: {link}")
        provenance.append({"originalPath": str(source), "originalSha256": sha(source),
                           "packagedFile": copied.name, "packagedSha256": sha(copied),
                           "macOSDeploymentMinimum": ".".join(map(str,deployment_minimum(copied))),
                           "originalDependencies": links, "packagedDependencies": dependencies(copied)})
    return provenance


def retain_sources(provenance, prepared, resources):
    """Retain corresponding pinned sources and installed build recipes, not just URLs."""
    target = resources / "share/native-sources"
    target.mkdir()
    packages = {}
    for item in provenance:
        source = Path(item["originalPath"])
        parts = source.parts
        if "Cellar" not in parts:
            continue  # The private libgdiplus source is retained separately below.
        index = parts.index("Cellar")
        keg = Path(*parts[:index + 3])
        formula = parts[index + 1]
        if formula in packages:
            continue
        recipe = keg / ".brew" / (formula + ".rb")
        content = recipe.read_text()
        # Installed recipes, unlike today's rolling formula API, describe the
        # binaries actually copied. Reject unpinned/interpolated source recipes.
        match = re.search(r'\burl "(https://[^"#]+)"\s*\n\s*(?:mirror [^\n]+\n\s*)?sha256 "([0-9a-f]{64})"', content)
        if not match:
            raise ValueError(f"No literal pinned source archive in installed formula {recipe}")
        url, expected = match.groups()
        directory = target / formula
        directory.mkdir()
        shutil.copy2(recipe, directory / recipe.name)
        receipt = keg / "INSTALL_RECEIPT.json"
        if receipt.is_file():
            shutil.copy2(receipt, directory / receipt.name)
        filename = Path(url.split("?", 1)[0]).name
        archive = directory / filename
        urllib.request.urlretrieve(url, archive)
        if sha(archive) != expected:
            raise ValueError("Corresponding native source hash mismatch: " + formula)
        licenses = []
        for path in keg.rglob("*"):
            if path.is_file() and not path.is_symlink() and re.match(r"^(COPYING|LICENSE|NOTICE|COPYRIGHT)(\.|$)", path.name, re.I):
                destination = directory / "licenses" / path.relative_to(keg)
                destination.parent.mkdir(parents=True, exist_ok=True)
                shutil.copy2(path, destination)
                licenses.append(destination.relative_to(resources).as_posix())
        packages[formula] = {"installedVersion": parts[index + 2], "url": url, "sha256": expected,
                             "sourceArchive": archive.relative_to(resources).as_posix(), "licenses": licenses}
    shutil.copy2(prepared / "downloads/libgdiplus-6.2.tar.gz", target / "libgdiplus-6.2.tar.gz")
    shutil.copy2(prepared / "downloads/libgdiplus-6.2-complete-git.tar.gz", target / "libgdiplus-6.2-complete-git.tar.gz")
    shutil.copy2(ROOT / "build/macos/native-dependencies.json", target / "native-dependencies.json")
    shutil.copy2(ROOT / "build/mono/gdiplus-source.json", target / "gdiplus-source.json")
    shutil.copy2(ROOT / "build/mono/libgdiplus-pango-fontmap.patch", target / "libgdiplus-pango-fontmap.patch")
    (target / "LOCAL-CHANGES.txt").write_text(
        "libgdiplus6.2: build with X11 and Pango. In its generated configure script, replace the single "
        "Darwin no_libpng16=yes assignment with no_libpng16=, retaining normal libpng16 checks. "
        "All packaged dylibs: replace only non-system Mach-O load-command paths with @loader_path, "
        "then apply local ad-hoc integrity signatures. The included Pango fontmap patch invalidates the private-font "
        "cache after registration. The ARM64 XIM shim bridges only the five reviewed Mono6.14.1 varargs imports. "
        "Build scripts and shim sources are in the included Computator.NET source archive.\n")
    return packages


def bundle_icon(resources):
    # Reuse the project's existing artwork; do not invent a new application icon.
    with tempfile.TemporaryDirectory(prefix="computator-mac-icon-") as temporary:
        iconset=Path(temporary)/"Computator.NET.iconset"
        iconset.mkdir()
        for size in (16,32,128,256,512):
            for scale in (1,2):
                name=f"icon_{size}x{size}"+("@2x" if scale==2 else "")+".png"
                subprocess.run(["/usr/bin/sips","--resampleHeightWidth",str(size*scale),str(size*scale),
                                str(resources/"share/computator.png"),"--out",str(iconset/name)],check=True,
                               stdout=subprocess.DEVNULL)
        subprocess.run(["/usr/bin/iconutil","--convert","icns",str(iconset),"--output",str(resources/"Computator.NET.icns")],check=True)


def runtime_data(provenance,resources):
    dictionaries=[]
    for item in provenance:
        source=Path(item['originalPath'])
        parts=source.parts
        if 'Cellar' not in parts:
            continue
        index=parts.index('Cellar')
        if parts[index+1]!='libthai':
            continue
        dictionary=Path(*parts[:index+3])/'share/libthai/thbrk.tri'
        if not dictionary.is_file() or dictionary.is_symlink():
            raise ValueError('The copied libthai requires its exact regular dictionary data: '+str(dictionary))
        target=resources/'native/libthai/thbrk.tri'
        target.parent.mkdir(parents=True,exist_ok=True)
        shutil.copy2(dictionary,target)
        dictionaries.append({'library':item['packagedFile'],'path':target.relative_to(resources).as_posix(),
                             'originalPath':str(dictionary),'sha256':sha(target),'size':target.stat().st_size})
    if len(dictionaries)>1:
        raise ValueError('More than one libthai dictionary provider was selected')
    return dictionaries


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=Path, required=True, help="new .app path")
    parser.add_argument("--configuration", choices=("Release", "Debug"), default="Release")
    parser.add_argument("--archive", type=Path)
    actions = parser.add_mutually_exclusive_group()
    actions.add_argument("--verify", action="store_true")
    actions.add_argument("--extract", type=Path)
    args = parser.parse_args()
    if args.verify:
        verify_bundle(args.output)
        print("Verified complete Mac bundle:", args.output)
        return
    if args.extract:
        print("Extracted and verified complete Mac bundle:", extract_bundle(args.extract, args.output))
        return
    if platform.system() != "Darwin" or platform.machine() not in ("arm64", "x86_64"):
        parser.error("native Mac packaging is required; a cross-publish is not acceptance")
    bundle = args.output.absolute()
    if bundle.suffix != ".app" or bundle.exists() or bundle.is_symlink():
        parser.error("--output must be a new .app path; existing data is never replaced")
    architecture = platform.machine()
    rid = "osx-arm64" if architecture == "arm64" else "osx-x64"
    prepared = ROOT / "artifacts/macos" / ("native-" + architecture)
    native = json.loads((prepared / "native-manifest.json").read_text())
    if native["architecture"] != architecture:
        raise ValueError("Prepared native graph differs from this packaging process")
    resources = bundle / "Contents/Resources"
    subprocess.run([sys.executable, str(ROOT / "build/mono/package.py"), "--stage-only",
                    "--configuration", args.configuration, "--output", str(resources)], check=True)
    if not (resources / "app/Computator.NET.Mono.Preflight.exe").is_file():
        raise ValueError("Build the shared Mono runtime preflight before packaging")
    entries = {entry["name"]: Path(entry["path"]) for entry in native["libraries"]}
    selected = {"gsl": entries["gsl"], "cblas": entries["gslcblas"], "gdiplus": entries["gdiplus"],
                "cairo": entries["libcairo-2.dll"], "x11": entries["libX11"],
                "xtst": entries["libXtst"], "xinerama": entries["libXinerama"],
                "xext": entries["libXext"], "xcursor": entries["libXcursor"], "xfixes": entries["libXfixes"]}
    if architecture == "arm64":
        selected["xim"] = entries["computator_xim"]
    destination = resources / "native" / rid
    provenance = copy_closure(selected, destination, architecture)
    dictionaries=runtime_data(provenance,resources)
    minimum=max((15,0,0),*(tuple(map(int,item['macOSDeploymentMinimum'].split('.'))) for item in provenance))
    minimum_text='.'.join(map(str,minimum[:2] if minimum[2]==0 else minimum))
    corresponding_sources = retain_sources(provenance, prepared, resources)
    for source, target in (("launcher.sh", bundle / "Contents/MacOS/Computator.NET"),
                           ("PACKAGE.txt", resources / "README.txt")):
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(ROOT / "build/macos" / source, target)
    instructions=resources/'README.txt'
    instructions.write_text(instructions.read_text().replace('@MINIMUM_MACOS_VERSION@',minimum_text))
    (bundle / "Contents/MacOS/Computator.NET").chmod(0o755)
    bundle_icon(resources)
    # Fontconfig must not retain a dependency on the build account's Homebrew.
    (resources / "native/fonts.conf").write_text('<?xml version="1.0"?>\n<!DOCTYPE fontconfig SYSTEM "urn:fontconfig:fonts.dtd">\n'
        '<fontconfig><dir>/System/Library/Fonts</dir><dir>/Library/Fonts</dir><dir>~/Library/Fonts</dir>'
        '<cachedir prefix="xdg">fontconfig</cachedir><alias><family>sans-serif</family><prefer><family>Arial</family>'
        '</prefer></alias><alias><family>monospace</family><prefer><family>Menlo</family></prefer></alias></fontconfig>\n')
    (resources / "native/gio-modules").mkdir()
    (resources / "native/gio-modules/README.txt").write_text(
        "No external GIO loadable modules are required by the accepted numerical/drawing desktop workflows. "
        "The launcher selects this private directory, rather than loading build-host Homebrew modules.\n")
    metadata = json.loads((resources / "mono-package.json").read_text())
    accepted_source=None
    reports=ROOT/'artifacts/macos'/('acceptance-'+architecture)/args.configuration
    for report_path in sorted(reports.glob('run-*/acceptance.json'),reverse=True):
        report=json.loads(report_path.read_text())
        if (report.get('passed') and report.get('checkoutCommit')==metadata['source']['commit']
                and not report.get('checkoutDirty') and report.get('architecture')==architecture):
            accepted_source=report
            break
    # An exclusive diagnostic Cake task may stage a red GUI build to investigate
    # authoring. Such an archive is explicitly unaccepted, never a release pass.
    metadata['sourceCheckoutAcceptance']=accepted_source or {
        'passed':False,'scope':'diagnostic-unaccepted-source-checkout',
        'reason':'No complete current-head native application acceptance report exists.'}
    (resources/'share/source-checkout-acceptance.json').write_text(json.dumps(metadata['sourceCheckoutAcceptance'],indent=2)+'\n')
    metadata.update(rid=rid, native={name: (Path("native") / rid / path.name).as_posix() for name, path in selected.items()},
                    runtime={"monoTestedVersion": "6.14.1", "monoMinimumVersion": "6.14.1", "xquartzVersion": "2.8.6", "framework": "net48",
                             "macOSMinimum": minimum_text,
                             "externalPrerequisites": ["native Mono>=6.14.1 with reviewed WinForms ABI", "XQuartz2.8.6", "macOS15 system fonts"],
                             "ownerSigned": False, "notarized": False, "adHocDylibIntegritySignatures": True},
                    nativeProvenance={"libraries": provenance, "correspondingSources": corresponding_sources})
    (resources / "mono-package.json").write_text(json.dumps(metadata, indent=2) + "\n")
    version = metadata["version"].split("-", 1)[0]
    with (bundle / "Contents/Info.plist").open("wb") as stream:
        plistlib.dump({"CFBundleName": "Computator.NET", "CFBundleDisplayName": "Computator.NET",
                      "CFBundleIdentifier": "net.computator.mono", "CFBundleExecutable": "Computator.NET",
                      "CFBundleIconFile": "Computator.NET.icns",
                      "CFBundlePackageType": "APPL", "CFBundleVersion": version,
                      "CFBundleShortVersionString": version, "LSMinimumSystemVersion": minimum_text,
                      "NSHighResolutionCapable": True}, stream)
    metadata["bundleBootstrap"] = [{"path": name, "size": (bundle / name).stat().st_size,
                                    "sha256": sha(bundle / name)}
                                   for name in ("Contents/Info.plist", "Contents/MacOS/Computator.NET")]
    metadata["runtimeData"] = {"fontconfig": "native/fonts.conf", "fonts": "macOS system/user font directories",
                               "thaiDictionaries": dictionaries,
                               "gioModules": "native/gio-modules", "xLocale": "/opt/X11/share/X11/locale",
                               "icu": "data dylib included when present in the Mach-O dependency closure"}
    (resources / "mono-package.json").write_text(json.dumps(metadata, indent=2) + "\n")
    subprocess.run([sys.executable, str(ROOT / "build/mono/package.py"), "--finalize", "--output", str(resources),
                    "--rid", rid, "--gsl", metadata["native"]["gsl"], "--cblas", metadata["native"]["cblas"],
                    "--gdiplus", metadata["native"]["gdiplus"]], check=True)
    verify_bundle(bundle)
    if args.archive:
        print("Archive SHA256:", archive_bundle(bundle, args.archive))
    print("Prepared unsigned runtime-dependent Mono app:", bundle)
    print("Complete current-head source application acceptance:",bool(accepted_source))


if __name__ == "__main__":
    try:
        main()
    except (OSError, ValueError, KeyError, subprocess.CalledProcessError) as error:
        print("Native Mac packaging failed:", error, file=sys.stderr)
        sys.exit(1)
