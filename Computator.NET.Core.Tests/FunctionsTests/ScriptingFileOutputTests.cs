using System;
using System.IO;
using Computator.NET.Core.Functions;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture]
    public class ScriptingFileOutputTests
    {
        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void ThrowingFormatterDoesNotTruncateOrCreateFile(bool writeLine, bool existingFile)
        {
            var path = Path.Combine(Path.GetTempPath(), "computator-script-output-" + Guid.NewGuid().ToString("N") + ".txt");
            try
            {
                if (existingFile) System.IO.File.WriteAllText(path, "existing document");
                var value = new ThrowingValue();

                Assert.Throws<InvalidOperationException>(() =>
                {
                    if (writeLine) ScriptingFunctions.writeln(path, value);
                    else ScriptingFunctions.write(path, value);
                });

                Assert.That(System.IO.File.Exists(path), Is.EqualTo(existingFile));
                if (existingFile)
                {
                    Assert.That(System.IO.File.ReadAllText(path), Is.EqualTo("existing document"));
                    // Failing conversion must not leave a writer holding the file open.
                    using (new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
                }
            }
            finally
            {
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SuccessfulFileOutputStillReplacesPreviousContents(bool writeLine)
        {
            var path = Path.Combine(Path.GetTempPath(), "computator-script-output-" + Guid.NewGuid().ToString("N") + ".txt");
            try
            {
                System.IO.File.WriteAllText(path, "old");
                if (writeLine) ScriptingFunctions.writeln(path, "new");
                else ScriptingFunctions.write(path, "new");

                Assert.That(System.IO.File.ReadAllText(path), Is.EqualTo("new" + (writeLine ? Environment.NewLine : "")));
            }
            finally
            {
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }
        }

        private sealed class ThrowingValue
        {
            public override string ToString() => throw new InvalidOperationException("Formatting failed.");
        }
    }
}
