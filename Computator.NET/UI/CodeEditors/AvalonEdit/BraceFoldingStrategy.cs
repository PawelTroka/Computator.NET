namespace Computator.NET.UI.CodeEditors
{
    /// <summary>
    ///     Allows producing foldings from a document based on braces.
    /// </summary>
    public class BraceFoldingStrategy
    {
        /// <summary>
        ///     Creates a new BraceFoldingStrategy.
        /// </summary>
        public BraceFoldingStrategy()
        {
            OpeningBrace = '{';
            ClosingBrace = '}';
        }

        /// <summary>
        ///     Gets/Sets the opening brace. The default value is '{'.
        /// </summary>
        public char OpeningBrace { get; set; }

        /// <summary>
        ///     Gets/Sets the closing brace. The default value is '}'.
        /// </summary>
        public char ClosingBrace { get; set; }

        public void UpdateFoldings(ICSharpCode.AvalonEdit.Folding.FoldingManager manager,
            ICSharpCode.AvalonEdit.Document.TextDocument document)
        {
            int firstErrorOffset;
            var newFoldings = CreateNewFoldings(document, out firstErrorOffset);
            manager.UpdateFoldings(newFoldings, firstErrorOffset);
        }

        /// <summary>
        ///     Create <see cref="ICSharpCode.AvalonEdit.Folding.NewFolding" />s for the specified document.
        /// </summary>
        public System.Collections.Generic.IEnumerable<ICSharpCode.AvalonEdit.Folding.NewFolding> CreateNewFoldings(
            ICSharpCode.AvalonEdit.Document.TextDocument document, out int firstErrorOffset)
        {
            firstErrorOffset = -1;
            return CreateNewFoldings(document);
        }

        /// <summary>
        ///     Create <see cref="ICSharpCode.AvalonEdit.Folding.NewFolding" />s for the specified document.
        /// </summary>
        public System.Collections.Generic.IEnumerable<ICSharpCode.AvalonEdit.Folding.NewFolding> CreateNewFoldings(
            ICSharpCode.AvalonEdit.Document.ITextSource document)
        {
            var newFoldings = new System.Collections.Generic.List<ICSharpCode.AvalonEdit.Folding.NewFolding>();

            var startOffsets = new System.Collections.Generic.Stack<int>();
            var lastNewLineOffset = 0;
            var openingBrace = OpeningBrace;
            var closingBrace = ClosingBrace;
            for (var i = 0; i < document.TextLength; i++)
            {
                var c = document.GetCharAt(i);
                if (c == openingBrace)
                {
                    startOffsets.Push(i);
                }
                else if (c == closingBrace && startOffsets.Count > 0)
                {
                    var startOffset = startOffsets.Pop();
                    // don't fold if opening and closing brace are on the same line
                    if (startOffset < lastNewLineOffset)
                    {
                        newFoldings.Add(new ICSharpCode.AvalonEdit.Folding.NewFolding(startOffset, i + 1));
                    }
                }
                else if (c == '\n' || c == '\r')
                {
                    lastNewLineOffset = i + 1;
                }
            }
            newFoldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
            return newFoldings;
        }
    }
}