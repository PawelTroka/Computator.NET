using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Computator.NET.Config;
using Computator.NET.Functions;

namespace Computator.NET.Data
{
    internal class FunctionsDetails
    {
        private readonly Dictionary<string, FunctionInfo> _details;

        public FunctionsDetails()
        {
            _details = LoadFunctionsDetailsFromXmlFile();
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
            //_details.Add(item.functionInfo.Signature, item.functionInfo);

            var serializer = new XmlSerializer(typeof (FunctionInfo[]),
                new XmlRootAttribute("FunctionsDetails"));
            var stream = new StreamWriter("functions_saved.xml");


            var listOfItems = Enumerable.ToList(Enumerable.Select(_details, kv =>
                new FunctionInfo
                {
                    Signature = kv.Key,
                    Url = kv.Value.Url,
                    Description = kv.Value.Description,
                    Title = kv.Value.Title,
                    Category = kv.Value.Category,
                    Type = kv.Value.Type
                }));


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
        private void SaveEmptyFunctionDetailsToXmlFile()
        {
            //var _details = new Dictionary<string, FunctionInfo>();
            //foreach (var item in sourceItems)
            //_details.Add(item.functionInfo.Signature, item.functionInfo);

            var serializer = new XmlSerializer(typeof (FunctionInfo[]),
                new XmlRootAttribute("FunctionsDetails"));
            var stream = new StreamWriter("functions_empty_saved.xml");


            var listOfItems = Enumerable.ToList(Enumerable.Select(_details, kv =>
                new FunctionInfo
                {
                    Signature = kv.Key,
                    Url = kv.Value.Url,
                    Description = kv.Value.Description,
                    Title = kv.Value.Title,
                    Category = kv.Value.Category,
                    Type = kv.Value.Type
                }));


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