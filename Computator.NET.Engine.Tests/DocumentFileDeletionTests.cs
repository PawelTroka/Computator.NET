using Computator.NET.Desktop.Controls.CodeEditors;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class DocumentFileDeletionTests
{
    private string _directory = null!;
    private string _path = null!;

    [SetUp]
    public void SetUp()
    {
        _directory = Path.Combine(Path.GetTempPath(), "Computator-Delete-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
        _path = Path.Combine(_directory, "original.tsl");
        File.WriteAllText(_path, "saved version");
    }

    [TearDown]
    public void TearDown() => Directory.Delete(_directory, true);

    [Test]
    public void CancelPreservesOriginalFileAndDoesNotCommitTheDocumentClose()
    {
        var committed = false;
        var validated = false;
        var result = DocumentFileDeletion.TryDelete(_path, () => false,
            () => validated = true, () => committed = true);
        NUnit.Framework.Assert.That(result, Is.False);
        NUnit.Framework.Assert.That(File.ReadAllText(_path), Is.EqualTo("saved version"));
        NUnit.Framework.Assert.That(validated, Is.False);
        NUnit.Framework.Assert.That(committed, Is.False);
    }

    [Test]
    public void ApprovedDiscardClosesOnlyAfterTheOriginalIsDeleted()
    {
        var committed = false;
        var result = DocumentFileDeletion.TryDelete(_path, () =>
        {
            NUnit.Framework.Assert.That(File.ReadAllText(_path), Is.EqualTo("saved version"));
            return true;
        }, () => true, () =>
        {
            NUnit.Framework.Assert.That(File.Exists(_path), Is.False);
            committed = true;
        });
        NUnit.Framework.Assert.That(result, Is.True);
        NUnit.Framework.Assert.That(committed, Is.True);
    }

    [Test]
    public void SavedCopySurvivesDeletingTheOriginal()
    {
        var copy = Path.Combine(_directory, "copy.tsl");
        var result = DocumentFileDeletion.TryDelete(_path, () =>
        {
            File.WriteAllText(copy, "unsaved draft");
            return true;
        }, () => true, () => { });
        NUnit.Framework.Assert.That(result, Is.True);
        NUnit.Framework.Assert.That(File.Exists(_path), Is.False);
        NUnit.Framework.Assert.That(File.ReadAllText(copy), Is.EqualTo("unsaved draft"));
    }

    [Test]
    public void InvalidatedApprovalPreservesTheFileAndBuffer()
    {
        var committed = false;
        var result = DocumentFileDeletion.TryDelete(_path, () => true, () => false, () => committed = true);
        NUnit.Framework.Assert.That(result, Is.False);
        NUnit.Framework.Assert.That(File.ReadAllText(_path), Is.EqualTo("saved version"));
        NUnit.Framework.Assert.That(committed, Is.False);
    }

    [Test]
    public void ExternalFileChangeDuringApprovalIsKeptWithTheSavedCopy()
    {
        var committed = false;
        var copy = Path.Combine(_directory, "copy.tsl");
        NUnit.Framework.Assert.Throws<IOException>(() => DocumentFileDeletion.TryDelete(_path, () =>
        {
            File.WriteAllText(copy, "unsaved draft");
            File.WriteAllText(_path, "external edit");
            return true;
        }, () => true, () => committed = true));
        NUnit.Framework.Assert.That(File.ReadAllText(_path), Is.EqualTo("external edit"));
        NUnit.Framework.Assert.That(File.ReadAllText(copy), Is.EqualTo("unsaved draft"));
        NUnit.Framework.Assert.That(committed, Is.False);
    }

    [Test]
    public void FailedSaveApprovalKeepsTheOriginalAndDoesNotClose()
    {
        var committed = false;
        NUnit.Framework.Assert.Throws<UnauthorizedAccessException>(() => DocumentFileDeletion.TryDelete(_path,
            () => throw new UnauthorizedAccessException("copy could not be saved"), () => true,
            () => committed = true));
        NUnit.Framework.Assert.That(File.ReadAllText(_path), Is.EqualTo("saved version"));
        NUnit.Framework.Assert.That(committed, Is.False);
    }

    [Test]
    public void UnreadableSourceFailsBeforeApprovalOrClosing()
    {
        var approved = false;
        var committed = false;
        using (new FileStream(_path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            NUnit.Framework.Assert.Throws<IOException>(() => DocumentFileDeletion.TryDelete(_path,
                () => approved = true, () => true, () => committed = true));
        NUnit.Framework.Assert.That(approved, Is.False);
        NUnit.Framework.Assert.That(committed, Is.False);
        NUnit.Framework.Assert.That(File.ReadAllText(_path), Is.EqualTo("saved version"));
    }

    [Test]
    public void FileRemovedDuringApprovalDoesNotDiscardTheBuffer()
    {
        var committed = false;
        NUnit.Framework.Assert.Throws<IOException>(() => DocumentFileDeletion.TryDelete(_path, () =>
        {
            File.Delete(_path);
            return true;
        }, () => true, () => committed = true));
        NUnit.Framework.Assert.That(committed, Is.False);
    }

    [Test]
    public void DeletePermissionFailureKeepsFileAndDoesNotCommitTheClose()
    {
        var committed = false;
        try
        {
            if (OperatingSystem.IsWindows()) File.SetAttributes(_path, FileAttributes.ReadOnly);
            else File.SetUnixFileMode(_directory, UnixFileMode.UserRead | UnixFileMode.UserExecute);
            NUnit.Framework.Assert.That(() => DocumentFileDeletion.TryDelete(_path, () => true,
                () => true, () => committed = true), Throws.InstanceOf<UnauthorizedAccessException>());
            NUnit.Framework.Assert.That(File.ReadAllText(_path), Is.EqualTo("saved version"));
            NUnit.Framework.Assert.That(committed, Is.False);
        }
        finally
        {
            if (OperatingSystem.IsWindows()) File.SetAttributes(_path, FileAttributes.Normal);
            else File.SetUnixFileMode(_directory,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }
    }
}
