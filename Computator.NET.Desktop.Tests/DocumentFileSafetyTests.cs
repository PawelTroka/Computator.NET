using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Computator.NET.Desktop.Controls.CodeEditors;
using Microsoft.Win32.SafeHandles;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture]
    public class DocumentFileSafetyTests
    {
        private string _directory;
        private string _path;

        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "Computator-FileSafety-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
            _path = Path.Combine(_directory, "document.tsl");
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var file in Directory.GetFiles(_directory)) File.SetAttributes(file, FileAttributes.Normal);
            Directory.Delete(_directory, true);
        }

        [Test]
        public void CaptureReadsTheSameBomEncodedBytesUsedToDisplayTheDocument()
        {
            File.WriteAllText(_path, "var α = π;", Encoding.Unicode);

            var snapshot = DocumentFileSafety.Capture(_path);

            NUnit.Framework.Assert.That(snapshot.Exists, Is.True);
            NUnit.Framework.Assert.That(snapshot.Text, Is.EqualTo("var α = π;"));
            NUnit.Framework.Assert.That(DocumentFileSafety.Capture(Path.Combine(_directory, "missing.tsl")).Exists, Is.False);
            NUnit.Framework.Assert.Throws<UnauthorizedAccessException>(() => DocumentFileSafety.Capture(_directory));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SuccessfulSaveReturnsABaselineThatCanBeUsedForTheNextSave(bool existing)
        {
            if (existing) File.WriteAllText(_path, "old disk contents");
            var baseline = DocumentFileSafety.Capture(_path);

            baseline = DocumentFileSafety.Write(_path, "saved with π", baseline);
            DocumentFileSafety.Write(_path, "next save with α", baseline);

            NUnit.Framework.Assert.That(File.ReadAllText(_path), Is.EqualTo("next save with α"));
            AssertNoTemporaryFiles();
        }

        [Test]
        public void ExternalChangesWithTheSameLengthAndTimestampAreNotOverwritten()
        {
            File.WriteAllText(_path, "original");
            var timestamp = File.GetLastWriteTimeUtc(_path);
            var baseline = DocumentFileSafety.Capture(_path);

            NUnit.Framework.Assert.Throws<IOException>(() => DocumentFileSafety.Write(_path, "my unsaved changes", baseline,
                beforeCommit: () =>
                {
                    File.WriteAllText(_path, "external");
                    File.SetLastWriteTimeUtc(_path, timestamp);
                }));

            NUnit.Framework.Assert.That(File.ReadAllText(_path), Is.EqualTo("external"));
            AssertNoTemporaryFiles();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void DestinationCreationOrDeletionAfterTheSnapshotIsNotOverridden(bool existed)
        {
            if (existed) File.WriteAllText(_path, "original");
            var baseline = DocumentFileSafety.Capture(_path);

            NUnit.Framework.Assert.Throws<IOException>(() => DocumentFileSafety.Write(_path, "my unsaved changes", baseline,
                beforeCommit: () =>
                {
                    if (existed) File.Delete(_path);
                    else File.WriteAllText(_path, "created externally");
                }));

            NUnit.Framework.Assert.That(File.Exists(_path), Is.EqualTo(!existed));
            if (!existed) NUnit.Framework.Assert.That(File.ReadAllText(_path), Is.EqualTo("created externally"));
            AssertNoTemporaryFiles();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void PartialStagingFailurePreservesTheDestinationAndRemovesTheTemporaryFile(bool existing)
        {
            if (existing) File.WriteAllText(_path, "original disk contents");
            var baseline = DocumentFileSafety.Capture(_path);

            NUnit.Framework.Assert.Throws<IOException>(() => DocumentFileSafety.Write(_path, "unsaved document", baseline,
                stageWriter: (stream, text) =>
                {
                    stream.WriteByte(65);
                    stream.Flush();
                    throw new IOException("Simulated failure after writing part of the staged file.");
                }));

            NUnit.Framework.Assert.That(File.Exists(_path), Is.EqualTo(existing));
            if (existing) NUnit.Framework.Assert.That(File.ReadAllText(_path), Is.EqualTo("original disk contents"));
            AssertNoTemporaryFiles();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void PartialReplacementFailurePreservesTheOriginalAndDoesNotOverwriteANewExternalFile(bool destinationRecreated)
        {
            File.WriteAllText(_path, "original disk contents");
            var baseline = DocumentFileSafety.Capture(_path);
            string backupPath = null;

            var error = NUnit.Framework.Assert.Throws<IOException>(() => DocumentFileSafety.Write(_path, "unsaved", baseline,
                replaceFile: (staged, destination, backup) =>
                {
                    backupPath = backup;
                    File.Move(destination, backup);
                    if (destinationRecreated) File.WriteAllText(destination, "new external file");
                    throw new IOException("Simulated replacement failure after moving the original.");
                }));

            if (destinationRecreated)
            {
                NUnit.Framework.Assert.That(File.ReadAllText(_path), Is.EqualTo("new external file"));
                NUnit.Framework.Assert.That(File.ReadAllText(backupPath), Is.EqualTo("original disk contents"));
                NUnit.Framework.Assert.That(error.Message, Does.Contain(backupPath));
            }
            else
            {
                NUnit.Framework.Assert.That(File.ReadAllText(_path), Is.EqualTo("original disk contents"));
                NUnit.Framework.Assert.That(File.Exists(backupPath), Is.False);
            }
            NUnit.Framework.Assert.That(Directory.GetFiles(_directory, ".computator-save-*.tmp"), Is.Empty);
        }

        [Test]
        public void ReadOnlyAndLockedDestinationsRemainUnchanged()
        {
            File.WriteAllText(_path, "original disk contents");
            var baseline = DocumentFileSafety.Capture(_path);
            File.SetAttributes(_path, FileAttributes.ReadOnly);
            NUnit.Framework.Assert.Throws<UnauthorizedAccessException>(() => DocumentFileSafety.Write(_path, "unsaved", baseline));
            File.SetAttributes(_path, FileAttributes.Normal);

            using (var locked = new FileStream(_path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                NUnit.Framework.Assert.Throws<IOException>(() => DocumentFileSafety.Capture(_path));
                NUnit.Framework.Assert.Throws<IOException>(() => DocumentFileSafety.Write(_path, "unsaved", baseline));
            }

            NUnit.Framework.Assert.That(File.ReadAllText(_path), Is.EqualTo("original disk contents"));
            AssertNoTemporaryFiles();
        }

        [Test]
        public void MissingCandidateDoesNotOpenALockedUnrelatedFile()
        {
            File.WriteAllText(_path, "unrelated locked contents");
            var candidate = Path.Combine(_directory, "new-document.tsl");
            using (var locked = new FileStream(_path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                NUnit.Framework.Assert.That(DocumentFileSafety.AreSameFile(candidate, _path), Is.False);

            NUnit.Framework.Assert.That(File.ReadAllText(_path), Is.EqualTo("unrelated locked contents"));
        }

        [Test]
        public void ExistingUnrelatedFilesCanBeComparedWhileTheirContentsAreExclusivelyLocked()
        {
            File.WriteAllText(_path, "locked document contents");
            var candidate = Path.Combine(_directory, "unrelated.tsl");
            File.WriteAllText(candidate, "unrelated document contents");
            using (var locked = new FileStream(_path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                NUnit.Framework.Assert.That(DocumentFileSafety.AreSameFile(candidate, _path), Is.False);
                NUnit.Framework.Assert.That(DocumentFileSafety.AreSameFile(_path, candidate), Is.False);
                if (OperatingSystem.IsWindows())
                    NUnit.Framework.Assert.That(DocumentFileSafety.AreSameFile(_path, Path.Combine(_directory, "DOCUMENT.TSL")), Is.True);
            }

            NUnit.Framework.Assert.That(File.ReadAllText(_path), Is.EqualTo("locked document contents"));
            NUnit.Framework.Assert.That(File.ReadAllText(candidate), Is.EqualTo("unrelated document contents"));
        }

        [Test]
        public void AnExternalVersionReplacedDuringCommitIsKeptAsABackupAfterSuccessfulSave()
        {
            File.WriteAllText(_path, "original document");
            var baseline = DocumentFileSafety.Capture(_path);

            var saved = DocumentFileSafety.Write(_path, "saved editor document", baseline,
                replaceFile: (staged, destination, backup) =>
                {
                    var external = Path.Combine(_directory, "external-version.tmp");
                    File.WriteAllText(external, "newer external document");
                    // An external atomic save replaces the namespace entry;
                    // the production guard still holds the original file open.
                    File.Replace(external, destination, null);
                    File.Replace(staged, destination, backup);
                });

            NUnit.Framework.Assert.That(File.ReadAllText(_path), Is.EqualTo("saved editor document"));
            NUnit.Framework.Assert.That(saved.RetainedBackupPath, Is.Not.Null);
            NUnit.Framework.Assert.That(File.ReadAllText(saved.RetainedBackupPath), Is.EqualTo("newer external document"));
            NUnit.Framework.Assert.That(Directory.GetFiles(_directory, ".computator-save-*.tmp"), Is.Empty);
        }

        [Test]
        public void DistinctCaseSensitiveNamesAreRefusedInsteadOfMergingDocumentIdentities()
        {
            if (!OperatingSystem.IsWindows()) NUnit.Framework.Assert.Ignore("Windows case-sensitive directory test.");
            // FILE_WRITE_ATTRIBUTES, OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS.
            using (var directoryHandle = CreateFile(_directory, 0x100, 7, IntPtr.Zero, 3, 0x02000000, IntPtr.Zero))
            {
                var enabled = 1U; // FILE_CS_FLAG_CASE_SENSITIVE_DIR.
                if (directoryHandle.IsInvalid || !SetFileInformationByHandle(directoryHandle, 23, ref enabled, sizeof(uint)))
                    NUnit.Framework.Assert.Ignore("This environment cannot enable NTFS directory case sensitivity: " +
                        new Win32Exception(Marshal.GetLastWin32Error()).Message);
            }

            File.WriteAllText(_path, "lowercase file");
            var upper = Path.Combine(_directory, "DOCUMENT.TSL");
            NUnit.Framework.Assert.That(File.Exists(upper), Is.False);
            NUnit.Framework.Assert.Throws<IOException>(() => DocumentFileSafety.AreSameFile(upper, _path));

            File.WriteAllText(upper, "uppercase file");
            NUnit.Framework.Assert.Throws<IOException>(() => DocumentFileSafety.AreSameFile(upper, _path));
            NUnit.Framework.Assert.That(File.ReadAllText(_path), Is.EqualTo("lowercase file"));
            NUnit.Framework.Assert.That(File.ReadAllText(upper), Is.EqualTo("uppercase file"));
            NUnit.Framework.Assert.That(DocumentFileSafety.AreSameFile(Path.Combine(_directory, "absent.tsl"),
                Path.Combine(_directory, "ABSENT.TSL")), Is.True);
        }

        [Test]
        public void HardLinkAliasesShareIdentityAndCannotBeSilentlySeparatedBySaving()
        {
            if (!OperatingSystem.IsWindows()) NUnit.Framework.Assert.Ignore("Windows file identity test.");
            File.WriteAllText(_path, "linked disk contents");
            var alias = Path.Combine(_directory, "hardlink.tsl");
            if (!CreateHardLink(alias, _path, IntPtr.Zero))
                NUnit.Framework.Assert.Ignore("This filesystem cannot create the test hard link: " + new Win32Exception(Marshal.GetLastWin32Error()).Message);

            NUnit.Framework.Assert.That(DocumentFileSafety.AreSameFile(_path, alias), Is.True);
            NUnit.Framework.Assert.Throws<IOException>(() => DocumentFileSafety.Write(alias, "unsaved", DocumentFileSafety.Capture(alias)));

            NUnit.Framework.Assert.That(File.ReadAllText(_path), Is.EqualTo("linked disk contents"));
            NUnit.Framework.Assert.That(File.ReadAllText(alias), Is.EqualTo("linked disk contents"));
            AssertNoTemporaryFiles();
        }

        [Test]
        public void SymbolicLinkAliasesShareIdentityAndArePreservedOnRefusedSave()
        {
            if (!OperatingSystem.IsWindows()) NUnit.Framework.Assert.Ignore("Windows file identity test.");
            File.WriteAllText(_path, "linked disk contents");
            var alias = Path.Combine(_directory, "symlink.tsl");
            try { File.CreateSymbolicLink(alias, _path); }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is PlatformNotSupportedException)
            {
                NUnit.Framework.Assert.Ignore("This Windows environment cannot create the test symbolic link: " + error.Message);
            }

            NUnit.Framework.Assert.That(DocumentFileSafety.AreSameFile(_path, alias), Is.True);
            NUnit.Framework.Assert.Throws<IOException>(() => DocumentFileSafety.Write(alias, "unsaved", DocumentFileSafety.Capture(alias)));

            NUnit.Framework.Assert.That(File.ReadAllText(_path), Is.EqualTo("linked disk contents"));
            NUnit.Framework.Assert.That((File.GetAttributes(alias) & FileAttributes.ReparsePoint) != 0, Is.True);
            AssertNoTemporaryFiles();
        }

        private void AssertNoTemporaryFiles()
        {
            NUnit.Framework.Assert.That(Directory.GetFiles(_directory, ".computator-save-*.tmp"), Is.Empty);
            NUnit.Framework.Assert.That(Directory.GetFiles(_directory, ".computator-backup-*.tmp"), Is.Empty);
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CreateHardLink(string newFileName, string existingFileName, IntPtr securityAttributes);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern SafeFileHandle CreateFile(string path, uint access, uint sharing, IntPtr securityAttributes,
            uint creationDisposition, uint flagsAndAttributes, IntPtr templateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetFileInformationByHandle(SafeFileHandle handle, int informationClass, ref uint information, uint bufferSize);
    }
}
