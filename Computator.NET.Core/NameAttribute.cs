using System;

namespace Computator.NET.UI
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class NameAttribute : Attribute
    {
        public NameAttribute(string str)
        {
            Name = str;
        }

        public string Name { get; set; }
    }
}