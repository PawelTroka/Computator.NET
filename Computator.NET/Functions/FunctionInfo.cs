namespace Computator.NET.Functions
{
    [System.Xml.Serialization.XmlRoot("FunctionInfo")]
    public class FunctionInfo
    {
        [System.Xml.Serialization.XmlElement("Category")] public string Category;
        [System.Xml.Serialization.XmlElement("Description")] public string Description;

        [System.Xml.Serialization.XmlAttribute(DataType = "string", AttributeName = "Signature")] public string
            Signature;

        [System.Xml.Serialization.XmlElement("Title")] public string Title;
        [System.Xml.Serialization.XmlElement("Type")] public string Type;
        [System.Xml.Serialization.XmlElement("Url")] public string Url;

        public FunctionInfo()
        {
            Signature = Type = Category = Url = Description = Title = " ";
        }
    }
}