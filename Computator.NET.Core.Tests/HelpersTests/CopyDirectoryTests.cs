using System;
using System.IO;
using Computator.NET.Core.Helpers;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.HelpersTests
{
    [TestFixture]
    public class CopyDirectoryTests
    {
        private string _root;
        private string _source;
        private string _destination;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "Computator-Copy-" + Guid.NewGuid().ToString("N"));
            _source = Path.Combine(_root, "installed");
            _destination = Path.Combine(_root, "restored");
            Directory.CreateDirectory(Path.Combine(_source, "nested", "deeper"));
            Directory.CreateDirectory(Path.Combine(_source, "empty"));
            File.WriteAllText(Path.Combine(_source, "README"), "No extension.");
            File.WriteAllText(Path.Combine(_source, "nested", "deeper", "example.tsl"), "write(1);");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }

        [Test]
        public void RestoresRootAndNestedDocumentsWhenDestinationDoesNotExist()
        {
            CopyDirectory.Copy(_source + Path.DirectorySeparatorChar, _destination);

            Assert.That(File.ReadAllText(Path.Combine(_destination, "README")), Is.EqualTo("No extension."));
            Assert.That(File.ReadAllText(Path.Combine(_destination, "nested", "deeper", "example.tsl")), Is.EqualTo("write(1);"));
            Assert.That(Directory.Exists(Path.Combine(_destination, "empty")), Is.True);
        }

        [Test]
        public void DoesNotOverwriteExistingUserDocuments()
        {
            Directory.CreateDirectory(_destination);
            File.WriteAllText(Path.Combine(_destination, "README"), "user version");

            Assert.That(() => CopyDirectory.Copy(_source, _destination), Throws.TypeOf<IOException>());
            Assert.That(File.ReadAllText(Path.Combine(_destination, "README")), Is.EqualTo("user version"));
        }

        [Test]
        public void RejectsDestinationsInsideTheSourceTree()
        {
            var nestedDestination = Path.Combine(_source, "child");
            Assert.That(() => CopyDirectory.Copy(_source, nestedDestination), Throws.TypeOf<IOException>());
            Assert.That(Directory.Exists(nestedDestination), Is.False);
        }
    }
}
