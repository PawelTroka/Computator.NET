using System.Threading;
using Computator.NET.Core.Model;
using Computator.NET.Validation;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class EditorFocusWorkflowTests
    {
        [TestCase(ViewName.Scripting)]
        [TestCase(ViewName.CustomFunctions)]
        public void EditCommandsFollowNativeTextAreaFocus(ViewName view) => EditorFocusContract.Run(view);
    }
}
