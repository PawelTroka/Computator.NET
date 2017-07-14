using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;

namespace Computator.NET.Installer
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

    [Serializable]
    public class NetVersion
    {
        public static NetVersion FromAssembly(string assemblyPath)
        {
            var ret = new NetVersion()
            {
                DisplayVersion = "0.0.0",
                RealVersion = new Version(0, 0, 0)
            };
            AppDomain sandbox=null; //create a discardable AppDomain
            try
            {
                sandbox = AppDomain.CreateDomain($"sandbox_{Guid.NewGuid()}",null, new AppDomainSetup
                {
                    ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                });


                var handle = Activator.CreateInstance(sandbox,
                    typeof(ReferenceLoader).Assembly.FullName,
                    typeof(ReferenceLoader).FullName,
                    false, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, null, CultureInfo.CurrentCulture, new object[0]);

                var loader = (ReferenceLoader)handle.Unwrap();
                //This operation is executed in the new AppDomain
                var frameworkName = loader.LoadReference(assemblyPath);


                if (frameworkName != null)
                {
                    var displayVersion = Regex.Match(frameworkName, @"(\d+\.\d+(?:\.\d+(?:\.\d+)?)?)").Value;
                    Console.WriteLine($"Assembly '{assemblyPath}' is compiled against .NET version '{displayVersion}'");

                    var versionArray =
                        displayVersion.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse)
                            .Merge(Enumerable.Repeat(0, 4), Math.Max).ToArray();

                    ret = new NetVersion()
                    {
                        DisplayVersion = displayVersion,
                        RealVersion = new Version(versionArray[0], versionArray[1], versionArray[2],
                            versionArray[3])
                    };
                }

            }

            catch (Exception exception)
            {
                Console.WriteLine(
                    $"Exception occured while processing assembly '{assemblyPath}' - details: '{exception.Message}'");
            }
            finally
            {
                if(sandbox!=null)
                    AppDomain.Unload(sandbox);
            }

            return ret;
        }

        public Version RealVersion { get; set; }
        public string DisplayVersion { get; set; }
    }

    public class ReferenceLoader : MarshalByRefObject
    {
        public string LoadReference(string assemblyPath)
        {
            string frameworkName = null;
            var assembly = Assembly.LoadFrom(assemblyPath);//ReflectionOnlyLoadFrom

            var customAttributes = assembly.GetCustomAttributesData();
            var targetFramework =
                customAttributes.FirstOrDefault(attr => attr.AttributeType == typeof(TargetFrameworkAttribute));
            
            if (targetFramework!=null && targetFramework.ConstructorArguments.Any())
            {
                // first argument is the name of the framework.
                frameworkName = (string)targetFramework.ConstructorArguments[0].Value;
            }
            return frameworkName;
        }
    }
}
