nuget install NUnit.Console -Version 3.6.0 -OutputDirectory testrunner

if [ -z "$build_config" ]; then export build_config="Release"; fi #default is Release
if [ -z "$netmoniker" ]; then export netmoniker="net4.6.1"; fi #default is net4.6.1

#nunit-console Computator.NET.Tests/bin/"$build_config"/Computator.NET.Tests.dll
#nunit-console Computator.NET.IntegrationTests/bin/"$build_config"/Computator.NET.IntegrationTests.dll

mono ./testrunner/NUnit.ConsoleRunner.3.6.1/tools/nunit3-console.exe --labels=All Computator.NET.Tests/bin/"$build_config"/"$netmoniker"/Computator.NET.Tests.dll
mono ./testrunner/NUnit.ConsoleRunner.3.6.1/tools/nunit3-console.exe --labels=All Computator.NET.IntegrationTests/bin/"$build_config"/"$netmoniker"/Computator.NET.IntegrationTests.dll