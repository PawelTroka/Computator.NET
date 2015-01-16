using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using AutocompleteMenuNS;
using Computator.NET.Config;
using Computator.NET.Evaluation;
using ScintillaNET;

namespace Computator.NET.UI
{
    internal class ScriptingRichTextBox : Scintilla
    {
        private readonly ScriptEvaluator _eval;
        private const int AutcompleteMode = 2;
        private AutocompleteMenuNS.AutocompleteMenu _autocompleteMenu;

        public ScriptingRichTextBox()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            _eval = new ScriptEvaluator();

            AutoComplete.List.Clear();
            foreach (var t in _autocompleteMenu.Items)
            {
                AutoComplete.List.Add(t);
            }
            this.Indentation.ShowGuides = true;
            this.Indentation.SmartIndentType = SmartIndent.Simple;
            this.Whitespace.Mode = WhitespaceMode.VisibleAfterIndent;
        }

        public bool Sort { get; set; }

        public override string Text
        {
            get { return base.Text.Replace("*", "·"); }
            set { base.Text = value.Replace("*", "⋅"); }
        }

        public string Expression
        {
            get { return base.Text.Replace("·", "*"); }
        }

        private void InitializeComponent()
        {
            ConfigurationManager.Language = "cs";
            ConfigurationManager.IsBuiltInEnabled = true;
            ConfigurationManager.IsUserEnabled = true;
            ConfigurationManager.Configure( /*config*/);
            Margins[0].Width = 35;
            KeyPress += SciriptingRichTextBox_KeyPress;
            IsBraceMatching = true;
            Name = "SciriptingRichTextBox";
            this.LoadKeywordsFromXml(GlobalConfig.fullPath("UI", "script_syntax.xml"), true);

            _autocompleteMenu = new AutocompleteMenuNS.AutocompleteMenu();
            _autocompleteMenu.SetAutocompleteMenu(this, _autocompleteMenu);
            RefreshAutoComplete();
        }

        public void ProcessScript(RichTextBox things, string customCode = "")
        {
            try
            {
                _eval.Evaluate(Expression, customCode);
                _eval.Invoke(things);
            }
            catch (Exception ex2)
            {
                var sb = new StringBuilder();
                sb.AppendLine(ex2.Message);
                if (ex2.InnerException == null) throw new Exception(sb.ToString(), ex2);
                sb.AppendLine(ex2.InnerException.Message);
                if (ex2.InnerException.InnerException == null) throw new Exception(sb.ToString(), ex2);
                sb.AppendLine(ex2.InnerException.InnerException.Message);
                if (ex2.InnerException.InnerException != null)
                    sb.AppendLine(ex2.InnerException.InnerException.Message);
                //MessageBox.Show(sb.ToString(), "Error");
                throw new Exception(sb.ToString(), ex2);
            }
        }


        public void RefreshAutoComplete()
        {
            switch (AutcompleteMode)
            {
                case 1:
                {
                    string[] strings = Evaluator.getAutocompleteStrings();
                    var autocompleteItems = strings.Select(s => new SubstringAutocompleteItem(s)).ToList();

                    _autocompleteMenu.SetAutocompleteItems(autocompleteItems);
                }
                    break;
                case 0:
                    _autocompleteMenu.Items = Evaluator.getAutocompleteStrings();
                    break;
                case 2:
                {
                    AutocompleteItem[] array = Evaluator.getAutocompleteItems();
                    AutocompleteItem[] array2 =
                        array.Distinct(new ExpressionTextBox.AutocompleteItemEqualityComparer()).ToArray();
                    if (Sort)
                        Array.Sort(array2, (a, b) => a.Text.CompareTo(b.Text));
                    _autocompleteMenu.SetAutocompleteItems(array2);
                }
                    break;
            }
            RefreshSize();
        }

        private void SciriptingRichTextBox_KeyPress(object s, KeyPressEventArgs e)
        {
            // if (isOperator(e.KeyChar))
            {
                if (e.KeyChar == '*')
                {
                    e.KeyChar = '·';
                    //for (int i = 0; i < this.AutoCompleteCustomSource.Count; i++)
                    // this.AutoCompleteCustomSource[i] += Text + e.KeyChar;
                }
            }
        }


        public void RefreshSize()
        {
            _autocompleteMenu.MaximumSize = new Size(Size.Width, _autocompleteMenu.MaximumSize.Height);
        }


        private static bool IsOperator(char c)
        {
            return c == '*' || c == '/' || c == '+' || c == '-' || c == '(' || c == '^' || c == '!';
        }
    }

    internal static class ScintillaExtension
    {
        /// <summary>
        ///     Loads keywords from text file into a specified keyword set and replaces existing keyword set.
        ///     Keywords must be space separated.
        /// </summary>
        public static bool LoadKeywordsFromText(this Scintilla scintilla, int keywordSet, string txtFileName)
        {
            return LoadKeywordsFromText(scintilla, keywordSet, txtFileName, false);
        }

        /// <summary>
        ///     Loads keywords from text file into a specified keyword set.
        ///     Specified append flag indicates whether keywords shall be appended to existing keyword set.
        ///     Keywords must be space separated.
        /// </summary>
        public static bool LoadKeywordsFromText(this Scintilla scintilla, int keywordSet, string txtFileName,
            bool append)
        {
            if (scintilla == null || String.IsNullOrEmpty(txtFileName) || !File.Exists(txtFileName))
            {
                return false;
            }

            try
            {
                var streamReader = new StreamReader(txtFileName);

                string keywords = streamReader.ReadToEnd();

                streamReader.Close();
                streamReader.Dispose();

                keywords = CleanUpKeywords(keywords);

                if (append)
                {
                    string oldKeywords = scintilla.Lexing.Keywords[keywordSet];
                    oldKeywords = CleanUpKeywords(oldKeywords);

                    if (!oldKeywords.Contains(keywords))
                    {
                        scintilla.Lexing.Keywords[keywordSet] = oldKeywords + " \r\n" + keywords;
                    }
                }
                else
                {
                    scintilla.Lexing.Keywords[keywordSet] = keywords;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }


        /// <summary>
        ///     Loads Scintilla keywords from xml file for a pre-defined lexer and replaces existing keyword set.
        /// </summary>
        public static bool LoadKeywordsFromXml(this Scintilla scintilla, string xmlFileName)
        {
            //  XML Format:
            //  <KeywordsList>
            //      <Keywords List="number"  Inherit="False">
            //          xx ccc www xxx
            //      </Keywords>
            //      ...
            //  </KeywordsList>
            //
            // If custom languge contains a keyword list, do not specify the Keywords 'Inherit' attribute or set it to FALSE.
            // 
            return LoadKeywordsFromXml(scintilla, xmlFileName, true);
        }

        /// <summary>
        ///     Loads Scintilla keywords from xml file for a pre-defined lexer.
        ///     Specified append flag indicates whether keywords shall be appended to existing keyword set.
        /// </summary>
        public static bool LoadKeywordsFromXml(this Scintilla scintilla, string xmlFileName, bool append)
        {
            if (String.IsNullOrEmpty(xmlFileName) || !File.Exists(xmlFileName) || scintilla == null)
                return false;

            var xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(xmlFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            try
            {
                XmlElement xmlElement = xmlDoc.DocumentElement;
                XmlNodeList xmlKeywordsList = xmlElement.GetElementsByTagName("Keywords");

                foreach (XmlElement xmlKeywords in xmlKeywordsList)
                {
                    // we need at least one attribute
                    if (xmlKeywords.Attributes.Count < 1)
                        continue;

                    foreach (var xmlAttr in xmlKeywords.Attributes.Cast<XmlAttribute>().Where(xmlAttr => xmlAttr.Name.ToUpper() == "LIST"))
                    {
                        int keywordSet;
                        if (!int.TryParse(xmlAttr.Value, out keywordSet)) continue;
                        string keywords = CleanUpKeywords(xmlKeywords.InnerText);

                        if (append)
                        {
                            string oldKeywords = scintilla.Lexing.Keywords[keywordSet];
                            oldKeywords = CleanUpKeywords(oldKeywords);

                            if (!oldKeywords.Contains(keywords))
                            {
                                // this does not work for our purpose:
                                // scintilla.Lexing.SetKeywords();
                                scintilla.Lexing.Keywords[keywordSet] = oldKeywords + " \r\n" + keywords;
                            }
                        }
                        else
                        {
                            scintilla.Lexing.Keywords[keywordSet] = keywords;
                        }
                    }
                } // foreach xmlKeywords
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Returns a single space delimited keyword list.
        /// </summary>
        private static string CleanUpKeywords(string keywords)
        {
            // replace all unnecessary junk to have a clean list
            keywords = keywords.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ').Replace('\0', ' ').Trim();
            while (keywords.Contains("  "))
                keywords = keywords.Replace("  ", " ");
            return keywords;
        }
    }
}