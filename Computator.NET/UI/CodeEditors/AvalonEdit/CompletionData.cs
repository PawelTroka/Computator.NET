using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Media;
using AutocompleteMenuNS;
using Computator.NET.Data;
using Computator.NET.Functions;
using Computator.NET.Properties;
using Computator.NET.UI.AutocompleteMenu;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Computator.NET.UI.CodeEditors
{
    /// <summary>
    ///     Implements AvalonEdit ICompletionData interface to provide the entries in the completion drop down.
    /// </summary>
    public class CompletionData : ICompletionData
    {
        private string _content;
        private ImageSource _image;
        private FunctionInfo _alternativeDescription;

        private int imageIndex;



        public CompletionData(string text)
        {
            Text = text;
        }

        public CompletionData(string text, string menuText, FunctionInfo functionInfo, int imageIndex)
        {
            this.Text = text;
            this._content = menuText;
            this._alternativeDescription = functionInfo;
           //////////////////////////////////// this._image= imageIndexToImage(imageIndex).ToBitmapSource();
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

        public ImageSource Image
        {
            get { return _image; }
        }

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
                if (FunctionsDetails.Details.ContainsKey(this.Text))
                    return FunctionsDetails.Details[this.Text].Title+Environment.NewLine+ StripTagsCharArray(FunctionsDetails.Details[this.Text].Description);
                return "Description for " + Text;
            }
        }

        public static string StripTagsCharArray(string source)
        {
            char[] array = new char[source.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < source.Length; i++)
            {
                char let = source[i];
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

        public double Priority
        {
            get { return 0; }
        }
        
        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }
    }
}