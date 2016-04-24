// <copyright file="PexAssemblyInfo.cs" company="Pawel Troka">Copyright ©  2016 Pawel Troka</copyright>
using Microsoft.Pex.Framework.Coverage;
using Microsoft.Pex.Framework.Creatable;
using Microsoft.Pex.Framework.Instrumentation;
using Microsoft.Pex.Framework.Settings;
using Microsoft.Pex.Framework.Validation;

// Microsoft.Pex.Framework.Settings
[assembly: PexAssemblySettings(TestFramework = "VisualStudioUnitTest")]

// Microsoft.Pex.Framework.Instrumentation
[assembly: PexAssemblyUnderTest("Computator.NET")]
[assembly: PexInstrumentAssembly("ICSharpCode.AvalonEdit")]
[assembly: PexInstrumentAssembly("PresentationFramework")]
[assembly: PexInstrumentAssembly("PresentationCore")]
[assembly: PexInstrumentAssembly("WindowsFormsIntegration")]
[assembly: PexInstrumentAssembly("System.Windows.Forms.DataVisualization")]
[assembly: PexInstrumentAssembly("Microsoft.VisualBasic")]
[assembly: PexInstrumentAssembly("System.Web")]
[assembly: PexInstrumentAssembly("Microsoft.CSharp")]
[assembly: PexInstrumentAssembly("MathNet.Numerics")]
[assembly: PexInstrumentAssembly("Meta.Numerics")]
[assembly: PexInstrumentAssembly("System.Numerics")]
[assembly: PexInstrumentAssembly("Computator.NET.Charting")]
[assembly: PexInstrumentAssembly("System.Drawing")]
[assembly: PexInstrumentAssembly("Computator.NET.DataTypes")]
[assembly: PexInstrumentAssembly("System.Core")]
[assembly: PexInstrumentAssembly("Accord")]
[assembly: PexInstrumentAssembly("SimpleLogger")]
[assembly: PexInstrumentAssembly("Accord.Math")]
[assembly: PexInstrumentAssembly("ScintillaNET")]
[assembly: PexInstrumentAssembly("System.Windows.Forms")]
[assembly: PexInstrumentAssembly("WindowsBase")]

// Microsoft.Pex.Framework.Creatable
[assembly: PexCreatableFactoryForDelegates]

// Microsoft.Pex.Framework.Validation
[assembly: PexAllowedContractRequiresFailureAtTypeUnderTestSurface]
[assembly: PexAllowedXmlDocumentedException]

// Microsoft.Pex.Framework.Coverage
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "ICSharpCode.AvalonEdit")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "PresentationFramework")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "PresentationCore")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "WindowsFormsIntegration")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "System.Windows.Forms.DataVisualization")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "Microsoft.VisualBasic")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "System.Web")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "Microsoft.CSharp")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "MathNet.Numerics")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "Meta.Numerics")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "System.Numerics")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "Computator.NET.Charting")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "System.Drawing")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "Computator.NET.DataTypes")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "System.Core")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "Accord")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "SimpleLogger")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "Accord.Math")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "ScintillaNET")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "System.Windows.Forms")]
[assembly: PexCoverageFilterAssembly(PexCoverageDomain.UserOrTestCode, "WindowsBase")]

