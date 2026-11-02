"""Portable bundle-integrity/archive contracts; these do not certify a Mac GUI."""
import importlib.util
import base64
import hashlib
import io
import json
import os
from pathlib import Path
import stat
import subprocess
import sys
import tempfile
import unittest
from unittest import mock
import zipfile


def module(name,path):
    spec=importlib.util.spec_from_file_location(name,path)
    result=importlib.util.module_from_spec(spec)
    spec.loader.exec_module(result)
    return result


mac=module('mac_package',Path(__file__).with_name('package.py'))
common=module('common_package',Path(__file__).parents[1]/'mono/package.py')


class MacPackageContracts(unittest.TestCase):
    def setUp(self):
        temporary=tempfile.TemporaryDirectory(prefix='computator-mac-contracts-')
        self.addCleanup(temporary.cleanup)
        # macOS /var is an alias of /private/var. The production closure queue
        # canonicalizes each owner before resolving same-keg dependencies; the
        # fixture and its mocked otool identities must use that same real root.
        self.base=Path(temporary.name).resolve(strict=True)
        self.bundle=self.base/'Computator.NET.app'
        self.resources=self.bundle/'Contents/Resources'
        for name in common.REQUIRED_APP:
            path=self.resources/'app'/name
            path.parent.mkdir(parents=True,exist_ok=True)
            path.write_bytes(b'managed payload fixture')
        (self.resources/'app/Computator.NET.exe.config').write_text('<configuration><startup><supportedRuntime sku=".NETFramework,Version=v4.8"/></startup></configuration>')
        for name in ('TSL Examples/example.tsl','Static/reference-equations/probe.png','Static/reference-figures/probe.png'):
            path=self.resources/'app'/name
            path.parent.mkdir(parents=True,exist_ok=True)
            path.write_bytes(b'asset')
        self.bootstrap=('Contents/Info.plist','Contents/MacOS/Computator.NET')
        for name in self.bootstrap:
            path=self.bundle/name
            path.parent.mkdir(parents=True,exist_ok=True)
            path.write_bytes(b'bootstrap fixture')
            path.chmod(0o755 if 'MacOS' in name else 0o644)
        self.metadata={'version':'3.0.0-preview.1','native':{},'bundleBootstrap':[
            {'path':name,'sha256':mac.sha(self.bundle/name),'size':(self.bundle/name).stat().st_size} for name in self.bootstrap]}
        self.write_manifest()

    def write_manifest(self):
        common.write_manifest(self.resources,self.metadata)

    def test_whole_bundle_roundtrip_keeps_hashes_and_executable_launcher(self):
        mac.verify_bundle(self.bundle)
        first,second=self.base/'first.zip',self.base/'second.zip'
        self.assertEqual(mac.archive_bundle(self.bundle,first),mac.archive_bundle(self.bundle,second))
        extracted=mac.extract_bundle(first,self.base/'relocated folder with spaces')
        self.assertEqual(common.inventory(self.resources),common.inventory(extracted/'Contents/Resources'))
        self.assertTrue(os.access(extracted/'Contents/MacOS/Computator.NET',os.X_OK))

    def test_changed_launcher_is_detected_outside_resources_inventory(self):
        launcher=self.bundle/'Contents/MacOS/Computator.NET'
        launcher.write_bytes(b'X'*launcher.stat().st_size)
        with self.assertRaisesRegex(ValueError,'bootstrap file differs'):
            mac.verify_bundle(self.bundle)

    def test_changed_plist_is_detected(self):
        (self.bundle/'Contents/Info.plist').write_bytes(b'changed')
        with self.assertRaisesRegex(ValueError,'bootstrap file differs'):
            mac.verify_bundle(self.bundle)

    def test_bootstrap_metadata_cannot_traverse_outside_bundle(self):
        self.metadata['bundleBootstrap'][0]['path']='../../outside'
        self.write_manifest()
        with self.assertRaisesRegex(ValueError,'exact two fixed'):
            mac.verify_bundle(self.bundle)

    def test_duplicate_bootstrap_entry_is_rejected(self):
        self.metadata['bundleBootstrap'][1]=self.metadata['bundleBootstrap'][0]
        self.write_manifest()
        with self.assertRaisesRegex(ValueError,'exact two fixed'):
            mac.verify_bundle(self.bundle)

    def test_unrecorded_outer_payload_is_rejected(self):
        (self.bundle/'Contents/MacOS/injected').write_bytes(b'injected')
        with self.assertRaisesRegex(ValueError,'Unrecorded bundle'):
            mac.verify_bundle(self.bundle)

    def malicious(self,names,mode=stat.S_IFREG|0o644):
        archive=self.base/'malicious.zip'
        with zipfile.ZipFile(archive,'w') as output:
            for name in names:
                item=zipfile.ZipInfo(name)
                item.create_system=3
                item.external_attr=mode<<16
                output.writestr(item,b'payload')
        return archive

    def reject_archive(self,names,mode=stat.S_IFREG|0o644):
        output=self.base/'must not exist'
        with self.assertRaises(ValueError):
            mac.extract_bundle(self.malicious(names,mode),output)
        self.assertFalse(output.exists(),'Every member must be checked before extraction starts.')

    def test_archive_traversal_is_rejected_before_writing_anything(self):
        self.reject_archive(['Computator.NET.app/Contents/safe','../outside'])

    def test_archive_symlink_is_rejected_before_writing(self):
        self.reject_archive(['Computator.NET.app/Contents/alias'],stat.S_IFLNK|0o777)

    def test_archive_case_aliases_are_rejected_on_all_filesystems(self):
        self.reject_archive(['Computator.NET.app/Contents/thing','Computator.NET.app/Contents/THING'])

    def test_archive_must_have_one_app_root(self):
        self.reject_archive(['Computator.NET.app/Contents/safe','Other.app/Contents/injected'])

    def test_existing_output_is_preserved(self):
        archive=self.base/'valid.zip'
        mac.archive_bundle(self.bundle,archive)
        with self.assertRaisesRegex(ValueError,'new output'):
            mac.extract_bundle(archive,self.bundle)
        self.assertEqual(b'bootstrap fixture',(self.bundle/'Contents/MacOS/Computator.NET').read_bytes())

    def test_self_install_id_is_not_traversed_but_real_load_commands_are(self):
        commands='''/private/build/libprobe.dylib:
Load command 0
          cmd LC_ID_DYLIB
      cmdsize 64
         name @rpath/libprobe.1.dylib (offset 24)
Load command 1
          cmd LC_LOAD_DYLIB
      cmdsize 64
         name @rpath/libdependency.2.dylib (offset 24)
Load command 2
          cmd LC_LOAD_WEAK_DYLIB
      cmdsize 64
         name /usr/lib/libSystem.B.dylib (offset 24)
'''
        with mock.patch.object(mac,'run',return_value=commands):
            self.assertEqual(['@rpath/libdependency.2.dylib','/usr/lib/libSystem.B.dylib'],mac.dependencies(Path('/private/build/libprobe.dylib')))
            self.assertEqual(['@rpath/libprobe.1.dylib'],mac.command_names(Path('/private/build/libprobe.dylib'),{'LC_ID_DYLIB'}))

    def test_same_keg_fallback_requires_matching_real_install_identity(self):
        directory=self.base/'Cellar/brotli/1.2.1/lib'
        directory.mkdir(parents=True)
        owner=directory/'libbrotlidec.1.2.1.dylib';owner.write_bytes(b'owner')
        sibling=directory/'libbrotlicommon.1.dylib';sibling.write_bytes(b'sibling')
        def commands(*args):
            if str(sibling)==args[-1]:
                return 'library:\nLoad command 0\n cmd LC_ID_DYLIB\n name @rpath/libbrotlicommon.1.dylib (offset 24)\n'
            return 'library:\nLoad command 0\n cmd LC_SEGMENT_64\n'
        with mock.patch.object(mac,'run',side_effect=commands):
            self.assertEqual(sibling.resolve(),mac.resolve_dependency('@rpath/libbrotlicommon.1.dylib',owner))
        with mock.patch.object(mac,'run',return_value='library:\nLoad command 0\n cmd LC_ID_DYLIB\n name @rpath/WRONG-ABI.dylib (offset 24)\n'):
            with self.assertRaisesRegex(ValueError,'one explicit installed ABI'):
                mac.resolve_dependency('@rpath/libbrotlicommon.1.dylib',owner)

    def test_runpath_ambiguity_does_not_guess_a_host_library(self):
        first=self.base/'first';second=self.base/'second'
        for directory in (first,second):
            directory.mkdir();(directory/'libprobe.1.dylib').write_bytes(b'different installed file')
        commands=f'library:\nLoad command 0\n cmd LC_RPATH\n path {first} (offset 12)\nLoad command 1\n cmd LC_RPATH\n path {second} (offset 12)\n'
        with mock.patch.object(mac,'run',return_value=commands):
            with self.assertRaisesRegex(ValueError,'one explicit installed ABI'):
                mac.resolve_dependency('@rpath/libprobe.1.dylib',self.base/'owner.dylib')

    def test_pinned_stanza_accepts_multiple_mirrors_without_downloading_them(self):
        recipe='''class Brotli < Formula
  url "https://example.invalid/brotli-1.2.1.tar.gz"
  mirror "http://example.invalid/first.tar.gz"
  mirror "https://example.invalid/second.tar.gz"
  sha256 "'''+('a'*64)+'''"
end
'''
        self.assertEqual(('https://example.invalid/brotli-1.2.1.tar.gz','a'*64),mac.source_stanza(recipe,'brotli'))

    def test_pinned_stanza_accepts_literal_version_comments_and_transport(self):
        recipe='url "https://example.invalid/source.tar.xz", using: :homebrew_curl\n# fixed upstream version\nversion "1.0"\n\nsha256 "'+('b'*64)+'"\n'
        self.assertEqual(('https://example.invalid/source.tar.xz','b'*64),mac.source_stanza(recipe,'source'))

    def test_unreviewed_main_git_source_cannot_be_replaced_by_a_test_resource(self):
        recipe='url "https://example.invalid/project.git", tag: "v1", revision: "123"\nresource "test" do\n url "https://example.invalid/test.tar.gz"\n sha256 "'+('c'*64)+'"\nend\n'
        with self.assertRaisesRegex(ValueError,'Unreviewed source URL'):
            mac.source_stanza(recipe,'source')

    def test_source_pin_does_not_cross_an_unreviewed_ruby_statement(self):
        recipe='url "https://example.invalid/source.tar.gz"\non_macos do\nsha256 "'+('d'*64)+'"\nend\n'
        with self.assertRaisesRegex(ValueError,'adjacent literal source SHA256'):
            mac.source_stanza(recipe,'source')

    def test_external_patch_archives_have_independent_pins_and_inline_data_is_preserved(self):
        recipe='''patch do
  url "https://example.invalid/fixed.patch"
  sha256 "'''+('e'*64)+'''"
end
patch :p0, :DATA
__END__
diff --git a/input.c b/input.c
'''
        self.assertEqual([('https://example.invalid/fixed.patch','e'*64)],mac.external_patches(recipe,'patched'))

    def test_external_patch_without_reviewed_literal_hash_is_rejected(self):
        recipe='patch do\n url "https://example.invalid/moving.patch"\nend\n'
        with self.assertRaisesRegex(ValueError,'adjacent literal source SHA256'):
            mac.external_patches(recipe,'patched')

    def test_data_patch_context_is_not_evaluated_as_ruby_patch_declarations(self):
        recipe='patch :DATA\n__END__\n patch do\n garbage source context\n end\n'
        self.assertEqual([],mac.external_patches(recipe,'inline'))

    def test_glib_local_patch_is_bound_to_the_exact_recipe_version_and_tap_commit(self):
        recipe='patch do\n file "Patches/glib/hardcoded-paths.diff"\nend\n'
        digest=hashlib.sha256(recipe.encode()).hexdigest()
        patch_path='Patches/glib/hardcoded-paths.diff'
        self.assertEqual([],mac.external_patches(recipe,'glib'))
        frozen={'formulae':[{'name':'glib','tap':'homebrew/core','versions':{'stable':'2.90.1'},'revision':0,
            'ruby_source_checksum':{'sha256':digest},'tap_git_head':'b'*40,
            'patches':[{'strip':'p1','file':patch_path}],
            'local_patch_checksums':{patch_path:'c'*64}}]}
        receipt={'source':{'tap':'homebrew/core','tap_git_head':'b'*40,'versions':{'stable':'2.90.1'}}}
        proof={patch_path:{'commit':'b'*40,'path':patch_path,'gitBlobSha1':'d'*40,'sha256':'c'*64}}
        pins=mac.local_patch_pins(recipe,'glib','2.90.1',digest,frozen,receipt=receipt,patch_proofs=proof)
        self.assertEqual('c'*64,pins[0]['sha256'])
        self.assertIn('/'+('b'*40)+'/Patches/glib/',pins[0]['url'])
        for version,changed_digest in (('2.90.2',digest),('2.90.1','d'*64)):
            with self.assertRaisesRegex(ValueError,'actual installed recipe/version|recorded SHA256'):
                mac.local_patch_pins(recipe,'glib',version,changed_digest,frozen,receipt=receipt,patch_proofs=proof)
        # The receipt can be older than the globally installed tap snapshot;
        # only the exact verified receipt tree supplies the actual patch bytes.
        receipt['source']['tap_git_head']='e'*40
        proof[patch_path]['commit']='e'*40
        del frozen['formulae'][0]['local_patch_checksums']
        pins=mac.local_patch_pins(recipe,'glib','2.90.1',digest,frozen,receipt=receipt,patch_proofs=proof)
        self.assertEqual('e'*40,pins[0]['tapCommit'])
        self.assertIn('older API has no declared SHA256',pins[0]['sha256Basis'])
        proof[patch_path]['commit']='b'*40
        with self.assertRaisesRegex(ValueError,'exact pinned local patch'):
            mac.local_patch_pins(recipe,'glib','2.90.1',digest,frozen,receipt=receipt,patch_proofs=proof)

    def test_pinned_git_file_checks_immutable_tree_blob_and_optional_declared_checksum(self):
        data=b'actual source bytes\n'
        blob=hashlib.sha1(b'blob '+str(len(data)).encode()+b'\0'+data).hexdigest()
        digest=hashlib.sha256(data).hexdigest()
        item={'type':'file','path':'Patches/glib/fix.diff','encoding':'base64','size':len(data),
              'content':base64.b64encode(data).decode(),'sha':blob}
        for name,changed,expected in (
                ('valid',{},digest),('older-schema',{},None),
                ('wrong-blob',{'sha':'f'*40},digest),('wrong-path',{'path':'Patches/wrong'},digest),
                ('wrong-size',{'size':len(data)+1},digest),('wrong-sha256',{},'0'*64)):
            target=self.base/(name+'.source')
            with self.subTest(name=name),mock.patch.object(mac.urllib.request,'urlopen',
                    return_value=io.BytesIO(json.dumps({**item,**changed}).encode())) as opened:
                if name in ('valid','older-schema'):
                    proof=mac.pinned_git_blob('a'*40,item['path'],target,expected)
                    self.assertEqual(data,target.read_bytes())
                    self.assertEqual(blob,proof['gitBlobSha1'])
                    self.assertEqual(digest,proof['sha256'])
                    self.assertIn('?ref='+('a'*40),opened.call_args[0][0].full_url)
                else:
                    with self.assertRaisesRegex(ValueError,'exact requested file|blob identity'):
                        mac.pinned_git_blob('a'*40,item['path'],target,expected)
                    self.assertFalse(target.exists())

    def test_post_install_equivalence_requires_the_complete_reviewed_raw_recipe(self):
        old='  post_install_steps do\n    mkdir_p "{{HOMEBREW_PREFIX}}/lib/gio/modules"\n  end\n'
        new='  post_install_steps do\n    mkdir_p "lib/gio/modules", base: :homebrew_prefix\n  end\n'
        raw='class Probe < Formula\n'+old+'  def install\n    system "make"\n  end\nend\n'
        self.assertEqual({raw},mac.staged_formula_variants(raw))
        with mock.patch.object(mac,'REVIEWED_STAGED_RECIPE_SHA256',{hashlib.sha256(raw.encode()).hexdigest()}):
            self.assertEqual({raw,raw.replace(old,new)},mac.staged_formula_variants(raw))
            changed=raw.replace('system "make"','system "changed-build"')
            self.assertEqual({changed},mac.staged_formula_variants(changed))
            # Even the exact declaration in arbitrary heredoc/DATA content
            # cannot be rewritten without the complete reviewed raw hash.
            heredoc=raw+'__END__\n'+old
            self.assertEqual({heredoc},mac.staged_formula_variants(heredoc))

    def test_only_literal_bottle_metadata_and_its_one_separator_may_be_removed(self):
        content='class Probe < Formula\n  url "https://example.invalid/source.tar.gz"\n\n  bottle do\n    rebuild 1\n    sha256 arm64_sequoia: "'+('a'*64)+'"\n  end\n\n  def install\n    system "make"\n  end\nend\n__END__\n\n patch context\n'
        expected='class Probe < Formula\n  url "https://example.invalid/source.tar.gz"\n\n  def install\n    system "make"\n  end\nend\n__END__\n\n patch context\n'
        self.assertEqual(expected,mac.remove_literal_bottle(content))
        with self.assertRaisesRegex(ValueError,'non-literal statement'):
            mac.remove_literal_bottle(content.replace('    rebuild 1','    system "unreviewed-build-command"'))

    def test_build_and_explicit_test_resources_keep_independent_archive_pins(self):
        recipe='resource "gobject-introspection" do\n url "https://example.invalid/build.tar.xz"\n sha256 "'+('a'*64)+'"\nend\nresource "test-font", :test do\n url "https://example.invalid/test.ttf"\n sha256 "'+('b'*64)+'"\nend\n'
        values=mac.recipe_resources(recipe,'resources')
        self.assertEqual(2,len(values))
        self.assertEqual('a'*64,values[0][2])
        self.assertEqual('test-only',values[1][3])

    def test_linux_conditional_test_resource_is_not_a_mac_build_input(self):
        recipe='on_linux do\n  resource "pngtest.png" do\n    url "https://example.invalid/pngtest.png"\n    sha256 "'+('a'*64)+'"\n  end\nend\n'
        self.assertEqual('linux-only',mac.recipe_resources(recipe,'libpng')[0][3])

    def test_retained_source_destinations_never_overwrite_an_earlier_pinned_input(self):
        target=self.base/'same-name.patch';target.write_bytes(b'first pinned input')
        with mock.patch.object(mac.urllib.request,'urlretrieve',side_effect=AssertionError('Must not download over an existing input')):
            with self.assertRaisesRegex(ValueError,'refusing to overwrite'):
                mac.download_verified('https://example.invalid/second.patch','a'*64,target)
        self.assertEqual(b'first pinned input',target.read_bytes())

    def test_native_trace_guard_requires_each_real_application_and_private_closure(self):
        self.metadata['source']={'commit':'a'*40,'tree':'b'*40}
        self.write_manifest()
        evidence=self.base/'runtime-evidence';(evidence/'normal-launch').mkdir(parents=True)
        mono=self.base/'mono-runtime';(mono/'bin').mkdir(parents=True);(mono/'bin/mono').write_bytes(b'native runtime fixture')
        (evidence/'launcher-preflight.json').write_text(json.dumps({'passed':True,'verifiedBootstrapFiles':2}))
        def trace(pid):
            return ''.join(f'dyld[{pid}]: {self.resources}/native/libfixture{number}.dylib\n' for number in range(10))
        harness=evidence/'harness';harness.mkdir()
        helper=harness/'libcomputator-mac-display.dylib'
        helper.write_bytes(b'external shared-test capture helper')
        for filename in ('startup.log','smoke.log'):
            (evidence/filename).write_text(trace(111)+f'dyld[111]: {helper}\n')
        for name,normal_trace,exit_code,expected in (
                ('valid',trace(777),0,True),('missing-normal-trace','',0,False),
                ('different-process',trace(888),0,False),('failed-normal-close',trace(777),1,False),
                ('host-X11',trace(777)+'dyld[777]: /opt/X11/lib/libX11.6.dylib\n',0,False),
                ('GUI-harness-leak',trace(777)+f'dyld[777]: {helper}\n',0,False),
                ('unrelated-PID-host-X11',trace(777)+'dyld[888]: /opt/X11/lib/libX11.6.dylib\n',0,True)):
            with self.subTest(name=name):
                (evidence/'normal-launch/launcher.stderr.log').write_text(normal_trace)
                (evidence/'normal-launch/normal-launch.json').write_text(json.dumps({'passed':True,'launcherExitCode':exit_code,
                    'source':self.metadata['source'],'applicationProcess':{'pid':777}}))
                result=subprocess.run([sys.executable,str(Path(__file__).with_name('verify-package-runtime.py')),
                    str(self.resources),str(evidence),str(mono)],capture_output=True,text=True)
                self.assertEqual(expected,result.returncode==0,result.stdout+result.stderr)
                if expected:
                    accepted=json.loads((evidence/'relocated-native-runtime.json').read_text())
                    self.assertEqual(10,accepted['applicationTraceCounts']['normal-launch/launcher.stderr.log'])
                    self.assertIn(str(helper),accepted['loadedImages'])
                    self.assertNotIn('/opt/X11/lib/libX11.6.dylib',accepted['loadedImages'])


if __name__=='__main__':
    unittest.main()
