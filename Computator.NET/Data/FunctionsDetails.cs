using Enumerable = System.Linq.Enumerable;

namespace Computator.NET.Data
{
    internal class FunctionsDetails
    {
        private readonly System.Collections.Generic.Dictionary<string, Functions.FunctionInfo> _details;

        public FunctionsDetails()
        {
            _details = LoadFunctionsDetailsFromXmlFile();
        }

        public static FunctionsDetails Details { get; } = new FunctionsDetails();

        public Functions.FunctionInfo this[string signature]
        {
            get { return _details[signature]; }
            set { _details[signature] = value; }
        }

        private void SaveFunctionDetailsToXmlFile()
        {
            //var _details = new Dictionary<string, FunctionInfo>();
            //foreach (var item in sourceItems)
            //_details.Add(item.functionInfo.Signature, item.functionInfo);

            var serializer = new System.Xml.Serialization.XmlSerializer(typeof (Functions.FunctionInfo[]),
                new System.Xml.Serialization.XmlRootAttribute("FunctionsDetails"));
            var stream = new System.IO.StreamWriter("functions_saved.xml");


            var listOfItems = Enumerable.ToList(Enumerable.Select(_details, kv =>
                new Functions.FunctionInfo
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
                    new Functions.FunctionInfo
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

            var serializer = new System.Xml.Serialization.XmlSerializer(typeof (Functions.FunctionInfo[]),
                new System.Xml.Serialization.XmlRootAttribute("FunctionsDetails"));
            var stream = new System.IO.StreamWriter("functions_empty_saved.xml");


            var listOfItems = Enumerable.ToList(Enumerable.Select(_details, kv =>
                new Functions.FunctionInfo
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
                    new Functions.FunctionInfo
                    {
                        Signature = kv.Key,
                        Url = kv.Value.Url,
                        Description = kv.Value.Description,
                        Title = kv.Value.Title,
                        Category = kv.Value.Category,
                        Type = kv.Value.Type
                    })));
        }

        private System.Collections.Generic.Dictionary<string, Functions.FunctionInfo> LoadFunctionsDetailsFromXmlFile()
        {
            // Constants.PhysicalConstants.parseConstantsFromNIST();
            //Constants.MathematicalConstants.parseConstantsFromNIST();

            var serializer = new System.Xml.Serialization.XmlSerializer(typeof (Functions.FunctionInfo[]),
                new System.Xml.Serialization.XmlRootAttribute("FunctionsDetails"));
            var stream = new System.IO.StreamReader(Config.GlobalConfig.FullPath("Data", "functions.xml"));

            var functionsInfos = Enumerable.ToDictionary(((Functions.FunctionInfo[]) serializer.Deserialize(stream)),
                kv => kv.Signature,
                kv =>
                    new Functions.FunctionInfo
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

        public System.Collections.Generic.KeyValuePair<string, Functions.FunctionInfo>[] ToArray()
        {
            return Enumerable.ToArray(_details);
        }
    }
}