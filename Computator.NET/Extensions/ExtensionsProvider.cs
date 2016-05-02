using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.ReflectionModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Computator.NET.DataTypes;
using Computator.NET.Extensibility;

namespace Computator.NET.Extensions
{
    class ExtensionsProvider
    {
        private CompositionContainer _container;

        [ImportMany]
        IEnumerable<Lazy<IFunctionsPackage, IFunctionsPackageData>> functionsPackages;

        private List<Assembly> _assemblies;

        private ExtensionsProvider()
        {
            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();
            //Adds all the parts found in the same assembly as the Program class
         //   catalog.Catalogs.Add(new AssemblyCatalog(typeof(Program).Assembly));
            catalog.Catalogs.Add(new DirectoryCatalog(GlobalConfig.FullPath("Extensions")));


            //Create the CompositionContainer with the parts in the catalog
            _container = new CompositionContainer(catalog);

            //Fill the imports of this object
           // try
          //  {
                this._container.ComposeParts(this);
            // }
            //   catch (CompositionException compositionException)
            //  {
            //      Console.WriteLine(compositionException.ToString());
            // }

            _assemblies = catalog.Parts
                .Select(part => ReflectionModelServices.GetPartType(part).Value.Assembly)
                .Distinct()
                .ToList();

        }

        public IEnumerable<IFunctionsPackage> GetFunctionsPackages(bool scriptingExclusive)
        {
            foreach (Lazy<IFunctionsPackage, IFunctionsPackageData> fp in functionsPackages)
            {
                if (fp.Metadata.IsScriptingOnly && scriptingExclusive)
                    yield return fp.Value;
                if (!fp.Metadata.IsScriptingOnly && !scriptingExclusive)
                    yield return fp.Value;
            }
        }



        public IEnumerable<Type> GetExtensionsTypes(bool scriptingExclusive)
        {
            foreach (var assembly in _assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!type.GetInterfaces().Contains(typeof(IFunctionsPackage)) ||
                        type.GetCustomAttributes(typeof(ExportMetadataAttribute), false).All(p => ((ExportMetadataAttribute) p).Name != nameof(IFunctionsPackageData.IsScriptingOnly)))
                        continue;
                    {
                        if (type.GetCustomAttributes(typeof(ExportMetadataAttribute),false).Any( p => (bool)((ExportMetadataAttribute)p).Value) && scriptingExclusive)
                            yield return type;

                        if (type.GetCustomAttributes(typeof(ExportMetadataAttribute), false).Any(p => !(bool)((ExportMetadataAttribute)p).Value) && !scriptingExclusive)
                            yield return type;
                    }
                }
            }
        }


        public static ExtensionsProvider Instance { get; } = new ExtensionsProvider();
    }
}
