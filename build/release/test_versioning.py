"""Independent calendar fixtures and public distribution-version boundaries."""
from datetime import date, datetime, timedelta, timezone
from pathlib import Path
import tempfile
import unittest
from unittest import mock

import versioning


class DateTimeVersionTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.root = Path(self.temporary.name)
        (self.root/'Version.props').write_text('<Project><PropertyGroup>'
            '<ComputatorBaseVersion>3.0.0</ComputatorBaseVersion>'
            '<ComputatorRevision>0</ComputatorRevision>'
            '<ComputatorVersionTimestamp></ComputatorVersionTimestamp>'
            '<ComputatorPrerelease>preview.1</ComputatorPrerelease>'
            '</PropertyGroup></Project>')
        (self.root/'SharedAssemblyInfo.cs').write_text(versioning.expected_block(versioning.identity(self.root)))
    def tearDown(self):
        self.temporary.cleanup()
    def bytes(self):
        return tuple((self.root/name).read_bytes() for name in ('Version.props','SharedAssemblyInfo.cs'))
    def stamp(self, value):
        return versioning.prepare(self.root, value, [])
    def release(self, tag, draft=False, published='2026-11-03T12:00:00Z'):
        return {'tag_name':tag,'draft':draft,'published_at':published}

    def test_independent_microsoft_example_and_day_boundaries(self):
        # Microsoft's documented 17 May 2005,16:04:27 example is1963.28933.
        for text, expected in [('2005-05-17T16:04:27+02:00',(1963,28933)),
                               ('2000-01-01T00:00:00+01:00',(0,0)),
                               ('2000-01-02T23:59:59+01:00',(1,43199)),
                               ('2026-11-03T12:03:24+01:00',(9803,21702))]:
            with self.subTest(timestamp=text):
                self.assertEqual(expected,versioning.wildcard_components(versioning.timestamp(text)))

    def test_offset_uses_local_calendar_and_wall_clock_not_utc_elapsed_time(self):
        local=versioning.timestamp('2026-11-03T00:30:00+01:00')
        utc=versioning.timestamp('2026-11-02T23:30:00Z')
        self.assertEqual(local,utc)
        self.assertEqual((9803,900),versioning.wildcard_components(local))
        self.assertEqual((9802,42300),versioning.wildcard_components(utc))

    def test_freeze_preserves_full_identity_and_store_mac_projections(self):
        value=self.stamp('2026-11-03T12:03:25.900+01:00')
        self.assertEqual('3.0.9803.21702',value['assemblyVersion'])
        self.assertEqual(value['assemblyVersion'],value['fileVersion'])
        self.assertEqual('3.0.9803.21702-preview.1',value['productVersion'])
        self.assertEqual('3.0.9803',value['msiVersion'])
        self.assertEqual('3.0.9803.0',value['msixVersion'])
        self.assertEqual('3.9803.21702',value['macBundleVersion'])
        self.assertEqual('3.0.9803',value['macBundleShortVersion'])
        self.assertEqual('2026-11-03T12:03:25+01:00',value['versionTimestamp'])
        frozen=self.bytes(); self.assertEqual(value,versioning.check(self.root))
        self.assertEqual(frozen,self.bytes(),'Normal validation must not rewrite source or read a clock.')

    def test_default_captures_the_clock_only_once(self):
        class Clock(datetime):
            calls=0
            @classmethod
            def now(cls,tz=None):
                cls.calls+=1
                return cls(2026,11,3,12,3,24,tzinfo=timezone.utc)
        with mock.patch.object(versioning,'datetime',Clock):
            versioning.prepare(self.root,published_releases=[])
        self.assertEqual(1,Clock.calls)

    def test_bad_clock_range_and_missing_offset_leave_sources_unchanged(self):
        original=self.bytes()
        overflow=(date(2000,1,1)+timedelta(days=65535)).isoformat()+'T00:00:00+00:00'
        for value in ('1999-12-31T23:59:59+01:00',overflow,'2026-11-03T12:00:00','invalid'):
            with self.subTest(timestamp=value),self.assertRaises(ValueError): self.stamp(value)
            self.assertEqual(original,self.bytes())

    def test_clock_rollback_and_version_drift_fail_before_source_writes(self):
        self.stamp('2026-11-03T12:03:25+01:00'); frozen=self.bytes()
        # Even a one-second rollback inside the same two-second version slot fails.
        with self.assertRaisesRegex(ValueError,'backwards'): self.stamp('2026-11-03T12:03:24+01:00')
        self.assertEqual(frozen,self.bytes())
        props=self.root/'Version.props'; props.write_text(props.read_text().replace('3.0.9803','3.0.9802'))
        drift=self.bytes()
        with self.assertRaisesRegex(ValueError,'timestamp differs'): self.stamp('2026-11-04T12:00:00+01:00')
        self.assertEqual(drift,self.bytes())

    def test_compatibility_source_drift_is_not_repaired_by_preparation(self):
        shared=self.root/'SharedAssemblyInfo.cs'; shared.write_text(shared.read_text().replace('AssemblyVersion("3.0.0.0")','AssemblyVersion("1.0.0.0")'))
        drift=self.bytes()
        with self.assertRaisesRegex(ValueError,'differs from Version.props'): self.stamp('2026-11-03T12:00:00+01:00')
        self.assertEqual(drift,self.bytes())

    def test_only_public_same_day_versions_collide_and_exact_retry_is_allowed(self):
        value=self.stamp('2026-11-03T12:03:24+01:00')
        other='v3.0.9803.1-preview.1'
        allowed=[self.release('v2.3.2'),self.release(other,draft=True),
                 self.release(other,published=None),{'tag_name':other},{'conclusion':'success'},
                 self.release('v'+value['productVersion'])]
        versioning.published_release_guard(value,allowed)
        for tag in (other,'v3.0.9803-preview.1','v3.0.9804.0-preview.1'):
            with self.subTest(tag=tag),self.assertRaises(ValueError):
                versioning.published_release_guard(value,[self.release(tag)])
        with self.assertRaises(ValueError): versioning.published_release_guard(value,{})

    def test_public_collision_does_not_change_the_current_frozen_sources(self):
        old=self.bytes()
        with self.assertRaisesRegex(ValueError,'collide'):
            versioning.prepare(self.root,'2026-11-03T12:00:00+01:00',[self.release('v3.0.9803.1-preview.1')])
        self.assertEqual(old,self.bytes())

    def test_published_retry_requires_original_stamp_even_within_same_two_second_slot(self):
        initial=self.bytes()
        product='3.0.9803.21702-preview.1'
        published=[self.release('v'+product)]
        # A different, unstamped source identity cannot adopt an existing release.
        with self.assertRaisesRegex(ValueError,'original frozen source identity and timestamp'):
            versioning.prepare(self.root,'2026-11-03T12:03:24+01:00',published)
        self.assertEqual(initial,self.bytes())
        frozen=self.stamp('2026-11-03T12:03:24+01:00'); original=self.bytes()
        # Both adjacent seconds produce revision21702, but the stamp still differs.
        with self.assertRaisesRegex(ValueError,'original frozen source identity and timestamp'):
            versioning.prepare(self.root,'2026-11-03T12:03:25+01:00',published)
        self.assertEqual(original,self.bytes())
        self.assertEqual(frozen,versioning.prepare(self.root,'2026-11-03T12:03:24+01:00',published))
        self.assertEqual(original,self.bytes())


if __name__=='__main__': unittest.main()
