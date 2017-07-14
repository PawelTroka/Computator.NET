#if !__MonoCS__
using System;
using System.Drawing;
using System.Windows.Media;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Properties;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Computator.NET.Desktop.Controls.CodeEditors.AvalonEdit
{
    /// <summary>
    ///     Implements AvalonEdit ICompletionData interface to provide the entries in the completion drop down.
    /// </summary>
    public class CompletionData : ICompletionData
    {
        private readonly string _content;
        private FunctionInfo _alternativeDescription;
        //private int imageIndex;//TODO: implement image show

        public CompletionData(string text, IFunctionsDetails functionsDetails)
        {
            Text = text;
            this._functionsDetails = functionsDetails;
        }

        public CompletionData(string text, string menuText, FunctionInfo functionInfo, int imageIndex, IFunctionsDetails functionsDetails)
        {
            Text = text;
            _content = menuText;
            _alternativeDescription = functionInfo;
            this._functionsDetails = functionsDetails;
            //////////////////////////////////// this._image= imageIndexToImage(imageIndex).ToBitmapSource();//TODO: implement image show
        }

        public ImageSource Image { get; }
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
                if (_functionsDetails.ContainsKey(Text))
                    return _functionsDetails[Text].Title + Environment.NewLine +
                           StripTagsCharArray(_functionsDetails[Text].Description);
                return "Description for " + Text;
            }
        }

        private IFunctionsDetails _functionsDetails;

        public double Priority
        {
            get { return 0; }
        }

        public void Complete(TextArea textArea,
            ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }

        private Image imageIndexToImage(int index)
        {
            switch (index)
            {
                case 0:
                    return Resources.Real;
                case 1:
                    return Resources.Complex;
                case 2:
                    return Resources.Natural;
                case 3:
                    return Resources.Integer;
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
#endif