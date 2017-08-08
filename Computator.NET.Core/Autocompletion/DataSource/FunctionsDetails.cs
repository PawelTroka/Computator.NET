using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Computator.NET.DataTypes.Utility;

namespace Computator.NET.Core.Autocompletion.DataSource
{

    public interface IFunctionsDetailsFileSource
    {
        IDictionary<string, FunctionInfo> Details { get; }
    }
    public class FunctionsDetailsFileSource : IFunctionsDetailsFileSource
    {
        public IDictionary<string, FunctionInfo> Details { get; }

        public FunctionsDetailsFileSource()
        {
            Details = LoadFunctionsDetailsFromXmlFile();
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
    }
}