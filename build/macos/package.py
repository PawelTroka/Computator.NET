#!/usr/bin/env python3
"""Package the actual Mono desktop as an unsigned, runtime-dependent Mac app.

Run natively after Mono-Mac-Validate. Mono and XQuartz are prerequisites; all
non-system drawing/numerical dylibs are copied, relocated and inventoried.
"""
import argparse
import base64
import datetime
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
import urllib.parse
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


def download_verified(url,expected,destination):
    if destination.exists() or destination.is_symlink():
        raise ValueError('Native source inputs must have distinct destinations; refusing to overwrite '+str(destination))
    urllib.request.urlretrieve(url,destination)
    if sha(destination)!=expected:
        raise ValueError('Native source input hash mismatch: '+url)


class ImmutableGitSourceRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self,request,file,code,message,headers,new_url):
        # A scoped CI token must never follow a redirect to another host or
        # endpoint. These exact immutable public contents URLs need no redirect.
        raise ValueError('Immutable Git source API reads must not redirect')


def pinned_git_blob(commit,path,destination,expected_sha256=None):
    if (not re.fullmatch('[0-9a-f]{40}',commit) or not path.startswith(('Formula/','Patches/'))
            or '\\' in path or ':' in path or any(part in ('','.','..') for part in path.split('/'))):
        raise ValueError('Unsafe immutable Homebrew source reference')
    if destination.exists() or destination.is_symlink():
        raise ValueError('Refusing to overwrite a retained Git source input: '+str(destination))
    url='https://api.github.com/repos/Homebrew/homebrew-core/contents/'+urllib.parse.quote(path,safe='/')+'?ref='+commit
    headers={'Accept':'application/vnd.github+json','User-Agent':'Computator.NET-native-source-verification'}
    token=os.environ.get('COMPUTATOR_GITHUB_TOKEN','')
    if token: headers['Authorization']='Bearer '+token
    request=urllib.request.Request(url,headers=headers)
    opener=urllib.request.build_opener(ImmutableGitSourceRedirect())
    with opener.open(request,timeout=30) as response:
        payload=response.read(2*1024*1024+1)
    if len(payload)>2*1024*1024: raise ValueError('Pinned Git input exceeds the bounded metadata limit')
    item=json.loads(payload)
    if (item.get('type')!='file' or item.get('path')!=path or item.get('encoding')!='base64'
            or not re.fullmatch('[0-9a-f]{40}',item.get('sha',''))):
        raise ValueError('Pinned Git response does not describe the exact requested file')
    data=base64.b64decode(''.join(item['content'].split()),validate=True)
    blob=hashlib.sha1(b'blob '+str(len(data)).encode()+b'\0'+data).hexdigest()
    digest=hashlib.sha256(data).hexdigest()
    if item.get('size')!=len(data) or blob!=item['sha'] or (expected_sha256 is not None and digest!=expected_sha256):
        raise ValueError('Pinned Git file differs from its blob identity or declared SHA256')
    with destination.open('xb') as stream: stream.write(data)
    return {'commit':commit,'path':path,'gitBlobSha1':blob,'sha256':digest,'declaredSha256':expected_sha256,
            'authenticatedApiRead':bool(token),
            'basis':'immutable receipt commit and verified Git blob'+('; declared SHA256 also checked' if expected_sha256 else '; SHA256 computed from verified bytes')}


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
    # otool -L also prints LC_ID_DYLIB. Parse command kinds so a self install
    # identity is never mistaken for a dependency (including outside Cellar).
    kinds={'LC_LOAD_DYLIB','LC_LOAD_WEAK_DYLIB','LC_REEXPORT_DYLIB','LC_LOAD_UPWARD_DYLIB','LC_LAZY_LOAD_DYLIB'}
    return command_names(path,kinds)


def command_names(path,kinds):
    names=[]
    for block in re.split(r'\nLoad command \d+\n',run('otool','-l',str(path))):
        command=re.search(r'^\s*cmd (LC_[A-Z_0-9]+)\s*$',block,re.M)
        if command and command[1] in kinds:
            name=re.search(r'^\s*name (.+?) \(offset \d+\)\s*$',block,re.M)
            if not name: raise ValueError('Cannot read an actual Mach-O dylib name in '+str(path))
            names.append(name[1])
    return names


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
    if name.startswith('@rpath/'):
        relative=name.removeprefix('@rpath/')
        if not relative or '/' in relative or relative in ('.','..'):
            raise ValueError('Only a reviewed single-library @rpath name is supported: '+name)
        candidates=[]
        for base in load_rpaths(owner):
            if base=='@loader_path' or base.startswith('@loader_path/'):
                directory=owner.parent/base.removeprefix('@loader_path').lstrip('/')
            elif base.startswith('/'):
                directory=Path(base)
            else:
                raise ValueError('Unreviewed Mach-O runpath '+base+' in '+str(owner))
            candidate=directory/relative
            if candidate.is_file():
                candidates.append(candidate.resolve(strict=True))
        # Homebrew CMake siblings (observed in Brotli/WebP) can retain @rpath
        # install IDs without an owner LC_RPATH. Resolve only the exact sibling
        # in the SAME installed formula/version keg, whose LC_ID_DYLIB equals
        # the requested ABI name. Never search arbitrary host library folders.
        if not candidates and 'Cellar' in owner.parts:
            index=owner.parts.index('Cellar')
            keg=Path(*owner.parts[:index+3])
            candidate=owner.parent/relative
            if candidate.is_file():
                candidate=candidate.resolve(strict=True)
                identities=command_names(candidate,{'LC_ID_DYLIB'})
                if candidate.is_relative_to(keg) and name in identities:
                    candidates.append(candidate)
        candidates=list(dict.fromkeys(candidates))
        if len(candidates)!=1:
            raise ValueError('Mach-O runpath must resolve one explicit installed ABI: '+name+' in '+str(owner))
        return candidates[0]
    # Guessing a different ABI from a same-named library is never acceptable.
    raise ValueError(f"Unresolved Mach-O dependency {name} in {owner}; add an explicit reviewed resolution.")


def load_rpaths(owner):
    paths=[]
    for block in re.split(r'\nLoad command \d+\n',run('otool','-l',str(owner))):
        if re.search(r'\bcmd LC_RPATH\b',block):
            match=re.search(r'^\s*path (.+?) \(offset \d+\)\s*$',block,re.M)
            if not match:
                raise ValueError('Cannot read actual LC_RPATH in '+str(owner))
            paths.append(match[1])
    return paths


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
    x11=next((source for source in libraries if source.name.startswith('libX11.')),None)
    if x11 is not None:
        signing_layout_probe(x11,architecture)
    provenance = []
    for source, links in libraries.items():
        copied = target / source.name
        # Edit intact signed Mach-O inputs. Removing the signature first can
        # leave a LINKEDIT layout gap rejected by Apple's install_name_tool.
        # Its documented signed-input path warns/updates the old signature;
        # replace it with the new ad-hoc integrity signature after ALL edits.
        edit=["install_name_tool", "-id", "@loader_path/" + copied.name]
        for link in links:
            dependency = resolve_dependency(link, source)
            if dependency is not None:
                edit.extend(["-change",link,"@loader_path/"+dependency.name])
        original_rpaths=load_rpaths(source)
        for search in dict.fromkeys(original_rpaths):
            edit.extend(['-delete_rpath',search])
        subprocess.run([*edit,str(copied)],check=True)
        # Apple Silicon requires valid code signatures after modifying Mach-O.
        # This is local integrity signing with no owner identity or notarization.
        subprocess.run(["codesign", "--force", "--sign", "-", "--timestamp=none", str(copied)], check=True)
        subprocess.run(["codesign", "--verify", "--strict", str(copied)], check=True)
        for link in dependencies(copied):
            if not link.startswith(("/usr/lib/", "/System/Library/", "@loader_path/")):
                raise ValueError(f"Relocated library retains a host dependency: {link}")
        if load_rpaths(copied):
            raise ValueError('Relocated library retains an undeclared native runpath: '+str(copied))
        provenance.append({"originalPath": str(source), "originalSha256": sha(source),
                           "packagedFile": copied.name, "packagedSha256": sha(copied),
                           "macOSDeploymentMinimum": ".".join(map(str,deployment_minimum(copied))),
                           "originalInstallIds":command_names(source,{'LC_ID_DYLIB'}),
                           "packagedInstallIds":command_names(copied,{'LC_ID_DYLIB'}),
                           "originalRunpaths":original_rpaths,"packagedRunpaths":load_rpaths(copied),
                           "resolvedDependencies":[{"loadCommand":link,"path":str(resolve_dependency(link,source))}
                                                   for link in links if resolve_dependency(link,source) is not None],
                           "originalDependencies": links, "packagedDependencies": dependencies(copied)})
    return provenance


def signing_layout_probe(source,architecture):
    """Retain actual Apple-tool before/after evidence for the observed gap."""
    output=ROOT/'artifacts/macos'/('relocation-'+architecture)
    output.mkdir(parents=True,exist_ok=True)
    report={'schemaVersion':1,'scope':'native-copied-Mach-O-signature-layout-diagnostic',
            'source':str(source),'sourceSha256':sha(source),'cases':[],
            'tools':run('xcodebuild','-version'),'editor':run('xcrun','--find','install_name_tool')}
    with tempfile.TemporaryDirectory(prefix='computator-relocation-') as temporary:
        for stripped in (False,True):
            copied=Path(temporary)/('stripped.dylib' if stripped else 'intact.dylib')
            shutil.copy2(source,copied);copied.chmod(0o755)
            case={'strippedBeforeEditing':stripped,'originalHeader':run('otool','-l',str(copied)),
                  'originalFileSize':copied.stat().st_size}
            if stripped:
                remove=subprocess.run(['codesign','--remove-signature',str(copied)],text=True,capture_output=True)
                case['removeSignature']={'exitCode':remove.returncode,'stdout':remove.stdout,'stderr':remove.stderr}
            header=subprocess.run(['otool','-l',str(copied)],text=True,capture_output=True)
            case['preparedFileSize']=copied.stat().st_size
            case['preparedHeader']={'exitCode':header.returncode,'stdout':header.stdout,'stderr':header.stderr}
            edit=subprocess.run(['install_name_tool','-id','@loader_path/'+source.name,str(copied)],text=True,capture_output=True)
            case['edit']={'exitCode':edit.returncode,'stdout':edit.stdout,'stderr':edit.stderr}
            case['editedFileSize']=copied.stat().st_size
            if edit.returncode==0:
                sign=subprocess.run(['codesign','--force','--sign','-','--timestamp=none',str(copied)],text=True,capture_output=True)
                verify=subprocess.run(['codesign','--verify','--strict',str(copied)],text=True,capture_output=True)
                case['replaceAndVerify']={'signExitCode':sign.returncode,'verifyExitCode':verify.returncode,
                                          'signError':sign.stderr,'verifyError':verify.stderr}
            report['cases'].append(case)
    report_path=output/(datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%S.%fZ')+'.json')
    report_path.write_text(json.dumps(report,indent=2)+'\n')
    first=report['cases'][0]
    if (first['edit']['exitCode']!=0 or first.get('replaceAndVerify',{}).get('signExitCode')!=0
            or first.get('replaceAndVerify',{}).get('verifyExitCode')!=0):
        raise ValueError('Intact signed Mach-O edit/re-sign failed; inspect the retained native tool diagnostic '+str(report_path))
    print('Native intact Mach-O relocation/signature diagnostic passed:',report_path)


def retain_sources(provenance, prepared, resources):
    """Retain corresponding pinned sources and installed build recipes, not just URLs."""
    target = resources / "share/native-sources"
    target.mkdir()
    packages = {}
    frozen=json.loads((prepared/'logs/homebrew-installed.json').read_text())
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
        receipt = keg / "INSTALL_RECEIPT.json"
        diagnostics=ROOT/'artifacts/macos'/('source-recipes-'+platform.machine())
        diagnostics.mkdir(parents=True,exist_ok=True)
        (diagnostics/(formula+'.json')).write_text(json.dumps({'schemaVersion':1,'installedKeg':str(keg),
            'recipePath':str(recipe),'recipeSha256':sha(recipe),'recipeText':content,
            'receipt':json.loads(receipt.read_text()) if receipt.is_file() else None},indent=2)+'\n')
        # Installed recipes, unlike today's rolling formula API, describe the
        # binaries actually copied. Reject unpinned/interpolated source recipes.
        url,expected=source_stanza(content,str(recipe))
        directory = target / formula
        directory.mkdir()
        shutil.copy2(recipe, directory / recipe.name)
        if receipt.is_file():
            shutil.copy2(receipt, directory / receipt.name)
        filename = Path(url.split("?", 1)[0]).name
        archive = directory / filename
        download_verified(url,expected,archive)
        patches=[]
        for patch_index,(patch_url,patch_sha) in enumerate(external_patches(content,str(recipe))):
            patch_name=Path(patch_url.split('?',1)[0]).name or 'source.patch'
            patch_file=directory/('patch-'+str(patch_index)+'-'+patch_name)
            download_verified(patch_url,patch_sha,patch_file)
            patches.append({'url':patch_url,'sha256':patch_sha,'file':patch_file.relative_to(resources).as_posix(),
                            'scope':'literal patch input retained; applicability remains in the installed recipe'})
        frozen_recipe=None;patch_proofs={};patch_files={};formula_proof=None
        if re.search(r'^\s*file\s+"Patches/',ruby_code(content),re.M):
            matching=[entry for entry in frozen.get('formulae',[]) if entry.get('name')==formula]
            if len(matching)!=1: raise ValueError('No frozen local-patch formula metadata: '+formula)
            metadata=matching[0]
            receipt_data=json.loads(receipt.read_text()) if receipt.is_file() else {}
            head=receipt_data.get('source',{}).get('tap_git_head','');source_path=metadata.get('ruby_source_path','')
            formula_hash=metadata.get('ruby_source_checksum',{}).get('sha256','')
            if (not re.fullmatch('[0-9a-f]{40}',head) or not re.fullmatch('[0-9a-f]{64}',formula_hash)
                    or not source_path.startswith('Formula/') or not source_path.endswith('.rb')
                    or '\\' in source_path or ':' in source_path or any(part in ('','.','..') for part in source_path.split('/'))):
                raise ValueError('Unsafe frozen formula reference for '+formula)
            frozen_file=directory/('frozen-upstream-'+formula+'.rb')
            formula_proof=pinned_git_blob(head,source_path,frozen_file,formula_hash)
            frozen_recipe=frozen_file.read_text()
            for path in re.findall(r'^\s*file\s+"(Patches/[^"#]+)"\s*$',ruby_code(content),re.M):
                patch_file=directory/('local-'+Path(path).name)
                declared=(metadata.get('local_patch_checksums') or {}).get(path)
                patch_proofs[path]=pinned_git_blob(head,path,patch_file,declared)
                patch_files[path]=patch_file
        for patch in local_patch_pins(content,formula,parts[index+2],sha(recipe),frozen,frozen_recipe,
                                      json.loads(receipt.read_text()) if receipt.is_file() else None,patch_proofs):
            patch_file=patch_files[patch['path']]
            patches.append({**patch,'file':patch_file.relative_to(resources).as_posix()})
        resources_retained=[]
        for name,resource_url,resource_sha,scope in recipe_resources(content,str(recipe)):
            if scope in ('test-only','linux-only'):
                resources_retained.append({'name':name,'url':resource_url,'sha256':resource_sha,'scope':scope,
                    'excludedFromMacBuildInputs':True,'reason':'Not an input to the shipped macOS libraries; declaration preserved in recipe.'})
                continue
            resource_file=directory/('resource-'+name+'-'+Path(resource_url.split('?',1)[0]).name)
            download_verified(resource_url,resource_sha,resource_file)
            resources_retained.append({'name':name,'url':resource_url,'sha256':resource_sha,'scope':scope,
                                       'file':resource_file.relative_to(resources).as_posix()})
        licenses = []
        for path in keg.rglob("*"):
            if path.is_file() and not path.is_symlink() and re.match(r"^(COPYING|LICENSE|NOTICE|COPYRIGHT)(\.|$)", path.name, re.I):
                destination = directory / "licenses" / path.relative_to(keg)
                destination.parent.mkdir(parents=True, exist_ok=True)
                shutil.copy2(path, destination)
                licenses.append(destination.relative_to(resources).as_posix())
        packages[formula] = {"installedVersion": parts[index + 2], "url": url, "sha256": expected,
                             "sourceArchive": archive.relative_to(resources).as_posix(), "licenses": licenses,
                             "recipeSha256":sha(recipe),"frozenFormulaProof":formula_proof,
                             "externalPatches":patches,"resources":resources_retained}
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


def source_stanza(content,label):
    lines=content.splitlines()
    first=next((index for index,line in enumerate(lines) if re.match(r'^\s*url\b',line)),None)
    if first is None:
        raise ValueError('No literal pinned source archive in installed recipe '+label)
    # Inspect the FIRST source stanza only. Never silently substitute a later
    # test resource or livecheck URL when the actual main source is unreviewed.
    match=re.fullmatch(r'\s*url\s+"(https://[^"#]+)"(?:,\s*using:\s*:[a-z_]+)?\s*(?:#.*)?',lines[first])
    if not match:
        raise ValueError('Unreviewed source URL form in installed recipe '+label)
    url=match[1]
    for line in lines[first+1:first+33]:
        stripped=line.strip()
        if not stripped or stripped.startswith('#'):
            continue
        checksum=re.fullmatch(r'sha256\s+"([0-9a-f]{64})"\s*(?:#.*)?',stripped)
        if checksum:
            return url,checksum[1]
        if re.fullmatch(r'mirror\s+"(?:https?|ftp)://[^"#]+"\s*(?:#.*)?',stripped):
            continue
        if re.fullmatch(r'version\s+"[A-Za-z0-9._+-]+"\s*(?:#.*)?',stripped):
            continue
        break
    raise ValueError('No adjacent literal source SHA256 in installed recipe '+label)


def external_patches(content,label):
    lines=ruby_code(content).splitlines();patches=[]
    for index,line in enumerate(lines):
        if not re.match(r'^\s*patch\b',line):
            continue
        if re.fullmatch(r'\s*patch(?:\s+:[A-Za-z0-9_]+,)?\s+:DATA\s*(?:#.*)?',line):
            if not re.search(r'^__END__\s*$',content,re.M):
                raise ValueError('Inline recipe patch data is absent: '+label)
            continue
        start=re.fullmatch(r'(\s*)patch(?:\s+:[A-Za-z0-9_]+)?\s+do\s*(?:#.*)?',line)
        if not start:
            raise ValueError('Unreviewed external patch form in installed recipe '+label)
        end=next((number for number in range(index+1,min(len(lines),index+65))
                  if lines[number].strip()=='end' and lines[number].startswith(start[1])
                  and len(lines[number])-len(lines[number].lstrip())==len(start[1])),None)
        if end is None:
            raise ValueError('Unreviewed external patch block in installed recipe '+label)
        body='\n'.join(lines[index+1:end])
        if re.fullmatch(r'\s*file\s+"Patches/[^"#]+"\s*',body):
            continue  # Bound to the frozen tap metadata by local_patch_pins.
        patches.append(source_stanza(body,label+' patch'))
    return patches


def local_patch_pins(content,formula,version,recipe_sha,frozen,frozen_recipe=None,receipt=None,patch_proofs=None):
    code=ruby_code(content)
    declarations=re.findall(r'^\s*file\s+"(Patches/[^"#]+)"\s*$',code,re.M)
    if not declarations:
        return []
    matches=[entry for entry in frozen.get('formulae',[]) if entry.get('name')==formula]
    if len(matches)!=1: raise ValueError('No exact frozen installed formula metadata: '+formula)
    metadata=matches[0]
    expected_version=metadata['versions']['stable']+('_'+str(metadata['revision']) if metadata.get('revision') else '')
    info_head=metadata.get('tap_git_head','')
    original_sha=metadata.get('ruby_source_checksum',{}).get('sha256')
    source=(receipt or {}).get('source',{})
    head=source.get('tap_git_head','')
    if (metadata.get('tap')!='homebrew/core' or expected_version!=version
            or not re.fullmatch('[0-9a-f]{40}',head) or not re.fullmatch('[0-9a-f]{40}',info_head)
            or source.get('tap')!='homebrew/core'
            or source.get('versions',{}).get('stable')!=metadata['versions']['stable']):
        raise ValueError('Frozen local patch metadata does not match the actual installed recipe/version: '+formula)
    if (not re.fullmatch('[0-9a-f]{64}',original_sha or '')
            or hashlib.sha256(content.encode()).hexdigest()!=recipe_sha):
        raise ValueError('Installed recipe bytes differ from their recorded SHA256: '+formula)
    if original_sha!=recipe_sha:
        if (frozen_recipe is None or hashlib.sha256(frozen_recipe.encode()).hexdigest()!=original_sha
                or content not in staged_formula_variants(frozen_recipe)):
            raise ValueError('Frozen formula differs from installed build instructions after the exact reviewed staging transformations: '+formula)
    result=[]
    for path in declarations:
        if (not path.startswith('Patches/') or '\\' in path or ':' in path or
                any(part in ('','.','..') for part in path.split('/'))):
            raise ValueError('Unsafe local recipe patch path')
        entries=[item for item in metadata.get('patches',[]) if item.get('file')==path]
        declared=(metadata.get('local_patch_checksums') or {}).get(path)
        proof=(patch_proofs or {}).get(path,{})
        digest=proof.get('sha256','')
        if (len(entries)!=1 or entries[0].get('strip') not in ('p0','p1','p2')
                or proof.get('commit')!=head or proof.get('path')!=path
                or not re.fullmatch('[0-9a-f]{40}',proof.get('gitBlobSha1',''))
                or not re.fullmatch('[0-9a-f]{64}',digest)
                or (declared is not None and digest!=declared)):
            raise ValueError('No exact pinned local patch checksum/strip level: '+path)
        result.append({'path':path,'tapCommit':head,'sha256':digest,'strip':entries[0]['strip'],
                       'url':'https://raw.githubusercontent.com/Homebrew/homebrew-core/'+head+'/'+path,
                       'scope':'local patch from the recipe-bound immutable receipt tap commit',
                       'infoTapCommit':info_head,'gitBlobSha1':proof['gitBlobSha1'],
                       'sha256Basis':'declared checksum plus verified receipt Git blob' if declared else 'computed from verified receipt Git blob; older API has no declared SHA256'})
    if set(declarations)!={item['file'] for item in metadata.get('patches',[]) if 'file' in item}:
        raise ValueError('Frozen local patch set differs from the installed recipe: '+formula)
    return result


def remove_literal_bottle(content):
    lines=content.splitlines(keepends=True)
    limit=next((number for number,line in enumerate(lines) if line.rstrip('\r\n')=='__END__'),len(lines))
    starts=[number for number in range(limit) if lines[number].rstrip('\r\n')=='  bottle do']
    if not starts: return content
    if len(starts)!=1: raise ValueError('Unreviewed number of top-level bottle metadata blocks')
    start=starts[0]
    end=next((number for number in range(start+1,limit) if lines[number].rstrip('\r\n')=='  end'),None)
    if end is None: raise ValueError('Unterminated literal bottle metadata block')
    for line in lines[start+1:end]:
        text=line.strip()
        if not text or text.startswith('#') or re.fullmatch(r'rebuild \d+',text): continue
        if re.fullmatch(r'sha256(?:\s+cellar:\s*:[a-z_]+,)?\s+[a-z0-9_]+:\s*"[0-9a-f]{64}"',text): continue
        raise ValueError('Unreviewed non-literal statement inside bottle metadata')
    after=end+1
    if after<len(lines) and lines[after] in ('\n','\r\n'): after+=1
    # Homebrew stages formulae without this metadata block and its one following
    # separator. Every other byte (including heredocs and :DATA) must match.
    # See Homebrew/brew Library/Homebrew/formula_installer.rb's staging gsub;
    # our accepted body is deliberately limited to literal bottle metadata.
    return ''.join(lines[:start]+lines[after:])


REVIEWED_STAGED_RECIPE_SHA256={
    # The complete immutable GLib 2.88.3 and 2.90.1 recipes inspected alongside
    # actual installed recipes/receipts on the Intel and ARM macOS runners.
    '658003a495be4141e3f5856ff34a1334c58fe7a0fc745afbc84258394472a6db',
    'e6c6094ef287cd9dd51af16a4e69cb899b732cdf92da900eb9371339099df005',
}


def staged_formula_variants(pinned_content):
    content=remove_literal_bottle(pinned_content)
    variants={content}
    if hashlib.sha256(pinned_content.encode()).hexdigest() not in REVIEWED_STAGED_RECIPE_SHA256:
        return variants
    old='    mkdir_p "{{HOMEBREW_PREFIX}}/lib/gio/modules"\n'
    new='    mkdir_p "lib/gio/modules", base: :homebrew_prefix\n'
    # Brew's install_steps DSL resolves both literal forms to the same prefix.
    # Accept this exact observed non-build declaration only in the two complete
    # reviewed upstream recipes. Never normalize arbitrary installed content,
    # build statements, whitespace, heredocs or DATA. Homebrew install_steps.rb
    # resolves both forms to HOMEBREW_PREFIX/lib/gio/modules.
    block='  post_install_steps do\n'+old+'  end\n'
    if block in ruby_code(content):
        if content.count(block)!=1: raise ValueError('Duplicate reviewed post-install declaration')
        variants.add(content.replace(block,'  post_install_steps do\n'+new+'  end\n',1))
    return variants


def recipe_resources(content,label):
    lines=ruby_code(content).splitlines();result=[]
    for index,line in enumerate(lines):
        if not re.match(r'^\s*resource\s+',line): continue
        start=re.fullmatch(r'(\s*)resource\s+"([A-Za-z0-9._+-]+)"(?:,\s*:(test))?\s+do\s*(?:#.*)?',line)
        if not start: raise ValueError('Unreviewed resource declaration in '+label)
        end=next((number for number in range(index+1,min(len(lines),index+257))
                  if lines[number].strip()=='end' and len(lines[number])-len(lines[number].lstrip())==len(start[1])),None)
        if end is None: raise ValueError('Unreviewed resource block in '+label)
        url,digest=source_stanza('\n'.join(lines[index+1:end]),label+' resource '+start[2])
        in_test=start[3]=='test'
        linux_only=False
        for before in range(index):
            test=re.fullmatch(r'(\s*)test\s+do\s*',lines[before])
            if test and len(test[1])<len(start[1]):
                closing=next((number for number in range(before+1,len(lines))
                              if lines[number].strip()=='end' and len(lines[number])-len(lines[number].lstrip())==len(test[1])),None)
                if closing is not None and closing>index: in_test=True
            linux=re.fullmatch(r'(\s*)on_linux\s+do\s*',lines[before])
            if linux and len(linux[1])<len(start[1]):
                closing=next((number for number in range(before+1,len(lines))
                              if lines[number].strip()=='end' and len(lines[number])-len(lines[number].lstrip())==len(linux[1])),None)
                if closing is not None and closing>index: linux_only=True
        result.append((start[2],url,digest,'test-only' if in_test else 'linux-only' if linux_only else 'declared-outside-test; conditional/build use preserved in recipe'))
    return result


def ruby_code(content):
    return re.split(r'^__END__\s*$',content,maxsplit=1,flags=re.M)[0]


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
        archive_sha=archive_bundle(bundle,args.archive)
        args.archive.with_suffix('.zip.json').write_text(json.dumps({'schemaVersion':1,'rid':rid,
            'source':metadata['source'],'archive':{'name':args.archive.name,'sha256':archive_sha,'size':args.archive.stat().st_size},
            'scope':'unsigned-native-mono-candidate','packagedRuntimeAccepted':False,
            'sourceCheckoutAccepted':bool(accepted_source),'ownerSigned':False,'notarized':False},indent=2)+'\n')
        print("Archive SHA256:",archive_sha)
    print("Prepared unsigned runtime-dependent Mono app:", bundle)
    print("Complete current-head source application acceptance:",bool(accepted_source))


if __name__ == "__main__":
    try:
        main()
    except (OSError, ValueError, KeyError, subprocess.CalledProcessError) as error:
        print("Native Mac packaging failed:", error, file=sys.stderr)
        sys.exit(1)
