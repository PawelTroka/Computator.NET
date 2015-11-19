using System.Collections.Generic;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace Computator.NET.UI.CodeEditors
{
    public class OffsetColorizer : DocumentColorizingTransformer
    {
        // public int StartOffset { get; set; }
        // public int EndOffset { get; set; }
        //

        private SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(100,255,0,0));
        public List<int> LinesWithErrors { get; set; } = new List<int>();

        public ITextRunConstructionContext Context { get { return CurrentContext; } }
        protected override void ColorizeLine(DocumentLine line)
        {
            if (!LinesWithErrors.Contains(line.LineNumber))
                return;
            //  if (line.Length == 0)
            //  return;
            //if (line.Offset < StartOffset || line.Offset > EndOffset)
            // return;

            // int start = line.Offset > StartOffset ? line.Offset : StartOffset;
            //int end = EndOffset > line.EndOffset ? line.EndOffset : EndOffset;
            int start = line.Offset;
            int end = line.EndOffset;

            ChangeLinePart(start, end, element => element.TextRunProperties.SetBackgroundBrush(brush));
        }
    }
}