"""Portable bundle-integrity/archive contracts; these do not certify a Mac GUI."""
import importlib.util
import json
import os
from pathlib import Path
import stat
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
        self.base=Path(temporary.name)
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


if __name__=='__main__':
    unittest.main()
