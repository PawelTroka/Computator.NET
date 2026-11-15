using System;
using System.IO;
using System.Reflection;
using Computator.NET.Core.Helpers;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.HelpersTests
{
    [TestFixture]
    public class CopyDirectoryTests
    {
        [Test]
        public void FlatSourceCreatesMissingDestination()
        {
            var root = NewTemporaryRoot();
            try
            {
                var source = Path.Combine(root, "examples");
                var target = Path.Combine(root, "restored");
                Directory.CreateDirectory(source);
                File.WriteAllText(Path.Combine(source, "sample.tsl"), "write(42);");

                CopyExamples(source, target);

                NUnit.Framework.Assert.That(File.ReadAllText(Path.Combine(target, "sample.tsl")),
                    Is.EqualTo("write(42);"));
            }
            finally { Directory.Delete(root, true); }
        }

        [Test]
        public void CopiesExtensionlessFilesAndRetainsNestedRelativePaths()
        {
            var root = NewTemporaryRoot();
            try
            {
                var source = Path.Combine(root, "examples");
                var target = Path.Combine(root, "restored");
                var nested = Path.Combine(source, "functions", "examples");
                Directory.CreateDirectory(nested);
                File.WriteAllText(Path.Combine(source, "README"), "Documentation");
                File.WriteAllText(Path.Combine(nested, "example.tslf"), "real f(real x) { return x; }");

                CopyExamples(source + Path.DirectorySeparatorChar, target);

                NUnit.Framework.Assert.That(File.ReadAllText(Path.Combine(target, "README")),
                    Is.EqualTo("Documentation"));
                NUnit.Framework.Assert.That(
                    File.ReadAllText(Path.Combine(target, "functions", "examples", "example.tslf")),
                    Does.Contain("real f"));
            }
            finally { Directory.Delete(root, true); }
        }

        [Test]
        public void ExistingDestinationDocumentsAreNotOverwritten()
        {
            var root = NewTemporaryRoot();
            try
            {
                var source = Path.Combine(root, "examples");
                var target = Path.Combine(root, "restored");
                Directory.CreateDirectory(source);
                Directory.CreateDirectory(target);
                File.WriteAllText(Path.Combine(source, "custom.tsl"), "new");
                File.WriteAllText(Path.Combine(target, "custom.tsl"), "user's content");

                var error = NUnit.Framework.Assert.Throws<TargetInvocationException>(
                    () => CopyExamples(source, target));

                NUnit.Framework.Assert.That(error.InnerException, Is.InstanceOf<IOException>());
                NUnit.Framework.Assert.That(File.ReadAllText(Path.Combine(target, "custom.tsl")),
                    Is.EqualTo("user's content"));
            }
            finally { Directory.Delete(root, true); }
        }

        private static string NewTemporaryRoot()
        {
            var root = Path.Combine(Path.GetTempPath(), "Computator-CopyDirectory-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            return root;
        }

        // The production helper is internal; test it without expanding its public API.
        private static void CopyExamples(string source, string target)
        {
            var type = typeof(BindingUtils).Assembly.GetType("Computator.NET.Core.Helpers.CopyDirectory", true);
            var method = type.GetMethod("Copy", BindingFlags.Public | BindingFlags.Static);
            method.Invoke(null, new object[] { source, target });
        }
    }
}
