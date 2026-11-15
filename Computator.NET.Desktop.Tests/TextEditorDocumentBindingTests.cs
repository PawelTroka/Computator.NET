using System.Threading;
using Computator.NET.Core.Natives;
using Computator.NET.Validation;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class TextEditorDocumentBindingTests
    {
        [OneTimeSetUp]
        public void InitializeGsl() => GSLInitializer.Initialize();

        [TestCase(ViewName.Scripting)]
        [TestCase(ViewName.CustomFunctions)]
        public void NativeDocumentOperationsFollowTheActiveBuffer(ViewName view)
        {
            EditorDocumentBindingContract.Run(view);
        }
    }
}
