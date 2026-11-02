"""Actual shipped preflight contracts on a complete native Mac .app copy."""
import argparse
import importlib.util
import json
from pathlib import Path
import platform
import sys
import unittest

spec=importlib.util.spec_from_file_location('shared_preflight_contracts',Path(__file__).parents[1]/'mono/test_preflight.py')
shared=importlib.util.module_from_spec(spec)
spec.loader.exec_module(shared)


class MacRuntimePreflightContracts(shared.RuntimePreflightContracts):
    def bootstrap_file(self,name):
        return self.package.parent.parent/name

    def test_changed_launcher_bootstrap_is_rejected(self):
        target=self.bootstrap_file('Contents/MacOS/Computator.NET')
        original=target.read_bytes()
        self.addCleanup(target.write_bytes,original)
        target.write_bytes(b'X'*len(original))
        self.reject(self.prepare(),'bootstrap file differs')

    def test_changed_plist_bootstrap_is_rejected(self):
        target=self.bootstrap_file('Contents/Info.plist')
        original=target.read_bytes()
        self.addCleanup(target.write_bytes,original)
        target.write_bytes(b'X'*len(original))
        self.reject(self.prepare(),'bootstrap file differs')

    def test_bootstrap_cannot_select_arbitrary_parent_file(self):
        self.alter_manifest(lambda value:value['bundleBootstrap'][0].update(path='../../outside'))
        self.reject(self.prepare(),'fixed Mac bootstrap inventory')

    def test_bootstrap_cannot_duplicate_a_fixed_path(self):
        self.alter_manifest(lambda value:value['bundleBootstrap'].__setitem__(1,value['bundleBootstrap'][0]))
        self.reject(self.prepare(),'fixed Mac bootstrap inventory')

    def test_bootstrap_inventory_is_required(self):
        self.alter_manifest(lambda value:value.pop('bundleBootstrap'))
        self.reject(self.prepare(),'exact fixed bootstrap inventory')

    def test_unrecorded_bootstrap_file_is_rejected(self):
        target=self.bootstrap_file('Contents/MacOS/injected')
        target.write_bytes(b'not inventoried')
        self.addCleanup(target.unlink)
        self.reject(self.prepare(),'Unrecorded Mac bundle bootstrap')

    def test_generated_config_cannot_modify_outer_bundle_directory(self):
        target=self.bootstrap_file('Contents/must-not-be-created.config')
        self.reject(self.prepare(target),'outside the read-only package')
        self.assertFalse(target.exists())

    def test_cache_symlink_cannot_redirect_into_outer_bundle(self):
        alias=self.output.parent/'redirected-outer-cache'
        alias.symlink_to(self.package.parent,target_is_directory=True)
        target=alias/'must-not-be-created.config'
        self.addCleanup(lambda:target.unlink() if target.exists() else None)
        self.reject(self.prepare(target),'outside the read-only package')
        self.assertFalse(target.exists())

    def test_case_alias_cannot_select_an_output_inside_outer_bundle(self):
        bundle=self.package.parent.parent
        alias=bundle.with_name(bundle.name.swapcase())
        target=alias/'Contents/must-not-be-created.config'
        self.reject(self.prepare(target),'outside the read-only package')
        self.assertFalse(target.exists())

    def test_bootstrap_parent_symlink_is_rejected(self):
        directory=self.bootstrap_file('Contents/MacOS')
        backup=directory.with_name('original-MacOS')
        directory.rename(backup)
        try:
            directory.symlink_to(backup,target_is_directory=True)
            self.reject(self.prepare(),'bootstrap paths must not traverse symbolic links')
        finally:
            if directory.is_symlink():
                directory.unlink()
            backup.rename(directory)


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--package-root',required=True,type=Path)
    parser.add_argument('--mono',default='mono')
    arguments,remaining=parser.parse_known_args()
    if platform.system()!='Darwin':
        parser.error('these contracts require the actual native Mac Mono runtime and complete .app')
    shared.PACKAGE=arguments.package_root.resolve(strict=True)
    shared.MONO=arguments.mono
    unittest.main(argv=[sys.argv[0],*remaining])
