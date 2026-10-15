using System.Collections.Generic;
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
            var serializer = new XmlSerializer(typeof(FunctionInfo[]), new XmlRootAttribute("FunctionsDetails"));
            // The committed reference is local data. Release its file handle and
            // prohibit DTD/entity resolution; rendering never needs external assets.
            using (var reader = XmlReader.Create(PathUtility.GetFullPath("Static", "functions.xml"),
                new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }))
            {
                Details = ((FunctionInfo[])serializer.Deserialize(reader))
                    .ToDictionary(item => item.Signature, item => item, System.StringComparer.Ordinal);
            }
        }
    }
}
