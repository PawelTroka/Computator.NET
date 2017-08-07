using System.Xml.Serialization;

namespace Computator.NET.Core.Autocompletion
{
    [XmlRoot("FunctionInfo")]
    public class FunctionInfo
    {
        [XmlElement("Category")] public string Category;


        //[XmlIgnore]
        [XmlElement("Description")] public string Description;

        [XmlAttribute(DataType = "string", AttributeName = "Signature")] public string
            Signature;

        [XmlElement("Title")] public string Title;
        [XmlElement("Type")] public string Type;
        [XmlElement("Url")] public string Url;


        public FunctionInfo()
        {
            Signature = Type = Category = Url = Description = Title = " ";
        }
    }

    public static class FunctionInfoExtensions
    {
        public static bool IsNullOrEmpty(this FunctionInfo functionInfo)
        {
            return functionInfo == null || (string.IsNullOrWhiteSpace(functionInfo.Category) &&
                                            string.IsNullOrWhiteSpace(functionInfo.Description) &&
                                            string.IsNullOrWhiteSpace(functionInfo.Signature) &&
                                            string.IsNullOrWhiteSpace(functionInfo.Title) &&
                                            string.IsNullOrWhiteSpace(functionInfo.Type) &&
                                            string.IsNullOrWhiteSpace(functionInfo.Url));
        }
    }
}