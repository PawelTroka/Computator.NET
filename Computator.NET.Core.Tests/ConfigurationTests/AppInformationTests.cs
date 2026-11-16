using System;
using System.IO;
using System.Reflection;
using Computator.NET.DataTypes.Configuration;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.ConfigurationTests
{
    [TestFixture]
    public class AppInformationTests
    {
        // Exercise the same resolution used by the static DataDirectory field
        // without changing process environment variables for other tests.
        private static string Resolve(string configured, string documents)
        {
            var method = typeof(AppInformation).GetMethod("ResolveDataDirectory",
                BindingFlags.NonPublic | BindingFlags.Static);
            NUnit.Framework.Assert.That(method, Is.Not.Null);
            return (string)method.Invoke(null, new object[] { configured, documents });
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        [TestCase("\t\n ")]
        public void MissingOrBlankOverrideFallsBackToDocuments(string configured)
        {
            var documents = Path.Combine(Path.GetTempPath(), "Portable documents λ");
            NUnit.Framework.Assert.That(Resolve(configured, documents),
                Is.EqualTo(Path.GetFullPath(Path.Combine(documents, AppInformation.Name))));
        }

        [Test]
        public void ExplicitOverrideStillTakesPriority()
        {
            var target = Path.Combine(Path.GetTempPath(), "Computator explicit λ");
            NUnit.Framework.Assert.That(Resolve(target, Path.Combine(Path.GetTempPath(), "ignored")),
                Is.EqualTo(Path.GetFullPath(target)));
        }

        [Test]
        public void RelativeOverrideKeepsItsExistingFullPathBehavior()
        {
            NUnit.Framework.Assert.That(Resolve("computator-isolated-profile", Path.GetTempPath()),
                Is.EqualTo(Path.GetFullPath("computator-isolated-profile")));
        }
    }
}
