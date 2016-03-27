using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using AutocompleteMenuNS;
using Computator.NET.Config;
using Computator.NET.Functions;

namespace Computator.NET.Data
{
    public class FunctionsDetails
    {
        private readonly Dictionary<string, FunctionInfo> _details;

        private FunctionsDetails()
        {
            _details = LoadFunctionsDetailsFromXmlFile();
          //  SaveEmptyFunctionDetailsToXmlFile();
        }

        public static FunctionsDetails Details { get; } = new FunctionsDetails();

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

            var serializer = new XmlSerializer(typeof (FunctionInfo[]),
                new XmlRootAttribute("FunctionsDetails"));
            var stream = new StreamWriter("functions_saved.xml");


            serializer.Serialize(stream,
                Enumerable.ToArray(Enumerable.Select(_details, kv =>
                    new FunctionInfo
                    {
                        Signature = kv.Key,
                        Url = kv.Value.Url,
                        Description = kv.Value.Description,
                        Title = kv.Value.Title,
                        Category = kv.Value.Category,
                        Type = kv.Value.Type
                    })));
        }

        //TODO: this function should create new functions file with all it's previous content and empty spaces for new functions which exists in ElementaryFuntions, SpecialFunctions etc but not in the xml file yet
        public void SaveEmptyFunctionDetailsToXmlFile()
        {
            //var _details = new Dictionary<string, FunctionInfo>();
            //foreach (var item in sourceItems)
            //_details.Add(item.FunctionInfo.Signature, item.FunctionInfo);

            var serializer = new XmlSerializer(typeof (FunctionInfo[]),
                new XmlRootAttribute("FunctionsDetails"));
            var stream = new StreamWriter("functions_empty_saved.xml");


            var detailsWithEmpties = new Dictionary<string,FunctionInfo>();

            var items = AutocompletionData.GetAutocompleteItemsForScripting();
            items = items.Distinct(new AutocompleteItemEqualityComparer()).ToArray();
            foreach (var item in items)
            {
                detailsWithEmpties.Add(item.Text,
                    _details.ContainsKey(item.Text) ? _details[item.Text] : item.Info);
            }


            serializer.Serialize(stream,
                Enumerable.ToArray(Enumerable.Select(detailsWithEmpties, kv =>
                    new FunctionInfo
                    {
                        Signature = kv.Key,
                        Url = kv.Value.Url,
                        Description = HttpUtility.HtmlEncode(kv.Value.Description),
                        Title = kv.Value.Title,
                        Category = kv.Value.Category,
                        Type = kv.Value.Type
                    })));
        }

        private Dictionary<string, FunctionInfo> LoadFunctionsDetailsFromXmlFile()
        {
            // Constants.PhysicalConstants.parseConstantsFromNIST();
            //Constants.MathematicalConstants.parseConstantsFromNIST();

            var serializer = new XmlSerializer(typeof (FunctionInfo[]),
                new XmlRootAttribute("FunctionsDetails"));
            var stream = new StreamReader(GlobalConfig.FullPath("Data", "functions.xml"));

            var functionsInfos = Enumerable.ToDictionary(((FunctionInfo[]) serializer.Deserialize(stream)),
                kv => kv.Signature,
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
            return Enumerable.ToArray(_details);
        }
    }
}