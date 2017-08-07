using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Utility;

namespace Computator.NET.Core.Autocompletion
{
    public interface IFunctionsDetails
    {
        FunctionInfo this[string signature] { get; set; }
        bool ContainsKey(string signature);
        KeyValuePair<string, FunctionInfo>[] ToArray();
    }

    public class FunctionsDetails : IFunctionsDetails
    {
        private readonly Dictionary<string, FunctionInfo> _details;

        public FunctionsDetails()
        {
            _details = LoadFunctionsDetailsFromXmlFile();
            AddDetailsFromMetadata();
             //SaveEmptyFunctionDetailsToXmlFile();
        }

        private void AddDetailsFromMetadata()
        {
            var items = AutocompleteProvider.GetAutocompleteItemsForScripting();
            items = items.Distinct(new AutocompleteItemEqualityComparer()).ToArray();
            foreach (var item in items)
            {
                if(!_details.ContainsKey(item.Text))
                _details.Add(item.Text,
                     item.Details);
            }
        }

        public FunctionInfo this[string signature]
        {
            get { return _details[signature]; }
            set { _details[signature] = value; }
        }

        private void SaveFunctionDetailsToXmlFile()
        {
            //var _details = new Dictionary<string, FunctionInfo>();
            //foreach (var item in sourceItems)
            //_details.Add(item.FunctionInfo.Signature, item.FunctionInfo);

            var serializer = new XmlSerializer(typeof(FunctionInfo[]),
                new XmlRootAttribute("FunctionsDetails"));
            var stream = new StreamWriter("functions_saved.xml");


            serializer.Serialize(stream,
                _details.Select(kv =>
                    new FunctionInfo
                    {
                        Signature = kv.Key,
                        Url = kv.Value.Url,
                        Description = kv.Value.Description,
                        Title = kv.Value.Title,
                        Category = kv.Value.Category,
                        Type = kv.Value.Type
                    }).ToArray());
        }

        //TODO: this function should create new functions file with all it's previous content and empty spaces for new functions which exists in ElementaryFuntions, SpecialFunctions etc but not in the xml file yet
        public void SaveEmptyFunctionDetailsToXmlFile()
        {
            //var _details = new Dictionary<string, FunctionInfo>();
            //foreach (var item in sourceItems)
            //_details.Add(item.FunctionInfo.Signature, item.FunctionInfo);

            var serializer = new XmlSerializer(typeof(FunctionInfo[]),
                new XmlRootAttribute("FunctionsDetails"));
            var stream = new StreamWriter("functions_empty_saved.xml");


            var detailsWithEmpties = new Dictionary<string, FunctionInfo>();

            var items = AutocompleteProvider.GetAutocompleteItemsForScripting();
            items = items.Distinct(new AutocompleteItemEqualityComparer()).ToArray();
            foreach (var item in items)
            {
                //if(!_details.ContainsKey(item.Text))
                detailsWithEmpties.Add(item.Text,
                    _details.ContainsKey(item.Text) ? _details[item.Text] : item.Details);
            }


            serializer.Serialize(stream,
                detailsWithEmpties.Select(kv =>
                    new FunctionInfo
                    {
                        Signature = kv.Key,
                        Url = kv.Value.Url,
                        Description = ToHtmlXmlEncoded(kv.Value.Description),
                        Title = kv.Value.Title,
                        Category = kv.Value.Category,
                        Type = kv.Value.Type
                    }).ToArray());
            stream.Close();

            var sss = File.ReadAllText("functions_empty_saved.xml");
            sss = sss.Replace(@"&gt;", @">");
            sss = sss.Replace(@"&lt;", @"<");
            File.WriteAllText("functions_empty_saved.xml",sss);
        }

        public static string XmlDecode(string value)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml("<root>" + value + "</root>");
            return xmlDoc.InnerText;
        }

        private string ToHtmlXmlEncoded(string description)
        {
            //TODO: does not work
            //description = HttpUtility.HtmlDecode(description);
            description= description.Replace(@"&gt;", @">");
            description = description.Replace(@"&lt;", @"<");
            return $@"<![CDATA[{description}]]>";
        }

        private Dictionary<string, FunctionInfo> LoadFunctionsDetailsFromXmlFile()
        {
            // Constants.PhysicalConstants.parseConstantsFromNIST();
            //Constants.MathematicalConstants.parseConstantsFromNIST();

            var serializer = new XmlSerializer(typeof(FunctionInfo[]),
                new XmlRootAttribute("FunctionsDetails"));
            var stream = new StreamReader(PathUtility.GetFullPath("Static", "functions.xml"));

            var functionsInfos = ((FunctionInfo[]) serializer.Deserialize(stream)).ToDictionary(kv => kv.Signature,
                kv =>
                    new FunctionInfo
                    {
                        Signature = kv.Signature,
                        Url = kv.Url,
                        Description = kv.Description,
                        Title = kv.Title,
                        Category = kv.Category,
                        Type = kv.Type
                    });

            return functionsInfos;
        }

        public bool ContainsKey(string signature)
        {
            return _details.ContainsKey(signature);
        }

        public KeyValuePair<string, FunctionInfo>[] ToArray()
        {
            return _details.ToArray();
        }
    }
}