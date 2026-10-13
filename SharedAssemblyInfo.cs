using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Retail")]
#endif
[assembly: AssemblyCompany("TROKA Software")]
[assembly: AssemblyProduct("Computator.NET")]
[assembly: AssemblyCopyright("Copyright © 2017 Pawel Troka")]
[assembly: AssemblyTrademark("TROKA Software")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]

// Derived compatibility attributes for projects with GenerateAssemblyInfo=false.
// Version.props is authoritative; build/release/versioning.py --write updates
// this block, and every normal build rejects drift rather than guessing a version.
// BEGIN GENERATED PRODUCT VERSION
[assembly: AssemblyVersion("3.0.0.0")]
[assembly: AssemblyFileVersion("3.0.0.0")]
[assembly: AssemblyInformationalVersion("3.0.0-preview.1")]
// END GENERATED PRODUCT VERSION

[assembly: NeutralResourcesLanguage("en")]
