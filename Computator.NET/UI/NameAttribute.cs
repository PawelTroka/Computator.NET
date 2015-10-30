namespace Computator.NET
{
    [System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = true)]
    internal class NameAttribute : System.Attribute
    {
        public NameAttribute(string str)
        {
            Name = str;
        }

        public string Name { get; set; }
    }
}