using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;

namespace Computator.NET.Setup
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<TResult> Merge<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first,
            IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> operation)
        {
            using (var iter1 = first.GetEnumerator())
            {
                using (var iter2 = second.GetEnumerator())
                {
                    while (iter1.MoveNext())
                    {
                        if (iter2.MoveNext())
                        {
                            yield return operation(iter1.Current, iter2.Current);
                        }
                        else
                        {
                            yield return operation(iter1.Current, default(TSecond));
                        }
                    }
                    while (iter2.MoveNext())
                    {
                        yield return operation(default(TFirst), iter2.Current);
                    }
                }
            }
        }
    }
    public class NetVersion
    {
        public static NetVersion FromAssembly(string assemblyPath)
        {
            try
            {
                object[] list = Assembly.LoadFrom(assemblyPath).GetCustomAttributes(true);
                var attribute = list.OfType<TargetFrameworkAttribute>().FirstOrDefault();

                if (attribute?.FrameworkName != null)
                {
                    var displayVersion = Regex.Match(attribute.FrameworkName, @"(\d+\.\d+(?:\.\d+(?:\.\d+)?)?)").Value;
                    Console.WriteLine($"Assembly '{assemblyPath}' is compiled against .NET version '{displayVersion}'");

                    var versionArray =
                        displayVersion.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse)
                            .Merge(Enumerable.Repeat(0, 4), Math.Max).ToArray();


                    return new NetVersion()
                    {
                        DisplayVersion = displayVersion,
                        RealVersion = new Version(versionArray[0], versionArray[1], versionArray[2], versionArray[3])
                    };

                }
            }
            catch(Exception exception)
            {
                Console.WriteLine($"Exception occured while processing assembly '{assemblyPath}' - details: '{exception.Message}'");
            }
            return new NetVersion() { DisplayVersion = "0.0.0", RealVersion = new Version(0,0,0) };
        }
        public Version RealVersion { get; set; }
        public string DisplayVersion { get; set; }
    }
}
