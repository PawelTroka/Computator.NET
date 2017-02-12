nuget install NUnit.Console -Version 3.6.0 -OutputDirectory testrunner

if [ -z "$build_config" ]; then export build_config="Release"; fi #default is Release

#nunit-console Computator.NET.Tests/bin/"$build_config"/Computator.NET.Tests.dll
#nunit-console Computator.NET.IntegrationTests/bin/"$build_config"/Computator.NET.IntegrationTests.dll

mono ./testrunner/NUnit.ConsoleRunner.3.6.0/tools/nunit3-console.exe --labels=All Computator.NET.Tests/bin/"$build_config"/Computator.NET.Tests.dll
mono ./testrunner/NUnit.ConsoleRunner.3.6.0/tools/nunit3-console.exe --labels=All Computator.NET.IntegrationTests/bin/"$build_config"/Computator.NET.IntegrationTests.dll