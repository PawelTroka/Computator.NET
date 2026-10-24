using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;

namespace Computator.NET.Desktop.Controls.CodeEditors.AvalonEdit
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

        public void UpdateFoldings(FoldingManager manager,
            TextDocument document)
        {
            int firstErrorOffset;
            var newFoldings = CreateNewFoldings(document, out firstErrorOffset);
            manager.UpdateFoldings(newFoldings, firstErrorOffset);
        }

        /// <summary>
        ///     Create <see cref="ICSharpCode.AvalonEdit.Folding.NewFolding" />s for the specified document.
        /// </summary>
        public IEnumerable<NewFolding> CreateNewFoldings(
            TextDocument document, out int firstErrorOffset)
        {
            firstErrorOffset = -1;
            return CreateNewFoldings(document);
        }

        /// <summary>
        ///     Create <see cref="ICSharpCode.AvalonEdit.Folding.NewFolding" />s for the specified document.
        /// </summary>
        public IEnumerable<NewFolding> CreateNewFoldings(
            ITextSource document)
        {
            var newFoldings = new List<NewFolding>();

            foreach (var range in BraceFoldingRanges.Create(document.Text, OpeningBrace, ClosingBrace))
            {
                newFoldings.Add(new NewFolding(range.Start, range.End));
            }
            return newFoldings;
        }
    }
}
