namespace Computator.NET.UI.CodeEditors
{
    /// <summary>
    ///     Implements AvalonEdit ICompletionData interface to provide the entries in the completion drop down.
    /// </summary>
    public class CompletionData : ICSharpCode.AvalonEdit.CodeCompletion.ICompletionData
    {
        private readonly string _content;
        private Functions.FunctionInfo _alternativeDescription;
        private int imageIndex;

        public CompletionData(string text)
        {
            Text = text;
        }

        public CompletionData(string text, string menuText, Functions.FunctionInfo functionInfo, int imageIndex)
        {
            Text = text;
            _content = menuText;
            _alternativeDescription = functionInfo;
            //////////////////////////////////// this._image= imageIndexToImage(imageIndex).ToBitmapSource();
        }

        public System.Windows.Media.ImageSource Image { get; }
        public string Text { get; }
        // Use this property if you want to show a fancy UIElement in the drop down list.
        public object Content
        {
            get { return _content ?? Text; }
        }

        public object Description
        {
            get
            {
                if (Data.FunctionsDetails.Details.ContainsKey(Text))
                    return Data.FunctionsDetails.Details[Text].Title + System.Environment.NewLine +
                           StripTagsCharArray(Data.FunctionsDetails.Details[Text].Description);
                return "Description for " + Text;
            }
        }

        public double Priority
        {
            get { return 0; }
        }

        public void Complete(ICSharpCode.AvalonEdit.Editing.TextArea textArea,
            ICSharpCode.AvalonEdit.Document.ISegment completionSegment, System.EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }

        private System.Drawing.Image imageIndexToImage(int index)
        {
            switch (index)
            {
                case 0:
                    return Properties.Resources.Real;
                case 1:
                    return Properties.Resources.Complex;
                case 2:
                    return Properties.Resources.Natural;
                case 3:
                    return Properties.Resources.Integer;
                default:
                    return null;
            }
        }

        public static string StripTagsCharArray(string source)
        {
            var array = new char[source.Length];
            var arrayIndex = 0;
            var inside = false;

            for (var i = 0; i < source.Length; i++)
            {
                var let = source[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }
    }
}