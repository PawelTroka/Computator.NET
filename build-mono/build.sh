if [ -z "$build_config" ]; then export build_config="Release"; fi #default is Release

msbuild /p:Configuration="$build_config" Computator.NET/Computator.NET"$netmoniker".csproj
msbuild /p:Configuration="$build_config" Computator.NET.Tests/Computator.NET.Tests.csproj
msbuild /p:Configuration="$build_config" Computator.NET.IntegrationTests/Computator.NET.IntegrationTests.csproj