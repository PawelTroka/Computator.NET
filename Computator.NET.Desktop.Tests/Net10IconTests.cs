using System.Drawing;
using Computator.NET.Desktop.Controls;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture]
    public class Net10IconTests
    {
        [TestCase(IconEx.SystemIcons.Error)]
        [TestCase(IconEx.SystemIcons.Warning)]
        public void PublicIconCloneRemainsUsableAfterOriginalHandleIsReleased(IconEx.SystemIcons kind)
        {
            Icon copy;
            using (var original = new IconEx(kind, new Size(16, 16)))
            {
                NUnit.Framework.Assert.That(original.Icon.Size, Is.EqualTo(new Size(16, 16)));
                copy = (Icon)original.Icon.Clone();
            }
            using (copy)
            using (var bitmap = copy.ToBitmap())
            {
                NUnit.Framework.Assert.That(bitmap.Width, Is.EqualTo(16));
                NUnit.Framework.Assert.That(bitmap.Height, Is.EqualTo(16));
            }
        }
    }
}
