using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using Computator.NET.DataTypes.Configuration;

namespace Computator.NET.DataTypes
{
    public class RuntimeObjectFactory
    {
        public static TInterface CreateInstance<TInterface>(string hintName=null) where TInterface : class
        {
            DirectoryInfo dir = new DirectoryInfo(AppInformation.Directory);

            var possibleFiles = new List<FileInfo>();

            if (hintName!=null)
                possibleFiles.AddRange(dir.GetFiles($"*{hintName}*.*"));

            possibleFiles.AddRange(dir.GetFiles("*.dll").Where(f => !f.Name.Contains("Microsoft.VisualBasic.PowerPacks.Vs") && (hintName == null || !f.Name.Contains(hintName))));

            foreach (FileInfo file in possibleFiles)
            {
                Assembly assembly = Assembly.LoadFrom(file.FullName);
                foreach (Type type in assembly.GetTypes())
                {
                    if (typeof(TInterface).IsAssignableFrom(type) && type.IsAbstract == false)
                    {
                        TInterface b = type.InvokeMember(null,
                            BindingFlags.CreateInstance,
                            null, null, null) as TInterface;
                        return b;
                    }
                }
            }
            return null;
        }
    }
}
