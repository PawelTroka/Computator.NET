using NUnit.Framework;

namespace Computator.NET.Core.Tests.CompilationTests
{
    [TestFixture]
    public class CompilerReferencePathTests
    {
        [TestCase("file")]
        [TestCase("directory")]
        public void DistinctCasePathsCanSupplyDifferentTypes(string layout) =>
            CompilerReferencePathContract.DistinctReferences(layout);

        [TestCase("file")]
        [TestCase("directory")]
        public void CompilerInstancesKeepTheirOwnReferenceMetadata(string layout) =>
            CompilerReferencePathContract.IsolatedMetadataCache(layout);

        [TestCase("file")]
        [TestCase("directory")]
        public void RepeatedAndEquivalentReferencesRemainValid(string layout) =>
            CompilerReferencePathContract.EquivalentReferences(layout);
    }
}
